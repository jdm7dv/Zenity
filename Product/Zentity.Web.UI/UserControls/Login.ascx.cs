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
using System.Globalization;
using Zentity.Web.UI;
using Zentity.Security.Authentication;
using System.Configuration;
using Zentity.Security.AuthorizationHelper;

public partial class UserControls_Login : System.Web.UI.UserControl
{
    #region Event Handlers

    /// <summary>
    /// Update status of login panels.
    /// </summary>
    /// <param name="e">Event arguments</param>
    protected override void OnLoad(EventArgs e)
    {
        AuthenticatedToken token = Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;

        //Update login control's visibility based on authentication information.
        if (token == null ||
            token.IdentityName.Equals(UserManager.GuestUserName, StringComparison.InvariantCultureIgnoreCase))
        {
            LoginPanel.Visible = true;
            LogoutPanel.Visible = false;
        }
        else
        {
            LoggedInUserLabel.Text = String.Format(CultureInfo.CurrentCulture, Resources.Resources.MsgWelcomeUser, token.IdentityName);

            LoginPanel.Visible = false;
            LogoutPanel.Visible = true;
        }

        base.OnLoad(e);

        ErrorMessageLabel.Text = string.Empty;
    }

    protected void LoginButton_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(UserNameTextBox.Text.Trim()) && !string.IsNullOrEmpty(PasswordTextBox.Text.Trim()))
        {
            using (ResourceDataAccess dataAccess = new ResourceDataAccess())
            {
                //Authenticate User
                AuthenticatedToken authenticatedToken = dataAccess.Authenticate(UserNameTextBox.Text, PasswordTextBox.Text);
                if (authenticatedToken == null)
                {
                    ErrorMessageLabel.Text = Resources.Resources.MsgLoginFailed;
                }
                else
                {
                    Session[Constants.AuthenticationTokenKey] = authenticatedToken;
                    LoginPanel.Visible = false;
                    LogoutPanel.Visible = true;
                    LoggedInUserLabel.Text = String.Format(CultureInfo.CurrentCulture, Resources.Resources.MsgWelcomeUser, UserNameTextBox.Text);
                    PasswordTextBox.Text = string.Empty;
                    if (Request.QueryString[Constants.URL] != null)
                        Response.Redirect(Request.QueryString[Constants.URL]);
                    else
                        Response.Redirect(Request.Url.ToString());
                }
            }
        }
    }

    protected void LogoutButton_Click(object sender, EventArgs e)
    {
        if (Session[Constants.AuthenticationTokenKey] != null)
            Session.Remove(Constants.AuthenticationTokenKey);
        Session.Abandon();
        Response.Redirect(Request.ApplicationPath);
    }

    #endregion Event Handlers
}
