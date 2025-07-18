using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace BrowserTabManager
{
    public static class TabContextMenuHelper
    {
        public static void AttachContextMenu(MainWindow mainWindow, CustomTab customTab)
        {
            var contextMenu = new ContextMenu();

            // Edit
            var editMenuItem = new MenuItem { Header = "Edit" };
            editMenuItem.Click += (s, e) =>
            {
                var dialog = new EditBookmarkDialog(customTab.Frame_TitleTextBox.Text, customTab.Frame_UrlTextBox.Text)
                {
                    Owner = mainWindow
                };
                if (dialog.ShowDialog() == true)
                {
                    customTab.Frame_TitleTextBox.Text = dialog.BookmarkTitle;
                    customTab.Frame_UrlTextBox.Text = dialog.BookmarkUrl;
                }
            };
            contextMenu.Items.Add(editMenuItem);

            // Hide/Show
            var hideShowMenuItem = new MenuItem();
            void UpdateHideShowHeader() => hideShowMenuItem.Header = customTab.displayFrame ? "Hide" : "Show";
            UpdateHideShowHeader();
            hideShowMenuItem.Click += (s, e) =>
            {
                customTab.displayFrame = !customTab.displayFrame;
                mainWindow.frameHelper.OrganizeFrames();
                UpdateHideShowHeader();
            };
            contextMenu.Items.Add(hideShowMenuItem);

            // Close
            var closeMenuItem = new MenuItem { Header = "Close" };
            closeMenuItem.Click += (s, e) => mainWindow.tabHelper.CloseTab(customTab);
            contextMenu.Items.Add(closeMenuItem);

            // Bookmark
            var bookmarkMenuItem = new MenuItem { Header = "Bookmark" };
            bookmarkMenuItem.Click += (s, e) =>
            {
                string url = customTab.Frame_UrlTextBox.Text;
                if (!mainWindow.BookmarksList.Any(b => string.Equals(b.URL?.Trim(), url?.Trim(), System.StringComparison.OrdinalIgnoreCase)))
                {
                    mainWindow.bookmarkHelper.CreateBookmark(url, customTab.Frame_TitleTextBox.Text);
                }
            };
            contextMenu.Items.Add(bookmarkMenuItem);

            customTab.Tab_Border.ContextMenu = contextMenu;
        }
    }
}
