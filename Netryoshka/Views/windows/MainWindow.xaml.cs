using Netryoshka.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui;

namespace Netryoshka;

public partial class MainWindow // : MainWindow // : Window
{
    private ILogger _logger;

    public MainWindow(
        MainWindowViewModel viewModel,
        IPageService pageService,
        IContentDialogService contentDialogService,
        INavigationService navigationService,
        ILogger logger)
    {
        DataContext = viewModel;
        _logger = logger;

        InitializeComponent();

        SetPageService(pageService);
        contentDialogService.SetContentPresenter(RootContentDialog);
        navigationService.SetNavigationControl(MainNavBar);
        Loaded += (_, _) => MainNavBar.Navigate(typeof(CapturePage));

        //PreviewMouseWheel += MainWindow_PreviewMouseWheel;

        //const Theme windowTheme = Theme.Dark;
        //DarkNet.Instance.SetWindowThemeWpf(this, windowTheme);
    }

    public void SetPageService(IPageService pageService) =>
        MainNavBar.SetPageService(pageService);

    // DON'T DELETE THIS (but don't subscribe unless debugging)
    private void MainWindow_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.OriginalSource is not FrameworkElement sourceElement)
        {
            _logger.Info("OriginalSource is not FrameworkElement");
            return;
        }

        var familyTuples = new List<(string name, string type)>();

        FrameworkElement? currentElement = sourceElement;
        while (currentElement != null)
        {
            var controlName = currentElement.Name ?? "?";
            var controlType = currentElement.GetType().Name ?? "?";
            familyTuples.Add((controlName, controlType));

            currentElement = VisualTreeHelper.GetParent(currentElement) as FrameworkElement;
        }

        familyTuples.Reverse();
        var displayString = string.Join("\n", familyTuples.Select((t, index) => $"{new string(' ', index)}- [{t.name}, {t.type}]"));
        _logger.Info($"MouseWheel event triggered by hierarchy:\n{displayString}");
        
        //var displayString = string.Join(" > ", familyTuples.Select(t => $"[{t.name}, {t.type}]"));
        //_logger.Info($"MouseWheel event triggered by hierarchy: {displayString}");
    }

}
