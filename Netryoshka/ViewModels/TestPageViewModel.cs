using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.DesignTime;
using System.Collections.Generic;

namespace Netryoshka.ViewModels
{
    public partial class TestPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<TreeNode> _treeNodes;

        public TestPageViewModel()
        {
            var packet = DesignTimeData.GetPackets()[0];
            TreeNodes = TreeNode.BuildNodesFromObject(packet);
        }


        //public List<string> HexBlobs { get; set; }
        //public TestPageViewModel()
        //{
        //    HexBlobs = new List<string>();

        //    for (int i = 0; i < 10; i++)
        //    {
        //        HexBlobs.Add(Util.GenerateRandomHexString(150));
        //    }
        //}
    }
}
