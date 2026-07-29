using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

using TrackOMatic.Logic.Enums;
using TrackOMatic.Services;

namespace TrackOMatic
{
    /// <summary>
    /// Interaction logic for HintItemSelectionDialog.xaml
    /// </summary>
    public partial class ProgHintDialog : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string selectedItemType = "";
        private string hintCap = "";
        private readonly Dictionary<ItemType, string> ItemTypeToPlural = new()
        {
            {ItemType.GOLDEN_BANANA, "Golden Bananas" },
            {ItemType.TOTAL_BLUEPRINTS, "Blueprints" },
            {ItemType.KEY, "Keys" },
            {ItemType.BANANA_MEDAL, "Medals" },
            {ItemType.BATTLE_CROWN, "Crowns" },
            {ItemType.FAIRY,"Fairies" },
            {ItemType.RAINBOW_COIN,"Rainbow Coins" },
            {ItemType.PEARL,"Pearls" },
            {ItemType.COLORED_BANANA, "Colored Bananas" }
        };
        private readonly Dictionary<string, ItemType> PluralToItemType = new()
        {
            {"Golden Bananas", ItemType.GOLDEN_BANANA },
            {"Blueprints", ItemType.TOTAL_BLUEPRINTS },
            {"Keys" , ItemType.KEY },
            {"Medals",ItemType.BANANA_MEDAL },
            {"Crowns",ItemType.BATTLE_CROWN },
            {"Fairies",ItemType.FAIRY },
            {"Rainbow Coins",ItemType.RAINBOW_COIN },
            {"Pearls", ItemType.PEARL },
            {"Colored Bananas",ItemType.COLORED_BANANA }
        };

        public IUserSettingsService UserSettings { get; init; }
        public IApplicationStateService AppState { get; init; }

        /// <summary>
        /// Proxy property for XAML binding to TopMost setting.
        /// </summary>
        public bool TopMostSetting
        {
            get => UserSettings.TopMost;
            set
            {
                if (UserSettings.TopMost != value)
                {
                    UserSettings.TopMost = value;
                    OnPropertyChanged(nameof(TopMostSetting));
                }
            }
        }

        public ProgHintDialog()
        {
            InitializeComponent();

            // Get the injected services via ServiceLocator
            UserSettings = ServiceLocator.GetService<IUserSettingsService>();
            AppState = ServiceLocator.GetService<IApplicationStateService>();

            // Set TopMost from user settings
            Topmost = UserSettings.TopMost;

            // Subscribe to user settings changes to keep TopMost in sync
            UserSettings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IUserSettingsService.TopMost))
                {
                    Topmost = UserSettings.TopMost;
                }
            };

            // Initialize from application state
            var itemType = AppState.ProgressiveHintItem;

            SelectedItemType = ItemTypeToPlural[itemType];
            var itemTypes = new ItemType[] { ItemType.GOLDEN_BANANA, ItemType.TOTAL_BLUEPRINTS, ItemType.KEY, ItemType.BANANA_MEDAL, ItemType.BATTLE_CROWN, ItemType.FAIRY, ItemType.RAINBOW_COIN, ItemType.PEARL, ItemType.COLORED_BANANA };
            var stringTypes = new List<string>();
            foreach (var item in itemTypes)
            {
                stringTypes.Add(ItemTypeToPlural[item]);
            }
            itemDropdown.ItemsSource = stringTypes;
            HintCap = AppState.ProgressiveHintCap.ToString();
        }

        public ItemType GetActualItemType()
        {
            var actualItem = PluralToItemType[SelectedItemType];
            return actualItem;
        }

        public string SelectedItemType
        {
            get => selectedItemType;
            set
            {
                if (selectedItemType != value)
                {
                    selectedItemType = value;
                    OnPropertyChanged(nameof(SelectedItemType));
                    var actualItem = PluralToItemType[SelectedItemType];
                    AppState.ProgressiveHintItem = actualItem;
                }
            }
        }

        public string HintCap
        {
            get => hintCap;
            set
            {
                if (hintCap != value)
                {
                    hintCap = value;
                    OnPropertyChanged(nameof(hintCap));
                    if (int.TryParse(hintCap, out int hintCapInt))
                    {
                        AppState.ProgressiveHintCap = hintCapInt;
                    }
                }
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void HintCap_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            HintHelper.GenerateThresholds(AppState.ProgressiveHintCap);
        }
    }
}
