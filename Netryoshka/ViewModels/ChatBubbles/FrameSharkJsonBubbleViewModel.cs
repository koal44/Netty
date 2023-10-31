using Netryoshka.DesignTime;
using System.ComponentModel;
using System.Windows;

namespace Netryoshka.ViewModels
{
    [CanContentScroll(true)]
    [RequiresWireShark]
    [RegisterBubbleViewModel("FrameSharkJson")]
    public partial class FrameSharkJsonBubbleViewModel : TreeViewBubbleViewModel
    {
        public FrameSharkJsonBubbleViewModel()
            : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var packet = DesignTimeData.GetPackets()[0];
                TreeNodes = TreeNode.BuildNodesFromObject(packet);
                IsExpanded = false;
            }
        }


        public FrameSharkJsonBubbleViewModel(BubbleData data)
            : base(data)
        {
            var layersObject = data.WireSharkData?.WireSharkPacket?.Source?.Layers ?? new TSharkLayers();
            TreeNodes = TreeNode.BuildNodesFromObject(layersObject);
            IsExpanded = false;
        }
    }
}
