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
using System.IO;
using System.Text.Json;
using BrowserTabManager;
using System.Windows.Media.Animation;

namespace BrowserTabManager
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Remove dialog fields, not needed anymore
        // private Window _newTabDialog;
        // private TextBox _dialogUrlBox;
        
        internal List<CustomBookmark> BookmarksList = new List<CustomBookmark>();
        internal List<CustomTab> TabsList = new List<CustomTab>();
        internal List<CustomScreen> ScreenList = new List<CustomScreen>();
        public FrameHelper frameHelper;
        public BookmarkHelper bookmarkHelper;
        private CustomScreen _currentScreen;
        public CustomScreen CurrentScreen
        {
            get => _currentScreen;
            set
            {
                if (_currentScreen != value)
                {
                    _currentScreen = value;
                    LoadScreen(value);
                }
            }
        }

        // Static images for bookmark toggle
        private static readonly Image BookmarkOffImage = new Image {
            Source = new BitmapImage(new Uri("pack://application:,,,/Icons/BookmarkOff.png")),
            Stretch = Stretch.Uniform,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        private static readonly Image BookmarkOnImage = new Image {
            Source = new BitmapImage(new Uri("pack://application:,,,/Icons/BookmarkOn.png")),
            Stretch = Stretch.Uniform,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        public static Image CreateBookmarkOffImage()
        {
            return new Image {
                Source = new BitmapImage(new Uri("pack://application:,,,/Icons/BookmarkOff.png")),
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
        }
        public static Image CreateBookmarkOnImage()
        {
            return new Image {
                Source = new BitmapImage(new Uri("pack://application:,,,/Icons/BookmarkOn.png")),
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
        }

        //private TextBox txtLaunchUrl => (TextBox)this.FindName("txtLaunchUrl");

        // Property to control showing/hiding bookmarks panel
        private bool _boolShowBookmarks = true;
        public bool boolShowBookmarks
        {
            get => _boolShowBookmarks;
            set
            {
                _boolShowBookmarks = value;
                if (this.FindName("CenterMasterGrid") is Grid CenterMasterGrid && CenterMasterGrid?.ColumnDefinitions.Count > 0)
                {
                    CenterMasterGrid.ColumnDefinitions[0].Width = value ? new GridLength(150) : new GridLength(0);
                    ShowHideBookmarksButton.Content = value ? "Hide Bookmarks" : "Display Bookmarks";
                }
            }
        }

        private bool _boolShowTabList = true;
        public bool boolShowTabList
        {
            get => _boolShowTabList;
            set
            {
                _boolShowTabList = value;
                if (this.FindName("CenterMasterGrid") is Grid CenterMasterGrid && CenterMasterGrid?.ColumnDefinitions.Count > 0)
                {
                    CenterMasterGrid.ColumnDefinitions[4].Width = value ? new GridLength(150) : new GridLength(0);
                    ShowHideTabListButton.Content = value ? "Hide Tab List" : "Display Tab Last";
                }
            }
        }

        private void ShowHideBookmarksButton_Click(object sender, RoutedEventArgs e)
        {
            boolShowBookmarks = !boolShowBookmarks;
        }

        private void ShowHideTabListButton_Click(object sender, RoutedEventArgs e)
        {
            boolShowTabList = !boolShowTabList;
        }

        private void NewTabButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentScreen != null)
            {
                var newTab = tabHelper.CreateTab("https://www.google.com", "Google");
                CurrentScreen.TabList.Add(newTab);
                newTab.displayFrame = true;
                this.frameHelper.OrganizeFrames();
            }
        }

        private void NewScreenButton_Click(object sender, RoutedEventArgs e)
        {
            CreateScreen();
        }

        public CustomScreen CreateScreen()
        {
            var newScreen = new CustomScreen(this.tabHelper);
            ScreenList.Add(newScreen);
            CurrentScreen = newScreen;
            return newScreen;
        }

        private void ScreenNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && CurrentScreen != null)
            {
                var newName = textBox.Text.Trim();
                if (CurrentScreen.ScreenName != newName)
                {
                    CurrentScreen.ScreenName = newName;
                }
            }
        }

        public void LoadScreen(CustomScreen screenToLoad)
        {
            // First, hide all tabs that are currently displayed.
            // The TabsList in MainWindow holds all tabs across all screens.
            foreach (var tab in TabsList)
            {
                tab.displayFrame = false;
            }

            // Then, show only the tabs belonging to the screen to load.
            if (screenToLoad != null)
            {
                foreach (var tab in screenToLoad.TabList)
                {
                    tab.displayFrame = true;
                }
                ScreenNameTextBox.Text = screenToLoad.ScreenName;
                int screenIndex = ScreenList.IndexOf(screenToLoad) + 1;
                CurrentScreenLabel.Content = $"You're on screen {screenIndex} of {ScreenList.Count}";
                var watermark = (ContentControl)ScreenNameTextBox.Template.FindName("ScreenNameWatermark", ScreenNameTextBox);
                if (watermark != null)
                {
                    watermark.Content = $"Give screen {screenIndex} a name";
                }
            }

            // Remove all rowdefinitions from the frames grid.
            this.FramesGrid.RowDefinitions.Clear();
            // Loop from 0 to CurrentScreen.RowCount - 1 and add a new row definition for each.
            int rowCount = Math.Max(1, screenToLoad.RowCount);
            for (int i = 0; i <= rowCount - 1; i++)
            {
                if (i % 2 == 0)
                {
                    // Add a new star-sized row for content
                    this.FramesGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                }
                else
                {
                    // Add a 4px high row for the splitter
                    var splitterRow = new RowDefinition { Height = new GridLength(4) };
                    this.FramesGrid.RowDefinitions.Add(splitterRow);
                }
            }

            // Re-organize the frames grid to reflect the changes.
            this.frameHelper.OrganizeFrames();
        }

        private void CurrentScreenLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ScreenNavigationBorder.Visibility = Visibility.Collapsed;
            ScreenNameTextBox.Visibility = Visibility.Visible;
            ScreenNameTextBox.Focus();
            ScreenNameTextBox.SelectAll();
        }

        public TabHelper tabHelper;

        private CustomTab _customTabInFocus;
        public CustomTab CustomTabInFocus
        {
            get => _customTabInFocus;
            set
            {
                // Reset previous tab's backgrounds if not null
                if (_customTabInFocus != null)
                {
                    var resetBrush = (Brush)new BrushConverter().ConvertFromString("#E2E6EC");
                    if (_customTabInFocus.Frame_Border != null)
                        _customTabInFocus.Frame_Border.Background = resetBrush;
                    if (_customTabInFocus.Frame_Grid != null)
                        _customTabInFocus.Frame_Grid.Background = resetBrush;
                    if (_customTabInFocus.FrameTitle_Grid != null)
                        _customTabInFocus.FrameTitle_Grid.Background = resetBrush;
                    if (_customTabInFocus.FrameAddress_Grid != null)
                        _customTabInFocus.FrameAddress_Grid.Background = resetBrush;
                    if (_customTabInFocus.Frame_TitleTextBox != null)
                        _customTabInFocus.Frame_TitleTextBox.Background = resetBrush;
                    if (_customTabInFocus.Frame_CloseButton != null)
                        _customTabInFocus.Frame_CloseButton.Background = resetBrush;
                    if (_customTabInFocus.Frame_RefreshButton != null)
                        _customTabInFocus.Frame_RefreshButton.Background = resetBrush;
                    if (_customTabInFocus.Frame_CloneButton != null)
                        _customTabInFocus.Frame_CloneButton.Background = resetBrush;
                    if (_customTabInFocus.Frame_HideButton != null)
                        _customTabInFocus.Frame_HideButton.Background = resetBrush;
                    if (_customTabInFocus.Frame_BackButton != null)
                        _customTabInFocus.Frame_BackButton.Background = resetBrush;
                    if (_customTabInFocus.Frame_ToggleBookmarkButton != null)
                        _customTabInFocus.Frame_ToggleBookmarkButton.Background = resetBrush;
                }

                _customTabInFocus = value;

                // Highlight new tab's backgrounds if not null
                if (_customTabInFocus != null)
                {
                    var highlightBrush = (Brush)new BrushConverter().ConvertFromString("#fdfd96");
                    if (_customTabInFocus.Frame_Border != null)
                        _customTabInFocus.Frame_Border.Background = highlightBrush;
                    if (_customTabInFocus.Frame_Grid != null)
                        _customTabInFocus.Frame_Grid.Background = highlightBrush;
                    if (_customTabInFocus.FrameTitle_Grid != null)
                        _customTabInFocus.FrameTitle_Grid.Background = highlightBrush;
                    if (_customTabInFocus.FrameAddress_Grid != null)
                        _customTabInFocus.FrameAddress_Grid.Background = highlightBrush;
                    if (_customTabInFocus.Frame_TitleTextBox != null)
                        _customTabInFocus.Frame_TitleTextBox.Background = highlightBrush;
                    if (_customTabInFocus.Frame_CloseButton != null)
                        _customTabInFocus.Frame_CloseButton.Background = highlightBrush;
                    if (_customTabInFocus.Frame_RefreshButton != null)
                        _customTabInFocus.Frame_RefreshButton.Background = highlightBrush;
                    if (_customTabInFocus.Frame_CloneButton != null)
                        _customTabInFocus.Frame_CloneButton.Background = highlightBrush;
                    if (_customTabInFocus.Frame_HideButton != null)
                        _customTabInFocus.Frame_HideButton.Background = highlightBrush;
                    if (_customTabInFocus.Frame_BackButton != null)
                        _customTabInFocus.Frame_BackButton.Background = highlightBrush;
                    if (_customTabInFocus.Frame_ToggleBookmarkButton != null)
                        _customTabInFocus.Frame_ToggleBookmarkButton.Background = highlightBrush;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            frameHelper = new FrameHelper(this);
            tabHelper = new TabHelper(this);
            bookmarkHelper = new BookmarkHelper(this);

            txtSearchBookmarks.TextChanged += this.bookmarkHelper.TxtSearchBookmarks_TextChanged;
            txtSearchTabs.TextChanged += TxtSearchTabs_TextChanged;

            ShowHideBookmarksButton.Click += ShowHideBookmarksButton_Click;
            ShowHideTabListButton.Click += ShowHideTabListButton_Click;
            NewTabButton.Click += NewTabButton_Click;
            NewScreenButton.Click += NewScreenButton_Click;
            ScreenNameTextBox.TextChanged += ScreenNameTextBox_TextChanged;
            btnAddRowToFramesGrid.Click += this.frameHelper.btnAddRowToFramesGrid_Click;
            btnSubtractRowFromFramesGrid.Click += this.frameHelper.btnSubtractRowFromFramesGrid_Click;
            btnPreviousScreen.Click += BtnPreviousScreen_Click;
            btnNextScreen.Click += BtnNextScreen_Click;
            CurrentScreenLabel.MouseLeftButtonUp += CurrentScreenLabel_MouseLeftButtonUp;

            boolShowBookmarks = true;
            boolShowTabList = true;

            bool loadedFromFile = false;
            try
            {
                if (File.Exists("bookmarks.json"))
                {
                    var json = File.ReadAllText("bookmarks.json");
                    var bookmarks = JsonSerializer.Deserialize<List<BookmarkJsonEntry>>(json);
                    if (bookmarks != null && bookmarks.Count > 0)
                    {
                        foreach (var entry in bookmarks)
                        {
                            bookmarkHelper.CreateBookmark(entry.URL, entry.Title);
                        }
                        loadedFromFile = true;
                    }
                }
            }
            catch { /* Ignore and fall back to hardcoded bookmarks */ }

            if (!loadedFromFile)
            {
                // Add bookmarks for the provided URLs with cleaned-up names
                this.bookmarkHelper.CreateBookmark("https://www.amazon.com/", "Amazon");
                this.bookmarkHelper.CreateBookmark("https://www.bing.com/", "Bing");
                this.bookmarkHelper.CreateBookmark("https://www.canva.com/", "Canva");
                this.bookmarkHelper.CreateBookmark("https://chatgpt.com/", "ChatGPT");
                this.bookmarkHelper.CreateBookmark("https://www.craigslist.org/", "Craigslist");
                this.bookmarkHelper.CreateBookmark("https://www.discord.com/", "Discord");
                this.bookmarkHelper.CreateBookmark("https://www.duckduckgo.com/", "DuckDuckGo");
                this.bookmarkHelper.CreateBookmark("https://www.ebay.com/", "Ebay");
                this.bookmarkHelper.CreateBookmark("https://www.facebook.com/", "Facebook");
                this.bookmarkHelper.CreateBookmark("https://www.fandom.com/", "Fandom");
                this.bookmarkHelper.CreateBookmark("https://www.google.com/", "Google");
                this.bookmarkHelper.CreateBookmark("https://www.imdb.com/", "IMDb");
                this.bookmarkHelper.CreateBookmark("https://www.instagram.com/", "Instagram");
                this.bookmarkHelper.CreateBookmark("https://www.linkedin.com/", "LinkedIn");
                this.bookmarkHelper.CreateBookmark("https://www.live.com/", "Live");
                this.bookmarkHelper.CreateBookmark("https://www.microsoft.com/", "Microsoft");
                this.bookmarkHelper.CreateBookmark("https://www.microsoftonline.com/", "Microsoft Online");
                this.bookmarkHelper.CreateBookmark("https://www.netflix.com/", "Netflix");
                this.bookmarkHelper.CreateBookmark("https://www.nytimes.com/", "NY Times");
                this.bookmarkHelper.CreateBookmark("https://www.office.com/", "Office");
                this.bookmarkHelper.CreateBookmark("https://www.openai.com/", "OpenAI");
                this.bookmarkHelper.CreateBookmark("https://www.pinterest.com/", "Pinterest");
                this.bookmarkHelper.CreateBookmark("https://www.reddit.com/", "Reddit");
                this.bookmarkHelper.CreateBookmark("https://www.sharepoint.com/", "SharePoint");
                this.bookmarkHelper.CreateBookmark("https://www.temu.com/", "Temu");
                this.bookmarkHelper.CreateBookmark("https://www.tiktok.com/", "TikTok");
                this.bookmarkHelper.CreateBookmark("https://www.tumblr.com/", "Tumblr");
                this.bookmarkHelper.CreateBookmark("https://www.twitch.tv/", "Twitter (X.com)");
                this.bookmarkHelper.CreateBookmark("https://www.twitter.com/", "Walmart");
                this.bookmarkHelper.CreateBookmark("https://www.weather.com/", "Weather");
                this.bookmarkHelper.CreateBookmark("https://www.whatsapp.com/", "WhatsApp");
                this.bookmarkHelper.CreateBookmark("https://www.wikipedia.org/", "Wikipedia");
                this.bookmarkHelper.CreateBookmark("https://www.yahoo.com/", "Yahoo");
                this.bookmarkHelper.CreateBookmark("https://www.youtube.com/", "YouTube");
            }

            // Load screens and tabs from screens.json
            try
            {
                if (File.Exists("screens.json"))
                {
                    var json = File.ReadAllText("screens.json");
                    var screenEntries = JsonSerializer.Deserialize<List<ScreenJsonEntry>>(json);
                    if (screenEntries != null && screenEntries.Count > 0)
                    {
                        foreach (var screenEntry in screenEntries)
                        {
                            var newScreen = new CustomScreen(this.tabHelper);
                            ScreenList.Add(newScreen);
                            foreach (var tabEntry in screenEntry.TabList)
                            {
                                var newTab = tabHelper.CreateTab(tabEntry.URL, tabEntry.Title);
                                newTab.displayFrame = tabEntry.DisplayFrame;
                                newScreen.TabList.Add(newTab);
                            }
                        }
                    }
                }
            }
            catch { /* Ignore and start fresh */ }

            if (ScreenList.Any())
            {
                CurrentScreen = ScreenList.Last();
            }
            else
            {
                // Create a default screen if none were loaded
                var defaultScreen = new CustomScreen (this.tabHelper);
                ScreenList.Add(defaultScreen);
                CurrentScreen = defaultScreen;
            }

            this.frameHelper.OrganizeFrames();
        }

        private void BtnNextScreen_Click(object sender, RoutedEventArgs e)
        {
            if (ScreenList.Count > 1)
            {
                int currentIndex = ScreenList.IndexOf(CurrentScreen);
                int nextIndex = (currentIndex + 1) % ScreenList.Count;
                CurrentScreen = ScreenList[nextIndex];
            }
        }

        private void BtnPreviousScreen_Click(object sender, RoutedEventArgs e)
        {
            if (ScreenList.Count > 1)
            {
                int currentIndex = ScreenList.IndexOf(CurrentScreen);
                int previousIndex = (currentIndex - 1 + ScreenList.Count) % ScreenList.Count;
                CurrentScreen = ScreenList[previousIndex];
            }
        }

        public async Task<string> FetchPageTitleAsync(string url)
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

        public void CloneTab(CustomTab customTab)
        {
            if (customTab == null || customTab.Frame_WebView == null) return;
            tabHelper.CreateTabInternal(customTab.Frame_UrlTextBox?.Text ?? "", customTab.Frame_TitleTextBox?.Text ?? "", customTab.Frame_WebView);
        }

        public void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            // Prevent the default new window behavior
            e.Handled = true;
            string uri = e.Uri;
            if (!string.IsNullOrEmpty(uri))
            {
                // Use the link's URL as both urlString and nameString for now
                tabHelper.CreateTab(uri, uri);
            }
        }

        internal async Task UpdateTabTitleFromUrlAsync(CustomTab tab, string url)
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

        protected override void OnClosing(CancelEventArgs e)
        {
            // Save bookmarks to JSON
            var bookmarksToSave = BookmarksList.Select(b => new { Title = b.TitleLabel.Content?.ToString() ?? "", URL = b.URL }).ToList();
            var json = JsonSerializer.Serialize(bookmarksToSave, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("bookmarks.json", json);

            // Save screens and their tabs to JSON
            var screensToSave = ScreenList.Select(s => new ScreenJsonEntry
            {
                ScreenName = s.ScreenName,
                TabList = s.TabList.Select(t => new OpenTabJsonEntry
                {
                    Title = t.Frame_TitleTextBox?.Text ?? "",
                    URL = t.Frame_UrlTextBox?.Text ?? "",
                    DisplayFrame = t.displayFrame
                }).ToList()
            }).ToList();
            var screensJson = JsonSerializer.Serialize(screensToSave, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("screens.json", screensJson);

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
                string name = string.Empty;
                if (tab.Tab_TitleLabel?.Content is TextBlock tb)
                    name = tb.Text.ToLower();
                else
                    name = tab.Tab_TitleLabel?.Content?.ToString().ToLower() ?? string.Empty;
                if (string.IsNullOrEmpty(search) || url.Contains(search) || name.Contains(search))
                {
                    filtered.Add(tab);
                }
            }
            foreach (var tab in filtered.OrderBy(t => (t.Tab_TitleLabel?.Content as TextBlock)?.Text ?? t.Tab_TitleLabel?.Content?.ToString(), StringComparer.OrdinalIgnoreCase))
            {
                TabStack.Children.Add(tab.Tab_Border);
            }
        }
    }
}