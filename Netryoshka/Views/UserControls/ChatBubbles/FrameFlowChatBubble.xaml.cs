using System;
using System.Windows;

namespace Netryoshka
{
    public partial class FrameFlowChatBubble
    {
        public FrameChatBubbleViewModel ViewModel { get; set; }
        public FrameFlowChatBubble()
        {
            ViewModel = new FrameChatBubbleViewModel();
            DataContextChanged += OnDataContextChanged;
            InitializeComponent();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is BubbleData bubbleData)
            {
                ViewModel = new FrameChatBubbleViewModel(bubbleData);
                DataContext = ViewModel;
            }
        }

    }
}
