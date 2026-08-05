using System.Globalization;
using System.Windows.Data;

namespace TrackOMatic.Converters;

/// <summary>
/// A converter that takes a string representing a count and returns a color name based on the count value.
/// </summary>
[ValueConversion(typeof(string), typeof(string))]
public class ItemPointCountToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string textCount)
        {
            if (int.TryParse(textCount, out int count))
            {
                return count == 0 ? "RegionComplete" : "RegionInProgress";
            }
        }
        return "RegionInProgress";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
