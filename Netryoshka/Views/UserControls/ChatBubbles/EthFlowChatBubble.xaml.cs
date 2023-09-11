using System;
using System.Windows;

namespace Netryoshka
{
    public partial class EthFlowChatBubble
    {
        public EthFlowChatBubble()
        {
            DataContextChanged += OnDataContextChanged;
            InitializeComponent();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is BubbleData bubbleData)
            {
                DataContext = new EthChatBubbleViewModel(bubbleData);
            }
            else if (e.NewValue is null)
            {
                throw new InvalidOperationException("DataContext cannot be null");
            }
            else if (e.NewValue is EthChatBubbleViewModel)
            {
                // It's already the type we want, so do nothing.
            }
            else
            {
                throw new InvalidOperationException($"Unexpected DataContext type. Expected: BubbleData or EthChatBubbleViewModel, Actual: {e.NewValue.GetType().FullName}");
            }
        }

    }
}
