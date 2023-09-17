using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using static Netryoshka.BasicPacket;

namespace Netryoshka
{
    public class BasicPacketToTcpDisplayConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not BasicPacket packet)
            {
                throw new InvalidOperationException($"Expected BasicPacket, but got {value?.GetType().Name ?? "null"}");
            }
            var headers = packet.TcpHeadersData ?? throw new InvalidOperationException("TcpHeadersData is null");
            var role = headers.Role();

            string seq = $"Seq={headers.SequenceNumber}";
            string ack = $"Ack={headers.AcknowledgmentNumber}";

            var flags = new List<string>();
            if (headers.IsAcknowledgment) flags.Add("Ack");
            if (headers.IsSynchronize) flags.Add("Syn");
            if (headers.IsFinal) flags.Add("Fin");
            if (headers.IsReset) flags.Add("Rst");
            if (headers.IsPush) flags.Add("Psh");
            if (headers.IsUrgent) flags.Add("Urg");

            return role switch
            {
                TcpRole.Syn => $"Syn: {seq}",
                TcpRole.SynAck => $"SynAck: {seq}, {ack}",
                TcpRole.Fin => $"Fin: {seq}",
                TcpRole.FinAck => $"FinAck: {seq}, {ack}",
                TcpRole.Psh => $"Psh: {seq}",
                TcpRole.Urg => $"Urg: {seq}",
                TcpRole.Ack => $"Ack: {seq}, {ack}",
                TcpRole.Rst => $"Rst: {seq}",
                TcpRole.Other => $"Other: {seq}, {ack}, Flags: [{string.Join(", ", flags)}]",
                _ => throw new InvalidOperationException($"Unexpected TCP role: {role}")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("BasicPacketToTcpDisplayConverter.ConvertBack()");
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
