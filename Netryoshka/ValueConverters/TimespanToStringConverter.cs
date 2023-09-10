using Netryoshka;
using System;
using System.Globalization;

namespace Netryoshka
{
    public class TimeSpanToStringConverter : BaseValueConverter<TimeSpanToStringConverter>
    {
        public override object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                string format = parameter as string ?? "g";  // Default to "g" (general short)
                return timeSpan.ToString(format, culture);
            }
            return string.Empty;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
