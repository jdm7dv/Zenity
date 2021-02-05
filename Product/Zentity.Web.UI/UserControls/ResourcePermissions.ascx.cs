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
using System.Data;
using System.Globalization;
using Zentity.Core;
using Zentity.Web.UI;
using Zentity.Security.Authentication;
using Zentity.Security.Authorization;
using Zentity.Security.AuthorizationHelper;
using Zentity.Web.UI.ToolKit;

public partial class UserControls_ResourcePermissions : System.Web.UI.UserControl
{
    #region Member Variables

    private Guid resourceId = Guid.Empty;
    private const int _pageSize = 10;
    private AuthenticatedToken userToken = null;
    private const string PreviousSelectedRowIndexKey = "PreviousSelectedRowIndex";
    private const string IsUserSearchKey = "IsUserSearch";

    #endregion

    #region Properties

    public Guid ResourceId
    {
        get
        {
            return resourceId;
        }
        set
        {
            resourceId = value;
        }
    }

    #endregion

    #region Methods

    protected void Page_Load(object sender, EventArgs e)
    {
        InitializeContols();
    }

    private void InitializeContols()
    {
        grantAccess.PermissionGranted += new EventHandler<GrantEventArgs>(grantAccess_PermissionGranted);
        lblErrorOrMessage.Visible = false;
        userToken = (AuthenticatedToken)Session[Constants.AuthenticationTokenKey];

        Validate();
    }

    protected void btnSearchUsers_Click(object sender, EventArgs e)
    {
        ViewState[PreviousSelectedRowIndexKey] = null;

        ViewState[IsUserSearchKey] = true;
        grantAccess.HideControl();
        string searchText = txtSearchText.Text.Trim();

        ClearResult();

        grdGroupSearchResult.Visible = false;
        grdVwSearchResult.Visible = true;
        FillUserGrid();
    }

    private void FillUserGrid()
    {
        string searchText = txtSearchText.Text.Trim();

        int totalRecords = 0;
        int fetchedRecords = _pageSize * grdVwSearchResult.PageIndex;
        grdVwSearchResult.PageSize = _pageSize;

        List<Identity> identityList = null;
        using (ResourceDataAccess dataAccess = new ResourceDataAccess(Utility.CreateContext()))
        {
            IEnumerable<Identity> identities = null;
            if (rbSearchOptions.SelectedItem.Text.Equals(Resources.Resources.SearchInExistingList))
                identities = dataAccess.GetIdentitiesWithExplicitPermissions(resourceId, searchText, userToken);
            else
                identities = dataAccess.GetIdentities(searchText);

            totalRecords = identities.Count();
            identities = identities.OrderBy(tuple => tuple.Title).Skip(fetchedRecords).Take(_pageSize);
            identityList = identities.ToList();

            if (!(totalRecords > 0))
            {
                lblErrorOrMessage.Text = Resources.Resources.NoRecordsFound;
                lblErrorOrMessage.Visible = true;
            }
            Utility.UpdateResourcesEmptyTitle(identityList);
        }

        if (identityList != null)
        {
            //Update page count
            UpdatePageCount(grdVwSearchResult, totalRecords);

            if (identityList.Count > 0)
            {
                grdVwSearchResult.DataSource = identityList;
                grdVwSearchResult.DataBind();
            }
        }
    }

    protected void btnSearchGroups_Click(object sender, EventArgs e)
    {
        ViewState[PreviousSelectedRowIndexKey] = null;
        ViewState[IsUserSearchKey] = false;
        grantAccess.HideControl();

        string searchText = txtSearchText.Text.Trim();

        ClearResult();
        grdGroupSearchResult.Visible = true;
        grdVwSearchResult.Visible = false;
        FillGroupGrid();
    }

    private void FillGroupGrid()
    {
        string searchText = txtSearchText.Text.Trim();

        int totalRecords = 0;
        int fetchedRecords = _pageSize * grdGroupSearchResult.PageIndex;
        grdGroupSearchResult.PageSize = _pageSize;

        List<Group> groupList = null;
        using (ResourceDataAccess dataAccess = new ResourceDataAccess(Utility.CreateContext()))
        {
            IEnumerable<Group> groups = null;

            if (rbSearchOptions.SelectedItem.Text.Equals(Resources.Resources.SearchInExistingList))
                groups = dataAccess.GetGroupsWithExplicitPermissions(resourceId, searchText, userToken);
            else
                groups = dataAccess.GetGroups(searchText);

            totalRecords = groups.Count();
            groupList = groups.OrderBy(tuple => tuple.Title).Skip(fetchedRecords).Take(_pageSize).ToList();

            if (!(totalRecords > 0))
            {
                lblErrorOrMessage.Text = Resources.Resources.NoRecordsFound;
                lblErrorOrMessage.Visible = true;
            }
            Utility.UpdateResourcesEmptyTitle(groupList);
        }

        if (groupList != null)
        {
            //Update page count
            UpdatePageCount(grdGroupSearchResult, totalRecords);

            if (groupList.Count > 0)
            {
                grdGroupSearchResult.DataSource = groupList;
                grdGroupSearchResult.DataBind();
            }
        }
    }

