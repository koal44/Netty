using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

/// <summary>
/// Base multi-value converter allowing direct xaml use.
/// </summary>
/// <typeparam name="T">The type of converter.</typeparam>
public abstract class BaseMultiValueConverter<T> : MarkupExtension, IMultiValueConverter
    where T : class, new()
{
    private static T? _converter;

    public abstract object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);

    public abstract object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture);

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return _converter ??= new T();
    }
}
