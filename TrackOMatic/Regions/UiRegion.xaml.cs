using System.Windows;
using System.Windows.Controls;

using TrackOMatic.Logic.Enums;
using TrackOMatic.Services;
using TrackOMatic.Services.TrackerState;
using TrackOMatic.ViewModels;

namespace TrackOMatic.Regions;
/// <summary>
/// Interaction logic for UiRegion.xaml
/// </summary>
public partial class UiRegion : UserControl
{
    public static readonly DependencyProperty RegionNameProperty =
        DependencyProperty.Register(
        nameof(RegionName),
        typeof(RegionName),
        typeof(UiRegion),
        new PropertyMetadata(RegionName.UNKNOWN, OnRegionNameChanged)
    );

    public RegionName RegionName
    {
        get => (RegionName)GetValue(RegionNameProperty);
        set => SetValue(RegionNameProperty, value);
    }

    internal IParsedSpoilerDataService ParsedSpoilerDataService { get; private init; }
    internal ISavedProgressProvider SavedProgressProvider { get; private init; }

    public UiRegion()
    {
        ParsedSpoilerDataService = ServiceLocator.GetService<IParsedSpoilerDataService>() ?? throw new InvalidOperationException("IParsedSpoilerDataService not found in service locator.");
        SavedProgressProvider = ServiceLocator.GetService<ISavedProgressProvider>() ?? throw new InvalidOperationException("ISavedProgressProvider not found in service locator.");

        InitializeComponent();

        ParsedSpoilerDataService.ParsedSpoilerDataChanged += OnSpoilerDataChanged;
    }

    public bool TryDropItem(ItemName itemName, MouseDragType dragType)
    {
        if (DataContext is not RegionViewModel viewModel)
        {
            return false;
        }

        return viewModel.TryAcceptDrop(itemName, dragType);
    }

    private void OnSpoilerDataChanged(object? sender, ParsedSpoilerDataChangedEventArgs e)
    {
        // throw new NotImplementedException();
    }

    private static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UiRegion control && e.NewValue is RegionName regionName)
        {
            // Update the DataContext or any other properties based on the new RegionName
            var itemTrackingService = ServiceLocator.GetService<IItemTrackingService>();
            var orchestrator = ServiceLocator.GetService<IRegionPlacementOrchestrator>();
            var themeService = ServiceLocator.GetService<IThemeService>();
            var slotProviderRegistry = ServiceLocator.GetService<RegionSlotProviderRegistry>();

            if (itemTrackingService != null && orchestrator != null && control.ParsedSpoilerDataService != null && control.SavedProgressProvider != null && themeService != null)
            {
                control.DataContext = new RegionViewModel(
                    regionName,
                    itemTrackingService,
                    control.ParsedSpoilerDataService,
                    control.SavedProgressProvider,
                    orchestrator,
                    themeService,
                    slotProviderRegistry);
            }
        }
    }
}
