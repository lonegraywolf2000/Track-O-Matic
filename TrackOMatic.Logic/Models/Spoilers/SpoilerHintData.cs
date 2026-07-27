using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TrackOMatic.Logic.Models.Spoilers;

public class SpoilerHintData
{
    /// <summary>
    /// Gets the spoiler data for the Jungle Japes region.
    /// </summary>
    [JsonPropertyName("0")]
    public RegionSpoilerData? JapesData { get; init; }

    /// <summary>
    /// Gets the spoiler data for the Angry Aztec region.
    /// </summary>
    [JsonPropertyName("1")]
    public RegionSpoilerData? AztecData { get; init; }

    /// <summary>
    /// Gets the spoiler data for the Frantic Factory region.
    /// </summary>
    [JsonPropertyName("2")]
    public RegionSpoilerData? FactoryData { get; init; }

    /// <summary>
    /// <summary>
    /// Gets the spoiler data for the Gloomy Galleon region.
    /// </summary>
    [JsonPropertyName("3")]
    public RegionSpoilerData? GalleonData { get; init; }

    /// <summary>
    /// Gets the spoiler data for the Fungi Forest region.
    /// </summary>
    [JsonPropertyName("4")]
    public RegionSpoilerData? ForestData { get; init; }

    /// <summary>
    /// Gets the spoiler data for the Crystal Caves region.
    /// </summary>
    [JsonPropertyName("5")]
    public RegionSpoilerData? CavesData { get; init; }

    /// <summary>
    /// Gets the spoiler data for the Castle Crush region.
    /// </summary>
    [JsonPropertyName("6")]
    public RegionSpoilerData? CastleData { get; init; }

    /// <summary>
    /// Gets the spoiler data for the Helm's Deep region.
    /// </summary>
    [JsonPropertyName("7")]
    public RegionSpoilerData? HelmData { get; init; }

    /// <summary>
    /// Gets the spoiler data for the DK Isles region.
    /// </summary>
    [JsonPropertyName("8")]
    public RegionSpoilerData? IslesData { get; init; }

    /// <summary>
    /// Gets the starting info for the game, including starting items and other relevant information.
    /// </summary>
    [JsonPropertyName("starting_info")]
    public StartingInfoRaw? StartingInfo { get; init; }

    /// <summary>
    /// Gets the point spread for the game, which indicates how many points each item is worth.
    /// </summary>
    /// <remarks>The string should be converted to an enum for better typing.</remarks>
    [JsonPropertyName("point_spread")]
    public Dictionary<string, int>? PointSpread { get; init; }
}
