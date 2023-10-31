using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.DesignTime;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Netryoshka.ViewModels
{
    public partial class TreeViewBubbleViewModel : BubbleViewModel
    {
        [ObservableProperty]
        private List<TreeNode> _treeNodes = new();
        [ObservableProperty]
        private bool _isExpanded;


        public TreeViewBubbleViewModel()
            : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var packet = DesignTimeData.GetPackets()[0];
                TreeNodes = TreeNode.BuildNodesFromObject(packet);
                IsExpanded = true;
            }
        }


        public TreeViewBubbleViewModel(BubbleData data)
            : base(data)
        {
            TreeNodes = new List<TreeNode>();
        }
    }
}
