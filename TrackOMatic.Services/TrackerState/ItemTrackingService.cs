using TrackOMatic.Logic.Enums;
using TrackOMatic.Logic.Events;
using TrackOMatic.Logic.Models;
using TrackOMatic.Logic;

namespace TrackOMatic.Services.TrackerState;

/// <summary>
/// Represents a deferred item state change during batch operations.
/// </summary>
internal record DeferredItemChange(ItemName ItemName, SavedItem? CurrentState, SavedItem? PreviousState);

/// <summary>
/// Service for managing item tracking state across regions.
/// Backed by SavedProgress and fires events when item state changes.
/// </summary>
public class ItemTrackingService : IItemTrackingService
{
    private readonly ISavedProgressProvider _progressProvider;
    private readonly Dictionary<ItemName, SavedItem> _itemCache;
    private bool _isBatchingUpdates;
    private readonly List<DeferredItemChange> _deferredUpdates;

    public event EventHandler<ItemStateChangedEventArgs>? ItemStateChanged;

    public ItemTrackingService(ISavedProgressProvider progressProvider)
    {
        _progressProvider = progressProvider ?? throw new ArgumentNullException(nameof(progressProvider));

        _itemCache = [];
        _isBatchingUpdates = false;
        _deferredUpdates = [];
        InitializeCache();

        // Subscribe to progress changes (Reset/Load operations)
        _progressProvider.ProgressChanged += OnProgressChanged;
    }

    private void InitializeCache()
    {
        _itemCache.Clear();
        foreach (var item in _progressProvider.CurrentProgress.SavedItems)
        {
            _itemCache[item.Key] = item.Value;
        }
    }

    private void OnProgressChanged(object? sender, ProgressReplacedEventArgs e)
    {
        // When SavedProgress is replaced (Reset/Load), reinitialize cache
        InitializeCache();
    }

    public SavedItem? GetItemState(ItemName itemName)
    {
        _itemCache.TryGetValue(itemName, out var item);
        return item;
    }

    public void SetItemState(ItemName itemName, SavedItem state)
    {
        if (state.ItemName != itemName)
        {
            throw new ArgumentException($"SavedItem.ItemName ({state.ItemName}) does not match itemName parameter ({itemName})", nameof(state));
        }

        SavedItem? previousState = null;
        if (_itemCache.TryGetValue(itemName, out var existing))
        {
            previousState = existing;
        }

        // Update cache
        _itemCache[itemName] = state;

        // Update SavedProgress
        _progressProvider.CurrentProgress.SavedItems[itemName] = state;

        // If batching, defer the event; otherwise fire immediately
        if (_isBatchingUpdates)
        {
            _deferredUpdates.Add(new DeferredItemChange(itemName, state, previousState));
        }
        else
        {
            ItemStateChanged?.Invoke(this, new(state, previousState, ChangeReason.UserModified));
        }
    }

    public void ClearItemState(ItemName itemName)
    {
        if (!_itemCache.ContainsKey(itemName))
        {
            return;
        }

        SavedItem? removedState = null;
        if (_itemCache.TryGetValue(itemName, out var existing))
        {
            removedState = existing;
        }

        // Remove from cache
        _itemCache.Remove(itemName);

        // Remove from SavedProgress
        _progressProvider.CurrentProgress.SavedItems.Remove(itemName);

        // If batching, defer the event; otherwise fire immediately
        if (_isBatchingUpdates)
        {
            if (removedState != null)
            {
                _deferredUpdates.Add(new DeferredItemChange(itemName, null, removedState));
            }
        }
        else
        {
            // Fire event to notify subscribers that the item was removed
            // Broadcast controls need to know to update their display
            if (removedState != null)
            {
                ItemStateChanged?.Invoke(this, new(removedState, removedState, ChangeReason.UserModified));
            }
        }
    }

