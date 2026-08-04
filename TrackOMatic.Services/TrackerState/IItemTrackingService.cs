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

    /// <summary>
    /// Begins a batch update scope where multiple item changes are deferred.
    /// When the returned scope is disposed, all collected changes fire ItemStateChanged events.
    /// Use with a 'using' statement for automatic cleanup.
    /// </summary>
    /// <example>
    /// using (itemService.BeginBatchUpdate())
    /// {
    ///     itemService.SetItemState(item1, state1);
    ///     itemService.SetItemState(item2, state2);
    ///     itemService.ClearItemState(item3);
    /// } // Events fire here
    /// </example>
    IDisposable BeginBatchUpdate();
}
