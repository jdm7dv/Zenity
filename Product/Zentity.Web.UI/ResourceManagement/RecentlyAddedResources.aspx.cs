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
using Zentity.Web.UI;
using Zentity.Security.Authentication;

public partial class ResourceManagement_RecentlyAddedResources : ZentityBasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        #region Set authentication token

        ReportingViewDateAdded.AuthenticatedToken = this.Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;

        #endregion
    }
}
