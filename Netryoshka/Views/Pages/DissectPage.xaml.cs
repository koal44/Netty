using Netryoshka.ViewModels;
using System.Windows.Controls;

namespace Netryoshka
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
