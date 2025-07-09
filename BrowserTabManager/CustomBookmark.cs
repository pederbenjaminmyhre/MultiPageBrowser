using System.Windows.Controls;

namespace BrowserTabManager
{
    public class CustomBookmark
    {
        public Border Border { get; set; }
        public Grid Grid { get; set; }
        public Label TitleLabel { get; set; } // Changed from TextBox to Label
        public string URL { get; set; }
    }
}
