using System.Collections.Generic;

namespace BrowserTabManager
{
    public class CustomScreen
    {
        public string ScreenName { get; set; }
        public List<CustomTab> TabList { get; set; } = new List<CustomTab>();

        public CustomScreen(TabHelper tabHelper)
        {
            var newTab = tabHelper.CreateTab("https://www.google.com", "Google");
            TabList.Add(newTab);
        }
    }
}
