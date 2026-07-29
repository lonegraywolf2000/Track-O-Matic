using TrackOMatic.Logic.Enums;

namespace TrackOMatic.Logic.Events.Autotracking;

/// <summary>
/// Arguments for collectible count change events.
/// </summary>
public class AutotrackerCollectibleEventArgs : EventArgs
{
    public ItemType CollectibleType { get; set; }
    public int NewTotal { get; set; }
}
