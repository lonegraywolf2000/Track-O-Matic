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

public class RegionItemViewModel : IRegionItemViewModel, INotifyPropertyChanged
{
    private readonly ItemName _itemName;
    private readonly RegionName _regionName;
    private readonly IItemTrackingService _itemTrackingService;
    private readonly IParsedSpoilerDataService _parsedSpoilerDataService;
    private readonly IThemeService _themeService;

    public ItemName? CurrentItemName => _itemName;

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

    #endregion

    // TODO: Maybe move this out of the view model specifically?
    private static readonly HashSet<ItemName> BarrelPadItems = new()
    {
        ItemName.STRONG_KONG,
        ItemName.ROCKETBARREL_BOOST,
        ItemName.ORANGSTAND_SPRINT,
        ItemName.MINI_MONKEY,
        ItemName.HUNKY_CHUNKY,
        ItemName.BABOON_BLAST,
        ItemName.SIMIAN_SPRING,
        ItemName.MONKEYPORT,
        ItemName.GORILLA_GONE
    };

    public RegionItemViewModel(
        ItemName itemName,
        RegionName regionName,
        IItemTrackingService itemTrackingService,
        IParsedSpoilerDataService parsedSpoilerDataService,
        IThemeService themeService
    )
    {
        _itemName = itemName;
        _regionName = regionName;
        _itemTrackingService = itemTrackingService ?? throw new ArgumentNullException(nameof(itemTrackingService));
        _parsedSpoilerDataService = parsedSpoilerDataService ?? throw new ArgumentNullException(nameof(parsedSpoilerDataService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        // Subscribe to events
        _itemTrackingService.ItemStateChanged += OnItemStateChanged;
        if (BarrelPadItems.Contains(_itemName))
        {
            _themeService.BarrelPadThemeChanged += OnBarrelPadThemeChanged;
        }
        // Initialize state from service (if item exists)
        InitializeState();
    }

    private void InitializeState()
    {
        var itemState = _itemTrackingService.GetItemState(_itemName);
        UpdateProperties(itemState);
    }

    /// <summary>
    /// A no-op implementation since this slot is already occupied with an item and cannot accept any drops.
    /// </summary>
    /// <param name="itemToPlace"></param>
    /// <param name="dragType"></param>
    /// <returns></returns>
    public bool CanAcceptDrop(ItemName itemToPlace, MouseDragType dragType) => false;

    /// <summary>
    /// A no-op implementation since this slot is already occupied with an item and cannot accept any drops.
    /// </summary>
    /// <param name="itemToPlace"></param>
    /// <param name="dragType"></param>
    /// <returns></returns>
    public bool AcceptDropAndMutate(ItemName itemToPlace, MouseDragType dragType) => false;

    public void RemoveFromRegion()
    {
        var current = _itemTrackingService.GetItemState(_itemName);
        if (current is not null)
        {
            _itemTrackingService.SetItemState(_itemName, current with { Region = RegionName.UNKNOWN });
        }
    }

    public int GetHoardPoints()
    {
        if (CurrentItemName is null)
        {
            return 0;
        }
        var itemState = _itemTrackingService.GetItemState(CurrentItemName.Value);
        if (itemState is null)
        {
            return 0;
        }
        return itemState.Starred == ItemVisibilityState.Visible ? 1 : 0;
    }

    public int GetItemPoints()
    {
        if (CurrentItemName is null)
        {
            return 0;
        }

        return _parsedSpoilerDataService.GetPointsForItem(CurrentItemName.Value);
    }

    public virtual void ToggleStar()
    {
        if (CurrentItemName is null)
        {
            return;
        }
        _itemTrackingService.ToggleStar(CurrentItemName.Value);
    }

    private void OnItemStateChanged(object? sender, ItemStateChangedEventArgs e)
    {
        if (e.UpdatedItem.ItemName == _itemName)
        {
            UpdateProperties(e.UpdatedItem);
        }
    }

    private void OnBarrelPadThemeChanged(object? sender, EventArgs e)
    {
        // If the item is a barrel pad, we need to update the image resource key to reflect the new theme.
        if (BarrelPadItems.Contains(_itemName))
        {
            var itemState = _itemTrackingService.GetItemState(_itemName);
            // Force the frontend to re-evaluate the binding via temporary property change.
            ImageResourceKey = "";
            UpdateProperties(itemState);
        }
    }

    private void UpdateProperties(SavedItem? itemState)
    {
        if (itemState == null)
        {
            ImageResourceKey = "";
            IsStarred = false;
        }
        else
        {
            // Set image based on item state
            string baseKey = _itemName.ToString().ToLower();
            ImageResourceKey = itemState.Hinted ? $"{baseKey}_bw" : baseKey;
            IsStarred = itemState.Starred != ItemVisibilityState.Hidden;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    #region IDisposable

    private bool _disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            _itemTrackingService.ItemStateChanged -= OnItemStateChanged;
            if (BarrelPadItems.Contains(_itemName))
            {
                _themeService.BarrelPadThemeChanged -= OnBarrelPadThemeChanged;
            }
        }
    }

    ~RegionItemViewModel()
    {
        Dispose(false);
    }

    #endregion
}
