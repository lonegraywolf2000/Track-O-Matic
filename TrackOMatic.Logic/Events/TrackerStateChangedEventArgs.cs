using TrackOMatic.Logic.Models;

namespace TrackOMatic.Logic.Events;

public class TrackerStateChangedEventArgs: EventChangeData
{
    /// <summary>
    /// The complete current tracker state.
    /// </summary>
    public required SavedProgress CurrentState { get; init; }

    /// <summary>
    /// Which domain was modified: "Items", "Barriers", "Hints", "Progression", or "Spoilers".
    /// </summary>
    public required string DomainChanged { get; init; }

    /// <summary>
    /// More specific description of what changed within the domain.
    /// Examples: "ItemVisibilityChanged", "BLockerCountUpdated", "HintMarkedAsUsed", etc.
    /// </summary>
    public string? SpecificChange { get; init; }
}
