using System.Windows;
using System.Windows.Controls;

namespace Netty
{
    public partial class FlowChatBubbleView : UserControl
    {
        public FlowChatBubbleView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty BubbleScaleProperty =
        DependencyProperty.Register("BubbleScale", typeof(double), typeof(FlowChatBubbleView), new PropertyMetadata(0.8));

        public double BubbleScale
        {
            get { return (double)GetValue(BubbleScaleProperty); }
            set { SetValue(BubbleScaleProperty, value); }
        }
    }
}
