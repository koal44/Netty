using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.Services;
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
        private WireSharkData? _wireSharkData;
        [ObservableProperty]
        private int _bubbleIndex;

        public BubbleData(BasicPacket packet, FlowEndpointRole endPointRole, TimeSpan? packetInterval, int bubbleIndex)
        {
            BasicPacket = packet;
            EndPointRole = endPointRole;
            PacketInterval = packetInterval;
            BubbleIndex = bubbleIndex;
        }
    }

}
