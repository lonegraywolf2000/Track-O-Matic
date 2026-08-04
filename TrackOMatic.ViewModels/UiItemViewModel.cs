using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

using TrackOMatic.Logic.Enums;
using TrackOMatic.Logic.Events;
using TrackOMatic.Logic.Models;
using TrackOMatic.Services;

namespace TrackOMatic.ViewModels;

public class UiItemViewModel : BroadcastItemViewModel, INotifyPropertyChanged
{
    private readonly IItemTrackingService _itemTrackingService;
    private readonly IParsedSpoilerDataService _parsedSpoilerDataService;
    private readonly ItemName _itemName;

    public UiItemViewModel(
        ItemName itemName,
        IItemTrackingService itemTrackingService,
        IParsedSpoilerDataService parsedSpoilerDataService,
        IThemeService themeService
    ) : base(itemName, itemTrackingService, parsedSpoilerDataService, themeService)
    {
        _itemName = itemName;
        _itemTrackingService = itemTrackingService ?? throw new ArgumentNullException(nameof(itemTrackingService));
        _parsedSpoilerDataService = parsedSpoilerDataService ?? throw new ArgumentNullException(nameof(parsedSpoilerDataService));
        _itemTrackingService.ItemStateChanged += OnItemStateChanged;
        _parsedSpoilerDataService.ParsedSpoilerDataChanged += OnParsedSpoilerDataChanged;
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

    private double _opacity = 1.0;
    public double Opacity
    {
        get => _opacity;
        private set
        {
            if (_opacity != value)
            {
                _opacity = value;
                OnPropertyChanged();
            }
        }
    }

    private RegionName _regionName = RegionName.UNKNOWN;
    public RegionName RegionName
    {
        get => _regionName;
        private set
        {
            if (_regionName != value)
            {
                _regionName = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    public void ToggleStar()
    {
        var oldItem = _itemTrackingService.GetItemState(_itemName)
            ?? new SavedItem(_itemName, RegionName.UNKNOWN, ItemVisibilityState.Visible, false, 1.0);

        var newStarred = oldItem.Starred == ItemVisibilityState.Visible
            ? ItemVisibilityState.Hidden
            : ItemVisibilityState.Visible;

        SavedItem updatedItem = new(oldItem.ItemName, oldItem.Region, newStarred, oldItem.Autotracked, oldItem.Opacity);
        _itemTrackingService.SetItemState(_itemName, updatedItem);
    }

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
    private void OnParsedSpoilerDataChanged(object? sender, ParsedSpoilerDataChangedEventArgs e)
    {
        // When parsed spoiler data changes, reinitialize to reflect the new spoiler state
        InitializeState();
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
