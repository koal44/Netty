using Microsoft.Extensions.DependencyInjection;
using Netryoshka.Services;
using Netryoshka.ViewModels;
using System;
using System.Diagnostics;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using static Netryoshka.Services.ConfigurableLogger;

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

            Logger.Error(eArgs.Exception.Message);
            //Logger.Error(eArgs.Exception.Message, eArgs.Exception);

            // Prevent default unhandled exception processing
            //eArgs.Handled = true;

        };

        ApplicationThemeManager.Changed += (applicationTheme, accent) =>
        {
            UpdateProtonTheme(applicationTheme);
        };

        //base.OnStartup(e);
        PresentationTraceSources.RoutedEventSource.Switch.Level = SourceLevels.Verbose;
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

    private static void ConfigureServices(IServiceCollection services)
    {
        // core services
        services.AddSingleton<ILogger, ConfigurableLogger>(provider 
            => new ConfigurableLogger(new LoggerConfiguration
            {
                InfoTarget = LogTarget.Console,
                ErrorTarget = LogTarget.Console | LogTarget.Popup
            }));
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
        services.AddTransient<TestPage>();
        services.AddTransient<TestPageViewModel>();
    }

}
