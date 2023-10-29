using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.DesignTime;
using Netryoshka.ViewModels.ChatBubbles;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Netryoshka.ViewModels
{
    [CanContentScroll(true)]
    [RegisterBubbleViewModel("FrameNoShark")]
    public partial class FrameNoSharkBubbleViewModel : BubbleViewModelBase
    {
        [ObservableProperty]
        private List<TreeNode> _treeNodes = new();


        public FrameNoSharkBubbleViewModel()
            : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var packet = DesignTimeData.GetPackets()[0];
                TreeNodes = TreeNode.BuildNodesFromObject(packet);
            }
        }


        public FrameNoSharkBubbleViewModel(BubbleData data)
            : base(data)
        {
            TreeNodes = TreeNode.BuildNodesFromObject(data.BasicPacket);
        }
    }
}
