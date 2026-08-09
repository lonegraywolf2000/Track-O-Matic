using TrackOMatic.Logic;
using TrackOMatic.Logic.Enums;
using TrackOMatic.Logic.Models;
using TrackOMatic.Services;

namespace TrackOMatic.ViewModels;

/// <summary>
/// A view model representing a vial item by default, only to be covered by a real item when found in a region.
/// </summary>
public class VialItemViewModel : RegionItemViewModel, IVialSlot
{
    private readonly VialColor _vialColor;
    private readonly bool _isStartRegion;

    /// <summary>
    /// Gets the vial color in this slot.
    /// </summary>
    public VialColor VialColor => _vialColor;

    private new ItemName? _itemName;

    #region Proxy Properties
    private bool _isVialStarred = false;
    /// <summary>
    /// Gets a value indicating whether the vial (and not the item itself) is starred.
    /// </summary>
    /// <remarks>This is UI-only state, and thus it's not persisted to SavedProgress.</remarks>
    public bool IsVialStarred
    {
        get => _isVialStarred;
        private set
        {
            if (_isVialStarred != value)
            {
                _isVialStarred = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    public VialItemViewModel(
        ItemName? itemName,
        RegionName regionName,
        VialColor vialColor,
        IItemTrackingService itemTrackingService,
        IParsedSpoilerDataService parsedSpoilerDataService,
        IThemeService themeService
    ) : base(itemName, regionName, itemTrackingService, parsedSpoilerDataService, themeService)
    {
        if (regionName == RegionName.START && !itemName.HasValue)
        {
            throw new ArgumentException("Start region vial must have an item name.", nameof(itemName));
        }
        if (regionName != RegionName.START && itemName.HasValue)
        {
            throw new ArgumentException("Non-start region vial cannot have an item name.", nameof(itemName));
        }
        _vialColor = vialColor;
        _isStartRegion = regionName == RegionName.START;
        // Initialize the vial starred state based on the item state
        if (_isStartRegion && itemName.HasValue)
        {
            itemTrackingService.SetItemState(itemName.Value, SavedItem.CreateEmpty(itemName.Value) with
            {
                Region = regionName,
                Starred = ItemVisibilityState.Hidden,
                Autotracked = false,
                Opacity = 1.0,
                Hinted = false
            });
        }
    }

    #region IVialSlot Implementation

    public bool CanAcceptAutotrackedItem(ItemName itemToPlace, SavedItem itemState)
    {
        // Vial color must match the item type.
        if (itemToPlace.ToVialColor() != _vialColor)
        {
            return false;
        }

        // Start region: only the specific item can be placed in the start region.
        if (_isStartRegion)
        {
            return CurrentItemName == itemToPlace;
        }

        if (!CurrentItemName.HasValue)
        {
            return true;
        }

        var currentState = ItemTrackingService.GetItemState(CurrentItemName.Value);
        return currentState is not null && !currentState.Autotracked;
    }

    public void AcceptAutotrackedItem(ItemName itemToPlace, SavedItem itemState)
    {
        // Possibly redundant call, but good to have a backup.
        ItemTrackingService.SetItemState(itemToPlace, itemState);

        CurrentItemName = itemToPlace;
        UpdateProperties(itemState);
    }

    #endregion

    #region Overridden Methods

    /// <summary>
    /// Determine if this vial slot can accept the dropped item.
    /// </summary>
    /// <remarks>This checks the item type, vial color, and autotracked state.</remarks>
    /// <param name="itemToPlace">The item to place</param>
    /// <param name="dragType">The type of mouse drag</param>
    /// <returns>True if the drop can be accepted; otherwise, false.</returns>
    public override bool CanAcceptDrop(ItemName itemToPlace, MouseDragType dragType)
    {
        // Start region: user drag/drops forbidden, but autotracking is allowed
        if (_isStartRegion && dragType != MouseDragType.None)
        {
            return false;
        }

        // Vial color must match the item type.
        if (itemToPlace.ToVialColor() != _vialColor)
        {
            return false;
        }

        // If the slot is occupied by an autotracked item, it must be protected.
        if (CurrentItemName.HasValue && IsItemAutotracked(CurrentItemName.Value))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Accept the dropped item and mutate the state accordingly.
    /// </summary>
    /// <param name="itemToPlace">The item to place</param>
    /// <param name="dragType">The type of mouse drag</param>
    /// <returns>True if the drop was accepted and the state was mutated; otherwise, false.</returns>
    public override bool AcceptDropAndMutate(ItemName itemToPlace, MouseDragType dragType)
    {
        if (!CanAcceptDrop(itemToPlace, dragType))
        {
            return false;
        }

        // Transfer the star over.
        var hasStarToTransfer = _isVialStarred;
        IsVialStarred = false;

        var oldItemState = ItemTrackingService.GetItemState(itemToPlace) ?? SavedItem.CreateEmpty(itemToPlace);
        var newItemState = oldItemState with
        {
            Region = RegionName,
            Starred = hasStarToTransfer ? ItemVisibilityState.Visible : oldItemState.Starred,
        };
        ItemTrackingService.SetItemState(itemToPlace, newItemState);
        CurrentItemName = itemToPlace;
        UpdateProperties(newItemState);
        return true;
    }

    /// <summary>
    /// Removes the item from the region if possible.
    /// </summary>
    /// <remarks>Any failures to remove the item are silently ignored.</remarks>
    public override void RemoveFromRegion()
    {
        if (_isStartRegion)
        {
            return;
        }
        if (!CurrentItemName.HasValue)
        {
            return;
        }

        var currentState = ItemTrackingService.GetItemState(CurrentItemName.Value);
        if (currentState is null)
        {
            return;
        }
        if (IsItemAutotracked(CurrentItemName.Value))
        {
            return;
        }

        ItemTrackingService.SetItemState(CurrentItemName.Value, currentState with
        {
            Region = RegionName.UNKNOWN,
            Opacity = 1,
        });
        CurrentItemName = null;
        UpdateProperties(null);
    }

    /// <summary>
    /// Toggles the star state of the vial or the item, depending on whether an item is present.
    /// </summary>
    public override void ToggleStar()
    {
        if (CurrentItemName.HasValue)
        {
            base.ToggleStar();
        }
        else
        {
            IsVialStarred = !IsVialStarred;
            // Update the IsStarred property to reflect the vial star state
            UpdateProperties(null);
        }
    }

    protected override void UpdateProperties(SavedItem? itemState)
    {
        if (itemState is null || !CurrentItemName.HasValue)
        {
            IsStarred = IsVialStarred;
            ImageResourceKey = "vial_" + _vialColor.ToString().ToLower();
            Opacity = 1;
        }
        else
        {
            // Set image based on item state
            string baseKey = CurrentItemName.Value.ToString().ToLower();
            ImageResourceKey = itemState.Hinted ? $"{baseKey}_bw" : baseKey;
            IsStarred = itemState.Starred != ItemVisibilityState.Hidden;
            Opacity = itemState.Opacity;
        }
    }

    #endregion

    #region Helper Methods

    private bool IsItemAutotracked(ItemName itemName)
    {
        var itemState = ItemTrackingService.GetItemState(itemName);
        return itemState?.Autotracked ?? false;
    }

    #endregion
}
