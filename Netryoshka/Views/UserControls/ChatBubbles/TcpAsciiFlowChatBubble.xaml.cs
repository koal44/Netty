﻿using System;
using System.Windows;

namespace Netryoshka
{
    public partial class TcpAsciiFlowChatBubble
    {
        public TcpAsciiFlowChatBubble()
        {
            DataContextChanged += OnDataContextChanged;
            InitializeComponent();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is BubbleData bubbleData)
            {
                DataContext = new TcpAsciiChatBubbleViewModel(bubbleData);
            }
            else if (e.NewValue is null)
            {
                throw new InvalidOperationException("DataContext cannot be null");
            }
            else if (e.NewValue is TcpAsciiChatBubbleViewModel)
            {
                // It's already the type we want, so do nothing.
            }
            else
            {
                throw new InvalidOperationException($"Unexpected DataContext type. Expected: BubbleData or TcpHexChatBubbleViewModel, Actual: {e.NewValue.GetType().FullName}");
            }
        }

    }
}
