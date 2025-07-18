using System.Collections.Generic;

namespace BrowserTabManager
{
    public class ScreenJsonEntry
    {
        public string ScreenName { get; set; }
        public List<OpenTabJsonEntry> TabList { get; set; }
    }
}