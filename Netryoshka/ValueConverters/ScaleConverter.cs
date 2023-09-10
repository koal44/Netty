using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Netryoshka
{
    public class ScaleConverter : MarkupExtension, IValueConverter
    {
        public double Amount { get; set; }

        public ScaleConverter()
        {
            Amount = 1.0;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double originalValue)
            {
                return originalValue * Amount;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double scaledValue)
            {
                return scaledValue / Amount;
            }
            return value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
