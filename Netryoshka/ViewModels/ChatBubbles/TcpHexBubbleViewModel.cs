using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.DesignTime;
using Netryoshka.Helpers;
using System.ComponentModel;
using System.Windows;

namespace Netryoshka
{
    public partial class TcpHexBubbleViewModel : ObservableObject
    {

        [ObservableProperty]
        private FlowEndpointRole _endPointRole;
        [ObservableProperty]
        private string? _headerContent;
        [ObservableProperty]
        private string? _bodyContent;
        [ObservableProperty]
        private string? _footerContent;

        public TcpHexBubbleViewModel()
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

        public TcpHexBubbleViewModel(BubbleData data)
        {
            EndPointRole = data.EndPointRole;
            HeaderContent = BubbleDataHelper.BuildTcpHeaderContent(data);
            BodyContent = BubbleDataHelper.GetDecodedTcpPayloadContent(data, TcpEncoding.Hex);
            FooterContent = $"#{data.BubbleIndex} {data.PacketInterval:mm\\.ss\\.ffff}";
        }
        
    }
}
