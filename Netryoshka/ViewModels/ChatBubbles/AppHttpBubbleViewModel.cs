﻿using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.Utils;
using System;
using System.ComponentModel;
using System.Windows;

namespace Netryoshka
{
    public partial class AppHttpBubbleViewModel : ObservableObject
    {

        [ObservableProperty]
        private FlowEndpointRole _endPointRole;
        [ObservableProperty]
        private string? _headerContent;
        [ObservableProperty]
        private string? _bodyContent;
        [ObservableProperty]
        private string? _footerContent;

        public AppHttpBubbleViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                //var packet = DesignTimeData.GetPackets()[0];
                EndPointRole = FlowEndpointRole.Pivot;
                HeaderContent = "IpHeader";
                BodyContent = "IpBody";
                FooterContent = "IpFooter";
            }
        }

        public AppHttpBubbleViewModel(BubbleData data)
        {
            EndPointRole = data.EndPointRole;
            HeaderContent = "";
            FooterContent = $"#{data.BubbleIndex} {data.PacketInterval:mm\\.ss\\.ffff}";
            var httpJsonList = JsonUtils.ExtractJsonObjectsFromKey(data.WireSharkData!.JsonString!, "http");
            BodyContent = StringUtils.StringJoin(Environment.NewLine, httpJsonList);
        }
        
    }
}
