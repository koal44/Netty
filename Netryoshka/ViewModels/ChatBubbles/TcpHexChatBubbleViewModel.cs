using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.DesignTime;
using Netryoshka.Helpers;
using System;
using System.ComponentModel;
using System.Windows;

namespace Netryoshka
{
    public partial class TcpHexChatBubbleViewModel : ObservableObject
    {

        [ObservableProperty]
        private FlowEndpointRole _endPointRole;
        [ObservableProperty]
        private string? _headerContent;
        [ObservableProperty]
        private string? _bodyContent;
        [ObservableProperty]
        private string? _footerContent;

        public TcpHexChatBubbleViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var packet = DesignTimeData.GetPackets()[0];
                EndPointRole = FlowEndpointRole.Pivot;
                HeaderContent = "TcpHeader";
                BodyContent = "TcpBody";
                FooterContent = "TcpFooter";
            }
        }

        public TcpHexChatBubbleViewModel(BubbleData data)
        {
            EndPointRole = data.EndPointRole;
            HeaderContent = BubbleDataHelper.BuildTcpHeaderContent(data);
            BodyContent = BubbleDataHelper.GetDecodedTcpPayloadContent(data, TcpEncoding.Hex);
            FooterContent = data.PacketInterval?.ToString("mm\\.ss\\.ffff");
        }
        
    }
}
