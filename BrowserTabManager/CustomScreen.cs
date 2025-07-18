using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BrowserTabManager
{
    public class CustomScreen
    {
        public ObservableCollection<CustomTab> TabList { get; set; }
        public string ScreenName { get; set; }
        public int RowCount { get; set; } = 1;
        private TabHelper _tabHelper;

        public CustomScreen(TabHelper tabHelper)
        {
            _tabHelper = tabHelper;
            TabList = new ObservableCollection<CustomTab>();
            ScreenName = "";
        }
    }
}