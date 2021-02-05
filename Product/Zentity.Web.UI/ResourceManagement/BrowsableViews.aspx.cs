// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Zentity.Web.UI.ToolKit;
using Zentity.Core;
using Zentity.Security.Authentication;
using Zentity.Platform;
using Zentity.Web.UI;
using Zentity.ScholarlyWorks;

public partial class ResourceManagement_BrowseViews : ZentityBasePage
{
    #region Constants

    const string _browseViewKey = "BrowseView";
    const string _BrowseByYear = "BrowseByYear";
    const string _BrowseByCategoryHierarchy = "BrowseByCategoryHierarchy";
    const string _BrowseByResourceType = "BrowseByResourceType";
    const string _BrowseByAuthors = "BrowseByAuthors";
    const string _space = " ";
    const int _pageSize = 10;
    protected const string ExpandedImagePath = "../App_Themes/DarkBlue/Images/Expanded.gif";
    protected const string CollapsedImagePath = "../App_Themes/DarkBlue/Images/Collapsed.gif";
    protected const string CollapsedCssClass = "Collapsed";

    #endregion

    #region Event Handlers

    protected void Page_Load(object sender, EventArgs e)
    {
        AuthenticatedToken token = this.Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;

        YearMonthtree.AuthenticatedToken = CategoryNodeTree.AuthenticatedToken =
            ResourceTypeTree.AuthenticatedToken = AuthorsView.AuthenticatedToken = token;

        if (!IsPostBack)
        {
            AdditionalFiltersStatusHidden.Value = CollapsedCssClass;
            AdditionalFilters.Attributes.Add("class", CollapsedCssClass);
            ToggleImagePathHidden.Value = CollapsedImagePath;
            ToggleImage.ImageUrl = CollapsedImagePath;
        }
        else
        {
            AdditionalFilters.Attributes.Add("class", AdditionalFiltersStatusHidden.Value);
            ToggleImage.ImageUrl = ToggleImagePathHidden.Value;
        }
    }

    /// <summary>
    /// Sets visibility of TreeView control.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="e">Event Arguments</param>
    protected void Page_Init(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(Request.QueryString[_browseViewKey]))
        {
            switch (Request.QueryString[_browseViewKey])
            {
                case _BrowseByYear:
                    YearMonthTreePanel.Visible = false;
                    break;
                case _BrowseByCategoryHierarchy:
                    CategoryNodeTreePanel.Visible = false;
                    break;
                case _BrowseByResourceType:
                    ResourceTypeTreePanel.Visible = false;
                    break;
                case _BrowseByAuthors:
                    AuthorsTreePanel.Visible = false;
                    break;
            }
        }

