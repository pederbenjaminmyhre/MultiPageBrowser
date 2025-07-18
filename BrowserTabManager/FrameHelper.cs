using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BrowserTabManager
{
    public class FrameHelper
    {
        private readonly MainWindow _mainWindow;

        public FrameHelper(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
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


                if (customTab.Frame_UrlTextBox != null && customTab.Frame_ToggleBookmarkButton != null && this._mainWindow.BookmarksList != null)
                {
                    string urlText = customTab.Frame_UrlTextBox.Text?.Trim();
                    bool found = this._mainWindow.BookmarksList.Any(b => string.Equals(b.URL?.Trim(), urlText, StringComparison.OrdinalIgnoreCase));
                    var offImg = CreateBookmarkOffImage();
                    var onImg = CreateBookmarkOnImage();
                    customTab.Frame_ToggleBookmarkButton.Content = found ? onImg : offImg;
                    customTab.Frame_ToggleBookmarkButton.Tag = found;
                }
            }
        }

        public static Image CreateBookmarkOffImage()
        {
            return new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/Icons/BookmarkOff.png")),
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
        }
        public static Image CreateBookmarkOnImage()
        {
            return new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/Icons/BookmarkOn.png")),
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
        }

        public void OrganizeFrames()
        {
            // Use only CustomTabs with displayFrame == true
            var frameTabs = _mainWindow.TabsList.Where(t => t.displayFrame).ToList();

            // Ensure FramesGrid has enough columns using the new logic
            int rowCount = _mainWindow.FramesGrid.RowDefinitions.Count > 0 ? _mainWindow.FramesGrid.RowDefinitions.Count : 1;
            int needed = (int)(System.Math.Ceiling((double)frameTabs.Count / System.Math.Ceiling((double)rowCount / 2)) * 2) - 1;
            while (_mainWindow.FramesGrid.ColumnDefinitions.Count < needed)
            {
                int newColIndex = _mainWindow.FramesGrid.ColumnDefinitions.Count;
                if (newColIndex % 2 == 0)
                {
                    // Even index: star size, empty
                    var colDef = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
                    _mainWindow.FramesGrid.ColumnDefinitions.Add(colDef);
                }
                else
                {
                    // Odd index: width 4
                    var colDef = new ColumnDefinition { Width = new GridLength(4) };
                    _mainWindow.FramesGrid.ColumnDefinitions.Add(colDef);
                }
            }
            while (_mainWindow.FramesGrid.ColumnDefinitions.Count > needed && _mainWindow.FramesGrid.ColumnDefinitions.Count > 1)
            {
                _mainWindow.FramesGrid.ColumnDefinitions.RemoveAt(_mainWindow.FramesGrid.ColumnDefinitions.Count - 1);
            }

            // Remove all children
            _mainWindow.FramesGrid.Children.Clear();

            // Place frames and splitters
            int frameIndex = 0;
            for (int row = 0; row < _mainWindow.FramesGrid.RowDefinitions.Count; row++)
            {
                // if row is even, change row height to star size
                if (row % 2 == 0)
                {
                    _mainWindow.FramesGrid.RowDefinitions[row].Height = new GridLength(1, GridUnitType.Star);
                }
                //else
                //{
                //    _mainWindow.FramesGrid.RowDefinitions[row].Height = new GridLength(4); // Splitter row
                //}


                for (int col = 0; col < _mainWindow.FramesGrid.ColumnDefinitions.Count; col++)
                {
                    // if col is even, change column width to star size
                    if (col % 2 == 0)
                    {
                        _mainWindow.FramesGrid.ColumnDefinitions[col].Width = new GridLength(1, GridUnitType.Star);
                    }

                    if (row % 2 == 0 && col % 2 == 0)
                    {
                        // Frame cell
                        if (frameIndex < frameTabs.Count)
                        {
                            var tab = frameTabs[frameIndex];
                            bool alreadyPresent = false;
                            foreach (UIElement child in _mainWindow.FramesGrid.Children)
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
                                var toRemove = _mainWindow.FramesGrid.Children.Cast<UIElement>().FirstOrDefault(child => Grid.GetRow(child) == row && Grid.GetColumn(child) == col);
                                if (toRemove != null)
                                    _mainWindow.FramesGrid.Children.Remove(toRemove);
                                Grid.SetRow(tab.Frame_Border, row);
                                Grid.SetColumn(tab.Frame_Border, col);
                                _mainWindow.FramesGrid.Children.Add(tab.Frame_Border);
                            }
                            frameIndex++;
                        }
                    }
                    else if (row % 2 == 0 && col % 2 == 1)
                    {
                        // Vertical splitter
                        var existing = _mainWindow.FramesGrid.Children.Cast<UIElement>().FirstOrDefault(child => Grid.GetRow(child) == row && Grid.GetColumn(child) == col && child is GridSplitter && ((GridSplitter)child).ResizeDirection == GridResizeDirection.Columns);
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
                            _mainWindow.FramesGrid.Children.Add(splitter);
                        }
                    }
                    else if (row % 2 == 1 && col % 2 == 0)
                    {
                        // Horizontal splitter
                        var existing = _mainWindow.FramesGrid.Children.Cast<UIElement>().FirstOrDefault(child => Grid.GetRow(child) == row && Grid.GetColumn(child) == col && child is GridSplitter && ((GridSplitter)child).ResizeDirection == GridResizeDirection.Rows);
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
                            _mainWindow.FramesGrid.Children.Add(splitter);
                        }
                    }
                    // else (row % 2 == 1 && col % 2 == 1): leave empty
                }
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

        internal void HideTabFrame(CustomTab customTab)
        {
            customTab.displayFrame = false;
            this._mainWindow.FramesGrid.Children.Remove(customTab.Frame_Border);
            // Remove columns from right to left until FramesGrid.ColumnDefinitions.Count < needed
            int rowCount = this._mainWindow.FramesGrid.RowDefinitions.Count > 0 ? this._mainWindow.FramesGrid.RowDefinitions.Count : 1;
            int needed = (int)(System.Math.Ceiling((double)this._mainWindow.TabsList.Count / rowCount) * 2) - 1;
            while (this._mainWindow.FramesGrid.ColumnDefinitions.Count > needed && this._mainWindow.FramesGrid.ColumnDefinitions.Count > 1)
            {
                this._mainWindow.FramesGrid.ColumnDefinitions.RemoveAt(this._mainWindow.FramesGrid.ColumnDefinitions.Count - 1);
            }
            this.OrganizeFrames();
        }

        internal void FrameWebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (sender is WebView2 webView && webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.NewWindowRequested += this._mainWindow.CoreWebView2_NewWindowRequested;
            }
        }

        public void btnAddRowToFramesGrid_Click(object sender, RoutedEventArgs e)
        {
            // Add a 4px high row for the splitter
            var splitterRow = new RowDefinition { Height = new GridLength(4) };
            this._mainWindow.FramesGrid.RowDefinitions.Add(splitterRow);
            // Add a new star-sized row for content
            this._mainWindow.FramesGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            // Update the current screen's row count
            this._mainWindow.CurrentScreen.RowCount = this._mainWindow.FramesGrid.RowDefinitions.Count;
            OrganizeFrames();
        }

        public void btnSubtractRowFromFramesGrid_Click(object sender, RoutedEventArgs e)
        {
            // Remove the last two rows if there are at least two rows
            if (this._mainWindow.FramesGrid.RowDefinitions.Count >= 2)
            {
                this._mainWindow.FramesGrid.RowDefinitions.RemoveAt(this._mainWindow.FramesGrid.RowDefinitions.Count - 1);
                this._mainWindow.FramesGrid.RowDefinitions.RemoveAt(this._mainWindow.FramesGrid.RowDefinitions.Count - 1);
                // Update the current screen's row count
                this._mainWindow.CurrentScreen.RowCount = this._mainWindow.FramesGrid.RowDefinitions.Count;
                OrganizeFrames();
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
    }
}
