using Netryoshka.ViewModels.ChatBubbles;

namespace Netryoshka.ViewModels
{
    [CanContentScroll(true)]
    [RegisterBubbleViewModel("TcpAscii")]
    public partial class TcpAsciiBubbleViewModel : TcpBubbleViewModelBase
    {
        public TcpAsciiBubbleViewModel()
            : base(TcpEncoding.Ascii)
        { }


        public TcpAsciiBubbleViewModel(BubbleData data)
            : base(data, TcpEncoding.Ascii)
        { }
    }
}
