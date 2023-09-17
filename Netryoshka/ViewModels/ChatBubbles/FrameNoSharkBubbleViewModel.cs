using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.DesignTime;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace Netryoshka
{
    public partial class FrameNoSharkBubbleViewModel : ObservableObject
    {
        [ObservableProperty]
        private double _bubbleScale = 0.8;
        [ObservableProperty]
        private FlowEndpointRole _endPointRole;
        [ObservableProperty]
        private string? _footerContent;
        [ObservableProperty]
        private ObservableCollection<TreeNode> _treeNodes = new();

        public FrameNoSharkBubbleViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var packet = DesignTimeData.GetPackets()[0];
                TreeNodes = new ObservableCollection<TreeNode> { TreeNode.BuildFromObject(packet) };
                EndPointRole = FlowEndpointRole.Pivot;
                FooterContent = TimeSpan.Zero.ToString("mm\\.ss\\.ffff");
            }
        }

        public FrameNoSharkBubbleViewModel(BubbleData data)
        {
            TreeNodes = new ObservableCollection<TreeNode> { TreeNode.BuildFromObject(data.BasicPacket) };
            EndPointRole = data.EndPointRole;
            FooterContent = data.PacketInterval?.ToString("mm\\.ss\\.ffff");
        }

    }
}
