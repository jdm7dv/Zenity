// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Zentity.Core;
using System.Linq.Expressions;
using Zentity.Web.UI;
using System.Globalization;
using Zentity.Web.UI.ToolKit;
using Zentity.Security.Authentication;

public partial class ResourceManagement_Resources : ZentityBasePage
{
    #region Event Handlers

    protected void Page_Load(object sender, EventArgs e)
    {
        string entityType = Constants.ResourceEntityType;

        if (Request.QueryString[Constants.TypeName] != null)
        {
            entityType = Request.QueryString[Constants.TypeName].ToString();
        }

        LocalizePage(entityType);

        if (!Page.IsPostBack)
        {
            try
            {
                resourceSearch.EntityType = entityType;
            }
            catch (InvalidOperationException ex)
            {
                resourceSearch.Visible = false;
                titleLabel.Text = resourceSearch.Title;
                errorLabel.Text = ex.Message;
            }
        }

        //Set authenticatedToken to resource search control
        resourceSearch.AuthenticatedToken = Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;
    }

    #endregion

    #region Methods

    private void LocalizePage(string entityType)
    {
        if (entityType == Constants.Contact ||
            entityType == Constants.Person ||
            entityType == Constants.Organization)
        {
            Page.Title = Resources.Resources.ViewPersonPageTitle;
            resourceSearch.Title = Resources.Resources.ViewPersonPageTitle;
        }
        else
        {
            Page.Title = Resources.Resources.ResourceLabelListscolworkText;
            resourceSearch.Title = Resources.Resources.ResourceLabelListscolworkText;
        }
    }
    #endregion
}
