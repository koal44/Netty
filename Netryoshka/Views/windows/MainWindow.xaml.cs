using Wpf.Ui.Contracts;
namespace Netryoshka;

public partial class MainWindow // : MainWindow // : Window
{
    public MainWindow(
        MainWindowViewModel viewModel,
        IPageService pageService,
        IContentDialogService contentDialogService,
        INavigationService navigationService)
    {
        DataContext = viewModel;
        InitializeComponent();

        SetPageService(pageService);
        contentDialogService.SetContentPresenter(RootContentDialog);
        navigationService.SetNavigationControl(MainNavBar);
        Loaded += (_, _) => MainNavBar.Navigate(typeof(CapturePage));

        //const Theme windowTheme = Theme.Dark;
        //DarkNet.Instance.SetWindowThemeWpf(this, windowTheme);
    }

    public void SetPageService(IPageService pageService) =>
        MainNavBar.SetPageService(pageService);


}
