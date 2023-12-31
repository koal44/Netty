﻿using Netryoshka.DesignTime;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Netryoshka.ViewModels
{
    [CanContentScroll(true)]
    [RequiresWireShark]
    [RegisterBubbleViewModel("AppHttp")]
    public partial class AppHttpBubbleViewModel : TreeViewBubbleViewModel
    {
        public AppHttpBubbleViewModel() : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var packet = DesignTimeData.GetPackets()[0];
                TreeNodes = TreeNode.BuildNodesFromObject(packet);
                IsExpanded = true;
            }
        }


        public AppHttpBubbleViewModel(BubbleData data)
            : base(data)
        {
            var httpList = data.WireSharkData?.WireSharkPacket.Source?.Layers?.Http;
            var http2List = data.WireSharkData?.WireSharkPacket.Source?.Layers?.Http2;

            var httpTreeNodes = httpList != null
                ? TreeNode.BuildNodesFromObject(httpList, "http")
                : Enumerable.Empty<TreeNode>();

            var http2TreeNodes = http2List != null
                ? TreeNode.BuildNodesFromObject(http2List, "http2")
                : Enumerable.Empty<TreeNode>();

            TreeNodes = httpTreeNodes.Concat(http2TreeNodes).ToList();
            IsExpanded = true;
        }
    }
}
