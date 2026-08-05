using System.ComponentModel;
using System.Runtime.CompilerServices;

using TrackOMatic.Logic.Enums;
using TrackOMatic.Logic.Events;
using TrackOMatic.Logic.Models;
using TrackOMatic.Services;

namespace TrackOMatic.ViewModels;

/// <summary>
/// A view model for an item that users can see with the dedicated broadcast view.
/// </summary>
/// <remarks>
/// This is meant to be a base class version.
///Additional view models can extend this for write functionality.
/// </remarks>
public class BroadcastItemViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IItemTrackingService _itemTrackingService;
    private readonly IParsedSpoilerDataService _parsedSpoilerDataService;
    private readonly IThemeService _themeService;
    private readonly ItemName _itemName;

    private static readonly HashSet<ItemName> BarrelPadItems = new()
    {
        ItemName.STRONG_KONG,
        ItemName.ROCKETBARREL_BOOST,
        ItemName.ORANGSTAND,
        ItemName.MINI_MONKEY,
        ItemName.HUNKY_CHUNKY,
        ItemName.BABOON_BLAST,
        ItemName.SIMIAN_SPRING,
        ItemName.MONKEYPORT,
        ItemName.GORILLA_GONE
    };

    public BroadcastItemViewModel(
        ItemName itemName,
        IItemTrackingService itemTrackingService,
        IParsedSpoilerDataService parsedSpoilerDataService,
        IThemeService themeService
    )
    {
        _itemName = itemName;
        _itemTrackingService = itemTrackingService ?? throw new ArgumentNullException(nameof(itemTrackingService));
        _parsedSpoilerDataService = parsedSpoilerDataService ?? throw new ArgumentNullException(nameof(parsedSpoilerDataService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _itemTrackingService.ItemStateChanged += OnItemStateChanged;
        _parsedSpoilerDataService.ParsedSpoilerDataChanged += OnParsedSpoilerDataChanged;

        if (BarrelPadItems.Contains(_itemName))
        {
            _themeService.BarrelPadThemeChanged += OnBarrelPadThemeChanged;
        }

        // Initialize state from service (if item exists)
        InitializeState();
    }

    /// <summary>
    /// Initialize properties from current service state.
    /// Called on construction and when state needs to be refreshed.
    /// </summary>
    private void InitializeState()
    {
        var itemState = _itemTrackingService.GetItemState(_itemName);

        if (itemState != null)
        {
            UpdateImageResourceKey(itemState);
            IsStarred = itemState.Starred != ItemVisibilityState.Hidden;
        }
        else
        {
            // No saved state yet - show default B&W variant
            UpdateImageResourceKey(null);
            IsStarred = false;
        }

        UpdateHoverText();
    }

    #region Proxy Properties

    private string _imageResourceKey = "";
    public string ImageResourceKey
    {
        get => _imageResourceKey;
        protected set
        {
            if (_imageResourceKey != value)
            {
                _imageResourceKey = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _isStarred = false;
    public bool IsStarred
    {
        get => _isStarred;
        protected set
        {
            if (_isStarred != value)
            {
                _isStarred = value;
                OnPropertyChanged();
            }
        }
    }

    private string _hoverText = "";
    public string HoverText
    {
        get => _hoverText;
        protected set
        {
            if (_hoverText != value)
            {
                _hoverText = value;
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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Unsubscribe from events to prevent memory leaks
            _itemTrackingService.ItemStateChanged -= OnItemStateChanged;
            _parsedSpoilerDataService.ParsedSpoilerDataChanged -= OnParsedSpoilerDataChanged;
            if (BarrelPadItems.Contains(_itemName))
            {
                _themeService.BarrelPadThemeChanged -= OnBarrelPadThemeChanged;
            }
        }
    }

    ~BroadcastItemViewModel()
    {
        Dispose(false);
    }

    #endregion

    private void OnItemStateChanged(object? sender, ItemStateChangedEventArgs e)
    {
        // Handle the item state change event here.
        if (e.UpdatedItem.ItemName == _itemName)
        {
            // Update the ImageResourceKey based on the new state of the item.
            UpdateImageResourceKey(e.UpdatedItem);
            IsStarred = e.UpdatedItem.Starred != ItemVisibilityState.Hidden;

            // Update hover text from spoiler service if available
            UpdateHoverText();
        }
    }

    /// <summary>
    /// Handles changes to the parsed spoiler data (load, reload, or clear).
    /// Reinitializes the image based on the new spoiler data.
    /// </summary>
    protected virtual void OnParsedSpoilerDataChanged(object? sender, ParsedSpoilerDataChangedEventArgs e)
    {
        // When parsed spoiler data changes, reinitialize to reflect the new spoiler state
        InitializeState();
    }

    private void OnBarrelPadThemeChanged(object? sender, EventArgs e)
    {
        // Force an update of the image resource key to reflect the new theme setting
        var itemState = _itemTrackingService.GetItemState(_itemName);
        ImageResourceKey = string.Empty;
        UpdateImageResourceKey(itemState);
    }

    private void UpdateHoverText()
    {
        // Query spoiler service for item information (currently just placeholder)
        HoverText = $"{_itemName}";
        // TODO: Query _spoilerService for detailed hint/point information when available
    }

    private void UpdateImageResourceKey(SavedItem? item)
    {
        string baseKey = _itemName.ToString().ToLower();
        RegionName spoiledRegion = RegionName.UNKNOWN;
        if (_parsedSpoilerDataService.CurrentData?.StartingItems.ContainsKey(_itemName) == true)
        {
            spoiledRegion = _parsedSpoilerDataService.CurrentData.StartingItems[_itemName];
        }

        // If no item state, show default B&W variant
        if (item == null && spoiledRegion == RegionName.UNKNOWN)
        {
            ImageResourceKey = $"{baseKey}_bw";
            return;
        }

        // If item is hinted, always show B&W
        // This logic may need to be revised.
        if (item?.Hinted == true)
        {
            ImageResourceKey = $"{baseKey}_bw";
            return;
        }

        // If item is in a valid region, show full-color
        if (spoiledRegion != RegionName.UNKNOWN || EndGameMappings.ValidMoveRegions.Contains(item?.Region ?? RegionName.UNKNOWN))
        {
            ImageResourceKey = baseKey;
            return;
        }

        // Otherwise, item is in ItemGrid or unknown region, show B&W
        ImageResourceKey = $"{baseKey}_bw";
    }
}
