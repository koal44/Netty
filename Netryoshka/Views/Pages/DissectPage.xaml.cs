using Netty.ViewModels;
using System.Windows.Controls;

namespace Netty
{
    /// <summary>
    /// Interaction logic for DissectPage.xaml
    /// </summary>
    public partial class DissectPage : Page
    {
        public DissectPageViewModel ViewModel { get; }

        public DissectPage(DissectPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
