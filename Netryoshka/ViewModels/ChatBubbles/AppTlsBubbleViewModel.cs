using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.DesignTime;
using Netryoshka.ViewModels.ChatBubbles;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Netryoshka.ViewModels
{
    [CanContentScroll(false)]
    [RequiresWireShark]
    [RegisterBubbleViewModel("AppTls")]
    public partial class AppTlsBubbleViewModel : BubbleViewModelBase
    {
        [ObservableProperty]
        private List<TreeNode> _treeNodes = new();
        [ObservableProperty]
        private bool _isExpanded = true;


        public AppTlsBubbleViewModel()
            : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var packet = DesignTimeData.GetPackets()[0];
                TreeNodes = TreeNode.BuildNodesFromObject(packet);
            }
        }


        public AppTlsBubbleViewModel(BubbleData data)
            : base(data)
        {
            var TlsList = data.WireSharkData?.WireSharkPacket.Source?.Layers?.Tls;
            TreeNodes = TlsList != null ? TreeNode.BuildNodesFromObject(TlsList, "tls") : new List<TreeNode>();
        }
    }
}
