using System.Windows.Controls;

namespace Netryoshka
{
    public partial class FlowsPage : Page
    {
        public FlowsPage(FlowsPageViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
