using TrackOMatic.Logic.Events;
using TrackOMatic.Logic.Models;
using TrackOMatic.Logic.Models.Spoilers;

namespace TrackOMatic.Services.TrackerState;

/// <summary>
/// A service for managing the currently loaded parsed spoiler data.
/// This service maintains state across tracker resets and load operations via <see cref="ISavedProgressProvider"/>.
/// </summary>
public class ParsedSpoilerDataService : IParsedSpoilerDataService
{
    private readonly ISavedProgressProvider _progressProvider;
    private ParsedSpoilerData? _currentData;

    public event EventHandler<ParsedSpoilerDataChangedEventArgs>? ParsedSpoilerDataChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParsedSpoilerDataService"/> class.
    /// </summary>
    /// <param name="progressProvider">The provider for saved progress state changes.</param>
    public ParsedSpoilerDataService(ISavedProgressProvider progressProvider)
    {
        _progressProvider = progressProvider ?? throw new ArgumentNullException(nameof(progressProvider));
        _currentData = null;
        // Set up listener for SavedProgress changes (e.g., reset or load)
        _progressProvider.ProgressChanged += OnProgressChanged;
    }

    /// <summary>
    /// Handles progress state changes by clearing the parsed spoiler data.
    /// This ensures that when tracker state is reset or loaded, the parsed data
    /// reflects the new state (empty until a new spoiler is loaded).
    /// </summary>
    private void OnProgressChanged(object? sender, ProgressReplacedEventArgs e)
    {
        // Clear the parsed data when progress changes (reset or load from file)
        UpdateParsedData(null);
    }

    #region IParsedSpoilerDataService Implementation

    public ParsedSpoilerData? CurrentData => _currentData;

    public void UpdateParsedData(ParsedSpoilerData? data)
    {
        var oldData = _currentData;
        _currentData = data;

        // Notify subscribers that the data has changed
        ParsedSpoilerDataChanged?.Invoke(this, new ParsedSpoilerDataChangedEventArgs(oldData, data));
    }

    #endregion
}
