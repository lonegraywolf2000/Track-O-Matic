namespace TrackOMatic.Logic.Events;

public abstract class EventChangeData : EventArgs
{
    /// <summary>
    /// Gets the reason for the change, or null if not specified.
    /// </summary>
    public string? ChangeReason { get; init; }

    /// <summary>
    /// Gets the timestamp when the value change occurred.
    /// </summary>
    public DateTime ChangedAt { get; init; } = DateTime.UtcNow;
}