    private void UpdatePageCount(ZentityDataGridView grdView, int totalRecords)
    {
        if (totalRecords > 0)
        {
            if (_pageSize > 0 && totalRecords > _pageSize)
            {
                grdView.PageCount =
                    Convert.ToInt32(Math.Ceiling((double)totalRecords / _pageSize));
            }
            else
            {
                grdView.PageCount = 1;
            }
        }
        else
        {
            grdView.PageCount = 0;
        }
    }

    protected void grdVwSearchResult_PageChanged(object sender, EventArgs e)
    {
        if (ViewState[IsUserSearchKey] != null)
        {
            if ((bool)ViewState[IsUserSearchKey])
                FillUserGrid();
            else
                FillGroupGrid();
        }
        grantAccess.HideControl();
        ViewState[PreviousSelectedRowIndexKey] = null;
    }

    public void ClearResult()
    {
        if (ViewState[IsUserSearchKey] != null)
        {
            if ((bool)ViewState[IsUserSearchKey])
            {
                grdVwSearchResult.DataSource = null;
                grdVwSearchResult.DataBind();
                grdVwSearchResult.PageIndex = 0;
            }
            else
            {
                grdGroupSearchResult.DataSource = null;
                grdGroupSearchResult.DataBind();
                grdGroupSearchResult.PageIndex = 0;
            }
        }
    }

    protected void grdVwSearchResult_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != string.Empty || e.CommandArgument == null || e.CommandArgument.ToString() == string.Empty)
        {
            return;
        }

        int selectedRowIndex = Convert.ToInt32(e.CommandArgument);

        if (ViewState[PreviousSelectedRowIndexKey] != null)
        {
            int previousRowIndex = int.Parse(ViewState[PreviousSelectedRowIndexKey].ToString());
            grdVwSearchResult.Rows[previousRowIndex].Font.Bold = false;
        }

        string securityObjectId = grdVwSearchResult.DataKeys[selectedRowIndex].Value.ToString();

        grdVwSearchResult.Rows[selectedRowIndex].Font.Bold = true;
        grantAccess.ShowRights(securityObjectId, resourceId.ToString());

        if (grantAccess.Visible == false)
        {
            Utility.ShowMessage(lblErrorOrMessage, Resources.Resources.NoPermissionText, true);
            lblErrorOrMessage.Visible = true;
        }

        ViewState[PreviousSelectedRowIndexKey] = selectedRowIndex;
    }

    protected void grdGroupSearchResult_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != string.Empty || e.CommandArgument == null || e.CommandArgument.ToString() == string.Empty)
        {
            return;
        }

        int selectedRowIndex = Convert.ToInt32(e.CommandArgument);

        if (ViewState[PreviousSelectedRowIndexKey] != null)
        {
            int previousRowIndex = int.Parse(ViewState[PreviousSelectedRowIndexKey].ToString());
            grdGroupSearchResult.Rows[previousRowIndex].Font.Bold = false;
        }

        string securityObjectId = grdGroupSearchResult.DataKeys[selectedRowIndex].Value.ToString();

        grdGroupSearchResult.Rows[selectedRowIndex].Font.Bold = true;
        grantAccess.ShowRights(securityObjectId, resourceId.ToString());

        if (grantAccess.Visible == false)
        {
            Utility.ShowMessage(lblErrorOrMessage, Resources.Resources.NoPermissionText, true);
            lblErrorOrMessage.Visible = true;
        }

        ViewState[PreviousSelectedRowIndexKey] = selectedRowIndex;
    }

    private void Validate()
    {
        if (resourceId == Guid.Empty)
        {
            lblErrorOrMessage.Text = Resources.Resources.ResourceNotFound;
            lblErrorOrMessage.Visible = true;
        }
        using (ResourceDataAccess dataAccess = new ResourceDataAccess())
        {
            if (!this.IsPostBack && !dataAccess.IsOwner(userToken, resourceId) && !dataAccess.IsAdmin(userToken))
                throw new UnauthorizedAccessException(Resources.Resources.MsgUnAuthorizeAccessOwner);
        }
    }

    void grantAccess_PermissionGranted(object sender, GrantEventArgs e)
    {
        if (e.GrantSuccess)
        {
            Utility.ShowMessage(lblErrorOrMessage, Resources.Resources.PermissionGranted, false);
        }
        else
        {
            Utility.ShowMessage(lblErrorOrMessage, Resources.Resources.FailToGrantPermission, false);
        }
        lblErrorOrMessage.Visible = true;

    }

    #endregion
}
