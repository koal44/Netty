using Netryoshka.DesignTime;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Netryoshka.ViewModels
{
    [CanContentScroll(false)]
    [RequiresWireShark]
    [RegisterBubbleViewModel("AppTls")]
    public partial class AppTlsBubbleViewModel : TreeViewBubbleViewModel
    {
        public AppTlsBubbleViewModel()
            : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var packet = DesignTimeData.GetPackets()[0];
                TreeNodes = TreeNode.BuildNodesFromObject(packet);
                IsExpanded = true;
            }
        }


        public AppTlsBubbleViewModel(BubbleData data)
            : base(data)
        {
            var TlsList = data.WireSharkData?.WireSharkPacket.Source?.Layers?.Tls;
            TreeNodes = TlsList != null ? TreeNode.BuildNodesFromObject(TlsList, "tls") : new List<TreeNode>();
            IsExpanded = true;
        }
    }
}
