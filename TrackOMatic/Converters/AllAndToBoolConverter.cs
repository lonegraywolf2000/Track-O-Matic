using System.Globalization;
using System.Windows.Data;

namespace TrackOMatic.Converters;

/// <summary>
/// A converter that takes an array of boolean values and returns true if all values are true, otherwise returns false.
/// </summary>
[ValueConversion(typeof(bool[]), typeof(bool))]
public class AllAndToBoolConverter : IMultiValueConverter
{
    /// <summary>
    /// Converts an array of boolean values to a single boolean value that is true if all values are true, otherwise false.
    /// </summary>
    /// <param name="values">An array of boolean values to be evaluated.</param>
    /// <param name="targetType">Unused</param>
    /// <param name="parameter">Unused</param>
    /// <param name="culture">Unused</param>
    /// <returns>True if all values are true, otherwise false.</returns>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null)
        {
            return false;
        }

        foreach (var value in values)
        {
            if (value is bool b && !b)
            {
                return false;
            }
        }
        return true;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
