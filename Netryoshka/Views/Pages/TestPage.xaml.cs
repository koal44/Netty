using Netryoshka.ViewModels;

namespace Netryoshka
{
    public partial class TestPage
    {
        public TestPage()
        {
            DataContext = new TestPageViewModel();
            InitializeComponent();
        }
    }
}
