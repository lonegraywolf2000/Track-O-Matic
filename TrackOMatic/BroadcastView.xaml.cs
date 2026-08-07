using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using TrackOMatic.Logic.Enums;
using TrackOMatic.Logic.Models;
using TrackOMatic.Services;

namespace TrackOMatic
{
    /// <summary>
    /// Interaction logic for HintItemSelectionDialog.xaml
    /// </summary>
    public partial class BroadcastView : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Dictionary<ItemName, bool> SelectedItems { get; private set; } = new();
        private Dictionary<ItemName, ItemBackground> ItemMap = new();

        private List<ProgressiveItem> KRoolKongs;
        private List<ProgressiveItem> HelmKongs;

        public Dictionary<ItemType, CollectibleItem> Collectibles { get; private set; }

        //need to keep count of which of these you currently have because their item display is different
        private Dictionary<ItemName, bool> SharedMoves = new()
        {
            {ItemName.PROGRESSIVE_SLAM_1, false },
            {ItemName.PROGRESSIVE_SLAM_2, false },
            {ItemName.PROGRESSIVE_SLAM_3, false },

            {ItemName.SNIPER_SCOPE, false },
            {ItemName.HOMING_AMMO, false },

            {ItemName.SHOCKWAVE, false },
            {ItemName.FAIRY_CAMERA, false },
        };

        private Dictionary<ItemName, bool> StarredSharedMoves = new()
        {
            {ItemName.PROGRESSIVE_SLAM_1, false },
            {ItemName.PROGRESSIVE_SLAM_2, false },
            {ItemName.PROGRESSIVE_SLAM_3, false },

            {ItemName.SNIPER_SCOPE, false },
            {ItemName.HOMING_AMMO, false },

            {ItemName.SHOCKWAVE, false },
            {ItemName.FAIRY_CAMERA, false },
        };

        private Dictionary<RegionName, int> LevelNumbers = new()
        {
            {RegionName.JUNGLE_JAPES, -1 },
            {RegionName.ANGRY_AZTEC , -1 },
            {RegionName.FRANTIC_FACTORY, -1 },
            {RegionName.GLOOMY_GALLEON , -1 },
            {RegionName.FUNGI_FOREST, -1 },
            {RegionName.CRYSTAL_CAVES , -1 },
            {RegionName.CREEPY_CASTLE, -1 },
            {RegionName.HIDEOUT_HELM, -1 }
        };

        private List<string> homingScopeImages =
        [
            "homing_scope_bw", "homingonly", "scopeonly", "homing_scope"
        ];

        private List<string> camShockwaveImages =
        [
            "camera_shockwave_bw", "fairycamonly", "shockwaveonly", "camera_shockwave"
        ];

        private List<string> slamImages =
        [
            "progressive_slam_1_bc_bw", "progressive_slam_1_bc", "progressive_slam_2_bc", "progressive_slam_3_bc"
        ];

        #region Proxy Properties for User Settings

        public IUserSettingsService UserSettings { get; init; }
        public IParsedSpoilerDataService ParsedSpoilerDataService { get; init; }

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

        /// <summary>
        /// Proxy property for XAML binding to Helm & K. Rool broadcast setting.
        /// </summary>
        public bool BroadcastHelmKRoolSetting
        {
            get => UserSettings.BroadcastHelmKRool;
            set
            {
                if (UserSettings.BroadcastHelmKRool != value)
                {
                    UserSettings.BroadcastHelmKRool = value;
                    OnPropertyChanged(nameof(BroadcastHelmKRoolSetting));
                }
            }
        }

        /// <summary>
        /// Proxy property for XAML binding to shopkeepers broadcast setting.
        /// </summary>
        public bool BroadcastShopkeepersSetting
        {
            get => UserSettings.BroadcastShopkeepers;
            set
            {
                if (UserSettings.BroadcastShopkeepers != value)
                {
                    UserSettings.BroadcastShopkeepers = value;
                    OnPropertyChanged(nameof(BroadcastShopkeepersSetting));
                }
            }
        }

        /// <summary>
        /// Proxy property for XAML binding to song display broadcast setting.
        /// </summary>
        public bool BroadcastSongDisplaySetting
        {
            get => UserSettings.BroadcastSongDisplay;
            set
            {
                if (UserSettings.BroadcastSongDisplay != value)
                {
                    UserSettings.BroadcastSongDisplay = value;
                    OnPropertyChanged(nameof(BroadcastSongDisplaySetting));
                }
            }
        }

