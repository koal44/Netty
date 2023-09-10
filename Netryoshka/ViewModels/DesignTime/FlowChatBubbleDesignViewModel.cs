using System;
using System.Windows;

namespace Netryoshka.DesignTime
{
    public class FlowChatBubbleDesignViewModel
    {
        //public BasicPacket BasicPacket { get; }
        public FlowEndpointRole EndPointRole { get; }
        //public TimeSpan? PacketInterval { get; }
        public string? HeaderContent { get; set; }
        public string? BodyContent { get; set; }
        public string? FooterContent { get; set; }
        //public Template Template { get; set; }

        public FlowChatBubbleDesignViewModel()
        {
            //BasicPacket = DesignTimeData.GetPackets()[0];
            EndPointRole = FlowEndpointRole.Pivot;
            //PacketInterval = new TimeSpan(0, 0, 0, 0, 500);
            HeaderContent = "Header";
            BodyContent = "Body";
            FooterContent = "Footer";
            //Template = Application.Current.FindResource("TcpHexDataTemplate") as Template ?? throw new InvalidOperationException("Could not find TcpHexDataTemplate");
        }
    }


}
