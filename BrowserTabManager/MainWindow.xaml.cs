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

        private static Image CreateBookmarkOffImage()
        {
            return new Image {
                Source = new BitmapImage(new Uri("pack://application:,,,/Icons/BookmarkOff.png")),
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
        }
        private static Image CreateBookmarkOnImage()
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
                OrganizeFrames();
            }
        }

        private void NewScreenButton_Click(object sender, RoutedEventArgs e)
        {
            var newScreen = new CustomScreen (this.tabHelper);
            ScreenList.Add(newScreen);
            CurrentScreen = newScreen;
        }

        // Launch a new tab from a URL string (refactored from ScreenNameTextBox_KeyDown)
        private void LaunchTabFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            // Simple URL validation: if it contains spaces or doesn't look like a domain, go directly to Google
            bool isLikelyUrl = !url.Contains(' ') &&
                (url.Contains('.') || url.StartsWith("http://") || url.StartsWith("https://")) &&
                !url.Any(c => "<>\"'{}|\\^`".Contains(c));

            if (!isLikelyUrl)
            {
                // Use Google search immediately
                string searchUrl = $"https://www.google.com/search?q={System.Net.WebUtility.UrlEncode(url)}";
                tabHelper.CreateTab(searchUrl, url);
                return;
            }

            tabHelper.CreateTab(url, url);
        }

        private void ScreenNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender is TextBox textBox && CurrentScreen != null)
                {
                    var newName = textBox.Text.Trim();
                    if (!string.IsNullOrEmpty(newName) && CurrentScreen.ScreenName != newName)
                    {
                        CurrentScreen.ScreenName = newName;
                    }
                }
                e.Handled = true;
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
                CurrentScreenLabel.Content = $"Screen {screenIndex} of {ScreenList.Count}";
            }

            // Re-organize the frames grid to reflect the changes.
            OrganizeFrames();
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
            tabHelper = new TabHelper(this);
            txtSearchBookmarks.TextChanged += TxtSearchBookmarks_TextChanged;
            txtSearchTabs.TextChanged += TxtSearchTabs_TextChanged;

            ShowHideBookmarksButton.Click += ShowHideBookmarksButton_Click;
            ShowHideTabListButton.Click += ShowHideTabListButton_Click;
            NewTabButton.Click += NewTabButton_Click;
            NewScreenButton.Click += NewScreenButton_Click;
            ScreenNameTextBox.KeyDown += new KeyEventHandler(ScreenNameTextBox_KeyDown);
            btnAddRowToFramesGrid.Click += btnAddRowToFramesGrid_Click;
            btnSubtractRowFromFramesGrid.Click += btnSubtractRowFromFramesGrid_Click;
            btnPreviousScreen.Click += BtnPreviousScreen_Click;
            btnNextScreen.Click += BtnNextScreen_Click;

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
                            CreateBookmark(entry.URL, entry.Title);
                        }
                        loadedFromFile = true;
                    }
                }
            }
            catch { /* Ignore and fall back to hardcoded bookmarks */ }

            if (!loadedFromFile)
            {
                // Add bookmarks for the provided URLs with cleaned-up names
                CreateBookmark("https://www.amazon.com/", "Amazon");
                CreateBookmark("https://www.bing.com/", "Bing");
                CreateBookmark("https://www.canva.com/", "Canva");
                CreateBookmark("https://chatgpt.com/", "ChatGPT");
                CreateBookmark("https://www.craigslist.org/", "Craigslist");
                CreateBookmark("https://www.discord.com/", "Discord");
                CreateBookmark("https://www.duckduckgo.com/", "DuckDuckGo");
                CreateBookmark("https://www.ebay.com/", "Ebay");
                CreateBookmark("https://www.facebook.com/", "Facebook");
                CreateBookmark("https://www.fandom.com/", "Fandom");
                CreateBookmark("https://www.google.com/", "Google");
                CreateBookmark("https://www.imdb.com/", "IMDb");
                CreateBookmark("https://www.instagram.com/", "Instagram");
                CreateBookmark("https://www.linkedin.com/", "LinkedIn");
                CreateBookmark("https://www.live.com/", "Live");
                CreateBookmark("https://www.microsoft.com/", "Microsoft");
                CreateBookmark("https://www.microsoftonline.com/", "Microsoft Online");
                CreateBookmark("https://www.netflix.com/", "Netflix");
                CreateBookmark("https://www.nytimes.com/", "NY Times");
                CreateBookmark("https://www.office.com/", "Office");
                CreateBookmark("https://www.openai.com/", "OpenAI");
                CreateBookmark("https://www.pinterest.com/", "Pinterest");
                CreateBookmark("https://www.reddit.com/", "Reddit");
                CreateBookmark("https://www.sharepoint.com/", "SharePoint");
                CreateBookmark("https://www.temu.com/", "Temu");
                CreateBookmark("https://www.tiktok.com/", "TikTok");
                CreateBookmark("https://www.tumblr.com/", "Tumblr");
                CreateBookmark("https://www.twitch.tv/", "Twitter (X.com)");
                CreateBookmark("https://www.twitter.com/", "Walmart");
                CreateBookmark("https://www.weather.com/", "Weather");
                CreateBookmark("https://www.whatsapp.com/", "WhatsApp");
                CreateBookmark("https://www.wikipedia.org/", "Wikipedia");
                CreateBookmark("https://www.yahoo.com/", "Yahoo");
                CreateBookmark("https://www.youtube.com/", "YouTube");
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

            OrganizeFrames();
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

        internal void CreateBookmark(string urlString, string nameString)
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
            bookmarkNameLabel.Background = Brushes.Transparent;
            bookmarkNameLabel.MouseEnter += (s, e) =>
            {
                if (s is Label lbl)
                {
                    var anim = new System.Windows.Media.Animation.ColorAnimation
                    {
                        To = (Color)ColorConverter.ConvertFromString("#FFEFEFEF"),
                        Duration = new Duration(TimeSpan.FromMilliseconds(200))
                    };
                    var brush = lbl.Background as SolidColorBrush;
                    if (brush == null || brush.IsFrozen || brush == Brushes.Transparent)
                    {
                        brush = new SolidColorBrush(Colors.Transparent);
                        lbl.Background = brush;
                    }
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
                }
            };
            bookmarkNameLabel.MouseLeave += (s, e) =>
            {
                if (s is Label lbl)
                {
                    var anim = new System.Windows.Media.Animation.ColorAnimation
                    {
                        To = Colors.Transparent,
                        Duration = new Duration(TimeSpan.FromMilliseconds(200))
                    };
                    var brush = lbl.Background as SolidColorBrush;
                    if (brush == null || brush.IsFrozen)
                    {
                        brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEFEFEF"));
                        lbl.Background = brush;
                    }
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
                }
            };
            Grid.SetColumn(bookmarkNameLabel, 0);
            bookmarkGrid.Children.Add(bookmarkNameLabel);
            customBookmark.TitleLabel = bookmarkNameLabel;
            bookmarkNameLabel.Tag = customBookmark;
            bookmarkNameLabel.Content = nameString;

            // Assign URL
            customBookmark.URL = urlString;

            // Add to stack (for initial creation)
            BookmarkStack.Children.Add(bookmarkBorder);

            // Attach context menu for right-click
            BookmarkContextMenuHelper.AttachContextMenu(this, customBookmark);

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
                tabHelper.CreateTab(customBookmark.URL, label.Content?.ToString() ?? string.Empty);
            }
        }

        public void CloneTab(CustomTab customTab)
        {
            if (customTab == null || customTab.Frame_WebView == null) return;
            tabHelper.CreateTabInternal(customTab.Frame_UrlTextBox?.Text ?? "", customTab.Frame_TitleTextBox?.Text ?? "", customTab.Frame_WebView);
        }

        internal async void FrameWebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
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
                // Bookmark toggle logic
                if (customTab.Frame_UrlTextBox != null && customTab.Frame_ToggleBookmarkButton != null && BookmarksList != null)
                {
                    string urlText = customTab.Frame_UrlTextBox.Text?.Trim();
                    bool found = BookmarksList.Any(b => string.Equals(b.URL?.Trim(), urlText, StringComparison.OrdinalIgnoreCase));
                    var offImg = GetType().GetMethod("CreateBookmarkOffImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, null);
                    var onImg = GetType().GetMethod("CreateBookmarkOnImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, null);
                    customTab.Frame_ToggleBookmarkButton.Content = found ? onImg : offImg;
                    customTab.Frame_ToggleBookmarkButton.Tag = found;
                }
            }
        }

        internal void FrameWebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
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
                tabHelper.CreateTab(uri, uri);
            }
        }

        internal void FrameBackButton_Click(object sender, RoutedEventArgs e)
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

        internal void TabTitleLabel_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Label label && label.Tag is CustomTab customTab)
            {
                customTab.displayFrame = !customTab.displayFrame;
                OrganizeFrames();
            }
        }

        internal void HideTabFrame(CustomTab customTab)
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

        internal void CloseTab(CustomTab customTab)
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

        internal void OrganizeFrames()
        {
            // Use only CustomTabs with displayFrame == true
            var frameTabs = TabsList.Where(t => t.displayFrame).ToList();

            // Ensure FramesGrid has enough columns using the new logic
            int rowCount = FramesGrid.RowDefinitions.Count > 0 ? FramesGrid.RowDefinitions.Count : 1;
            int needed = (int)(System.Math.Ceiling((double)frameTabs.Count / System.Math.Ceiling((double)rowCount/2)) * 2) - 1;
            while (FramesGrid.ColumnDefinitions.Count < needed)
            {
                int newColIndex = FramesGrid.ColumnDefinitions.Count;
                if (newColIndex % 2 == 0)
                {
                    // Even index: star size, empty
                    var colDef = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
                    FramesGrid.ColumnDefinitions.Add(colDef);
                }
                else
                {
                    // Odd index: width 4
                    var colDef = new ColumnDefinition { Width = new GridLength(4) };
                    FramesGrid.ColumnDefinitions.Add(colDef);
                }
            }
            while (FramesGrid.ColumnDefinitions.Count > needed && FramesGrid.ColumnDefinitions.Count > 1)
            {
                FramesGrid.ColumnDefinitions.RemoveAt(FramesGrid.ColumnDefinitions.Count - 1);
            }

            // Remove all children
            FramesGrid.Children.Clear();

            // Place frames and splitters
            int frameIndex = 0;
            for (int row = 0; row < FramesGrid.RowDefinitions.Count; row++)
            {
                // if row is even, change row height to star size
                if (row % 2 == 0)
                {
                    FramesGrid.RowDefinitions[row].Height = new GridLength(1, GridUnitType.Star);
                }
                //else
                //{
                //    FramesGrid.RowDefinitions[row].Height = new GridLength(4); // Splitter row
                //}


                for (int col = 0; col < FramesGrid.ColumnDefinitions.Count; col++)
                {
                    // if col is even, change column width to star size
                    if (col % 2 == 0)
                    {
                        FramesGrid.ColumnDefinitions[col].Width = new GridLength(1, GridUnitType.Star);
                    }

                    if (row % 2 == 0 && col % 2 == 0)
                    {
                        // Frame cell
                        if (frameIndex < frameTabs.Count)
                        {
                            var tab = frameTabs[frameIndex];
                            bool alreadyPresent = false;
                            foreach (UIElement child in FramesGrid.Children)
                            {
                                if (Grid.GetRow(child) == row && Grid.GetColumn(child) == col && child == tab.Frame_Border)
                                {
                                    alreadyPresent = true;
                                    break;
                                }
                            }
                            if (!alreadyPresent)
                            {
                                // Remove any existing child in this cell
                                var toRemove = FramesGrid.Children.Cast<UIElement>().FirstOrDefault(child => Grid.GetRow(child) == row && Grid.GetColumn(child) == col);
                                if (toRemove != null)
                                    FramesGrid.Children.Remove(toRemove);
                                Grid.SetRow(tab.Frame_Border, row);
                                Grid.SetColumn(tab.Frame_Border, col);
                                FramesGrid.Children.Add(tab.Frame_Border);
                            }
                            frameIndex++;
                        }
                    }
                    else if (row % 2 == 0 && col % 2 == 1)
                    {
                        // Vertical splitter
                        var existing = FramesGrid.Children.Cast<UIElement>().FirstOrDefault(child => Grid.GetRow(child) == row && Grid.GetColumn(child) == col && child is GridSplitter && ((GridSplitter)child).ResizeDirection == GridResizeDirection.Columns);
                        if (existing == null)
                        {
                            var splitter = new GridSplitter
                            {
                                Width = 4,
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Stretch,
                                Background = Brushes.Transparent,
                                ResizeDirection = GridResizeDirection.Columns
                            };
                            Grid.SetRow(splitter, row);
                            Grid.SetColumn(splitter, col);
                            FramesGrid.Children.Add(splitter);
                        }
                    }
                    else if (row % 2 == 1 && col % 2 == 0)
                    {
                        // Horizontal splitter
                        var existing = FramesGrid.Children.Cast<UIElement>().FirstOrDefault(child => Grid.GetRow(child) == row && Grid.GetColumn(child) == col && child is GridSplitter && ((GridSplitter)child).ResizeDirection == GridResizeDirection.Rows);
                        if (existing == null)
                        {
                            var splitter = new GridSplitter
                            {
                                Height = 4,
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Stretch,
                                Background = Brushes.Transparent,
                                ResizeDirection = GridResizeDirection.Rows
                            };
                            Grid.SetRow(splitter, row);
                            Grid.SetColumn(splitter, col);
                            FramesGrid.Children.Add(splitter);
                        }
                    }
                    // else (row % 2 == 1 && col % 2 == 1): leave empty
                }
            }
        }

        private void btnAddRowToFramesGrid_Click(object sender, RoutedEventArgs e)
        {
            // Add a 4px high row for the splitter
            var splitterRow = new RowDefinition { Height = new GridLength(4) };
            FramesGrid.RowDefinitions.Add(splitterRow);
            // Add a new star-sized row for content
            FramesGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            OrganizeFrames();
        }

        private void btnSubtractRowFromFramesGrid_Click(object sender, RoutedEventArgs e)
        {
            // Remove the last two rows if there are at least two rows
            if (FramesGrid.RowDefinitions.Count >= 2)
            {
                FramesGrid.RowDefinitions.RemoveAt(FramesGrid.RowDefinitions.Count - 1);
                FramesGrid.RowDefinitions.RemoveAt(FramesGrid.RowDefinitions.Count - 1);
                OrganizeFrames();
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

        internal async void FrameUrlTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textBox && textBox.Tag is CustomTab customTab)
            {
                var webView = customTab.Frame_WebView;
                string url = textBox.Text.Trim();
                if (!string.IsNullOrEmpty(url))
                {
                    // Simple URL validation: if it contains spaces or doesn't look like a domain, go directly to Google
                    bool isLikelyUrl = !url.Contains(' ') &&
                        (url.Contains('.') || url.StartsWith("http://") || url.StartsWith("https://")) &&
                        !url.Any(c => "<>\"'{}|\\^`".Contains(c));

                    if (!isLikelyUrl)
                    {
                        // Use Google search immediately
                        string searchUrl = $"https://www.google.com/search?q={System.Net.WebUtility.UrlEncode(url)}";
                        try { webView.Source = new System.Uri(searchUrl); } catch { }
                        return;
                    }

                    bool loaded = false;
                    try
                    {
                        // Wait for navigation to complete or fail
                        var tcs = new TaskCompletionSource<bool>();
                        void handler(object s, CoreWebView2NavigationCompletedEventArgs args)
                        {
                            tcs.TrySetResult(args.IsSuccess);
                            webView.NavigationCompleted -= handler;
                        }
                        webView.NavigationCompleted += handler;
                        try
                        {
                            webView.Source = new System.Uri(url.StartsWith("http") ? url : $"https://{url}");
                        }
                        catch { tcs.TrySetResult(false); }
                        loaded = await tcs.Task;
                    }
                    catch { loaded = false; }

                    if (!loaded)
                    {
                        // Use Google search
                        string searchUrl = $"https://www.google.com/search?q={System.Net.WebUtility.UrlEncode(url)}";
                        try { webView.Source = new System.Uri(searchUrl); } catch { }
                    }
                }
            }
        }

        internal void FrameTitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is CustomTab customTab && customTab.Tab_TitleLabel != null)
            {
                if (customTab.Tab_TitleLabel.Content is TextBlock tb) tb.Text = textBox.Text;
            }
        }

        // Context menu handlers for bookmarks
        internal void AddBookmarkContext(CustomBookmark bookmark)
        {
            // Show AddBookmarkDialog
            var dialog = new AddBookmarkDialog { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                // Use dialog input to create the new bookmark
                CreateBookmark(dialog.BookmarkUrl, dialog.BookmarkTitle);
                // Move the new bookmark visually below the current one
                int idx = BookmarksList.IndexOf(bookmark);
                var newBookmark = BookmarksList.Last();
                BookmarkStack.Children.Remove(newBookmark.Border);
                int insertIdx = idx >= 0 ? idx + 1 : BookmarkStack.Children.Count;
                BookmarkStack.Children.Insert(insertIdx, newBookmark.Border);
            }
        }

        internal void EditBookmarkContext(CustomBookmark bookmark)
        {
            // Example: Prompt for new title and URL
            var inputDialog = new EditBookmarkDialog(bookmark.TitleLabel.Content?.ToString() ?? "", bookmark.URL);
            if (inputDialog.ShowDialog() == true)
            {
                bookmark.TitleLabel.Content = inputDialog.BookmarkTitle;
                bookmark.URL = inputDialog.BookmarkUrl;
            }
        }

        internal void DeleteBookmarkContext(CustomBookmark bookmark)
        {
            var dialog = new DeleteBookmarkDialog(bookmark.TitleLabel.Content?.ToString() ?? "");
            dialog.Owner = this;
            if (dialog.ShowDialog() == true && dialog.IsConfirmed)
            {
                BookmarksList.Remove(bookmark);
                BookmarkStack.Children.Remove(bookmark.Border);
            }
        }

        private class BookmarkJsonEntry
        {
            public string Title { get; set; }
            public string URL { get; set; }
        }

        public class OpenTabJsonEntry
        {
            public string Title { get; set; }
            public string URL { get; set; }
            public bool DisplayFrame { get; set; }
        }

        public class ScreenJsonEntry
        {
            public string ScreenName { get; set; }
            public List<OpenTabJsonEntry> TabList { get; set; }
        }


    }

    // CustomBookmark and CustomTab classes have been moved to their own files.
}