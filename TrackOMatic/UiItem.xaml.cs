using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using TrackOMatic.Logic.Enums;
using TrackOMatic.Services;
using TrackOMatic.ViewModels;

namespace TrackOMatic;
/// <summary>
/// Interaction logic for UiItem.xaml
/// </summary>
public partial class UiItem : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty ItemNameProperty = DependencyProperty.Register(
        nameof(ItemName),
        typeof(ItemName),
        typeof(UiItem),
        new PropertyMetadata(ItemName.BASIC_KEY, OnItemNameChanged));

    public ItemName ItemName
    {
        get => (ItemName)GetValue(ItemNameProperty);
        set => SetValue(ItemNameProperty, value);
    }

    public static readonly DependencyProperty HoverTextProperty = DependencyProperty.Register(
        nameof(HoverText),
        typeof(string),
        typeof(UiItem)
    );

    public string HoverText
    {
        get => (string)GetValue(HoverTextProperty);
        set => SetValue(HoverTextProperty, value);
    }

    private bool Pressed { get; set; } = false;

    internal IItemTrackingService ItemTrackingService { get; private init; }
    internal IParsedSpoilerDataService ParsedSpoilerDataService { get; private init; }
    internal IThemeService ThemeService { get; private init; }

    public UiItem()
    {
        ItemTrackingService = ServiceLocator.GetService<IItemTrackingService>() ?? throw new InvalidOperationException("IItemTrackingService not found in service locator.");
        ParsedSpoilerDataService = ServiceLocator.GetService<IParsedSpoilerDataService>() ?? throw new InvalidOperationException("IParsedSpoilerDataService not found in service locator.");
        ThemeService = ServiceLocator.GetService<IThemeService>() ?? throw new InvalidOperationException("IThemeService not found in service locator.");
        InitializeComponent();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static void OnItemNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UiItem control && e.NewValue is ItemName itemName)
        {
            control.DataContext = new UiItemViewModel(itemName, control.ItemTrackingService, control.ParsedSpoilerDataService, control.ThemeService);
        }
    }

    private void ToggleStar()
    {
        if (DataContext is UiItemViewModel viewModel)
        {
            viewModel.ToggleStar();
        }
    }

    private void CheckMiddleClick(object sender, MouseEventArgs e)
    {
        if (e.MiddleButton == MouseButtonState.Pressed)
        {
            ToggleStar();
        }
    }

    private void UiItem_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // TODO: only want to prevent certain actions when triggered. Mouse down by itself isn't bad.
        /*
        var inSpoilerLog = ParsedSpoilerDataService.CurrentData?.StartingItems.ContainsKey(ItemName) ?? false;
        if (inSpoilerLog)
        {
            // Ignore clicks on starting items
            return;
        }
        var inStart = ItemTrackingService.GetItemState(ItemName);
        if (inStart != null && (inStart.Region == RegionName.START || inStart.Region == RegionName.UNHINTABLE_MOVES))
        {
            // Ignore clicks on starting items
            return;
        }
        */
        CheckMiddleClick(sender, e);
        Pressed = (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed);
        var shiftClicked = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        // a shift click will stop dragging and dropping.
        if (shiftClicked)
        {
            Pressed = false;
        }
        if (e.LeftButton != MouseButtonState.Pressed && shiftClicked)
        {
            ToggleStar();
        }
    }

    private void UiItem_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta != 0)
        {
            ToggleStar();
        }
    }
}
