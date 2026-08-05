using System.Globalization;
using System.Windows.Data;

using TrackOMatic.Logic.Enums;
using TrackOMatic.Logic.Models;

namespace TrackOMatic.Converters;

[ValueConversion(typeof(string), typeof(bool))]
[ValueConversion(typeof(RegionName), typeof(string))]
public class PlayableRegionConverter : IValueConverter
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

        return EndGameMappings.LOBBY_ORDER.Concat([RegionName.DK_ISLES]).Contains(targetRegion);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
