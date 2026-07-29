using TrackOMatic.Logic.Models.Spoilers;

namespace TrackOMatic.Services;

/// <summary>
/// Provides access to the currently loaded parsed spoiler data.
/// This service maintains state across tracker resets and load operations.
/// </summary>
public interface IParsedSpoilerDataService
{
    /// <summary>
    /// Event raised when the parsed spoiler data changes (loaded, reloaded, or cleared).
    /// </summary>
    event EventHandler<ParsedSpoilerDataChangedEventArgs>? ParsedSpoilerDataChanged;

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

/// <summary>
/// Event args for when parsed spoiler data changes.
/// </summary>
public class ParsedSpoilerDataChangedEventArgs : EventArgs
{
    public ParsedSpoilerData? OldData { get; }
    public ParsedSpoilerData? NewData { get; }

    public ParsedSpoilerDataChangedEventArgs(ParsedSpoilerData? oldData, ParsedSpoilerData? newData)
    {
        OldData = oldData;
        NewData = newData;
    }
}
