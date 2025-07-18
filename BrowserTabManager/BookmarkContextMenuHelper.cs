using System.Windows;
using System.Windows.Controls;

namespace BrowserTabManager
{
    public static class BookmarkContextMenuHelper
    {
        public static void AttachContextMenu(MainWindow mainWindow, CustomBookmark customBookmark)
        {
            var contextMenu = new ContextMenu();

            // Launch menu item
            var launchMenuItem = new MenuItem { Header = $"Browse to {customBookmark.TitleLabel.Content}" };
            launchMenuItem.Click += (s, e) =>
            {
                CustomTab customTab = mainWindow.tabHelper.CreateTab(customBookmark.URL, customBookmark.TitleLabel.Content?.ToString() ?? string.Empty);
                mainWindow.CurrentScreen.TabList.Add(customTab);
            };
            contextMenu.Items.Add(launchMenuItem);

            // Launch in New Screen menu item
            var launchNewScreenMenuItem = new MenuItem { Header = $"Browse to {customBookmark.TitleLabel.Content} in a new screen" };
            launchNewScreenMenuItem.Click += (s, e) => {
                foreach (var tab in mainWindow.TabsList)
                {
                    tab.displayFrame = false;
                }
                
                CustomTab customTab = mainWindow.tabHelper.CreateTab(customBookmark.URL, customBookmark.TitleLabel.Content?.ToString() ?? string.Empty);
                CustomScreen customScreen = mainWindow.CreateScreen();
                mainWindow.CurrentScreen.TabList.Add(customTab);
            };
            contextMenu.Items.Add(launchNewScreenMenuItem);

            var editMenuItem = new MenuItem { Header = $"Edit {customBookmark.TitleLabel.Content}" };
            editMenuItem.Click += (s, e) => mainWindow.bookmarkHelper.EditBookmarkContext(customBookmark);

            var deleteMenuItem = new MenuItem { Header = $"Delete {customBookmark.TitleLabel.Content}" };
            deleteMenuItem.Click += (s, e) => mainWindow.bookmarkHelper.DeleteBookmarkContext(customBookmark);

            var addMenuItem = new MenuItem { Header = "Create a New Bookmark" };
            addMenuItem.Click += (s, e) => mainWindow.bookmarkHelper.AddBookmarkContext(customBookmark);

            
            contextMenu.Items.Add(editMenuItem);
            contextMenu.Items.Add(deleteMenuItem);
            contextMenu.Items.Add(addMenuItem);

            customBookmark.TitleLabel.ContextMenu = contextMenu;
        }
    }
}
