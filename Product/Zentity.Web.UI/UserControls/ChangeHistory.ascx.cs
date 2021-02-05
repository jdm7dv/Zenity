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

public partial class UserControls_ChangeHistory : System.Web.UI.UserControl
{
    #region Member Variables

    Guid _resourceId = Guid.Empty;

    #endregion

    #region Properties

    public Guid ResourceId
    {
        get
        {
            return _resourceId;
        }
        set
        {
            _resourceId = value;
        }
    }

    #endregion

    protected void Page_Load(object sender, EventArgs e)
    {
        ChangeHistoryControl.AuthenticatedToken = Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;
        ChangeHistoryControl.EntityId = ResourceId;
    }
}
