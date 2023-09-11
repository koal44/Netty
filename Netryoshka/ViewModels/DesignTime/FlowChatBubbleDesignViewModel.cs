using System;
using System.Windows;

namespace Netryoshka.DesignTime
{
    public class FlowChatBubbleDesignViewModel
    {
        public FlowEndpointRole EndPointRole { get; }
        public string? HeaderContent { get; set; }
        public string? BodyContent { get; set; }
        public string? FooterContent { get; set; }

        public FlowChatBubbleDesignViewModel()
        {
            EndPointRole = FlowEndpointRole.Pivot;
            HeaderContent = "Header";
            BodyContent = "Body";
            FooterContent = "Footer";
        }
    }


}
