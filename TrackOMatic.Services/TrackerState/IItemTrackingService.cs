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
    /// Get all items in a specific region.
    /// </summary>
    IEnumerable<SavedItem> GetItemsInRegion(RegionName region);

    /// <summary>
    /// Get all items with a specific visibility state.
    /// </summary>
    IEnumerable<SavedItem> GetItemsByVisibility(ItemVisibilityState state);

    /// <summary>
    /// Toggles the star visibility state of an item.
    /// If no SavedItem exists for the item, creates one with the star toggled to Visible.
    /// This method centralizes star toggle logic to ensure consistent behavior across all view models.
    /// </summary>
    /// <param name="itemName">The name of the item whose star should be toggled.</param>
    void ToggleStar(ItemName itemName);

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
