using TrackOMatic.Logic.Events;

namespace TrackOMatic.Services.TrackerState;

/// <summary>
/// Manages spoiler log integration with saved game state.
/// </summary>
public interface ISpoilerLogService
{
    /// <summary>
    /// Get the current spoiler log file path.
    /// </summary>
    string? GetSpoilerLogPath();

    /// <summary>
    /// Set the spoiler log file path (does not load; coordinate with IDataPersistenceService).
    /// </summary>
    void SetSpoilerLogPath(string? path);

    /// <summary>
    /// Reload the spoiler log from disk.
    /// </summary>
    Task<bool> ReloadSpoilerLogAsync();

    /// <summary>
    /// Fired when spoiler log state changes.
    /// </summary>
    event EventHandler<SpoilerLogStatusChangedEventArgs>? SpoilerLogStatusChanged;
}
