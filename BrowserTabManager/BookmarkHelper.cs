using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace BrowserTabManager
{
    public class BookmarkHelper
    {
        private MainWindow mainWindow;

        public BookmarkHelper(MainWindow mainWindow) { 
            this.mainWindow = mainWindow;

        }
        internal void CreateBookmark(string urlString, string nameString)
        {
            // Uncollapse the BookmarkScroll to make bookmarks visible
            mainWindow.BookmarkScroll.Visibility = Visibility.Visible;

            // Create CustomBookmark
            var customBookmark = new CustomBookmark();
            mainWindow.BookmarksList.Add(customBookmark);

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
            mainWindow.BookmarkStack.Children.Add(bookmarkBorder);

            // Attach context menu for right-click
            BookmarkContextMenuHelper.AttachContextMenu(this.mainWindow, customBookmark);

            // Attach click event to the bookmark label
            bookmarkNameLabel.MouseLeftButtonUp += mainWindow.bookmarkHelper.BookmarkNameLabel_Click;

            // If urlString == nameString, fetch the page title and update label
            if (urlString == nameString)
            {
                _ = mainWindow.bookmarkHelper.UpdateBookmarkTitleFromUrlAsync(customBookmark, urlString);
            }
        }

        public void AddBookmarkContext(CustomBookmark bookmark)
        {
            // Show AddBookmarkDialog
            var dialog = new AddBookmarkDialog { Owner = this.mainWindow };
            if (dialog.ShowDialog() == true)
            {
                // Use dialog input to create the new bookmark
                this.CreateBookmark(dialog.BookmarkUrl, dialog.BookmarkTitle);
                // Move the new bookmark visually below the current one
                int idx = mainWindow.BookmarksList.IndexOf(bookmark);
                var newBookmark = mainWindow.BookmarksList.Last();
                mainWindow.BookmarkStack.Children.Remove(newBookmark.Border);
                int insertIdx = idx >= 0 ? idx + 1 : mainWindow.BookmarkStack.Children.Count;
                mainWindow.BookmarkStack.Children.Insert(insertIdx, newBookmark.Border);
            }
        }

        public void EditBookmarkContext(CustomBookmark bookmark)
        {
            // Example: Prompt for new title and URL
            var inputDialog = new EditBookmarkDialog(bookmark.TitleLabel.Content?.ToString() ?? "", bookmark.URL);
            if (inputDialog.ShowDialog() == true)
            {
                bookmark.TitleLabel.Content = inputDialog.BookmarkTitle;
                bookmark.URL = inputDialog.BookmarkUrl;
            }
        }

        public void DeleteBookmarkContext(CustomBookmark bookmark)
        {
            var dialog = new DeleteBookmarkDialog(bookmark.TitleLabel.Content?.ToString() ?? "");
            dialog.Owner = this.mainWindow;
            if (dialog.ShowDialog() == true && dialog.IsConfirmed)
            {
                mainWindow.BookmarksList.Remove(bookmark);
                mainWindow.BookmarkStack.Children.Remove(bookmark.Border);
            }
        }

        public async Task UpdateBookmarkTitleFromUrlAsync(CustomBookmark bookmark, string url)
        {
            var title = await mainWindow.FetchPageTitleAsync(url);
            if (!string.IsNullOrWhiteSpace(title))
            {
                // Update on UI thread
                mainWindow.Dispatcher.Invoke(() => bookmark.TitleLabel.Content = title);
            }
        }

        public void BookmarkNameLabel_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Label label && label.Tag is CustomBookmark customBookmark)
            {
                CustomTab customTab = mainWindow.tabHelper.CreateTab(customBookmark.URL, label.Content?.ToString() ?? string.Empty);
                mainWindow.CurrentScreen.TabList.Add(customTab);
            }
        }

        public void TxtSearchBookmarks_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = mainWindow.txtSearchBookmarks.Text.Trim().ToLower();
            mainWindow.BookmarkStack.Children.Clear();
            var filtered = new List<CustomBookmark>();
            foreach (var bookmark in mainWindow.BookmarksList)
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
                mainWindow.BookmarkStack.Children.Add(bookmark.Border);
            }
        }
    }
}
