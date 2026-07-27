namespace TrackOMatic.Logic.Events;

public class SpoilerLogStatusChangedEventArgs : EventChangeData
{
    /// <summary>
    /// New spoiler log file path (null if cleared).
    /// </summary>
    public string? SpoilerLogPath { get; init; }

    /// <summary>
    /// Whether the spoiler log was successfully loaded/parsed.
    /// </summary>
    public bool LoadSuccessful { get; init; }

    /// <summary>
    /// Error message if load failed (null if successful).
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Number of hints parsed from the spoiler log (informational).
    /// </summary>
    public int ParsedHintCount { get; init; }
}
