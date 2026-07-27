using TrackOMatic.Logic.Enums;
using TrackOMatic.Logic.Events;
using TrackOMatic.Logic.Models;
using TrackOMatic.Logic;

namespace TrackOMatic.Services.TrackerState;

/// <summary>
/// Service for managing item tracking state across regions.
/// Backed by SavedProgress and fires events when item state changes.
/// </summary>
public class ItemTrackingService : IItemTrackingService
{
    private readonly ISavedProgressProvider _progressProvider;
    private readonly Dictionary<ItemName, SavedItem> _itemCache;

    public event EventHandler<ItemStateChangedEventArgs>? ItemStateChanged;

    public ItemTrackingService(ISavedProgressProvider progressProvider)
    {
        _progressProvider = progressProvider ?? throw new ArgumentNullException(nameof(progressProvider));

        _itemCache = [];
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

        // Fire event to notify subscribers
        ItemStateChanged?.Invoke(this, new(state, previousState, "ItemUpdated"));
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

        // Fire event to notify subscribers that the item was removed
        // Broadcast controls need to know to update their display
        if (removedState != null)
        {
            ItemStateChanged?.Invoke(this, new(removedState, removedState, "ItemCleared"));
        }
    }

    /// <summary>
    /// Updates a specific property of an item without replacing the entire entry.
    /// Useful for region changes, opacity updates, etc.
    /// </summary>
    public void UpdateItemProperty(ItemName itemName, Action<SavedItem> updateAction)
    {
        if (!_itemCache.TryGetValue(itemName, out var item))
        {
            return;
        }

        var previousState = new SavedItem(item.ItemName, item.Region, item.Starred, item.Autotracked, item.Opacity, item.Hinted);

        updateAction(item);

        // Item already in both cache and SavedProgress; just notify
        ItemStateChanged?.Invoke(this, new(item, previousState, "ItemPropertyUpdated"));
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
        ItemStateChanged?.Invoke(this, new(item, previous, "ItemRegionUpdated"));

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
}
