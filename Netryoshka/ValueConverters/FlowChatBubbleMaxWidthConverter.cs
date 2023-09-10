using System;
using System.Globalization;

namespace Netryoshka
{
    public class FlowChatBubbleMaxWidthConverter : BaseMultiValueConverter<FlowChatBubbleMaxWidthConverter>
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is double actualWidth && values[1] is double bubbleScale)
            {
                return actualWidth * bubbleScale;
            }
            return 0; 
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
