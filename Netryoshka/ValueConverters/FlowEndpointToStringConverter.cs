using System;
using System.Globalization;

namespace Netryoshka
{
    public class FlowEndpointToStringConverter : BaseValueConverter<FlowEndpointToStringConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "null";
            }
            if (value is FlowEndpoint flowEndpoint)
            {
                return $"{flowEndpoint.IpAddress} [{flowEndpoint.Port}]";
            }
            // happens when clearing/reseting endpoints
            if (value is string valueString && string.IsNullOrEmpty(valueString))
            {
                return string.Empty; 
            }

            throw new InvalidOperationException($"Invalid binding type. Expected FlowEndpoint, got {value?.GetType().Name ?? "null"}.");
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
