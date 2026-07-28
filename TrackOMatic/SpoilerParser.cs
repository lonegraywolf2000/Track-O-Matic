using System.Windows;

using TrackOMatic.Logic.Enums;
using TrackOMatic.Logic.Models;
using TrackOMatic.Logic.Models.Spoilers;
using TrackOMatic.Services;

namespace TrackOMatic
{
    public class SpoilerParser
    {
        public MainWindow MainWindow { get; }
        public Dictionary<ItemName, RegionName> StartingItems { get; private set; } = [];

        private IUserSettingsService UserSettings { get; init; }
        private ISpoilerService SpoilerService { get; set; }

        public SpoilerParser(MainWindow mainWindow, IUserSettingsService userSettings, ISpoilerService spoilerService)
        {
            MainWindow = mainWindow;
            UserSettings = userSettings;
            SpoilerService = spoilerService;
        }

        private void ReadStartingItemsIntoUI()
        {
            var childElements = new List<Item>(MainWindow.Items.Children.Count);
            foreach (UIElement child in MainWindow.Items.Children)
            {
                if (child is Item item)
                {
                    childElements.Add(item);
                }
            }
            var sortedElements = childElements.OrderBy(child => (ItemName)child.Tag).ToList();
            foreach (var item in sortedElements)
            {
                if (StartingItems.ContainsKey((ItemName)item.Tag))
                {
                    var itemName = (ItemName)item.Tag;
                    var region = StartingItems[itemName];
                    MainWindow.Regions[region].RegionGrid.Add_Item(item);
                    item.SetResourceReference(Item.ItemImageProperty, itemName.ToString().ToLower());
                }
            }
        }

        public async Task<SpoilerSettings> ParseSpoilerAsync(string fileName)
        {
            var spoilerSettings = new SpoilerSettings();
            StartingItems = [];

            try
            {
                // Use the new service to deserialize and parse
                var result = await SpoilerService.DeserializeAndParseAsync(fileName);
                if (!result.IsSuccess || result.Data == null)
                {
                    MainWindow.InitRegionsFromEmptySpoiler();
                    return spoilerSettings;
                }

                var parsedData = result.Data;

                // Extract settings from parsed data
                spoilerSettings = parsedData.SpoilerSettings;

                UpdateWindowsWithSpoilerData(parsedData);

                // Populate starting items
                if (parsedData.StartingItems != null && parsedData.StartingItems.Count > 0)
                {
                    StartingItems = parsedData.StartingItems;
                    ReadStartingItemsIntoUI();
                }

                // Set spoiler settings for START region
                MainWindow.Regions[RegionName.START].SpoilerSettings = spoilerSettings;

                foreach (var entry in ImportantCheckList.ITEMS)
                {
                    entry.Value.InitPointValue();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing spoiler file: {ex.Message}");
                MainWindow.InitRegionsFromEmptySpoiler();
            }

            return spoilerSettings;
        }

        /// <summary>
        /// Updates the MainWindow and its regions with the parsed spoiler data.
        /// </summary>
        /// <remarks>
        /// The <see cref="ParsedSpoilerData"/> will eventually be made available in its own standalone service
        /// for XAML classes to consume more locally.
        /// </remarks>
        /// <param name="spoilerData">The parsed spoiler data.</param>
        private void UpdateWindowsWithSpoilerData(ParsedSpoilerData spoilerData)
        {
            // Extract settings from parsed data
            var spoilerSettings = spoilerData.SpoilerSettings;

            // Populate regions with parsed spoiler data
            if (spoilerData.RegionData != null && spoilerData.RegionData.Count > 0)
            {
                foreach (var regionEntry in spoilerData.RegionData)
                {
                    if (MainWindow.Regions.TryGetValue(regionEntry.Key, out var region))
                    {
                        region.AddPoints(regionEntry.Value.Points);
                        region.AddRequiredCheckTotal(regionEntry.Value.WothCount);
                        region.SpoilerSettings = spoilerSettings;

                        // Process vials for this region
                        if (regionEntry.Value.VialColors != null && regionEntry.Value.VialColors.Count > 0)
                        {
                            foreach (var vial in regionEntry.Value.VialColors)
                            {
                                region.RegionGrid.AddInitialVial(vial);
                            }
                        }
                    }
                }
            }

            // Populate level order
            foreach (var levelEntry in spoilerData.LevelOrder)
            {
                if (MainWindow.Regions.TryGetValue(levelEntry.Key, out var region))
                {
                    region.SetLevelOrderNumber(levelEntry.Value);
                }
            }

            // Populate Helm Kong order
            if (spoilerData.HelmOrder != null && spoilerData.HelmOrder.Count > 0)
            {
                if (UserSettings.ShowHelmOrder)
                {
                    for (int i = 0; i < MainWindow.HelmKongs.Count; i++)
                    {
                        if (i < spoilerData.HelmOrder.Count)
                        {
                            MainWindow.HelmKongs[i].SetIndex(spoilerData.HelmOrder[i]);
                            MainWindow.HelmKongs[i].Enabled = false;
                            MainWindow.HelmKongs[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            MainWindow.HelmKongs[i].Visibility = Visibility.Hidden;
                        }
                    }
                }
            }

            // Populate Final Boss Kong order (KRool order)
            if (spoilerData.FinalBossOrder != null && spoilerData.FinalBossOrder.Count > 0)
            {
                if (UserSettings.ShowKRoolOrder)
                {
                    for (int i = 0; i < MainWindow.BossKongs.Count; i++)
                    {
                        if (i < spoilerData.FinalBossOrder.Count)
                        {
                            MainWindow.BossKongs[i].SetIndex(spoilerData.FinalBossOrder[i]);
                            MainWindow.BossKongs[i].Enabled = false;
                            MainWindow.BossKongs[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            MainWindow.BossKongs[i].Visibility = Visibility.Hidden;
                        }
                    }
                }
            }

            // B Locker data is loaded here. Using the old records/methods for now.
            MainWindow.BLockerHints.LoadBLockerInfo([.. spoilerData.RegionBarrierInfo.Select((b) => new BLockerInfo(b.Item, b.Cost))]);
        }
    }
}
