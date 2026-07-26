namespace TrackOMatic.Logic;

/// <summary>
/// Attribute to map enum values to their legacy string representations for backward compatibility.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class LegacyNameAttribute(string legacyName) : Attribute
{
    public string LegacyName { get; } = legacyName;
}
