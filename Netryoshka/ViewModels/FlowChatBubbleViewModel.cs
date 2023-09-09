using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using static Netty.BasicPacket;

namespace Netty
{
    public partial class FlowChatBubbleViewModel : ObservableObject, IFlowChatBubbleViewModel
    {
        public FlowEndpointRole EndPointRole { get; set; }
        public TimeSpan PacketInterval { get; set; }
        public string TcpHeadersDisplay { get; set; }
        public string MacHeadersDisplay { get; set; }

        [ObservableProperty]
        public string? _content;

        public IFlowsPageViewModel ParentViewModel { get; }

        private readonly byte[] _byteContent;

        private string? _hexContent;
        public string? HexContent => _hexContent ??= Convert.ToHexString(_byteContent);

        private string? _asciiContent;
        public string? AsciiContent => _asciiContent ??= Utils.Util.BytesToAscii(_byteContent);

        public FlowChatBubbleViewModel(BasicPacket packet, FlowEndpointRole endPointRole, IFlowsPageViewModel flowsPageViewModel, TimeSpan packetInterval = default)
        {
            EndPointRole = endPointRole;
            PacketInterval = packetInterval;
            TcpHeadersDisplay = BuildTcpHeadersDisplay(packet.TcpHeadersData);
            MacHeadersDisplay = BuildMacHeadersDisplay(packet.SrcEndpoint, packet.DstEndpoint);
            ParentViewModel = flowsPageViewModel;
            ParentViewModel.PropertyChanged += DataDisplayChangedHandler;

            _byteContent = packet.Payload;
            //UpdateDisplay();
        }

        // TODO: use this!
        public void UnSubscribeFromEvents()
        {
            ParentViewModel.PropertyChanged -= DataDisplayChangedHandler;
        }


        private void DataDisplayChangedHandler(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ParentViewModel.SelectedNetworkLayer): UpdateLayerDisplay(); break;
                case nameof(ParentViewModel.SelectedTcpEncoding): UpdateTcpDisplay(); break;
                case nameof(ParentViewModel.SelectedDeframeMethod): UpdateAppDisplay(); break;
                case nameof(ParentViewModel.MessagePrefixLength): UpdateAppPrefixDisplay(); break;
                case nameof(ParentViewModel.MessageTypeLength): UpdateAppPrefixDisplay(); break;
            }
        }

        

        private void UpdateLayerDisplay()
        {
            throw new NotImplementedException();
        }

        private void UpdateTcpDisplay()
        {
            Content = ParentViewModel.SelectedTcpEncoding switch
            {
                TcpEncoding.Ascii => AsciiContent,
                TcpEncoding.Hex => HexContent,
                _ => throw new NotImplementedException(),
            };
        }

        private void UpdateAppDisplay()
        {
            throw new NotImplementedException();
        }

        private void UpdateAppPrefixDisplay()
        {
            throw new NotImplementedException();
        }


        private static string BuildMacHeadersDisplay(FlowEndpoint srcEndPoint, FlowEndpoint dstEndPoint)
        {
            return $"MacSrc={srcEndPoint.MacAddress}, MacDst={dstEndPoint.MacAddress}";
        }

        private static string BuildTcpHeadersDisplay(TcpHeaders? nullableheaders)
        {
            if (nullableheaders is not TcpHeaders headers) return "N/A";

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

        [RelayCommand]
        public static void Copy(string textToCopy)
        {
            Clipboard.SetText(textToCopy);
        }
    }


}
