using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

using TrackOMatic.Services;

namespace TrackOMatic
{
    public partial class BasicItemSelector : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int SelectedImageIndex { get; private set; }
        private List<Image> images;
        public IUserSettingsService UserSettings { get; init; }

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

        public BasicItemSelector(List<List<BitmapImage>> toAdd)
        {
            SelectedImageIndex = -1;
            images = new();
            InitializeComponent();

            // Get the injected user settings service via ServiceLocator
            UserSettings = ServiceLocator.GetService<IUserSettingsService>();
            UserSettings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IUserSettingsService.TopMost))
                {
                    OnPropertyChanged(nameof(TopMostSetting));
                }
            };
            Topmost = UserSettings.TopMost;

            // Subscribe to settings changes to keep TopMost in sync
            UserSettings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IUserSettingsService.TopMost))
                {
                    Topmost = UserSettings.TopMost;
                }
            };

            for (int i = 0; i < toAdd.Count; ++i)
            {
                var row = toAdd[i];
                for (int j = 0; j < row.Count; ++j)
                {
                    var border = new Border
                    {
                        BorderThickness = new Thickness(0),
                        UseLayoutRounding = true,
                        SnapsToDevicePixels = true,
                    };
                    var shadow = new DropShadowEffect
                    {
                        Color = Colors.Black,
                        Direction = 0,
                        ShadowDepth = 0,
                        BlurRadius = 14,
                        Opacity = 0.5
                    };
                    border.Effect = shadow;
                    RenderOptions.SetBitmapScalingMode(border, BitmapScalingMode.Fant);
                    RenderOptions.SetClearTypeHint(border, ClearTypeHint.Enabled);

                    var imageSource = toAdd[i][j];
                    Image image = new()
                    {
                        Source = imageSource,
                        Margin = new Thickness(1)
                    };
                    image.MouseDown += ImagePressed;
                    border.Child = image;
                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                    images.Add(image);
                    ItemGrid.Children.Add(border);
                }
            }
            Height = (toAdd.Count < 2) ? 80 : 120;
            Row2.Height = (toAdd.Count < 2) ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
        }

        private void ImagePressed(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var image = (Image)sender;
                SelectedImageIndex = images.IndexOf(image);
                Close();
            }
        }
    }
}
