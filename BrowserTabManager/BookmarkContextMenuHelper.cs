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
            var launchMenuItem = new MenuItem { Header = "Launch" };
            launchMenuItem.Click += (s, e) => TabHelper.CreateTab(mainWindow, customBookmark.URL, customBookmark.TitleLabel.Content?.ToString() ?? string.Empty);
            contextMenu.Items.Add(launchMenuItem);

            // Launch in New Screen menu item
            var launchNewScreenMenuItem = new MenuItem { Header = "Launch in New Screen" };
            launchNewScreenMenuItem.Click += (s, e) => {
                foreach (var tab in mainWindow.TabsList)
                {
                    tab.displayFrame = false;
                }
                mainWindow.OrganizeFrames();
                TabHelper.CreateTab(mainWindow, customBookmark.URL, customBookmark.TitleLabel.Content?.ToString() ?? string.Empty);
            };
            contextMenu.Items.Add(launchNewScreenMenuItem);

            var editMenuItem = new MenuItem { Header = "Edit" };
            editMenuItem.Click += (s, e) => mainWindow.EditBookmarkContext(customBookmark);

            var deleteMenuItem = new MenuItem { Header = "Delete" };
            deleteMenuItem.Click += (s, e) => mainWindow.DeleteBookmarkContext(customBookmark);

            var addMenuItem = new MenuItem { Header = "Create Bookmark" };
            addMenuItem.Click += (s, e) => mainWindow.AddBookmarkContext(customBookmark);

            
            contextMenu.Items.Add(editMenuItem);
            contextMenu.Items.Add(deleteMenuItem);
            contextMenu.Items.Add(addMenuItem);

            customBookmark.TitleLabel.ContextMenu = contextMenu;
        }
    }
}
