using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.Helpers;

namespace Netryoshka
{
    public partial class TcpAsciiBubbleViewModel : ObservableObject
    {

        [ObservableProperty]
        private FlowEndpointRole _endPointRole;
        [ObservableProperty]
        private string? _headerContent;
        [ObservableProperty]
        private string? _bodyContent;
        [ObservableProperty]
        private string? _footerContent;

        public TcpAsciiBubbleViewModel()
        {

        }

        public TcpAsciiBubbleViewModel(BubbleData data)
        {
            EndPointRole = data.EndPointRole;
            HeaderContent = BubbleDataHelper.BuildTcpHeaderContent(data);
            BodyContent = BubbleDataHelper.GetDecodedTcpPayloadContent(data, TcpEncoding.Ascii);
            FooterContent = $"#{data.BubbleIndex} {data.PacketInterval:mm\\.ss\\.ffff}";
        }
        
    }
}
