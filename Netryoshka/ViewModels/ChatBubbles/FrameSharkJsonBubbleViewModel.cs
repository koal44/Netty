using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.DesignTime;
using Netryoshka.ViewModels.ChatBubbles;
using System.Collections.ObjectModel;
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
        private ObservableCollection<TreeNode> _treeNodes = new();
        [ObservableProperty]
        private bool _isExpanded = true;


        public FrameSharkJsonBubbleViewModel()
            : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var packet = DesignTimeData.GetPackets()[0];
                TreeNodes = new ObservableCollection<TreeNode> { TreeNode.BuildFromObject(packet) };
            }
        }


        public FrameSharkJsonBubbleViewModel(BubbleData data)
            : base(data)
        {
            TreeNodes = new ObservableCollection<TreeNode>
            {
                TreeNode.BuildFromObject(
                    data.WireSharkData?.WireSharkPacket?.Source?.Layers
                    ?? new TSharkLayers()
                )
            };
        }
    }
}
