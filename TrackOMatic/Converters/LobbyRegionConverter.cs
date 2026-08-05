using System.Globalization;
using System.Windows.Data;

using TrackOMatic.Logic.Enums;
using TrackOMatic.Logic.Models;

namespace TrackOMatic.Converters;

/// <summary>
/// A converter that checks if a given region is part of the lobby order defined in EndGameMappings.
/// </summary>
[ValueConversion(typeof(string), typeof(bool))]
[ValueConversion(typeof(RegionName), typeof(string))]
public class LobbyRegionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        RegionName targetRegion;
        if (value is string hopeful)
        {
            if (!Enum.TryParse(hopeful, out targetRegion))
            {
                return false;
            }
        }
        else if (value is RegionName region)
        {
            targetRegion = region;
        }
        else
        {
            return false;
        }

        return EndGameMappings.LOBBY_ORDER.Contains(targetRegion);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
