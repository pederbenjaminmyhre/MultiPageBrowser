using System.Windows;
using System.Windows.Controls;

namespace BrowserTabManager
{
    public static class BookmarkContextMenuHelper
    {
        public static void AttachContextMenu(MainWindow mainWindow, CustomBookmark customBookmark)
        {
            var contextMenu = new ContextMenu();

            var addMenuItem = new MenuItem { Header = "Add" };
            addMenuItem.Click += (s, e) => mainWindow.AddBookmarkContext(customBookmark);

            var editMenuItem = new MenuItem { Header = "Edit" };
            editMenuItem.Click += (s, e) => mainWindow.EditBookmarkContext(customBookmark);

            var deleteMenuItem = new MenuItem { Header = "Delete" };
            deleteMenuItem.Click += (s, e) => mainWindow.DeleteBookmarkContext(customBookmark);

            contextMenu.Items.Add(addMenuItem);
            contextMenu.Items.Add(editMenuItem);
            contextMenu.Items.Add(deleteMenuItem);

            customBookmark.TitleLabel.ContextMenu = contextMenu;
        }
    }
}
