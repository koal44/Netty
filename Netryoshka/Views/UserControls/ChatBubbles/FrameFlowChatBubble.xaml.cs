using System;
using System.Windows;

namespace Netryoshka
{
    public partial class FrameBubble
    {
        public FrameBubbleViewModel ViewModel { get; set; }
        public FrameBubble()
        {
            ViewModel = new FrameBubbleViewModel();
            DataContextChanged += OnDataContextChanged;
            InitializeComponent();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is BubbleData bubbleData)
            {
                ViewModel = new FrameBubbleViewModel(bubbleData);
                DataContext = ViewModel;
            }
        }

    }
}
