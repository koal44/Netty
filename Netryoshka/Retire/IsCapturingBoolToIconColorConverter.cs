using System;
//using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Proton
{
    public class IsCapturingBoolToIconColorConverter : BaseValueConverter<IsCapturingBoolToIconColorConverter>
    {
        private static readonly SolidColorBrush GrayBrush = new(Colors.Gray);

        static IsCapturingBoolToIconColorConverter()
        {

        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCapturing)
            {
                if (isCapturing)
                {
                    return "Red6Brush";
                    //return Application.Current.FindResource("Red6Brush");
                    //return new DynamicResourceExtension("Red6Brush");
                }
                else
                {
                    return "Green6Brush";
                    //return Application.Current.FindResource("Green6Brush");
                    //return new DynamicResourceExtension("Green6Brush");
                }
                
                //return isCapturing ? RedBrush : LightGreenBrush;
            }

            return "Green6Brush";
            //else { return GrayBrush; }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
