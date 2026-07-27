using TrackOMatic.Logic.Enums;

namespace TrackOMatic.Logic.Events;

/// <summary>
/// Represents the event arguments for when the amount of a B. Locker door changes in a specific region.
/// </summary>
/// <param name="regionName">The name of the region where the barrier constraint changed.</param>
/// <param name="oldValue">The old value of the barrier constraint.</param>
/// <param name="newValue">The new value of the barrier constraint.</param>
public class BlockerBarrierAmountChangedEventArgs(RegionName regionName, int oldValue, int newValue) : ValueChangedEventArgs<int>(oldValue, newValue)
{
    /// <summary>
    /// Gets the name of the region where the barrier constraint changed.
    /// </summary>
    public required RegionName RegionName { get; init; } = regionName;
}
