using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Netryoshka
{
    public class InteractionEndpointAndModeToTextConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is not InteractionEndpoint endpoint || values[1] is not InteractionComboBoxMode mode)
            {
                return "";
            }

            var res = mode switch
            {
                InteractionComboBoxMode.ProcessInfo => endpoint.ProcessInfo?.ProcessName,
                InteractionComboBoxMode.DomainName => endpoint.DomainName,
                _ => null,
            };

            return $"{res ?? endpoint.FlowEndpoint.IpAddress.ToString()} [{endpoint.FlowEndpoint.Port}]";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

}
