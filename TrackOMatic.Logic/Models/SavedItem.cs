using TrackOMatic.Logic.Enums;

namespace TrackOMatic.Logic.Models;
/// <summary>
/// Represents a saved item with its associated data, including the item name, region, visibility state, autotracked status, opacity, and hinted status.
/// </summary>
/// <param name="itemName">The name of the item.</param>
/// <param name="region">The region where the item is located.</param>
/// <param name="starred">The visibility state of the item.</param>
/// <param name="autotracked">Indicates whether the item is autotracked.</param>
/// <param name="opacity">The opacity of the item.</param>
/// <param name="hinted">Indicates whether the item is hinted.</param>
public class SavedItem(ItemName itemName, RegionName region, ItemVisibilityState starred, bool autotracked, double opacity, bool hinted = false)
{
    public ItemName ItemName { get; } = itemName;
    public RegionName Region { get; set; } = region;
    public ItemVisibilityState Starred { get; set; } = starred;
    public bool Autotracked { get; set; } = autotracked;
    public double Opacity { get; set; } = opacity;
    public bool Hinted { get; set; } = hinted;
}
