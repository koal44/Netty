using Microsoft.Extensions.DependencyInjection;
using Netryoshka.Services;
using Netryoshka.ViewModels;
using System;
using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Contracts;
using Wpf.Ui.Services;

namespace Netryoshka;

public partial class App : Application
{
    public IServiceProvider? ServiceProvider { get; private set; }
    private ILogger? Logger { get; set; }

    protected void OnStartup(object sender, StartupEventArgs e)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();

        MainWindow mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        Logger = ServiceProvider.GetRequiredService<ILogger>();
        //Logger.Info("logging started");

        AppDomain.CurrentDomain.UnhandledException += (sender, eArgs) =>
        {
            // Log or display the exception details here
            if (eArgs.ExceptionObject is Exception ex)
            {
                Logger.Error(ex.Message);
            }
        };

        Application.Current.DispatcherUnhandledException += (sender, eArgs) =>
        {

            // Logger.Error(e.Exception);
            Logger.Error(eArgs.Exception.Message);

            // Prevent default unhandled exception processing
            eArgs.Handled = true;

            // Terminate the app if you think it's in an unstable state
            // Application.Current.Shutdown();

        };

        ApplicationThemeManager.Changed += (applicationTheme, accent) =>
        {
            UpdateProtonTheme(applicationTheme);
        };

        //base.OnStartup(e);
    }

    private static void UpdateProtonTheme(ApplicationTheme applicationTheme)
    {
        var libraryNamespace = "netty";
        var themeSpace = "skins";
        var libraryThemeDictionariesUri = "pack://application:,,,/Netryoshka;component/Skins/";
        var appDictionaries = new ResourceDictionaryManager(libraryNamespace);


        var themeDictionaryName = applicationTheme switch
        {
            ApplicationTheme.Dark => "Dark",
            ApplicationTheme.Light => "Light",
            ApplicationTheme.HighContrast => "HighContrast",
            _ => "Light"
        };

        var newResourceUri = new Uri(
                libraryThemeDictionariesUri + themeDictionaryName + ".xaml",
                UriKind.Absolute
            );

        appDictionaries.UpdateDictionary(themeSpace, newResourceUri);
    }

    /*private void ConfigureTheme()
    {
        //const Theme processTheme = Theme.Auto;
        //IDarkNet darkNet = DarkNet.Instance;
        //darkNet.SetCurrentProcessTheme(processTheme);
        //Logger?.Info($"Process theme is {processTheme}");
        //Logger?.Info($"System theme is {(darkNet.UserDefaultAppThemeIsDark ? "Dark" : "Light")}");
        //Logger?.Info($"Taskbar theme {(darkNet.UserTaskbarThemeIsDark ? "Dark" : "Light")}");

        //new SkinManager().RegisterSkins(new Uri("Skins/Skin.Light.xaml", UriKind.Relative), new Uri("Skins/Skin.Dark.xaml", UriKind.Relative));

        //DarkNet.Instance.UserDefaultAppThemeIsDarkChanged += (_, isSystemDarkTheme) => { Console.WriteLine($"System theme is {(isSystemDarkTheme ? "Dark" : "Light")}"); };
        //DarkNet.Instance.UserTaskbarThemeIsDarkChanged += (_, isTaskbarDarkTheme) => { Console.WriteLine($"Taskbar theme is {(isTaskbarDarkTheme ? "Dark" : "Light")}"); };
    }*/

    private static void ConfigureServices(IServiceCollection services)
    {
        // core services
        services.AddSingleton<ILogger, SimpleLogger>(provider => new SimpleLogger(logToConsole: true, logToPopup: true));
        services.AddSingleton<FlowManager>();
        services.AddTransient<ICaptureService, CaptureWindowsService>();
        services.AddSingleton<ISocketProcessMapperService, WindowsSocketProcessMapper>();
        services.AddSingleton<TSharkService>();
        services.AddSingleton<IFileDialogService, FileDialogService>();

        // wpf.ui services
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IContentDialogService, ContentDialogService>();

        // views/viewmodels
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<CapturePage>();
        services.AddSingleton<CapturePageViewModel>();
        services.AddTransient<FlowsPage>();
        services.AddTransient<FlowsPageViewModel>();
        services.AddTransient<DissectPage>();
        services.AddTransient<DissectPageViewModel>();

        // debug
        services.AddTransient<DevPage>();
    }

}
