using System.Reflection;

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
                return (T) field.GetValue(null)!;
            }
        }

        // Fallback to default
        return default!;

    }
}
