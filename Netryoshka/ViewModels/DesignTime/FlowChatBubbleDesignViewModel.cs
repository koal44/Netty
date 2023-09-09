using System;

namespace Netty.DesignTime
{
    public class FlowChatBubbleDesignViewModel : IFlowChatBubbleViewModel
    {
        public string? Content { get; set; }
        public FlowEndpointRole EndPointRole { get; set; }
        public TimeSpan PacketInterval { get; set; }
        public string TcpHeadersDisplay { get; set; }
        public string MacHeadersDisplay { get; set; }
        public IFlowsPageViewModel ParentViewModel { get; }

        public FlowChatBubbleDesignViewModel()
        {
            Content = "Sample Content";
            EndPointRole = FlowEndpointRole.Pivot;
            PacketInterval = new TimeSpan(0, 0, 0, 0, 500);
            TcpHeadersDisplay = "Sample TCP Headers";
            MacHeadersDisplay = "Sample MAC Headers";
            ParentViewModel = FlowsPageDesignViewModel.Instance;
        }
    }


}
