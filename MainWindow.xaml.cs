// ...existing code...
        private void CreateTabInternal(string urlString, string nameString, WebView2 webViewToClone = null)
        {
            TabHelper.CreateTabInternal(this, urlString, nameString, webViewToClone);
        }

        private void CreateTab(string urlString, string nameString)
        {
            TabHelper.CreateTab(this, urlString, nameString);
        }
// ...existing code...
