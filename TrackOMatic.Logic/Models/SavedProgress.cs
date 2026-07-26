using System.Text.Json.Serialization;

using TrackOMatic.Logic.Enums;

namespace TrackOMatic.Logic.Models;

public class SavedProgress
{
    public Dictionary<ItemName, SavedItem> SavedItems { get; }
    public Dictionary<RegionName, string> SavedGBCounts { get; set; }
    public Dictionary<RegionName, int> BLockerImageIndexes { get; set; }
    public List<int> HelmDoorImageIndexes { get; set; }
    public List<string> HelmDoorCounts { get; set; }
    public List<SavedHint> SavedHints { get; }
    public string spoilerPath { get; set; }
    public List<int> HelmKongs { get; set; }
    public List<int> BossKongs { get; set; }
    public List<int> LevelOrder { get; set; }
    [JsonIgnore]
    public Dictionary<ItemType, int> Collectibles { get; set; }
    [JsonIgnore]
    public bool IsSpoilerLoaded => !string.IsNullOrWhiteSpace(spoilerPath);

    public SavedProgress()
    {
        SavedItems = [];
        SavedHints = [];
        SavedGBCounts = [];
        BLockerImageIndexes = [];
        HelmDoorImageIndexes = [];
        HelmDoorCounts = [];
        Collectibles = [];
        HelmKongs = new(5);
        BossKongs = new(5);
        LevelOrder = new(8);
        spoilerPath = "";
    }
}
