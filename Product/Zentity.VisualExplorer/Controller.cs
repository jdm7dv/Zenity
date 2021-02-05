// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.VisualExplorer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Json;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Windows;
    using System.Windows.Browser;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Xml.Serialization;
    using GuanxiMapCore;
    using Zentity.VisualExplorer.VisualExplorerService;

    /// <summary>
    /// Contains all event handlers and logic of VisualExplorer
    /// </summary>
    public class Controller
    {
        /// <summary>
        /// Default Node color - black
        /// </summary>
        private const string GuanxiMapDefaultNodeColor = "000000";

        /// <summary>
        /// Singleton instance of Controller class
        /// </summary>
        private static Controller instance = new Controller();
        
        /// <summary>
        /// Flag denoting if search due to AutoComplete is in progress
        /// </summary>
        private bool searchInProgress = false;
        
        /// <summary>
        /// VisualExplorer service client
        /// </summary>
        private VisualExplorerServiceClient client;
        
        /// <summary>
        /// FilterWindow instance
        /// </summary>
        private FilterWindow filterWindow;

        /// <summary>
        /// X axis offset to display graph
        /// </summary>
        private double guanximapOffsetX;        

        /// <summary>
        /// Y axis offset to display graph
        /// </summary>
        private double guanximapOffsetY;

        /// <summary>
        /// Gets the singleton instance of Controller class
        /// </summary>
        public static Controller Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Gets or sets the graph filter options.
        /// </summary>
        private VisualExplorerGraphFilterOptions GraphFilterOptions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets master graph filter options
        /// </summary>
        private VisualExplorerGraphFilterOptions MasterGraphFilterOptions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the master graph data.
        /// This graph contains the original search results, and copies of this 
        /// are used to filter the data based on the current filter options.
        /// </summary>
        private VisualExplorerGraph MasterGraph
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Filter options are loading.
        /// 1. Disables the Ok button.
        /// 2. Disables the select all entities checkbox.
        /// 3. Disables the select all relationships checkbox.
        /// </summary>
        private bool IsFilterOptionsLoading
        {
            get
            {
                return this.IsFilterOptionsLoading;
            }

            set
            {
                if (value)
                {
                    this.filterWindow.OKButton.IsEnabled = false;
                    this.filterWindow.cbAllEntities.IsEnabled = false;
                    this.filterWindow.cbAllRelations.IsEnabled = false;
                    this.filterWindow.busyIndicator.IsBusy = true;
                }
                else
                {
                    this.filterWindow.OKButton.IsEnabled = true;
                    this.filterWindow.cbAllEntities.IsEnabled = true;
                    this.filterWindow.cbAllRelations.IsEnabled = true;
                    this.filterWindow.busyIndicator.IsBusy = false;
                }
            }
        }

        /// <summary>
        /// Initialize method
        /// </summary>
        public void Init()
        {
            this.BuildUI();
            this.AddListeners();
            this.LoadMap();
        }

        /// <summary>
        /// Helper method to return the host site url
        /// </summary>
        /// <returns>Host site url</returns>
        private static string GetUrl()
        {
            string str = Application.Current.Host.Source.ToString();
            str = str.Substring(0, str.LastIndexOf("/"));
            if (str.Contains("/"))
            {
                str = str.Substring(0, str.LastIndexOf("/") + 1);
            }

            return str;
        }
        
        /// <summary>
        /// Add all event handlers
        /// </summary>
        private void AddListeners()
        {
            MainPage.Instance.searchBtn.Click += new RoutedEventHandler(this.OnSearchBtnClick);
            MainPage.Instance.searchTextBox.KeyUp += new KeyEventHandler(this.OnSearchTextBoxKeyUp);
            MainPage.Instance.searchTextBox.Populating += new PopulatingEventHandler(this.OnSearchTextBoxPopulating);

            MainPage.Instance.mapControl.arrowBtn.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnArrowImageMouseLeftButtonDown);
            MainPage.Instance.mapControl.zoomInBtn.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnZoomInBtnMouseLeftButtonDown);
            MainPage.Instance.mapControl.zoomOutBtn.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnZoomOutBtnMouseLeftButtonDown);

            MainPage.Instance.guanxiMap.EdgeClick += new MouseEventHandler(this.OnGuanxiMapEdgeClick);
            MainPage.Instance.guanxiMap.ItemClick += new MouseEventHandler(this.OnGuanxiMapItemClick);
            MainPage.Instance.guanxiMap.MenuClick += new MouseEventHandler(this.OnGuanxiMapMenuClick);
            MainPage.Instance.guanxiMap.MoveMapEvent += new MoveMap(this.MoveMap);            

            MainPage.Instance.optionsBtn.Click += new RoutedEventHandler(this.OnOptionsBtnClick);

            this.client = new VisualExplorerServiceClient("BasicHttpBinding_IVisualExplorerService", string.Format("{0}services/VisualExplorer.svc", GetUrl()));
            this.client.GetResourcesByKeywordCompleted += new EventHandler<GetResourcesByKeywordCompletedEventArgs>(this.OnGetResourcesByKeywordCompleted);
            this.client.GetResourceRelationByResourceIdCompleted += new EventHandler<GetResourceRelationByResourceIdCompletedEventArgs>(this.OnGetResourceRelationByResourceIdCompleted);
            this.client.GetVisualExplorerGraphByResourceIdCompleted += new EventHandler<GetVisualExplorerGraphByResourceIdCompletedEventArgs>(this.OnGetVisualExplorerGraphByResourceIdCompleted);
            this.client.GetResourceMetadataByResourceIdCompleted += new EventHandler<GetResourceMetadataByResourceIdCompletedEventArgs>(this.OnGetResourceMetadataByResourceIdCompleted);
            this.client.GetVisualExplorerGraphBySearchKeywordCompleted += new EventHandler<GetVisualExplorerGraphBySearchKeywordCompletedEventArgs>(this.OnGetVisualExplorerGraphBySearchKeywordCompleted);
        }

        /// <summary>
        /// Callback event that gets called when the visual explorer filter list gets fetched from the zentity server.
        /// </summary>
        /// <param name="sender">Callback sender</param>
        /// <param name="e">Callback parameters</param>
        private void OnGetVisualExplorerFilterListCompleted(object sender, GetVisualExplorerFilterListCompletedEventArgs e)
        {
            VisualExplorerFilterList filterList = e.Result;
            VisualExplorerGraphFilterOptions filterOptions = null;
            if (filterList != null && filterList.RelationShipTypes != null && filterList.ResourceTypes != null)
            {
                filterOptions = VisualExplorerGraphFilterOptions.Merge(filterList.ResourceTypes.ToList(), filterList.RelationShipTypes.ToList());
            }

            this.GraphFilterOptions = filterOptions;
            this.UpdateAllEntitiesSelectionState();
            this.UpdateAllRelationshipsSelectionState();
            this.MasterGraphFilterOptions = this.DeepCopy(this.GraphFilterOptions);
            if (this.GraphFilterOptions != null && this.GraphFilterOptions.Relationships != null && this.GraphFilterOptions.Resources != null)
            {
                this.filterWindow.entityListBox.ItemsSource = this.GraphFilterOptions.Resources;
                this.filterWindow.relationsListBox.ItemsSource = this.GraphFilterOptions.Relationships;
            }

            this.IsFilterOptionsLoading = false;
            if (filterList == null)
            {
                this.filterWindow.Close();
                MainPage.Instance.noResultPopUpWindow.Visibility = Visibility.Visible;
                MainPage.Instance.noResultPopUpWindow.txtMessage.Text = string.Format("{0}", Messages.ErrorOccured);
                MainPage.Instance.noResultPopUpWindow.HorizontalAlignment = HorizontalAlignment.Center;
                MainPage.Instance.noResultPopUpWindow.VerticalAlignment = VerticalAlignment.Center;
                MainPage.Instance.noResultPopUpWindow.btnClose.Focus();
            }
        }

        /// <summary>
        /// Options button Click event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for Click event</param>
        private void OnOptionsBtnClick(object sender, RoutedEventArgs e)
        {
            this.filterWindow = new FilterWindow();

            this.filterWindow.KeyUp += new KeyEventHandler(this.OnFilterWindowKeyUp);
            this.filterWindow.OKButton.Click += new RoutedEventHandler(this.OnFilterWindowOKButtonClick);
            this.filterWindow.CancelButton.Click += new RoutedEventHandler(this.OnFilterWindowCancelButtonClick);

            this.filterWindow.cbAllEntities.Checked += new RoutedEventHandler(this.OnAllEntitiesChecked);
            this.filterWindow.cbAllEntities.Unchecked += new RoutedEventHandler(this.OnAllEntitiesUnchecked);

            this.filterWindow.cbAllRelations.Checked += new RoutedEventHandler(this.OnAllRelationsChecked);
            this.filterWindow.cbAllRelations.Unchecked += new RoutedEventHandler(this.OnAllRelationsUnchecked);

            this.filterWindow.FilterEntityCheckBoxClicked += new FilterCheckBoxClicked(this.OnFilterEntityCheckBoxClicked);
            this.filterWindow.FilterRelationsCheckBoxClicked += new FilterCheckBoxClicked(this.OnFilterRelationsCheckBoxClicked);

            this.client.GetVisualExplorerFilterListCompleted += new EventHandler<GetVisualExplorerFilterListCompletedEventArgs>(this.OnGetVisualExplorerFilterListCompleted);
            this.client.GetVisualExplorerFilterListAsync();

            this.IsFilterOptionsLoading = true;
            this.filterWindow.Show();
        }

        /// <summary>
        /// Search TextBox KeyUp event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for KeyUp event</param>
        private void OnSearchTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    string query = MainPage.Instance.searchTextBox.Text.Trim();
                    if (query.Length > 0)
                    {
                        this.searchInProgress = true;
                        MainPage.Instance.busyIndicator.Visibility = Visibility.Visible;
                        MainPage.Instance.busyIndicator.IsBusy = true;
                        MainPage.Instance.smallCircleProgress.stop();
                        MainPage.Instance.smallCircleProgress.Visibility = Visibility.Collapsed;
                        this.OnSearchBtnClick(sender, null);
                    }

                    break;
                case Key.Escape:
                    MainPage.Instance.authorPopUpWindow.Visibility = Visibility.Collapsed;
                    MainPage.Instance.popUpWindow.Visibility = Visibility.Collapsed;                    
                    break;
            }
        }

        /// <summary>
        /// Search TextBox Populating event
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for Populating event</param>
        private void OnSearchTextBoxPopulating(object sender, PopulatingEventArgs e)
        {
            e.Cancel = true;
            if (this.searchInProgress)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(e.Parameter))
            {
                this.client.GetResourcesByKeywordAsync(e.Parameter, (AutoCompleteBox)sender);
                MainPage.Instance.smallCircleProgress.Visibility = Visibility.Visible;
                MainPage.Instance.smallCircleProgress.start();
            }
        }

        /// <summary>
        /// Callback method that gets called after a search by keyword is performed
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Result of the webservice call</param>
        private void OnGetResourcesByKeywordCompleted(object sender, GetResourcesByKeywordCompletedEventArgs e)
        {
            AutoCompleteBox autoComplete = e.UserState as AutoCompleteBox;
            if (!this.searchInProgress && autoComplete != null && e.Error == null && !e.Cancelled && !string.IsNullOrEmpty(e.Result))
            {
                try
                {
                    List<KeyValuePair<Guid, string>> data = new List<KeyValuePair<Guid, string>>();

                    JsonArray results = (JsonArray)JsonArray.Parse(e.Result);
                    if (results.Count > 1)
                    {
                        string originalSearchString = results[0]["value"];
                        if (originalSearchString == autoComplete.SearchText)
                        {
                            for (int count = 1; count < results.Count; count++)
                            {
                                data.Add(new KeyValuePair<Guid, string>(results[count]["key"], results[count]["value"]));
                            }

                            // Diplay the AutoCompleteBox drop down with any suggestions
                            autoComplete.ItemsSource = data;
                            autoComplete.ItemFilter = (search, item) =>
                            {
                                try
                                {
                                    KeyValuePair<Guid, string> resultItem = (KeyValuePair<Guid, string>)item;
                                    string filter = search.ToLower();
                                    return resultItem.Value.ToLower().Contains(filter);
                                }
                                catch
                                {
                                    return false;
                                }
                            };
                            autoComplete.PopulateComplete();
                        }
                    }
                }
                catch
                {
                }

                MainPage.Instance.smallCircleProgress.stop();
                MainPage.Instance.smallCircleProgress.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Arrow button MouseLeftButtonDown event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for MouseLeftButtonDown event</param>
        private void OnArrowImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Grid relativeTo = sender as Grid;
            Point position = e.GetPosition(relativeTo);
            MainPage.Instance.guanxiMap.moveMap((position.X - (relativeTo.Width / 2.0)) * -4.0, (position.Y - (relativeTo.Height / 2.0)) * -4.0);
        }

        /// <summary>
        /// Initialize VisualExplorer
        /// </summary>
        private void BuildUI()
        {
            MainPage.Instance.mapControl.Visibility = Visibility.Visible;
            MainPage.Instance.guanxiMap.mapScale = Config.Scale;
            MainPage.Instance.guanxiMap.defaultFontSize = Config.FontSize;
            MainPage.Instance.guanxiMap.defaultRadius = Config.Radius;
            MainPage.Instance.guanxiMap.defaultImageUrl = string.Empty;
            MainPage.Instance.guanxiMap.edgeThickness = Config.EdgeThickness;
            MainPage.Instance.guanxiMap.hoverThickness = Config.EdgeThickness * 2.0;
            MainPage.Instance.guanxiMap.isEdgeClickable = Config.IsEdgeClickable;
            MainPage.Instance.guanxiMap.IsHitTestVisible = Config.IsInteractive;
            MainPage.Instance.guanxiMap.centerItemColor = 0xff9900;
            MainPage.Instance.guanxiMap.defaultItemColor = 0x6699;
            MainPage.Instance.guanxiMap.menuText = VisualExplorerResource.VisualExplorerMenuText;
            MainPage.Instance.searchTextBox.IsEnabled = true;
        }

        /// <summary>
        /// VisualExplorer EdgeClick event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for EdgeClick event</param>
        private void OnGuanxiMapEdgeClick(object sender, MouseEventArgs e)
        {
            string uid;
            string str2;
            GuanxiMapCore.Edge edge = sender as GuanxiMapCore.Edge;
            MainPage.Instance.authorPopUpWindow.Visibility = Visibility.Collapsed;

            if (edge.edgeData.item1.query == MainPage.Instance.guanxiMap.graphData.query)
            {
                uid = edge.edgeData.item1.uid.Remove(0, 1).Remove(edge.edgeData.item1.uid.Length - 2, 1);
                str2 = edge.edgeData.item2.uid.Remove(0, 1).Remove(edge.edgeData.item2.uid.Length - 2, 1);
                this.client.GetResourceRelationByResourceIdAsync(uid, str2);
            }
            else
            {
                uid = edge.edgeData.item2.uid.Remove(0, 1).Remove(edge.edgeData.item2.uid.Length - 2, 1);
                str2 = edge.edgeData.item1.uid.Remove(0, 1).Remove(edge.edgeData.item1.uid.Length - 2, 1);
                this.client.GetResourceRelationByResourceIdAsync(str2, uid);
            }

            MainPage.Instance.popUpWindow.Visibility = Visibility.Visible;
            MainPage.Instance.popUpWindow.smallCircleProgress.start();
            double delta = Application.Current.Host.Content.ActualWidth - (e.GetPosition(MainPage.Instance.LayoutRoot).X + 350);
            if (delta < 0)
            {
                Canvas.SetLeft(MainPage.Instance.popUpWindow, e.GetPosition(MainPage.Instance.LayoutRoot).X + 5.0 + delta);
            }
            else
            {
                Canvas.SetLeft(MainPage.Instance.popUpWindow, e.GetPosition(MainPage.Instance.LayoutRoot).X + 10.0);
            }

            delta = Application.Current.Host.Content.ActualHeight - (e.GetPosition(MainPage.Instance.LayoutRoot).Y + 150);
            if (delta < 0)
            {
                Canvas.SetTop(MainPage.Instance.popUpWindow, e.GetPosition(MainPage.Instance.LayoutRoot).Y + 5.0 + delta);
            }
            else
            {
                Canvas.SetTop(MainPage.Instance.popUpWindow, e.GetPosition(MainPage.Instance.LayoutRoot).Y + 10.0);
            }
        }

        /// <summary>
        /// Callback method that gets called after a search for relation metadata is performed for the resource
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Result of the webservice call</param>
        private void OnGetResourceRelationByResourceIdCompleted(object sender, GetResourceRelationByResourceIdCompletedEventArgs e)
        {
            try
            {
                JsonArray arr = JsonObject.Parse(e.Result) as JsonArray;
                JsonValue obj1 = arr[0];
                JsonValue obj2 = arr[1];
                JsonValue obj3 = arr[2];

                MainPage.Instance.popUpWindow.SubjectResourceTitle = obj1.ToString();
                MainPage.Instance.popUpWindow.ObjectResourceTitle = obj3.ToString();
                MainPage.Instance.popUpWindow.PropertyTitle = obj2.ToString();
                MainPage.Instance.popUpWindow.LoadText();
            }
            catch
            {
                MainPage.Instance.noResultPopUpWindow.Visibility = Visibility.Visible;
                MainPage.Instance.noResultPopUpWindow.txtMessage.Text = string.Format("{0}", Messages.ErrorOccured);
                MainPage.Instance.noResultPopUpWindow.HorizontalAlignment = HorizontalAlignment.Center;
                MainPage.Instance.noResultPopUpWindow.VerticalAlignment = VerticalAlignment.Center;
                MainPage.Instance.popUpWindow.text1.Text = string.Empty;
                MainPage.Instance.popUpWindow.text2.Text = string.Empty;
            }

            MainPage.Instance.popUpWindow.smallCircleProgress.stop();
        }

        /// <summary>
        /// VisualExplorer ItemClick event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for ItemClick event</param>
        private void OnGuanxiMapItemClick(object sender, MouseEventArgs e)
        {
            Item item = sender as Item;
            string id = item.itemData.uid.Remove(0, 1).Remove(item.itemData.uid.Length - 2, 1);
            string name = HttpUtility.UrlEncode(item.itemData.title);

            string uri = string.Format("{0}Default.aspx?Id={1}&k={2}", GetUrl(), id, name);
            HtmlPage.Window.Navigate(new Uri(uri));
        }

        /// <summary>
        /// VisualExplorer MenuClick event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for MenuClick event</param>
        private void OnGuanxiMapMenuClick(object sender, MouseEventArgs e)
        {
            HoverItem item = sender as HoverItem;
            string id = item.itemData.uid;
            MainPage.Instance.popUpWindow.Visibility = Visibility.Collapsed;
            this.GetResourceMetadata(id.Remove(0, 1).Remove(id.Length - 2, 1));            
            MainPage.Instance.authorPopUpWindow.Visibility = Visibility.Visible;

            double delta = Application.Current.Host.Content.ActualWidth - (e.GetPosition(MainPage.Instance.LayoutRoot).X + 410);
            if (delta < 0)
            {
                Canvas.SetLeft(MainPage.Instance.authorPopUpWindow, e.GetPosition(MainPage.Instance.LayoutRoot).X + 5.0 + delta);
            }
            else
            {
                Canvas.SetLeft(MainPage.Instance.authorPopUpWindow, e.GetPosition(MainPage.Instance.LayoutRoot).X + 10.0);
            }

            delta = Application.Current.Host.Content.ActualHeight - (e.GetPosition(MainPage.Instance.LayoutRoot).Y + 410);

            if (delta < 0)
            {
                Canvas.SetTop(MainPage.Instance.authorPopUpWindow, e.GetPosition(MainPage.Instance.LayoutRoot).Y + 5.0 + delta);
            }
            else
            {
                Canvas.SetTop(MainPage.Instance.authorPopUpWindow, e.GetPosition(MainPage.Instance.LayoutRoot).Y + 10.0);
            }
        }

        /// <summary>
        /// Helper method to get Resource metadata
        /// </summary>
        /// <param name="resourceId">Identifier of the resource</param>
        private void GetResourceMetadata(string resourceId)
        {
            this.client.GetResourceMetadataByResourceIdAsync(resourceId);
            MainPage.Instance.authorPopUpWindow.smallCircleProgress.start();
        }

        /// <summary>
        /// Callback method that gets called after metadata for the resource is returned by the service
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Result of the webservice call</param>
        private void OnGetResourceMetadataByResourceIdCompleted(object sender, GetResourceMetadataByResourceIdCompletedEventArgs e)
        {
            try
            {
                object obj2 = e;
                JsonArray obj3 = JsonObject.Parse(e.Result) as JsonArray;

                if (obj3 == null)
                {
                    MainPage.Instance.authorPopUpWindow.Visibility = Visibility.Collapsed;
                    MainPage.Instance.authorPopUpWindow.smallCircleProgress.stop();
                    return;
                }

                string title = (string)obj3[0]["Value"];
                string resourceType = (string)obj3[1]["Value"];
                string dateAdded = (string)obj3[2]["Value"];
                string dateModified = (string)obj3[3]["Value"];
                string description = (string)obj3[4]["Value"];
                string uri = (string)obj3[5]["Value"];
                string id = (string)obj3[6]["Value"];

                if (!string.IsNullOrEmpty(id))
                {
                    MainPage.Instance.authorPopUpWindow.ResourceId = id;
                }

                if (!string.IsNullOrEmpty(resourceType))
                {
                    MainPage.Instance.authorPopUpWindow.txtResourceType.Visibility = Visibility.Visible;
                    MainPage.Instance.authorPopUpWindow.txtResourceType.Text = string.Format("{0}: ", resourceType);
                }
                else
                {
                    MainPage.Instance.authorPopUpWindow.txtResourceType.Visibility = Visibility.Collapsed;
                }

                if (!string.IsNullOrEmpty(title))
                {
                    MainPage.Instance.authorPopUpWindow.txtResourceTitle.Visibility = Visibility.Visible;
                    MainPage.Instance.authorPopUpWindow.txtResourceTitle.Text = title;
                    MainPage.Instance.authorPopUpWindow.ResourceTitle = title;
                }
                else
                {
                    MainPage.Instance.authorPopUpWindow.txtResourceTitle.Visibility = Visibility.Collapsed;
                }

                if (!string.IsNullOrEmpty(dateAdded))
                {
                    MainPage.Instance.authorPopUpWindow.txtDateAdded.Visibility = Visibility.Visible;
                    MainPage.Instance.authorPopUpWindow.txtDateAdded.Text = string.Format("Added: {0}", dateAdded);
                }
                else
                {
                    MainPage.Instance.authorPopUpWindow.txtDateAdded.Visibility = Visibility.Collapsed;
                }

                if (!string.IsNullOrEmpty(dateModified))
                {
                    MainPage.Instance.authorPopUpWindow.txtDateModified.Visibility = Visibility.Visible;
                    MainPage.Instance.authorPopUpWindow.txtDateModified.Text = string.Format("Modified: {0}", dateModified);
                }
                else
                {
                    MainPage.Instance.authorPopUpWindow.txtDateModified.Visibility = Visibility.Collapsed;
                }

                if (!string.IsNullOrEmpty(description))
                {
                    MainPage.Instance.authorPopUpWindow.txtDescription.Visibility = Visibility.Visible;
                    MainPage.Instance.authorPopUpWindow.txtDescription.Text = string.Format("{0}", description);
                }
                else
                {
                    MainPage.Instance.authorPopUpWindow.txtDescription.Visibility = Visibility.Collapsed;
                }
            }
            catch
            {
                MainPage.Instance.authorPopUpWindow.Visibility = Visibility.Collapsed;
                MainPage.Instance.noResultPopUpWindow.txtMessage.Text = string.Format("{0}", Messages.ErrorOccured);
                MainPage.Instance.noResultPopUpWindow.Visibility = Visibility.Visible;
            }

            MainPage.Instance.authorPopUpWindow.smallCircleProgress.stop();
        }

        /// <summary>
        /// Helper method to get VisualExplorer graph on load
        /// </summary>
        private void LoadMap()
        {
            string resourceId = string.Empty, searchText = string.Empty;
            if (HtmlPage.Document.QueryString.ContainsKey("Id"))
            {
                resourceId = HtmlPage.Document.QueryString["Id"];
            }

            if (HtmlPage.Document.QueryString.ContainsKey("k"))
            {
                searchText = HtmlPage.Document.QueryString["k"];
            }

            if (!string.IsNullOrWhiteSpace(resourceId))
            {
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    MainPage.Instance.searchTextBox.Text = searchText;
                }

                this.client.GetVisualExplorerGraphByResourceIdAsync(resourceId);
                this.searchInProgress = true;
            }
            else if (!string.IsNullOrWhiteSpace(searchText))
            {
                MainPage.Instance.searchTextBox.Text = searchText;
                this.client.GetVisualExplorerGraphBySearchKeywordAsync(searchText);
                this.searchInProgress = true;
            }

            if (this.searchInProgress)
            {
                MainPage.Instance.busyIndicator.IsBusy = true;
                MainPage.Instance.busyIndicator.Visibility = Visibility.Visible;

                MainPage.Instance.smallCircleProgress.stop();
                MainPage.Instance.smallCircleProgress.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Event handler for MoveMap event
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="isLeft">flag to denote direction</param>
        private void MoveMap(object sender, bool isLeft)
        {
            if (isLeft)
            {
                double num = MainPage.Instance.guanxiMap.mapOffsetX - this.guanximapOffsetX;
                num *= 0.5;
                this.guanximapOffsetX = MainPage.Instance.guanxiMap.mapOffsetX;
                if (MainPage.Instance.authorPopUpWindow.Visibility == Visibility.Visible)
                {
                    Canvas.SetLeft(MainPage.Instance.authorPopUpWindow, Canvas.GetLeft(MainPage.Instance.authorPopUpWindow) + num);
                }

                if (MainPage.Instance.popUpWindow.Visibility == Visibility.Visible)
                {
                    Canvas.SetLeft(MainPage.Instance.popUpWindow, Canvas.GetLeft(MainPage.Instance.popUpWindow) + num);
                }

                double num2 = MainPage.Instance.guanxiMap.mapOffsetY - this.guanximapOffsetY;
                num2 *= 0.5;
                this.guanximapOffsetY = MainPage.Instance.guanxiMap.mapOffsetY;
                if (MainPage.Instance.authorPopUpWindow.Visibility == Visibility.Visible)
                {
                    Canvas.SetTop(MainPage.Instance.authorPopUpWindow, Canvas.GetTop(MainPage.Instance.authorPopUpWindow) + num2);
                }

                if (MainPage.Instance.popUpWindow.Visibility == Visibility.Visible)
                {
                    Canvas.SetTop(MainPage.Instance.popUpWindow, Canvas.GetTop(MainPage.Instance.popUpWindow) + num2);
                }
            }
            else
            {
                MainPage.Instance.popUpWindow.Visibility = Visibility.Collapsed;
                MainPage.Instance.authorPopUpWindow.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Search button Click event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for Click event</param>
        private void OnSearchBtnClick(object sender, RoutedEventArgs e)
        {
            string query = MainPage.Instance.searchTextBox.Text.Trim();
            if (query.Length > 0)
            {
                string selectedText = string.Empty, selectedItemId = string.Empty;
                if (MainPage.Instance.searchTextBox.SelectedItem != null)
                {
                    KeyValuePair<Guid, string> item = (KeyValuePair<Guid, string>)MainPage.Instance.searchTextBox.SelectedItem;
                    selectedItemId = item.Key.ToString();
                    selectedText = item.Value;
                }

                string uri = string.Empty;
                string name = HttpUtility.UrlEncode(query);
                if (selectedText == query)
                {
                    uri = string.Format("{0}Default.aspx?Id={1}&k={2}", GetUrl(), selectedItemId, name);
                }
                else
                {
                    uri = string.Format("{0}Default.aspx?k={1}", GetUrl(), name);
                }

                HtmlPage.Window.Navigate(new Uri(uri));
            }
            else
            {
                MainPage.Instance.busyIndicator.IsBusy = false;
                MainPage.Instance.busyIndicator.Visibility = Visibility.Collapsed;
                this.searchInProgress = false;
            }
        }

        /// <summary>
        /// Callback method after getting VisualExplorer graph for a keyword
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Result of the webservice call</param>
        private void OnGetVisualExplorerGraphBySearchKeywordCompleted(object sender, GetVisualExplorerGraphBySearchKeywordCompletedEventArgs e)
        {
            this.ShowVisualExplorerGraph(e.Result);
        }

        /// <summary>
        /// Callback method after getting VisualExplorer graph by resource identifier
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Result of the webservice call</param>
        private void OnGetVisualExplorerGraphByResourceIdCompleted(object sender, GetVisualExplorerGraphByResourceIdCompletedEventArgs e)
        {
            this.ShowVisualExplorerGraph(e.Result);
        }

        /// <summary>
        /// ZoomIn button MouseLeftButtonDown event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for MouseLeftButtonDown event</param>
        private void OnZoomInBtnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainPage.Instance.guanxiMap.zoomIn();
        }

        /// <summary>
        /// ZoomOut button MouseLeftButtonDown event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for MouseLeftButtonDown event</param>
        private void OnZoomOutBtnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainPage.Instance.guanxiMap.zoomOut();
        }
        
        /// <summary>
        /// This event is called when All Relationships checkbox is unchecked.
        /// This unchecks all the relationships.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void OnAllRelationsUnchecked(object sender, RoutedEventArgs e)
        {
            this.GraphFilterOptions.Relationships.ForEach(relationship => { relationship.IsVisible = false; });
            this.filterWindow.cbAllRelations.IsThreeState = false;
        }

        /// <summary>
        /// This event is called when All Relationships checkbox is checked.
        /// This checks all the relationships.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void OnAllRelationsChecked(object sender, RoutedEventArgs e)
        {
            this.GraphFilterOptions.Relationships.ForEach(relationship => { relationship.IsVisible = true; });
            this.filterWindow.cbAllRelations.IsThreeState = false;
        }

        /// <summary>
        /// This event is called when All Entities checkbox is unchecked.
        /// This ynchecks all the entities.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void OnAllEntitiesUnchecked(object sender, RoutedEventArgs e)
        {
            this.GraphFilterOptions.Resources.ForEach(resource => { resource.IsVisible = false; });
            this.filterWindow.cbAllEntities.IsThreeState = false;
        }

        /// <summary>
        /// This event is called when All Entities checkbox is checked.
        /// This checks all the entities.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void OnAllEntitiesChecked(object sender, RoutedEventArgs e)
        {
            this.GraphFilterOptions.Resources.ForEach(resource => { resource.IsVisible = true; });
            this.filterWindow.cbAllEntities.IsThreeState = false;
        }

        /// <summary>
        /// Relationship checkbox checked changed event handler.
        /// 1. If any of the relationship checkboxes is selected, the 'Select all relationships' checkbox
        /// goes to tri-state.
        /// 2. If none of the relationship checkboxes is selected, the 'Select all relationships' checkbox
        /// gets un-checked.
        /// 3. If all the relationship checkboxes are checked, the 'Select all relationships' checkbox
        /// gets checked.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event arguments</param>
        private void OnFilterRelationsCheckBoxClicked(object sender, RoutedEventArgs args)
        {
            this.UpdateAllRelationshipsSelectionState();
        }

        /// <summary>
        /// Entity checkbox checked changed event handler.
        /// 1. If any of the entity checkboxes is selected, the 'Select all entities' checkbox
        /// goes to tri-state.
        /// 2. If none of the entity checkboxes is selected, the 'Select all entities' checkbox
        /// gets un-checked.
        /// 3. If all the entity checkboxes are checked, the 'Select all entities' checkbox
        /// gets checked.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event arguments</param>
        private void OnFilterEntityCheckBoxClicked(object sender, RoutedEventArgs args)
        {
            this.UpdateAllEntitiesSelectionState();
        }

        /// <summary>
        /// This event is called on key press on visual explorer graph filter window.
        /// ESC - closes the window.
        /// ENTER - filters based on the filter options.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void OnFilterWindowKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    this.filterWindow.Close();
                    this.CancelVisualGraphFiltering();
                    break;

                case Key.Enter:
                    this.filterWindow.Close();
                    this.FilterVisualExplorerGraph();
                    break;
            }
        }

        /// <summary>
        /// This event is called when the cancel button on the filter options screen 
        /// is called.
        /// This closes the filter options window.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void OnFilterWindowCancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.filterWindow.Close();
            this.CancelVisualGraphFiltering();
        }

        /// <summary>
        /// Cancels the filtering of the visual explorer graph.
        /// </summary>
        private void CancelVisualGraphFiltering()
        {
            if (this.MasterGraphFilterOptions == null ||
                this.MasterGraphFilterOptions.Resources == null ||
                this.MasterGraphFilterOptions.Relationships == null)
            {
                return;
            }

            this.MasterGraphFilterOptions.Resources.ForEach(resource =>
                {
                    this.GraphFilterOptions.GetResource(resource.Name).Color = resource.Color;
                    this.GraphFilterOptions.GetResource(resource.Name).IsVisible = resource.IsVisible;
                });
            this.MasterGraphFilterOptions.Relationships.ForEach(relationship =>
            {
                this.GraphFilterOptions.GetRelationship(relationship.Name).IsVisible = relationship.IsVisible;
            });
        }

        /// <summary>
        /// This event is called when the OK button on the filter options screen 
        /// is called.
        /// This closes the filter options window and filters the graph based on the options.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void OnFilterWindowOKButtonClick(object sender, RoutedEventArgs e)
        {
            this.filterWindow.Close();
            this.MasterGraphFilterOptions = this.DeepCopy(this.GraphFilterOptions);
            if (this.MasterGraphFilterOptions == null)
            {
                return;
            }

            this.MasterGraphFilterOptions.Save();
            this.FilterVisualExplorerGraph();
        }

        /// <summary>
        /// Updates the state of the 'Select all entities' checkbox.
        /// 1. If any of the entity checkboxes is selected, the 'Select all entities' checkbox
        /// goes to tri-state.
        /// 2. If none of the entity checkboxes is selected, the 'Select all entities' checkbox
        /// gets un-checked.
        /// 3. If all the entity checkboxes are checked, the 'Select all entities' checkbox
        /// gets checked.
        /// </summary>
        private void UpdateAllEntitiesSelectionState()
        {
            if (this.GraphFilterOptions == null || this.GraphFilterOptions.Resources == null)
            {
                return;
            }

            int visibleResources = this.GraphFilterOptions.Resources.Count(resource => resource.IsVisible == true);

            if (visibleResources.Equals(this.GraphFilterOptions.Resources.Count))
            {
                this.filterWindow.cbAllEntities.IsChecked = true;
                this.filterWindow.cbAllEntities.IsThreeState = false;
            }
            else if (visibleResources.Equals(0))
            {
                this.filterWindow.cbAllEntities.IsChecked = false;
                this.filterWindow.cbAllEntities.IsThreeState = false;
            }
            else
            {
                this.filterWindow.cbAllEntities.IsThreeState = true;
                this.filterWindow.cbAllEntities.IsChecked = null;
            }
        }

        /// <summary>
        /// Updates the state of the 'Select all relationships' checkbox.
        /// 1. If any of the relationship checkboxes is selected, the 'Select all relationships' checkbox
        /// goes to tri-state.
        /// 2. If none of the relationship checkboxes is selected, the 'Select all relationships' checkbox
        /// gets un-checked.
        /// 3. If all the relationship checkboxes are checked, the 'Select all relationships' checkbox
        /// gets checked.
        /// </summary>
        private void UpdateAllRelationshipsSelectionState()
        {
            if (this.GraphFilterOptions == null || this.GraphFilterOptions.Relationships == null)
            {
                return;
            }

            int visibleRelationships = this.GraphFilterOptions.Relationships.Count(relationship => relationship.IsVisible == true);

            if (visibleRelationships.Equals(this.GraphFilterOptions.Relationships.Count))
            {
                this.filterWindow.cbAllRelations.IsChecked = true;
                this.filterWindow.cbAllRelations.IsThreeState = false;
            }
            else if (visibleRelationships.Equals(0))
            {
                this.filterWindow.cbAllRelations.IsChecked = false;
                this.filterWindow.cbAllRelations.IsThreeState = false;
            }
            else
            {
                this.filterWindow.cbAllRelations.IsThreeState = true;
                this.filterWindow.cbAllRelations.IsChecked = null;
            }
        }

        /// <summary>
        /// 1. Saves the current filter options.
        /// 2. Filters the visual explorer graph. 
        /// </summary>
        private void FilterVisualExplorerGraph()
        {
            if (this.MasterGraph != null)
            {
                this.ShowVisualExplorerGraph(this.MasterGraph);
            }
        }

        /// <summary>
        /// Filters the visual explorer graph based on the filter options.
        /// </summary>
        /// <param name="graph">Visual explorer graph to filter.</param>
        private void FilterVisualExplorerGraph(VisualExplorerGraph graph)
        {
            if (this.GraphFilterOptions == null)
            {
                this.GraphFilterOptions = VisualExplorerGraphFilterOptions.Load();
            }

            if (this.GraphFilterOptions.Resources == null || this.GraphFilterOptions.Resources.Count.Equals(0))
            {
                graph.JSONGraph.Nodes.ToList<GuanxiUI.Node>().ForEach(node =>
                {
                    if (!graph.JSONGraph.query.Equals(node.DisplayName))
                    {
                        node.BorderColor = GuanxiMapDefaultNodeColor;
                    }
                });

                return;
            }

            List<Guid> invalidResourceTypes = new List<Guid>();
            if (graph.JSONGraph != null && graph.JSONGraph.Nodes != null)
            {
                //// Find out the guids of the resource types that are not visible.
                //// Remove these from the resourcemap of the graph.
                graph.ResourceMap.Keys.ToList<Guid>().ForEach(guid =>
                    {
                        if (!this.GraphFilterOptions.IsResourceVisible(graph.ResourceMap[guid]))
                        {
                            invalidResourceTypes.Add(guid);
                        }
                    });

                invalidResourceTypes.ForEach(g => { graph.ResourceMap.Remove(g); });

                //// 1. Remove nodes which have resource types not present in the resourcemap.                
                var newNodes = from node in graph.JSONGraph.Nodes.ToList<GuanxiUI.Node>()
                               where !invalidResourceTypes.Contains(Guid.Parse(node.Id))
                               select node;

                graph.JSONGraph.Nodes = new System.Collections.ObjectModel.Collection<GuanxiUI.Node>(newNodes.ToList<GuanxiUI.Node>());

                graph.JSONGraph.Nodes.ToList<GuanxiUI.Node>().ForEach(node =>
                {
                    if (!graph.JSONGraph.query.Equals(node.DisplayName))
                    {
                        node.BorderColor = this.GraphFilterOptions.GetResource(graph.ResourceMap[Guid.Parse(node.Id)]).Color;
                    }
                });
            }

            if (graph.JSONGraph != null && graph.JSONGraph.Edges != null)
            {
                //// 1. Remove edges which are between nodes that no longer exist.
                //// Even if one node in a edge is missing, this edge is removed.
                //// 2. Remove edges that are not present in the search options.            
                var newEdges = from edge in graph.JSONGraph.Edges.ToList<GuanxiUI.Edge>()
                               where !invalidResourceTypes.Contains(Guid.Parse(edge.Node1))
                                  && !invalidResourceTypes.Contains(Guid.Parse(edge.Node2))
                                  && this.GraphFilterOptions.IsRelationshipVisible(edge.Desc)
                               select edge;

                graph.JSONGraph.Edges = new Collection<GuanxiUI.Edge>(newEdges.ToList<GuanxiUI.Edge>());
            }
        }

        /// <summary>
        /// Shows the visual explorer graph.
        /// This graph will be filtered based on the filter options set by the user.
        /// </summary>
        /// <param name="graph">Visual eplorer graph to dislay</param>
        private void ShowVisualExplorerGraph(VisualExplorerGraph graph)
        {
            try
            {
                if (graph != null)
                {
                    this.MasterGraph = this.DeepCopy(graph);
                    this.FilterVisualExplorerGraph(graph);
                    if (this.GraphFilterOptions != null &&
                        this.GraphFilterOptions.Resources != null && this.GraphFilterOptions.Resources.Count > 0)
                    {
                        int visibleResources = this.GraphFilterOptions.Resources.Count(resource => resource.IsVisible == true);
                        if (visibleResources.Equals(0))
                        {
                            MainPage.Instance.noResultPopUpWindow.txtMessage.Text = Messages.NoResourcesSelected;
                            MainPage.Instance.noResultPopUpWindow.Visibility = Visibility.Visible;
                            MainPage.Instance.noResultPopUpWindow.btnClose.Focus();
                            return;
                        }
                    }

                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GuanxiUI.JSONGraph));
                    MemoryStream stream = new MemoryStream();
                    serializer.WriteObject(stream, graph.JSONGraph);
                    string graphString = Encoding.UTF8.GetString(stream.ToArray(), 0, stream.ToArray().Length);
                    MainPage.Instance.guanxiMap.scaleMode = "exactFit";
                    MainPage.Instance.guanxiMap.parseGraphString(graphString);
                }
                else
                {
                    MainPage.Instance.noResultPopUpWindow.txtMessage.Text = Messages.NoResults;
                    MainPage.Instance.noResultPopUpWindow.Visibility = Visibility.Visible;
                    MainPage.Instance.noResultPopUpWindow.btnClose.Focus();
                }
            }
            catch
            {
                MainPage.Instance.noResultPopUpWindow.txtMessage.Text = string.Format("{0}", Messages.ErrorOccured);
                MainPage.Instance.noResultPopUpWindow.Visibility = Visibility.Visible;
                MainPage.Instance.noResultPopUpWindow.btnClose.Focus();
            }
            finally
            {
                MainPage.Instance.busyIndicator.IsBusy = false;
                MainPage.Instance.busyIndicator.Visibility = Visibility.Collapsed;
                this.searchInProgress = false;
            }
        }

        /// <summary>
        /// Creates a deep copy of the visual graph.
        /// </summary>
        /// <param name="graph">Graph to be copied.</param>
        /// <returns>Cloned graph.</returns>
        private VisualExplorerGraph DeepCopy(VisualExplorerGraph graph)
        {
            if (graph == null)
            {
                return null;
            }

            VisualExplorerGraph clonedGraph = null;
            DataContractSerializer ds = new DataContractSerializer(graph.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                ds.WriteObject(ms, graph);
                ms.Position = 0;
                clonedGraph = (VisualExplorerGraph)ds.ReadObject(ms);
            }

            return clonedGraph;
        }

        /// <summary>
        /// Creates a deep copy of the visual explorer graph filter options.
        /// </summary>
        /// <param name="filter">Filter options to be copied.</param>
        /// <returns>Cloned filter options.</returns>
        private VisualExplorerGraphFilterOptions DeepCopy(VisualExplorerGraphFilterOptions filter)
        {
            if (filter == null)
            {
                return null;
            }

            VisualExplorerGraphFilterOptions clonedFilter = null;
            XmlSerializer serializer = new XmlSerializer(filter.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Serialize(ms, filter);
                ms.Position = 0;
                clonedFilter = (VisualExplorerGraphFilterOptions)serializer.Deserialize(ms);
            }

            return clonedFilter;
        }
    }

    /// <summary>
    /// Color converter class
    /// </summary>
    public class ColorConverter : IValueConverter
    {
        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target</param>
        /// <param name="targetType">The System.Type of data expected by the target dependency property</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic</param>
        /// <param name="culture">The culture of the conversion</param>
        /// <returns>The value to be passed to the target dependency property</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string color = value.ToString();
            byte a = System.Convert.ToByte("FF", 16);
            byte r = System.Convert.ToByte(color.Substring(0, 2), 16);
            byte g = System.Convert.ToByte(color.Substring(2, 2), 16);
            byte b = System.Convert.ToByte(color.Substring(4, 2), 16);
            return new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object. This method
        /// is called only in System.Windows.Data.BindingMode.TwoWay bindings.
        /// </summary>
        /// <param name="value">The target data being passed to the source</param>
        /// <param name="targetType">The System.Type of data expected by the source object</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic</param>
        /// <param name="culture">The culture of the conversion</param>
        /// <returns>The value to be passed to the source object</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((SolidColorBrush)value).Color.ToString().Substring(3, 6);
        }
    }
}
