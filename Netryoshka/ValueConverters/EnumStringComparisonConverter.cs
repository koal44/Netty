using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Markup;

namespace Netryoshka
{
    public class EnumStringComparisonConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum valueEnum && parameter is string parameterString)
            {
                return $"{valueEnum}" == parameterString;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException($"{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}() is not implemented yet.");
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

}
