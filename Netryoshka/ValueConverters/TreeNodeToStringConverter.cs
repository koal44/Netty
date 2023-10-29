using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Markup;

namespace Netryoshka
{
    public class TreeNodeToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not TreeNode node) return string.Empty;

            return !string.IsNullOrEmpty(node.PropertyValue)
                ? $"{node.PropertyName}: {node.PropertyValue}"
                : $"{node.PropertyName}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException($"{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}() has not been implemented.");
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
