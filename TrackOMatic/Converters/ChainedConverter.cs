using System.Globalization;
using System.Windows.Data;

namespace TrackOMatic.Converters;

/// <summary>
/// A chained converter that allows multiple IValueConverter instances to be applied in sequence.
/// </summary>
[ValueConversion(typeof(object), typeof(object))]
public class ChainedConverter : IValueConverter
{
    /// <summary>
    /// Gets the list of converters to be applied in sequence.
    /// </summary>
    public List<IValueConverter> Converters { get; } = [];
    /// <summary>
    /// Converts a value by applying each converter in the Converters list in sequence.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>The converted value.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        object currentValue = value;
        foreach (var converter in Converters)
        {
            currentValue = converter.Convert(currentValue, targetType, parameter, culture);
        }
        return currentValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
