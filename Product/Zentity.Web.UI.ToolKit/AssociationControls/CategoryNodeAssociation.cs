// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Zentity.ScholarlyWorks;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This control displays a tree view of categories. Each category has a checkbox to get/set whether the 
    /// category is associated or not.
    /// </summary>
    /// <example>
    ///     The code below is the source for CategoryNodeAssociation.aspx.
    ///     It shows an example of using <see cref="CategoryNodeAssociation"/> control.
    ///     <code>
    ///         &lt;%@ Page Language="C#" AutoEventWireup="true" %&gt;
    ///         
    ///         &lt;%@ Register Assembly="Zentity.Web.UI.ToolKit" Namespace="Zentity.Web.UI.ToolKit" 
    ///             tagPrefix="Zentity" %&gt;
    ///         &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"&gt;
    ///         &lt;html xmlns="http://www.w3.org/1999/xhtml"&gt;
    ///         &lt;head id="Head1" runat="server"&gt;
    ///             &lt;title&gt;CategoryNodeAssociation Sample&lt;/title&gt;
    ///              
    ///             &lt;script runat="server"&gt;
    ///                 protected void Page_Load(object sender, EventArgs e)
    ///                 {
    ///                     string id = Convert.ToString(Request.QueryString["Id"]);
    ///                     if (string.IsNullOrEmpty(id))
    ///                     {
    ///                         StatusLabel.Text = "Please pass an Id parameter (GUID) in the query string. Example: " +
    ///                             Request.Url.AbsolutePath + "?Id=6bd35f74-5a07-4df8-98a0-7ddb71f88c24";
    ///                         CategoryNodeAssociation1.Visible = false;
    ///                         SaveButton.Visible = false;
    ///                    }
    ///                     else
    ///                     {
    ///                         StatusLabel.Text = string.Empty;
    ///                         Guid entityId = new Guid(id);
    ///                         CategoryNodeAssociation1.ResourceItemId = entityId;
    ///                     }
    ///                 }
    ///                 
    ///                 protected void Save_Click(object sender, EventArgs e)
    ///                 {
    ///                    CategoryNodeAssociation1.SaveAssociation();
    ///                 }
    ///             &lt;/script&gt;
    ///         &lt;/head&gt;
    ///         &lt;body&gt;
    ///             &lt;form id="mainForm" runat="server"&gt;
    ///             &lt;div&gt;
    ///                 &lt;Zentity:CategoryNodeAssociation ID="CategoryNodeAssociation1" runat="server" TreeViewCaption="Associate CategoryNode" 
    ///                     IsSecurityAwareControl="false" TreeNodeNavigationUrl="ResourceDetailView.aspx?Id={0}" 
    ///                     SubjectNavigationUrl="ResourceDetailView.aspx?Id={0}" &gt;
    ///                 &lt;/Zentity:CategoryNodeAssociation&gt;
    ///                 &lt;br /&gt;
    ///                 &lt;asp:Button ID="SaveButton" runat="server" Text="Save" OnClick="Save_Click" /&gt;
    ///                 &lt;asp:Label ID="StatusLabel" runat="server" /&gt;
    ///             &lt;/div&gt;
    ///             &lt;/form&gt;
    ///         &lt;/body&gt;
    ///         &lt;/html&gt;
    ///     </code>
    /// </example>
    public class CategoryNodeAssociation : BaseTreeView
    {

        #region Constants

        #region Private

        const int _maxCharShown = 30;
        const int _maxCharShownTree = 20;
        const string _selectedListViewStateKey = "SelectedListViewStateKey";
        const string _resourceItemIdViewStateKey = "ResourceItemIdViewStateKey";
        const string _treeNodeNavigationUrlViewState = "TreeNodeNavigationUrlViewState";
        const string _defaultNavigationUrl = "#";
        const string _subjectNavigationUrlViewState = "subjectNavigationUrlViewState";
        const string _subjectLinkPanelId = "subjectLinkPanelId";

        #endregion Private

        #endregion Constants

        #region Member variables

        #region Private

        private IList<CategoryNode> _rootNodeList;
        private List<CategoryNode> _selectedNodeList = new List<CategoryNode>();
        private List<Guid> _selectedIdList = new List<Guid>();
        private ScholarlyWorkItem resource = new ScholarlyWorkItem();
        private HyperLink _subjectLink = new HyperLink();
        private Label _subjectLabel = new Label();
        Panel _subjectLinkPanel;
        #endregion Private

        #endregion Member variables

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets the list of selected category node items.
        /// </summary>
        [Browsable(false)]
        public IList<CategoryNode> SelectedList
        {
            get
            {
                return _selectedNodeList;
            }
        }

        /// <summary>
        /// Gets or sets the id of the item that has to be associated using this control.
        /// </summary>
        [Browsable(false)]
        public Guid ResourceItemId
        {
            get
            {
                return (ViewState[_resourceItemIdViewStateKey] != null) ?
                    (Guid)ViewState[_resourceItemIdViewStateKey] : Guid.Empty;
            }
            set
            {
                ViewState[_resourceItemIdViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the list of selected category node ids.
        /// </summary>
        [Browsable(false)]
        public List<Guid> SelectedIdList
        {
            get
            {
                return _selectedIdList;
            }
        }

        /// <summary>
        /// Gets or sets tree node navigation URL.
        /// </summary>
        public string TreeNodeNavigationUrl
        {
            get
            {
                return ViewState[_treeNodeNavigationUrlViewState] != null ?
                    ViewState[_treeNodeNavigationUrlViewState].ToString() : _defaultNavigationUrl;
            }
            set
            {
                ViewState[_treeNodeNavigationUrlViewState] = value;
            }
        }

        /// <summary>
        /// Gets or sets subject navigation URL.
        /// </summary>
        public string SubjectNavigationUrl
        {
            get
            {
                return ViewState[_subjectNavigationUrlViewState] != null ?
                    ViewState[_subjectNavigationUrlViewState].ToString() : _defaultNavigationUrl;
            }
            set
            {
                ViewState[_subjectNavigationUrlViewState] = value;
            }
        }

        #endregion Public

        #region Private

        private Panel SubjectLinkPanel
        {
            get
            {
                if (_subjectLinkPanel == null)
                {
                    _subjectLinkPanel = new Panel();
                    _subjectLinkPanel.Style.Add(HtmlTextWriterStyle.Margin, GlobalResource.CssSubjectLinkPanelMargin);
                    _subjectLinkPanel.ID = _subjectLinkPanelId;
                    
                    if(this.DesignMode)
                    {
                        _subjectLabel.Text = GlobalResource.CategoryAssociationSubjectLabel;
                        _subjectLinkPanel.Controls.Add(_subjectLabel);
                    }
                    else
                        _subjectLinkPanel.Controls.Add(_subjectLabel);
                    _subjectLinkPanel.Controls.Add(_subjectLink);
                }

                return _subjectLinkPanel;
            }
        }

        #endregion

        #endregion Properties

        #region Methods

        #region Public

        /// <summary>
        /// Updates subject ScholarlyworkItem to selected CategoryNodes association.
        /// </summary>
        /// <returns>true, if successful; else false.</returns>
        public bool SaveAssociation()
        {
            bool result = false;

            if (TreeView.Nodes.Count != 0)
            {
                for (int Index = 0; Index < TreeView.CheckedNodes.Count; Index++)
                {
                    SelectedIdList.Add(new Guid(TreeView.CheckedNodes[Index].Value));
                }

                using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
                {
                    List<CategoryNode> categoryNodeList = new List<CategoryNode>();
                    categoryNodeList = dataAccess.GetCategoryNodeList(SelectedIdList);

                    foreach (CategoryNode node in categoryNodeList)
                        SelectedList.Add(node);

                    AuthorizeResourcesBeforeSave(dataAccess);

                    result = dataAccess.SaveScholarlyItemCategoryAssociation(ResourceItemId,
                        SelectedList as IList<Zentity.ScholarlyWorks.CategoryNode>, AuthenticatedToken, Constants.PermissionRequiredForAssociation);
                }
                if (result)
                {
                    HeaderMessageLabel.Visible = true;
                    ShowMessage(GlobalResource.MsgCategoryNodeAssociationSuccessfull, false);
                }
            }
            else
            {
                HeaderMessageLabel.Visible = true;
                ShowMessage(GlobalResource.MsgCategoryNodeNotFound, false);
                result = false;
            }
            return result;
        }

        #endregion

        #region Protected

        /// <summary>
        /// Create child controls layout.
        /// </summary>
        /// <param name="e">Event Arguments.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.ContainerPanel.Controls.AddAt(2, SubjectLinkPanel);

            if(this.DesignMode)
                TreeView.ShowCheckBoxes = TreeNodeTypes.All;
        }

        /// <summary>
        /// Authorizes Resource before save.
        /// </summary>
        /// <param name="dataAccess">Object of ResourceDataAccess class.</param>
        internal void AuthorizeResourcesBeforeSave(ResourceDataAccess dataAccess)
        {
            if (IsSecurityAwareControl && SelectedIdList != null)
            {
                int authorizedResourcesCount = dataAccess.GetAuthorizedResources<CategoryNode>(AuthenticatedToken,
                    Constants.PermissionRequiredForAssociation, SelectedList as IEnumerable<CategoryNode>).Count();
                if (authorizedResourcesCount != SelectedList.Count)
                {
                    throw new UnauthorizedAccessException(string.Format(CultureInfo.CurrentCulture,
                        GlobalResource.UnauthorizedAccessExceptionMultipleResources, Constants.PermissionRequiredForAssociation));
                }
            }
        }

        /// <summary>
        /// Fetches the category nodes to be displayed in the control.  
        /// </summary>
        protected override void GetDataSource()
        {
            if (!this.Page.IsPostBack)
                _rootNodeList = GetTreeHierarchy();
        }

        /// <summary>
        /// Populate tree view from data source.
        /// </summary>
        protected override void PopulateTree()
        {
            if (!this.Page.IsPostBack)
            {
                resource = GetScholarlyWorkItem();
                _subjectLabel.Text = GlobalResource.CategoryAssociationSubjectLabel;
                _subjectLink.Text = HttpUtility.HtmlEncode(CoreHelper.FitString(CoreHelper.UpdateEmptyTitle(
                    CoreHelper.GetTitleByResourceType(resource)), _maxCharShownTree));
                _subjectLink.NavigateUrl = string.Format(CultureInfo.CurrentCulture, SubjectNavigationUrl,
                    resource.Id);

                BindTreeView(true);
            }
        }

        /// <summary>
        /// Gets subject resource.
        /// </summary>
        /// <returns>Subject resource.</returns>
        protected ScholarlyWorkItem GetScholarlyWorkItem()
        {
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
            {
                ScholarlyWorkItem item;
                if (IsSecurityAwareControl)
                {
                    if (AuthenticatedToken != null)
                    {
                        item = dataAccess.GetScholarlyWorkItem(ResourceItemId, AuthenticatedToken, UserResourcePermissions.Update);
                    }
                    else
                    {
                        throw new UnauthorizedAccessException(string.Format(CultureInfo.CurrentCulture,
                            GlobalResource.UnauthorizedAccessException, UserResourcePermissions.Update));
                    }
                }
                else
                {
                    item = dataAccess.GetScholarlyWorkItem(ResourceItemId, null, UserResourcePermissions.Update);
                }
                item.CategoryNodes.Load();
                return item;
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Populate category node tree view hierarchy.
        /// </summary>
        /// <param name="rootNodes">List of root CategoryNodes.</param>
        private void PopulateRootNodes(IList<CategoryNode> rootNodes)
        {
            if (rootNodes != null)
            {
                TreeView.Nodes.Clear();
                foreach (CategoryNode rootNode in rootNodes)
                {
                    TreeNode root = CreateNode(rootNode);
                    TreeView.Nodes.Add(root);
                    PopulateTreeHierarchy(rootNode, root);
                }
            }
        }

        private TreeNode CreateNode(CategoryNode category)
        {
            TreeNode node = new TreeNode();
            node.Text = HttpUtility.HtmlEncode(CoreHelper.FitString(CoreHelper.UpdateEmptyTitle(category.Title), _maxCharShownTree));
            node.ToolTip = category.Title;
            node.Value = category.Id.ToString();

            if (IsSecurityAwareControl)
            {
                node.ShowCheckBox = false;
                if (AuthenticatedToken != null)
                {
                    if (HasAccess(category.Id))
                    {
                        node.NavigateUrl = string.Format(CultureInfo.CurrentCulture, TreeNodeNavigationUrl, category.Id);
                        node.ShowCheckBox = true;
                    }
                    else
                    {
                        node.SelectAction = TreeNodeSelectAction.None;
                    }
                }
            }
            else
            {
                node.NavigateUrl = string.Format(CultureInfo.CurrentCulture, TreeNodeNavigationUrl, category.Id);
                node.ShowCheckBox = true;
            }

            //Hide checkbox for subject categoryNode.
            if (category.Id == ResourceItemId)
            {
                node.ShowCheckBox = false;
            }
            else
            {
                if (node.ShowCheckBox.Value == true)
                {
                    int count = resource.CategoryNodes.Where(Tuple => Tuple.Id == category.Id).Count();
                    if (count > 0)
                    {
                        node.Checked = true;
                    }
                }
            }

            return node;
        }

        private bool HasAccess(Guid categoryId)
        {
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
            {
                return dataAccess.AuthorizeResource<CategoryNode>(AuthenticatedToken, Constants.PermissionRequiredForAssociation, categoryId, false);
            }
        }

        /// <summary>
        /// Populates tree view hierarchy.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parentNode"></param>
        private void PopulateTreeHierarchy(CategoryNode node, TreeNode parentNode)
        {
            List<CategoryNode> childCategories = node.Children.OrderBy(tuple => tuple.Title).ToList();
            foreach (CategoryNode childCategory in childCategories)
            {
                TreeNode child = CreateNode(childCategory);
                parentNode.ChildNodes.Add(child);
                PopulateTreeHierarchy(childCategory, child);
            }
        }

        /// <summary>
        /// Bind tree view control with category node hierarchy.
        /// </summary>
        /// <param name="needToBind">Bool false indicates need to bind tree view.</param>
        private void BindTreeView(bool needToBind)
        {
            if (needToBind == true)
            {
                //Populate Node Hierarchy in Treeview.
                PopulateRootNodes(_rootNodeList);

                TreeView.ExpandAll();
                TreeView.ShowCheckBoxes = TreeNodeTypes.All;
            }
        }

        /// <summary>
        /// Retrieve category node hierarchy from the database.
        /// </summary>
        /// <returns></returns>
        private IList<CategoryNode> GetTreeHierarchy()
        {
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
            {
                return dataAccess.GetRootCategoryNodesWithHierarchy().ToList();
            }
        }

        /// <summary>
        /// Show operation perform message.
        /// </summary>
        /// <param name="message">message to shown on UI</param>
        /// <param name="isError">Indicates whether the message is error or not.</param>
        private void ShowMessage(string message, bool isError)
        {
            if (isError)
                HeaderMessageLabel.ForeColor = System.Drawing.Color.Red;
            else
                HeaderMessageLabel.ForeColor = System.Drawing.Color.Black;

            HeaderMessageLabel.Text = message;
        }

        #endregion

        #endregion
    }
}
