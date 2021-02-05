// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Zentity.Core;
using Zentity.ScholarlyWorks;
using Zentity.Security.Authentication;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This controls allows user to manage CategoryHierarchy objects.
    /// </summary>
    /// <example>
    ///     The following example shows how to use the <see cref="CategoryHierarchy"/> control.
    ///     The <see cref="CategoryHierarchy"/> control internally uses <see cref="ResourceProperties"/> control. 
    ///     Therefore, to configure the <see cref="ResourceProperties"/> control, add the following section to web configuration file.
    ///     <code lang="xml">
    ///         &lt;configuration&gt;
    ///           &lt;configSections&gt;
    ///             &lt;!-- Resource properties control configuration. --&gt;
    ///             &lt;section name="resourcePropertiesSettings" type="Zentity.Web.UI.ToolKit.ResourcePropertiesConfigSection, Zentity.Web.UI.ToolKit" /&gt;
    ///           &lt;/configSections&gt;
    ///           &lt;!-- Configures the resource properties control. --&gt;
    ///           &lt;resourcePropertiesSettings&gt;
    ///             &lt;requiredProperties&gt;
    ///               &lt;add name="Title" class="Zentity.Core.Resource" /&gt;
    ///               &lt;add name="GroupName" class="Zentity.Security.Authorization.Group" /&gt;
    ///             &lt;/requiredProperties&gt;
    ///             &lt;emailProperties regularExpression="^([^@\s]+)@((?:[-a-z0-9]+\.)+[a-z]{2,})$"&gt;
    ///               &lt;add name="From" class="Zentity.ScholarlyWorks.Email" /&gt;
    ///               &lt;add name="To" class="Zentity.ScholarlyWorks.Email" /&gt;
    ///               &lt;add name="Email" class="Zentity.ScholarlyWorks.Contact" /&gt;
    ///             &lt;/emailProperties&gt;
    ///             &lt;dateRangeProperties&gt;
    ///               &lt;add name="DateAvailableFrom" endDateName="DateAvailableUntil" class="Zentity.ScholarlyWorks.ScholarlyWork" /&gt;
    ///               &lt;add name="DateValidFrom" endDateName="DateValidUntil" class="Zentity.ScholarlyWorks.ScholarlyWork" /&gt;
    ///               &lt;add name="DateStart" endDateName="DateEnd" class="Zentity.ScholarlyWorks.Lecture" /&gt;
    ///             &lt;/dateRangeProperties&gt;
    ///             &lt;readOnlyProperties&gt;
    ///               &lt;add name="DayPublished" class="Zentity.ScholarlyWorks.Publication" /&gt;
    ///               &lt;add name="MonthPublished" class="Zentity.ScholarlyWorks.Publication" /&gt;
    ///               &lt;add name="YearPublished" class="Zentity.ScholarlyWorks.Publication" /&gt;
    ///               &lt;add name="DateAdded" class="Zentity.Core.Resource" /&gt;
    ///               &lt;add name="DateModified" class="Zentity.Core.Resource" /&gt;
    ///             &lt;/readOnlyProperties&gt;
    ///             &lt;imageProperties regularExpression="^.+\.(jpg|JPG|gif|GIF|png|PNG|bmp|BMP){1}$"&gt;
    ///               &lt;add name="Image" class="Zentity.ScholarlyWorks.Lecture" /&gt;
    ///             &lt;/imageProperties&gt;
    ///             &lt;excludedProperties&gt;
    ///               &lt;add name="Id" class="Zentity.Core.Resource" /&gt;
    ///             &lt;/excludedProperties&gt;
    ///             &lt;orderedProperties&gt;
    ///               &lt;add class="Zentity.ScholarlyWorks.Person" order="FirstName,MiddleName,LastName" /&gt;
    ///               &lt;add class="Zentity.ScholarlyWorks.Lecture" order="DateStart,DateEnd" /&gt;
    ///               &lt;add class="Zentity.ScholarlyWorks.ScholarlyWork" order="DateAvailableFrom,DateAvailableUntil" /&gt;
    ///               &lt;add class="Zentity.ScholarlyWorks.ScholarlyWork" order="DateValidFrom,DateValidUntil" /&gt;
    ///             &lt;/orderedProperties&gt;
    ///           &lt;/resourcePropertiesSettings&gt;
    ///         &lt;/configuration&gt;
    ///     </code>
    ///     The code below is the source for CategoryHierarchy.aspx.
    ///     It shows an example of using <see cref="CategoryHierarchy"/> control.
    ///     <code>
    ///         &lt;%@ Page Language="C#" AutoEventWireup="true" %&gt;
    ///         
    ///         &lt;%@ Register Assembly="Zentity.Web.UI.ToolKit" Namespace="Zentity.Web.UI.ToolKit" 
    ///             TagPrefix="Zentity" %&gt;
    ///         &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"&gt;
    ///         &lt;html xmlns="http://www.w3.org/1999/xhtml"&gt;
    ///         &lt;head id="Head1" runat="server"&gt;
    ///             &lt;title&gt;CategoryHierarchy Sample&lt;/title&gt;
    ///         &lt;/head&gt;
    ///         &lt;body&gt;
    ///             &lt;form id="mainForm" runat="server"&gt;
    ///             &lt;div&gt;
    ///                 &lt;Zentity:CategoryHierarchy ID="CategoryHierarchy1" runat="server" TreeViewCaption="Manage Category Hierarchies"
    ///                     TreeNodeNavigationURL="ResourceDetailView.aspx?Id={0}" IsSecurityAwareControl="false"&gt;
    ///                 &lt;/Zentity:CategoryHierarchy&gt;
    ///             &lt;/div&gt;
    ///             &lt;/form&gt;
    ///         &lt;/body&gt;
    ///         &lt;/html&gt;
    ///     </code>
    /// </example>
    public class CategoryHierarchy : BaseTreeView
    {
        #region Constants

        #region Private
        const string _headerViewStateKey = "TreeViewHeader";
        const string _imageId = "ImageId";
        const string _validationGroup = "ValidationGroup";
        const string _editButtonId = "EditButtonId";
        const string _addButtonId = "AddButtonId";
        const string _deleteButtonId = "DeleteButtonId";
        const string _cutButtonId = "CutButtonId";
        const string _pasteButtonId = "PasteButtonId";
        const string _saveButtonId = "SaveButtonId";
        const string _updateButtonId = "UpdateButtonId";
        const string _cancelButtonId = "CancelButtonId";
        const string _parentNodeViewState = "ParentNodeViewState";
        const string _cutNodeViewState = "CutNodeViewState";
        const string _defaultButtonViewState = "DefaultButtonViewState";
        const string _selectedButtonViewState = "SelectedButtonViewState";
        const string _contextMenuViewState = "ContextMenuViewState";
        const string _resourceType = "Zentity.ScholarlyWorks.CategoryNode";
        const string _treeViewStyle = "TreeViewStyle";
        const string _space = "&nbsp;";
        const string _resourceCreatePanelId = "resourceCreatePanelId";
        const string _resourcePropertiesControlId = "resourcePropertiesControlId";
        const string _rootNodeDropDownListId = "rootNodeDropDownListId";
        const string _contextPanelId = "ContextPanelId";
        const int _maxCharShown = 30;
        const int _maxCharShownTree = 20;
        const string _categoryHierarchyScriptPath = "Zentity.Web.UI.ToolKit.ResourceControls.CaregoryHierarchyScript.js";
        const string _javascriptFile = "CategoryJavascriptFile";
        const string _noImagePath = "Zentity.Web.UI.ToolKit.Image.Arrow.GIF";
        const string _contextButtonWidth = "100%";
        const string _onClickEvent = "onclick";
        const string _deleteResourceFunction = "javascript:return DeleteResource('{0}')";
        const string _onMouseOutEvent = "onmouseout";
        const string _hideContextMenuFunction = "HideContextMenu('{0}', event)";
        const string _onMouseOverEvent = "onmouseover";
        const string _selectMenuFunction = "SelectMenu(this, '{0}');";
        const string _deselectMenuFunction = "DeselectMenu(this,  '{0}');";
        const string _rootNodeDDLTextField = "Title";
        const string _rootNodeDDLValueField = "Id";
        const string _rootNodeDDLDataMember = "CategoryNode";
        const string _treeNodeNavigationURLViewState = "TreeNodeNavigationURLViewState";
        const string _defaultNavigationURL = "#";
        const string _nodeTextScript = @"<span onmouseout='HideArrowImage({0}, {1}, event)' onmouseover='ShowArrowImage({2})'>{3}<img id='{4}' src='{5}' style='visibility:hidden' onclick='return ShowContextMenu({6}, event);' onmouseout='HideArrowImage({7}, {8}, event)'/></span>";
        #endregion Private

        #endregion Constants

        #region Member variables

        #region Private

        private TableItemStyle _rowStyle;
        private TableItemStyle _alternateRowStyle;
        private TableItemStyle _resourceProeprtyHeaderStyle;
        private TableItemStyle _resourceProeprtyTitleStyle;
        private TableItemStyle _leftContainerStyle;
        private TableItemStyle _rightContainerStyle;
        private Style _buttonStyle;
        private Panel _contextPanel;
        private Panel _resourceCreatePanel;
        ResourceProperties _resourceProperties;
        private Button _saveButton;
        private Button _updateButton;
        private Button _cancelButton;
        private Button _addMenuButton;
        private Button _editMenuButton;
        private Button _deleteMenuButton;
        private Button _cutMenuButton;
        private Button _pasteMenuButton;
        private DropDownList _rootNodesDropDownList;

        #endregion Private

        #endregion Member variables

        #region Enums

        #region Private

        enum UserOperationMode
        {
            View,
            Add,
            Edit,
            Delete,
            Cut,
            Paste
        }

        #endregion

        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets the tree view header.
        /// </summary>
        public override string TreeViewCaption
        {
            get
            {
                return ViewState[_headerViewStateKey] != null ?
                    ViewState[_headerViewStateKey].ToString() : GlobalResource.DefaultCategoryHierarchyCaption;
            }
            set
            {
                ViewState[_headerViewStateKey] = value;
            }
        }

        /// <summary>
        /// Get or set tree node navigation URL.
        /// </summary>
        public string TreeNodeNavigationUrl
        {
            get
            {
                return ViewState[_treeNodeNavigationURLViewState] != null ?
                    ViewState[_treeNodeNavigationURLViewState].ToString() : _defaultNavigationURL;
            }
            set
            {
                ViewState[_treeNodeNavigationURLViewState] = value;
            }
        }

        /// <summary>
        /// Get or set default context menu button style.
        /// </summary>
        [Bindable(true)]
        [ZentityCategory("CategoryAppearance")]
        [ZentityDescription("DescriptionContextDefaultStyle")]
        [DefaultValue("")]
        [Localizable(true)]
        public string DefaultContextButtonStyle
        {
            get
            {
                return ViewState[_defaultButtonViewState] != null ?
                    ViewState[_defaultButtonViewState].ToString() : GlobalResource.DefaultContextButtonStyle;
            }
            set
            {
                ViewState[_defaultButtonViewState] = value;
            }
        }

        /// <summary>
        /// Get or set Highlighted context menu button style.
        /// </summary>
        [Bindable(true)]
        [ZentityCategory("CategoryAppearance")]
        [ZentityDescription("DescriptionContextHighLightStyle")]
        [DefaultValue("")]
        [Localizable(true)]
        public string SelectedContextButtonStyle
        {
            get
            {
                return ViewState[_selectedButtonViewState] != null ?
                    ViewState[_selectedButtonViewState].ToString() : GlobalResource.HighLightContextButtonStyle;
            }
            set
            {
                ViewState[_selectedButtonViewState] = value;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of the ResourceProperty row style.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryApperance"),
        ZentityDescription("DescriptionZentityTableRowStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ResourcePropertyRowStyle
        {
            get
            {
                if (this._rowStyle == null)
                {
                    this._rowStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._rowStyle).TrackViewState();
                    }
                }
                return this._rowStyle;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of the ResourceProperty alternate row style.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryApperance"),
        ZentityDescription("DescriptionZentityTableAlternateRowStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ResourcePropertyAlternateRowStyle
        {
            get
            {
                if (this._alternateRowStyle == null)
                {
                    this._alternateRowStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._alternateRowStyle).TrackViewState();
                    }
                }
                return this._alternateRowStyle;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of the header row.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryApperance"),
        ZentityDescription("DescriptionZentityTableHeaderStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ResourcePropertyHeaderStyle
        {
            get
            {
                if (this._resourceProeprtyHeaderStyle == null)
                {
                    this._resourceProeprtyHeaderStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._resourceProeprtyHeaderStyle).TrackViewState();
                    }
                }
                return this._resourceProeprtyHeaderStyle;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="Style" /> object that allows you to set the appearance 
        /// of Resource properties title.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryApperance"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public Style ResourcePropertyTitleStyle
        {
            get
            {
                if (this._resourceProeprtyTitleStyle == null)
                {
                    this._resourceProeprtyTitleStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._resourceProeprtyTitleStyle).TrackViewState();
                    }
                }
                return this._resourceProeprtyTitleStyle;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="Style" /> object that allows you to set the appearance 
        /// of buttons.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryApperance"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public Style ButtonStyle
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
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of left container.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryApperance"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle LeftContainerStyle
        {
            get
            {
                if (this._leftContainerStyle == null)
                {
                    this._leftContainerStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._leftContainerStyle).TrackViewState();
                    }
                }
                return this._leftContainerStyle;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of right container.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryApperance"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle RightContainerStyle
        {
            get
            {
                if (this._rightContainerStyle == null)
                {
                    this._rightContainerStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._rightContainerStyle).TrackViewState();
                    }
                }
                return this._rightContainerStyle;
            }
        }

        #endregion Public

        #region Private

        private DropDownList RootNodesDropDownList
        {
            get
            {
                if (_rootNodesDropDownList == null)
                {
                    _rootNodesDropDownList = new DropDownList();
                    _rootNodesDropDownList.ID = _rootNodeDropDownListId;
                    _rootNodesDropDownList.DataMember = _rootNodeDDLDataMember;
                    _rootNodesDropDownList.DataTextField = _rootNodeDDLTextField;
                    _rootNodesDropDownList.DataValueField = _rootNodeDDLValueField;
                    _rootNodesDropDownList.Width = new Unit(180);
                    _rootNodesDropDownList.AutoPostBack = true;
                }

                return _rootNodesDropDownList;
            }
        }

        private Panel ContextPanel
        {
            get
            {
                if (_contextPanel == null)
                {
                    _contextPanel = new Panel();
                    _contextPanel.ID = _contextPanelId;
                    _contextPanel.Attributes.Add("class", "contextPanel");
                }

                return _contextPanel;
            }
        }

        private Panel ResourcePropertiesPanel
        {
            get
            {
                if (_resourceCreatePanel == null)
                {
                    _resourceCreatePanel = new Panel();
                    _resourceCreatePanel.ID = _resourceCreatePanelId;
                }
                return _resourceCreatePanel;
            }
        }

        private ResourceProperties ResourcePropertiesControl
        {
            get
            {
                if (_resourceProperties == null)
                {
                    _resourceProperties = new ResourceProperties();
                    _resourceProperties.ID = _resourcePropertiesControlId;
                    _resourceProperties.ControlMode = ResourcePropertiesOperationMode.Add;
                    _resourceProperties.ValidationRequired = true;
                    _resourceProperties.ValidationGroup = _validationGroup;
                    _resourceProperties.ResourceType = _resourceType;
                    _resourceProperties.Title = GlobalResource.CategoryNodeProperty;
                }

                return _resourceProperties;
            }
        }

        private Button SaveButton
        {
            get
            {
                if (_saveButton == null)
                {
                    _saveButton = CreateDefaultButton(_saveButtonId, GlobalResource.SaveButton);
                    _saveButton.ValidationGroup = _validationGroup;
                    _saveButton.Click += new EventHandler(Save_Click);
                }

                return _saveButton;
            }
        }

        private Button UpdateButton
        {
            get
            {
                if (_updateButton == null)
                {
                    _updateButton = CreateDefaultButton(_updateButtonId, GlobalResource.UpdateButton);
                    _updateButton.ValidationGroup = _validationGroup;
                    _updateButton.Click += new EventHandler(Update_Click);
                }

                return _updateButton;
            }
        }

        private Button CancelButton
        {
            get
            {
                if (_cancelButton == null)
                {
                    _cancelButton = CreateDefaultButton(_cancelButtonId, GlobalResource.CancelButton);
                    _cancelButton.Click += new EventHandler(CancelButton_Click);
                }

                return _cancelButton;
            }
        }

        private Button AddMenuButton
        {
            get
            {
                if (_addMenuButton == null)
                {
                    _addMenuButton = CreateButton(_addButtonId, GlobalResource.AddCategoryNode);
                    _addMenuButton.Click += new EventHandler(AddButton_Click);
                }

                return _addMenuButton;
            }
        }

        private Button EditMenuButton
        {
            get
            {
                if (_editMenuButton == null)
                {
                    _editMenuButton = CreateButton(_editButtonId, GlobalResource.EditCategoryNode);
                    _editMenuButton.Click += new EventHandler(EditButton_Click);
                }

                return _editMenuButton;
            }
        }

        private Button DeleteMenuButton
        {
            get
            {
                if (_deleteMenuButton == null)
                {
                    _deleteMenuButton = CreateButton(_deleteButtonId, GlobalResource.DeleteCategoryNode);
                    _deleteMenuButton.Click += new EventHandler(DeleteButton_Click);
                    _deleteMenuButton.Attributes.Add(_onClickEvent,
                        string.Format(CultureInfo.CurrentCulture, _deleteResourceFunction,
                        GlobalResource.DeletecategoryNodeConfirmation));
                }

                return _deleteMenuButton;
            }
        }

        private Button CutMenuButton
        {
            get
            {
                if (_cutMenuButton == null)
                {
                    _cutMenuButton = CreateButton(_cutButtonId, GlobalResource.CutCategoryNode);
                    _cutMenuButton.Click += new EventHandler(CutButton_Click);
                }

                return _cutMenuButton;
            }
        }

        private Button PasteMenuButton
        {
            get
            {
                if (_pasteMenuButton == null)
                {
                    _pasteMenuButton = CreateButton(_pasteButtonId, GlobalResource.PasteCategoryNode);
                    _pasteMenuButton.Click += new EventHandler(PasteButton_Click);
                }

                return _pasteMenuButton;
            }
        }

        private Guid SelectedNodeId
        {
            get
            {
                return ViewState[_parentNodeViewState] != null ?
                    (Guid)ViewState[_parentNodeViewState] : Guid.Empty;
            }

            set
            {
                ViewState[_parentNodeViewState] = value;
            }
        }

        private Guid SelectedCutNodeId
        {
            get
            {
                return ViewState[_cutNodeViewState] != null ?
                    (Guid)ViewState[_cutNodeViewState] : Guid.Empty;
            }

            set
            {
                ViewState[_cutNodeViewState] = value;
            }
        }

        #endregion

        #endregion Properties

        #region Methods

        #region Protected

        /// <summary>
        /// Creates the layout of child controls.
        /// </summary>
        /// <param name="e">Event Arguments.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ContainerPanel.Controls.Remove(TreeViewPanel);

            ContainerPanel.Style.Add(HtmlTextWriterStyle.Width, GlobalResource.CssControlWidth100Percent);
            GetTreeViewControl();
            ContainerPanel.Controls.Add(GetTreeViewControl());
            ContainerPanel.Controls.Add(GetResourceControl());

            if (!this.DesignMode)
            {
                this.Controls.Add(ContextPanel);
                CreateContextMenuForNodes();
            }
        }

        /// <summary>
        /// Updates the status of child controls based on current state.
        /// </summary>
        /// <param name="e">Event Arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
            if (!DesignMode)
            {
                //Refresh RootNodesDropDownList.
                if (!Page.IsPostBack)
                {
                    PopulateRootNodeDropDownList();
                }
                else
                {
                    PopulateRootNodeDropDownList(RootNodesDropDownList.SelectedValue);
                }

                if (RootNodesDropDownList.Items.Count <= 1)
                {
                    ShowMessage(GlobalResource.MsgCategoriesNotPresent, false);
                }

                //Update status of the controls.
                //Selected operation is Create New RootNode.
                if (string.IsNullOrEmpty(RootNodesDropDownList.SelectedValue))
                {
                    SelectedNodeId = Guid.Empty;
                    TreeViewPanel.Visible = false;
                    RootNodesDropDownList.Enabled = true;
                    //If control is in security  mode and user is having add permission then only allow user to Add new node.
                    if (IsSecurityAwareControl &&
                        !IsOperationAllowedToUser(UserOperationMode.Add, AuthenticatedToken, Guid.Empty, Guid.Empty))
                    {

                        ResourcePropertiesPanel.Visible = false;
                        ShowMessage(string.Format(CultureInfo.InvariantCulture,
                                GlobalResource.UnauthorizedAccessExceptionCreate, UserResourcePermissions.Create), true);
                    }
                    else
                    {
                        ResourcePropertiesPanel.Visible = true;
                        UpdateResourcePropertiesControlStatus(ResourcePropertiesOperationMode.Add);
                    }

                }
                else if (SelectedNodeId == Guid.Empty) //No operation is selected.
                {
                    ResourcePropertiesPanel.Visible = false;
                    RootNodesDropDownList.Enabled = true;
                }
                else //next possible operation is Update/Save.
                {
                    ResourcePropertiesPanel.Visible = true;
                    RootNodesDropDownList.Enabled = false;
                }
                if (SelectedCutNodeId == Guid.Empty)
                    PasteMenuButton.Enabled = false;
                else
                    PasteMenuButton.Enabled = true;
            }

            this.Page.ClientScript.RegisterClientScriptInclude(this.GetType(), _javascriptFile, Page.ClientScript.GetWebResourceUrl(this.GetType(), _categoryHierarchyScriptPath));

            base.OnPreRender(e);
        }

        /// <summary>
        /// Fetches data source.
        /// </summary>
        protected override void GetDataSource()
        {

        }

        /// <summary>
        /// Populates tree view from data source.
        /// </summary>
        protected override void PopulateTree()
        {
            string selectedNodeId = string.Empty;

            if (TreeView.SelectedNode != null)
            {
                selectedNodeId = TreeView.SelectedNode.Value;
            }

            if (!string.IsNullOrEmpty(RootNodesDropDownList.SelectedValue))
            {
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    CategoryNode selectedNode = dataAccess.GetCategoryNodeWithHierarchy(new Guid(RootNodesDropDownList.SelectedValue));

                    BindTreeView(selectedNode);
                }
            }

            if (TreeView.Nodes.Count > 0 && !string.IsNullOrEmpty(selectedNodeId))
                SelectNode(TreeView, TreeView.Nodes[0], selectedNodeId);
        }

        /// <summary>
        /// Deletes selected categoryNode hierarchy from database.
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        /// <param name="e">Event arguments.</param>
        protected void DeleteButton_Click(object sender, EventArgs e)
        {
            SelectedNodeId = new Guid(TreeView.SelectedNode.Value);

            //If control is in security mode and user is not having Delete permission on 
            //selected node then don't allow user to Delete the Node
            if (IsSecurityAwareControl &&
                !IsOperationAllowedToUser(UserOperationMode.Delete, AuthenticatedToken, SelectedNodeId, Guid.Empty))
            {
                ShowMessage(GlobalResource.UnauthorizedAccessExceptionCategoryDelete, true);
            }
            else
            {
                bool success = false;

                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    success = dataAccess.DeleteCategoryNodeWithHierarchy(SelectedNodeId);
                }

                if (success)
                {
                    ShowMessage(GlobalResource.MsgDeletedCategory, false);
                }
                else
                {
                    ShowMessage(GlobalResource.MsgDeleteCategoryOperationFailed, true);
                }
            }

            if (SelectedNodeId == SelectedCutNodeId)
            {
                SelectedCutNodeId = Guid.Empty;
            }

            SelectedNodeId = Guid.Empty;
        }

        /// <summary>
        /// Shows resource property control in Add mode.
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        /// <param name="e">Event arguments.</param>
        protected void AddButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TreeView.SelectedNode.Value))
            {
                bool isValid = true;

                //If control is in security mode and user is not having Add permission and Update permission on parent node 
                //then don't allow user to Add new Node
                if (IsSecurityAwareControl)
                {
                    if (!IsOperationAllowedToUser(UserOperationMode.Add, AuthenticatedToken, Guid.Empty, Guid.Empty))
                    {
                        ShowMessage(string.Format(CultureInfo.InvariantCulture,
                                    GlobalResource.UnauthorizedAccessExceptionCreate, UserResourcePermissions.Create), true);
                        isValid = false;
                    }
                    else if (!IsOperationAllowedToUser(UserOperationMode.Edit, AuthenticatedToken,
                        new Guid(TreeView.SelectedNode.Value), Guid.Empty))
                    {
                        ShowMessage(GlobalResource.UnauthorizedAccessExceptionUpdateParentCategory, true);
                        isValid = false;
                    }
                }

                if (isValid)
                {
                    SelectedNodeId = new Guid(TreeView.SelectedNode.Value);
                    UpdateResourcePropertiesControlStatus(ResourcePropertiesOperationMode.Add);
                }
            }
        }

        /// <summary>
        /// Shows resource property control in Edit mode.
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        /// <param name="e">Event arguments.</param>
        protected void EditButton_Click(object sender, EventArgs e)
        {
            SelectedNodeId = new Guid(TreeView.SelectedNode.Value);

            //If control is in security mode and user is not having update permission on 
            //selected node then don't allow user to edit selected node.
            if (IsSecurityAwareControl &&
                !IsOperationAllowedToUser(UserOperationMode.Edit, AuthenticatedToken, SelectedNodeId, Guid.Empty))
            {
                SelectedNodeId = Guid.Empty;
                ShowMessage(string.Format(CultureInfo.InvariantCulture,
                                GlobalResource.UnauthorizedAccessException, UserResourcePermissions.Update), true);
            }
            else
            {
                UpdateResourcePropertiesControlStatus(ResourcePropertiesOperationMode.Edit);
            }
        }

        /// <summary>
        /// Cancels the selected operation and hid resource property control.
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        /// <param name="e">Event arguments.</param>
        protected void CancelButton_Click(object sender, EventArgs e)
        {
            SelectedNodeId = Guid.Empty;
            //If canceled operation is create new root node then select the first root node
            //in the RootNodeDropDownList by default.
            if (string.IsNullOrEmpty(RootNodesDropDownList.SelectedValue))
            {
                RootNodesDropDownList.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Cuts the selected category node from database.
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        /// <param name="e">Event arguments.</param>
        protected void CutButton_Click(object sender, EventArgs e)
        {
            Guid nodeId = new Guid(TreeView.SelectedNode.Value);

            //If control is in security mode and user is not having update permission on 
            //selected node then don't allow user to perform cut-paste operation on selected node.
            if (IsSecurityAwareControl &&
                !IsOperationAllowedToUser(UserOperationMode.Cut, AuthenticatedToken, nodeId, Guid.Empty))
            {
                ShowMessage(string.Format(CultureInfo.InvariantCulture,
                                GlobalResource.UnauthorizedAccessException, UserResourcePermissions.Update), true);
            }
            else
            {
                SelectedCutNodeId = nodeId;
            }

        }

        /// <summary>
        /// Pastes category node in selected hierarchy.
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        /// <param name="e">Event arguments.</param>
        protected void PasteButton_Click(object sender, EventArgs e)
        {
            if (SelectedCutNodeId != null)
            {
                Guid targetId = new Guid(TreeView.SelectedNode.Value);

                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    //If control is in security mode and user is not having update permission on 
                    //selected node then don't allow user to perform cut-paste operation on selected node.
                    if (IsSecurityAwareControl &&
                        !IsOperationAllowedToUser(UserOperationMode.Paste, AuthenticatedToken, SelectedCutNodeId, targetId))
                    {
                        ShowMessage(string.Format(CultureInfo.InvariantCulture,
                                GlobalResource.UnauthorizedAccessException, UserResourcePermissions.Update), true);
                    }
                    else
                    {
                        CategoryNode categoryChildNode = dataAccess.GetCategoryNode(SelectedCutNodeId);

                        CategoryNode categoryParentNode = dataAccess.GetCategoryNode(targetId);

                        //Check if parent node is in hierarchy of child node.
                        if (CheckIsInHierarchy(categoryChildNode, categoryParentNode))
                        {
                            ShowMessage(GlobalResource.CutPasteError, true);
                        }
                        else
                        {
                            //Save Child parent Relationship.
                            categoryChildNode.Parent = categoryParentNode;
                            dataAccess.SaveResource(true, categoryParentNode);

                            ShowMessage(GlobalResource.CutPateCategory, false);
                        }
                    }

                    SelectedCutNodeId = Guid.Empty;
                }
            }
        }

        /// <summary>
        /// Updates the selected category node in database.
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        /// <param name="e">Event arguments.</param>
        protected void Update_Click(object sender, EventArgs e)
        {
            if (SelectedNodeId != Guid.Empty)
            {
                //If control is in security mode and user is not having update permission on 
                //selected node then don't allow user to update selected node.
                if (IsSecurityAwareControl &&
                    !IsOperationAllowedToUser(UserOperationMode.Edit, AuthenticatedToken, SelectedNodeId, Guid.Empty))
                {
                    SelectedNodeId = Guid.Empty;
                    ShowMessage(string.Format(CultureInfo.InvariantCulture,
                                    GlobalResource.UnauthorizedAccessException, UserResourcePermissions.Update), true);
                }
                else
                {
                    AssignControlMode(SelectedNodeId);
                    Resource updatedResource = ResourcePropertiesControl.GetResourceDetails();
                    ResourcePropertiesControl.SaveResourceProperties();

                    ShowMessage(string.Format(CultureInfo.CurrentCulture, GlobalResource.UpdatedCategory, CoreHelper.FitString(updatedResource.Title, _maxCharShown)), false);
                    SelectedNodeId = Guid.Empty;
                }
            }
        }

        /// <summary>
        /// Adds a new category node in database.
        /// </summary>
        /// <param name="sender">he object that raised this event.</param>
        /// <param name="e">Event arguments.</param>
        protected void Save_Click(object sender, EventArgs e)
        {
            bool isValid = true;

            //If control is in security mode and user is not having Add permission and Update permission on parent node 
            //then don't allow user to Add new Node
            if (IsSecurityAwareControl)
            {
                if (!IsOperationAllowedToUser(UserOperationMode.Add, AuthenticatedToken, Guid.Empty, Guid.Empty))
                {
                    ShowMessage(string.Format(CultureInfo.InvariantCulture,
                                GlobalResource.UnauthorizedAccessExceptionCreate, UserResourcePermissions.Create), true);
                    isValid = false;
                }
                else if (SelectedNodeId != Guid.Empty && !IsOperationAllowedToUser(UserOperationMode.Edit, AuthenticatedToken, SelectedNodeId, Guid.Empty))
                {
                    ShowMessage(GlobalResource.UnauthorizedAccessExceptionUpdateParentCategory, true);
                    isValid = false;
                }
            }

            if (!isValid)
            {
                SelectedNodeId = Guid.Empty;
            }
            else
            {
                CategoryNode newNode = (CategoryNode)ResourcePropertiesControl.GetResourceDetails();

                //Save new Category Node.
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    if (SelectedNodeId != Guid.Empty)
                    {
                        newNode.Parent = dataAccess.GetCategoryNode(SelectedNodeId);
                    }
                    else
                    {
                        newNode.Parent = null;
                    }

                    bool success = dataAccess.SaveResource(false, newNode);
                    if (success && IsSecurityAwareControl)
                    {
                        dataAccess.GrantDefaultOwnership(AuthenticatedToken, newNode.Id);
                    }
                }

                //If new CategoryNode is root node.
                if (SelectedNodeId == Guid.Empty)
                {
                    PopulateRootNodeDropDownList(newNode.Id.ToString());
                }

                SelectedNodeId = Guid.Empty;

                ShowMessage(string.Format(CultureInfo.CurrentCulture, GlobalResource.AddedCategory, CoreHelper.FitString(newNode.Title, _maxCharShown)), false);
            }
        }

        /// <summary>
        /// Handles SelectedNodeChanged event of tree view.
        /// </summary>
        /// <param name="sender">Originator of event.</param>
        /// <param name="e">Event Arguments.</param>
        void _treeView_SelectedNodeChanged(object sender, EventArgs e)
        {
            if (IsSecurityAwareControl &&
                !IsOperationAllowedToUser(UserOperationMode.View, AuthenticatedToken, new Guid(TreeView.SelectedNode.Value), Guid.Empty))
            {
                ShowMessage(string.Format(CultureInfo.InvariantCulture,
                               GlobalResource.UnauthorizedAccessException, UserResourcePermissions.Read), true);
            }
            else
            {
                this.Page.Response.Redirect(string.Format(CultureInfo.CurrentCulture,
                    TreeNodeNavigationUrl, TreeView.SelectedNode.Value));
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Updates the status of ResourceProperties control.
        /// </summary>
        /// <param name="operationMode">Operation Mode.</param>
        private void UpdateResourcePropertiesControlStatus(
            ResourcePropertiesOperationMode operationMode)
        {
            if (operationMode == ResourcePropertiesOperationMode.Edit)
            {
                SaveButton.Visible = false;
                UpdateButton.Visible = true;
                ResourcePropertiesControl.ClearControls();

                PopulateResourceInEdit(SelectedNodeId);
            }
            else if (operationMode == ResourcePropertiesOperationMode.Add)
            {
                UpdateButton.Visible = false;
                SaveButton.Visible = true;
                ResourcePropertiesControl.ClearControls();
            }

            if (operationMode == ResourcePropertiesOperationMode.Add)
                ContainerPanel.DefaultButton = SaveButton.ID;
            else
                ContainerPanel.DefaultButton = UpdateButton.ID;
        }

        /// <summary>
        /// Populate DropDownList with available root nodes and select 
        /// the item with the specified value.
        /// </summary>
        /// <param name="selectedValue">value of a item to be selected.</param>
        private void PopulateRootNodeDropDownList(string selectedValue)
        {
            PopulateRootNodeDropDownList();
            SelectItemInDropDownList(selectedValue);
        }

        /// <summary>
        /// Select the item in the dropdown with the specified value.
        /// </summary>
        /// <param name="selectedValue">Value of a item to be selected.</param>
        private void SelectItemInDropDownList(string selectedValue)
        {
            int selectedIndex = 0;

            if (RootNodesDropDownList.Items.Count > 1)
            {
                foreach (ListItem item in RootNodesDropDownList.Items)
                {
                    if (selectedValue == item.Value)
                    {
                        RootNodesDropDownList.SelectedIndex = selectedIndex;
                        break;
                    }

                    selectedIndex++;
                }
            }
        }

        /// <summary>
        /// Populate DropDownList with available root nodes.
        /// </summary>
        private void PopulateRootNodeDropDownList()
        {
            IList<CategoryNode> rootNodeList = null;
            RootNodesDropDownList.Items.Clear();

            using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
            {
                rootNodeList = dataAccess.GetRootCategoryNodes();
            }

            if (rootNodeList != null && rootNodeList.Count > 0)
            {
                //Adjust length of text to be displayed in the DropDown.
                foreach (CategoryNode node in rootNodeList)
                {
                    string title = CoreHelper.UpdateEmptyTitle(node.Title);
                    if (title.Length > _maxCharShownTree)
                    {
                        node.Title = CoreHelper.FitString(title, _maxCharShownTree);
                    }
                    else
                    {
                        node.Title = title;
                    }
                }
                RootNodesDropDownList.DataSource = rootNodeList;
                RootNodesDropDownList.DataBind();
            }
            //Item to provide option to create new Root CategoryNode.
            RootNodesDropDownList.Items.Add(new ListItem(GlobalResource.CreateNewRootCategoryNode, string.Empty));
            RootNodesDropDownList.SelectedIndex = 0;
        }

        /// <summary>
        /// Get resource property control with Save\Edit\Update button.
        /// </summary>
        /// <returns>returns resource property control table</returns>
        private Panel GetResourceControl()
        {
            if (!this.DesignMode)
            {
                ResourcePropertiesControl.IsSecurityAwareControl = false;
                ResourcePropertiesControl.TitleStyle.MergeWith(ResourcePropertyTitleStyle);
                ResourcePropertiesControl.HeaderStyle = ResourcePropertyHeaderStyle;
                ResourcePropertiesControl.RowStyle = ResourcePropertyRowStyle;
                ResourcePropertiesControl.AlternateRowStyle = ResourcePropertyAlternateRowStyle;
                ResourcePropertiesPanel.Controls.Add(ResourcePropertiesControl);
            }

            Panel buttonPanel = new Panel();
            buttonPanel.HorizontalAlign = HorizontalAlign.Left;
            SaveButton.ApplyStyle(ButtonStyle);
            UpdateButton.ApplyStyle(ButtonStyle);
            CancelButton.ApplyStyle(ButtonStyle);
            buttonPanel.Controls.Add(SaveButton);
            buttonPanel.Controls.Add(UpdateButton);
            buttonPanel.Controls.Add(CancelButton);
            ResourcePropertiesPanel.Controls.Add(buttonPanel);

            ResourcePropertiesPanel.ApplyStyle(RightContainerStyle);

            return ResourcePropertiesPanel;
        }

        /// <summary>
        /// Create TreeViewPanel.
        /// </summary>
        /// <returns>Panel</returns>
        private Panel GetTreeViewControl()
        {
            Panel leftPanel = new Panel();
            leftPanel.Controls.Add(RootNodesDropDownList);
            leftPanel.Controls.Add(TreeViewPanel);
            TreeView.SelectedNodeChanged += new EventHandler(_treeView_SelectedNodeChanged);

            leftPanel.ApplyStyle(LeftContainerStyle);

            return leftPanel;
        }

        /// <summary>
        /// Create context menu for node.
        /// </summary>
        private void CreateContextMenuForNodes()
        {
            ContextPanel.Attributes.Add(_onMouseOutEvent, string.Format(CultureInfo.CurrentCulture,
                _hideContextMenuFunction, ContextPanel.ClientID));

            AddMenuButton.CssClass = DefaultContextButtonStyle;
            EditMenuButton.CssClass = DefaultContextButtonStyle;
            DeleteMenuButton.CssClass = DefaultContextButtonStyle;
            CutMenuButton.CssClass = DefaultContextButtonStyle;
            PasteMenuButton.CssClass = DefaultContextButtonStyle;

            HtmlGenericControl space = new HtmlGenericControl();
            space.InnerHtml = _space;
            HtmlGenericControl space1 = new HtmlGenericControl();
            space.InnerHtml = _space;

            ContextPanel.Controls.Add(AddMenuButton);
            ContextPanel.Controls.Add(EditMenuButton);
            ContextPanel.Controls.Add(space1);
            ContextPanel.Controls.Add(DeleteMenuButton);
            ContextPanel.Controls.Add(CutMenuButton);
            ContextPanel.Controls.Add(PasteMenuButton);
        }

        /// <summary>
        /// Create button control with specific id, text and style.
        /// </summary>
        /// <param name="id">id for the button</param>
        /// <param name="text">text for the button</param>
        /// <returns>Button.</returns>
        private Button CreateButton(string id, string text)
        {
            Button button = new Button();
            button.ID = id;
            button.Text = text;
            button.Width = new Unit(_contextButtonWidth, CultureInfo.InvariantCulture);
            button.Attributes.Add(_onMouseOverEvent, string.Format(CultureInfo.CurrentCulture,
                _selectMenuFunction, SelectedContextButtonStyle));
            button.Attributes.Add(_onMouseOutEvent, string.Format(CultureInfo.CurrentCulture,
                _deselectMenuFunction, DefaultContextButtonStyle));
            return button;
        }

        /// <summary>
        /// Create and initialize Button object with given parameters.
        /// </summary>
        /// <param name="id">Button Id.</param>
        /// <param name="text">Button Text.</param>
        /// <returns></returns>
        private static Button CreateDefaultButton(string id, string text)
        {
            Button button = new Button();
            button.ID = id;
            button.Text = text;
            return button;
        }

        /// <summary>
        /// Check if categoryChild node already exist in the categoryParent node.
        /// </summary>
        /// <param name="categoryChildNode">category node on cut operation perform</param>
        /// <param name="categoryParentNode">category node on paste operation perform</param>
        /// <returns></returns>
        private bool CheckIsInHierarchy(CategoryNode categoryChildNode, CategoryNode categoryParentNode)
        {
            bool isInHierarchy = false;

            if (categoryChildNode.Id == categoryParentNode.Id)
                isInHierarchy = true;
            else
            {
                categoryChildNode = ResourcePropertiesControl.GetCategoryNode(categoryChildNode.Id);
                foreach (CategoryNode currentCategory in categoryChildNode.Children)
                {
                    if (currentCategory.Id.ToString() == categoryParentNode.Id.ToString())
                    {
                        isInHierarchy = true;
                    }
                    else
                    {
                        isInHierarchy = CheckIsInHierarchy(currentCategory, categoryParentNode);
                    }

                    if (isInHierarchy)
                        break;
                }
            }

            return isInHierarchy;
        }

        /// <summary>
        /// Populate resource property control in edit mode.
        /// </summary>
        private void PopulateResourceInEdit(Guid selectedNodeId)
        {
            AssignControlMode(selectedNodeId);
            CategoryNode resourceNode = (CategoryNode)ResourcePropertiesControl.GetResourceData(selectedNodeId);
            if (resourceNode != null)
            {
                ResourcePropertiesControl.PopulateHashtable(resourceNode);
                ResourcePropertiesControl.PopulateControls();

            }
        }

        /// <summary>
        /// Assign control mode for the resource property control.
        /// </summary>
        private void AssignControlMode(Guid selectedNodeId)
        {
            ResourcePropertiesControl.ControlMode = ResourcePropertiesOperationMode.Edit;
            if (selectedNodeId != Guid.Empty)
                ResourcePropertiesControl.ResourceId = selectedNodeId;
        }

        /// <summary>
        /// Show operation perform message.
        /// </summary>
        /// <param name="message">message to shown on UI</param>
        /// <param name="isError">Indicates if error message is to be displayed.</param>
        private void ShowMessage(string message, bool isError)
        {
            if (isError)
                HeaderMessageLabel.ForeColor = Color.Red;
            else
                HeaderMessageLabel.ForeColor = Color.Black;

            HeaderMessageLabel.Text = message;
        }

        /// <summary>
        /// Bind tree view control with category node hierarchy.
        /// </summary>
        /// <param name="rootNode">Root node.</param>
        private void BindTreeView(CategoryNode rootNode)
        {
            TreeViewPanel.Visible = true;
            TreeView.Nodes.Clear();
            PopulateRoot(TreeView, rootNode);
            TreeView.ExpandAll();
        }

        /// <summary>
        /// Populate category node tree view hierarchy.
        /// </summary>
        /// <param name="treeView">Tree view to populate.</param>
        /// <param name="top">root category node</param>
        private void PopulateRoot(TreeView treeView, CategoryNode top)
        {
            TreeNode root = new TreeNode(top.Title);
            root.ToolTip = top.Title;
            string categoryTitle = HttpUtility.HtmlEncode(CoreHelper.FitString(
                CoreHelper.UpdateEmptyTitle(root.Text), _maxCharShownTree));
           
            StringBuilder rootText = new StringBuilder();
            rootText.AppendFormat(CultureInfo.InvariantCulture, _nodeTextScript,
                "\"" + _imageId + "\"",
                "\"" + ContextPanel.ClientID + "\"",
                "\"" + _imageId + "\"",
                categoryTitle,
                _imageId, Page.ClientScript.GetWebResourceUrl(this.GetType(), _noImagePath),
                "\"" + ContextPanel.ClientID + "\"", "\"" + _imageId + "\"",
                "\"" + ContextPanel.ClientID + "\"");
            root.Text = rootText.ToString();
            root.Value = top.Id.ToString();
            root.SelectAction = TreeNodeSelectAction.Select;
           
            treeView.Nodes.Add(root);

            PopulateTreeHierarcy(top, root);
        }

        /// <summary>
        /// populate tree view hierarchy.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parentNode"></param>
        private void PopulateTreeHierarcy(CategoryNode node, TreeNode parentNode)
        {
            foreach (CategoryNode current in node.Children.ToList())
            {
                TreeNode child = new TreeNode();
                child.SelectAction = TreeNodeSelectAction.Select;
                child.ToolTip = current.Title;
                string categoryTitle = HttpUtility.HtmlEncode(CoreHelper.FitString(
                    CoreHelper.UpdateEmptyTitle(current.Title), _maxCharShownTree));
              
                StringBuilder childText = new StringBuilder();
                childText.AppendFormat(CultureInfo.InvariantCulture, _nodeTextScript,
                    "\"" + _imageId + current.Id.ToString() + "\"",
                    "\"" + ContextPanel.ClientID + "\"",
                    "\"" + _imageId + current.Id.ToString() + "\"",
                    categoryTitle,
                    _imageId + current.Id.ToString(),
                    Page.ClientScript.GetWebResourceUrl(this.GetType(), _noImagePath),
                    "\"" + ContextPanel.ClientID + "\"",
                    "\"" + _imageId + current.Id.ToString() + "\"",
                    "\"" + ContextPanel.ClientID + "\"");
                child.Text = childText.ToString();
                child.Value = current.Id.ToString();
              
                parentNode.ChildNodes.Add(child);
                PopulateTreeHierarcy(current, child);
            }
        }

        /// <summary>
        /// Set node as selected based on specified node Id.
        /// </summary>
        /// <param name="tree">TreeView</param>
        /// <param name="parentNode">Parent TreeNode</param>
        /// <param name="selectedNodeId">Selected Node Id</param>
        /// <returns>true if node selected, else false.</returns>
        private bool SelectNode(TreeView tree, TreeNode parentNode, string selectedNodeId)
        {
            bool success = false;
            if (tree != null && parentNode != null && !string.IsNullOrEmpty(selectedNodeId))
            {
                if (selectedNodeId.Equals(parentNode.Value))
                {
                    parentNode.Select();
                    success = true;
                }
                else
                {
                    foreach (TreeNode childNode in parentNode.ChildNodes)
                    {
                        SelectNode(tree, childNode, selectedNodeId);
                        success = true;
                    }
                }
            }

            return success;
        }

        /// <summary>
        /// Return true if login user have permission.
        /// </summary>
        /// <param name="operation">Operation mode</param>
        /// <param name="token">Token</param>
        /// <param name="nodeId">CategoryNode Id</param>
        /// <param name="targetNodeId">Target CategoryNodeId</param>
        /// <returns></returns>
        private bool IsOperationAllowedToUser(UserOperationMode operation, AuthenticatedToken token, Guid nodeId, Guid targetNodeId)
        {
            bool operationAllowed = false;

            using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
            {
                switch (operation)
                {
                    case UserOperationMode.View:
                        if (dataAccess.AuthorizeUser(token, UserResourcePermissions.Read, nodeId))
                        {
                            operationAllowed = true;
                        }
                        break;
                    case UserOperationMode.Add:
                        if (dataAccess.HasCreatePermission(token))
                        {
                            operationAllowed = true;
                        }
                        break;
                    case UserOperationMode.Edit:
                        if (dataAccess.AuthorizeUser(token, UserResourcePermissions.Update, nodeId))
                        {
                            operationAllowed = true;
                        }
                        break;
                    case UserOperationMode.Delete:
                        if (dataAccess.AuthorizeUserForDeletePermissionOnCategory(token, nodeId))
                        {
                            operationAllowed = true;
                        }
                        break;
                    case UserOperationMode.Cut:
                        CategoryNode catNode = dataAccess.GetCategoryNode(nodeId);
                        if (dataAccess.AuthorizeUser(token, UserResourcePermissions.Update, nodeId) &&
                            (catNode.Parent == null || dataAccess.AuthorizeUser(token, UserResourcePermissions.Update, catNode.Parent.Id)))
                        {
                            operationAllowed = true;
                        }
                        break;
                    case UserOperationMode.Paste:
                        CategoryNode childCatNode = dataAccess.GetCategoryNode(nodeId);
                        if (dataAccess.AuthorizeUser(token, UserResourcePermissions.Update, nodeId) &&
                            dataAccess.AuthorizeUser(token, UserResourcePermissions.Update, targetNodeId) &&
                            (childCatNode.Parent == null || dataAccess.AuthorizeUser(token, UserResourcePermissions.Update, childCatNode.Parent.Id)))
                        {
                            operationAllowed = true;
                        }
                        break;
                }
            }

            return operationAllowed;
        }

        #endregion

        #endregion

    }
  

}
