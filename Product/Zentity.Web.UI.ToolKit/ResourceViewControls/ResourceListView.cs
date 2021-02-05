// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.Design;
using System.Text;
using System.Web.UI.WebControls;
using Zentity.Core;
using Zentity.ScholarlyWorks;
using Zentity.Web.UI.ToolKit.Resources;
using System.Web.UI.HtmlControls;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This control displays input resources in the form of List.
    /// </summary>
    /// <example>
    ///     The code below is the source for ResourceListView.aspx. It shows an example of using 
    ///     <see cref="ResourceListView"/> control.
    ///     <code>
    ///         &lt;%@ Page Language="C#" AutoEventWireup="true" %&gt;
    ///         
    ///         &lt;%@ Register Assembly="Zentity.Web.UI.ToolKit" Namespace="Zentity.Web.UI.ToolKit"
    ///             TagPrefix="zentity" %&gt;
    ///         &lt;%@ Import Namespace="System.Collections.Generic" %&gt;
    ///         &lt;%@ Import Namespace="System.Linq" %&gt;
    ///         &lt;%@ Import Namespace="Zentity.Core" %&gt;
    ///         &lt;%@ Import Namespace="Zentity.ScholarlyWorks" %&gt;
    ///         &lt;%@ Assembly Name="System.Data.Entity" %&gt;
    ///         &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"&gt;
    ///         &lt;html xmlns="http://www.w3.org/1999/xhtml"&gt;
    ///         &lt;head runat="server"&gt;
    ///             &lt;title&gt;ResourceListView Sample&lt;/title&gt;
    ///              
    ///             &lt;script runat="server"&gt;
    ///                 protected void Page_Load(object sender, EventArgs e)
    ///                 {
    ///                     if (!Page.IsPostBack)
    ///                     {
    ///                         RefreshDataSource(0);
    ///                     }
    ///                 }
    ///                 
    ///                 protected void ResourceListView_PageChanged(object sender, EventArgs e)
    ///                 {
    ///                     RefreshDataSource(ResourceListView1.PageIndex);
    ///                 }
    ///                 
    ///                 protected void ResourceListView_SortChanged(Object sender, EventArgs e)
    ///                 {
    ///                     RefreshDataSource(ResourceListView1.PageIndex);
    ///                 }
    ///                 
    ///                 private void RefreshDataSource(int pageIndex)
    ///                 {
    ///                     using (ZentityContext context = new ZentityContext())
    ///                     {
    ///                         context.MetadataWorkspace.LoadFromAssembly(typeof(ScholarlyWork).Assembly);
    ///                         IEnumerable&lt;Resource&gt; resources = context.Resources;
    ///                         ResourceListView1.TotalRecords = resources.Count();
    ///                         
    ///                         if (ResourceListView1.SortExpression == "Title")
    ///                         {
    ///                             if (ResourceListView1.SortDirection == SortDirection.Ascending)
    ///                             {
    ///                                 resources = resources.OrderBy(res => res.Title);
    ///                             }
    ///                             else
    ///                             {
    ///                                 resources = resources.OrderByDescending(res => res.Title);
    ///                             }
    ///                         }
    ///                         else
    ///                         {
    ///                             if (ResourceListView1.SortDirection == SortDirection.Ascending)
    ///                             {
    ///                                 resources = resources.OrderBy(res => res.DateAdded);
    ///                             }
    ///                             else
    ///                             {
    ///                                 resources = resources.OrderByDescending(res => res.DateAdded);
    ///                             }
    ///                         }
    ///                         
    ///                         List&lt;Resource&gt; pagedResources = resources
    ///                             .Skip(pageIndex * ResourceListView1.PageSize)
    ///                             .Take(ResourceListView1.PageSize)
    ///                             .ToList();
    ///                             
    ///                           foreach (Resource resource in pagedResources)
    ///                                 ResourceListView1.DataSource.Add(resource);
    ///                         ResourceListView1.DataBind();
    ///                     }
    ///                 }
    ///              &lt;/script&gt;
    ///              
    ///         &lt;/head&gt;
    ///         &lt;body&gt;
    ///             &lt;form id="form1" runat="server"&gt;
    ///             &lt;div&gt;
    ///                 &lt;zentity:ResourceListView ID="ResourceListView1" runat="server" PageSize="5"
    ///                     ViewUrl="ResourceDetailView.aspx?Id={0}" OnOnPageChanged="ResourceListView_PageChanged"
    ///                     EnableDeleteOption="true" OnOnSortButtonClicked="ResourceListView_SortChanged"&gt;
    ///                 &lt;/zentity:ResourceListView&gt;
    ///             &lt;/div&gt;
    ///             &lt;/form&gt;
    ///         &lt;/body&gt;
    ///         &lt;/html&gt;
    ///     </code>
    /// </example>
    [DefaultProperty("Text")]
    public class ResourceListView : WebControl, INamingContainer
    {
        #region Member Variables

        private Table _titleTable;
        private Panel _HeaderPanel;
        private Panel _ItemPanel;
        private Panel _FooterPanel;
        private Pager _pagerControl;
        private Label _errorMsgLabel;
        private Label _titleLabel;
        private ImageButton _rssFeedButton;
        private ImageButton _atomFeedButton;
        private Button _deleteButton;
        private Label _totalRecordsMsgLabel;
        private Table _resourceListViewTable;
        private CheckBox _deleteAllCheckBox;
        private LinkButton _titleButton;
        private LinkButton _dateAddedButton;

        private TableItemStyle _buttonStyle;
        private TableItemStyle _titleStyle;
        private TableItemStyle _separatorStyle;
        private TableItemStyle _itemStyle;
        private TableItemStyle _alternateItemStyle;
        private TableItemStyle _containerStyle;
        private List<Resource> _dataSource = new List<Resource>();
        #endregion

        #region constants

        private const string _sortDirectionViewStateKey = "SortDirection";
        private const string _sortExpressionViewStateKey = "SortExpression";
        private const string _dataSourceKey = "dataSourceKey";
        private const string _userPermissionsKey = "userPermissionsKey";
        private const string _viewUrlKey = "viewUrlKey";
        private const string _totalRecordsKey = "totalRecordsKey";
        private const string _pageSizeKey = "pageSizeKey";
        private const string _enableDeleteOptionKey = "enableDeleteOptionKey";
        private const string _showHeaderKey = "showHeaderKey";
        private const string _showFooterKey = "showFooterKey";
        private const string _titleKey = "titleKey";
        private const string _showDateKey = "showDateKey";

        private const string _pagerId = "pagerId";
        private const string _footerPanelId = "pagerContainerId";
        private const string _itemPanelId = "resourceViewContainerId";
        private const string _headerPanelId = "sortingContainerId";
        private const string _titleTableId = "titleTableId";
        private const string _titleButtonId = "titleButtonId";
        private const string _titleLabelCellId = "titleLabelCellId";
        private const string _rssButtonCellId = "rssButtonCellId";
        private const string _atomButtonCellId = "atomButtonCellId";

        private const string _dateAddedButtonId = "dateAddedButtonId";
        private const string _errorMessageLabelId = "ErrorMessageLabelId";
        private const string _deleteAllCheckBoxId = "deleteAllCheckBoxId";
        private const string _deleteCheckBoxId = "deleteCheckBoxId";
        private const string _resourceViewId = "resourceViewId";
        private const string _deleteButtonId = "deleteButtonId";
        private const string _totalRecordsMsgLabelId = "totalRecordsMsgLabelId";
        private const string _resourceListViewTableId = "resourceListViewTableId";

        private const string _javascriptFile = "JavascriptFile";
        private const string _searchControlScriptPath = "Zentity.Web.UI.ToolKit.ResourceViewControls.ResourceViewScript.js";
        private const string _validateDeleteCheckBoxSelection = "javascript: return ValidateDeleteCheckBoxSelection('{0}','{1}', '{2}');";
        private const string _selectDeselectAll = "javascript: SelectDeselectAll('{0}','{1}');";
        private const string _selectDeselectHeaderCheckBox = "javascript: SelectDeselectHeaderCheckBox('{0}','{1}');";
        private const string _onClickEvent = "OnClick";

        private const string _titleColumn = "Title";
        private const string _dateAddedColumn = "DateAdded";
        private const string _seperator = " / ";
        private const string _colan = ": ";

        private const string _rssEnabledImagePath = "Zentity.Web.UI.ToolKit.Image.RSS.png";
        private const string _rssDisabledImagePath = "Zentity.Web.UI.ToolKit.Image.RSSDisabled.png";
        private const string _atomEnabledImagePath = "Zentity.Web.UI.ToolKit.Image.Atom.png";
        private const string _atomDisabledImagePath = "Zentity.Web.UI.ToolKit.Image.AtomDisabled.png";
        private const string _syndicationUrlPart = "/Syndication/Syndication.ashx";
        private const string _atomUrlPart = "/atom/";
        private const string _rssUrlPart = "/rss/";
        private const string _atom = "Atom";
        private const string _rss = "RSS";
        private const string _questionMark = "?";
        private const string _searchString = "SearchString";
        private const string _title = "title";
        private const string _feedTitle = "Zentity {0} feed for the search criteria selected on this page";
        private const string _rel = "rel";
        private const string _relValue = "alternate";
        private const string _type = "type";
        private const string _rssLinkType = "application/rss+xml";
        private const string _atomLinkType = "application/atom+xml";
        private const string _rssLinkId = "RSSLinkId";
        private const string _atomLinkId = "AtomLinkId";
        private const string _isFeedSupported = "IsFeedSupported";

        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets title of the control.
        /// </summary>
        [ZentityCategory("CategoryAppearance")]
        [ZentityDescription("DescriptionResourceListViewTitle")]
        public String Title
        {
            get { return ViewState[_titleKey] != null ? (string)ViewState[_titleKey] : Resources.GlobalResource.SearchResultTitleText; }
            set { ViewState[_titleKey] = value; }
        }

        /// <summary>
        /// Gets or sets data source.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceListViewDataSource")]
        public IList<Resource> DataSource
        {
            get
            {
                return _dataSource != null ? _dataSource as IList<Resource> : null;
            }
        }

        /// <summary>
        /// Gets or sets current page index.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceListViewPageIndex")]
        public int PageIndex
        {
            get { return PagerControl.PageIndex; }
            set
            {
                if (value < 0)
                    throw new ArgumentException(GlobalResource.MsgPageIndexLessThanZero);

                PagerControl.PageIndex = value;
            }
        }

        /// <summary>
        /// Gets or sets total records.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceListViewTotalRecords")]
        public int TotalRecords
        {
            get { return ViewState[_totalRecordsKey] != null ? (int)ViewState[_totalRecordsKey] : 0; }
            set
            {
                if (value < 0)
                    throw new ArgumentException(GlobalResource.MsgTotalRecordLessThanZero);

                ViewState[_totalRecordsKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets page size.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceListViewPageSize")]
        public int PageSize
        {
            get { return ViewState[_pageSizeKey] != null ? (int)ViewState[_pageSizeKey] : 0; }
            set
            {
                if (value < 0)
                    throw new ArgumentException(GlobalResource.MsgPageSizeLessThanZero);

                ViewState[_pageSizeKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets view link url. Url format should be Url?Id={0}. {0} will be replaced by actual entity id.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceListViewViewUrl")]
        public string ViewUrl
        {
            get
            {
                return ViewState[_viewUrlKey] != null ? (string)ViewState[_viewUrlKey] : string.Empty;
            }
            set
            {
                ViewState[_viewUrlKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets flag to enable or disable delete option.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceListViewEnableDeleteOption")]
        public bool EnableDeleteOption
        {
            get { return ViewState[_enableDeleteOptionKey] != null ? (bool)ViewState[_enableDeleteOptionKey] : false; }
            set { ViewState[_enableDeleteOptionKey] = value; }
        }

        /// <summary>
        /// Gets or sets visibility of header.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceListViewShowHeader")]
        public bool ShowHeader
        {
            get { return ViewState[_showHeaderKey] != null ? (bool)ViewState[_showHeaderKey] : true; }
            set { ViewState[_showHeaderKey] = value; }
        }

        /// <summary>
        /// Gets or sets visibility of footer.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceListViewShowFooter")]
        public bool ShowFooter
        {
            get { return ViewState[_showFooterKey] != null ? (bool)ViewState[_showFooterKey] : true; }
            set { ViewState[_showFooterKey] = value; }
        }

        /// <summary>
        /// Gets or sets sort direction.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceListViewSortDirection")]
        public SortDirection SortDirection
        {
            get
            {
                return ViewState[_sortDirectionViewStateKey] != null ?
                    (SortDirection)ViewState[_sortDirectionViewStateKey] : SortDirection.Ascending;
            }
            set
            {
                ViewState[_sortDirectionViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets sort expression.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceListViewSortExpression")]
        public string SortExpression
        {
            get
            {
                return ViewState[_sortExpressionViewStateKey] != null ?
                  (string)ViewState[_sortExpressionViewStateKey] : _dateAddedColumn;
            }
            set
            {
                ViewState[_sortExpressionViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets type of date to be displayed.
        /// </summary>
        public DateType ShowDate
        {
            get { return ViewState[_showDateKey] != null ? (DateType)ViewState[_showDateKey] : DateType.DateAdded; }
            set { ViewState[_showDateKey] = value; }
        }

        /// <summary>
        /// Gets or sets a list of user permissions.
        /// </summary>
        public IDictionary<Guid, IEnumerable<string>> UserPermissions
        {
            get { return ViewState[_userPermissionsKey] != null ? ViewState[_userPermissionsKey] as Dictionary<Guid, IEnumerable<string>> : null; }
            set { ViewState[_userPermissionsKey] = value; }
        }

        private string SearchString
        {
            get { return ViewState[_searchString] != null ? ViewState[_searchString] as string : string.Empty; }
            set { ViewState[_searchString] = value; }
        }

        /// <summary>
        /// Property which determines whether this control should support feed buttons
        /// </summary>
        /// <remarks>Feed is supported for queries which use AQL for getting results from the search module.</remarks>
        public bool IsFeedSupported
        {
            get 
            {
                if (ViewState[_isFeedSupported] == null)
                {
                    return false;
                }
                return (bool)ViewState[_isFeedSupported]; 
            }
            set { ViewState[_isFeedSupported] = value; }
        }

        #region Style Properties

        /// <summary>
        /// Gets or sets style to be applied to the buttons.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionResourceListViewButtonStyle"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ButtonStyle
        {
            get
            {
                if (this._buttonStyle == null)
                {
                    this._buttonStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._buttonStyle).TrackViewState();
                    }
                }
                return this._buttonStyle;
            }
        }

        /// <summary>
        /// Gets or sets style to be applied to the title.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionResourceListViewTitleStyle"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle TitleStyle
        {
            get
            {
                if (this._titleStyle == null)
                {
                    this._titleStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._titleStyle).TrackViewState();
                    }
                }
                return this._titleStyle;
            }
        }

        /// <summary>
        /// Gets or sets style to be applied to the Separator that separates a record.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionResourceListViewSeparatorStyle"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle SeparatorStyle
        {
            get
            {
                if (this._separatorStyle == null)
                {
                    this._separatorStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._separatorStyle).TrackViewState();
                    }
                }
                return this._separatorStyle;
            }
        }

        /// <summary>
        /// Gets or sets style to be applied to the All Items.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionResourceListViewItemStyle"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ItemStyle
        {
            get
            {
                if (this._itemStyle == null)
                {
                    this._itemStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._itemStyle).TrackViewState();
                    }
                }
                return this._itemStyle;
            }
        }

        /// <summary>
        /// Gets or sets style to be applied to the alternate resource items.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionResourceListViewAlternateItemStyle"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle AlternateItemStyle
        {
            get
            {
                if (this._alternateItemStyle == null)
                {
                    this._alternateItemStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._alternateItemStyle).TrackViewState();
                    }
                }
                return this._alternateItemStyle;
            }
        }

        /// <summary>
        /// Gets or sets style to be applied to this control.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionResourceListViewContainerStyle"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ContainerStyle
        {
            get
            {
                if (this._containerStyle == null)
                {
                    this._containerStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._containerStyle).TrackViewState();
                    }
                }
                return this._containerStyle;
            }
        }

        #endregion

        #endregion

        #region private

        private Table TitleTable
        {
            get
            {
                if (_titleTable == null)
                {
                    _titleTable = new Table();
                    _titleTable.Width = Unit.Percentage(100);
                    _titleTable.ID = _titleTableId;
                    _titleTable.Rows.Add(new TableRow());
                    AddTitleLabel();
                    if (IsFeedSupported)
                    {
                        AddFeedButtons();
                    }
                }
                return _titleTable;
            }
        }

        private Label TitleLabel
        {
            get
            {
                if (_titleLabel == null)
                {
                    _titleLabel = new Label();
                    _titleLabel.Font.Bold = true;
                    _titleLabel.Text = this.Title;
                }
                return _titleLabel;

            }
        }

        /// <summary>
        /// The image button for feed. It is up to the client code for this control to determine the action
        /// to be taken for the feed button click. 
        /// If a client page does not support feed it may disable this button or set its visibility to false. 
        /// </summary>
        private ImageButton RssFeedButton
        {
            get
            {
                if (_rssFeedButton == null)
                {
                    _rssFeedButton = new ImageButton();
                    _rssFeedButton.Attributes.Add("align", "right");
                    _rssFeedButton.Attributes.Add("padding-right", "1");
                    _rssFeedButton.Enabled = false; //Calling EnableFeed with search string as parameter will enable this button.
                    _rssFeedButton.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), _rssDisabledImagePath);
                    _rssFeedButton.ToolTip = GlobalResource.RSSTooltip;
                    _rssFeedButton.Click += new ImageClickEventHandler(RssFeedButton_Click);

                }
                return _rssFeedButton;
            }
        }

        private void RssFeedButton_Click(object sender, ImageClickEventArgs e)
        {
            string feedUri = BuildFeedUri(_rssUrlPart);
            this.Page.Response.Redirect(feedUri);
        }

        /// <summary>
        /// The image button for feed. It is up to the client code for this control to determine the action
        /// to be taken for the feed button click. 
        /// If a client page does not support feed it may disable this button or set its visibility to false. 
        /// </summary>
        private ImageButton AtomFeedButton
        {
            get
            {
                if (_atomFeedButton == null)
                {
                    _atomFeedButton = new ImageButton();
                    _atomFeedButton.Attributes.Add("align", "right");
                    _atomFeedButton.Attributes.Add("padding-right", "1");
                    _atomFeedButton.Enabled = false; //Calling EnableFeed with search string as parameter will enable this button.
                    _atomFeedButton.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), _atomDisabledImagePath);
                    _atomFeedButton.ToolTip = GlobalResource.AtomTooltip;
                    _atomFeedButton.Click += new ImageClickEventHandler(AtomFeedButton_Click);
                }
                return _atomFeedButton;
            }
        }

        private void AtomFeedButton_Click(object sender, ImageClickEventArgs e)
        {
            string feedUri = BuildFeedUri(_atomUrlPart);
            this.Page.Response.Redirect(feedUri);
        }

        private Panel ItemPanel
        {
            get
            {
                if (_ItemPanel == null)
                {
                    _ItemPanel = new Panel();
                    _ItemPanel.ID = _itemPanelId;
                    _ItemPanel.Style.Add(HtmlTextWriterStyle.Margin, "5px");
                    _ItemPanel.HorizontalAlign = HorizontalAlign.Left;
                    _ItemPanel.Controls.Add(ErrorMessageLabel);
                }

                return _ItemPanel;
            }
        }

        private Panel FooterPanel
        {
            get
            {
                if (_FooterPanel == null)
                {
                    _FooterPanel = new Panel();
                    _FooterPanel.Style.Add(HtmlTextWriterStyle.Margin, "5px");
                    _FooterPanel.ID = _footerPanelId;
                    _FooterPanel.HorizontalAlign = HorizontalAlign.Left;

                    Table table = new Table();
                    table.Width = Unit.Percentage(100);

                    TableRow tr = new TableRow();

                    TableCell td = new TableCell();
                    td.Width = Unit.Percentage(33);
                    td.HorizontalAlign = HorizontalAlign.Left;
                    td.Controls.Add(DeleteButton);
                    tr.Cells.Add(td);

                    td = new TableCell();
                    td.Width = Unit.Percentage(33);
                    td.HorizontalAlign = HorizontalAlign.Center;
                    td.Controls.Add(PagerControl);
                    tr.Cells.Add(td);

                    td = new TableCell();
                    td.HorizontalAlign = HorizontalAlign.Right;
                    td.Controls.Add(TotalRecordsMessageLabel);
                    tr.Cells.Add(td);

                    table.Rows.Add(tr);

                    _FooterPanel.Controls.Add(table);
                }

                return _FooterPanel;
            }
        }

        private Pager PagerControl
        {
            get
            {
                if (_pagerControl == null)
                {
                    _pagerControl = new Pager();
                    _pagerControl.ID = _pagerId;
                    _pagerControl.OnPageChanged += new EventHandler<EventArgs>(_pagerControl_OnPageChanged);
                }
                return _pagerControl;
            }
        }

        private Button DeleteButton
        {
            get
            {
                if (_deleteButton == null)
                {
                    _deleteButton = new Button();
                    _deleteButton.ID = _deleteButtonId;
                    _deleteButton.Text = GlobalResource.ButtonDeleteText;
                    _deleteButton.Click += new EventHandler(_deleteButton_Click);

                }

                return _deleteButton;
            }
        }

        private Panel HeaderPanel
        {
            get
            {
                if (_HeaderPanel == null)
                {
                    _HeaderPanel = new Panel();
                    _HeaderPanel.Style.Add(HtmlTextWriterStyle.Margin, "5px");
                    _HeaderPanel.ID = _headerPanelId;

                    Table table = new Table();
                    table.Width = Unit.Percentage(100);

                    TableRow tr = new TableRow();

                    TableCell titleTD = new TableCell();
                    titleTD.HorizontalAlign = HorizontalAlign.Left;
                    titleTD.Controls.Add(DeleteAllCheckBox);

                    tr.Cells.Add(titleTD);

                    TableCell sortingTD = new TableCell();
                    sortingTD.HorizontalAlign = HorizontalAlign.Right;
                    Label sortByLabel = new Label();
                    sortByLabel.Text = GlobalResource.SortByLabelText;
                    sortByLabel.Font.Bold = true;
                    sortingTD.Controls.Add(sortByLabel);

                    sortingTD.Controls.Add(TitleButton);

                    Label seperator = new Label();
                    seperator.Text = _seperator;
                    sortingTD.Controls.Add(seperator);

                    sortingTD.Controls.Add(DateAddedButton);

                    tr.Cells.Add(sortingTD);

                    table.Rows.Add(tr);

                    _HeaderPanel.Controls.Add(table);
                }

                return _HeaderPanel;
            }
        }

        private CheckBox DeleteAllCheckBox
        {
            get
            {
                if (_deleteAllCheckBox == null)
                {
                    _deleteAllCheckBox = new CheckBox();
                    _deleteAllCheckBox.ID = _deleteAllCheckBoxId;
                    _deleteAllCheckBox.Text = GlobalResource.SelectDeselectLabelText;
                    _deleteAllCheckBox.EnableViewState = false;
                    _deleteAllCheckBox.Checked = false;
                }
                return _deleteAllCheckBox;
            }
        }

        private Label ErrorMessageLabel
        {
            get
            {
                if (_errorMsgLabel == null)
                {
                    _errorMsgLabel = new Label();
                    _errorMsgLabel.ID = _errorMessageLabelId;
                    _errorMsgLabel.EnableViewState = false;
                }

                return _errorMsgLabel;
            }
        }

        private Label TotalRecordsMessageLabel
        {
            get
            {
                if (_totalRecordsMsgLabel == null)
                {
                    _totalRecordsMsgLabel = new Label();
                    _totalRecordsMsgLabel.ID = _totalRecordsMsgLabelId;
                    _totalRecordsMsgLabel.EnableViewState = false;
                }

                return _totalRecordsMsgLabel;
            }
        }

        private Table ResourceListViewTable
        {
            get
            {
                if (_resourceListViewTable == null)
                {
                    _resourceListViewTable = new Table();
                    _resourceListViewTable.ID = _resourceListViewTableId;
                    _resourceListViewTable.Width = Unit.Percentage(100);
                }

                return _resourceListViewTable;
            }
        }

        private LinkButton TitleButton
        {
            get
            {
                if (_titleButton == null)
                {
                    _titleButton = new LinkButton();
                    _titleButton.ID = _titleButtonId;
                    _titleButton.Text = GlobalResource.TitleText;
                    _titleButton.Click += new EventHandler(titleButton_Click);
                }

                return _titleButton;
            }
        }

        private LinkButton DateAddedButton
        {
            get
            {
                if (_dateAddedButton == null)
                {
                    _dateAddedButton = new LinkButton();
                    _dateAddedButton.ID = _dateAddedButtonId;
                    _dateAddedButton.Text = GlobalResource.DateAddedText;
                    _dateAddedButton.Click += new EventHandler(dateAddedButton_Click);
                }

                return _dateAddedButton;
            }
        }

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// This event is fired when current page index changes.
        /// </summary>
        public event EventHandler<EventArgs> OnPageChanged;
        /// <summary>
        /// This event is fired when Delete button is clicked.
        /// </summary>
        public event EventHandler<DeleteEventArgs> OnDeleteButtonClicked;

        /// <summary>
        /// This event is fired when sort buttons are clicked.
        /// </summary>
        public event EventHandler<EventArgs> OnSortButtonClicked;

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Bind data source to child controls.
        /// </summary>
        public override void DataBind()
        {
            Clear();

            if (DataSource != null && DataSource.Count > 0)
            {
                int index = 0;


                //Enable or Disable delete option
                DeleteAllCheckBox.Visible = DeleteButton.Visible = EnableDeleteOption;

                //Add resource view for each resource in the data source.
                foreach (Resource res in DataSource)
                {
                    TableRow tr = new TableRow();

                    //Add delete checkbox for each resource.
                    TableCell td = new TableCell();
                    td.VerticalAlign = VerticalAlign.Top;
                    CheckBox deletechkBox = new CheckBox();
                    deletechkBox.ID = _deleteCheckBoxId + index.ToString(CultureInfo.InvariantCulture);
                    deletechkBox.Visible = EnableDeleteOption;
                    deletechkBox.EnableViewState = false;
                    td.Controls.Add(deletechkBox);
                    tr.Cells.Add(td);

                    //Add ResourceView control for each resource.
                    td = new TableCell();
                    ResourceView resView = new ResourceView();
                    resView.ID = _resourceViewId + index.ToString(CultureInfo.InvariantCulture);
                    resView.DataSource = res;
                    resView.UserPermissions = this.UserPermissions;
                    resView.TitleLink = ViewUrl;
                    resView.AuthorsLink = ViewUrl;
                    resView.FilesLink = ViewUrl;
                    resView.ShowDate = this.ShowDate;
                    resView.DataBind();
                    td.Controls.Add(resView);
                    tr.Cells.Add(td);
                    ResourceListViewTable.Rows.Add(tr);

                    //Add separator.
                    if (index < (DataSource.Count - 1))
                    {
                        tr = new TableRow();
                        td = new TableCell();
                        td.ColumnSpan = 2;
                        Panel separator = new Panel();
                        separator.ApplyStyle(this.SeparatorStyle);
                        td.Controls.Add(separator);
                        tr.Cells.Add(td);
                        ResourceListViewTable.Rows.Add(tr);
                    }

                    index++;
                }

                ItemPanel.Controls.Add(ResourceListViewTable);
            }
            else
            {
                if (this.TotalRecords > 0)
                {
                    ErrorMessageLabel.Text = Resources.GlobalResource.MessageRefreshPage;
                }
                else
                {
                    ErrorMessageLabel.Text = Resources.GlobalResource.MessageNoRecordFound;
                }
            }
        }

        /// <summary>
        /// Sorts data source based on sort expression and sort direction.
        /// </summary>
        /// <param name="sortDirection">Sort direction.</param>
        /// <param name="sortExpression">Sort expression.</param>
        public void SortDataSource(SortDirection sortDirection, string sortExpression)
        {
            if (this.DataSource != null)
            {
                this.SortDirection = sortDirection;
                this.SortExpression = sortExpression;
                this.SortDataSource();
            }

        }

        /// <summary>
        /// Enables feed buttons for the control. 
        /// </summary>
        /// <param name="searchCriteria">Search criteria in the AQL supported by Zentity Search module. e.g. author:Tony</param>
        public void EnableFeed(string searchCriteria)
        {
            if (!IsFeedSupported)
            {
                throw new NotSupportedException(GlobalResource.SetIsFeedSupportedMessage);
            }
            if (string.IsNullOrEmpty(searchCriteria))
            {
                throw new ArgumentNullException("searchCriteria");
            }
            this.SearchString = searchCriteria;
            _rssFeedButton.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), _rssEnabledImagePath);
            _rssFeedButton.Enabled = true;
            _atomFeedButton.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), _atomEnabledImagePath);
            _atomFeedButton.Enabled = true;
            EnableBrowserFeed();
        }



        /// <summary>
        /// Disables feed buttons. 
        /// </summary>
        public void DisableFeed()
        {
            _rssFeedButton.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), _rssDisabledImagePath);
            _rssFeedButton.Enabled = false;
            _atomFeedButton.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), _atomDisabledImagePath);
            _atomFeedButton.Enabled = false;
            DisableBrowserFeed();
        }



        #endregion

        #region Protected

        /// <summary>
        /// Sets design time data source to this control.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
            // Register javascript for ResourceListView control
            this.Page.ClientScript.RegisterClientScriptInclude(
                this.GetType(),
                _javascriptFile,
                Page.ClientScript.GetWebResourceUrl(this.GetType(), _searchControlScriptPath));

            //Set design time data source.
            if (DesignMode)
            {
                IList<Resource> dataSource = null;
                dataSource = GetDesignTimeDataSource();

                DataSource.Clear();
                foreach (Resource resource in dataSource)
                    DataSource.Add(resource);

                this.DataBind();
            }

            base.OnLoad(e);
            if (IsFeedSupported)
            {
                DisableFeed();
            }
        }

        /// <summary>
        /// Creates Child controls layout.
        /// </summary>
        /// <param name="e">Event Arguments.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //Add child controls
            this.Controls.Add(TitleTable);
            if (ShowHeader)
                this.Controls.Add(HeaderPanel);
            this.Controls.Add(ItemPanel);
            if (ShowFooter)
                this.Controls.Add(FooterPanel);
            this.Controls.Add(ErrorMessageLabel);

            if (DesignMode)
            {
                IList<Resource> dataSource = null;
                dataSource = GetDesignTimeDataSource();

                DataSource.Clear();
                foreach (Resource resource in dataSource)
                    DataSource.Add(resource);

                this.DataBind();
            }

        }

        /// <summary>
        /// Bind data source saved in a view state.
        /// </summary>
        /// <param name="savedState">Saved state.</param>
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);

            if (this.DataSource != null)
                this.DataBind();
        }

        /// <summary>
        /// Set visibility status of the controls.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
            //Enable or disable delete CheckBox on each row based on user permissions on 
            //the resource displayed in corresponding row.
            SetDeleteCheckBoxStatus();

            if (DataSource == null || DataSource.Count == 0)
            {
                TotalRecords = 0;
            }

            //Set Total pages of Pager control.
            if (PageSize > 0 && TotalRecords > 0)
            {
                if (TotalRecords > PageSize)
                    PagerControl.TotalPages = Convert.ToInt32(Math.Ceiling((double)this.TotalRecords / this.PageSize));
                else
                {
                    PagerControl.PageIndex = 0;
                    PagerControl.TotalPages = 1;
                }
            }
            else
            {
                PagerControl.PageIndex = 0;
                PagerControl.TotalPages = 0;
            }

            //Set Total Records Message.
            TotalRecordsMessageLabel.Text = Resources.GlobalResource.TotalRecordCount + _colan + TotalRecords;

            if (DataSource == null || DataSource.Count == 0)
            {
                ItemPanel.Visible = false;
                FooterPanel.Visible = false;
                HeaderPanel.Visible = false;
            }
            else
            {
                ItemPanel.Visible = true;
                if (ShowHeader)
                    HeaderPanel.Visible = true;
                if (ShowFooter)
                    FooterPanel.Visible = true;

                if (PagerControl.TotalPages <= 1)
                    PagerControl.Visible = false;
                else
                    PagerControl.Visible = true;

            }

            if (EnableDeleteOption)
            {
                RegisterJavascriptFunctions();
            }

            //Set Title
            TitleLabel.Text = this.Title;
            //Apply styles to control.
            ApplyStyles();

            base.OnPreRender(e);
        }

        #endregion

        #region Private

        private void _pagerControl_OnPageChanged(object sender, EventArgs e)
        {
            if (OnPageChanged != null)
                OnPageChanged(sender, e);
        }

        private void dateAddedButton_Click(object sender, EventArgs e)
        {
            if (OnSortButtonClicked != null)
                RaiseSortingEvent(_dateAddedColumn);

            TitleButton.Font.Bold = false;
            DateAddedButton.Font.Bold = true;

        }

        private void titleButton_Click(object sender, EventArgs e)
        {
            if (OnSortButtonClicked != null)
                RaiseSortingEvent(_titleColumn);

            TitleButton.Font.Bold = true;
            DateAddedButton.Font.Bold = false;
        }

        private void _deleteButton_Click(object sender, EventArgs e)
        {
            if (OnDeleteButtonClicked != null)
            {
                Collection<Guid> idCollection = FindItemsToBeDeleted();
                OnDeleteButtonClicked(sender, new DeleteEventArgs(idCollection));
            }
        }

        private Collection<Guid> FindItemsToBeDeleted()
        {
            Collection<Guid> idCollection = new Collection<Guid>();

            if (ResourceListViewTable != null)
            {
                int index = 0;
                foreach (TableRow tr in ResourceListViewTable.Rows)
                {
                    CheckBox deleteCheckBox = tr.FindControl(_deleteCheckBoxId + index.ToString(CultureInfo.InvariantCulture)) as CheckBox;

                    if (deleteCheckBox != null && deleteCheckBox.Checked)
                    {
                        ResourceView resourceView = tr.FindControl(_resourceViewId + index.ToString(CultureInfo.InvariantCulture)) as ResourceView;

                        if (resourceView != null && resourceView.DataSource != null)
                        {
                            idCollection.Add(resourceView.DataSource.Id);
                        }
                    }
                    index++;
                }
            }

            return idCollection;
        }

        private void RaiseSortingEvent(string sortExpression)
        {
            if (!string.IsNullOrEmpty(sortExpression) || this.DataSource == null || this.DataSource.Count == 0)
            {
                if (string.IsNullOrEmpty(this.SortExpression) || sortExpression != this.SortExpression)
                {
                    this.SortExpression = sortExpression;
                    this.SortDirection = SortDirection.Ascending;
                    OnSortButtonClicked(this, new EventArgs());
                }
                else
                {
                    if (this.SortDirection == SortDirection.Ascending)
                    {
                        //SortDataSource();
                        this.SortDirection = SortDirection.Descending;
                        OnSortButtonClicked(this, new EventArgs());
                    }
                    else
                    {
                        this.SortDirection = SortDirection.Ascending;
                        OnSortButtonClicked(this, new EventArgs());
                    }
                }

            }
        }

        private void SortDataSource()
        {
            if (!string.IsNullOrEmpty(this.SortExpression))
            {
                IList<Resource> dataSource = null;
                if (this.SortDirection == SortDirection.Ascending)
                {
                    dataSource = DataSource.OfType<Resource>().OrderBy(
                        zentityType => zentityType.GetType().InvokeMember(
                            this.SortExpression,
                            System.Reflection.BindingFlags.GetProperty, null,
                            zentityType, null, CultureInfo.InvariantCulture)
                        ).ToList();

                    DataSource.Clear();
                    foreach (Resource resource in dataSource)
                        DataSource.Add(resource);
                }
                else
                {
                    dataSource = DataSource.OfType<Resource>().OrderByDescending(
                        zentityType => zentityType.GetType().InvokeMember(
                            this.SortExpression,
                            System.Reflection.BindingFlags.GetProperty, null,
                            zentityType, null, CultureInfo.InvariantCulture)
                        ).ToList();

                    DataSource.Clear();
                    foreach (Resource resource in dataSource)
                        DataSource.Add(resource);
                }
            }
        }

        private static IList<Resource> GetDesignTimeDataSource()
        {
            IList<Resource> resources = new List<Resource>();

            ScholarlyWork res = new ScholarlyWork();
            res.Title = GlobalResource.ResourceTitle1;
            res.Description = GlobalResource.ResourceDescription1;
            res.DateAdded = DateTime.Now;

            Contact contact = new Contact();
            contact.Title = GlobalResource.ResourceContactTitle1;
            res.Authors.Add(contact);

            File file = new File();
            file.Title = GlobalResource.ResourceFileTitle1;
            res.Files.Add(file);

            resources.Add(res);


            ScholarlyWork res1 = new ScholarlyWork();
            res1.Title = GlobalResource.ResourceTitle1;
            res1.Description = GlobalResource.ResourceDescription1;
            res1.DateAdded = DateTime.Now;

            Contact contact1 = new Contact();
            contact1.Title = GlobalResource.ResourceContactTitle1;
            res1.Authors.Add(contact1);

            File file1 = new File();
            file1.Title = GlobalResource.ResourceFileTitle1;
            res1.Files.Add(file1);

            resources.Add(res1);

            return resources;

        }

        private void SetDeleteCheckBoxStatus()
        {
            int index = 0;
            foreach (TableRow tr in ResourceListViewTable.Rows)
            {
                CheckBox deleteChkBox = tr.FindControl(_deleteCheckBoxId + index.ToString(CultureInfo.InvariantCulture)) as CheckBox;
                ResourceView resView = tr.FindControl(_resourceViewId + index.ToString(CultureInfo.InvariantCulture)) as ResourceView;
                Resource res = null;
                if (resView != null)
                {
                    res = resView.DataSource;
                }

                if (deleteChkBox != null && res != null)
                {
                    //If user is having delete permission then enable delete checkbox else disable.
                    if (UserPermissions == null ||
                        (UserPermissions.Keys.Contains(res.Id) && UserPermissions[res.Id].Contains(UserResourcePermissions.Delete)))
                    {
                        deleteChkBox.Enabled = true;
                    }
                    else
                    {
                        deleteChkBox.Enabled = false;
                    }
                }
                index++;
            }

        }

        private void RegisterJavascriptFunctions()
        {
            DeleteAllCheckBox.Attributes.Add(_onClickEvent,
                    string.Format(CultureInfo.InvariantCulture, _selectDeselectAll,
                    ResourceListViewTable.ClientID, _deleteAllCheckBox.ClientID));

            int index = 0;
            foreach (TableRow tr in ResourceListViewTable.Rows)
            {
                CheckBox deleteCheckBox = tr.FindControl(_deleteCheckBoxId + index.ToString(CultureInfo.InvariantCulture)) as CheckBox;

                if (deleteCheckBox != null)
                {
                    deleteCheckBox.Attributes.Add(_onClickEvent,
                        string.Format(CultureInfo.InvariantCulture, _selectDeselectHeaderCheckBox,
                        ResourceListViewTable.ClientID, DeleteAllCheckBox.ClientID));

                }
                index++;
            }

            DeleteButton.Attributes.Add(_onClickEvent,
                      string.Format(CultureInfo.InvariantCulture, _validateDeleteCheckBoxSelection,
                          ResourceListViewTable.ClientID,
                          Resources.GlobalResource.MessageDeleteRecords,
                          Resources.GlobalResource.MessageRecordNotSelected));
        }

        private void Clear()
        {
            ResourceListViewTable.Rows.Clear();
            DeleteAllCheckBox.Checked = false;
            ErrorMessageLabel.Text = string.Empty;
        }

        private void ApplyStyles()
        {
            this.ApplyStyle(ContainerStyle);
            TitleTable.ApplyStyle(TitleStyle);
            ItemPanel.ApplyStyle(ItemStyle);

            int index = 0;
            foreach (TableRow tr in ResourceListViewTable.Rows)
            {
                if (tr.Cells.Count > 1)
                {
                    if (index % 2 == 1)
                        tr.ApplyStyle(AlternateItemStyle);
                    index++;
                }
            }

            DeleteButton.ApplyStyle(ButtonStyle);
        }

        private void AddTitleLabel()
        {
            TableCell titleLabelCell = new TableCell();
            titleLabelCell.ID = _titleLabelCellId;
            titleLabelCell.Width = Unit.Percentage(100);
            titleLabelCell.Controls.Add(TitleLabel);
            _titleTable.Rows[0].Cells.Add(titleLabelCell);
        }

        #region Methods related to Feeds
        private string BuildFeedUri(string feedType)
        {
            System.Text.StringBuilder feedUriBuilder = new System.Text.StringBuilder(this.Page.Request.Url.GetLeftPart(UriPartial.Authority));
            feedUriBuilder.Append(_syndicationUrlPart)
                .Append(feedType)
                .Append(this.PageSize.ToString(CultureInfo.CurrentCulture))
                .Append(_questionMark)
                .Append(this.SearchString);
            return feedUriBuilder.ToString();
        }

        private void AddFeedButtons()
        {
            TableCell rssCell = new TableCell();
            rssCell.ID = _rssButtonCellId;
            rssCell.Width = Unit.Percentage(100);
            rssCell.Controls.Add(RssFeedButton);

            TableCell atomCell = new TableCell();
            atomCell.ID = _atomButtonCellId;
            atomCell.Controls.Add(AtomFeedButton);
            atomCell.Width = Unit.Percentage(100);

            _titleTable.Rows[0].Cells.Add(rssCell);
            _titleTable.Rows[0].Cells.Add(atomCell);
        }

        #region Methods related to browser feed button
        private void EnableBrowserFeed()
        {
            //Add or update RSS and Atom feed links to the browser feed button.
            AddOrUpdateFeedLink(_rssLinkId, _rssLinkType, _rssUrlPart, _rss);
            AddOrUpdateFeedLink(_atomLinkId, _atomLinkType, _atomUrlPart, _atom);
        }

        private void AddOrUpdateFeedLink(string linkId, string linkType, string feedUrlPart, string feedType)
        {
            HtmlLink feedLink = this.Page.Header.Controls.OfType<HtmlLink>().Where(control => control.ID == "RssLinkId").FirstOrDefault();
            if (feedLink == null)
            {
                feedLink = AddFeedLink(linkId, linkType, feedLink);
            }
            //Update URI, title. 
            feedLink.Href = BuildFeedUri(feedUrlPart);
            feedLink.Attributes[_title] = string.Format(CultureInfo.CurrentCulture, _feedTitle, feedType);
            //Enable the link.
            feedLink.Disabled = false;
        }

        private HtmlLink AddFeedLink(string linkId, string linkType, HtmlLink feedLink)
        {
            //Create new link and set static property values (which do not change if search string changes).
            feedLink = new HtmlLink();
            feedLink.Attributes[_rel] = _relValue;
            feedLink.ID = linkId;
            feedLink.Attributes[_type] = linkType;
            //Add the link to the header controls.
            this.Page.Header.Controls.Add(feedLink);
            return feedLink;
        }

        //Disables the links associated with browser feed button.
        private void DisableBrowserFeed()
        {
            IEnumerable<HtmlLink> links = this.Page.Header.Controls.OfType<HtmlLink>();
            if (links.Any())
            {
                links = links.Where(link => !string.IsNullOrEmpty(link.ID)
                    && (link.ID.Equals(_rssLinkId, StringComparison.OrdinalIgnoreCase)
                    || link.ID.Equals(_atomLinkId, StringComparison.OrdinalIgnoreCase)));
                if (links != null)
                {
                    foreach (HtmlLink link in links)
                    {
                        link.Disabled = true;
                    }
                }
            }
        }
        #endregion

        #endregion

        #endregion

        #endregion
    }
}
