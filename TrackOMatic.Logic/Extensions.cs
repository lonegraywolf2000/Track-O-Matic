using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TrackOMatic.Logic.Attributes;
using TrackOMatic.Logic.Enums;
using TrackOMatic.Logic.Models;

namespace TrackOMatic.Logic;

public static class Extensions
{
    /// <summary>
    /// Converts a legacy name string (from <see cref="LegacyNameAttribute"/>)
    /// to the corresponding enum value.
    /// </summary>
    public static T FromLegacyToEnum<T>(this string value) where T : struct, Enum
    {
        // Try direct enum parse first
        if (Enum.TryParse<T>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        // Try legacy names
        foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var legacyNameAttr = field.GetCustomAttribute<LegacyNameAttribute>();
            if (legacyNameAttr?.LegacyName == value)
            {
                return (T)field.GetValue(null)!;
            }
        }

        // Fallback to default
        return default!;

    }

    /// <summary>
    /// Determines whether a <see cref="SavedItem"/> entry should be kept based on its state.
    /// Rules:
    /// 1. If starred, always keep
    /// 2. If hinted with opacity 0.375 and region is not <see cref="RegionName.UNKNOWN"/>, keep (right-click drag hint)
    /// 3. If found (opacity >= 1.0) in non-<see cref="RegionName.UNKNOWN"/> region and not autotracked, keep
    /// 4. If autotracked (opacity >= 1.0, region != <see cref="RegionName.UNKNOWN"/>), keep
    /// Otherwise remove.
    /// </summary>
    /// <returns>A boolean indicating whether the entry is kept.</returns>
    public static bool ShouldKeepSavedItem(this SavedItem item)
    {
        // Rule 1: Starred items always kept
        if (item.Starred == ItemVisibilityState.Visible)
        {
            return true;
        }

        // Rule 2: Right-click drag hint (opacity 0.375 in a region)
        if (Math.Abs(item.Opacity - 0.375) < 0.01 && item.Region != RegionName.UNKNOWN)
        {
            return true;
        }

        // Rule 3 & 4: Found or autotracked items
        if (item.Opacity >= 1.0 && item.Region != RegionName.UNKNOWN)
        {
            // Either found (not autotracked) or autotracked - both are valid
            return true;
        }

        // No valid reason to keep
        return false;
    }

    /// <summary>
    /// Gets the spoiler log label for a <see cref="RegionName"/> value.
    /// If no label is defined, returns the enum value name (UPPER_CASE format).
    /// </summary>
    /// <param name="region">The region enum value.</param>
    /// <returns>The spoiler log label string, or the enum name if no attribute is defined.</returns>
    public static string GetSpoilerLogLabel(this RegionName region)
    {
        var field = region.GetType().GetField(region.ToString());
        if (field == null)
        {
            return region.ToString();
        }

        var attribute = field.GetCustomAttribute<SpoilerLogLabelAttribute>();
        return attribute?.Label ?? region.ToString();
    }

    /// <summary>
    /// Attempts to parse a spoiler log label into a <see cref="RegionName"/> enum value.
    /// Invalid labels return <see cref="RegionName.UNKNOWN"/>.
    /// </summary>
    /// <param name="label">The spoiler log label (e.g., "Angry Aztec").</param>
    /// <returns>
    /// The corresponding <see cref="RegionName"/> value, or <see cref="RegionName.UNKNOWN"/> if not found.
    /// </returns>
    public static RegionName TryParseRegionFromLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return RegionName.UNKNOWN;
        }

        var fields = typeof(RegionName).GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var field in fields)
        {
            var attribute = field.GetCustomAttribute<SpoilerLogLabelAttribute>();
            if (attribute?.Label == label)
            {
                return (RegionName)field.GetValue(null)!;
            }
        }

        return RegionName.UNKNOWN;
    }
}
