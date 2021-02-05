// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using Zentity.Core;
using Zentity.ScholarlyWorks;
using Zentity.Web.UI;
using System;
using Zentity.Security.Authentication;

public partial class ResourceManagement_Tags : ZentityBasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //Set authenticatedToken to tag search control
        tagSearch.AuthenticatedToken = Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;
    }
}
