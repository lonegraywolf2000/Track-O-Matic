using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using TrackOMatic.Logic.Enums;
using TrackOMatic.Logic.Events;
using TrackOMatic.Logic.Models;
using TrackOMatic.Services;
using TrackOMatic.Services.TrackerState;

namespace TrackOMatic.ViewModels;

public class RegionViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IItemTrackingService _itemTrackingService;
    private readonly IParsedSpoilerDataService _parsedSpoilerDataService;
    private readonly ISavedProgressProvider _savedProgressProvider;
    private readonly IThemeService? _themeService;
    private readonly IAutotrackingHandoffRegistry _autotrackingRegistry;
    private readonly RegionName _regionName;

    // Placed items in the grid
    private ObservableCollection<IPotentialRegionItemViewModel> _placedItems = new();
    public ObservableCollection<IPotentialRegionItemViewModel> PlacedItems
    {
        get => _placedItems;
        private set
        {
            if (_placedItems != value)
            {
                _placedItems = value;
                OnPropertyChanged();
            }
        }
    }


    public RegionViewModel(
        RegionName regionName,
        IItemTrackingService itemTrackingService,
        IParsedSpoilerDataService parsedSpoilerDataService,
        ISavedProgressProvider savedProgressProvider,
        IAutotrackingHandoffRegistry autotrackingRegistry,
        IThemeService? themeService = null
    )
    {
        _regionName = regionName;
        _itemTrackingService = itemTrackingService ?? throw new ArgumentNullException(nameof(itemTrackingService));
        _parsedSpoilerDataService = parsedSpoilerDataService ?? throw new ArgumentNullException(nameof(parsedSpoilerDataService));
        _savedProgressProvider = savedProgressProvider ?? throw new ArgumentNullException(nameof(savedProgressProvider));
        _autotrackingRegistry = autotrackingRegistry ?? throw new ArgumentNullException(nameof(autotrackingRegistry));
        _themeService = themeService;

        _itemTrackingService.ItemStateChanged += OnItemStateChanged;
        _parsedSpoilerDataService.ParsedSpoilerDataChanged += OnParsedSpoilerDataChanged;
        _savedProgressProvider.ProgressChanged += OnProgressChanged;
        _autotrackingRegistry.RegisterRegionHandler(_regionName, TryAutoPlaceItem);

        InitializeState();
    }

    private void InitializeState()
    {
        RefreshPlacedItems();
        UpdateHoardText();
        UpdateItemPointText();
        UpdateRegionResourceKey();
    }

    private void RefreshPlacedItems()
    {
        PlacedItems.Clear();

        /*
         * Ideally this code will only be called after every event is fired at once.
         * There is major potential for race conditions if not handled properly.
         */

        // Special handling for START region - it uses StartingItems from spoiler data, not RegionData vials
        if (_regionName == RegionName.START && _parsedSpoilerDataService.CurrentData?.StartingItems.Count > 0)
        {
            // Create vial slots for starting items (excluding UNHINTABLE_MOVES)
            var startingItemsForDisplay = _parsedSpoilerDataService.CurrentData.StartingItems
                            .Where(kvp => kvp.Value == RegionName.START)
                            .ToList();

            for (int slotIndex = 0; slotIndex < startingItemsForDisplay.Count; slotIndex++)
            {
                var itemName = startingItemsForDisplay[slotIndex].Key;
                var vialColor = itemName.ToVialColor();

                PlacedItems.Add(new SpoilerStartItemViewModel(
                vialColor,
                RegionName.START,
                slotIndex,
                itemName, // Fixed item name for START slots
                _itemTrackingService,
                _parsedSpoilerDataService,
                _savedProgressProvider,
                _themeService));
            }
        }
        else if (_parsedSpoilerDataService.CurrentData?.RegionData.TryGetValue(_regionName, out var regionData) == true)
        {
            for (int slotIndex = 0; slotIndex < regionData.VialColors.Count; slotIndex++)
            {
                var vialColor = regionData.VialColors[slotIndex];
                PlacedItems.Add(new SpoilerItemViewModel(
                vialColor,
                _regionName,
                slotIndex,
                _itemTrackingService,
                _parsedSpoilerDataService,
                _savedProgressProvider,
                _themeService)
                                );
            }
        }
        else
        {
            var itemsInRegion = _itemTrackingService.GetItemsInRegion(_regionName);
            foreach (var item in itemsInRegion)
            {
                PlacedItems.Add(new RegionItemViewModel(item.ItemName, _itemTrackingService, _parsedSpoilerDataService, _savedProgressProvider, _themeService));
            }
        }
    }

    /// <summary>
    /// Determines if this region is in spoiler mode (has vials from a spoiler log).
    /// </summary>
    private bool IsSpoilerMode => _parsedSpoilerDataService.CurrentData?.RegionData.ContainsKey(_regionName) == true;

    /// <summary>
    /// Attempts to drop an item into this region.
    /// </summary>
    public bool TryAcceptDrop(ItemName itemToPlace, MouseDragType dragType)
    {
        if (IsSpoilerMode)
        {
            // In spoiler mode: find the first empty slot that matches the vial color
            var targetColor = itemToPlace.ToVialColor();
            var emptySlot = PlacedItems.FirstOrDefault(slot =>
            slot is SpoilerStartItemViewModel spoilerSlot &&
            spoilerSlot.VialColor == targetColor &&
            !spoilerSlot.CurrentItemName.HasValue);

            return emptySlot?.TryAcceptDrop(itemToPlace, dragType) ?? false;
        }
        else
        {
            // In no-spoiler mode: if there are existing items, try them
            // Otherwise, accept the drop directly (it will create a new RegionItemViewModel)
            if (PlacedItems.Count > 0)
            {
                return PlacedItems.Any(slot => slot.TryAcceptDrop(itemToPlace, dragType));
            }
            else
            {
                // Empty region in no-spoiler mode: accept the drop
                // The tracking service will handle the actual state change
                return true;
            }
        }
    }

    #region Proxy Properties

    private string _regionResourceKey = "";
    public string RegionResourceKey
    {
        get => _regionResourceKey;
        set
        {
            if (_regionResourceKey != value)
            {
                _regionResourceKey = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _hasWothPoints = false;
    public bool HasWothPoints
    {
        get => _hasWothPoints;
        set
        {
            if (_hasWothPoints != value)
            {
                _hasWothPoints = value;
                OnPropertyChanged();
            }
        }
    }

    private string _wothPoints = "N/A";
    public string WothPoints
    {
        get => _wothPoints;
        set
        {
            if (_wothPoints != value)
            {
                _wothPoints = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _hasItemPoints = false;
    public bool HasItemPoints
    {
        get => _hasItemPoints;
        set
        {
            if (_hasItemPoints != value)
            {
                _hasItemPoints = value;
                OnPropertyChanged();
            }
        }
    }

    private string _itemPoints = "N/A";
    public string ItemPoints
    {
        get => _itemPoints;
        set
        {
            if (_itemPoints != value)
            {
                _itemPoints = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region INotifyPropertyChanged Implementation

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region IDisposable

    private bool _disposed = false;

    // Cleanup
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Unsubscribe from events
                _autotrackingRegistry.UnregisterRegionHandler(_regionName);
                _itemTrackingService.ItemStateChanged -= OnItemStateChanged;
                _parsedSpoilerDataService.ParsedSpoilerDataChanged -= OnParsedSpoilerDataChanged;
                _savedProgressProvider.ProgressChanged -= OnProgressChanged;
            }
            _disposed = true;
        }
    }

    ~RegionViewModel()
    {
        Dispose(false);
    }

    #endregion

    private void OnItemStateChanged(object? sender, ItemStateChangedEventArgs e)
    {
        // Only process if this item was in our region previously or is in our region now
        bool wasInThisRegion = e.PreviousState?.Region == _regionName;
        bool isInThisRegion = e.UpdatedItem.Region == _regionName;

        if (!wasInThisRegion && !isInThisRegion)
        {
            // Item change is not relevant to this region
            return;
        }

        // Only process certain change reasons to avoid stale state assumptions
        bool isRelevantChange = e.ChangeReason == "ItemUpdated"
                    || e.ChangeReason == "ItemMovedToUnknown"
                    || e.ChangeReason == "ItemCleared";

        if (!isRelevantChange)
        {
            return;
        }

        // In spoiler mode, update hoard text based on starred items
        if (IsSpoilerMode)
        {
            UpdateHoardText(e.UpdatedItem);
        }
        else
        {
            // In non-spoiler mode, refresh if an item is added/removed from this region
            InitializeState();
        }
    }

    private void OnParsedSpoilerDataChanged(object? sender, EventArgs e) => InitializeState();

    private void OnProgressChanged(object? sender, ProgressReplacedEventArgs e) => InitializeState();

    private void UpdateHoardText(SavedItem? changedItem = null)
    {
        var wothPoints = _parsedSpoilerDataService.GetWothPointsForRegion(_regionName);
        if (wothPoints == int.MinValue || wothPoints == -1)
        {
            WothPoints = "N/A";
            HasWothPoints = false;
        }
        else
        {
            // Count starred items by querying the actual item state from the tracking service,
            // not from the UI property which may be stale or in transition during optimistic updates
            int starredCount = 0;
            foreach (var item in PlacedItems)
            {
                // Get the item name from the view model (handles both fixed and current items)
                ItemName? itemName = null;
                if (item is SpoilerStartItemViewModel spoilerStart)
                {
                    itemName = spoilerStart.CurrentItemName ?? spoilerStart.FixedItemName;
                }
                else if (item is SpoilerItemViewModel spoiler)
                {
                    itemName = spoiler.CurrentItemName;
                }
                else if (item is RegionItemViewModel region)
                {
                    itemName = region.ItemName;
                }

                // Check if this item is actually starred in the service
                if (itemName.HasValue)
                {
                    // If this is the item that just changed and we have it in the event args,
                    // use that instead of querying the service (more up-to-date)
                    bool isStarred;
                    if (changedItem?.ItemName == itemName.Value)
                    {
                        isStarred = changedItem.Starred == ItemVisibilityState.Visible;
                    }
                    else
                    {
                        var savedItem = _itemTrackingService.GetItemState(itemName.Value);
                        isStarred = savedItem is not null && savedItem.Starred == ItemVisibilityState.Visible;
                    }

                    if (isStarred)
                    {
                        starredCount++;
                    }
                }
            }
            WothPoints = (wothPoints - starredCount).ToString();
            HasWothPoints = true;
        }
    }

    private void UpdateItemPointText()
    {
        // TODO: Have the point calculations take place in the service layer.
        var regionPoints = _parsedSpoilerDataService.GetPointsForRegion(_regionName);
        var pointSpread = _parsedSpoilerDataService.GetPointSpread();
        if (pointSpread.Count == 0)
        {
            ItemPoints = "N/A";
            HasItemPoints = false;
        }
        else
        {
            var itemsInRegion = _itemTrackingService.GetItemsInRegion(_regionName);
            var categories = itemsInRegion.Select(i => i.ItemName.ToPointCategory());
            var points = regionPoints - categories.Sum(c => pointSpread[c]);
            ItemPoints = points.ToString();
            HasItemPoints = points >= 0;
        }
    }

    private void UpdateRegionResourceKey()
    {
        RegionResourceKey = _regionName.ToString().ToLowerInvariant();
    }

    /// <summary>
    /// Attempts to auto-place an item into an appropriate vial slot in this region.
    /// Iterates through all vials of matching color until one accepts the placement.
    /// </summary>
    public bool TryAutoPlaceItem(ItemName itemToPlace)
    {
        // Non-spoiler mode: direct placement through ItemTrackingService
        if (!IsSpoilerMode)
        {
            var savedItem = new SavedItem(
            itemToPlace,
            _regionName,
            ItemVisibilityState.Hidden,
            autotracked: true,
            opacity: 1.0
                        );

            _itemTrackingService.SetItemState(itemToPlace, savedItem);
            return true;
        }

        // Spoiler mode: find a vial slot that matches the color
        var targetColor = itemToPlace.ToVialColor();

        var matchingSlots = PlacedItems
                    .OfType<SpoilerItemViewModel>()
                    .Where(slot => slot.VialColor == targetColor)
                    .ToList();

        // Scenario 3: No vials of this color in this region
        if (matchingSlots.Count == 0)
        {
            return false; // Invalid operation
        }

        // Try each matching slot in order
        foreach (var slot in matchingSlots)
        {
            if (slot.TryAutoPlaceItem(itemToPlace))
            {
                return true;
            }
        }

        // All matching slots rejected the placement
        return false;
    }

    #region Equality Members

    public override bool Equals(object? obj) => obj is RegionViewModel other && other._regionName == _regionName;

    public override int GetHashCode() => _regionName.GetHashCode();

    #endregion
}
