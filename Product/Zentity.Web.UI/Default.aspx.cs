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
using Zentity.Security.Authentication;
using Zentity.Web.UI;

public partial class Default : ZentityBasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Control leftNavigationMenu = ((Master)Page.Master).NavigationMenuContainer;
        leftNavigationMenu.Visible = false;

        #region Set authentication token
        TagCloudControl.AuthenticatedToken = Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;
        #endregion
    }

    protected void SearchButton_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(SearchTextBox.Text.Trim()))
        {
            Response.Redirect(Constants.UriBasicSearch + Server.UrlEncode(SearchTextBox.Text.Trim()));
        }
    }
}