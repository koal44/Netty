using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.Helpers;

namespace Netryoshka
{
    public partial class TcpAsciiChatBubbleViewModel : ObservableObject
    {

        [ObservableProperty]
        private FlowEndpointRole _endPointRole;
        [ObservableProperty]
        private string? _headerContent;
        [ObservableProperty]
        private string? _bodyContent;
        [ObservableProperty]
        private string? _footerContent;

        public TcpAsciiChatBubbleViewModel()
        {

        }

        public TcpAsciiChatBubbleViewModel(BubbleData data)
        {
            EndPointRole = data.EndPointRole;
            HeaderContent = BubbleDataHelper.BuildTcpHeaderContent(data);
            BodyContent = BubbleDataHelper.GetDecodedTcpPayloadContent(data, TcpEncoding.Ascii);
            FooterContent = data.PacketInterval?.ToString("mm\\.ss\\.ffff");
        }
        
    }
}
