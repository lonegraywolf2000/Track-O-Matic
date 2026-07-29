using System.Windows;

using Microsoft.Extensions.DependencyInjection;

using TrackOMatic.AutoTracking.Windows;
using TrackOMatic.Services;

namespace TrackOMatic;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        services.AddTrackOMaticServices();
        services.AddWindowsAutotracking();

        _serviceProvider = services.BuildServiceProvider();
        ServiceLocator.Initialize(_serviceProvider);
        Dispatcher.UnhandledException += OnDispatcherUnhandledException;
    }

    private void App_Exit(object sender, ExitEventArgs e)
    {
    }

    void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var settingsService = _serviceProvider.GetRequiredService<IUserSettingsService>();
        var appStateService = _serviceProvider.GetRequiredService<IApplicationStateService>();
        var dataPersistenceService = _serviceProvider.GetRequiredService<IDataPersistenceService>();
        var spoilerService = _serviceProvider.GetRequiredService<ISpoilerService>();
        var autotrackingService = _serviceProvider.GetRequiredService<IAutotrackerService>();
        MainWindow mainWindow = new
        (
            settingsService,
            appStateService,
            dataPersistenceService,
            spoilerService,
            autotrackingService
        );
        mainWindow.Show();

        UpdatePadBarrelImages();
        settingsService.PropertyChanged += (s, args) =>
        {
            if (args.PropertyName == nameof(IUserSettingsService.ColoredBarrelPadMoves))
            {
                UpdatePadBarrelImages();
            }
        };
    }

    public void UpdatePadBarrelImages()
    {
        var settingsService = _serviceProvider.GetRequiredService<IUserSettingsService>();
        var dicts = Resources.MergedDictionaries;
        dicts.Clear();
        dicts.Add(new ResourceDictionary
        {
            Source = new Uri("Dictionary1.xaml", UriKind.Relative)
        });
        var path = settingsService.ColoredBarrelPadMoves ? "ColoredBarrelPadImages.xaml" : "BaseBarrelPadImages.xaml";
        dicts.Add(new ResourceDictionary
        {
            Source = new Uri(path, UriKind.Relative)
        });
    }
}