        Master masterPage = (Master)this.Page.Master;
        if (masterPage.SelectedBrowseTreeView != null)
        {
            AuthorsListView authorListViw = masterPage.SelectedBrowseTreeView as AuthorsListView;
            if (authorListViw != null)
            {
                authorListViw.SelectedIndexChanged += new EventHandler<EventArgs>(ResourceListView_OnSortButtonClicked);
            }
            else
            {
                (masterPage.SelectedBrowseTreeView as BrowseTreeView).SelectedNodeChanged +=
                    new EventHandler<EventArgs>(ResourceListView_OnSortButtonClicked);
            }
        }
    }

    /// <summary>
    /// Apply sorting on data source of ResourceListView control.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="e">Event Arguments</param>
    protected void ResourceListView_OnSortButtonClicked(object sender, EventArgs e)
    {
        RefreshDataSource(ResourceListView.PageIndex, GetSearchString());
    }

    /// <summary>
    /// Page data source of ResourceListView control.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="e">Event Arguments</param>
    protected void SearchResourceListView_PageChanged(object sender, EventArgs e)
    {
        RefreshDataSource(ResourceListView.PageIndex, GetSearchString());
    }

    /// <summary>
    /// add more  filters to current filter collections and refreshes data source of ResourceListView controls.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="e">Event Arguments</param>
    protected void btnFilter_Click(object sender, EventArgs e)
    {
        RefreshDataSource(0, GetSearchString());
    }

    /// <summary>
    /// Resets selected filters.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="e">Event Arguments</param>
    protected void btnResetFilters_Click(object sender, EventArgs e)
    {
        YearMonthtree.ClearSelection();
        CategoryNodeTree.ClearSelection();
        AuthorsView.ClearSelection();
        ResourceTypeTree.ClearSelection();
        txtFilter.Text = string.Empty;

        ResourceListView_OnSortButtonClicked(sender, e);
    }

    #endregion

    #region Methods

    private string GetSearchString()
    {
        string searchString = string.Empty;
        BrowseTreeView yearsTreeView = null;
        BrowseTreeView categoriesTreeView = null;
        BrowseTreeView resourceTypesTreeView = null;
        AuthorsListView authorsListView = null;

        //Find selected TreeView control on the master page.
        Master masterPage = (Master)this.Page.Master;
        if (masterPage.SelectedBrowseTreeView != null)
        {
            if (masterPage.SelectedBrowseTreeView is AuthorsListView)
                authorsListView = masterPage.SelectedBrowseTreeView as AuthorsListView;
            else
            {
                BrowseTreeView selectedTree = (BrowseTreeView)masterPage.SelectedBrowseTreeView;
                if (selectedTree.BrowseBy == YearMonthtree.BrowseBy)
                    yearsTreeView = selectedTree;
                else if (selectedTree.BrowseBy == CategoryNodeTree.BrowseBy)
                    categoriesTreeView = selectedTree;
                else if (selectedTree.BrowseBy == ResourceTypeTree.BrowseBy)
                    resourceTypesTreeView = selectedTree;
            }

        }
        if (yearsTreeView == null)
            yearsTreeView = YearMonthtree;
        if (categoriesTreeView == null)
            categoriesTreeView = CategoryNodeTree;
        if (resourceTypesTreeView == null)
            resourceTypesTreeView = ResourceTypeTree;
        if (authorsListView == null)
            authorsListView = AuthorsView;

        //Construct query string.
        searchString += yearsTreeView.SelectedSearchValue + _space
            + categoriesTreeView.SelectedSearchValue + _space
            + resourceTypesTreeView.SelectedSearchValue + _space
            + authorsListView.SelectedSearchValue + _space
            + txtFilter.Text;

        return searchString.Trim();
    }

    private void RefreshDataSource(int pageIndex, string searchText)
    {
        int totalPages = 0;
        ResourceListView.PageIndex = 0;
        ResourceListView.TotalRecords = 0;
        ResourceListView.DataSource.Clear();
        if (!string.IsNullOrEmpty(searchText))
        {
            int totalRecords = 0;
            ResourceListView.PageSize = _pageSize;
            int totalParsedRecords = totalParsedRecords = _pageSize * pageIndex;
            List<Resource> foundRestResource = null;
            Dictionary<Guid, IEnumerable<string>> userPermissions = null;

            SortProperty sortProp = null;
            if (ResourceListView.SortDirection == System.Web.UI.WebControls.SortDirection.Ascending)
            {
                sortProp = new SortProperty(ResourceListView.SortExpression, Zentity.Platform.SortDirection.Ascending);
            }
            else
            {
                sortProp = new SortProperty(ResourceListView.SortExpression, Zentity.Platform.SortDirection.Descending);
            }

            //search resources and user permissions on the searched resources.
            using (ResourceDataAccess dataAccess = new ResourceDataAccess())
            {
                AuthenticatedToken token = this.Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;
                if (token != null)
                {
                    foundRestResource = dataAccess.SearchResources(token, searchText, _pageSize, sortProp, totalParsedRecords,
                        false, out totalRecords).ToList();
                    userPermissions = GetPermissions(token, foundRestResource);
                }
            }

            //Calculate total pages
            if (totalRecords > 0)
            {
                totalPages = Convert.ToInt32(Math.Ceiling((double)totalRecords / _pageSize));
            }

            // Update empty resource's title with default value.
            if (foundRestResource != null && foundRestResource.Count() > 0)
            {
                Utility.UpdateResourcesEmptyTitle(foundRestResource);
            }

            //Bind data to GridView
            ResourceListView.TotalRecords = totalRecords;
            ResourceListView.PageIndex = pageIndex;

            foreach (Resource resource in foundRestResource)
                ResourceListView.DataSource.Add(resource);

            ResourceListView.UserPermissions = userPermissions;
            //Enable feed for this page.
            string searchString = GetSearchString();
            if (!string.IsNullOrEmpty(searchString))
            {
                ResourceListView.EnableFeed(searchString);
            }
            ResourceListView.DataBind();
        }
    }

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

            using (ResourceDataAccess dataAccess = new ResourceDataAccess())
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
            userPermissions = new Dictionary<Guid, IEnumerable<string>>();

        return userPermissions;
    }

    #endregion
}
