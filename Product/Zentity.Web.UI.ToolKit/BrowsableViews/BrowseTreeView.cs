// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Zentity.Core;
using Zentity.ScholarlyWorks;
using Zentity.Security.Authentication;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This control populates following types of data in a TreeView control:
    /// 1. Year and months
    /// 2. Authors
    /// 3. Resource types
    /// 4. Category nodes.
    /// </summary>
    /// <example>
    ///     The code below is the source for BrowseTreeView.aspx.
    ///     It shows an example of using <see cref="BrowseTreeView"/> control. The type of browsing can be 
    ///     controlled by the property 'BrowseBy'. The sample below shows browsing by resource type.
    ///     <code>
    ///          &lt;%@ Page Language="C#" AutoEventWireup="true" %&gt;
    ///          
    ///         &lt;%@ Register Assembly="Zentity.Web.UI.ToolKit" Namespace="Zentity.Web.UI.ToolKit"
    ///             TagPrefix="Zentity" %&gt;
    ///          &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"&gt;
    ///          &lt;html xmlns="http://www.w3.org/1999/xhtml"&gt;
    ///          &lt;head id="Head1" runat="server"&gt;
    ///             &lt;title&gt;BrowseTreeView Sample&lt;/title&gt;
    ///              
    ///             &lt;script runat="server"&gt;
    ///                 protected void Page_PreRender(object sender, EventArgs e)
    ///                 {
    ///                     // Display the search string.
    ///                     SearchStringLabel.Text = BrowseTreeView1.SelectedSearchValue;
    ///                 }
    ///              &lt;/script&gt;
    ///              
    ///          &lt;/head&gt;
    ///          &lt;body&gt;
    ///             &lt;form id="mainForm" runat="server"&gt;
    ///             &lt;div&gt;
    ///                 &lt;Zentity:BrowseTreeView ID="BrowseTreeView1" runat="server" 
    ///                     BrowseBy="BrowseByResourceType" IsSecurityAwareControl="false"&gt;
    ///                 &lt;/Zentity:BrowseTreeView&gt;
    ///                 &lt;br /&gt;
    ///                 Search string:
    ///                 &lt;asp:Label ID="SearchStringLabel" runat="server" /&gt;
    ///             &lt;/div&gt;
    ///             &lt;/form&gt;
    ///         &lt;/body&gt;
    ///         &lt;/html&gt;
    ///     </code>
    /// </example>
    [DefaultProperty("Text")]
    public class BrowseTreeView : BaseTreeView
    {
        #region MemberVariables

        List<DateTime?> _yearRange;
        BrowseByCriteria _browseBy;

        List<Contact> _authorList;

        List<ResourceType> _resTypeList;

        CategoryNode _rootCategoryNode;
        IEnumerable<Guid> _authorizedCategoryNodeIds;
        DropDownList _rootNodeDropDown;



        #endregion

        #region Constants

        const int _maxCharShownTree = 20;

        private const string _searchYearStringWithSeperator = "DateAdded:(>='{0}{1}{2}' AND < '{3}{4}{5}')";
        private const string _searchYearString = "DateAdded:(>='1{0}{1}' AND < '1{2}{3}')";

        private const string _authors = "Authors";
        private const string _nodeAE = "a-eA-E";
        private const string _nodeFJ = "f-jF-J";
        private const string _nodeKO = "k-oK-O";
        private const string _nodePT = "p-tP-T";
        private const string _nodeUZ = "u-zU-Z";
        private const string _reqExp = "^([{0}]).*";
        private const string _nodeMisc = "Miscellaneous";
        private const string _miscRegExp = "\\A[^a-zA-Z]+.*|\\A\\s+";
        private const string _searchResourceTypeString = "resourcetype:{0}";

        private const string _searchAuthorString = "author:(resourcetype:contact Id:='{0}')";

        private const string _searchCategoryNodeString = "SubjectArea:(Id:='{0}')";
        private const string _rootNodeDropDownId = "rootNodeDropDownId";
        const string _rootNodeDDLTextField = "Title";
        const string _rootNodeDDLValueField = "Id";
        const string _rootNodeDDLDataMember = "CategoryNode";

        #endregion

        #region properties

        #region Public

        /// <summary>
        /// Gets selected search value.
        /// </summary>
        public string SelectedSearchValue
        {
            get
            {
                return GetSearchQueryString();
            }
        }

        /// <summary>
        /// Gets or sets the mode of the browsable criteria selected by the user.
        /// </summary>
        public BrowseByCriteria BrowseBy
        {
            get
            {
                return _browseBy;
            }
            set
            {
                _browseBy = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of levels that are expanded the control is displayed for the first time.
        /// </summary>
        public int ExpandDepth
        {
            get { return TreeView.ExpandDepth; }
            set { TreeView.ExpandDepth = value; }
        }

        /// <summary>
        /// Occurs when a node is selected in BrowseTreeView control.
        /// </summary>
        public event EventHandler<EventArgs> SelectedNodeChanged;

        #endregion

        #region Private

        private DropDownList RootNodeDropDown
        {
            get
            {
                if (_rootNodeDropDown == null)
                {
                    _rootNodeDropDown = new DropDownList();
                    _rootNodeDropDown.ID = _rootNodeDropDownId;
                    _rootNodeDropDown.Width = Unit.Percentage(100);
                    _rootNodeDropDown.AutoPostBack = true;
                    _rootNodeDropDown.DataMember = _rootNodeDDLDataMember;
                    _rootNodeDropDown.DataTextField = _rootNodeDDLTextField;
                    _rootNodeDropDown.DataValueField = _rootNodeDDLValueField;
                    _rootNodeDropDown.SelectedIndexChanged += new EventHandler(RootNodeDropDown_SelectedIndexChanged);
                }

                return _rootNodeDropDown;
            }
        }

        #endregion

        #endregion

  
        #region Methods

        #region Public

        /// <summary>
        /// Clears the selection from the tree.
        /// </summary>
        public void ClearSelection()
        {
            if (TreeView.SelectedNode != null)
            {
                TreeView.SelectedNode.Selected = false;
            }
        }

        #endregion

        #region Protected

        /// <summary>
        /// Adds RootNodeDropDown control to the TreeViewContainer if selected browsable view is BrowseByCategoryHierarchy.
        /// </summary>
        /// <param name="e">Event Arguments.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (BrowseBy == BrowseByCriteria.BrowseByCategoryHierarchy)
            {
                ContainerPanel.Controls.AddAt(2, RootNodeDropDown);
            }

            TreeView.SelectedNodeChanged += new EventHandler(BaseTreeview_SelectedNodeChanged);
        }

        /// <summary>
        /// Gets the data source.
        /// </summary>
        protected override void GetDataSource()
        {
            if (!this.Page.IsPostBack)
            {
                using (ResourceDataAccess resourceAccess = new ResourceDataAccess(base.CreateContext()))
                {
                    switch (BrowseBy)
                    {
                        case BrowseByCriteria.BrowseByYear:
                            if (!IsSecurityAwareControl)
                            {
                                this._yearRange = resourceAccess.GetYear(null);
                            }
                            else if (this.AuthenticatedToken != null)
                            {
                                this._yearRange = resourceAccess.GetYear(this.AuthenticatedToken);
                            }

                            break;
                        case BrowseByCriteria.BrowseByAuthors:
                            if (!IsSecurityAwareControl)
                            {
                                this._authorList = resourceAccess.GetAuthors(null).ToList();
                            }
                            else if (this.AuthenticatedToken != null)
                            {
                                this._authorList = resourceAccess.GetAuthors(this.AuthenticatedToken).ToList();
                            }
                            break;
                        case BrowseByCriteria.BrowseByResourceType:
                            this._resTypeList = resourceAccess.GetResourceTypes().ToList();
                            _resTypeList = CoreHelper.FilterSecurityResourceTypes(_resTypeList).ToList();
                            break;
                        case BrowseByCriteria.BrowseByCategoryHierarchy:
                            PopulateRootNodeDropDown();
                            if (!string.IsNullOrEmpty(RootNodeDropDown.SelectedValue))
                            {
                                this._rootCategoryNode = resourceAccess.GetCategoryNodeWithHierarchy(
                                    new Guid(RootNodeDropDown.SelectedValue));
                                if (IsSecurityAwareControl && _rootCategoryNode != null)
                                {
                                    _authorizedCategoryNodeIds = GetAuthorizedCategoryNodes(AuthenticatedToken, _rootCategoryNode, resourceAccess);
                                }
                            }
                            break;
                    }

                }
            }
        }

        /// <summary>
        /// Populates the tree view.
        /// </summary>
        protected override void PopulateTree()
        {
            if (!this.Page.IsPostBack)
            {
                TreeView.Nodes.Clear();
                switch (BrowseBy)
                {
                    case BrowseByCriteria.BrowseByYear:
                        FillYearNodes();
                        break;
                    case BrowseByCriteria.BrowseByAuthors:
                        FillAuthorNodes();
                        break;
                    case BrowseByCriteria.BrowseByResourceType:
                        FillResourceTypesNodes();
                        break;
                    case BrowseByCriteria.BrowseByCategoryHierarchy:
                        FillCategoryNodes();
                        break;

                }

            }
        }

        #endregion

        #region Private

        void BaseTreeview_SelectedNodeChanged(object sender, EventArgs e)
        {
            if (SelectedNodeChanged != null)
                SelectedNodeChanged(sender, e);
        }

        private string GetSearchQueryString()
        {
            string searchQuery = string.Empty;

            if (!string.IsNullOrEmpty(TreeView.SelectedValue))
            {
                switch (BrowseBy)
                {
                    case BrowseByCriteria.BrowseByYear:
                        searchQuery = GetQueryString(TreeView.SelectedValue);
                        break;
                    case BrowseByCriteria.BrowseByAuthors:
                        searchQuery = string.Format(CultureInfo.InstalledUICulture, _searchAuthorString, TreeView.SelectedValue);
                        break;
                    case BrowseByCriteria.BrowseByResourceType:
                        searchQuery = string.Format(CultureInfo.InstalledUICulture, _searchResourceTypeString, TreeView.SelectedValue);
                        break;
                    case BrowseByCriteria.BrowseByCategoryHierarchy:
                        searchQuery = string.Format(CultureInfo.InstalledUICulture, _searchCategoryNodeString, TreeView.SelectedValue);
                        break;

                }
            }

            return searchQuery;
        }

        #region BrowseByYear Methods

        /// <summary>
        /// Fill year nodes in the tree view.
        /// </summary>
        private void FillYearNodes()
        {
            if (_yearRange != null && _yearRange.Count > 0)
            {
                int minYear = _yearRange[0].HasValue ? _yearRange[0].Value.Year : DateTime.Now.Year;
                int maxYear = _yearRange[1].HasValue ? _yearRange[1].Value.Year : DateTime.Now.Year;

                int minMonth = _yearRange[0].HasValue ? _yearRange[0].Value.Month : DateTime.Now.Month;
                int maxMonth = _yearRange[1].HasValue ? _yearRange[1].Value.Month : DateTime.Now.Month;

                TreeNode rootNode = CreateNode(string.Empty, GlobalResource.YearText);

                for (int i = minYear; i <= maxYear; i++)
                {
                    TreeNode yearNode = CreateNode(i.ToString(CultureInfo.CurrentCulture), i.ToString(CultureInfo.CurrentCulture));
                    if (i != minYear && i != maxYear)
                    {
                        AddMonthNodes(yearNode, 0, 11);
                    }
                    else
                    {
                        if (minYear != maxYear)
                        {
                            AddMonthNodes(yearNode, i == minYear ? minMonth - 1 : 0, i == minYear ? 11 : maxMonth - 1);
                        }
                        else
                        {
                            AddMonthNodes(yearNode, minMonth - 1, minMonth == maxMonth ? minMonth - 1 : maxMonth - 1);
                        }
                    }
                    rootNode.ChildNodes.Add(yearNode);
                }

                TreeView.Nodes.Add(rootNode);
            }
        }

        /// <summary>
        /// Add month nodes to year node
        /// </summary>
        /// <param name="yearNode"></param>
        /// <param name="startMonth"></param>
        /// <param name="stopMonth"></param>
        private static void AddMonthNodes(TreeNode yearNode, int startMonth, int stopMonth)
        {
            string[] months = GlobalResource.MonthsOfYear.Split(',');

            for (int i = startMonth; i <= stopMonth; i++)
            {
                string month = (string)months.GetValue(i);
                string monthValue = i + 1 + "/" + yearNode.Text;
                TreeNode monthNode = CreateNode(monthValue, month);
                yearNode.ChildNodes.Add(monthNode);
            }
        }

        /// <summary>
        /// Create query string.
        /// </summary>
        /// <param name="selectedValue">Selected tree node value.</param>
        /// <returns>Query string</returns>
        private static string GetQueryString(string selectedValue)
        {
            int numberOfMonthsInAYear = 12;
            string strSearchQuery = string.Empty; ;
            if (!string.IsNullOrEmpty(selectedValue))
            {
                string strDateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;

                if (selectedValue.Contains(strDateSeparator))
                {
                    string[] selectedYear = selectedValue.Split(strDateSeparator.ToCharArray());
                    int month = Convert.ToInt32(selectedYear[0], CultureInfo.InvariantCulture);
                    int year = Convert.ToInt32(selectedYear[1], CultureInfo.InvariantCulture);
                    int nextmonth = month == numberOfMonthsInAYear ? 1 : month + 1;
                    int nextyear = month == numberOfMonthsInAYear ? year + 1 : year;

                    strSearchQuery = string.Format(CultureInfo.InvariantCulture,
                                                    _searchYearStringWithSeperator,
                                                    month, strDateSeparator, year,
                                                    nextmonth, strDateSeparator, nextyear);
                }
                else
                {
                    int year = Convert.ToInt32(selectedValue, CultureInfo.CurrentCulture);
                    strSearchQuery = string.Format(CultureInfo.InvariantCulture,
                                                    _searchYearString,
                                                    strDateSeparator, year,
                                                    strDateSeparator, year + 1);
                }
            }

            return strSearchQuery;
        }

        #endregion

        #region BrowseByAuthor Methods

        /// <summary>
        /// Fills Authors in the tree view.
        /// </summary>
        private void FillAuthorNodes()
        {
            if (_authorList != null && _authorList.Count > 0)
            {
                TreeNode rootNode = new TreeNode(_authors);

                TreeNode node1 = CreateNode(_nodeAE.Substring(3), _nodeAE.Substring(3));
                rootNode.ChildNodes.Add(node1);
                AddAuthorChildNodes(_authorList, string.Format(CultureInfo.CurrentCulture,_reqExp, _nodeAE), node1);

                TreeNode node2 = CreateNode(_nodeFJ.Substring(3), _nodeFJ.Substring(3));
                rootNode.ChildNodes.Add(node2);
                AddAuthorChildNodes(_authorList, string.Format(CultureInfo.CurrentCulture, _reqExp, _nodeFJ), node2);

                TreeNode node3 = CreateNode(_nodeKO.Substring(3), _nodeKO.Substring(3));
                rootNode.ChildNodes.Add(node3);
                AddAuthorChildNodes(_authorList, string.Format(CultureInfo.CurrentCulture, _reqExp, _nodeKO), node3);

                TreeNode node4 = CreateNode(_nodePT.Substring(3), _nodePT.Substring(3));
                rootNode.ChildNodes.Add(node4);
                AddAuthorChildNodes(_authorList, string.Format(CultureInfo.CurrentCulture, _reqExp, _nodePT), node4);

                TreeNode node5 = CreateNode(_nodeUZ.Substring(3), _nodeUZ.Substring(3));
                rootNode.ChildNodes.Add(node5);
                AddAuthorChildNodes(_authorList, string.Format(CultureInfo.CurrentCulture, _reqExp, _nodeUZ), node5);

                TreeNode node6 = CreateNode(_nodeMisc, _nodeMisc);
                rootNode.ChildNodes.Add(node6);
                AddAuthorChildNodes(_authorList, _miscRegExp, node6);

                this.TreeView.Nodes.Add(rootNode);
            }
        }

        /// <summary>
        /// Append child tree nodes to parent tree node.
        /// </summary>
        /// <param name="authorList">Authors list</param>
        /// <param name="regExMatch">regular expression</param>
        /// <param name="node">Parent tree node</param>
        private static void AddAuthorChildNodes(List<Contact> authorList, string regExMatch, TreeNode node)
        {
            List<BrowsableNodeInfo> nodeInfoList = new List<BrowsableNodeInfo>();

            Regex regexAuthor = GetRegularExpression(regExMatch);

            nodeInfoList = authorList.OfType<Contact>().Where(tuple => !(tuple is Organization || tuple is Person)
                        && regexAuthor.IsMatch(tuple.Title)).Select(nodeInfo => new BrowsableNodeInfo(nodeInfo.Id.ToString()
                            , nodeInfo.Title, 0)).ToList();

            nodeInfoList.AddRange(authorList.OfType<Organization>().Where(tuple => regexAuthor.IsMatch(tuple.Title))
                        .Select(nodeInfo => new BrowsableNodeInfo(nodeInfo.Id.ToString(), nodeInfo.Title, 0)).ToList());

            nodeInfoList.AddRange(authorList.OfType<Person>().Where(tuple => !string.IsNullOrEmpty(tuple.FirstName) && regexAuthor.IsMatch(tuple.FirstName))
                        .Select(nodeInfo => new BrowsableNodeInfo(nodeInfo.Id.ToString(), nodeInfo.FirstName, 0)).ToList());

            nodeInfoList.AddRange(authorList.OfType<Person>().Where(tuple => string.IsNullOrEmpty(tuple.FirstName) && regexAuthor.IsMatch(tuple.Title))
                        .Select(nodeInfo => new BrowsableNodeInfo(nodeInfo.Id.ToString(), nodeInfo.Title, 0)).ToList());

            nodeInfoList = nodeInfoList.OrderBy(tuple => tuple.Text).ToList();

            if (nodeInfoList.Count > 0)
            {
                //Add authors to the nodes 
                foreach (BrowsableNodeInfo person in nodeInfoList)
                {
                    TreeNode childNode = CreateNode(person.Value, person.Text);
                    node.ChildNodes.Add(childNode);
                }
            }
        }

        private static Regex GetRegularExpression(string authorNode)
        {
            Regex regexAuthor = null;
            regexAuthor = new Regex(authorNode);
            return regexAuthor;
        }

        #endregion

        #region BrowseByResourceType Methods

        /// <summary>
        /// Fills resource types in the tree view.
        /// </summary>
        private void FillResourceTypesNodes()
        {
            if (_resTypeList != null)
            {
                List<ResourceType> typeList = new List<ResourceType>();

                typeList = _resTypeList.Where(tuple => tuple.BaseType == null).ToList();

                foreach (ResourceType type in typeList)
                {
                    ResourceType typetoRemove = _resTypeList.Where(tuple => tuple.Name == type.Name).FirstOrDefault();
                    if (typetoRemove != null)
                    {
                        _resTypeList.Remove(typetoRemove);
                    }
                }

                foreach (ResourceType type in typeList)
                {
                    TreeNode node1 = CreateNode(type.FullName, type.Name);
                    TreeView.Nodes.Add(node1);
                    //AddOnClickToNode(node1);
                    AddChildResourceTypesNodes(node1, type);
                }
            }
        }

        /// <summary>
        /// Append child tree nodes to parent tree node.
        /// </summary>
        /// <param name="parentNode">Parent node</param>
        /// <param name="type">parent ResourceType object</param>
        private void AddChildResourceTypesNodes(TreeNode parentNode, ResourceType type)
        {
            List<ResourceType> childTypes = _resTypeList.Where(tuple => tuple.BaseType.Name == type.Name)
                .ToList();

            foreach (ResourceType childType in childTypes)
            {
                TreeNode node1 = CreateNode(childType.FullName, childType.Name);
                parentNode.ChildNodes.Add(node1);
                //AddOnClickToNode(node1);
                AddChildResourceTypesNodes(node1, childType);
            }
        }

        #endregion

        #region BrowseByCategoryNode Methods

        /// <summary>
        /// Fill CategoryNodes in the tree view.
        /// </summary>
        private void FillCategoryNodes()
        {
            if (_rootCategoryNode != null)
            {
                TreeNode node = CreateNode(_rootCategoryNode.Id.ToString(), _rootCategoryNode.Title);
                this.TreeView.Nodes.Add(node);

                //If categoryNode is not authorized node then don't allow selection.
                if (_authorizedCategoryNodeIds != null && !_authorizedCategoryNodeIds.Contains(_rootCategoryNode.Id))
                {
                    node.SelectAction = TreeNodeSelectAction.None;
                }

                //AddOnClickToNode(node);
                AddCategoryChildNodes(node, _rootCategoryNode);
            }
        }

        /// <summary>
        /// Append child tree nodes to parent tree node.
        /// </summary>
        /// <param name="nd">Parent tree node</param>
        /// <param name="cat">Parent category node</param>
        private void AddCategoryChildNodes(TreeNode nd, CategoryNode cat)
        {
            foreach (CategoryNode child in cat.Children.ToList())
            {
                TreeNode node = CreateNode(child.Id.ToString(), child.Title);

                //If categoryNode is not authorized node then don't allow selection.
                if (_authorizedCategoryNodeIds != null && !_authorizedCategoryNodeIds.Contains(child.Id))
                {
                    node.SelectAction = TreeNodeSelectAction.None;
                }

                //AddOnClickToNode(node);
                nd.ChildNodes.Add(node);
                if (child.Children.Count > 0)
                {
                    AddCategoryChildNodes(node, child);
                }
            }
        }

        /// <summary>
        /// Fills DropDownList with root CategoryNodes.
        /// </summary>
        private void PopulateRootNodeDropDown()
        {
            RootNodeDropDown.Items.Clear();
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
            {
                IList<CategoryNode> rootNodeList = dataAccess.GetRootCategoryNodes();

                if (rootNodeList != null && rootNodeList.Count > 0)
                {
                    foreach (CategoryNode node in rootNodeList)
                    {
                        node.Title = CoreHelper.FitString(CoreHelper.UpdateEmptyTitle(node.Title), _maxCharShownTree);
                    }
                }

                RootNodeDropDown.DataSource = rootNodeList;
                RootNodeDropDown.DataBind();
            }
        }

        /// <summary>
        /// Handles selected index change event and repopulate tree view based on selection of root node.
        /// </summary>
        /// <param name="sender">Sender Object</param>
        /// <param name="e">Event Arguments</param>
        private void RootNodeDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (ResourceDataAccess resourceAccess = new ResourceDataAccess(base.CreateContext()))
            {
                this._rootCategoryNode = resourceAccess.GetCategoryNodeWithHierarchy(
                    new Guid(RootNodeDropDown.SelectedValue));

                if (IsSecurityAwareControl && _rootCategoryNode != null)
                {
                    _authorizedCategoryNodeIds = GetAuthorizedCategoryNodes(AuthenticatedToken, _rootCategoryNode, resourceAccess);
                }

            }

            TreeView.Nodes.Clear();
            FillCategoryNodes();
        }

        /// <summary>
        /// Filter category nodes based on Read permission.
        /// </summary>
        /// <param name="token">Authenticated Token.</param>
        /// <param name="rootCategoryNode">Root CategoryNode Object.</param>
        /// <param name="dataAccess">ResourceDataAccess Object.</param>
        /// <returns>List of Authorized Resources</returns>
        private IEnumerable<Guid> GetAuthorizedCategoryNodes(AuthenticatedToken token, CategoryNode rootCategoryNode,
            ResourceDataAccess dataAccess)
        {
            ICollection<CategoryNode> categoryNodes = new List<CategoryNode>();

            categoryNodes = AddChildCategoryNodes(rootCategoryNode, categoryNodes);

            var authorizedCatNodes = dataAccess.GetAuthorizedResources<CategoryNode>(token,
                UserResourcePermissions.Read, categoryNodes);

            if (authorizedCatNodes != null)
                return authorizedCatNodes.Select(tuple => tuple.Id).ToList();
            else
                return new List<Guid>();
        }

        /// <summary>
        /// Creates collection of Category nodes is the tree.
        /// </summary>
        /// <param name="srcParentNode">Source Parent CategoryNode.</param>
        /// <param name="destinationNodeList">Destination node list.</param>
        /// <returns>List of CategoryNode objects.</returns>
        private ICollection<CategoryNode> AddChildCategoryNodes(CategoryNode srcParentNode, ICollection<CategoryNode> destinationNodeList)
        {
            if (srcParentNode != null && destinationNodeList != null)
            {
                destinationNodeList.Add(srcParentNode);

                foreach (CategoryNode chilNode in srcParentNode.Children)
                {
                    AddChildCategoryNodes(chilNode, destinationNodeList);
                }
            }

            return destinationNodeList;
        }

        #endregion

        #endregion

        #endregion
    }

    /// <summary>
    /// Enumeration for identifying in which browse by mode control is configured.
    /// </summary>
    public enum BrowseByCriteria
    {
        /// <summary>
        /// Authors browse mode.
        /// </summary>
        BrowseByAuthors,
        /// <summary>
        /// Year browse mode.
        /// </summary>
        BrowseByYear,
        /// <summary>
        /// Category Hierarchy browse by mode.
        /// </summary>
        BrowseByCategoryHierarchy,
        /// <summary>
        /// ResourceType browse by mode.
        /// </summary>
        BrowseByResourceType
    }
}
