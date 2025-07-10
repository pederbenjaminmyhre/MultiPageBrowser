using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Web.WebView2.Wpf;

namespace BrowserTabManager
{
    public class CustomTab
    {
        private bool _displayFrame = true;
        public bool displayFrame
        {
            get => _displayFrame;
            set
            {
                _displayFrame = value;
                if (Tab_TitleLabel != null)
                {
                    if (value)
                    {
                        Tab_TitleLabel.BorderBrush = Brushes.Gray;
                        Tab_TitleLabel.BorderThickness = new System.Windows.Thickness(1, 0, 0, 0);
                        Tab_TitleLabel.Background = Brushes.White;
                        TabTitleLabelBackground = Brushes.White;
                    }
                    else
                    {
                        Tab_TitleLabel.BorderBrush = Brushes.Gray;
                        Tab_TitleLabel.BorderThickness = new System.Windows.Thickness(1, 0, 0, 0);
                        Tab_TitleLabel.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E2E6EC"));
                        TabTitleLabelBackground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E2E6EC"));
                    }
                    // Set font weight and color on the TextBlock inside the Label
                    var darkGray = new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 40)); // almost black
                    if (Tab_TitleLabel.Content is System.Windows.Controls.TextBlock tb)
                    {
                        tb.FontWeight = value ? System.Windows.FontWeights.SemiBold : System.Windows.FontWeights.Normal;
                        tb.Foreground = value ? Brushes.Black : darkGray;
                    }
                    else
                    {
                        Tab_TitleLabel.FontWeight = value ? System.Windows.FontWeights.SemiBold : System.Windows.FontWeights.Normal;
                        Tab_TitleLabel.Foreground = value ? Brushes.Black : darkGray;
                    }
                }
            }
        }
        public Border Tab_Border { get; set; }
        public Grid Tab_Grid { get; set; }
        public Label Tab_TitleLabel { get; set; }
        public Border Frame_Border { get; set; }
        public Grid Frame_Grid { get; set; }
        public Grid FrameTitle_Grid { get; set; }
        public Grid FrameAddress_Grid { get; set; }
        public TextBox Frame_TitleTextBox { get; set; }
        public TextBox Frame_UrlTextBox { get; set; }
        public Button Frame_CloseButton { get; set; }
        public Button Frame_RefreshButton { get; set; }
        public Button Frame_CloneButton { get; set; }
        public Button Frame_HideButton { get; set; }
        public Button Frame_BackButton { get; set; }
        public Button Frame_ToggleBookmarkButton { get; set; }
        public WebView2 Frame_WebView { get; set; }
        public int frameUrlTextBoxClickState { get; set; } = 0; // 0: cursor, 1: word, 2: all
        public Brush TabTitleLabelBackground { get; set; }
    }
}
