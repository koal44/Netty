using Netryoshka.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using static Netryoshka.BasicPacket;

namespace Netryoshka.ViewModels
{
    [CanContentScroll(true)]
    [RegisterBubbleViewModel("TcpHex")]
    public partial class TcpHexBubbleViewModel : TextBubbleViewModel
    {
        public TcpEncoding Encoding { get; } = TcpEncoding.Hex;

        public TcpHexBubbleViewModel()
            : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                HeaderContent = "Header";
                BodyContent = "Hex";
            }
        }


        public TcpHexBubbleViewModel(BubbleData data)
            : base(data)
        {
            HeaderContent = GetTcpHeaderContent(data);
            BodyContent = GetTcpBodyContent(data, Encoding);
        }



        public static string GetTcpBodyContent(BubbleData data, TcpEncoding encoding)
        {
            var packet = data.BasicPacket;
            return encoding switch
            {
                TcpEncoding.Hex => Convert.ToHexString(packet.Payload),
                TcpEncoding.Ascii => BitUtils.BytesToAscii(packet.Payload),
                _ => throw new InvalidOperationException($"Unexpected TCP encoding: {encoding}")
            };
        }


        public static string? GetTcpHeaderContent(BubbleData data)
        {
            var packet = data.BasicPacket;

            var headers = packet.TcpHeadersData
                ?? throw new InvalidOperationException("TcpHeadersData is null");
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
    }
}
