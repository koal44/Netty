using System;
using System.Globalization;

namespace Proton.Retire
{
    /// <summary>
    /// converts the <see cref="EAppPage"/> to an actual view/page
    /// </summary>
    public class ApplicationPageValueConverter : BaseValueConverter<ApplicationPageValueConverter>
    {

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
            //switch ((EAppPage)value)
            //{
            //    case EAppPage.CapturePage:
            //        return new CapturePage();
            //    default:
            //        throw new Exception("expected to return a page");
            //}
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
