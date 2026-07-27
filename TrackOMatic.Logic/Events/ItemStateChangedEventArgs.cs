using TrackOMatic.Logic.Models;

namespace TrackOMatic.Logic.Events;

public class ItemStateChangedEventArgs : EventChangeData
{
    public required SavedItem UpdatedItem { get; init; }

    public SavedItem? PreviousState { get; init; }

    public ItemStateChangedEventArgs(SavedItem updatedItem, SavedItem? previousState = null, string? changeReason = null)
    {
        UpdatedItem = updatedItem;
        PreviousState = previousState;
        ChangeReason = changeReason;
    }
}
