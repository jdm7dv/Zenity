// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using System.Reflection;
using Zentity.Core;
using Zentity.Web.UI.ToolKit.Resources;
using Zentity.Security.AuthorizationHelper;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This controls displays resource information and its related resources.
    /// </summary>
    /// <example>
    ///     The code below is the source for ResourceDetailView.aspx. It shows an example of using 
    ///     <see cref="ResourceDetailView"/> control.
    ///     <code>
    ///         &lt;%@ Page Language="C#" AutoEventWireup="true" %&gt;
    ///         
    ///         &lt;%@ Register Assembly="Zentity.Web.UI.ToolKit" Namespace="Zentity.Web.UI.ToolKit"
    ///             TagPrefix="zentity" %&gt;
    ///         &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"&gt;
    ///         &lt;html xmlns="http://www.w3.org/1999/xhtml"&gt;
    ///         &lt;head runat="server"&gt;
    ///             &lt;title&gt;ResourceDetailView Sample&lt;/title&gt;
    ///              
    ///             &lt;script runat="server"&gt;
    ///                 protected void Page_Load(object sender, EventArgs e)
    ///                 {
    ///                     string id = Convert.ToString(Request.QueryString["Id"]);
    ///                     if (string.IsNullOrEmpty(id))
    ///                     {
    ///                         StatusLabel.Text = "Please pass an Id parameter (GUID) in the query string. Example: " +
    ///                             Request.Url.AbsolutePath + "?Id=6bd35f74-5a07-4df8-98a0-7ddb71f88c24";
    ///                         ResourceDetailView1.Visible = false;
    ///                    }
    ///                     else
    ///                     {
    ///                         StatusLabel.Text = string.Empty;
    ///                         Guid entityId = new Guid(id);
    ///                         ResourceDetailView1.ResourceId = entityId;
    ///                     }
    ///                 }
    ///              &lt;/script&gt;
    ///              
    ///         &lt;/head&gt;
    ///         &lt;body&gt;
    ///             &lt;form id="form1" runat="server"&gt;
    ///             &lt;div&gt;
    ///                 &lt;zentity:ResourceDetailView ID="ResourceDetailView1" runat="server"
    ///                     ViewUrl="ResourceDetailView.aspx?Id={0}" ViewChangeHistoryUrl="ChangeHistory.aspx?Id={0}"
    ///                     IsSecurityAwareControl="false"&gt;
    ///                 &lt;/zentity:ResourceDetailView&gt;
    ///                  &lt;asp:Label ID="StatusLabel" runat="server" /&gt;
    ///             &lt;/div&gt;
    ///             &lt;/form&gt;
    ///         &lt;/body&gt;
    ///         &lt;/html&gt;
    ///     </code>
    /// </example>
    [DefaultProperty("Text")]
    [Designer(typeof(ResourceDetailViewDesigner))]
    public class ResourceDetailView : ZentityBase
    {
        #region Member Variables

        private Panel _resourceInfoPanel;
        private Panel _resourceInfoTitlePanel;
        //TODO: Uncomment to show edit link.
        //private Panel _resourceInfoHeaderPanel;
        private Panel _resourceInfoDetailsPanel;
        private Panel _resourceInfoFooterPanel;
        private Panel _relatedResourcePanel;
        private Panel _relatedResourceTitlePanel;
        private Panel _relatedResourceDetailsPanel;
        private Label _resourceInfoLabel;
        private Label _relatedResourcesLabel;
        //TODO: Uncomment to show edit link.
        //private HyperLink _resourceInfoEditLink;
        private HyperLink _viewChangeHistoryLink;
        private Table _containerTable;
        private Label _errorMessageLabel;
        private LinkButton _uploadedFileLink;
        private RelatedDownloads _relatedDownloads;
        private byte[] _fullLectureImage;

        private TableItemStyle _resourceInfoContainerStyle;
        private TableItemStyle _resourceInfoTitleStyle;
        private TableItemStyle _relatedResourcesContainerStyle;
        private TableItemStyle _relatedResourcesTitleStyle;
        private TableItemStyle _relatedResourcesHeaderStyle;
        private TableItemStyle _relatedResourcesItemStyle;
        private string[] orderedProperties = new string[] { _title, _description, _uri, _dateModified };
        private string[] excludedProperties = new string[] { _id };
        private string[] datePublishedProperties = new string[] { _dayPublished, _monthPublished, _yearPublished };
        #endregion

        #region Constants

        private const string _resourceInfoPanelId = "resourceInfoPanelId";
        private const string _resourceInfoTitlePanelId = "resourceInfoTitlePanelId";
        //TODO: Uncomment to show edit link.
        //private const string _resourceInfoHeaderPanelId = "resourceInfoHeaderPanelId";
        private const string _resourceInfoDetailsPanelId = "resourceInfoDetailsPanelId";
        private const string _resourceInfoFooterPanelId = "resourceInfoFooterPanelId";
        private const string _relatedResourcePanelId = "relatedResourcePanelId";
        private const string _relatedResourceTitlePanelId = "relatedResourceTitlePanelId";
        private const string _relatedResourceDetailsPanelId = "relatedResourceDetailsPanelId";
        private const string _resourceInfoLabelId = "resourceInfoLabelId";
        private const string _relatedResourcesLabelId = "relatedResourcesLabelId";
        private const string _errorMessageLabelId = "errorMessageLabelId";
        //TODO: Uncomment to show edit link.
        //private const string _resourceInfoEditLinkId = "relatedInfoEditLinkId";
        private const string _viewChangeHistoryLinkId = "viewChangeHistoryLinkId";
        private const string _uploadedFileLinkId = "uploadedFileLinkId";
        private const string _resourceCloudId = "resourceCloudId";
        private const string _relatedDownloadId = "relatedDownloadId";

        private const string _dataSourceKey = "dataSourceKey";
        private const string _resourcesInfoTitleKey = "resourcesInfoTitleKey";
        private const string _relatedResourcesTitleKey = "relatedResourcesTitleKey";
        private const string _viewUrlKey = "viewUrlKey";
        //TODO: Uncomment to show edit link.
        //private const string _editResourceInfoUrlKey = "editResourceInfoUrlKey";
        private const string _editResourceAssociaionUrlKey = "editResourceAssociaionUrlKey";
        private const string _editTagAssociationUrlKey = "editTagAssociationUrlKey";
        private const string _editCategoryAssociationUrlKey = "editCategotyAssociationUrlKey";
        private const string _viewChangeHistoryUrlKey = "viewChangeHistoryUrlKey";
        private const string _bibTexImportUrlKey = "bibTexImportUrlKey";

        private const string _title = "Title";
        private const string _description = "Description";
        private const string _dateAdded = "DateAdded";
        private const string _dateModified = "DateModified";
        private const string _uri = "Uri";
        private const string _id = "Id";
        private const string _entityState = "EntityState";

        private const string _colan = ":";

        private const string _CategoryNodesProperty = "CategoryNodes";
        private const string _TagsProperty = "Tags";
        private const string _FilesProperty = "Files";

        private const string _noImagePath = "Zentity.Web.UI.ToolKit.Image.NoImage.JPG";
        private const int _imageWidth = 32;
        private const int _imageHeight = 32;
        private const string _thumbnailImage = "ThumbnailImage";
        private const string _thumbnailCallbackUrl = "ThumbnailCallback.ashx?ResourceId=";

        private const string _datePublished = "DatePublished";
        private const string _dayPublished = "DayPublished";
        private const string _monthPublished = "MonthPublished";
        private const string _yearPublished = "YearPublished";

        private int _maxTitleCharWidth = 30;

        #endregion

        #region Properties

        #region public

        /// <summary>
        /// Gets or sets resource Id.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceDetailViewReourceId")]
        public Guid ResourceId
        {
            get { return ViewState[_dataSourceKey] != null ? (Guid)ViewState[_dataSourceKey] : Guid.Empty; }
            set { ViewState[_dataSourceKey] = value; }
        }

        /// <summary>
        /// Gets or sets title of resource info panel.
        /// </summary>
        [ZentityCategory("CategoryAppearance")]
        [ZentityDescription("DescriptionResourceDetailViewResourceInfoTitle")]
        public string ResourceInfoTitle
        {
            get { return ViewState[_resourcesInfoTitleKey] != null ? (string)ViewState[_resourcesInfoTitleKey] : GlobalResource.DefaultResourceInfoText; }
            set { ViewState[_resourcesInfoTitleKey] = value; }
        }

        /// <summary>
        /// Gets or sets title for related resources panel.
        /// </summary>
        [ZentityCategory("CategoryAppearance")]
        [ZentityDescription("DescriptionResourceDetailsViewRelatedResourcesTitle")]
        public string RelatedResourcesTitle
        {
            get { return ViewState[_relatedResourcesTitleKey] != null ? (string)ViewState[_relatedResourcesTitleKey] : GlobalResource.DefaultRelatedResourcesText; }
            set { ViewState[_relatedResourcesTitleKey] = value; }
        }

        /// <summary>
        /// Gets or sets view url.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceDetailViewViewUrl")]
        public string ViewUrl
        {
            get { return ViewState[_viewUrlKey] != null ? (string)ViewState[_viewUrlKey] : string.Empty; }
            set { ViewState[_viewUrlKey] = value; }
        }

        //TODO: Uncomment to show edit link.
        ///// <summary>
        ///// Gets or sets edit resource information url.
        ///// </summary>
        //[ZentityCategory("CategoryBehavior")]
        //[ZentityDescription("DescriptionResourceDetailViewEditResourceInfoUrl")]
        //public string EditResourceInfoUrl
        //{
        //    get { return ViewState[_editResourceInfoUrlKey] != null ? (string)ViewState[_editResourceInfoUrlKey] : string.Empty; }
        //    set { ViewState[_editResourceInfoUrlKey] = value; }
        //}

        ///// <summary>
        ///// Gets or sets edit resource assocation url.
        ///// </summary>
        //[ZentityCategory("CategoryBehavior")]
        //[ZentityDescription("DescriptionResourceDetailViewEditResourceAssociationUrl")]
        //public string EditResourceAssociationUrl
        //{
        //    get { return ViewState[_editResourceAssociaionUrlKey] != null ? (string)ViewState[_editResourceAssociaionUrlKey] : string.Empty; }
        //    set { ViewState[_editResourceAssociaionUrlKey] = value; }
        //}

        /// <summary>
        /// Gets or sets view change history url.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceDetailViewViewChangeHistoryUrl")]
        public string ViewChangeHistoryUrl
        {
            get { return ViewState[_viewChangeHistoryUrlKey] != null ? (string)ViewState[_viewChangeHistoryUrlKey] : string.Empty; }
            set { ViewState[_viewChangeHistoryUrlKey] = value; }
        }

        /// <summary>
        /// Gets or sets BibTex import url.
        /// </summary>
        public string BibTexImportUrl
        {
            get { return ViewState[_bibTexImportUrlKey] != null ? (string)ViewState[_bibTexImportUrlKey] : string.Empty; }
            set { ViewState[_bibTexImportUrlKey] = value; }
        }

        /// <summary>
        /// Gets or sets Tag association url.
        /// </summary>
        public string EditTagAssociationUrl
        {
            get { return ViewState[_editTagAssociationUrlKey] != null ? (string)ViewState[_editTagAssociationUrlKey] : string.Empty; }
            set { ViewState[_editTagAssociationUrlKey] = value; }
        }

        /// <summary>
        /// Gets or sets Category association url.
        /// </summary>
        public string EditCategoryAssociationUrl
        {
            get { return ViewState[_editCategoryAssociationUrlKey] != null ? (string)ViewState[_editCategoryAssociationUrlKey] : string.Empty; }
            set { ViewState[_editCategoryAssociationUrlKey] = value; }
        }


        #region Styles

        /// <summary>
        /// Gets or sets ResourceInfo panel style.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionResourceDetailViewResourceInfoContainerStyle"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ResourceInfoContainerStyle
        {
            get
            {
                if (this._resourceInfoContainerStyle == null)
                {
                    this._resourceInfoContainerStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._resourceInfoContainerStyle).TrackViewState();
                    }
                }
                return this._resourceInfoContainerStyle;
            }
        }

        /// <summary>
        /// Gets or sets ResourceInfo title style.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionResourceDetailViewResourceInfoTitleStyle"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ResourceInfoTitleStyle
        {
            get
            {
                if (this._resourceInfoTitleStyle == null)
                {
                    this._resourceInfoTitleStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._resourceInfoTitleStyle).TrackViewState();
                    }
                }
                return this._resourceInfoTitleStyle;
            }
        }

        /// <summary>
        /// Gets or sets RelatedResources panel style.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionResourceDetailViewRelatedResourcesContainerStyle"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle RelatedResourcesContainerStyle
        {
            get
            {
                if (this._relatedResourcesContainerStyle == null)
                {
                    this._relatedResourcesContainerStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._relatedResourcesContainerStyle).TrackViewState();
                    }
                }
                return this._relatedResourcesContainerStyle;
            }
        }

        /// <summary>
        /// Gets or sets RelatedResources title style.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionResourceDetailViewRelatedResourcesTitleStyle"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle RelatedResourcesTitleStyle
        {
            get
            {
                if (this._relatedResourcesTitleStyle == null)
                {
                    this._relatedResourcesTitleStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._relatedResourcesTitleStyle).TrackViewState();
                    }
                }
                return this._relatedResourcesTitleStyle;
            }
        }


        /// <summary>
        /// Gets or sets RelatedResources header style.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionResourceDetailViewRelatedResourcesHeaderStyle"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle RelatedResourcesHeaderStyle
        {
            get
            {
                if (this._relatedResourcesHeaderStyle == null)
                {
                    this._relatedResourcesHeaderStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._relatedResourcesHeaderStyle).TrackViewState();
                    }
                }
                return this._relatedResourcesHeaderStyle;
            }
        }

        /// <summary>
        /// Gets or sets RelatedResources item style.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionResourceDetailViewRelatedResourcesItemStyle"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle RelatedResourcesItemStyle
        {
            get
            {
                if (this._relatedResourcesItemStyle == null)
                {
                    this._relatedResourcesItemStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._relatedResourcesItemStyle).TrackViewState();
                    }
                }
                return this._relatedResourcesItemStyle;
            }
        }

        #endregion

        #endregion

        #region Private

        private Table ContainerTable
        {
            get
            {
                if (_containerTable == null)
                {
                    _containerTable = new Table();
                    _containerTable.Width = Unit.Percentage(100);
                    //_containerTable.CellSpacing = 10;
                }
                return _containerTable;
            }
        }

        private Panel ResourceInfoPanel
        {
            get
            {
                if (_resourceInfoPanel == null)
                {
                    _resourceInfoPanel = new Panel();
                    _resourceInfoPanel.ID = _resourceInfoPanelId;
                    _resourceInfoPanel.Controls.Add(ResourceInfoTitlePanel);
                    //_resourceInfoPanel.Controls.Add(ResourceInfoHeaderPanel);
                    _resourceInfoPanel.Controls.Add(ResourceInfoDetailsPanel);
                    _resourceInfoPanel.Controls.Add(ResourceInfoFooterPanel);

                }

                return _resourceInfoPanel;
            }
        }

        private Panel ResourceInfoTitlePanel
        {
            get
            {
                if (_resourceInfoTitlePanel == null)
                {
                    _resourceInfoTitlePanel = new Panel();
                    _resourceInfoTitlePanel.ID = _resourceInfoTitlePanelId;
                    _resourceInfoTitlePanel.HorizontalAlign = HorizontalAlign.Left;
                    _resourceInfoTitlePanel.Controls.Add(ResourceInfoLabel);
                }

                return _resourceInfoTitlePanel;
            }
        }

        //TODO: Uncomment to show edit link.
        //private Panel ResourceInfoHeaderPanel
        //{
        //    get
        //    {
        //        if (_resourceInfoHeaderPanel == null)
        //        {
        //            _resourceInfoHeaderPanel = new Panel();
        //            _resourceInfoHeaderPanel.ID = _resourceInfoHeaderPanelId;
        //            _resourceInfoHeaderPanel.HorizontalAlign = HorizontalAlign.Right;
        //            _resourceInfoHeaderPanel.Style.Add(HtmlTextWriterStyle.Margin, "10px");
        //            //_resourceInfoHeaderPanel.Controls.Add(ResourceInfoEditLink);
        //        }

        //        return _resourceInfoHeaderPanel;
        //    }
        //}

        private Panel ResourceInfoDetailsPanel
        {
            get
            {
                if (_resourceInfoDetailsPanel == null)
                {
                    _resourceInfoDetailsPanel = new Panel();
                    _resourceInfoDetailsPanel.ID = _resourceInfoDetailsPanelId;
                    _resourceInfoDetailsPanel.EnableViewState = false;
                    _resourceInfoDetailsPanel.Style.Add(HtmlTextWriterStyle.Overflow, "auto");
                }

                return _resourceInfoDetailsPanel;
            }

        }

        private Panel ResourceInfoFooterPanel
        {
            get
            {
                if (_resourceInfoFooterPanel == null)
                {
                    _resourceInfoFooterPanel = new Panel();
                    _resourceInfoFooterPanel.ID = _resourceInfoFooterPanelId;
                    _resourceInfoFooterPanel.Height = new Unit("30px", CultureInfo.InvariantCulture);
                    _resourceInfoFooterPanel.Style.Add(HtmlTextWriterStyle.Margin, "10px");
                    _resourceInfoFooterPanel.HorizontalAlign = HorizontalAlign.Left;
                    _resourceInfoFooterPanel.Controls.Add(ViewChangeHistoryLink);
                }

                return _resourceInfoFooterPanel;
            }
        }

        private Panel RelatedResourcePanel
        {
            get
            {
                if (_relatedResourcePanel == null)
                {
                    _relatedResourcePanel = new Panel();
                    _relatedResourcePanel.ID = _relatedResourcePanelId;
                    _relatedResourcePanel.Controls.Add(RelatedResourceTitlePanel);
                    _relatedResourcePanel.Controls.Add(RelatedResourceDetailsPanel);
                }

                return _relatedResourcePanel;
            }

        }

        private Panel RelatedResourceTitlePanel
        {
            get
            {
                if (_relatedResourceTitlePanel == null)
                {
                    _relatedResourceTitlePanel = new Panel();
                    _relatedResourceTitlePanel.ID = _relatedResourceTitlePanelId;
                    _relatedResourceTitlePanel.HorizontalAlign = HorizontalAlign.Left;
                    _relatedResourceTitlePanel.Controls.Add(RelatedResourcesLabel);
                }

                return _relatedResourceTitlePanel;
            }

        }

        private Panel RelatedResourceDetailsPanel
        {
            get
            {
                if (_relatedResourceDetailsPanel == null)
                {
                    _relatedResourceDetailsPanel = new Panel();
                    _relatedResourceDetailsPanel.ID = _relatedResourceDetailsPanelId;
                    _relatedResourceDetailsPanel.Style.Add(HtmlTextWriterStyle.Margin, "10px");
                    _relatedResourceDetailsPanel.EnableViewState = false;
                }

                return _relatedResourceDetailsPanel;
            }
        }

        private RelatedDownloads RelatedDownloadsPanel
        {
            get
            {
                if (_relatedDownloads == null)
                {
                    _relatedDownloads = new RelatedDownloads();
                    _relatedDownloads.ID = _relatedDownloadId;
                }
                return _relatedDownloads;
            }
        }

        private Label ResourceInfoLabel
        {
            get
            {
                if (_resourceInfoLabel == null)
                {
                    _resourceInfoLabel = new Label();
                    _resourceInfoLabel.ID = _resourceInfoLabelId;
                    _resourceInfoLabel.Font.Bold = true;
                    _resourceInfoLabel.Text = ResourceInfoTitle;
                }

                return _resourceInfoLabel;
            }
        }

        private Label RelatedResourcesLabel
        {
            get
            {
                if (_relatedResourcesLabel == null)
                {
                    _relatedResourcesLabel = new Label();
                    _relatedResourcesLabel.ID = _relatedResourcesLabelId;
                    _relatedResourcesLabel.Font.Bold = true;
                    _relatedResourcesLabel.Text = RelatedResourcesTitle;
                }

                return _relatedResourcesLabel;
            }
        }

        private Label ErrorMessageLabel
        {
            get
            {
                if (_errorMessageLabel == null)
                {
                    _errorMessageLabel = new Label();
                    _errorMessageLabel.ID = _errorMessageLabelId;
                }

                return _errorMessageLabel;
            }
        }

        //TODO: Uncomment to show edit link.
        //private HyperLink ResourceInfoEditLink
        //{
        //    get
        //    {
        //        if (_resourceInfoEditLink == null)
        //        {
        //            _resourceInfoEditLink = new HyperLink();
        //            _resourceInfoEditLink.ID = _resourceInfoEditLinkId;
        //            _resourceInfoEditLink.Text = GlobalResource.EditResourceInfoLinkText;
        //        }

        //        return _resourceInfoEditLink;
        //    }
        //}

        private HyperLink ViewChangeHistoryLink
        {
            get
            {
                if (_viewChangeHistoryLink == null)
                {
                    _viewChangeHistoryLink = new HyperLink();
                    _viewChangeHistoryLink.ID = _viewChangeHistoryLinkId;
                    _viewChangeHistoryLink.Text = GlobalResource.ViewChangeHistoryLinkText;
                }

                return _viewChangeHistoryLink;
            }
        }

        private LinkButton UploadedFileLink
        {
            get
            {
                if (_uploadedFileLink == null)
                {
                    _uploadedFileLink = new LinkButton();
                    _uploadedFileLink.ID = _uploadedFileLinkId;
                    _uploadedFileLink.Click += new EventHandler(UploadedFileLink_Click);
                }

                return _uploadedFileLink;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Protected

        /// <summary>
        /// Creates child controls layout.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.Controls.Add(CreateChildControlLayout());

            if (this.DesignMode)
            {
                PopulateResourceInfoDetailsInDesign();
            }
        }

        /// <summary>
        /// Populates child controls with the data.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
            if (ResourceId != Guid.Empty && !DesignMode)
            {
                Resource resource = null;
                IEnumerable<string> userPermissions = null;
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    if (IsSecurityAwareControl)
                    {
                        ResourcePermissions<Resource> resourcePermissions = dataAccess.GetResourcePermissions(AuthenticatedToken, ResourceId);

                        if (resourcePermissions != null && resourcePermissions.Permissions.Contains(UserResourcePermissions.Read))
                        {
                            resource = resourcePermissions.Resource;
                            userPermissions = resourcePermissions.Permissions;
                        }
                        else
                        {
                            throw new UnauthorizedAccessException(string.Format(CultureInfo.InvariantCulture,
                                Resources.GlobalResource.UnauthorizedAccessException, UserResourcePermissions.Read));
                        }
                    }
                    else
                    {
                        resource = dataAccess.GetResource(ResourceId);
                    }
                }

                if (resource != null)
                {
                    PopulateResourceInfoDetails(resource);
                    PopulateRelatedResourcesDetailsTable(resource, userPermissions);
                }
                else
                {
                    //Set error message.
                    this.Controls.Clear();
                    this.Controls.Add(ErrorMessageLabel);
                    if (!IsSecurityAwareControl)
                        ErrorMessageLabel.Text = Resources.GlobalResource.MsgResourceNotFound;
                    else
                        ErrorMessageLabel.Text = Resources.GlobalResource.MsgResourceNotFoundOrNoViewPermission;
                }
            }

            ApplyStyles();

            base.OnLoad(e);
        }

        #endregion

        #region Private

        private Table CreateChildControlLayout()
        {
            TableRow tr = new TableRow();

            TableCell td = new TableCell();
            td.VerticalAlign = VerticalAlign.Top;
            //td.BorderWidth = 2;
            td.ApplyStyle(ResourceInfoContainerStyle);
            //td.BorderColor = System.Drawing.Color.LightGray;
            td.Width = Unit.Percentage(60);
            td.Controls.Add(ResourceInfoPanel);
            tr.Cells.Add(td);

            td = new TableCell();
            td.VerticalAlign = VerticalAlign.Top;
            //td.BorderWidth = 2;
            td.ApplyStyle(RelatedResourcesContainerStyle);
            //td.BorderColor = System.Drawing.Color.LightGray;
            td.Width = Unit.Percentage(40);
            td.Controls.Add(RelatedResourcePanel);

            tr.Cells.Add(td);

            ContainerTable.Rows.Add(tr);

            return ContainerTable;
        }

        private void PopulateResourceInfoDetailsInDesign()
        {
            Type objectType = GetObjectTypForDesignMode("Resource");

            if (objectType == null)
            {
                objectType = typeof(Resource);
            }
            PropertyInfo[] properties = objectType.GetProperties();

            List<PropertyInfo> schalarProperties = null;
            schalarProperties = properties.Where(tuple => orderedProperties.Contains(tuple.Name))
                        .OrderBy(tup => tup.Name).ToList();

            Table resInfoDetailsTable = new Table();
            resInfoDetailsTable.Width = Unit.Percentage(100);
            resInfoDetailsTable.CellSpacing = 10;
            resInfoDetailsTable.CellPadding = 10;
            TableRow tr = GetResourceInfoRow(GlobalResource.ResourceText + _colan, HttpUtility.HtmlEncode(CoreHelper.UpdateEmptyTitle(GlobalResource.ResourceText)));
            resInfoDetailsTable.Rows.Add(tr);

            foreach (PropertyInfo propertyInfo in schalarProperties)
            {
                tr = GetResourceInfoRow(propertyInfo.Name + _colan, GlobalResource.DesignTimeDummyDataString);
                resInfoDetailsTable.Rows.Add(tr);
            }

            ResourceInfoDetailsPanel.Controls.Add(resInfoDetailsTable);
        }

        private static Type GetObjectTypForDesignMode(string typeName)
        {
            Type objectType = null;

            if (typeName.Contains("."))
            {
                objectType = Type.GetType(typeName);
            }
            else
            {
                objectType = Type.GetType(ResourcePropertyConstants.CoreAssemblyName +
                    ResourcePropertyConstants.Dot + typeName);
                if (objectType == null)
                    objectType = Type.GetType(ResourcePropertyConstants.ScholarlyAssemblyName +
                        ResourcePropertyConstants.Dot + typeName);
            }

            return objectType;
        }



        private void PopulateResourceInfoDetails(Resource resource)
        {
            ResourceInfoDetailsPanel.Controls.Clear();

            if (resource != null)
            {
                UpdateLinks(resource);

                //Get scalar properties of the resource.
                Type t = resource.GetType();
                List<ScalarProperty> schalarProperties = null;
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    schalarProperties = dataAccess.GetScalarProperties(this.Page.Cache, t.Name)
                        .Where(tuple => !orderedProperties.Contains(tuple.Name) && !excludedProperties.Contains(tuple.Name)
                            && !datePublishedProperties.Contains(tuple.Name))
                        .OrderBy(tup => tup.Name).ToList();
                }

                //Create resource info table.
                Table resInfoDetailsTable = new Table();
                resInfoDetailsTable.Width = Unit.Percentage(100);
                resInfoDetailsTable.CellSpacing = 10;
                resInfoDetailsTable.CellPadding = 10;

                //Add row for Title property
                TableRow tr = GetResourceInfoRow(t.Name + _colan, HttpUtility.HtmlEncode(CoreHelper.UpdateEmptyTitle(
                    CoreHelper.GetTitleByResourceType(resource))));
                resInfoDetailsTable.Rows.Add(tr);

                //Add row for other mandatory ordered properties
                foreach (string propertyName in orderedProperties.Where(tuple => !tuple.Equals(_title)))
                {
                    object value = t.GetProperty(propertyName).GetValue(resource, null);
                    string strValue = value != null ? HttpUtility.HtmlEncode(value.ToString()) : string.Empty;
                    tr = GetResourceInfoRow(propertyName + _colan, strValue);
                    resInfoDetailsTable.Rows.Add(tr);
                }

                foreach (ScalarProperty property in schalarProperties)
                {
                    object value = t.GetProperty(property.Name).GetValue(resource, null);
                    if (property.DataType != DataTypes.Binary)
                    {
                        if (value != null)
                        {
                            if (property.Name == _datePublished)
                            {
                                //** Special case **//
                                //Use DatePublish property to populate DayPublished, MothPublished and YearPublished properties.
                                DateTime datePublished = Convert.ToDateTime(value, CultureInfo.CurrentCulture);
                                tr = GetResourceInfoRow(_datePublished + _colan, HttpUtility.HtmlEncode(datePublished.ToString()));
                                resInfoDetailsTable.Rows.Add(tr);
                                tr = GetResourceInfoRow(_dayPublished + _colan, HttpUtility.HtmlEncode(datePublished.Day.ToString(CultureInfo.CurrentCulture)));
                                resInfoDetailsTable.Rows.Add(tr);
                                tr = GetResourceInfoRow(_monthPublished + _colan, HttpUtility.HtmlEncode(datePublished.Month.ToString(CultureInfo.CurrentCulture)));
                                resInfoDetailsTable.Rows.Add(tr);
                                tr = GetResourceInfoRow(_yearPublished + _colan, HttpUtility.HtmlEncode(datePublished.Year.ToString(CultureInfo.CurrentCulture)));
                                resInfoDetailsTable.Rows.Add(tr);
                            }
                            else
                            {
                                bool isValueString = false;
                                string strvalue = value as string;
                                if (strvalue != null)
                                    isValueString = true;
                                if ((isValueString && !string.IsNullOrEmpty(strvalue)) ||
                               (!(isValueString) && value != null))
                                {
                                    tr = GetResourceInfoRow(property.Name + _colan, HttpUtility.HtmlEncode(value.ToString()));
                                    resInfoDetailsTable.Rows.Add(tr);
                                }
                            }


                        }
                    }
                    else
                    {
                        //**Special case Scalar property Image**
                        ImageButton imageButton = new ImageButton();
                        InitializeImageButton(imageButton, value);
                        Label imageLabel = new Label();
                        imageLabel.Font.Bold = true;
                        imageLabel.Text = property.Name + _colan;

                        tr = GetResourceInfoRow(imageLabel, imageButton);
                        resInfoDetailsTable.Rows.Add(tr);
                    }
                }
                //**Special case**
                //If resource is of type File then add row do display uploaded file.
                Zentity.Core.File file = resource as Zentity.Core.File;
                if (file != null)
                {
                    tr = GetUploadedFileRow(GlobalResource.UploadedFileLabelText, file);
                    if (tr != null)
                        resInfoDetailsTable.Rows.Add(tr);
                }

                ResourceInfoDetailsPanel.Controls.Add(resInfoDetailsTable);
            }
        }

        private void UpdateLinks(Resource resource)
        {
            //TODO: Uncomment to show edit link.
            ////Update ResourceInfoLink
            //if (!IsSecurityAwareControl || (userPermissions != null && userPermissions.Contains(UserResourcePermissions.Update)))
            //{
            //    ResourceInfoEditLink.Visible = true;
            //    if (!string.IsNullOrEmpty(EditResourceInfoUrl))
            //        ResourceInfoEditLink.NavigateUrl = string.Format(CultureInfo.CurrentCulture, EditResourceInfoUrl, resource.Id);
            //}
            //else
            //{
            //    ResourceInfoEditLink.Visible = false;
            //}

            //Update ViewChangeHistoryLink.
            if (!string.IsNullOrEmpty(ViewChangeHistoryUrl))
                ViewChangeHistoryLink.NavigateUrl = string.Format(CultureInfo.CurrentCulture, ViewChangeHistoryUrl, resource.Id);
        }

        private TableRow GetUploadedFileRow(string header, Zentity.Core.File file)
        {
            TableRow tr = null;
            if (file != null)
            {
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    string fileSize = dataAccess.GetUploadedFileSize(file);
                    if (!string.IsNullOrEmpty(fileSize))
                    {
                        tr = new TableRow();
                        TableCell td = new TableCell();
                        td.HorizontalAlign = HorizontalAlign.Left;
                        td.VerticalAlign = VerticalAlign.Top;
                        td.Width = Unit.Percentage(20);
                        Label headerLabel = new Label();
                        headerLabel.Font.Bold = true;
                        headerLabel.Text = header + _colan;
                        td.Controls.Add(headerLabel);
                        tr.Cells.Add(td);
                        td = new TableCell();
                        td.ColumnSpan = 3;
                        td.HorizontalAlign = HorizontalAlign.Left;
                        td.VerticalAlign = VerticalAlign.Top;
                        td.Width = Unit.Percentage(80);

                        string fileName = HttpUtility.HtmlEncode(CoreHelper.FitString(
                            CoreHelper.GetFileName(file.Title, file.FileExtension), _maxTitleCharWidth));

                        UploadedFileLink.Text = fileName + " (" + fileSize + ")";
                        UploadedFileLink.ID = file.Id.ToString();
                        td.Controls.Add(UploadedFileLink);
                        tr.Cells.Add(td);

                        return tr;
                    }
                }
            }

            return tr;
        }

        private static TableRow GetResourceInfoRow(string header, string value)
        {
            TableRow tr = new TableRow();
            TableCell td = new TableCell();
            td.HorizontalAlign = HorizontalAlign.Left;
            td.VerticalAlign = VerticalAlign.Top;
            td.Width = Unit.Percentage(20);
            Label headerLabel = new Label();
            headerLabel.Font.Bold = true;
            headerLabel.Text = header;
            td.Controls.Add(headerLabel);
            tr.Cells.Add(td);
            td = new TableCell();
            td.ColumnSpan = 3;
            td.HorizontalAlign = HorizontalAlign.Left;
            td.VerticalAlign = VerticalAlign.Top;
            td.Width = Unit.Percentage(80);
            Label valueLabel = new Label();
            valueLabel.Text = value;
            td.Controls.Add(valueLabel);
            tr.Cells.Add(td);

            return tr;
        }

        private static TableRow GetResourceInfoRow(Control header, Control value)
        {
            TableRow tr = new TableRow();
            TableCell td = new TableCell();
            td.HorizontalAlign = HorizontalAlign.Left;
            td.VerticalAlign = VerticalAlign.Top;
            td.Width = Unit.Percentage(20);
            td.Controls.Add(header);
            tr.Cells.Add(td);
            td = new TableCell();
            td.ColumnSpan = 3;
            td.HorizontalAlign = HorizontalAlign.Left;
            td.VerticalAlign = VerticalAlign.Top;
            td.Width = Unit.Percentage(80);
            td.Controls.Add(value);
            tr.Cells.Add(td);

            return tr;
        }

        private void PopulateRelatedResourcesDetailsTable(Resource resource, IEnumerable<string> userPermissions)
        {
            if (resource != null)
            {
                Type t = resource.GetType();

                List<NavigationProperty> navigationProperties = null;
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    navigationProperties = dataAccess.GetNavigationalProperties(this.Page.Cache, t.Name).OrderBy(tuple => tuple.Name).ToList();
                }

                //** Special case **//
                //Add RelatedResourceCloud control at first place for related files at first place.
                NavigationProperty fileProperty = navigationProperties.Where(tuple => tuple.Name.Equals(_FilesProperty)).FirstOrDefault();
                if (fileProperty != null)
                {
                    RelatedResourceDetailsPanel.Controls.Add(
                        CreateRelatedReseourceCloudControl(resource, userPermissions, 0, fileProperty));
                }

                int index = 1;
                foreach (NavigationProperty property in navigationProperties.Where(tuple => !tuple.Name.Equals(_FilesProperty)))
                {
                    RelatedResourceDetailsPanel.Controls.Add(
                        CreateRelatedReseourceCloudControl(resource, userPermissions, index, property));

                    index++;
                }

                //** Special case**//
                //Add Related Downloads control
                if (fileProperty != null)
                {
                    RelatedDownloadsPanel.SubjectResource = resource;
                    RelatedDownloadsPanel.UserPermissions = userPermissions;
                    RelatedDownloadsPanel.IsSecurityAwareControl = this.IsSecurityAwareControl;
                    RelatedDownloadsPanel.AuthenticatedToken = this.AuthenticatedToken;
                    RelatedDownloadsPanel.ConnectionString = this.ConnectionString;
                    RelatedDownloadsPanel.TitleStyle.MergeWith(RelatedResourcesHeaderStyle);
                    RelatedDownloadsPanel.ItemStyle.MergeWith(RelatedResourcesItemStyle);
                    RelatedResourceDetailsPanel.Controls.Add(RelatedDownloadsPanel);
                }
            }
        }

        private RelatedResourcesCloud CreateRelatedReseourceCloudControl(Resource resource,
            IEnumerable<string> userPermissions, int index, NavigationProperty property)
        {
            RelatedResourcesCloud resCloud = new RelatedResourcesCloud();
            resCloud.ID = _resourceCloudId + index.ToString(CultureInfo.InvariantCulture);
            resCloud.SubjectResource = resource;
            resCloud.UserPermissions = userPermissions;
            resCloud.IsSecurityAwareControl = this.IsSecurityAwareControl;
            resCloud.AuthenticatedToken = this.AuthenticatedToken;
            resCloud.ConnectionString = this.ConnectionString;
            resCloud.BibTexImportUrl = this.BibTexImportUrl;
            resCloud.Title = property.Name;
            resCloud.NavigationPropertyName = property.Name;
            resCloud.ViewUrl = this.ViewUrl;
            resCloud.TitleStyle.MergeWith(RelatedResourcesHeaderStyle);
            resCloud.ItemStyle.MergeWith(RelatedResourcesItemStyle);
            //TODO: Uncomment to show edit link.
            //Set edit url based on navigation property.
            //if (property.Name.Equals(_TagsProperty))
            //    resCloud.EditUrl = EditTagAssociationUrl;
            //else if (property.Name.Equals(_CategoryNodesProperty))
            //    resCloud.EditUrl = EditCategoryAssociationUrl;
            //else
            //    resCloud.EditUrl = EditResourceAssociationUrl; ;
            return resCloud;
        }

        private void ApplyStyles()
        {
            //Apply style to ResourceInfo container.
            ContainerTable.Rows[0].Cells[0].ApplyStyle(ResourceInfoContainerStyle);
            //Apply style to RelatedResources container.
            ContainerTable.Rows[0].Cells[1].ApplyStyle(RelatedResourcesContainerStyle);

            ResourceInfoTitlePanel.ApplyStyle(ResourceInfoTitleStyle);
            RelatedResourceTitlePanel.ApplyStyle(RelatedResourcesTitleStyle);
        }

        void UploadedFileLink_Click(object sender, EventArgs e)
        {
            LinkButton lnkButton = sender as LinkButton;
            if (lnkButton != null)
            {
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    dataAccess.DownloadFile(new Guid(lnkButton.ID), this.Page.Response);
                }

            }
        }

        #region Image rendering related methods

        /// <summary>
        /// Sets image url of image control.
        /// </summary>
        /// <param name="imgCtrl"> Image control whose image url to be set </param>
        /// <param name="value"> Image to be rendered on server file system and attached to image control </param>
        private void InitializeImageButton(System.Web.UI.WebControls.ImageButton imgCtrl, object value)
        {
            if (value == null)
            {
                imgCtrl.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), _noImagePath);
                imgCtrl.Attributes.Add("onclick", "return false");
                imgCtrl.Style.Add("cursor", "default");
                return;
            }
            _fullLectureImage = (byte[])value;

            System.Drawing.Image htmlImage = ByteArrayToImage(_fullLectureImage);
            System.Drawing.Image thumnailImage = htmlImage.GetThumbnailImage(_imageWidth, _imageHeight, new System.Drawing.Image.GetThumbnailImageAbort(ThumbnailCallback), IntPtr.Zero);
            this.Page.Session[_thumbnailImage] = ImageToByteArray(thumnailImage);

            imgCtrl.ImageUrl = _thumbnailCallbackUrl + this.ResourceId;
            imgCtrl.Click += new ImageClickEventHandler(imgCtrl_Click);
            imgCtrl.ToolTip = GlobalResource.AlternateThumbImageText;
        }

        /// <summary>
        ///     Returns System.Drawing.Image from byte array.
        /// </summary>
        /// <param name="byteArrayIn">array of byte</param>
        /// <returns>System.Drawing.Image</returns>
        private static System.Drawing.Image ByteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream memoryStream = new MemoryStream(byteArrayIn);
            System.Drawing.Image returnImage = System.Drawing.Image.FromStream(memoryStream);
            return returnImage;
        }

        /// <summary>
        ///     Returns byte array from System.Drawing.Image.
        /// </summary>
        /// <param name="imageIn">System.Drawing.Image</param>
        /// <returns>array of byte</returns>
        private static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream memoryStream = new MemoryStream();
            imageIn.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            return memoryStream.ToArray();
        }

        /// <summary>
        ///     Click event redirect to the full image.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event argument</param>
        private void imgCtrl_Click(object sender, ImageClickEventArgs e)
        {
            System.Drawing.Image image = ByteArrayToImage(_fullLectureImage);
            image.Save(this.Page.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            image.Dispose();
        }

        /// <summary>
        /// This method used by RenderImage() for create Thumbnail of the Image.
        /// </summary>
        /// <returns>Return true for Thumbnail.</returns>
        public bool ThumbnailCallback()
        {
            return true;
        }

        #endregion

        #endregion

        #endregion
    }
  
}
