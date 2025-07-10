using System.Windows;
using System.Windows.Controls;

namespace BrowserTabManager
{
    public class NewTabDialog : Window
    {
        public TextBox UrlBox { get; private set; }
        public NewTabDialog(TextBox launchUrlBox)
        {
            Title = "Open New Tab";
            Width = 400;
            Height = 120;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.ToolWindow;

            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            UrlBox = new TextBox
            {
                Height = 25,
                Margin = new Thickness(0, 0, 0, 10),
                Text = launchUrlBox.Text,
                Style = launchUrlBox.Style,
                Effect = launchUrlBox.Effect,
                MaxWidth = launchUrlBox.MaxWidth
            };
            Grid.SetRow(UrlBox, 0);
            grid.Children.Add(UrlBox);

            Content = grid;
        }
    }
}
