using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows;

namespace Netryoshka
{
    public partial class BubbleData
    {
        public BasicPacket BasicPacket { get;}
        public FlowEndpointRole EndPointRole { get; }
        public TimeSpan? PacketInterval { get; }
        public DataTemplate Template { get; set; }

        //[ObservableProperty]
        //private string? _headerContent;
        //[ObservableProperty]
        //private string? _bodyContent;
        //[ObservableProperty]
        //private string? _footerContent;
        //[ObservableProperty]
        //private Template _currentDataTemplate;

        //private string? _hexContent;
        //public string? HexContent => _hexContent ??= Convert.ToHexString(BasicPacket.Payload);
        //private string? _asciiContent;
        //public string? AsciiContent => _asciiContent ??= Utils.Util.BytesToAscii(BasicPacket.Payload);

        public BubbleData(BasicPacket packet, FlowEndpointRole endPointRole, TimeSpan? packetInterval)
        {
            BasicPacket = packet;
            EndPointRole = endPointRole;
            PacketInterval = packetInterval;
            Template = Application.Current.FindResource("TcpHexDataTemplate") as DataTemplate
                ?? throw new InvalidOperationException("TcpHexDataTemplate resource not found.");
        }
    }

}
