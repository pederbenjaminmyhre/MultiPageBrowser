using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Generic;
using Microsoft.Web.WebView2.Wpf;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Web.WebView2.Core;

namespace BrowserTabManager
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<CustomBookmark> BookmarksList = new List<CustomBookmark>();
        private List<CustomTab> TabsList = new List<CustomTab>();

        //private TextBox txtLaunchUrl => (TextBox)this.FindName("txtLaunchUrl");

        public MainWindow()
        {
            InitializeComponent();
            txtSearchBookmarks.TextChanged += TxtSearchBookmarks_TextChanged;
            txtLaunchUrl.KeyDown += TxtLaunchUrl_KeyDown;
            txtSearchTabs.TextChanged += TxtSearchTabs_TextChanged;
            // Add bookmarks for the provided URLs with cleaned-up names
            CreateBookmark("amazon.com", "Amazon");
            CreateBookmark("bing.com", "Bing");
            CreateBookmark("canva.com", "Canva");
            CreateBookmark("chatgpt.com", "ChatGPT");
            CreateBookmark("craigslist.org", "Craigslist");
            CreateBookmark("discord.com", "Discord");
            CreateBookmark("duckduckgo.com", "DuckDuckGo");
            CreateBookmark("ebay.com", "Ebay");
            CreateBookmark("facebook.com", "Facebook");
            CreateBookmark("fandom.com", "Fandom");
            CreateBookmark("google.com", "Google");
            CreateBookmark("imdb.com", "IMDb");
            CreateBookmark("instagram.com", "Instagram");
            CreateBookmark("linkedin.com", "LinkedIn");
            CreateBookmark("live.com", "Live");
            CreateBookmark("microsoft.com", "Microsoft");
            CreateBookmark("microsoftonline.com", "Microsoft Online");
            CreateBookmark("netflix.com", "Netflix");
            CreateBookmark("nytimes.com", "NY Times");
            CreateBookmark("office.com", "Office");
            CreateBookmark("openai.com", "OpenAI");
            CreateBookmark("pinterest.com", "Pinterest");
            CreateBookmark("reddit.com", "Reddit");
            CreateBookmark("sharepoint.com", "SharePoint");
            CreateBookmark("temu.com", "Temu");
            CreateBookmark("tiktok.com", "TikTok");
            CreateBookmark("tumblr.com", "Tumblr");
            CreateBookmark("twitch.tv", "Twitch");
            CreateBookmark("twitter.com", "Twitter (X.com)");
            CreateBookmark("walmart.com", "Walmart");
            CreateBookmark("weather.com", "Weather");
            CreateBookmark("whatsapp.com", "WhatsApp");
            CreateBookmark("wikipedia.org", "Wikipedia");
            CreateBookmark("yahoo.com", "Yahoo");
            CreateBookmark("youtube.com", "YouTube");
        }

        private void TxtSearchBookmarks_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = txtSearchBookmarks.Text.Trim().ToLower();
            BookmarkStack.Children.Clear();
            var filtered = new List<CustomBookmark>();
            foreach (var bookmark in BookmarksList)
            {
                string url = bookmark.URL?.ToLower() ?? string.Empty;
                string name = bookmark.TitleLabel.Content?.ToString().ToLower() ?? string.Empty;
                if (string.IsNullOrEmpty(search) || url.Contains(search) || name.Contains(search))
                {
                    filtered.Add(bookmark);
                }
            }
            foreach (var bookmark in filtered.OrderBy(b => b.TitleLabel.Content?.ToString(), StringComparer.OrdinalIgnoreCase))
            {
                BookmarkStack.Children.Add(bookmark.Border);
            }
        }

        private async Task<string> FetchPageTitleAsync(string url)
        {
            try
            {
                using var httpClient = new System.Net.Http.HttpClient();
                if (!url.StartsWith("http"))
                    url = "https://" + url;
                var html = await httpClient.GetStringAsync(url);
                var titleTagStart = html.IndexOf("<title", StringComparison.OrdinalIgnoreCase);
                if (titleTagStart >= 0)
                {
                    var titleStart = html.IndexOf('>', titleTagStart);
                    var titleEnd = html.IndexOf("</title>", titleStart, StringComparison.OrdinalIgnoreCase);
                    if (titleStart >= 0 && titleEnd > titleStart)
                    {
                        var title = html.Substring(titleStart + 1, titleEnd - titleStart - 1).Trim();
                        return System.Net.WebUtility.HtmlDecode(title);
                    }
                }
            }
            catch { }
            return null;
        }

        private void CreateBookmark(string urlString, string nameString)
        {
            // Uncollapse the BookmarkScroll to make bookmarks visible
            BookmarkScroll.Visibility = Visibility.Visible;

            // Create CustomBookmark
            var customBookmark = new CustomBookmark();
            BookmarksList.Add(customBookmark);

            // Create Border
            var bookmarkBorder = new Border();
            customBookmark.Border = bookmarkBorder;
            bookmarkBorder.Tag = customBookmark;

            // Create Grid
            var bookmarkGrid = new Grid();
            bookmarkBorder.Child = bookmarkGrid;
            customBookmark.Grid = bookmarkGrid;
            bookmarkGrid.Tag = customBookmark;
            bookmarkGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Create Label instead of TextBox
            var bookmarkNameLabel = new Label();
            Grid.SetColumn(bookmarkNameLabel, 0);
            bookmarkGrid.Children.Add(bookmarkNameLabel);
            customBookmark.TitleLabel = bookmarkNameLabel;
            bookmarkNameLabel.Tag = customBookmark;
            bookmarkNameLabel.Content = nameString;

            // Assign URL
            customBookmark.URL = urlString;

            // Add to stack (for initial creation)
            BookmarkStack.Children.Add(bookmarkBorder);

            // Attach click event to the bookmark label
            bookmarkNameLabel.MouseLeftButtonUp += BookmarkNameLabel_Click;

            // If urlString == nameString, fetch the page title and update label
            if (urlString == nameString)
            {
                _ = UpdateBookmarkTitleFromUrlAsync(customBookmark, urlString);
            }
        }

        private async Task UpdateBookmarkTitleFromUrlAsync(CustomBookmark bookmark, string url)
        {
            var title = await FetchPageTitleAsync(url);
            if (!string.IsNullOrWhiteSpace(title))
            {
                // Update on UI thread
                Dispatcher.Invoke(() => bookmark.TitleLabel.Content = title);
            }
        }

        private void BookmarkNameLabel_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Label label && label.Tag is CustomBookmark customBookmark)
            {
                CreateTab(customBookmark.URL, label.Content?.ToString() ?? string.Empty);
            }
        }

        private void CreateTab(string urlString, string nameString)
        {
            var customTab = new CustomTab();
            TabsList.Add(customTab);
            customTab.displayFrame = true;

            // Tab Border
            var tab_Border = new Border();
            TabStack.Children.Add(tab_Border);
            customTab.Tab_Border = tab_Border;
            tab_Border.Tag = customTab;

            // Tab Grid
            var tab_Grid = new Grid();
            tab_Border.Child = tab_Grid;
            customTab.Tab_Grid = tab_Grid;
            tab_Grid.Tag = customTab;
            tab_Grid.RowDefinitions.Add(new RowDefinition());
            tab_Grid.ColumnDefinitions.Add(new ColumnDefinition());

            // Tab Title Label
            var tab_TitleLabel = new Label();
            tab_TitleLabel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9E5")); // Very light gold
            tab_TitleLabel.Margin = new Thickness(2); // Add 2 pixel margin around the label
            Grid.SetRow(tab_TitleLabel, 0);
            Grid.SetColumn(tab_TitleLabel, 0);
            tab_Grid.Children.Add(tab_TitleLabel);
            customTab.Tab_TitleLabel = tab_TitleLabel;
            tab_TitleLabel.Tag = customTab;
            // Use a TextBlock for wrapping
            var tabTitleTextBlock = new TextBlock { Text = nameString, TextWrapping = TextWrapping.Wrap, MaxWidth = 160, Foreground = Brushes.Black };
            tab_TitleLabel.Content = tabTitleTextBlock;
            tab_TitleLabel.MouseLeftButtonUp += TabTitleLabel_Click;

            // Frame Border
            var frame_Border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(4) // Add margin around the frame
                // No CornerRadius, no Effect (shadow)
            };
            customTab.Frame_Border = frame_Border;
            frame_Border.Tag = customTab;

            // Frame Grid
            var frame_Grid = new Grid();
            frame_Border.Child = frame_Grid;
            customTab.Frame_Grid = frame_Grid;
            frame_Grid.Tag = customTab;
            frame_Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            frame_Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            frame_Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Frame Title Grid
            var frameTitle_Grid = new Grid
            {
                Background = new SolidColorBrush(Color.FromRgb(10, 36, 106)) // dark blue
            };
            Grid.SetRow(frameTitle_Grid, 0);
            frame_Grid.Children.Add(frameTitle_Grid);
            customTab.FrameTitle_Grid = frameTitle_Grid;
            frameTitle_Grid.Tag = customTab;
            frameTitle_Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            frameTitle_Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            frameTitle_Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Frame Address Grid
            var frameAddress_Grid = new Grid();
            Grid.SetRow(frameAddress_Grid, 1);
            frame_Grid.Children.Add(frameAddress_Grid);
            customTab.FrameAddress_Grid = frameAddress_Grid;
            frameAddress_Grid.Tag = customTab;
            frameAddress_Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            frameAddress_Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Frame Title TextBox
            var frame_TitleTextBox = new TextBox
            {
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };
            Grid.SetColumn(frame_TitleTextBox, 0);
            frameTitle_Grid.Children.Add(frame_TitleTextBox);
            customTab.Frame_TitleTextBox = frame_TitleTextBox;
            frame_TitleTextBox.Tag = customTab;
            frame_TitleTextBox.Text = nameString;
            frame_TitleTextBox.TextChanged += FrameTitleTextBox_TextChanged;

            // Frame Url TextBox
            var frame_UrlTextBox = new TextBox();
            Grid.SetColumn(frame_UrlTextBox, 1);
            frameAddress_Grid.Children.Add(frame_UrlTextBox);
            customTab.Frame_UrlTextBox = frame_UrlTextBox;
            frame_UrlTextBox.Tag = customTab;
            frame_UrlTextBox.Text = urlString;
            frame_UrlTextBox.KeyDown += FrameUrlTextBox_KeyDown;

            // Frame Close Button
            var frame_CloseButton = new Button { Content = "X", Height = 15, Width = 15, FontSize = 9, FontWeight = FontWeights.Bold };
            Grid.SetColumn(frame_CloseButton, 2);
            frameTitle_Grid.Children.Add(frame_CloseButton);
            customTab.Frame_CloseButton = frame_CloseButton;
            frame_CloseButton.Tag = customTab;
            frame_CloseButton.Click += (s, e) => CloseTab(customTab);

            // Frame Hide Button
            var frame_HideButton = new Button { Content = "_", Height = 15, Width = 15, FontSize = 9, FontWeight = FontWeights.Bold };
            Grid.SetColumn(frame_HideButton, 1);
            frameTitle_Grid.Children.Add(frame_HideButton);
            customTab.Frame_HideButton = frame_HideButton;
            frame_HideButton.Tag = customTab;
            frame_HideButton.Click += (s, e) => HideTabFrame(customTab);

            // Frame Back Button
            var frame_BackButton = new Button { Content = "<", Height = 15, Width = 15, FontSize = 9, FontWeight = FontWeights.Bold };
            Grid.SetColumn(frame_BackButton, 0);
            frameAddress_Grid.Children.Add(frame_BackButton);
            customTab.Frame_BackButton = frame_BackButton;
            frame_BackButton.Tag = customTab;
            frame_BackButton.Click += FrameBackButton_Click;

            // Frame WebView2
            var frame_WebView = new WebView2();
            Grid.SetRow(frame_WebView, 2);
            frame_Grid.Children.Add(frame_WebView);
            customTab.Frame_WebView = frame_WebView;
            frame_WebView.Tag = customTab;
            frame_WebView.NavigationCompleted += FrameWebView_NavigationCompleted;
            frame_WebView.CoreWebView2InitializationCompleted += FrameWebView_CoreWebView2InitializationCompleted;
            try
            {
                frame_WebView.Source = new System.Uri(urlString.StartsWith("http") ? urlString : $"https://{urlString}");
            }
            catch { }

            // If urlString == nameString, fetch the page title and update tab label and frame title
            if (urlString == nameString)
            {
                _ = UpdateTabTitleFromUrlAsync(customTab, urlString);
            }

            OrganizeFrames();
        }

        private async void FrameWebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (sender is WebView2 webView && webView.Tag is CustomTab customTab)
            {
                if (customTab.Frame_UrlTextBox != null && webView.Source != null)
                {
                    customTab.Frame_UrlTextBox.Text = webView.Source.ToString();
                }
                // Get the page title and set it to Frame_TitleTextBox
                try
                {
                    string script = "document.title";
                    string title = await webView.ExecuteScriptAsync(script);
                    // Remove quotes from the result
                    if (!string.IsNullOrEmpty(title) && title.Length > 1 && title.StartsWith("\"") && title.EndsWith("\""))
                    {
                        title = title.Substring(1, title.Length - 2);
                    }
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        customTab.Frame_TitleTextBox.Text = title;
                    }
                }
                catch { }
            }
        }

        private void FrameWebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (sender is WebView2 webView && webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            }
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            // Prevent the default new window behavior
            e.Handled = true;
            string uri = e.Uri;
            if (!string.IsNullOrEmpty(uri))
            {
                // Use the link's URL as both urlString and nameString for now
                CreateTab(uri, uri);
            }
        }

        private void FrameBackButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CustomTab customTab)
            {
                var webView = customTab.Frame_WebView;
                if (webView != null && webView.CanGoBack)
                {
                    webView.GoBack();
                }
            }
        }

        private void TabTitleLabel_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Label label && label.Tag is CustomTab customTab)
            {
                customTab.displayFrame = !customTab.displayFrame;
                OrganizeFrames();
            }
        }

        private void HideTabFrame(CustomTab customTab)
        {
            customTab.displayFrame = false;
            FramesGrid.Children.Remove(customTab.Frame_Border);
            // Remove columns from right to left until FramesGrid.ColumnDefinitions.Count < needed
            int rowCount = FramesGrid.RowDefinitions.Count > 0 ? FramesGrid.RowDefinitions.Count : 1;
            int needed = (int)(System.Math.Ceiling((double)TabsList.Count / rowCount) * 2) - 1;
            while (FramesGrid.ColumnDefinitions.Count > needed && FramesGrid.ColumnDefinitions.Count > 1)
            {
                FramesGrid.ColumnDefinitions.RemoveAt(FramesGrid.ColumnDefinitions.Count - 1);
            }
            OrganizeFrames();
        }

        private void CloseTab(CustomTab customTab)
        {
            TabsList.Remove(customTab);
            TabStack.Children.Remove(customTab.Tab_Border);
            FramesGrid.Children.Remove(customTab.Frame_Border);
            customTab.Frame_WebView?.Dispose();

            // Remove columns from right to left until FramesGrid.ColumnDefinitions.Count < needed
            int rowCount = FramesGrid.RowDefinitions.Count > 0 ? FramesGrid.RowDefinitions.Count : 1;
            int needed = (int)(System.Math.Ceiling((double)TabsList.Count / rowCount) * 2) - 1;
            while (FramesGrid.ColumnDefinitions.Count > needed && FramesGrid.ColumnDefinitions.Count > 1)
            {
                FramesGrid.ColumnDefinitions.RemoveAt(FramesGrid.ColumnDefinitions.Count - 1);
            }

            OrganizeFrames();
        }

        private async Task UpdateTabTitleFromUrlAsync(CustomTab tab, string url)
        {
            var title = await FetchPageTitleAsync(url);
            if (!string.IsNullOrWhiteSpace(title))
            {
                Dispatcher.Invoke(() => {
                    if (tab.Tab_TitleLabel.Content is TextBlock tb) tb.Text = title;
                    tab.Frame_TitleTextBox.Text = title;
                });
            }
        }

        private void OrganizeFrames()
        {
            // Use only CustomTabs with displayFrame == true
            var frameTabs = TabsList.Where(t => t.displayFrame).ToList();

            // Ensure FramesGrid has enough columns using the new logic
            int rowCount = FramesGrid.RowDefinitions.Count > 0 ? FramesGrid.RowDefinitions.Count : 1;
            int needed = (int)(System.Math.Ceiling((double)frameTabs.Count / rowCount) * 2) - 1;
            while (FramesGrid.ColumnDefinitions.Count < needed)
            {
                int newColIndex = FramesGrid.ColumnDefinitions.Count;
                if (newColIndex % 2 == 0)
                {
                    // Even index: star width, empty
                    var colDef = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
                    FramesGrid.ColumnDefinitions.Add(colDef);
                }
                else
                {
                    // Odd index: width 4, add GridSplitter
                    var colDef = new ColumnDefinition { Width = new GridLength(4) };
                    FramesGrid.ColumnDefinitions.Add(colDef);
                    for (int row = 0; row < FramesGrid.RowDefinitions.Count; row++)
                    {
                        var splitter = new GridSplitter
                        {
                            Width = 4,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            Background = Brushes.Transparent,
                            ResizeDirection = GridResizeDirection.Columns
                        };
                        Grid.SetColumn(splitter, newColIndex);
                        Grid.SetRow(splitter, row);
                        FramesGrid.Children.Add(splitter);
                    }
                }
            }

            // Remove all non-splitter children from FramesGrid (across all rows)
            for (int i = FramesGrid.Children.Count - 1; i >= 0; i--)
            {
                if (!(FramesGrid.Children[i] is GridSplitter))
                    FramesGrid.Children.RemoveAt(i);
            }

            // Make sure FramesGrid is visible and has at least one row
            if (FramesGrid.RowDefinitions.Count == 0)
            {
                FramesGrid.RowDefinitions.Add(new RowDefinition());
            }

            // Place each frame in FramesGrid (across then down, skipping splitter columns)
            int framesPerRow = (FramesGrid.ColumnDefinitions.Count + 1) / 2;
            int frameIndex = 0;
            for (int row = 0; row < FramesGrid.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < FramesGrid.ColumnDefinitions.Count; col += 2)
                {
                    if (frameIndex >= frameTabs.Count)
                        break;
                    var tab = frameTabs[frameIndex];
                    Grid.SetColumn(tab.Frame_Border, col);
                    Grid.SetRow(tab.Frame_Border, row);
                    FramesGrid.Children.Add(tab.Frame_Border);
                    frameIndex++;
                }
            }
        }

        private void TxtLaunchUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string url = txtLaunchUrl.Text.Trim();
                if (!string.IsNullOrEmpty(url))
                {
                    CreateTab(url, url);
                }
                txtLaunchUrl.Text = string.Empty;
            }
        }

        private void btnAddRowToFramesGrid_Click(object sender, RoutedEventArgs e)
        {
            FramesGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            OrganizeFrames();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            foreach (var tab in TabsList)
            {
                tab.Frame_WebView?.Dispose();
            }
            base.OnClosing(e);
        }

        private void TxtSearchTabs_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = txtSearchTabs.Text.Trim().ToLower();
            TabStack.Children.Clear();
            var filtered = new List<CustomTab>();
            foreach (var tab in TabsList)
            {
                string url = tab.Frame_UrlTextBox?.Text?.ToLower() ?? string.Empty;
                string name = tab.Tab_TitleLabel?.Content?.ToString().ToLower() ?? string.Empty;
                if (string.IsNullOrEmpty(search) || url.Contains(search) || name.Contains(search))
                {
                    filtered.Add(tab);
                }
            }
            foreach (var tab in filtered.OrderBy(t => t.Tab_TitleLabel.Content?.ToString(), StringComparer.OrdinalIgnoreCase))
            {
                TabStack.Children.Add(tab.Tab_Border);
            }
        }

        private void FrameUrlTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textBox && textBox.Tag is CustomTab customTab)
            {
                var webView = customTab.Frame_WebView;
                string url = textBox.Text.Trim();
                if (!string.IsNullOrEmpty(url))
                {
                    try
                    {
                        webView.Source = new System.Uri(url.StartsWith("http") ? url : $"https://{url}");
                    }
                    catch { }
                }
            }
        }

        private void FrameTitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is CustomTab customTab && customTab.Tab_TitleLabel != null)
            {
                if (customTab.Tab_TitleLabel.Content is TextBlock tb) tb.Text = textBox.Text;
            }
        }
    }


    public class CustomBookmark
    {
        public Border Border { get; set; }
        public Grid Grid { get; set; }
        public Label TitleLabel { get; set; } // Changed from TextBox to Label
        public string URL { get; set; }
    }

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
                    Tab_TitleLabel.Background = value
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9E5"))
                        : Brushes.White;
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
        public Button Frame_HideButton { get; set; }
        public Button Frame_BackButton { get; set; }
        public WebView2 Frame_WebView { get; set; }
    }
}