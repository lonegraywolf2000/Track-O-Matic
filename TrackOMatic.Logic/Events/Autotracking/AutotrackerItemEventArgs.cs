using TrackOMatic.Logic.Enums;

namespace TrackOMatic.Logic.Events.Autotracking;

public class AutotrackerItemEventArgs : EventArgs
{
    public ItemName ItemName { get; set; }
    public RegionName RegionName { get; set; }
    public bool IsHint { get; set; }
    public bool IsNewRegion { get; set; }
}
