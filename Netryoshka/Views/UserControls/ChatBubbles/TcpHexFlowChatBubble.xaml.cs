using System;
using System.Windows;

namespace Netryoshka
{
    public partial class TcpHexBubble
    {
        public TcpHexBubble()
        {
            DataContextChanged += OnDataContextChanged;
            InitializeComponent();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is BubbleData bubbleData)
            {
                DataContext = new TcpHexBubbleViewModel(bubbleData);
            }
            else if (e.NewValue is null)
            {
                throw new InvalidOperationException("DataContext cannot be null");
            }
            else if (e.NewValue is TcpHexBubbleViewModel)
            {
                // It's already the type we want, so do nothing.
            }
            else
            {
                throw new InvalidOperationException($"Unexpected DataContext type. Expected: BubbleData or TcpHexBubbleViewModel, Actual: {e.NewValue.GetType().FullName}");
            }
        }

    }
}
