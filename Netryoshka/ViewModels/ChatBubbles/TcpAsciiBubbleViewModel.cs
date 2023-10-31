using System.ComponentModel;
using System.Windows;

namespace Netryoshka.ViewModels
{
    [CanContentScroll(true)]
    [RegisterBubbleViewModel("TcpAscii")]
    public partial class TcpAsciiBubbleViewModel : TextBubbleViewModel
    {
        public TcpEncoding Encoding { get; } = TcpEncoding.Ascii;

        public TcpAsciiBubbleViewModel()
            : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                BodyContent = "Ascii";
            }
        }


        public TcpAsciiBubbleViewModel(BubbleData data)
            : base(data)
        {
            HeaderContent = TcpHexBubbleViewModel.GetTcpHeaderContent(data);
            BodyContent = TcpHexBubbleViewModel.GetTcpBodyContent(data, Encoding);
        }
    }
}
