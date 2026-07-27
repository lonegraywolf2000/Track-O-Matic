using TrackOMatic.Logic.Events;

namespace TrackOMatic.Services.TrackerState;

/// <summary>
/// Service for managing spoiler log integration (path, load status, reload coordination).
/// Coordinates with SpoilerParser to load and parse spoiler files.
/// </summary>
public class SpoilerLogService : ISpoilerLogService
{
    public event EventHandler<SpoilerLogStatusChangedEventArgs>? SpoilerLogStatusChanged;

    private string? _spoilerLogPath;

    public SpoilerLogService()
    {
        // TODO: Initialize from user preferences
    }

    public string? GetSpoilerLogPath()
    {
        return _spoilerLogPath;
    }

    public void SetSpoilerLogPath(string? path)
    {
        // TODO: Set path and raise status change event (without parsing)
        _spoilerLogPath = path;
    }

    public async Task<bool> ReloadSpoilerLogAsync()
    {
        // TODO: Validate path, parse spoiler, populate backing items, raise event
        throw new NotImplementedException();
    }
}
