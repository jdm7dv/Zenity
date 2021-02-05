// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Configuration;
using System.Linq;
using System.Web.UI.WebControls;
using Zentity.Web.UI;
using System.Collections.Generic;
using Zentity.Core;
using Zentity.Platform;
using System.Globalization;
using Zentity.ScholarlyWorks;
using Zentity.Security.Authentication;

public partial class ResourceManagement_BasicSearch : ZentityBasePage
{
    #region Constants
    private const string _SearchText = "SearchText";
    private const string _ftsStatus = "FtsStatus";
    private string _ftsCurrentStatus;
    #endregion

    #region Event Handlers

    protected void Page_Load(object sender, EventArgs e)
    {
        //GridViewStatusLabel.Visible = false;
        if (!IsPostBack)
        {
            SetContentSearchStatus();
            LocalizePage();
            Utility.FillPageSizeDropDownList(PageSizeDropDownList);

            //If search text is passed in the querystring then search for the text.
            if (Request.QueryString[_SearchText] != null)
            {
                SearchText.Text = Request.QueryString[_SearchText].ToString().Trim();
                if (Request.QueryString[Constants.PageSizeQueryStringParameter] != null)
                {
                    PageSizeDropDownList.SelectedValue = Request.QueryString[Constants.PageSizeQueryStringParameter].ToString().Trim();
                }
                if (Request.QueryString[Constants.ContentSearchQueryStringParameter] != null)
                {
                    bool content;
                    bool.TryParse(Request.QueryString[Constants.ContentSearchQueryStringParameter].ToString().Trim(), out content);
                    ContentSearchCheckBox.Checked = content;
                }

                GoButton_Click(GoButton, new EventArgs());
            }
        }
        this.LoadComplete += new EventHandler(ResourceManagement_BasicSearch_LoadComplete);
    }



