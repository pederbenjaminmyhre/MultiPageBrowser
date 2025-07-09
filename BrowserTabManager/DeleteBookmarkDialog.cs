using System.Windows;
using System.Windows.Controls;

namespace BrowserTabManager
{
    public class DeleteBookmarkDialog : Window
    {
        public bool IsConfirmed { get; private set; }

        public DeleteBookmarkDialog(string bookmarkTitle)
        {
            Title = "Delete Bookmark";
            Width = 350;
            Height = 140;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.ToolWindow;

            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Message
            var message = new TextBlock
            {
                Text = $"Are you sure you want to delete the bookmark '{bookmarkTitle}'?",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(message, 0);
            grid.Children.Add(message);

            // Buttons
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var yesButton = new Button { Content = "Yes", Width = 70, Margin = new Thickness(0, 0, 10, 0), IsDefault = true };
            var cancelButton = new Button { Content = "Cancel", Width = 70, IsCancel = true };
            yesButton.Click += (s, e) => { IsConfirmed = true; DialogResult = true; Close(); };
            cancelButton.Click += (s, e) => { IsConfirmed = false; DialogResult = false; Close(); };
            buttonPanel.Children.Add(yesButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 1);
            grid.Children.Add(buttonPanel);

            Content = grid;
        }
    }
}
