using Netty.DesignTime;
using System.Windows;
using System.Windows.Controls;

namespace Netty
{
    public partial class FlowsPage : Page
    {
        public FlowsPageViewModel ViewModel { get; }

        public FlowsPage(FlowsPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.ParseAsHttpAsync();
        }

    }
}
