using System.Windows;
using System.Windows.Controls;

namespace Netryoshka
{
    public partial class FlowChatBubble
    {

        public FlowChatBubble()
        {
            //DataContext = this;
            InitializeComponent();
        }

        public static readonly DependencyProperty BubbleScaleProperty =
        DependencyProperty.Register("BubbleScale", typeof(double), typeof(FlowChatBubble), new PropertyMetadata(0.8));
        public double BubbleScale
        {
            get { return (double)GetValue(BubbleScaleProperty); }
            set { SetValue(BubbleScaleProperty, value); }
        }

        public static readonly DependencyProperty HeaderContentProperty = 
            DependencyProperty.Register(nameof(HeaderContent), typeof(string), typeof(FlowChatBubble), new PropertyMetadata(null));
        public string HeaderContent 
        { 
            get => (string)GetValue(HeaderContentProperty); 
            set => SetValue(HeaderContentProperty, value); 
        }

        public static readonly DependencyProperty BodyContentProperty =
            DependencyProperty.Register(nameof(BodyContent), typeof(string), typeof(FlowChatBubble), new PropertyMetadata(null));
        public string BodyContent
        {
            get => (string)GetValue(BodyContentProperty);
            set => SetValue(BodyContentProperty, value);
        }

        public static readonly DependencyProperty FooterContentProperty =
            DependencyProperty.Register(nameof(FooterContent), typeof(string), typeof(FlowChatBubble), new PropertyMetadata(null));
        public string FooterContent
        {
            get => (string)GetValue(FooterContentProperty);
            set => SetValue(FooterContentProperty, value);
        }

        public static readonly DependencyProperty EndPointRoleProperty =
            DependencyProperty.Register(nameof(EndPointRole), typeof(FlowEndpointRole), typeof(FlowChatBubble), new PropertyMetadata(FlowEndpointRole.Pivot));
        public FlowEndpointRole EndPointRole
        {
            get => (FlowEndpointRole)GetValue(EndPointRoleProperty);
            set => SetValue(EndPointRoleProperty, value);
        }

        private void CopyHeader_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(HeaderTextBlock.Text);
        }

        private void CopyBody_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(BodyTextBlock.Text);
        }
    }
}
