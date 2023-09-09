using System;

namespace Netty
{
    public interface IFlowChatBubbleViewModel
    {
        string? Content { get; set; }
        FlowEndpointRole EndPointRole { get; set; }
        TimeSpan PacketInterval { get; set; }
        string TcpHeadersDisplay { get; set; }
        string MacHeadersDisplay { get; set; }
        IFlowsPageViewModel ParentViewModel { get; }
    }
}
