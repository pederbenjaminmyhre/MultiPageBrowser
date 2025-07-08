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

namespace BrowserTabManager
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<CustomBookmark> BookmarksList = new List<CustomBookmark>();
        private List<CustomTab> TabsList = new List<CustomTab>();

        // Static images for bookmark toggle
        private static readonly Image BookmarkOffImage = new Image {
            Source = new BitmapImage(new Uri("pack://application:,,,/BookmarkOff.png")),
            Stretch = Stretch.Uniform,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        private static readonly Image BookmarkOnImage = new Image {
            Source = new BitmapImage(new Uri("pack://application:,,,/BookmarkOn.png")),
            Stretch = Stretch.Uniform,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        //private TextBox txtLaunchUrl => (TextBox)this.FindName("txtLaunchUrl");

        public MainWindow()
        {
            InitializeComponent();
            txtSearchBookmarks.TextChanged += TxtSearchBookmarks_TextChanged;
            txtLaunchUrl.KeyDown += TxtLaunchUrl_KeyDown;
            txtSearchTabs.TextChanged += TxtSearchTabs_TextChanged;

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
                CreateBookmark("https://www.chatgpt.com/", "ChatGPT");
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
                CreateBookmark("https://www.twitch.tv/", "Twitch");
                CreateBookmark("https://www.twitter.com/", "Twitter (X.com)");
                CreateBookmark("https://www.walmart.com/", "Walmart");
                CreateBookmark("https://www.weather.com/", "Weather");
                CreateBookmark("https://www.whatsapp.com/", "WhatsApp");
                CreateBookmark("https://www.wikipedia.org/", "Wikipedia");
                CreateBookmark("https://www.yahoo.com/", "Yahoo");
                CreateBookmark("https://www.youtube.com/", "YouTube");
            }

            // Load open tabs from opentabs.json
            try
            {
                if (File.Exists("opentabs.json"))
                {
                    var json = File.ReadAllText("opentabs.json");
                    var tabs = JsonSerializer.Deserialize<List<OpenTabJsonEntry>>(json);
                    if (tabs != null && tabs.Count > 0)
                    {
                        foreach (var entry in tabs)
                        {
                            var tab = new CustomTab();
                            CreateTab(entry.URL, entry.Title);
                            // Set displayFrame after tab is created
                            var createdTab = TabsList.LastOrDefault();
                            if (createdTab != null)
                            {
                                createdTab.displayFrame = entry.DisplayFrame;
                            }
                        }
                    }
                }
                OrganizeFrames();
            }
            catch { /* Ignore and do not open tabs if error */ }
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

        private void CreateTabInternal(string urlString, string nameString, WebView2 webViewToClone = null)
        {
            var customTab = new CustomTab();
            TabsList.Add(customTab);

            // Tab Border
            var tab_Border = new Border();
            tab_Border.Background = Brushes.Transparent;
            TabStack.Children.Add(tab_Border);
            customTab.Tab_Border = tab_Border;
            tab_Border.Tag = customTab;

            // Tab Grid
            var tab_Grid = new Grid();
            tab_Grid.Background = Brushes.Transparent;
            tab_Border.Child = tab_Grid;
            customTab.Tab_Grid = tab_Grid;
            tab_Grid.Tag = customTab;
            tab_Grid.RowDefinitions.Add(new RowDefinition());
            tab_Grid.ColumnDefinitions.Add(new ColumnDefinition());

            // Tab Title Label
            var tab_TitleLabel = new Label();
            tab_TitleLabel.Margin = new Thickness(0, 3, 8, 3);
            tab_TitleLabel.Background = Brushes.White;
            var labelTemplate = new ControlTemplate(typeof(Label));
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(0,8,8,0));
            borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Label.BackgroundProperty));
            borderFactory.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Label.BorderBrushProperty));
            borderFactory.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Label.BorderThicknessProperty));
            var contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(ContentPresenter.ContentProperty, new TemplateBindingExtension(Label.ContentProperty));
            contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, new TemplateBindingExtension(Label.HorizontalContentAlignmentProperty));
            contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, new TemplateBindingExtension(Label.VerticalContentAlignmentProperty));
            contentPresenterFactory.SetValue(ContentPresenter.MarginProperty, new TemplateBindingExtension(Label.PaddingProperty));
            borderFactory.AppendChild(contentPresenterFactory);
            labelTemplate.VisualTree = borderFactory;
            var labelStyle = new Style(typeof(Label));
            labelStyle.Setters.Add(new Setter(Label.TemplateProperty, labelTemplate));
            tab_TitleLabel.Style = labelStyle;
            Grid.SetRow(tab_TitleLabel, 0);
            Grid.SetColumn(tab_TitleLabel, 0);
            tab_Grid.Children.Add(tab_TitleLabel);
            customTab.Tab_TitleLabel = tab_TitleLabel;
            tab_TitleLabel.Tag = customTab;
            var tabTitleTextBlock = new TextBlock {
                Text = nameString,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 160,
                Foreground = Brushes.Black,
                FontFamily = new FontFamily("Verella Round"),
                FontWeight = FontWeights.SemiBold,
                Effect = new System.Windows.Media.Effects.DropShadowEffect {
                    Color = Colors.Black,
                    BlurRadius = 2,
                    ShadowDepth = 0.5,
                    Opacity = 0.4
                }
            };
            tab_TitleLabel.Content = tabTitleTextBlock;
            tab_TitleLabel.MouseLeftButtonUp += TabTitleLabel_Click;
            tab_TitleLabel.MouseEnter += (s, e) => {
                if (s is Label lbl) {
                    var anim = new System.Windows.Media.Animation.ColorAnimation {
                        To = (Color)ColorConverter.ConvertFromString("#FFEFEFEF"),
                        Duration = new Duration(TimeSpan.FromMilliseconds(200))
                    };
                    var brush = lbl.Background as SolidColorBrush;
                    if (brush == null || brush.IsFrozen || brush == Brushes.Transparent) {
                        brush = new SolidColorBrush(Colors.Transparent);
                        lbl.Background = brush;
                    }
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
                }
            };
            tab_TitleLabel.MouseLeave += (s, e) => {
                if (s is Label lbl && lbl.Tag is CustomTab tab)
                {
                    lbl.Background = tab.TabTitleLabelBackground;
                }
            };

            // Frame Border
            var frame_Border = new Border {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(4),
                Background = Brushes.White
            };
            customTab.Frame_Border = frame_Border;
            frame_Border.Tag = customTab;

            // Frame Grid
            var frame_Grid = new Grid {
                Background = Brushes.White
            };
            frame_Border.Child = frame_Grid;
            customTab.Frame_Grid = frame_Grid;
            frame_Grid.Tag = customTab;
            frame_Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            frame_Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            frame_Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Frame Title Grid
            var frameTitle_Grid = new Grid {
                Background = Brushes.White
            };
            Grid.SetRow(frameTitle_Grid, 0);
            frame_Grid.Children.Add(frameTitle_Grid);
            customTab.FrameTitle_Grid = frameTitle_Grid;
            frameTitle_Grid.Tag = customTab;
            frameTitle_Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            frameTitle_Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            frameTitle_Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            frameTitle_Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Frame Address Grid
            var frameAddress_Grid = new Grid();
            Grid.SetRow(frameAddress_Grid, 1);
            frame_Grid.Children.Add(frameAddress_Grid);
            customTab.FrameAddress_Grid = frameAddress_Grid;
            frameAddress_Grid.Tag = customTab;
            frameAddress_Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            frameAddress_Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            frameAddress_Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            frameAddress_Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Frame Refresh Button
            var frame_RefreshButton = new Button { Name = "btnRefresh", Height = 15, Width = 15, FontSize = 9, FontWeight = FontWeights.Bold, Background = Brushes.White, BorderThickness = new Thickness(0), BorderBrush = Brushes.Transparent };
            frame_RefreshButton.ToolTip = "Refresh";
            var refreshImage = new Image {
                Source = new BitmapImage(new Uri("pack://application:,,,/Refresh.png")),
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            frame_RefreshButton.Content = refreshImage;
            Grid.SetColumn(frame_RefreshButton, 0);
            frameAddress_Grid.Children.Add(frame_RefreshButton);
            customTab.Frame_RefreshButton = frame_RefreshButton;
            frame_RefreshButton.Tag = customTab;
            frame_RefreshButton.Click += (s, e) => {
                if (s is Button btn && btn.Tag is CustomTab tab && tab.Frame_WebView != null) {
                    tab.Frame_WebView.Reload();
                }
            };

            // Frame Back Button
            var frame_BackButton = new Button { Height = 15, Width = 15, FontSize = 9, FontWeight = FontWeights.Bold, Background = Brushes.White, BorderThickness = new Thickness(0), BorderBrush = Brushes.Transparent };
            frame_BackButton.ToolTip = "Back";
            var backImage = new Image {
                Source = new BitmapImage(new Uri("pack://application:,,,/Back.png")),
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            frame_BackButton.Content = backImage;
            Grid.SetColumn(frame_BackButton, 1);
            frameAddress_Grid.Children.Add(frame_BackButton);
            customTab.Frame_BackButton = frame_BackButton;
            frame_BackButton.Tag = customTab;
            frame_BackButton.Click += FrameBackButton_Click;

            // Frame Url TextBox
            var frame_UrlTextBox = new TextBox();
            Grid.SetColumn(frame_UrlTextBox, 2);
            frameAddress_Grid.Children.Add(frame_UrlTextBox);
            customTab.Frame_UrlTextBox = frame_UrlTextBox;
            frame_UrlTextBox.Tag = customTab;
            frame_UrlTextBox.Text = urlString;
            frame_UrlTextBox.KeyDown += FrameUrlTextBox_KeyDown;
            frame_UrlTextBox.PreviewMouseDown += (s, e) => {
                if (s is TextBox tb && tb.Tag is CustomTab tab) {
                    if (!tb.IsKeyboardFocusWithin) {
                        tab.frameUrlTextBoxClickState = 0;
                        return;
                    }
                    tab.frameUrlTextBoxClickState = (tab.frameUrlTextBoxClickState + 1) % 3;
                    if (tab.frameUrlTextBoxClickState == 0) {
                        return;
                    } else if (tab.frameUrlTextBoxClickState == 1) {
                        int caret = tb.CaretIndex;
                        string text = tb.Text;
                        if (string.IsNullOrEmpty(text)) return;
                        int start = caret;
                        int end = caret;
                        while (start > 0 && !char.IsWhiteSpace(text[start - 1]) && text[start - 1] != '.' && text[start - 1] != '/' && text[start - 1] != '?') start--;
                        while (end < text.Length && !char.IsWhiteSpace(text[end]) && text[end] != '.' && text[end] != '/' && text[end] != '?') end++;
                        tb.SelectionStart = start;
                        tb.SelectionLength = end - start;
                        e.Handled = true;
                    } else if (tab.frameUrlTextBoxClickState == 2) {
                        tb.SelectAll();
                        e.Handled = true;
                    }
                }
            };

            // Frame Toggle Bookmark Button
            var frame_ToggleBookmarkButton = new Button {
                Height = 15,
                Width = 15,
                Background = Brushes.White,
                BorderThickness = new Thickness(0),
                BorderBrush = Brushes.Transparent,
                ToolTip = "Bookmark"
            };
            frame_ToggleBookmarkButton.Content = BookmarkOffImage;
            Grid.SetColumn(frame_ToggleBookmarkButton, 3);
            frameAddress_Grid.Children.Add(frame_ToggleBookmarkButton);
            customTab.Frame_ToggleBookmarkButton = frame_ToggleBookmarkButton;
            frame_ToggleBookmarkButton.Tag = false; // false = off, true = on
            frame_ToggleBookmarkButton.Click += (s, e) => {
                if (s is Button btn) {
                    bool isOn = (bool)btn.Tag;
                    btn.Content = isOn ? BookmarkOffImage : BookmarkOnImage;
                    btn.Tag = !isOn;
                }
            };

            // Frame Clone Button
            var frame_CloneButton = new Button { Height = 15, Width = 15, FontSize = 9, FontWeight = FontWeights.Bold, Background = Brushes.White, BorderThickness = new Thickness(0), BorderBrush = Brushes.Transparent };
            frame_CloneButton.ToolTip = "Copy Tab";
            var cloneImage = new Image {
                Source = new BitmapImage(new Uri("pack://application:,,,/Clone.png")),
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            frame_CloneButton.Content = cloneImage;
            Grid.SetColumn(frame_CloneButton, 1);
            frameTitle_Grid.Children.Add(frame_CloneButton);
            customTab.Frame_CloneButton = frame_CloneButton;
            frame_CloneButton.Tag = customTab;
            frame_CloneButton.Click += (s, e) => CloneTab(customTab);

            // Frame Close Button
            var frame_CloseButton = new Button { Height = 15, Width = 15, FontSize = 9, FontWeight = FontWeights.Bold, Background = Brushes.White, BorderThickness = new Thickness(0), BorderBrush = Brushes.Transparent };
            frame_CloseButton.ToolTip = "Close";
            var closeImage = new Image {
                Source = new BitmapImage(new Uri("pack://application:,,,/Close.png")),
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            frame_CloseButton.Content = closeImage;
            Grid.SetColumn(frame_CloseButton, 3);
            frameTitle_Grid.Children.Add(frame_CloseButton);
            customTab.Frame_CloseButton = frame_CloseButton;
            frame_CloseButton.Tag = customTab;
            frame_CloseButton.Click += (s, e) => CloseTab(customTab);

            // Frame Hide Button
            var frame_HideButton = new Button { Height = 15, Width = 15, FontSize = 9, FontWeight = FontWeights.Bold, Background = Brushes.White, BorderThickness = new Thickness(0), BorderBrush = Brushes.Transparent };
            frame_HideButton.ToolTip = "Hide";
            var minimizeImage = new Image {
                Source = new BitmapImage(new Uri("pack://application:,,,/Minimize.png")),
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            frame_HideButton.Content = minimizeImage;
            Grid.SetColumn(frame_HideButton, 2);
            frameTitle_Grid.Children.Add(frame_HideButton);
            customTab.Frame_HideButton = frame_HideButton;
            frame_HideButton.Tag = customTab;
            frame_HideButton.Click += (s, e) => HideTabFrame(customTab);

            // Frame Title TextBox
            var frame_TitleTextBox = new TextBox {
                Foreground = Brushes.Black,
                FontWeight = FontWeights.SemiBold,
                Background = Brushes.White,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 0, 15, 0)
            };
            Grid.SetColumn(frame_TitleTextBox, 0);
            frameTitle_Grid.Children.Add(frame_TitleTextBox);
            customTab.Frame_TitleTextBox = frame_TitleTextBox;
            frame_TitleTextBox.Tag = customTab;
            frame_TitleTextBox.Text = nameString;
            frame_TitleTextBox.TextChanged += FrameTitleTextBox_TextChanged;

            // Frame WebView2
            WebView2 frame_WebView;
            if (webViewToClone != null)
            {
                // Create a new WebView2 and navigate to the same URL as the original
                frame_WebView = new WebView2();
                try
                {
                    // Use the current URL of the original WebView2 if available
                    var src = webViewToClone.Source?.ToString();
                    if (!string.IsNullOrEmpty(src))
                        frame_WebView.Source = new System.Uri(src);
                    else
                        frame_WebView.Source = new System.Uri(urlString.StartsWith("http") ? urlString : $"https://{urlString}");
                }
                catch { }
            }
            else
            {
                frame_WebView = new WebView2();
                try
                {
                    frame_WebView.Source = new System.Uri(urlString.StartsWith("http") ? urlString : $"https://{urlString}");
                }
                catch { }
            }
            Grid.SetRow(frame_WebView, 2);
            frame_Grid.Children.Add(frame_WebView);
            customTab.Frame_WebView = frame_WebView;
            frame_WebView.Tag = customTab;
            frame_WebView.NavigationCompleted += FrameWebView_NavigationCompleted;
            frame_WebView.CoreWebView2InitializationCompleted += FrameWebView_CoreWebView2InitializationCompleted;

            if (urlString == nameString && webViewToClone == null)
            {
                _ = UpdateTabTitleFromUrlAsync(customTab, urlString);
            }

            customTab.displayFrame = true;
            OrganizeFrames();
        }

        private void CreateTab(string urlString, string nameString)
        {
            CreateTabInternal(urlString, nameString, null);
        }

        public void CloneTab(CustomTab customTab)
        {
            if (customTab == null || customTab.Frame_WebView == null) return;
            CreateTabInternal(customTab.Frame_UrlTextBox?.Text ?? "", customTab.Frame_TitleTextBox?.Text ?? "", customTab.Frame_WebView);
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
                // Bookmark toggle logic
                if (customTab.Frame_UrlTextBox != null && customTab.Frame_ToggleBookmarkButton != null && BookmarksList != null)
                {
                    string urlText = customTab.Frame_UrlTextBox.Text?.Trim();
                    bool found = BookmarksList.Any(b => string.Equals(b.URL?.Trim(), urlText, StringComparison.OrdinalIgnoreCase));
                    customTab.Frame_ToggleBookmarkButton.Content = found ? BookmarkOnImage : BookmarkOffImage;
                    customTab.Frame_ToggleBookmarkButton.Tag = found;
                }
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
            int needed = (int)(System.Math.Ceiling((double)frameTabs.Count / System.Math.Ceiling((double)rowCount/2)) * 2) - 1;
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
                for (int col = 0; col < FramesGrid.ColumnDefinitions.Count; col++)
                {
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

        private async void TxtLaunchUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string url = txtLaunchUrl.Text.Trim();
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
                        CreateTab(searchUrl, url);
                        txtLaunchUrl.Text = string.Empty;
                        return;
                    }

                    bool loaded = false;
                    try
                    {
                        // Try to create a tab and check if navigation is successful
                        var customTab = new CustomTab();
                        CreateTab(url, url);
                        var createdTab = TabsList.LastOrDefault();
                        if (createdTab != null && createdTab.Frame_WebView != null)
                        {
                            var webView = createdTab.Frame_WebView;
                            // Wait for navigation to complete or fail
                            var tcs = new TaskCompletionSource<bool>();
                            void handler(object s, CoreWebView2NavigationCompletedEventArgs args)
                            {
                                tcs.TrySetResult(args.IsSuccess);
                                webView.NavigationCompleted -= handler;
                            }
                            webView.NavigationCompleted += handler;
                            // Try to navigate
                            try
                            {
                                webView.Source = new System.Uri(url.StartsWith("http") ? url : $"https://{url}");
                            }
                            catch { tcs.TrySetResult(false); }
                            loaded = await tcs.Task;
                        }
                    }
                    catch { loaded = false; }

                    if (!loaded)
                    {
                        // Remove the failed tab if it was added
                        var failedTab = TabsList.LastOrDefault();
                        if (failedTab != null && failedTab.Frame_UrlTextBox?.Text == url)
                        {
                            CloseTab(failedTab);
                        }
                        // Use Google search
                        string searchUrl = $"https://www.google.com/search?q={System.Net.WebUtility.UrlEncode(url)}";
                        CreateTab(searchUrl, url);
                    }
                }
                txtLaunchUrl.Text = string.Empty;
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
            var bookmarksToSave = BookmarksList.Select(b => new { Title = b.TitleLabel.Content?.ToString() ?? string.Empty, URL = b.URL }).ToList();
            var json = JsonSerializer.Serialize(bookmarksToSave, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("bookmarks.json", json);

            // Save open tabs to JSON
            var openTabsToSave = TabsList.Select(t => new { Title = t.Frame_TitleTextBox?.Text ?? string.Empty, URL = t.Frame_UrlTextBox?.Text ?? string.Empty, DisplayFrame = t.displayFrame }).ToList();
            var openTabsJson = JsonSerializer.Serialize(openTabsToSave, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("opentabs.json", openTabsJson);

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

        private async void FrameUrlTextBox_KeyDown(object sender, KeyEventArgs e)
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

        private void FrameTitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is CustomTab customTab && customTab.Tab_TitleLabel != null)
            {
                if (customTab.Tab_TitleLabel.Content is TextBlock tb) tb.Text = textBox.Text;
            }
        }

        private class BookmarkJsonEntry
        {
            public string Title { get; set; }
            public string URL { get; set; }
        }

        private class OpenTabJsonEntry
        {
            public string Title { get; set; }
            public string URL { get; set; }
            public bool DisplayFrame { get; set; } = true;
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
                    if (value)
                    {
                        Tab_TitleLabel.BorderBrush = Brushes.Gray;
                        Tab_TitleLabel.BorderThickness = new Thickness(0, 1, 1, 1);
                        Tab_TitleLabel.Background = Brushes.White;
                        TabTitleLabelBackground = Brushes.White;
                    }
                    else
                    {
                        Tab_TitleLabel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDBDBDB"));
                        Tab_TitleLabel.BorderThickness = new Thickness(1, 1, 1, 1);
                        Tab_TitleLabel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EFF3FB"));
                        TabTitleLabelBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EFF3FB"));
                    }
                    // Set font weight and color on the TextBlock inside the Label
                    var darkGray = new SolidColorBrush(Color.FromRgb(40, 40, 40)); // almost black
                    if (Tab_TitleLabel.Content is TextBlock tb)
                    {
                        tb.FontWeight = value ? FontWeights.SemiBold : FontWeights.Normal;
                        tb.Foreground = value ? Brushes.Black : darkGray;
                    }
                    else
                    {
                        Tab_TitleLabel.FontWeight = value ? FontWeights.SemiBold : FontWeights.Normal;
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