using System;
using System.Windows;

namespace Netryoshka
{
    public partial class FrameSharkJsonBubble
    {
        public FrameSharkJsonBubbleViewModel ViewModel { get; set; }

        public FrameSharkJsonBubble()
        {
            ViewModel = new FrameSharkJsonBubbleViewModel();
            InitializeComponent();
        }

    }
}
