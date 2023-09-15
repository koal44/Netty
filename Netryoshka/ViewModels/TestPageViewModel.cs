using Netryoshka.DesignTime;
using System.Collections.ObjectModel;

namespace Netryoshka.ViewModels
{
    public class TestPageViewModel
    {
        public ObservableCollection<TreeNode> Nodes { get; set; }

        public TestPageViewModel()
        {
            var packet = DesignTimeData.GetPackets()[0];
            var node = TreeNode.BuildFromObject(packet);
            Nodes = new ObservableCollection<TreeNode>() { node };
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
