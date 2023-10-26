using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.DesignTime;
using Netryoshka.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using static Netryoshka.BasicPacket;

namespace Netryoshka.ViewModels
{
    public abstract partial class TcpBubbleViewModelBase : ObservableObject
    {
        public TcpEncoding? Encoding { get; }

        [ObservableProperty]
        private FlowEndpointRole _endPointRole;
        [ObservableProperty]
        private string? _headerContent;
        [ObservableProperty]
        private string? _bodyContent;
        [ObservableProperty]
        private string? _footerContent;


        protected TcpBubbleViewModelBase()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var packet = DesignTimeData.GetPackets()[0];
                EndPointRole = FlowEndpointRole.Pivot;
                HeaderContent = "TcpHeader";
                BodyContent = "TcpBody";
                FooterContent = "TcpFooter";
            }
        }


        protected TcpBubbleViewModelBase(BubbleData data, TcpEncoding encoding)
        {
            Encoding = encoding;
            EndPointRole = data.EndPointRole;
            HeaderContent = BuildTcpHeaderContent(data);
            BodyContent = GetDecodedTcpPayloadContent(data);
            FooterContent = $"#{data.BubbleIndex} {data.PacketInterval:mm\\.ss\\.ffff}";
        }


        private string GetDecodedTcpPayloadContent(BubbleData data)
        {
            var packet = data.BasicPacket;
            return Encoding switch
            {
                TcpEncoding.Hex => Convert.ToHexString(packet.Payload),
                TcpEncoding.Ascii => BitUtils.BytesToAscii(packet.Payload),
                _ => throw new InvalidOperationException($"Unexpected TCP encoding: {Encoding}")
            };
        }


        private static string? BuildTcpHeaderContent(BubbleData data)
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
