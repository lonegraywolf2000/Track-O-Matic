using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using TrackOMatic.Logic;
using TrackOMatic.Logic.Enums;
using TrackOMatic.Logic.Events;
using TrackOMatic.Logic.Models;
using TrackOMatic.Services;
using TrackOMatic.Services.TrackerState;


[assembly: InternalsVisibleTo("TrackOMatic.ViewModels.Test")]

namespace TrackOMatic.ViewModels;

public class RegionViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IItemTrackingService _itemTrackingService;
    private readonly IParsedSpoilerDataService _parsedSpoilerDataService;
    private readonly ISavedProgressProvider _savedProgressProvider;
    private readonly IThemeService _themeService;
    // private readonly IAutotrackingHandoffRegistry _autotrackingRegistry;
    private readonly RegionName _regionName;

    public RegionViewModel(
        RegionName regionName,
        IItemTrackingService itemTrackingService,
        IParsedSpoilerDataService parsedSpoilerDataService,
        ISavedProgressProvider savedProgressProvider,
        // IAutotrackingHandoffRegistry autotrackingRegistry,
        IThemeService themeService
    )
    {
        _regionName = regionName;
        _itemTrackingService = itemTrackingService ?? throw new ArgumentNullException(nameof(itemTrackingService));
        _parsedSpoilerDataService = parsedSpoilerDataService ?? throw new ArgumentNullException(nameof(parsedSpoilerDataService));
        _savedProgressProvider = savedProgressProvider ?? throw new ArgumentNullException(nameof(savedProgressProvider));
        // _autotrackingRegistry = autotrackingRegistry ?? throw new ArgumentNullException(nameof(autotrackingRegistry));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));

        _itemTrackingService.ItemStateChanged += OnItemStateChanged;
        _parsedSpoilerDataService.ParsedSpoilerDataChanged += OnParsedSpoilerDataChanged;
        _savedProgressProvider.ProgressChanged += OnProgressChanged;
        // _autotrackingRegistry.RegisterRegionHandler(_regionName, TryAutoPlaceItem);

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

                PlacedItems.Add(new VialItemViewModel(
                itemName, // Fixed item name for START slots
                RegionName.START,
                vialColor,
                _itemTrackingService,
                _parsedSpoilerDataService,
                _themeService));
            }
        }
        else if (_parsedSpoilerDataService.CurrentData?.RegionData.TryGetValue(_regionName, out var regionData) == true)
        {
            for (int slotIndex = 0; slotIndex < regionData.VialColors.Count; slotIndex++)
            {
                var vialColor = regionData.VialColors[slotIndex];
                PlacedItems.Add(new VialItemViewModel(
                    null,
                    _regionName,
                    vialColor,
                    _itemTrackingService,
                    _parsedSpoilerDataService,
                    _themeService)
                );
            }
        }
        else
        {
            var itemsInRegion = _itemTrackingService.GetItemsInRegion(_regionName);
            foreach (var item in itemsInRegion)
            {
                PlacedItems.Add(new RegionItemViewModel(item.ItemName, _regionName, _itemTrackingService, _parsedSpoilerDataService, _themeService));
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
            slot is VialItemViewModel spoilerSlot &&
            spoilerSlot.VialColor == targetColor &&
            !spoilerSlot.CurrentItemName.HasValue);

            if (emptySlot == null)
            {
                return false;
            }

            // Check if the slot can accept the drop
            if (!emptySlot.CanAcceptDrop(itemToPlace, dragType))
            {
                return false;
            }

            // Actually mutate the slot state - this updates CurrentItemName and fires UpdateProperties
            return emptySlot.AcceptDropAndMutate(itemToPlace, dragType);
        }
        else
        {
            // In no-spoiler mode: always accept the drop
            // The tracking service will handle the actual state change (replace or add) via UiItemViewModel.CompleteDrag
            return true;
        }
    }

    #region Proxy Properties

    // Placed items in the grid
    private ObservableCollection<IRegionItemViewModel> _placedItems = [];
    public ObservableCollection<IRegionItemViewModel> PlacedItems
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

    private int _wothPoints = -1;
    public int WothPoints
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

    private int _itemPoints = -1;
    public int ItemPoints
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

    public bool ShouldShowItemPoints
    {
        get => _regionName != RegionName.START && HasItemPoints;
    }

    public bool ShouldShowRegionLevel
    {
        get => _regionName != RegionName.START && _regionName != RegionName.DK_ISLES;
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
                // _autotrackingRegistry.UnregisterRegionHandler(_regionName);
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
        bool isRelevantChange = e.ChangeReason == ChangeReason.UserModified
                    || e.ChangeReason == ChangeReason.AutoTracked;
                    //|| e.ChangeReason == "ItemMovedToUnknown"
                    //|| e.ChangeReason == ChangeReason. "ItemCleared";

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
        if (wothPoints < 0)
        {
            WothPoints = -1;
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
                if (item is VialItemViewModel spoiler)
                {
                    itemName = spoiler.CurrentItemName;
                }
                else if (item is RegionItemViewModel region)
                {
                    itemName = region.CurrentItemName;
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
            WothPoints = wothPoints - starredCount;
            HasWothPoints = true;
        }
    }

    internal void UpdateItemPointText()
    {
        // TODO: Have the point calculations take place in the service layer.
        var regionPoints = _parsedSpoilerDataService.GetPointsForRegion(_regionName);
        var pointSpread = _parsedSpoilerDataService.GetPointSpread();
        if (pointSpread.Count == 0)
        {
            ItemPoints = -1;
            HasItemPoints = false;
        }
        else
        {
            var itemsInRegion = _itemTrackingService.GetItemsInRegion(_regionName);
            var categories = itemsInRegion.Select(i => i.ItemName.ToPointCategory());
            var points = regionPoints - categories.Sum(c => pointSpread[c]);
            ItemPoints = points;
            HasItemPoints = points >= 0;
        }
    }

    internal void UpdateRegionResourceKey()
    {
        RegionResourceKey = _regionName.ToString().ToLowerInvariant();
    }

    /// <summary>
    /// Attempts to auto-place an item into an appropriate slot in this region.
    /// In spoiler mode: prioritizes empty slots, then non-autotracked slots.
    /// In non-spoiler mode: direct placement through ItemTrackingService.
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
                Autotracked: true,
                Opacity: 1.0
            );

            _itemTrackingService.SetItemState(itemToPlace, savedItem);
            return true;
        }

        // Spoiler mode: find a vial slot that matches the color
        var targetColor = itemToPlace.ToVialColor();

        var matchingSlots = PlacedItems
                    .OfType<VialItemViewModel>()
                    .Where(slot => slot.VialColor == targetColor)
                    .ToList();

        // No vials of this color in this region
        if (matchingSlots.Count == 0)
        {
            return false;
        }

        // Priority 1: Try empty slots first
        var emptySlots = matchingSlots
            .Where(slot => !slot.CurrentItemName.HasValue)
            .ToList();

        foreach (var slot in emptySlots)
        {
            if (slot.CanAcceptDrop(itemToPlace, MouseDragType.None)
                && slot.AcceptDropAndMutate(itemToPlace, MouseDragType.None))
            {
                return true;
            }
        }

        // Priority 2: Try non-autotracked slots (can be overridden)
        var overridableSlots = matchingSlots
            .Where(slot => slot.CurrentItemName.HasValue)
            .ToList();

        foreach (var slot in overridableSlots)
        {
            // Check if the current item is autotracked; if so, skip it
            var currentItem = _itemTrackingService.GetItemState(slot.CurrentItemName!.Value);
            if (currentItem?.Autotracked == true)
            {
                // Skip autotracked items—can't override them
                continue;
            }

            if (slot.CanAcceptDrop(itemToPlace, MouseDragType.None)
                && slot.AcceptDropAndMutate(itemToPlace, MouseDragType.None))
            {
                return true;
            }
        }

        // All matching slots either rejected placement or are protected autotracked items
        return false;
    }

    #region Equality Members

    public override bool Equals(object? obj) => obj is RegionViewModel other && other._regionName == _regionName;

    public override int GetHashCode() => _regionName.GetHashCode();

    #endregion
}
