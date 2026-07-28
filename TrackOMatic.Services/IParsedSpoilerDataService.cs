using TrackOMatic.Logic.Models.Spoilers;

namespace TrackOMatic.Services;

/// <summary>
/// Provides access to the currently loaded parsed spoiler data.
/// This service maintains state across tracker resets and load operations.
/// </summary>
public interface IParsedSpoilerDataService
{
    /// <summary>
    /// Gets the current parsed spoiler data, or null if no spoiler has been loaded.
    /// </summary>
    ParsedSpoilerData? CurrentData { get; }

    /// <summary>
    /// Updates the parsed spoiler data (e.g., when a new spoiler file is loaded).
    /// </summary>
    /// <param name="data">The new parsed spoiler data, or null to clear.</param>
    void UpdateParsedData(ParsedSpoilerData? data);
}
