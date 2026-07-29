namespace TrackOMatic.Logic.Events.Autotracking;

/// <summary>
/// Arguments for song change events.
/// </summary>
public class AutotrackerSongEventArgs : EventArgs
{
    public string SongGame { get; set; } = string.Empty;
    public string SongName { get; set; } = string.Empty;
}