    /// <summary>
    /// Updates the region of an item entry and evaluates whether it should still exist.
    /// Checks validity first before making any changes.
    /// Returns true if the item still exists after the update, false if it was removed.
    /// </summary>
    public bool UpdateItemRegion(ItemName itemName, RegionName newRegion)
    {
        if (!_itemCache.TryGetValue(itemName, out var item))
        {
            return false;
        }

        // Create a temporary item with the new region to check if it would be valid
        var tempItem = new SavedItem(item.ItemName, newRegion, item.Starred, item.Autotracked, item.Opacity, item.Hinted);

        var previousRegion = item.Region;

        // Check if item should be kept with the new region
        if (!tempItem.ShouldKeepSavedItem())
        {
            // Item should be cleared - do that instead
            ClearItemState(itemName);
            return false;
        }

        // Item is valid - update the region and notify
        item.Region = newRegion;

        // Also update in SavedProgress to keep in sync
        if (_progressProvider.CurrentProgress.SavedItems.TryGetValue(itemName, out var savedItem))
        {
            savedItem.Region = newRegion;
        }

        SavedItem previous = new(item.ItemName, previousRegion, item.Starred, item.Autotracked, item.Opacity, item.Hinted);

        // If batching, defer the event; otherwise fire immediately
        if (_isBatchingUpdates)
        {
            _deferredUpdates.Add(new DeferredItemChange(itemName, item, previous));
        }
        else
        {
            ItemStateChanged?.Invoke(this, new(item, previous, ChangeReason.UserModified));
        }

        return true;
    }

    public IEnumerable<SavedItem> GetItemsInRegion(RegionName region)
    {
        return _itemCache.Values.Where(item => item.Region == region);
    }

    public IEnumerable<SavedItem> GetItemsByVisibility(ItemVisibilityState state)
    {
        return _itemCache.Values.Where(item => item.Starred == state);
    }

    /// <summary>
    /// Toggles the star visibility state of an item.
    /// If no SavedItem exists for the item, creates one with the star toggled to Visible.
    /// This method centralizes star toggle logic to ensure consistent behavior across all view models.
    /// </summary>
    /// <param name="itemName">The name of the item whose star should be toggled.</param>
    public void ToggleStar(ItemName itemName)
    {
        var currentItem = GetItemState(itemName) ?? SavedItem.CreateEmpty(itemName);

        var newStarred = currentItem.Starred == ItemVisibilityState.Visible
            ? ItemVisibilityState.Hidden
            : ItemVisibilityState.Visible;

        SavedItem updatedItem = new(currentItem.ItemName, currentItem.Region, newStarred, currentItem.Autotracked, currentItem.Opacity, currentItem.Hinted);
        SetItemState(itemName, updatedItem);
    }

    /// <summary>
    /// Begins a batch update scope where multiple item changes are deferred.
    /// When the returned scope is disposed, all collected changes fire a single ItemStateChanged event.
    /// </summary>
    public IDisposable BeginBatchUpdate()
    {
        return new BatchUpdateScope(this);
    }

    /// <summary>
    /// Private helper class that manages the batch update scope lifecycle.
    /// </summary>
    private class BatchUpdateScope : IDisposable
    {
        private readonly ItemTrackingService _service;
        private bool _disposed;

        public BatchUpdateScope(ItemTrackingService service)
        {
            _service = service;
            _service._isBatchingUpdates = true;
            _service._deferredUpdates.Clear();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            // Exit batch mode and process all deferred updates
            _service._isBatchingUpdates = false;
            var deferredUpdates = _service._deferredUpdates.ToList();
            _service._deferredUpdates.Clear();

            // Fire a composite event for all collected changes
            if (deferredUpdates.Count > 0)
            {
                foreach (var change in deferredUpdates)
                {
                    // For each deferred change, fire an event
                    // Note: CurrentState is null only for cleared items; PreviousState is always present
                    // because we only defer changes that actually occurred.
                    var itemToReport = change.CurrentState ?? change.PreviousState;
                    ArgumentNullException.ThrowIfNull(itemToReport, nameof(itemToReport));

                    _service.ItemStateChanged?.Invoke(_service, new(itemToReport, change.PreviousState, ChangeReason.Batched));
                }
            }
        }
    }
}
