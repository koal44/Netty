using System;

namespace Netryoshka
{
    public class BubbleData
    {
        public BasicPacket BasicPacket { get; set; }
        public FlowEndpointRole EndPointRole { get; set; }
        public TimeSpan? PacketInterval { get; set; }
        public WireSharkData? WireSharkData { get; set; }
        public int BubbleIndex { get; set; }

        public BubbleData(BasicPacket packet, FlowEndpointRole endPointRole, TimeSpan? packetInterval, int bubbleIndex)
        {
            BasicPacket = packet;
            EndPointRole = endPointRole;
            PacketInterval = packetInterval;
            BubbleIndex = bubbleIndex;
        }
    }

}
