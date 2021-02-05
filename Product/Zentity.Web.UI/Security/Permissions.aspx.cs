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
using System.Collections;

public partial class Security_Permissions : ZentityBasePage
{
    const string PreviousSelectedRowIndex = "PreviousSelectedRowIndex";


    private Guid Id = Guid.Empty;
    const int _pageSize = 10;
    private AuthenticatedToken userToken = null;
    string selectedIdentityOrGroupName = string.Empty;

    protected void Page_Load(object sender, EventArgs e)
    {
        InitializeContols();
    }

    private void InitializeContols()
    {
        Initialize();

        userToken = (AuthenticatedToken)Session[Constants.AuthenticationTokenKey];

        ValidatePage();

        if (!Page.IsPostBack)
        {
            PopulateGlobalPermission();
        }
    }

    private void Initialize()
    {
        lblErrorOrMessage.Visible = false;
        lblErrorGlobalPermission.Visible = false;
        lblErrorResourcePermission.Visible = false;

        grantAccess.PermissionGranted += new EventHandler<GrantEventArgs>(grantAccess_PermissionGranted);

        if (Request.QueryString[Resources.Resources.QuerystringResourceId] != null)
        {
            try
            {
                Id = new Guid(Request.QueryString[Resources.Resources.QuerystringResourceId]);

                using (ResourceDataAccess dataAccess = new ResourceDataAccess(Utility.CreateContext()))
                {
                    Resource resourceData = dataAccess.GetResource(Id);
                    if (resourceData.GetType().Name == "Group")
                    {
                        Group group = (Group)resourceData;
                        lblPageTitle.Text = resourceData.GetType().Name + ": " + group.GroupName;
                        selectedIdentityOrGroupName = group.GroupName;
                    }
                    else
                    {
                        Identity identity = (Identity)resourceData;
                        lblPageTitle.Text = resourceData.GetType().Name + ": " + identity.IdentityName;
                        selectedIdentityOrGroupName = identity.IdentityName;
                    }
                }
            }
            catch
            {
                lblErrorOrMessage.Visible = true;
                PageContent.Visible = false;
                lblErrorOrMessage.Text = Resources.Resources.MsgUserOrGroupInvalid;
                return;
            }
        }

        if (UserManager.AdminUserName == selectedIdentityOrGroupName || UserManager.GuestUserName == selectedIdentityOrGroupName
            || UserManager.AdminGroupName == selectedIdentityOrGroupName)
        {
            GlobalPermissionPanel.Visible = false;
            return;
        }
    }

    private void PopulateGlobalPermission()
    {
        using (ResourceDataAccess resDataAccess = new ResourceDataAccess())
        {
            IEnumerable<PermissionMap> permissionMap = resDataAccess.GetCreatePremissions(Id.ToString(), userToken);

            grdGlobalPermission.DataSource = permissionMap;
            grdGlobalPermission.DataBind();


            if (grdGlobalPermission.Rows.Count <= 0)
            {
                GlobalPermissionPanel.Visible = false;
            }
        }
    }

    protected void btnGrantAccess_Click(object sender, EventArgs e)
    {
        bool result = false;
        List<PermissionMap> permissionList = new List<PermissionMap>();
        foreach (GridViewRow item in grdGlobalPermission.Rows)
        {
            PermissionMap pm = new PermissionMap();
            pm.Permission = ((Label)item.FindControl("lblPermission")).Text;
            pm.Allow = ((CheckBox)item.FindControl("chkGrantPermission")).Checked;
            pm.Deny = ((CheckBox)item.FindControl("chkRevokePermission")).Checked;
            permissionList.Add(pm);
        }
        using (ResourceDataAccess dataAccess = new ResourceDataAccess(Utility.CreateContext()))
        {
            result = dataAccess.SetCreatePremissions(permissionList[0], Id.ToString(), userToken);
        }

        if (result)
        {
            Utility.ShowMessage(lblErrorGlobalPermission, Resources.Resources.PermissionGranted, false);
        }
        else
        {
            Utility.ShowMessage(lblErrorGlobalPermission, Resources.Resources.FailToGrantPermission, false);
        }
        lblErrorGlobalPermission.Visible = true;
    }

