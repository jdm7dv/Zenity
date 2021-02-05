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
using Zentity.Security.Authentication;

public partial class ResourceManagement_ManageCategories : ZentityBasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //Set authenticatedToken to resource search control
        categoryHierarchy.AuthenticatedToken = Session[Zentity.Web.UI.Constants.AuthenticationTokenKey] as AuthenticatedToken;
    }
}
