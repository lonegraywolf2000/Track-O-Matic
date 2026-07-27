using TrackOMatic.Logic.Events;
using TrackOMatic.Logic.Models;

namespace TrackOMatic.Services.TrackerState;

/// <summary>
/// Coordinates access to all tracker state domains.
/// Serves as the primary abstraction for MainWindow to query and update game state.
/// </summary>
public interface ITrackerStateService
{
    /// <summary>
    /// Get the current tracker state snapshot.
    /// </summary>
    SavedProgress GetCurrentState();

    /// <summary>
    /// Replace the entire tracker state (e.g., when loading a save file).
    /// </summary>
    void LoadState(SavedProgress newState);

    /// <summary>
    /// Reset all state to defaults.
    /// </summary>
    void Reset();

    /// <summary>
    /// Fired when any domain state changes.
    /// </summary>
    event EventHandler<TrackerStateChangedEventArgs>? StateChanged;
}
