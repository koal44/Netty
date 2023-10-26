using Netryoshka.ViewModels;

namespace Netryoshka
{
    public partial class TcpHexBubbleViewModel : TcpBubbleViewModelBase
    {
        public TcpHexBubbleViewModel(BubbleData data)
            : base(data, TcpEncoding.Hex)
        {
        }
    }
}
