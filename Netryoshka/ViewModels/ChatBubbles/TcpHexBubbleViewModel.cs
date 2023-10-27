using Netryoshka.ViewModels.ChatBubbles;

namespace Netryoshka.ViewModels
{
    [CanContentScroll(true)]
    [RegisterBubbleViewModel("TcpHex")]
    public partial class TcpHexBubbleViewModel : TcpBubbleViewModelBase
    {
        public TcpHexBubbleViewModel()
            : base(TcpEncoding.Hex)
        { }


        public TcpHexBubbleViewModel(BubbleData data)
            : base(data, TcpEncoding.Hex)
        { }
    }
}
