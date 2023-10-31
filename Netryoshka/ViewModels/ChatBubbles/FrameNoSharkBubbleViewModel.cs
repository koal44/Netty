using Netryoshka.DesignTime;
using System.ComponentModel;
using System.Windows;

namespace Netryoshka.ViewModels
{
    [CanContentScroll(true)]
    [RegisterBubbleViewModel("FrameNoShark")]
    public partial class FrameNoSharkBubbleViewModel : TreeViewBubbleViewModel
    {
        public FrameNoSharkBubbleViewModel()
            : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var packet = DesignTimeData.GetPackets()[0];
                TreeNodes = TreeNode.BuildNodesFromObject(packet);
                IsExpanded = false;
            }
        }


        public FrameNoSharkBubbleViewModel(BubbleData data)
            : base(data)
        {
            TreeNodes = TreeNode.BuildNodesFromObject(data.BasicPacket);
            IsExpanded = false;
        }
    }
}