    void ResourceManagement_BasicSearch_LoadComplete(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(SearchText.Text) && !this.ContentSearchCheckBox.Checked)
        {
            SearchResourceListView.EnableFeed(SearchText.Text);
        }
    }

    protected void GoButton_Click(object sender, EventArgs e)
    {
        string searchText = (SearchText.Text == null) ? null : SearchText.Text.Trim();

        if (IsPostBack)
        {
            string queryStringSearchText = (Request.QueryString[_SearchText] == null) ? string.Empty :
                Request.QueryString[_SearchText].Trim();
            string queryStringPageSize = (Request.QueryString[Constants.PageSizeQueryStringParameter] == null) ? string.Empty :
                Request.QueryString[Constants.PageSizeQueryStringParameter].ToString().Trim();
            string queryStringContent = (Request.QueryString[Constants.ContentSearchQueryStringParameter] == null) ? string.Empty :
             Request.QueryString[Constants.ContentSearchQueryStringParameter].ToString().Trim();

            if (!string.IsNullOrEmpty(searchText) &&
                (searchText != queryStringSearchText || !PageSizeDropDownList.SelectedValue.Equals(queryStringPageSize)
                || !ContentSearchCheckBox.Checked.Equals(queryStringContent)))
            {
                string urlToRedirect = string.Format(CultureInfo.CurrentCulture, Constants.UriBasicSearchPage,
                        Server.UrlEncode(searchText), PageSizeDropDownList.SelectedValue, ContentSearchCheckBox.Checked);
                Response.Redirect(urlToRedirect, true);
            }
        }

        SearchResourceListView.DataSource.Clear();
        SearchResourceListView.DataBind();

        if (!string.IsNullOrEmpty(searchText))
        {
            SearchResourceListView.PageSize = Convert.ToInt32(PageSizeDropDownList.SelectedValue);
            SearchResourceListView.SortDirection = System.Web.UI.WebControls.SortDirection.Descending;
            RefreshDataSource(0);
        }
    }

    protected void SearchResourceListView_PageChanged(object sender, EventArgs e)
    {
        RefreshDataSource(SearchResourceListView.PageIndex);
    }

    #endregion

    #region Methods

    private void LocalizePage()
    {
        searchLabel.Text = Resources.Resources.ResourceLabelBasicSearchText;
        searchForLabel.Text = Resources.Resources.ResourceLabelSearchforText;
        pageSizeLabel.Text = Resources.Resources.ResourceLabelPagesizeText;
        SearchExampleLabel.Text = Resources.Resources.BasicSearch_Example;
        ContentSearchInformationLabel.Text = _ftsCurrentStatus;

        //searchResultsLabel.Text = Resources.Resources.ResourceLabelSearchresultText;
        GoButton.Text = Resources.Resources.ResourceButtonGoText;
        SearchTextRequiredValidator.ErrorMessage = Resources.Resources.AlertSearchValue;
        ContentSearchCheckBox.Text = Resources.Resources.BasicSearch_ContentSearch;
    }

    protected void ResourceListView_OnSortButtonClicked(Object sender, EventArgs e)
    {
        RefreshDataSource(SearchResourceListView.PageIndex);
    }

    private void RefreshDataSource(int pageIndex)
    {
        int totalPages = 0;
        string searchText = SearchText.Text;
        if (!string.IsNullOrEmpty(searchText))
        {
            LabelErrorMessage.Text = string.Empty;
            try
            {
                searchText = SearchText.Text.Trim();
                SearchText.Text = searchText;
                int totalRecords = 0;
                int pageSize = SearchResourceListView.PageSize;
                int totalParsedRecords = totalParsedRecords = pageSize * pageIndex;
                List<Resource> foundRestResource = null;
                Dictionary<Guid, IEnumerable<string>> userPermissions = null;

                SortProperty sortProp = null;
                if (SearchResourceListView.SortDirection == System.Web.UI.WebControls.SortDirection.Ascending)
                {
                    sortProp = new SortProperty(SearchResourceListView.SortExpression, Zentity.Platform.SortDirection.Ascending);
                }
                else
                {
                    sortProp = new SortProperty(SearchResourceListView.SortExpression, Zentity.Platform.SortDirection.Descending);
                }

                //search resources and user permissions on the searched resources.
                using (ResourceDataAccess dataAccess = new ResourceDataAccess())
                {
                    AuthenticatedToken token = this.Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;
                    if (token != null)
                    {
                        foundRestResource = dataAccess.SearchResources(token, searchText, pageSize, sortProp, totalParsedRecords,
                            ContentSearchCheckBox.Checked, out totalRecords).ToList();
                        userPermissions = GetPermissions(token, foundRestResource);
                    }
                }

                //Calculate total pages
                if (totalRecords > 0)
                {
                    //this.DisplayMatchedResourcesCount(totalRecords);
                    totalPages = Convert.ToInt32(Math.Ceiling((double)totalRecords / pageSize));
                }

                // Update empty resource's title with default value.
                if (foundRestResource != null && foundRestResource.Count() > 0)
                {
                    Utility.UpdateResourcesEmptyTitle(foundRestResource);
                }

                //Bind data to GridView
                SearchResourceListView.TotalRecords = totalRecords;

                SearchResourceListView.DataSource.Clear();
                foreach (Resource resource in foundRestResource)
                    SearchResourceListView.DataSource.Add(resource);

                SearchResourceListView.UserPermissions = userPermissions;
                SearchResourceListView.DataBind();
                if (!this.ContentSearchCheckBox.Checked)
                {
                    SearchResourceListView.EnableFeed(searchText);
                }
            }
            catch (SearchException ex)
            {
                LabelErrorMessage.Text = ex.Message;
            }
        }
        SearchText.Focus();
    }

    /// <summary>
    /// Get user permissions for the specified list of resources and their related resources.
    /// </summary>
    /// <param name="token">Authentication token</param>
    /// <param name="resources">List or resources</param>
    /// <returns>Mapping resource id and user permissons on the resource</returns>
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

    //Sets the status of content search check box depending on session setting whether
    //FTS is enabled for the database.
    private void SetContentSearchStatus()
    {
        if (Session[_ftsStatus] != null)
        {
            if (Convert.ToBoolean(Session[_ftsStatus]))
            {
                _ftsCurrentStatus = Resources.Resources.ContentSearchEnabledMessage;
                ContentSearchCheckBox.Enabled = true;
            }
            else
            {
                _ftsCurrentStatus = Resources.Resources.ContentSearchDisabledMessage;
                ContentSearchCheckBox.Enabled = false;
            }
        }
    }
    
    #endregion
}