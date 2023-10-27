using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.DesignTime;
using Netryoshka.ViewModels.ChatBubbles;
using System.Collections.ObjectModel;
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
        private ObservableCollection<TreeNode> _treeNodes = new();
        [ObservableProperty]
        private bool _isExpanded = true;


        public AppTlsBubbleViewModel()
            : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var packet = DesignTimeData.GetPackets()[0];
                TreeNodes = new ObservableCollection<TreeNode> { TreeNode.BuildFromObject(packet) };
            }
        }


        public AppTlsBubbleViewModel(BubbleData data)
            : base(data)
        {
            var TlsNode = data.WireSharkData?.WireSharkPacket.Source?.Layers?.Tls;
            var treeNode = TlsNode != null ? TreeNode.BuildFromObject(TlsNode) : new TreeNode();
            TreeNodes = new ObservableCollection<TreeNode> { treeNode };
        }
    }
}
