using Newtonsoft.Json;

using System.IO;

using TrackOMatic.Logic.Enums;
using TrackOMatic.Logic.Models;
using TrackOMatic.Services;
using TrackOMatic.Services.TrackerState;

namespace TrackOMatic
{
    public class DataSaver
    {
        private SavedProgress CurrentSavedProgress { get; set; }
        public MainWindow MainWindow { get; }

        private readonly IDataPersistenceService _dataPersistenceService;

        public bool WriteToFile { get; set; }

        public DataSaver(MainWindow mainWindow, IDataPersistenceService dataPersistenceService)
        {
            MainWindow = mainWindow;
            _dataPersistenceService = dataPersistenceService;
            CurrentSavedProgress = new SavedProgress();
        }

        public void Reset()
        {
            CurrentSavedProgress = new SavedProgress();

            // Sync fresh SavedProgress with provider
            var provider = ServiceLocator.GetService<ISavedProgressProvider>();
            if (provider != null && CurrentSavedProgress != null)
            {
                provider.UpdateProgress(CurrentSavedProgress);
            }
        }

        private void FindSavedHints()
        {
            CurrentSavedProgress.SavedHints.Clear();
            foreach (var hintPanel in MainWindow.HintPanels)
            {
                foreach (var hint in hintPanel.GetSavedHints())
                {
                    CurrentSavedProgress.SavedHints.Add(hint);
                }
            }
        }

        public void Save(string filePath = "autosave.json", bool writeToFile = true)
        {
            if (CurrentSavedProgress == null)
            {
                return;
            }

            FindSavedHints();
            CurrentSavedProgress.SavedGBCounts = MainWindow.BLockerHints.GetGBCounts();
            CurrentSavedProgress.BLockerImageIndexes = MainWindow.BLockerHints.GetImageIndexes();
            CurrentSavedProgress.HelmDoorImageIndexes = MainWindow.HelmDoorHints.GetImageIndexes();
            CurrentSavedProgress.HelmDoorCounts = MainWindow.HelmDoorHints.GetItemCounts();
            CurrentSavedProgress.HelmKongs = MainWindow.GetHelmKongs();
            CurrentSavedProgress.BossKongs = MainWindow.GetBossKongs();
            CurrentSavedProgress.LevelOrder = MainWindow.GetLevelOrder();
            var JSONString = JsonConvert.SerializeObject(CurrentSavedProgress);
            if (writeToFile)
            {
                File.WriteAllText(filePath, JSONString);
            }
        }

        private Item? FindMatchingItem(ItemName toFind)
        {
            foreach (var item in MainWindow.DraggableItems)
            {
                var itemName = (ItemName)item.Tag;
                if (itemName == toFind)
                {
                    return item;
                }
            }
            return null;
        }

        //if the user turns autotracking off we need to remark the items as not autotracked in the saved data
        public void TurnOffAutotrackingField()
        {
            if (CurrentSavedProgress == null)
            {
                return;
            }

            foreach (var savedItemEntry in CurrentSavedProgress.SavedItems)
            {
                savedItemEntry.Value.Autotracked = false;
            }
        }

        private void ReadSavedProgress()
        {
            if (CurrentSavedProgress == null)
            {
                return;
            }

            if (CurrentSavedProgress.spoilerPath != "" && File.Exists(CurrentSavedProgress.spoilerPath))
            {
                MainWindow.ParseSpoiler(CurrentSavedProgress.spoilerPath);
            }
            foreach (var savedItemEntry in CurrentSavedProgress.SavedItems)
            {
                var savedItem = savedItemEntry.Value;
                var region = savedItem.Region;
                bool autoPlace = (savedItem.Autotracked || savedItem.Hinted);
                Item? matchingItem = FindMatchingItem(savedItem.ItemName);
                if (matchingItem == null)
                {
                    continue;
                }
                matchingItem.SetStarVisibility(savedItem.Starred.ToWpfVisibility());
                matchingItem.ChangeOpacity(savedItem.Opacity);
                if (savedItem.Autotracked)
                {
                    MainWindow.Autotracker.ProcessSavedItem(savedItem.ItemName);
                }

                if (savedItem.Region != RegionName.UNKNOWN && !savedItem.Hinted)
                {
                    MainWindow.Regions[region].RegionGrid.Add_Item(matchingItem, !savedItem.Autotracked, !savedItem.Hinted);
                }
                if (savedItem.Hinted)
                {
                    matchingItem.Darken();
                }
                matchingItem.ChangeOpacity(savedItem.Opacity);
            }
            foreach (var savedHint in CurrentSavedProgress.SavedHints.ToList())
            {
                var hintPanel = (HintPanel)MainWindow.FindName(savedHint.HintPanelKey);
                hintPanel.AddSavedHint(savedHint);
            }
            MainWindow.BLockerHints.LoadSavedGBCounts(CurrentSavedProgress.SavedGBCounts);
            MainWindow.BLockerHints.LoadSavedImageIndexes(CurrentSavedProgress.BLockerImageIndexes);
            MainWindow.HelmDoorHints.LoadSavedHelmDoorCounts(CurrentSavedProgress.HelmDoorCounts);
            MainWindow.HelmDoorHints.LoadSavedImageIndexes(CurrentSavedProgress.HelmDoorImageIndexes);
            if (CurrentSavedProgress.HelmKongs != null)
            {
                MainWindow.LoadHelmKongs(CurrentSavedProgress.HelmKongs);
            }

            if (CurrentSavedProgress.BossKongs != null)
            {
                MainWindow.LoadBossKongs(CurrentSavedProgress.BossKongs);
            }

            if (CurrentSavedProgress.LevelOrder != null)
            {
                MainWindow.LoadLevelOrder(CurrentSavedProgress.LevelOrder);
            }
        }

        public void AddSavedItem(SavedItem savedItem)
        {
            if (savedItem == null || CurrentSavedProgress == null)
            {
                return;
            }

            var itemName = savedItem.ItemName;
            if (CurrentSavedProgress.SavedItems.ContainsKey(itemName))
            {
                CurrentSavedProgress.SavedItems[itemName] = savedItem;
            }
            else
            {
                CurrentSavedProgress.SavedItems.Add(itemName, savedItem);
            }
        }

        public void ReadSavedDataFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                var jsonString = File.ReadAllText(filePath);
                SavedProgress? savedData = JsonConvert.DeserializeObject<SavedProgress>(jsonString);
                if (savedData == null)
                {
                    return;
                }
                CurrentSavedProgress = savedData;
                ReadSavedProgress();

                // Sync SavedProgress with provider after loading from file
                var provider = ServiceLocator.GetService<ISavedProgressProvider>();
                if (provider != null && CurrentSavedProgress != null)
                {
                    provider.UpdateProgress(CurrentSavedProgress);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void setSpoilerPath(string newSpoilerPath)
        {
            if (CurrentSavedProgress == null)
            {
                return;
            }

            CurrentSavedProgress.spoilerPath = newSpoilerPath;
        }

        /*

        private void CheckForAutosave()
        {
            var filePath = "autosave.json";
            ReadSavedDataFromFile(filePath);
        }

        public void InitSavedDataFromSpoiler(string fileName)
        {
            savedProgress = new SavedProgress(fileName);
            CheckForAutosave();
        }
        */
    }
}
