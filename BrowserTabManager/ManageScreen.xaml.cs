using System.ComponentModel;
using System.Windows;

namespace BrowserTabManager
{
    public partial class ManageScreen : Window
    {
        private CustomScreen _customScreen;
        private MainWindow _mainWindow;

        public ManageScreen(CustomScreen customScreen, MainWindow mainWindow)
        {
            InitializeComponent();
            _customScreen = customScreen;
            _mainWindow = mainWindow;
            this.DataContext = _customScreen;

            int screenIndex = _mainWindow.ScreenList.IndexOf(_customScreen) + 1;
            ScreenTitleLabel.Content = $"Screen {screenIndex}";

            foreach (var tab in _customScreen.TabList)
            {
                tab.PropertyChanged += Tab_PropertyChanged;
            }

            this.Closed += (s, e) =>
            {
                foreach (var tab in _customScreen.TabList)
                {
                    tab.PropertyChanged -= Tab_PropertyChanged;
                }
            };
        }

        private void Tab_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CustomTab.displayFrame))
            {
                _mainWindow.frameHelper.OrganizeFrames();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is CustomTab customTab)
            {
                _mainWindow.tabHelper.CloseTab(customTab);
                _customScreen.TabList.Remove(customTab);
            }
        }
    }
}
