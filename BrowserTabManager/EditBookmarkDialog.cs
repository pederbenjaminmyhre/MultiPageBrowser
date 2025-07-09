using System.Windows;
using System.Windows.Controls;

namespace BrowserTabManager
{
    public class EditBookmarkDialog : Window
    {
        public string BookmarkTitle { get; private set; }
        public string BookmarkUrl { get; private set; }

        private TextBox titleBox;
        private TextBox urlBox;
        private Button okButton;
        private Button cancelButton;

        public EditBookmarkDialog(string currentTitle, string currentUrl)
        {
            Title = "Edit Bookmark";
            Width = 350;
            Height = 180;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.ToolWindow;

            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Title
            var titleLabel = new Label { Content = "Title:", VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(titleLabel, 0);
            Grid.SetColumn(titleLabel, 0);
            grid.Children.Add(titleLabel);
            titleBox = new TextBox { Text = currentTitle };
            Grid.SetRow(titleBox, 0);
            Grid.SetColumn(titleBox, 1);
            grid.Children.Add(titleBox);

            // URL
            var urlLabel = new Label { Content = "URL:", VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(urlLabel, 1);
            Grid.SetColumn(urlLabel, 0);
            grid.Children.Add(urlLabel);
            urlBox = new TextBox { Text = currentUrl };
            Grid.SetRow(urlBox, 1);
            Grid.SetColumn(urlBox, 1);
            grid.Children.Add(urlBox);

            // Buttons
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            okButton = new Button { Content = "OK", Width = 70, Margin = new Thickness(0, 10, 10, 0), IsDefault = true };
            okButton.Click += OkButton_Click;
            cancelButton = new Button { Content = "Cancel", Width = 70, Margin = new Thickness(0, 10, 0, 0), IsCancel = true };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 3);
            Grid.SetColumnSpan(buttonPanel, 2);
            grid.Children.Add(buttonPanel);

            Content = grid;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            BookmarkTitle = titleBox.Text.Trim();
            BookmarkUrl = urlBox.Text.Trim();
            DialogResult = true;
            Close();
        }
    }
}
