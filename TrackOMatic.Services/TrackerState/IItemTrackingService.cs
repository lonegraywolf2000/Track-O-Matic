using TrackOMatic.Logic.Enums;
using TrackOMatic.Logic.Events;
using TrackOMatic.Logic.Models;

namespace TrackOMatic.Services;

/// <summary>
/// Manages item collection state across all regions.
/// </summary>
public interface IItemTrackingService
{
    /// <summary>
    /// Get the tracking state for a specific item.
    /// </summary>
    SavedItem? GetItemState(ItemName itemName);

    /// <summary>
    /// Update tracking state for an item (replaces entire entry).
    /// </summary>
    void SetItemState(ItemName itemName, SavedItem state);

    /// <summary>
    /// Remove tracking state for an item.
    /// </summary>
    void ClearItemState(ItemName itemName);

    /// <summary>
    /// Update a specific property of an item without replacing the entire entry.
    /// </summary>
    void UpdateItemProperty(ItemName itemName, Action<SavedItem> updateAction);

    /// <summary>
    /// Update the region of an item and evaluate whether it should still exist.
    /// Returns true if item remains, false if it was removed.
    /// </summary>
    bool UpdateItemRegion(ItemName itemName, RegionName newRegion);

    /// <summary>
    /// Get all items in a specific region.
    /// </summary>
    IEnumerable<SavedItem> GetItemsInRegion(RegionName region);

    /// <summary>
    /// Get all items with a specific visibility state.
    /// </summary>
    IEnumerable<SavedItem> GetItemsByVisibility(ItemVisibilityState state);

    /// <summary>
    /// Fired when any item state changes.
    /// </summary>
    event EventHandler<ItemStateChangedEventArgs>? ItemStateChanged;
}
