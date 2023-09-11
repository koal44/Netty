using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.DesignTime;
using Netryoshka.Extensions;
using Netryoshka.Helpers;
using System;
using System.ComponentModel;
using System.Windows;

namespace Netryoshka
{
    public partial class FrameChatBubbleViewModel : ObservableObject
    {
        [ObservableProperty]
        private double _bubbleScale = 0.8;
        [ObservableProperty]
        private FlowEndpointRole _endPointRole;
        [ObservableProperty]
        private BasicPacket? _basicPacket;
        [ObservableProperty]
        private string? _footerContent;

        public FrameChatBubbleViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var packet = DesignTimeData.GetPackets()[0];
                EndPointRole = FlowEndpointRole.Pivot;
                BasicPacket = packet;
                FooterContent = TimeSpan.Zero.ToString("mm\\.ss\\.ffff");
            }
        }

        public FrameChatBubbleViewModel(BubbleData data)
        {
            EndPointRole = data.EndPointRole;
            BasicPacket = data.BasicPacket;
            FooterContent = data.PacketInterval?.ToString("mm\\.ss\\.ffff");
        }

    }
}
