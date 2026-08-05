using System.Globalization;
using System.Windows.Data;

namespace TrackOMatic.Converters;

public class MultiToSingleConverter : IMultiValueConverter
{
    // Holds the primary converter that processes the object[] array
    public required IMultiValueConverter StartConverter { get; init; }

    // Holds any subsequent single-value steps
    public List<IValueConverter> TrailingConverters { get; set; } = [];

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (StartConverter == null)
        {
            return Binding.DoNothing;
        }

        // Step 1: Reduce the multi-value array into a single object
        object result = StartConverter.Convert(values, targetType, parameter, culture);

        // Step 2: Pass that single object through the remaining steps
        foreach (var converter in TrailingConverters)
        {
            result = converter.Convert(result, targetType, parameter, culture);
        }

        return result;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException("Complex pipeline reverse-evaluation not supported.");
}
