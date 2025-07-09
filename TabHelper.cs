using System.Windows.Controls;
using Microsoft.Web.WebView2.Wpf;
using System.Windows.Media;
using System.Windows;
using System.Linq;

namespace BrowserTabManager
{
    public static class TabHelper
    {
        public static void CreateTab(MainWindow mainWindow, string urlString, string nameString)
        {
            CreateTabInternal(mainWindow, urlString, nameString, null);
        }

        public static void CreateTabInternal(MainWindow mainWindow, string urlString, string nameString, WebView2 webViewToClone = null)
        {
            var customTab = new CustomTab();
            mainWindow.TabsList.Add(customTab);

            // Tab Border
            var tab_Border = new Border();
            tab_Border.Background = Brushes.Transparent;
            mainWindow.TabStack.Children.Add(tab_Border);
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
            tab_TitleLabel.MouseLeftButtonUp += mainWindow.TabTitleLabel_Click;
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
                Background = (Brush)new BrushConverter().ConvertFromString("#E2E6EC")
            };
            customTab.Frame_Border = frame_Border;
            frame_Border.Tag = customTab;

            // Frame Grid
            var frame_Grid = new Grid {
                Background = (Brush)new BrushConverter().ConvertFromString("#E2E6EC")
            };
            frame_Border.Child = frame_Grid;
            customTab.Frame_Grid = frame_Grid;
            frame_Grid.Tag = customTab;
            frame_Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            frame_Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            frame_Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Frame Title Grid
            var frameTitle_Grid = new Grid {
                Background = (Brush)new BrushConverter().ConvertFromString("#E2E6EC")
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
            var frameAddress_Grid = new Grid(Background = (Brush)new BrushConverter().ConvertFromString("#E2E6EC"));
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
                Source = new BitmapImage(new System.Uri("pack://application:,,,/Refresh.png")),
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
            var frame_BackButton = new Button { Height = 15, Width = 15, FontSize = 9, FontWeight = FontWeights.Bold, Background = Brushes.Transparent, BorderThickness = new Thickness(0), BorderBrush = Brushes.Transparent };
            frame_BackButton.ToolTip = "Back";
            var backImage = new Image {
                Source = new BitmapImage(new System.Uri("pack://application:,,,/Back.png")),
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            frame_BackButton.Content = backImage;
            Grid.SetColumn(frame_BackButton, 1);
            frameAddress_Grid.Children.Add(frame_BackButton);
            customTab.Frame_BackButton = frame_BackButton;
            frame_BackButton.Tag = customTab;
            frame_BackButton.Click += mainWindow.FrameBackButton_Click;

            // Frame Url TextBox
            var frame_UrlTextBox = new TextBox();
            Grid.SetColumn(frame_UrlTextBox, 2);
            frameAddress_Grid.Children.Add(frame_UrlTextBox);
            customTab.Frame_UrlTextBox = frame_UrlTextBox;
            frame_UrlTextBox.Tag = customTab;
            frame_UrlTextBox.Text = urlString;
            frame_UrlTextBox.KeyDown += mainWindow.FrameUrlTextBox_KeyDown;
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
            frame_ToggleBookmarkButton.Content = mainWindow.GetType().GetField("BookmarkOffImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);
            Grid.SetColumn(frame_ToggleBookmarkButton, 3);
            frameAddress_Grid.Children.Add(frame_ToggleBookmarkButton);
            customTab.Frame_ToggleBookmarkButton = frame_ToggleBookmarkButton;
            frame_ToggleBookmarkButton.Tag = false; // false = off, true = on
            frame_ToggleBookmarkButton.Click += (s, e) => {
                if (s is Button btn) {
                    bool isOn = (bool)btn.Tag;
                    var offImg = mainWindow.GetType().GetField("BookmarkOffImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);
                    var onImg = mainWindow.GetType().GetField("BookmarkOnImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);
                    btn.Content = isOn ? offImg : onImg;
                    btn.Tag = !isOn;
                }
            };

            // Frame Clone Button
            var frame_CloneButton = new Button { Height = 15, Width = 15, FontSize = 9, FontWeight = FontWeights.Bold, Background = Brushes.White, BorderThickness = new Thickness(0), BorderBrush = Brushes.Transparent };
            frame_CloneButton.ToolTip = "Copy Tab";
            var cloneImage = new Image {
                Source = new BitmapImage(new System.Uri("pack://application:,,,/Clone.png")),
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            frame_CloneButton.Content = cloneImage;
            Grid.SetColumn(frame_CloneButton, 1);
            frameTitle_Grid.Children.Add(frame_CloneButton);
            customTab.Frame_CloneButton = frame_CloneButton;
            frame_CloneButton.Tag = customTab;
            frame_CloneButton.Click += (s, e) => mainWindow.CloneTab(customTab);

            // Frame Close Button
            var frame_CloseButton = new Button { Height = 15, Width = 15, FontSize = 9, FontWeight = FontWeights.Bold, Background = Brushes.White, BorderThickness = new Thickness(0), BorderBrush = Brushes.Transparent };
            frame_CloseButton.ToolTip = "Close";
            var closeImage = new Image {
                Source = new BitmapImage(new System.Uri("pack://application:,,,/Close.png")),
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            frame_CloseButton.Content = closeImage;
            Grid.SetColumn(frame_CloseButton, 3);
            frameTitle_Grid.Children.Add(frame_CloseButton);
            customTab.Frame_CloseButton = frame_CloseButton;
            frame_CloseButton.Tag = customTab;
            frame_CloseButton.Click += (s, e) => mainWindow.CloseTab(customTab);

            // Frame Hide Button
            var frame_HideButton = new Button { Height = 15, Width = 15, FontSize = 9, FontWeight = FontWeights.Bold, Background = (Brush)new BrushConverter().ConvertFromString("#E2E6EC"), BorderThickness = new Thickness(0), BorderBrush = Brushes.Transparent };
            frame_HideButton.ToolTip = "Hide";
            // Custom style to keep background #E2E6EC in all states
            var hideButtonStyle = new Style(typeof(Button));
            hideButtonStyle.Setters.Add(new Setter(Button.BackgroundProperty, new BrushConverter().ConvertFromString("#E2E6EC")));
            hideButtonStyle.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.Transparent));
            hideButtonStyle.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(0)));
            frame_HideButton.Style = hideButtonStyle;
            var minimizeImage = new Image {
                Source = new BitmapImage(new System.Uri("pack://application:,,,/Icons/Minimize.png")),
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            frame_HideButton.Content = minimizeImage;
            Grid.SetColumn(frame_HideButton, 2);
            frameTitle_Grid.Children.Add(frame_HideButton);
            customTab.Frame_HideButton = frame_HideButton;
            frame_HideButton.Tag = customTab;
            frame_HideButton.Click += (s, e) => mainWindow.HideTabFrame(customTab);

            // Frame Title TextBox
            var frame_TitleTextBox = new TextBox {
                Foreground = Brushes.Black,
                FontWeight = FontWeights.SemiBold,
                Background = (Brush)new BrushConverter().ConvertFromString("#E2E6EC"),
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 0, 15, 0)
            };
            Grid.SetColumn(frame_TitleTextBox, 0);
            frameTitle_Grid.Children.Add(frame_TitleTextBox);
            customTab.Frame_TitleTextBox = frame_TitleTextBox;
            frame_TitleTextBox.Tag = customTab;
            frame_TitleTextBox.Text = nameString;
            frame_TitleTextBox.TextChanged += mainWindow.FrameTitleTextBox_TextChanged;

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
            frame_WebView.NavigationCompleted += mainWindow.FrameWebView_NavigationCompleted;
            frame_WebView.CoreWebView2InitializationCompleted += mainWindow.FrameWebView_CoreWebView2InitializationCompleted;

            if (urlString == nameString && webViewToClone == null)
            {
                _ = mainWindow.UpdateTabTitleFromUrlAsync(customTab, urlString);
            }

            customTab.displayFrame = true;
            mainWindow.OrganizeFrames();
        }
    }
}
