// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Pivot.Web.Viewer
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Browser;
    using System.Windows.Controls;
    using System.Windows.Pivot;
    using System.Windows.Threading;

    /// <summary>
    /// Code behind of PivotPage.xaml
    /// Contains the implementation of loading collections to PivotViewer control
    /// </summary>
    public partial class PivotPage : UserControl
    {
        /// <summary>
        /// ResourceId identifier initialized from querystring
        /// </summary>
        private string resourceId = string.Empty;
        
        /// <summary>
        /// Collection Uri initialized from querystring
        /// </summary>
        private string collectionUri = string.Empty;

        /// <summary>
        /// Instance of ZentityViewer control
        /// </summary>
        private ZentityViewer pivotViewer;

        /// <summary>
        /// Initializes a new instance of the PivotPage class
        /// </summary>
        public PivotPage()
        {
            this.InitializeComponent();
            this.Loaded += new RoutedEventHandler(this.OnPivotPageLoaded);
        }

        /// <summary>
        /// Loaded event handler
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">Event data of Loaded event</param>
        public void OnPivotPageLoaded(object sender, RoutedEventArgs e)
        {
            if (this.ReadQueryString())
            {
                this.pivotViewer = new ZentityViewer();                
                this.pivotViewer.CollectionLoadingCompleted += new EventHandler(this.OnCollectionLoadingCompleted);
                this.pivotViewer.ItemActionExecuted += new EventHandler<ItemActionEventArgs>(this.OnPivotViewerItemActionExecuted);
                this.pivotViewer.LinkClicked += new EventHandler<LinkEventArgs>(this.OnPivotViewerLinkClicked);
                this.pivotViewer.CollectionLoadingFailed += new EventHandler<CollectionErrorEventArgs>(this.OnCollectionLoadingFailed);

                this.rootGrid.Children.Add(this.pivotViewer);
                if (!string.IsNullOrWhiteSpace(this.resourceId))
                {
                    this.pivotViewer.CurrentItemId = this.resourceId;
                }

                this.pivotViewer.LoadCollection(this.collectionUri, string.Empty);                  
            }
        }

        /// <summary>
        /// PivotViewer LinkClicked event handler
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">Event data for LinkClicked event</param>
        private void OnPivotViewerLinkClicked(object sender, LinkEventArgs e)
        {
            string linkUriString = e.Link.ToString();
            this.resourceId = string.Empty;
            if (!linkUriString.Contains(".cxml"))
            {
                HtmlPage.Window.Navigate(e.Link, "_blank");
            }
            else
            {
                int index = linkUriString.IndexOf("?", StringComparison.InvariantCultureIgnoreCase);
                if (index > 0)
                {
                    this.collectionUri = linkUriString.Remove(index);
                    index = linkUriString.IndexOf("?Id=EQ.", StringComparison.InvariantCultureIgnoreCase);
                    if (index > 0)
                    {
                        this.resourceId = linkUriString.Substring(index + 7);
                    }
                }

                this.pivotViewer.LoadCollection(linkUriString, string.Empty);
            }
        }

        /// <summary>
        /// PivotViewer ItemActionExecuted event handler
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">Event data for ItemActionExecuted event</param>
        private void OnPivotViewerItemActionExecuted(object sender, ItemActionEventArgs e)
        {
            if (e.CustomActionId == ZentityViewer.VisualExplorerActionId)
            { 
                HtmlPage.Window.Navigate(new Uri(string.Format("../Default.aspx?{0}={1}", QueryStringKeys.ResourceId, e.ItemId), UriKind.Relative), "_blank");
            }
        }

        /// <summary>
        /// PivotViewer CollectionLoadingCompleted event handler
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">Event data for CollectionLoadingCompleted event</param>
        private void OnCollectionLoadingCompleted(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(this.resourceId))
                {
                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(2);
                    EventHandler tickHandler = null;
                    tickHandler = (EventHandler)delegate(object o, EventArgs eventArgs)
                    {
                        timer.Stop();
                        timer.Tick -= tickHandler;
                        this.pivotViewer.CurrentItemId = this.resourceId;
                    };
                    timer.Tick += tickHandler;
                    timer.Start();
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Read QueryString and initialize member
        /// </summary>
        /// <returns>true if collectionUri is specified in QueryString else false</returns>
        private bool ReadQueryString()
        {
            try
            {
                this.collectionUri = HtmlPage.Document.QueryString[QueryStringKeys.CollectionUri];
                if (HtmlPage.Document.QueryString.ContainsKey(QueryStringKeys.ResourceId))
                {
                    this.resourceId = HtmlPage.Document.QueryString[QueryStringKeys.ResourceId];
                }

                if (string.IsNullOrWhiteSpace(this.collectionUri))
                {
                    return false;
                }

                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// PivotViewer CollectionLoadingFailed event handler
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">Event data for CollectionLoadingFailed event</param>
        private void OnCollectionLoadingFailed(object sender, CollectionErrorEventArgs e)
        {
            MessageBox.Show(string.Format("An error occured while loading the collection. \nPlease try again."), "PivotViewer - Error", MessageBoxButton.OK);
        }
    }

    /// <summary>
    /// Contains keys of QueryString values used by PivotPage
    /// </summary>
    public class QueryStringKeys
    {
        /// <summary>
        /// QueryString key for ResourceId
        /// </summary>
        public const string ResourceId = "Id";

        /// <summary>
        /// QueryString key for CollectionUri
        /// </summary>
        public const string CollectionUri = "Uri";
    }
}
