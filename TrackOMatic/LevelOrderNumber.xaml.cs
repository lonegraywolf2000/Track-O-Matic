using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using TrackOMatic.Logic.Enums;
using TrackOMatic.Services;

namespace TrackOMatic
{
    public partial class LevelOrderNumber : UserControl
    {

        private int currentNumber = 0;
        public RegionName RegionName { get; private set; }
        public IUserSettingsService UserSettings { get; init; }
        public IParsedSpoilerDataService ParsedSpoilerDataService { get; init; }

        public LevelOrderNumber()
        {
            UserSettings = ServiceLocator.GetService<IUserSettingsService>();
            ParsedSpoilerDataService = ServiceLocator.GetService<IParsedSpoilerDataService>();
            InitializeComponent();
            currentNumber = 0;
        }

        public void UpdateLabel()
        {
            NumberLabel.Text = (currentNumber != 0) ? currentNumber.ToString() : "?";
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            if (mainWindow.BroadcastView != null && currentNumber != -1)
            {
                mainWindow.BroadcastView.UpdateLevelNumber(RegionName, currentNumber);
            }
        }

        public void SetRegion(RegionName newRegion)
        {
            RegionName = newRegion;
        }

        public void Reset()
        {
            currentNumber = 0;
            UpdateLabel();
        }

        public void SetNumber(int newNumber)
        {
            currentNumber = newNumber;
            UpdateLabel();
        }

        public int GetNumber()
        {
            return currentNumber;
        }

        private void LevelOrder_LeftPress(object sender, RoutedEventArgs e)
        {
            if (ParsedSpoilerDataService.CurrentData?.HasLevelOrder ?? false)
            {
                return;
            }

            if (RegionName == RegionName.HIDEOUT_HELM && currentNumber == 8 && !UserSettings.HelmInLevelOrder)
            {
                return;
            }

            int max = (UserSettings.HelmInLevelOrder) ? 8 : 7;
            currentNumber = (currentNumber + 1) % (max + 1);
            UpdateLabel();
        }

        private void LevelOrder_RightPress(object sender, RoutedEventArgs e)
        {
            if (ParsedSpoilerDataService.CurrentData?.HasLevelOrder ?? false)
            {
                return;
            }

            if (RegionName == RegionName.HIDEOUT_HELM && currentNumber == 8 && !UserSettings.HelmInLevelOrder)
            {
                return;
            }

            int max = (UserSettings.HelmInLevelOrder) ? 8 : 7;
            currentNumber = (currentNumber + max) % (max + 1);
            UpdateLabel();
        }

        private void LevelOrder_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ParsedSpoilerDataService.CurrentData?.HasLevelOrder ?? false)
            {
                return;
            }

            if (e.Delta > 0)
            {
                LevelOrder_LeftPress(sender, e);
            }
            else if (e.Delta < 0)
            {
                LevelOrder_RightPress(sender, e);
            }

            e.Handled = true;
        }
    }
}
