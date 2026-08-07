using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using TrackOMatic.Logic.Enums;
using TrackOMatic.Services;
using TrackOMatic.Services.TrackerState;
using TrackOMatic.ViewModels;

namespace TrackOMatic.Regions;

/// <summary>
/// Interaction logic for RegionLevel.xaml
/// </summary>
public partial class RegionLevel : UserControl
{
    public static readonly DependencyProperty RegionNameProperty = DependencyProperty.Register(
        nameof(RegionName),
        typeof(RegionName),
        typeof(RegionLevel),
        new PropertyMetadata(RegionName.SHOPS, OnRegionNameChanged)
    );

    internal IUserSettingsService UserSettings { get; private init; }
    internal ILevelOrderService LevelOrder { get; private init; }
    internal IParsedSpoilerDataService SpoilerData { get; private init; }
    internal ISavedProgressProvider SavedProgressProvider { get; private init; }

    public RegionName RegionName
    {
        get => (RegionName)GetValue(RegionNameProperty);
        set => SetValue(RegionNameProperty, value);
    }

    public RegionLevel()
    {
        UserSettings = ServiceLocator.GetService<IUserSettingsService>() ?? throw new InvalidOperationException($"{nameof(IUserSettingsService)} not found in service locator.");
        LevelOrder = ServiceLocator.GetService<ILevelOrderService>() ?? throw new InvalidOperationException($"{nameof(ILevelOrderService)} not found in service locator.");
        SpoilerData = ServiceLocator.GetService<IParsedSpoilerDataService>() ?? throw new InvalidOperationException($"{nameof(IParsedSpoilerDataService)} not found in service locator.");
        SavedProgressProvider = ServiceLocator.GetService<ISavedProgressProvider>() ?? throw new InvalidOperationException($"{nameof(ISavedProgressProvider)} not found in service locator.");
        InitializeComponent();
    }

    private static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RegionLevel control && e.NewValue is RegionName regionName)
        {
            control.DataContext = new RegionLevelViewModel(
                regionName,
                control.LevelOrder,
                control.UserSettings,
                control.SpoilerData,
                control.SavedProgressProvider
            );
        }
    }

    private void RegionLevel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not RegionLevelViewModel viewModel)
        {
            return;
        }

        viewModel.IncrementLevelOrder();
    }

    private void RegionLevel_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not RegionLevelViewModel viewModel)
        {
            return;
        }

        viewModel.DecrementLevelOrder();
    }

    private void RegionLevel_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (DataContext is not RegionLevelViewModel viewModel)
        {
            return;
        }

        viewModel.HandleMouseWheel(e.Delta);
        e.Handled = true;
    }
}
