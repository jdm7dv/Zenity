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
using Zentity.Security.AuthorizationHelper;
using Zentity.Security.AuthenticationProvider;
using Zentity.Web.UI;
using Zentity.Security.Authentication;

public partial class Security_ChangePassword : ZentityBasePage
{
    private string userName;

    protected void Page_Load(object sender, EventArgs e)
    {
        #region Get authentication token
        AuthenticatedToken token = Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;
        if (token != null)
        {
            userName = token.IdentityName;
            if (string.Compare(userName, UserManager.GuestUserName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                throw new UnauthorizedAccessException(Resources.Resources.UnauthorizedAccessException);
            }
        }
        #endregion
    }

    protected void ChangePasswordButton_Click(object sender, EventArgs e)
    {
        if (Page.IsValid)
        {
            bool success;
            try
            {
                success = ZentityUserManager.ChangePassword(userName, CurrentPasswordTextBox.Text,
                    NewPasswordTextBox.Text);
                if (success)
                {
                    Utility.ShowMessage(StatusLabel, Resources.Resources.PasswordChangeSuccess, false);
                    ChangePasswordPanel.Visible = false;
                }
                else
                {
                    Utility.ShowMessage(StatusLabel, Resources.Resources.PasswordChangeError, true);
                }
            }
            catch (AuthenticationException ex)
            {
                Utility.ShowMessage(StatusLabel,
                    Resources.Resources.PasswordChangeError + Constants.Space + ex.Message, true);
            }
        }
    }
}
