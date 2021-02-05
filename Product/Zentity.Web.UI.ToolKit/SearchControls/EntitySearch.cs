// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using System.Web.UI;
using Zentity.Core;
using Zentity.ScholarlyWorks;
using Zentity.Platform;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Configuration;
using System.Collections;
using System.Text;
using Zentity.Security.AuthorizationHelper;
using Zentity.Security.Authorization;
using Zentity.Security.Authentication;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This custom control provide entity search functionality 
    /// based on selection of search criteria.
    /// </summary>
    /// <example>
    ///     The code below is the source for EntitySearch.aspx. It shows an example of using 
    ///     <see cref="EntitySearch"/> control.
    ///     It can be configured to provide search functionality for resource, tag and category
    ///     just by setting the EntityType property.
    ///     <code>
    ///         &lt;%@ Page Language="C#" AutoEventWireup="true" %&gt;
    ///         
    ///         &lt;%@ Register Assembly="Zentity.Web.UI.ToolKit" Namespace="Zentity.Web.UI.ToolKit"
    ///             TagPrefix="zentity" %&gt;
    ///         &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"&gt;
    ///         &lt;html xmlns="http://www.w3.org/1999/xhtml"&gt;
    ///         &lt;head runat="server"&gt;
    ///             &lt;title&gt;EntitySearch Sample&lt;/title&gt;
    ///         &lt;/head&gt;
    ///         &lt;body&gt;
    ///             &lt;form id="form1" runat="server"&gt;
    ///                 &lt;zentity:EntitySearch ID="EntitySearch1" runat="server" EntityType="Resource" 
    ///                     EnableDeleteOption="true" Title="List Resources" ViewUrl="ResourceDetailView.aspx?Id={0}"
    ///                     IsSecurityAwareControl="false"&gt;
    ///                 &lt;/zentity:EntitySearch&gt;
    ///             &lt;/form&gt;
    ///         &lt;/body&gt;
    ///         &lt;/html&gt;
    ///     </code>
    /// </example>
    [Designer(typeof(EntitySearchDesigner))]
    public class EntitySearch : ZentityBase
    {
        #region Member Variables
        Label _mainTitleLabel;
        DropDownList _resourceTypeList;
        Label _rtLabel;
        DropDownList _pageSizeList;
        RadioButton _allRadioButton;
        RadioButton _filterRadioButton;
        Label _psLabel;
        PropertyFilterCriteria _filterCriteria;
        Button _submitButton;
        ResourceListView _resourceListView;
        TableItemStyle _labelStyle;
        TableItemStyle _buttonStyle;
        TableRow _mainTitleRow;
        private int[] _pageSizeRage = new int[4] { 10, 15, 25, 50 };

        private static readonly object _entityTypeChangedEventKey = new object();
        #endregion

        #region Constants

        private const string _mainTitleLabelId = "MainTitleLabelId";
        private const string _resourceTypesId = "ResourceTypes";
        private const string _resourceTypesLabelId = "ResourceTypesLabel";
        private const string _allRadioButtonId = "AllRadioButton";
        private const string _filterRadioButtonId = "FilterRadioButtonId";
        private const string _radioButtonGroupName = "FilterTypesGroup";
        private const string _filterCriteriaId = "FilterCriteria";
        private const string _resourceListViewId = "resourceListViewId";
        private const string _pageSizeListId = "PageSizeList";
        private const string _pageSizeListLabelId = "PageSizeListLabel";
        private const string _submitButtonId = "SubmitButtonID";
        private const string _searchCriteriaViewStateKey = "SearchCriteria";
        private const string _resourceTypeNameField = "Name";
        private const string _resourceTypeNameValueField = "FullName";
        private const string _pageSize10 = "10";
        private const string _pageSize15 = "15";
        private const string _pageSize25 = "25";
        private const string _pageSize50 = "50";
        private const string _propertyTitle = "Title";
        private const string _propertyName = "Name";
        private const string _namespaceZentityCore = "Zentity.Core.";
        private const string _resource = "Resource";
        private const string CategoryUriPrefix = "urn:category:";
        private const string _entityTypeViewStateKey = "EntityTypeViewStateKey";
        private const string _searchResoultTitleKey = "SearchResoultTitleKey";
        private const string _editAssociationUrlViewStateKey = "EditAssociationUrl";
        private const string _javascriptFile = "JavascriptFile";
        private const string _commonScriptPath = "Zentity.Web.UI.ToolKit.Scripts.CommonScript.js";
        private const string _showHideFilterCriteria = "javascript: ShowHideControl('{0}','{1}');";
        private const string _OnClickEvent = "OnClick";
        private const string _handleKeyUpEvent = "javascript: HandleKeyUpEvent('{0}',{1},{2});";
        private const string _styleDisplayNone = "none";
        private const string _styleDisplayBlocked = "blocked";
        private const string _true = "true";
        private const string _false = "false";
        private const string _titleText = "TitleText";
        private const string _validationSummary = "validationSummary";
        private const string _resourceTypeSearchCriterion = "resourcetype:{0}";
        private const string _sortDirectionViewStateKey = "SortDirection";
        private const string _sortExpressionViewStateKey = "SortExpression";

        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets connection string  of database to be used.
        /// </summary>
        [Browsable(false)]
        public override string ConnectionString
        {
            get
            {
                return base.ConnectionString;
            }
            set
            {
                base.ConnectionString = value;
                FilterCriteriaGrid.ConnectionString = value;
            }
        }

        /// <summary>
        /// Register or un-register event for EntityType change.
        /// </summary>
        [Localizable(true)]
        public event EventHandler<EntityTypeEventArgs> EntityTypeClicked
        {
            add { Events.AddHandler(_entityTypeChangedEventKey, value); }
            remove { Events.RemoveHandler(_entityTypeChangedEventKey, value); }
        }

        /// <summary>
        /// Gets or sets entity type.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionEntitySearchEntityType")]
        public string EntityType
        {
            get
            {
                return ViewState[_entityTypeViewStateKey] != null ?
                    ViewState[_entityTypeViewStateKey].ToString() : _resource;

            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value", Resources.GlobalResource.MessageEntityTypeNull);
                }
                if (!this.DesignMode)
                {
                    //Check is selected resource type is available or not.
                    using (ResourceDataAccess resourceDAL = new ResourceDataAccess(CreateContext()))
                    {
                        if (!resourceDAL.IsValidResourceType(value))
                        {
                            throw new InvalidOperationException(Resources.GlobalResource.MessageInvalidEntityType);
                        }
                    }
                }

                ViewState[_entityTypeViewStateKey] = value;
                FilterCriteriaGrid.EntityType = value;
            }
        }

        /// <summary>
        /// Gets or sets title of search result view.
        /// </summary>
        [ZentityCategory("CategoryAppearance")]
        [ZentityDescription("DescriptionResourceListViewSearchResultTitleText")]
        public string SearchResultTitleText
        {
            get
            {
                return ViewState[_searchResoultTitleKey] != null ?
                    ViewState[_searchResoultTitleKey].ToString() : Resources.GlobalResource.SearchResultTitleText;
            }
            set
            {
                ViewState[_searchResoultTitleKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets view link Url. Url format should be Url?Id={0}. {0} will be replaced by actual entity id.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceListViewViewUrl")]
        public string ViewUrl
        {
            get
            {
                return ResourceListView.ViewUrl;
            }
            set
            {
                ResourceListView.ViewUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets flag to enable or disable delete option.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceListViewEnableDeleteOption")]
        public bool EnableDeleteOption
        {
            get { return ResourceListView.EnableDeleteOption; }
            set { ResourceListView.EnableDeleteOption = value; }
        }

        #region Style Propeties

        /// <summary>
        /// Gets the style defined for labels.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionEntitySearchLabelStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle LabelStyle
        {
            get
            {
                if (this._labelStyle == null)
                {
                    this._labelStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._labelStyle).TrackViewState();
                    }
                }
                return this._labelStyle;
            }
        }

        /// <summary>
        /// Gets style defined for buttons.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionEntitySearchButtonStyle"),
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
        /// Gets style defined for FilterCriteria header row.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionFilterCriteriaHeaderStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle FilterCriteriaHeaderStyle
        {
            get
            {
                return FilterCriteriaGrid.HeaderStyle;
            }
        }

        /// <summary>
        /// Gets style defined for FilterCriteria row.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionFilterCriteriaRowStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle FilterCriteriaRowStyle
        {
            get
            {
                return FilterCriteriaGrid.RowStyle;
            }
        }

        /// <summary>
        /// Gets style defined for FilterCriteria alternate row.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionFilterCriteriaAlternateRowStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle FilterCriteriaAlternatingRowStyle
        {
            get
            {
                return FilterCriteriaGrid.AlternatingRowStyle;
            }
        }

        /// <summary>
        /// Gets style defined for FilterCriteria FooterRow.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionFilterCriteriaFooterRowStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle FilterCriteriaFooterStyle
        {
            get
            {
                return FilterCriteriaGrid.FooterStyle;
            }
        }

        /// <summary>
        /// Gets title style of ResourceListView.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionResourceListViewTitleStyle"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ResourceListViewTitleStyle
        {
            get
            {
                return ResourceListView.TitleStyle;
            }
        }

        /// <summary>
        /// Gets item style of ResourceListView.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionResourceListViewItemStyle"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ResourceListViewItemStyle
        {
            get
            {
                return ResourceListView.ItemStyle;
            }
        }

        /// <summary>
        /// Gets alternate item style of ResourceListView.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionResourceListViewAlternateItemStyle"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ResourceListViewAlternateItemStyle
        {
            get
            {
                return ResourceListView.AlternateItemStyle;
            }
        }

        /// <summary>
        /// Gets container style of ResourceListView.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionResourceListViewContainerStyle"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ResourceListViewContainerStyle
        {
            get
            {
                return ResourceListView.ContainerStyle;
            }
        }

        /// <summary>
        /// Gets item separator style of ResourceListView.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ResourceListViewSeparatorStyle
        {
            get
            {
                return ResourceListView.SeparatorStyle;
            }
        }

        /// <summary>
        /// Gets button style of ResourceListView.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ResourceListViewButtonStyle
        {
            get
            {
                return ResourceListView.ButtonStyle;
            }
        }

        #endregion

        #endregion

        #region Private

        private Label MainTitleLabel
        {
            get
            {
                if (_mainTitleLabel == null)
                {
                    _mainTitleLabel = new Label();
                    _mainTitleLabel.ID = _mainTitleLabelId;
                    _mainTitleLabel.Text = Title;
                }
                return _mainTitleLabel;
            }
        }

        private DropDownList ResourceTypeList
        {
            get
            {
                if (_resourceTypeList == null)
                {
                    _resourceTypeList = new DropDownList();
                    _resourceTypeList.ID = _resourceTypesId;
                    _resourceTypeList.AutoPostBack = true;
                    _resourceTypeList.SelectedIndexChanged +=
                        new EventHandler(ResourceTypeList_SelectedIndexChanged);
                }

                return _resourceTypeList;
            }
        }

        private Label ResourceTypeLabel
        {
            get
            {
                if (_rtLabel == null)
                {
                    _rtLabel = new Label();
                    _rtLabel.ID = _resourceTypesLabelId;
                    _rtLabel.Text = Resources.GlobalResource.LabelResourceTypeText;
                }
                return _rtLabel;
            }
        }

        private DropDownList PageSizeList
        {
            get
            {
                if (_pageSizeList == null)
                {
                    _pageSizeList = new DropDownList();
                    _pageSizeList.ID = _pageSizeListId;
                    //Added for disabling feed buttons when page size is changed.
                    _pageSizeList.AutoPostBack = true;
                }

                return _pageSizeList;
            }
        }

        private Label PageSizeLabel
        {
            get
            {
                if (_psLabel == null)
                {
                    _psLabel = new Label();
                    _psLabel.ID = _pageSizeListLabelId;
                    _psLabel.Text = Resources.GlobalResource.LabelPageSizeListText;

                }
                return _psLabel;
            }
        }

        private RadioButton AllRadioButton
        {
            get
            {
                if (_allRadioButton == null)
                {
                    _allRadioButton = new RadioButton();
                    _allRadioButton.ID = _allRadioButtonId;
                    _allRadioButton.Text = Resources.GlobalResource.RadioButtonAllText;
                    _allRadioButton.GroupName = _radioButtonGroupName;
                    _allRadioButton.Checked = true;
                    _allRadioButton.AutoPostBack = true;
                    _allRadioButton.CheckedChanged += new EventHandler(RadioButton_CheckedChanged);
                }
                return _allRadioButton;
            }
        }

        private RadioButton FilterRadioButton
        {
            get
            {
                if (_filterRadioButton == null)
                {
                    _filterRadioButton = new RadioButton();
                    _filterRadioButton.ID = _filterRadioButtonId;
                    _filterRadioButton.Text = Resources.GlobalResource.RadioButtonFilterText;
                    _filterRadioButton.GroupName = _radioButtonGroupName;
                    _filterRadioButton.AutoPostBack = true;
                    _filterRadioButton.CheckedChanged += new EventHandler(RadioButton_CheckedChanged);
                }
                return _filterRadioButton;
            }
        }

        private PropertyFilterCriteria FilterCriteriaGrid
        {
            get
            {
                if (_filterCriteria == null)
                {
                    _filterCriteria = new PropertyFilterCriteria();
                    _filterCriteria.ID = _filterCriteriaId;
                    _filterCriteria.Height = _filterCriteria.Width = new Unit("100%", CultureInfo.InvariantCulture);
                    _filterCriteria.EntityType = ResourceTypeList.Text;
                }

                return _filterCriteria;
            }
        }

        private Button SubmitButton
        {
            get
            {
                if (_submitButton == null)
                {
                    _submitButton = new Button();
                    _submitButton.ID = _submitButtonId;
                    _submitButton.Text = Resources.GlobalResource.ButtonSubmitText;
                    _submitButton.Width = 100;
                    _submitButton.Click += new EventHandler(SubmitButton_Click);
                }

                return _submitButton;
            }
        }

        private ResourceListView ResourceListView
        {
            get
            {
                if (_resourceListView == null)
                {
                    _resourceListView = new ResourceListView();
                    _resourceListView.ID = _resourceListViewId;
                    _resourceListView.Width = Unit.Percentage(100);
                    _resourceListView.IsFeedSupported = true;
                    _resourceListView.OnPageChanged += new EventHandler<EventArgs>(ResourceListView_OnPageChanged);
                    _resourceListView.OnDeleteButtonClicked += new EventHandler<DeleteEventArgs>(ResourceListView_OnDeleteButtonClicked);
                    _resourceListView.OnSortButtonClicked += new EventHandler<EventArgs>(ResourceListView_OnSortButtonClicked);
                }
                return _resourceListView;
            }
        }

        private string SearchCriteria
        {
            get
            {
                return ViewState[_searchCriteriaViewStateKey] as string;
            }
            set
            {
                ViewState[_searchCriteriaViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets sort direction of the column selected for sorting operation.
        /// </summary>        
        private System.Web.UI.WebControls.SortDirection SortDirection
        {
            get
            {
                return ResourceListView.SortDirection;
            }
            set
            {
                ResourceListView.SortDirection = value;
            }
        }

        /// <summary>
        /// Gets sort expression associated with the column selected for sorting operation.
        /// </summary>
        [Browsable(false)]
        private string SortExpression
        {
            get
            {
                return ResourceListView.SortExpression;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Protected

        /// <summary>
        /// Updates properties of child controls.
        /// </summary>
        /// <param name="savedState">View state saved in previous post-back.</param>
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
            FilterCriteriaGrid.EntityType = EntityType;
        }

        /// <summary>
        /// Registers client script.
        /// </summary>
        /// <param name="e">Event argument</param>
        protected override void OnLoad(EventArgs e)
        {
            // Register javascript for search control
            this.Page.ClientScript.RegisterClientScriptInclude(
                this.GetType(),
                _javascriptFile,
                Page.ClientScript.GetWebResourceUrl(this.GetType(), _commonScriptPath));
            RefreshResourceData();
            base.OnLoad(e);
        }

        /// <summary>
        /// Associates all child controls with parent control.
        /// </summary>
        protected override void CreateChildControls()
        {
            this.AssociateChildControls();

            //Apply styles on child controls
            this.ApplyStyles();
        }

        /// <summary>
        /// Registers OnClick events of radio buttons and 
        /// update visibility of FilterCriteriaGrid.
        /// </summary>
        /// <param name="e">Event argument.</param>
        protected override void OnPreRender(EventArgs e)
        {
            //Set the visibility of FilterCriteria control based on status of radio buttons.
            if (AllRadioButton.Checked)
                FilterCriteriaGrid.Style.Add(HtmlTextWriterStyle.Display, _styleDisplayNone);
            else
                FilterCriteriaGrid.Style.Add(HtmlTextWriterStyle.Display, _styleDisplayBlocked);

            //Apply styles on child controls
            this.ApplyStyles();

            base.OnPreRender(e);
        }

        #endregion

        #region Private

        private void ResourceTypeList_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterCriteriaGrid.EntityType = _resourceTypeList.SelectedValue;
            ResourceListView.DataSource.Clear();
            ResourceListView.TotalRecords = 0;

            //Raise EntityTypeChanged event
            EventHandler<EntityTypeEventArgs> handler =
                Events[_entityTypeChangedEventKey] as EventHandler<EntityTypeEventArgs>;

            if (handler != null)
            {
                handler(this, new EntityTypeEventArgs(_resourceTypeList.Text));
            }
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            CreateSearchCriteria();
            ResourceListView.PageIndex = 0;
            this.SortDirection = System.Web.UI.WebControls.SortDirection.Descending;
            Refresh();
        }

        void ResourceListView_OnSortButtonClicked(Object sender, EventArgs e)
        {
            Refresh();
        }

        void ResourceListView_OnPageChanged(object sender, EventArgs e)
        {
            Refresh();
        }

        void ResourceListView_OnDeleteButtonClicked(object sender, DeleteEventArgs e)
        {
            if (e.EntityIdList != null && e.EntityIdList.Count > 0)
            {
                using (ResourceDataAccess resourceDAL = new ResourceDataAccess(this.CreateContext()))
                {
                    if (IsSecurityAwareControl)
                    {
                        Collection<Guid> resourceIdsToBeDeleted = new Collection<Guid>();

                        foreach (Guid resId in e.EntityIdList.ToList())
                        {
                            Resource resource = resourceDAL.GetResource(resId);

                            if (resource != null)
                            {
                                resourceIdsToBeDeleted.Add(resource.Id);
                                CategoryNode categoryNodeobject = resource as CategoryNode;

                                bool isCategoryNode = false;
                                if (categoryNodeobject != null)
                                    isCategoryNode = true;

                                if (IsSecurityAwareControl && AuthenticatedToken != null)
                                {
                                    bool isAuthorized = isCategoryNode ?
                                        resourceDAL.AuthorizeUserForDeletePermissionOnCategory(AuthenticatedToken, resId) :
                                        resourceDAL.AuthorizeUser(AuthenticatedToken, UserResourcePermissions.Delete, resId);

                                    if (!isAuthorized)
                                    {
                                        if (isCategoryNode)
                                        {
                                            throw new UnauthorizedAccessException(GlobalResource.UnauthorizedAccessExceptionCategoryDelete);
                                        }
                                        else
                                        {
                                            throw new UnauthorizedAccessException(string.Format(CultureInfo.InvariantCulture,
                                                GlobalResource.UnauthorizedAccessException, UserResourcePermissions.Delete));
                                        }
                                    }
                                }

                                if (isCategoryNode)
                                {
                                    UpdateEntityListWithCategoryNodes(categoryNodeobject, resourceIdsToBeDeleted);
                                }
                            }
                        }

                        resourceDAL.DeleteResources(resourceIdsToBeDeleted);
                    }

                }

                //Refresh data source for current page.
                Refresh();

                //If page count is changed and Data fetched is empty then try to fetch last page
                IList entityList = ResourceListView.DataSource as IList;
                if ((entityList == null || entityList.Count == 0) && ResourceListView.TotalRecords > 0)
                {
                    ResourceListView.PageIndex = Convert.ToInt32(Math.Ceiling((double)ResourceListView.TotalRecords / ResourceListView.PageSize)) - 1;
                    Refresh();
                }
            }
        }

        private Collection<Guid> UpdateEntityListWithCategoryNodes(CategoryNode categoryNode, Collection<Guid> entityIdList)
        {
            if (categoryNode != null && entityIdList != null)
            {
                if (!entityIdList.Contains(categoryNode.Id))
                    entityIdList.Add(categoryNode.Id);

                categoryNode.Children.Load();
                foreach (CategoryNode childNode in categoryNode.Children)
                {
                    UpdateEntityListWithCategoryNodes(childNode, entityIdList);
                }
            }

            return entityIdList;
        }

        void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == AllRadioButton)
            {
                SubmitButton.ValidationGroup = string.Empty;
                FilterCriteriaGrid.Visible = false;
            }
            else
            {
                SubmitButton.ValidationGroup = FilterCriteriaGrid.ValidationGroup;
                FilterCriteriaGrid.Visible = true;
            }
        }

        private void CreateSearchCriteria()
        {
            StringBuilder searchStringBuilder = new StringBuilder();

            string resourceType = ResourceTypeList.SelectedValue;
            searchStringBuilder.Append(string.Format(CultureInfo.CurrentCulture, _resourceTypeSearchCriterion,
                   resourceType));

            if (!AllRadioButton.Checked)
            {
                string propertyCriteria = FilterCriteriaGrid.SearchString;
                if (!string.IsNullOrEmpty(propertyCriteria))
                {
                    searchStringBuilder.Append(Constants.Space);
                    searchStringBuilder.Append(propertyCriteria);
                }
            }
            SearchCriteria = searchStringBuilder.ToString();
        }

        /// <summary>
        /// Associate child controls
        /// </summary>
        private void AssociateChildControls()
        {
            Table containerTable = new Table();

            containerTable.Rows.Add(CreateMainTitleRow());

            TableRow row = new TableRow();
            TableCell cell = new TableCell();
            cell.HorizontalAlign = HorizontalAlign.Left;

            Table table = new Table();
            table.Rows.Add(CreateResourceTypeRow());

            table.Rows.Add(CreatePageSizeRow());

            cell.Controls.Add(table);
            row.Cells.Add(cell);
            containerTable.Rows.Add(row);

            containerTable.Rows.Add(CreateAllRadioButtonRow());
            containerTable.Rows.Add(CreateFilterRadioButtonRow());
            containerTable.Rows.Add(CreateFilterCriteriaGridRow());
            containerTable.Rows.Add(CreatePlaceHolderRow());
            containerTable.Rows.Add(CreateValidationSummaryRow());
            containerTable.Rows.Add(CreatePlaceHolderRow());
            containerTable.Rows.Add(CreateSubmitButtonRow());
            containerTable.Rows.Add(CreatePlaceHolderRow());

            row = new TableRow();
            cell = new TableCell();
            cell.HorizontalAlign = HorizontalAlign.Left;

            // adding div to grid control
            HtmlGenericControl gridDiv = new HtmlGenericControl("div");
            gridDiv.Style.Add("height", "100%");
            gridDiv.Style.Add("overflow", "auto");
            if (this.Width != null)
            {
                gridDiv.Style.Add("width", this.Width.ToString((CultureInfo.InvariantCulture)));
            }

            table = new Table();
            table.Rows.Add(CreateResourceListViewRow());
            table.Width = new Unit("100%", CultureInfo.InvariantCulture);
            table.CellPadding = 10;

            gridDiv.Controls.Add(table);

            cell.Controls.Add(gridDiv);
            row.Cells.Add(cell);
            containerTable.Rows.Add(row);

            containerTable.Width = new Unit("100%", CultureInfo.InvariantCulture);

            Panel containerPanel = new Panel();
            containerPanel.DefaultButton = SubmitButton.ID;
            containerPanel.Controls.Add(containerTable);

            this.Controls.Add(containerPanel);


            if (!this.DesignMode)
            {
                if (!this.Page.IsPostBack)
                {
                    ResourceTypeList.Focus();
                }
            }
        }

        private TableRow CreateMainTitleRow()
        {
            _mainTitleRow = new TableRow();
            TableCell cell = new TableCell();
            cell.HorizontalAlign = HorizontalAlign.Left;
            cell.Controls.Add(MainTitleLabel);
            cell.HorizontalAlign = HorizontalAlign.Left;
            _mainTitleRow.Cells.Add(cell);

            return _mainTitleRow;
        }

        private TableRow CreateResourceTypeRow()
        {
            TableRow row = new TableRow();
            TableCell cell = new TableCell();
            cell.HorizontalAlign = HorizontalAlign.Left;
            cell.Controls.Add(ResourceTypeLabel);
            row.Cells.Add(cell);
            cell = new TableCell();
            cell.HorizontalAlign = HorizontalAlign.Left;

            if (!this.DesignMode)
            {
                using (ResourceDataAccess resourceDAL = new ResourceDataAccess(CreateContext()))
                {
                    List<ResourceType> resourceInfoList = resourceDAL.GetResourceTypeList(EntityType)
                                    .OrderBy(tuple => tuple.Name).ToList();

                    resourceInfoList = CoreHelper.FilterSecurityResourceTypes(resourceInfoList).ToList();

                    if (resourceInfoList.Count > 1)
                    {
                        ResourceType parentResourceType = resourceInfoList
                            .Where(tuple => tuple.Name.Equals(EntityType, StringComparison.OrdinalIgnoreCase))
                            .First();

                        ResourceTypeList.Items.Add(new ListItem(GlobalResource.All, parentResourceType.FullName));
                    }

                    foreach (var item in resourceInfoList)
                    {
                        ResourceTypeList.Items.Add(new ListItem(item.Name, item.FullName));
                    }
                    if (ResourceTypeList.Items.Count > 0)
                    {
                        using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
                        {
                            FilterCriteriaGrid.EntityType = ResourceTypeList.SelectedValue;
                        }
                    }
                }
            }
            cell.Controls.Add(ResourceTypeList);
            row.Cells.Add(cell);
            return row;
        }

        private TableRow CreatePageSizeRow()
        {
            TableRow row = new TableRow();
            TableCell cell = new TableCell();
            cell.HorizontalAlign = HorizontalAlign.Left;
            cell.Controls.Add(PageSizeLabel);
            row.Cells.Add(cell);
            cell = new TableCell();
            cell.HorizontalAlign = HorizontalAlign.Left;
            //Set Page size list
            if (PageSizeList.DataSource == null)
            {
                PageSizeList.DataSource = GetPageSizeList();
                PageSizeList.DataBind();
                PageSizeList.SelectedIndex = 0;
            }

            cell.Controls.Add(PageSizeList);
            row.Cells.Add(cell);

            return row;
        }

        private TableRow CreateAllRadioButtonRow()
        {
            TableRow row = new TableRow();
            TableCell cell = new TableCell();
            cell.HorizontalAlign = HorizontalAlign.Left;
            cell.Controls.Add(AllRadioButton);
            row.Cells.Add(cell);

            return row;
        }

        private TableRow CreateFilterRadioButtonRow()
        {
            TableRow row = new TableRow();
            TableCell cell = new TableCell();
            cell.HorizontalAlign = HorizontalAlign.Left;
            cell.Controls.Add(FilterRadioButton);
            row.Cells.Add(cell);

            return row;
        }

        private TableRow CreateSubmitButtonRow()
        {
            TableRow row = new TableRow();
            TableCell cell = new TableCell();
            cell.HorizontalAlign = HorizontalAlign.Center;
            cell.Controls.Add(SubmitButton);
            row.Cells.Add(cell);

            return row;
        }

        private static TableRow CreatePlaceHolderRow()
        {
            TableRow row = new TableRow();
            row.Height = new Unit(20);

            return row;
        }

        private TableRow CreateValidationSummaryRow()
        {
            TableRow row = new TableRow();
            TableCell cell = new TableCell();

            ValidationSummary validationsummary = new ValidationSummary();
            validationsummary.ID = _validationSummary;
            validationsummary.ValidationGroup = FilterCriteriaGrid.ValidationGroup;
            validationsummary.ShowMessageBox = false;
            validationsummary.ShowSummary = true;
            validationsummary.HeaderText = Resources.GlobalResource.validationSummaryHeader;
            cell.Controls.Add(validationsummary);
            row.Cells.Add(cell);

            return row;
        }

        private TableRow CreateFilterCriteriaGridRow()
        {
            TableRow row = new TableRow();
            TableCell cell = new TableCell();
            cell.HorizontalAlign = HorizontalAlign.Left;
            FilterCriteriaGrid.Width = new Unit("100%", CultureInfo.InvariantCulture);
            cell.Controls.Add(FilterCriteriaGrid);
            row.Cells.Add(cell);
            return row;
        }

        private TableRow CreateResourceListViewRow()
        {
            TableRow row = new TableRow();
            TableCell cell = new TableCell();
            cell.HorizontalAlign = HorizontalAlign.Left;
            //ResourceListView.ButtonStyle.MergeWith(ButtonStyle);
            cell.Controls.Add(ResourceListView);
            row.Cells.Add(cell);
            return row;
        }

        /// <summary>
        /// Creates list of available page size.
        /// </summary>
        /// <returns>List of page size</returns>
        private static List<string> GetPageSizeList()
        {
            List<string> pageIndex = new List<string>();
            pageIndex.Add(Resources.GlobalResource.PageSize10);
            pageIndex.Add(Resources.GlobalResource.PageSize15);
            pageIndex.Add(Resources.GlobalResource.PageSize25);
            pageIndex.Add(Resources.GlobalResource.PageSize50);
            return pageIndex;
        }

        /// <summary>
        /// Update data source of ZentityGridView base of current page index.
        /// </summary>
        private void Refresh()
        {
            RefreshResourceData();

            //Enable feed for this control
            CreateSearchCriteria();
            ResourceListView.EnableFeed(SearchCriteria);
        }

        private void RefreshResourceData()
        {
            IList<Resource> entityList = null;
            Dictionary<Guid, IEnumerable<string>> userPermissions = null;
            int totalRecords = 0;

            int pageSize = _pageSizeRage[PageSizeList.SelectedIndex > 0 ? PageSizeList.SelectedIndex : 0];
            int fetchedRecords = pageSize * ResourceListView.PageIndex;

            // Fetch entities from database.
            if (!IsSecurityAwareControl)
            {
                entityList = GetEntityList(fetchedRecords, pageSize, out totalRecords);
            }
            else
            {
                entityList = GetEntityList(AuthenticatedToken, fetchedRecords, pageSize, out totalRecords,
                    this.SortExpression, this.SortDirection);
                if (entityList != null && entityList.Count > 0)
                {
                    userPermissions = GetPermissions(AuthenticatedToken, entityList);
                }
            }

            ResourceListView.PageSize = pageSize;
            ResourceListView.TotalRecords = totalRecords;

            ResourceListView.DataSource.Clear();
            foreach (Resource resource in entityList)
                ResourceListView.DataSource.Add(resource);

            ResourceListView.UserPermissions = userPermissions;
            ResourceListView.DataBind();
        }

        /// <summary>
        /// Fetches list of entities based on set EntityType
        /// </summary>
        /// <param name="fetchedRecords">Records fetched</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalRecords">Total record count</param>
        /// <returns>List of Entities</returns>
        private IList<Resource> GetEntityList(int fetchedRecords, int pageSize, out int totalRecords)
        {
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
            {
                if (SortDirection == System.Web.UI.WebControls.SortDirection.Ascending)
                {
                    return dataAccess.SearchResources(SearchCriteria, fetchedRecords,
                    pageSize, out totalRecords, Zentity.Platform.SortDirection.Ascending, SortExpression, AuthenticatedToken, IsSecurityAwareControl).ToList();
                }
                else
                {
                    return dataAccess.SearchResources(SearchCriteria, fetchedRecords,
                    pageSize, out totalRecords, Zentity.Platform.SortDirection.Descending, SortExpression, AuthenticatedToken, IsSecurityAwareControl).ToList();
                }
            }
        }

        /// <summary>
        /// Gets resources on which user is having read permission.
        /// </summary>
        /// <param name="token">Authentication Token</param>
        /// <param name="fetchedRecords">Records fetched</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalRecords">Total record count</param>
        /// <param name="sortExpression">Column to sort on</param>
        /// <param name="sortDirection">Direction to sort in</param>
        /// <returns>List of Entities</returns>
        private IList<Resource> GetEntityList(AuthenticatedToken token, int fetchedRecords, int pageSize,
            out int totalRecords, string sortExpression, System.Web.UI.WebControls.SortDirection sortDirection)
        {
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
            {
                if (sortDirection == System.Web.UI.WebControls.SortDirection.Ascending)
                {
                    return dataAccess.SearchForResources(token, SearchCriteria, fetchedRecords,
                        pageSize, out totalRecords, sortExpression, Zentity.Platform.SortDirection.Ascending, IsSecurityAwareControl).ToList();
                }
                else
                {
                    return dataAccess.SearchForResources(token, SearchCriteria, fetchedRecords,
                        pageSize, out totalRecords, sortExpression, Zentity.Platform.SortDirection.Descending, IsSecurityAwareControl).ToList();
                }
            }
        }

        /// <summary>
        /// Get user permissions for the specified list of resources and their related resources.
        /// </summary>
        /// <param name="token">Authentication token</param>
        /// <param name="resources">List or resources</param>
        /// <returns>Mapping resource id and user permissions on the resource</returns>
        private Dictionary<Guid, IEnumerable<string>> GetPermissions(AuthenticatedToken token, IList<Resource> resources)
        {
            Dictionary<Guid, IEnumerable<string>> userPermissions = null;

            if (resources != null && token != null && resources.Count > 0)
            {
                IList<Resource> srcResources = resources.ToList();
                foreach (Resource res in resources)
                {
                    //add file resources to source list
                    if (res.Files != null && res.Files.Count > 0)
                    {
                        srcResources = srcResources.Union(res.Files.Select(tuple => tuple as Resource).ToList()).ToList();
                    }

                    //add author resources to source list
                    ScholarlyWork scholWork = res as ScholarlyWork;
                    if (scholWork != null && scholWork.Authors != null && scholWork.Authors.Count > 0)
                    {
                        srcResources = srcResources.Union(scholWork.Authors.Select(tuple => tuple as Resource).ToList()).ToList();
                    }
                }

                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    //Get user permission for all resources in the source list.
                    var permissons = dataAccess.GetResourcePermissions(token, srcResources);

                    if (permissons != null)
                    {
                        userPermissions = permissons.ToDictionary(tuple => tuple.Resource.Id, tuple => tuple.Permissions);
                    }
                }
            }

            //This is a default case which indicates that user is not having any permission.
            if (userPermissions == null)
                userPermissions = new Dictionary<Guid, IEnumerable<string>>(); ;

            return userPermissions;
        }

        /// <summary>
        /// Iterates through all the item cells and apply stem style.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="style"></param>
        private static void ApplyRowStyle(TableRow row, TableItemStyle style)
        {
            for (int cellIndex = 0; cellIndex < row.Cells.Count; cellIndex++)
            {
                TableCell cell = row.Cells[cellIndex];
                cell.ApplyStyle(style);
            }
        }

        /// <summary>
        /// Applies styles on child controls.
        /// </summary>
        private void ApplyStyles()
        {
            ApplyRowStyle(_mainTitleRow, TitleStyle);
            ResourceTypeLabel.ApplyStyle(LabelStyle);
            PageSizeLabel.ApplyStyle(LabelStyle);
            AllRadioButton.ApplyStyle(LabelStyle);
            FilterRadioButton.ApplyStyle(LabelStyle);
            SubmitButton.ApplyStyle(ButtonStyle);
        }

        #endregion

        #endregion
    }
}
