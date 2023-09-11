using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows;

namespace Netryoshka
{
    public partial class BubbleData : ObservableObject
    {
        [ObservableProperty]
        private BasicPacket _basicPacket;
        [ObservableProperty]
        private FlowEndpointRole _endPointRole;
        [ObservableProperty] 
        private TimeSpan? _packetInterval;
        [ObservableProperty]
        private DataTemplate _template;

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
