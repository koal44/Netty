using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Netryoshka
{
    public class MultiplicativeConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is double d1 && values[1] is double d2)
            {
                return d1 * d2;
            }
            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("MultiplicativeConverter is a one-way converter.");
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
