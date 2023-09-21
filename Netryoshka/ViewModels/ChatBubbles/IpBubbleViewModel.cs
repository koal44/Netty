﻿using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.DesignTime;
using Netryoshka.Helpers;
using System.ComponentModel;
using System.Windows;

namespace Netryoshka
{
    public partial class IpBubbleViewModel : ObservableObject
    {

        [ObservableProperty]
        private FlowEndpointRole _endPointRole;
        [ObservableProperty]
        private string? _headerContent;
        [ObservableProperty]
        private string? _bodyContent;
        [ObservableProperty]
        private string? _footerContent;

        public IpBubbleViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var packet = DesignTimeData.GetPackets()[0];
                EndPointRole = FlowEndpointRole.Pivot;
                HeaderContent = "IpHeader";
                BodyContent = "IpBody";
                FooterContent = "IpFooter";
            }
        }

        public IpBubbleViewModel(BubbleData data)
        {
            EndPointRole = data.EndPointRole;
            HeaderContent = "";
            FooterContent = data.PacketInterval?.ToString("mm\\.ss\\.ffff");
            BodyContent = $"IP: {data.BasicPacket.SrcEndpoint.IpAddress} ⟶ {data.BasicPacket.DstEndpoint.IpAddress}";
        }
        
    }
}