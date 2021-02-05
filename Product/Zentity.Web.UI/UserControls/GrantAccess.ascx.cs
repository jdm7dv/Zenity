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
using System.ComponentModel;
using Zentity.Core;
using Zentity.Web.UI;
using Zentity.Security.AuthorizationHelper;
using Zentity.Security.Authentication;

public partial class UserControls_GrantAccess : System.Web.UI.UserControl
{
    const string JS = "javascript:GranPermission({0},this);";
    private AuthenticatedToken userToken = null;
    public event EventHandler<GrantEventArgs> PermissionGranted;
    private const string UserOrGroupIdKey = "UserOrGroupId";
    private const string ResourceIdKey = "ResourceId";


    protected void Page_Load(object sender, EventArgs e)
    {
        userToken = (AuthenticatedToken)Session[Constants.AuthenticationTokenKey];

        HideControl();
    }

    public string UserOrGroupId
    {
        get
        {
            if (ViewState[UserOrGroupIdKey] != null)
                return ViewState[UserOrGroupIdKey].ToString();
            return null;
        }
        set
        {

            ViewState[UserOrGroupIdKey] = value;
        }
    }

    public string ResourceId
    {
        get
        {
            if (ViewState[ResourceIdKey] != null)
                return ViewState[ResourceIdKey].ToString();
            return null;
        }
        set
        {

            ViewState[ResourceIdKey] = value;
        }
    }

    public void HideControl()
    {
        this.Visible = false;
    }
    public void ShowRights(string userOrGroupId, string resourceId)
    {
        this.ResourceId = resourceId;
        this.UserOrGroupId = userOrGroupId;
        using (ResourceDataAccess resDataAccess = new ResourceDataAccess())
        {
            grdVwPermission.DataSource = resDataAccess.GetUsersOrGroupPremissions(
                                                  this.ResourceId,
                                                  this.UserOrGroupId,
                                                  userToken);
            grdVwPermission.DataBind();
        }
        if (grdVwPermission.Rows.Count > 0)
        {
            this.Visible = true;
        }
    }

    protected void grdVwPermission_OnRowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.DataItemIndex == -1)
            return;

        ((CheckBox)e.Row.FindControl("chkGrantPermission")).Attributes.Add("id", "Allow");
        ((CheckBox)e.Row.FindControl("chkRevokePermission")).Attributes.Add("id", "Deny");

    }
    protected void btnGrantAccess_Click(object sender, EventArgs e)
    {
        List<PermissionMap> permissionList = new List<PermissionMap>();
        foreach (GridViewRow item in grdVwPermission.Rows)
        {
            PermissionMap pm = new PermissionMap();
            pm.Permission = ((Label)item.FindControl("lblPermission")).Text;
            pm.Allow = ((CheckBox)item.FindControl("chkGrantPermission")).Checked;
            pm.Deny = ((CheckBox)item.FindControl("chkRevokePermission")).Checked;
            permissionList.Add(pm);
        }

        bool result = false;
        using (ResourceDataAccess dataAccess = new ResourceDataAccess(Utility.CreateContext()))
        {
            result = dataAccess.SetPermissionToResource(ResourceId, permissionList,
                                               UserOrGroupId,
                                                userToken);

        }

        if (PermissionGranted != null)
            PermissionGranted(sender, new GrantEventArgs(result));
    }
}

public class GrantEventArgs : EventArgs
{
    bool _grantSuccess = false;

    public bool GrantSuccess
    {
        get { return _grantSuccess; }
        set { _grantSuccess = value; }
    }

    public GrantEventArgs(bool success)
        : base()
    {
        _grantSuccess = success;
    }
}
