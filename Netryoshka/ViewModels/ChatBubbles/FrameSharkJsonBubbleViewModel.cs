using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.DesignTime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace Netryoshka
{
    public partial class FrameSharkJsonBubbleViewModel : ObservableObject
    {
        [ObservableProperty]
        private double _bubbleScale = 0.8;
        [ObservableProperty]
        private FlowEndpointRole _endPointRole;
        [ObservableProperty]
        private string? _footerContent;
        [ObservableProperty]
        private ObservableCollection<TreeNode> _treeNodes = new();
        [ObservableProperty]
        private bool _isExpanded = true;

        public FrameSharkJsonBubbleViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var packet = DesignTimeData.GetPackets()[0];
                TreeNodes = new ObservableCollection<TreeNode> { TreeNode.BuildFromObject(packet) };
                EndPointRole = FlowEndpointRole.Pivot;
                FooterContent = TimeSpan.Zero.ToString("mm\\.ss\\.ffff");
            }
        }

        public FrameSharkJsonBubbleViewModel(BubbleData data)
        {
            TreeNodes = new ObservableCollection<TreeNode>
            {
                TreeNode.BuildFromObject(
                    data.WireSharkData?.WireSharkPacket?.Source?.Layers
                    ?? new TSharkLayers()
                )
            };
            EndPointRole = data.EndPointRole;
            FooterContent = $"#{data.BubbleIndex} {data.PacketInterval:mm\\.ss\\.ffff}";
        }

    }
}
