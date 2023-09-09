using System;
using System.Globalization;
using System.Windows;

namespace Netty
{
    public class IncrementConverter : BaseValueConverter<IncrementConverter>
    {
        public double Amount { get; set; }
        public IncrementConverter()
        {
            Amount = 0;
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double dimension)
            {
                return dimension + Amount;
            }
            return value;
            //return DependencyProperty.UnsetValue;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double dimension)
            {
                return dimension - Amount;
            }
            return value;
            //return DependencyProperty.UnsetValue;
        }
    }

}