    protected void btnSearchResources_Click(object sender, EventArgs e)
    {
        ViewState[PreviousSelectedRowIndex] = null;
        grantAccess.HideControl();

        ClearResult();

        FillResourceGrid();
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

    protected void ResourceTable_PageChanged(object sender, EventArgs e)
    {
        FillResourceGrid();
        ViewState[PreviousSelectedRowIndex] = null;
    }

    private void FillResourceGrid()
    {
        string searchText = txtSearchText.Text.Trim();

        int totalRecords = 0;
        int fetchedRecords = _pageSize * ResourceTable.PageIndex;
        ResourceTable.PageSize = _pageSize;
        List<Resource> resourceList = new List<Resource>();
        using (ResourceDataAccess dataAccess = new ResourceDataAccess(Utility.CreateContext()))
        {
            IEnumerable<Resource> resources = null;

            if (!rbSearchOptions.SelectedItem.Text.Equals(Resources.Resources.SearchInExistingList))
            {
                resources = dataAccess.GetResources(searchText);
            }
            else
            {
                resources = dataAccess.GetResourcesWithExplicitPermissions(Id, searchText, userToken);
            }
            resourceList = resources.OrderBy(tuple => tuple.Title).Skip(fetchedRecords).Take(_pageSize).ToList();
            totalRecords = resources.Count();

            Utility.UpdateResourcesEmptyTitle(resourceList);

        }

        if (!(totalRecords > 0))
        {
            lblErrorResourcePermission.Text = Resources.Resources.NoRecordsFound;
            lblErrorResourcePermission.ForeColor = System.Drawing.Color.Red;
            lblErrorResourcePermission.Visible = true;
        }

        if (resourceList != null)
        {
            //Update page count
            UpdatePageCount(ResourceTable, totalRecords);

            if (resourceList.Count > 0)
            {
                ResourceTable.DataSource = resourceList;
                ResourceTable.DataBind();
            }
        }
    }

    public void ClearResult()
    {
        ResourceTable.DataSource = null;
        ResourceTable.DataBind();
        ResourceTable.PageIndex = 0;
    }

    protected void ResourceTable_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != string.Empty || e.CommandArgument == null || e.CommandArgument.ToString() == string.Empty)
        {
            return;
        }

        int selectedRowIndex = Convert.ToInt32(e.CommandArgument);
        if (ViewState[PreviousSelectedRowIndex] != null)
        {
            int previousRowIndex = int.Parse(ViewState[PreviousSelectedRowIndex].ToString());
            ResourceTable.Rows[previousRowIndex].Font.Bold = false;
        }

        string securityObjectId = ResourceTable.DataKeys[selectedRowIndex].Value.ToString();
        ResourceTable.Rows[selectedRowIndex].Font.Bold = true;
        grantAccess.ShowRights(Id.ToString(), securityObjectId);

        if (grantAccess.Visible == false)
        {
            Utility.ShowMessage(lblErrorResourcePermission, Resources.Resources.NoPermissionText, true);
            lblErrorResourcePermission.Visible = true;
        }

        ViewState[PreviousSelectedRowIndex] = selectedRowIndex;
    }

    protected void ResourceTable_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ViewState[PreviousSelectedRowIndex] != null)
        {
            int previousRowIndex = int.Parse(ViewState[PreviousSelectedRowIndex].ToString());
            ResourceTable.Rows[previousRowIndex].Font.Bold = false;
        }
        string securityObjectId = ResourceTable.SelectedDataKey.Value.ToString();

        ResourceTable.SelectedRow.Font.Bold = true;
        ResourceTable.SelectedRow.BackColor = System.Drawing.Color.LightGray;
        grantAccess.ShowRights(Id.ToString(), securityObjectId);
        ViewState[PreviousSelectedRowIndex] = ResourceTable.SelectedIndex;
    }

    private void ValidatePage()
    {
        if (Id == Guid.Empty)
        {
            lblErrorOrMessage.Text = Resources.Resources.ResourceNotFound;
            lblErrorOrMessage.Visible = true;
        }

        using (ResourceDataAccess dataAccess = new ResourceDataAccess())
        {
            if (!userToken.IsAdmin(Utility.CreateContext()))
                throw new UnauthorizedAccessException(Resources.Resources.UnauthorizedAccessException);
        }
    }

    void grantAccess_PermissionGranted(object sender, GrantEventArgs e)
    {
        if (e.GrantSuccess)
        {
            Utility.ShowMessage(lblErrorResourcePermission, Resources.Resources.PermissionGranted, false);
        }
        else
        {
            Utility.ShowMessage(lblErrorResourcePermission, Resources.Resources.FailToGrantPermission, false);
        }
        lblErrorResourcePermission.Visible = true;
    }
}
