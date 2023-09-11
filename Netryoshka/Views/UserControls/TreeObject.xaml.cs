using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Netryoshka
{
    // https://michaelscodingspot.com/displaying-any-net-object-in-a-wpf-treeview/
    public partial class TreeObject : UserControl
    {
        public TreeObject()
        {
            InitializeComponent();
        }

        public object ObjectToVisualize
        {
            get { return (object)GetValue(ObjectToVisualizeProperty); }
            set { SetValue(ObjectToVisualizeProperty, value); }
        }
        public static readonly DependencyProperty ObjectToVisualizeProperty =
            DependencyProperty.Register("ObjectToVisualize", typeof(object), typeof(TreeObject), new PropertyMetadata(null, OnObjectChanged));

        private static void OnObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tree = TreeNode.CreateTree(e.NewValue);
            if (d is TreeObject obj)
            {
                obj.TreeNodes = new List<TreeNode>() { tree };
            }
        }

        public List<TreeNode> TreeNodes
        {
            get { return (List<TreeNode>)GetValue(TreeNodesProperty); }
            set { SetValue(TreeNodesProperty, value); }
        }
        public static readonly DependencyProperty TreeNodesProperty =
            DependencyProperty.Register("TreeNodes", typeof(List<TreeNode>), typeof(TreeObject), new PropertyMetadata(null));
    }
}
