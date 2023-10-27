using Netryoshka.Utils;
using Netryoshka.ViewModels.ChatBubbles;
using System;
using System.Collections.Generic;
using static Netryoshka.BasicPacket;

namespace Netryoshka.ViewModels
{
    public abstract partial class TcpBubbleViewModelBase : BubbleViewModelBase
    {
        public TcpEncoding? Encoding { get; }


        protected TcpBubbleViewModelBase(TcpEncoding encoding)
            : base()
        {
            Encoding = encoding;
        }


        protected TcpBubbleViewModelBase(BubbleData data, TcpEncoding encoding)
            : base(data)
        {
            Encoding = encoding;
            HeaderContent = BuildTcpHeaderContent(data);
            BodyContent = GetDecodedTcpPayloadContent(data);
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
