using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Netryoshka
{
    public class JsonNodeToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not TreeNode jsonNode) return string.Empty;

            var name = string.IsNullOrEmpty(jsonNode.PropertyName) ? "" : $"{jsonNode.PropertyName}: ";
            return $"{name}{jsonNode.PropertyValue}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
