using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Proton.Retire
{
    public class IsCapturingBoolToIconColorConverter2 : IValueConverter
    {
        private static readonly SolidColorBrush GrayBrush = new SolidColorBrush(Colors.Gray);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

            return "Green6Brush"; // Return green by default, modify as per your requirement
                                  //return GrayBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
