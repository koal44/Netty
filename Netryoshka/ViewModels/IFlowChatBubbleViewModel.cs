using System;
using System.Windows;

namespace Netryoshka
{
    public interface IFlowChatBubbleViewModel
    {
        BasicPacket BasicPacket { get; }
        FlowEndpointRole EndPointRole { get; }
        TimeSpan? PacketInterval { get; }
        string? HeaderContent { get; set; }
        string? BodyContent { get; set; }
        string? FooterContent { get; set; }
        DataTemplate CurrentDataTemplate { get; set; }
    }
}
