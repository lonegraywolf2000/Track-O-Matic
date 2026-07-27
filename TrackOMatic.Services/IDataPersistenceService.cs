using TrackOMatic.Logic.Events;
using TrackOMatic.Logic.Models;

namespace TrackOMatic.Services;

/// <summary>
/// Service for loading and saving game progress data to/from JSON files.
/// This service handles all file I/O and serialization without any UI coupling.
/// </summary>
public interface IDataPersistenceService
{
    /// <summary>
    /// Load saved progress from a JSON file.
    /// </summary>
    /// <param name="filePath">Path to the JSON file (e.g., "autosave.json")</param>
    /// <returns>Deserialized SavedProgress object, or null if file doesn't exist or is invalid</returns>
    SavedProgress? Load(string filePath);

    /// <summary>
    /// Save game progress to a JSON file.
    /// </summary>
    /// <param name="progress">The SavedProgress object to persist</param>
    /// <param name="filePath">Target file path (e.g., "autosave.json")</param>
    /// <exception cref="IOException">If write operation fails</exception>
    void Save(SavedProgress progress, string filePath);

    /// <summary>
    /// Clear in-memory cached state.
    /// </summary>
    void Reset();

    /// <summary>
    /// Fired when data is successfully saved.
    /// </summary>
    event EventHandler<SavedProgressChangedEventArgs>? SaveCompleted;

    /// <summary>
    /// Fired when a save operation fails.
    /// </summary>
    event EventHandler<string>? SaveFailed;
}
