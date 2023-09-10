using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Netryoshka
{
    /// <summary>
    /// Base value converter allowing direct xaml use
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseValueConverter<T> : MarkupExtension, IValueConverter
        where T : class, new()
    {
        private static T? _converter;

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter ??= new T();
        }
    }
}
