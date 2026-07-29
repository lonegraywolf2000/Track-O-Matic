using System.ComponentModel;
using System.Runtime.CompilerServices;

using TrackOMatic.Logic.Enums;
using TrackOMatic.Logic.Events;
using TrackOMatic.Logic.Models;
using TrackOMatic.Services;

namespace TrackOMatic.ViewModels;

public class BroadcastItemViewModel : INotifyPropertyChanged
{
    private readonly IItemTrackingService _itemTrackingService;
    private readonly ISpoilerService _spoilerService;
    private readonly ItemName _itemName;

    // List of regions to show the full color icon for this item.
    private static readonly HashSet<RegionName> _validRegions =
    [
        RegionName.JUNGLE_JAPES, RegionName.ANGRY_AZTEC, RegionName.FRANTIC_FACTORY,
        RegionName.GLOOMY_GALLEON, RegionName.FUNGI_FOREST, RegionName.CRYSTAL_CAVES,
        RegionName.CREEPY_CASTLE, RegionName.HIDEOUT_HELM, RegionName.START,
        RegionName.DK_ISLES, RegionName.UNHINTABLE_MOVES
    ];

    public BroadcastItemViewModel(
        ItemName itemName,
        IItemTrackingService itemTrackingService,
        ISpoilerService spoilerService
    )
    {
        _itemName = itemName;
        _itemTrackingService = itemTrackingService ?? throw new ArgumentNullException(nameof(itemTrackingService));
        _spoilerService = spoilerService ?? throw new ArgumentNullException(nameof(spoilerService));
        _itemTrackingService.ItemStateChanged += OnItemStateChanged;

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
        private set
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
        private set
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
        private set
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

    // Cleanup
    public void Dispose()
    {
        _itemTrackingService.ItemStateChanged -= OnItemStateChanged;
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

    private void UpdateHoverText()
    {
        // Query spoiler service for item information (currently just placeholder)
        HoverText = $"{_itemName}";
        // TODO: Query _spoilerService for detailed hint/point information when available
    }

    private void UpdateImageResourceKey(SavedItem? item)
    {
        string baseKey = _itemName.ToString().ToLower();

        // If no item state, show default B&W variant
        if (item == null)
        {
            ImageResourceKey = $"{baseKey}_bw";
            return;
        }

        // If item is hinted, always show B&W
        if (item.Hinted)
        {
            ImageResourceKey = $"{baseKey}_bw";
            return;
        }

        // If item is in a valid region, show full-color
        if (_validRegions.Contains(item.Region))
        {
            ImageResourceKey = baseKey;
            return;
        }

        // Otherwise, item is in ItemGrid or unknown region, show B&W
        ImageResourceKey = $"{baseKey}_bw";
    }
}
