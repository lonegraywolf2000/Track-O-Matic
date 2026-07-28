using System;

namespace TrackOMatic.Logic.Attributes;

/// <summary>
/// Maps an enum value to its corresponding label in the spoiler log.
/// Used to deserialize spoiler log strings into strongly-typed enums.
/// </summary>
/// <remarks>
/// Example: ANGRY_AZTEC → "Angry Aztec", DK_ISLES → "DK Isles"
/// The attribute handles cases where PascalCase conversion wouldn't work.
/// </remarks>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class SpoilerLogLabelAttribute : Attribute
{
    /// <summary>
    /// Gets the label as it appears in the spoiler log.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpoilerLogLabelAttribute"/> class.
    /// </summary>
    /// <param name="label">The exact label string from the spoiler log (e.g., "Angry Aztec").</param>
    public SpoilerLogLabelAttribute(string label)
    {
        Label = label;
    }
}
