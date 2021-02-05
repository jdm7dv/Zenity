// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.VisualExplorer
{
    using System;
    using System.Windows;
    using System.Windows.Browser;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Code behind of AuthorPopUpWindow.xaml
    /// </summary>
    public partial class AuthorPopUpWindow : UserControl
    {
        /// <summary>
        /// Uri of Bing
        /// </summary>
        private string bingURL = VisualExplorerResource.BingSearchUri;

        /// <summary>
        /// Initializes a new instance of the AuthorPopUpWindow class
        /// </summary>
        public AuthorPopUpWindow()
        {
            this.InitializeComponent();
            this.txtPivot.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnPivotMouseLeftButtonUp);
            this.closeBtn.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCloseBtnMouseLeftButtonUp);
            this.txtBing.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnBingMouseLeftButtonUp);
            this.txtZentity.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnZentityMouseLeftButtonUp);
            this.ResourceTitle = string.Empty;
            this.ResourceId = string.Empty;
        }

        /// <summary>
        /// Gets or sets the title of resource
        /// </summary>
        public string ResourceTitle
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Identifier of the Resource
        /// </summary>
        public string ResourceId
        {
            get;
            set;
        }

        /// <summary>
        /// Set ToolTip for Resource title
        /// </summary>
        internal void SetToolTip()
        {
            ToolTipService.SetToolTip(this.txtResourceTitle, this.txtResourceTitle.Text);
        }

        /// <summary>
        /// MouseLeftButtonUp event handler for Zentity Web UI text
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for MouseLeftButtonUp event</param>
        private void OnZentityMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Uri navigateToUri = new Uri(string.Format("{0}Redirect.aspx?WebUI=1&Id={1}&Type={2}", this.GetURL(), this.ResourceId, this.txtResourceType.Text.Replace(":", string.Empty)));
            HtmlPage.Window.Navigate(navigateToUri, "_blank");
        }

        /// <summary>
        /// MouseLeftButtonUp event handler for Pivot text
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for MouseLeftButtonUp event</param>
        private void OnPivotMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Uri navigateToUri = new Uri(string.Format("{0}Redirect.aspx?Pivot=1&Id={1}&Type={2}", this.GetURL(), this.ResourceId, this.txtResourceType.Text.Replace(":", string.Empty)));
            HtmlPage.Window.Navigate(navigateToUri);
        }

        /// <summary>
        /// MouseLeftButtonUp event handler for Bing text
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for MouseLeftButtonUp event</param>
        private void OnBingMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Uri navigateToUri = new Uri(string.Format(this.bingURL, this.ResourceTitle), UriKind.Absolute);
            HtmlPage.Window.Navigate(navigateToUri, "_blank");
        }

        /// <summary>
        /// MouseLeftButtonUp event handler for Close button
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for MouseLeftButtonUp event</param>
        private void OnCloseBtnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Get the Url of the current host site
        /// </summary>
        /// <returns>Uri of the current host site</returns>
        private string GetURL()
        {
            string str = Application.Current.Host.Source.ToString();
            str = str.Substring(0, str.LastIndexOf("/"));
            if (str.Contains("/"))
            {
                str = str.Substring(0, str.LastIndexOf("/") + 1);
            }

            return str;
        }
    }
}
