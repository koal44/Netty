using System;
using System.Globalization;

namespace Netryoshka
{
    public class EnumToStringConverter : BaseValueConverter<EnumToStringConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) throw new ArgumentNullException(nameof(value), "Value cannot be null");

            Type enumType = value.GetType();
            if (!enumType.IsEnum) throw new ArgumentException("Value must be of enum type", nameof(value));
            //if (targetType != typeof(string)) throw new InvalidOperationException($"Cannot convert to target type {targetType.FullName}. Expected type is string.");
            if (Enum.GetName(enumType, value) is not string name) throw new InvalidOperationException($"Cannot convert enum {enumType.FullName} to string");

            return name;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
