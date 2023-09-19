using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.DesignTime;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;

namespace Netryoshka
{
    public partial class AppHttpsBubbleViewModel : ObservableObject
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

        public AppHttpsBubbleViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var packet = DesignTimeData.GetPackets()[0];
                TreeNodes = new ObservableCollection<TreeNode> { TreeNode.BuildFromObject(packet) };
                EndPointRole = FlowEndpointRole.Pivot;
                FooterContent = TimeSpan.Zero.ToString("mm\\.ss\\.ffff");
            }
        }

        public AppHttpsBubbleViewModel(BubbleData data)
        {
            var httpNode = data.WireSharkData?.WireSharkPacket.Source?.Layers?.Http;
            var treeNode = httpNode != null ? TreeNode.BuildFromObject(httpNode) : new TreeNode();
            TreeNodes = new ObservableCollection<TreeNode> { treeNode };
            EndPointRole = data.EndPointRole;
            FooterContent = data.PacketInterval?.ToString("mm\\.ss\\.ffff");
        }

    }
}