        /// <summary>
        /// Proxy property for XAML binding to the broadcast number label setting.
        /// </summary>
        public BroadcastNumberLabel BroadcastNumberLabelSetting
        {
            get => UserSettings.BroadcastNumberLabel;
            set
            {
                if (UserSettings.BroadcastNumberLabel != value)
                {
                    UserSettings.BroadcastNumberLabel = value;
                    OnPropertyChanged(nameof(BroadcastNumberLabelSetting));
                }
            }
        }

        #endregion

        private void InitializeMap()
        {
            var itemGrids = new List<UIElementCollection>()
            {
                MainKongMoves.Children, TrainingMovesGrid.Children, CollectiblesGrid.Children, ShopkeepersGrid.Children
            };
            foreach (var itemGrid in itemGrids)
            {
                foreach (var control in itemGrid)
                {
                    if (control is ItemBackground item)
                    {
                        ItemName itemName = (ItemName)item.Tag;
                        ItemMap[itemName] = item;
                    }
                }
            }
        }

        public void InitializeFromItems(Dictionary<ItemName, Item> items)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            foreach (var entry in items)
            {
                var itemName = entry.Key;
                var item = entry.Value;
                if (itemName == ItemName.KEY_6)
                {
                    Console.WriteLine("???");
                }
                if (GetMatchingItem(itemName) != null || SharedMoves.ContainsKey(itemName))
                {
                    if (mainWindow.ITEM_NAME_TO_ITEM.ContainsKey(itemName))
                    {
                        mainWindow.ITEM_NAME_TO_ITEM[itemName].InitHoverPoints();
                    }
                    SetItemStar(itemName, item.Star.Visibility);
                    if (item.Brightened && item.Image.Opacity > 0.9)
                    {
                        TurnItemOn(itemName);
                    }
                }
            }
        }
        public BroadcastView(IUserSettingsService userSettings, IParsedSpoilerDataService parsedSpoilerDataService)
        {
            UserSettings = userSettings;
            ParsedSpoilerDataService = parsedSpoilerDataService;
            UserSettings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IUserSettingsService.TopMost))
                {
                    OnPropertyChanged(nameof(TopMostSetting));
                }
                else if (e.PropertyName == nameof(IUserSettingsService.BroadcastHelmKRool))
                {
                    OnPropertyChanged(nameof(BroadcastHelmKRoolSetting));
                }
                else if (e.PropertyName == nameof(IUserSettingsService.BroadcastShopkeepers))
                {
                    OnPropertyChanged(nameof(BroadcastShopkeepersSetting));
                }
                else if (e.PropertyName == nameof(IUserSettingsService.BroadcastSongDisplay))
                {
                    OnPropertyChanged(nameof(BroadcastSongDisplaySetting));
                }
                else if (e.PropertyName == nameof(IUserSettingsService.BroadcastNumberLabel))
                {
                    OnPropertyChanged(nameof(BroadcastNumberLabelSetting));
                }
            };

            InitializeComponent();
            InitializeMap();
            Collectibles = new() {
                { ItemType.DONKEY_BLUEPRINT, DonkeyBPs},
                { ItemType.DIDDY_BLUEPRINT, DiddyBPs},
                { ItemType.LANKY_BLUEPRINT, LankyBPs },
                { ItemType.TINY_BLUEPRINT, TinyBPs },
                { ItemType.CHUNKY_BLUEPRINT, ChunkyBPs },

                {ItemType.PEARL, pearls },
                {ItemType.BATTLE_CROWN, battle_crowns },
                {ItemType.BANANA_MEDAL, banana_medals },
                {ItemType.RAINBOW_COIN, rainbow_coins },
                {ItemType.FAIRY, banana_fairies },
                {ItemType.GOLDEN_BANANA, golden_bananas },
            };

            var mainWindow = (MainWindow)Application.Current.MainWindow;
            KRoolKongs = new() { KRoolKong1, KRoolKong2, KRoolKong3, KRoolKong4, KRoolKong5 };
            HelmKongs = new() { HelmKong1, HelmKong2, HelmKong3, HelmKong4, HelmKong5 };
            for (int i = 0; i < KRoolKongs.Count; ++i)
            {
                var item = KRoolKongs[i];
                item.Enabled = false;
                UpdateKRoolKong(i, mainWindow.BossKongs[i].image.Source);
            }
            for (int i = 0; i < HelmKongs.Count; ++i)
            {
                var item = HelmKongs[i];
                item.Enabled = false;
                UpdateHelmKong(i, mainWindow.HelmKongs[i].image.Source);
            }
            UpdateShopkeeperHeight();
            AdjustWindowSize();
        }

        public void UpdateKRoolKong(int index, ImageSource newSource)
        {
            KRoolKongs[index].image.Source = newSource;
        }

        public void UpdateHelmKong(int index, ImageSource newSource)
        {
            HelmKongs[index].image.Source = newSource;
        }

        public void UpdateCollectible(ItemType itemType, int newAmount)
        {
            if (!Collectibles.ContainsKey(itemType))
            {
                return;
            }

            Collectibles[itemType].SetAmount(newAmount);
        }

        public void Reset()
        {
            foreach (var entry in Collectibles)
            {
                entry.Value.SetAmount(0);
            }
            foreach (var key in SharedMoves.Keys.ToList())
            {
                SharedMoves[key] = false;
            }
            foreach (var key in StarredSharedMoves.Keys.ToList())
            {
                StarredSharedMoves[key] = false;
            }
            foreach (var key in LevelNumbers.Keys.ToList())
            {
                LevelNumbers[key] = -1;
            }
            MovesWidth.Width = new GridLength(345, GridUnitType.Pixel);
        }

        public void AdjustLayout()
        {
            var pointsEnabled = ParsedSpoilerDataService.HasItemPoints();
            var hoardEnabled = ParsedSpoilerDataService.HasHoardPoints();
            var bothEnabled = pointsEnabled && hoardEnabled;
            var displayOption = UserSettings.BroadcastNumberLabel;

            var pointsCanDisplay = pointsEnabled && (!bothEnabled || displayOption == BroadcastNumberLabel.Points);
            var hoardCanDisplay = hoardEnabled && (!bothEnabled || displayOption == BroadcastNumberLabel.WothCount);

            MovesWidth.Width = new GridLength(pointsEnabled ? 315 : 345, GridUnitType.Pixel);
        }

        public void ProcessSpoilerSettings(SpoilerSettings settings)
        {
            AdjustLayout();
        }

        public void AdjustWindowSize()
        {
            var baseHeight = 394;
            if (ShopkeepersRow.Height.Value > 0)
            {
                baseHeight += 47;
            }
            if (song_display.Height.Value > 0)
            {
                baseHeight += 50;
            }
            if (HelmKRool.Height.Value > 0)
            {
                baseHeight += 47;
            }
            Height = baseHeight;
        }

        public void UpdateSongInfo(string songGame, string songName)
        {
            SongName.Text = songName;
            SongGame.Text = songGame;
        }

        public void UpdateShopkeeperHeight()
        {
            bool on = UserSettings.BroadcastShopkeepers;
            var shopkeeperHeight = on ? 1.0 : 0;
            var mainItemsHeight = on ? 336 : 290;
            ShopkeepersRow.Height = new GridLength(shopkeeperHeight, GridUnitType.Star);
            MainItemsRow.Height = new GridLength(mainItemsHeight, GridUnitType.Pixel);
            AdjustWindowSize();
        }

        public void SetItemStar(ItemName item, Visibility visibility)
        {
            if (StarredSharedMoves.ContainsKey(item))
            {
                StarredSharedMoves[item] = (visibility == Visibility.Visible);
                return;
            }
            var match = GetMatchingItem(item);
            if (match != null)
            {
                match.SetStarVisibility(visibility);
            }
        }

        private ItemBackground? GetMatchingItem(ItemName item)
        {
            var name = item.ToString();
            if (name.StartsWith("KEY"))
            {
                return (ItemBackground)FindName(item.ToString().ToLower());
            }

            if (ItemMap.ContainsKey(item))
            {
                return ItemMap[item];
            }

            return null;
        }

        public void TurnItemOn(ItemName item)
        {
            if (SharedMoves.ContainsKey(item))
            {
                SharedMoves[item] = true;
                return;
            }
            var match = GetMatchingItem(item);
            if (match != null)
            {
                match.SetResourceReference(ItemBackground.BackgroundItemImageProperty, ((ItemName)match.Tag).ToString().ToLower());
            }
        }

        public void TurnItemOff(ItemName item)
        {
            if (SharedMoves.ContainsKey(item))
            {
                SharedMoves[item] = false;
                return;
            }
            var match = GetMatchingItem(item);
            if (match != null)
            {
                match.SetResourceReference(ItemBackground.BackgroundItemImageProperty, ((ItemName)match.Tag).ToString().ToLower() + "_bw");
            }
        }

        public void ActivateTooltip(ItemName item, string hoverText)
        {
            var match = GetMatchingItem(item);
            if (match == null)
            {
                return;
            }

            match.ToolTip.Content = hoverText;
            match.ToolTip.Visibility = Visibility.Visible;
        }

        public void DisableTooltip(ItemName item)
        {
            var match = GetMatchingItem(item);
            if (match == null)
            {
                return;
            }

            match.ToolTip.Visibility = Visibility.Collapsed;
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }
    }
}
