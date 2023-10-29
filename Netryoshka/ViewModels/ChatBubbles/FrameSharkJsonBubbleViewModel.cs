using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.DesignTime;
using Netryoshka.ViewModels.ChatBubbles;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Netryoshka.ViewModels
{
    [CanContentScroll(true)]
    [RequiresWireShark]
    [RegisterBubbleViewModel("FrameSharkJson")]
    public partial class FrameSharkJsonBubbleViewModel : BubbleViewModelBase
    {
        [ObservableProperty]
        private List<TreeNode> _treeNodes = new();
        [ObservableProperty]
        private bool _isExpanded = true;


        public FrameSharkJsonBubbleViewModel()
            : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var packet = DesignTimeData.GetPackets()[0];
                TreeNodes = TreeNode.BuildNodesFromObject(packet);
            }
        }


        public FrameSharkJsonBubbleViewModel(BubbleData data)
            : base(data)
        {
            var layersObject = data.WireSharkData?.WireSharkPacket?.Source?.Layers ?? new TSharkLayers();
            TreeNodes = TreeNode.BuildNodesFromObject(layersObject);
        }
    }
}
