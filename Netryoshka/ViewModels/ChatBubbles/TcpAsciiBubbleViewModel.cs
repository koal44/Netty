using Netryoshka.ViewModels;

namespace Netryoshka
{
    public partial class TcpAsciiBubbleViewModel : TcpBubbleViewModelBase
    {
        public TcpAsciiBubbleViewModel(BubbleData data)
            : base(data, TcpEncoding.Ascii)
        {
        }
        
    }
}
