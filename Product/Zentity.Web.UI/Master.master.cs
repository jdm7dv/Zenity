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
using System.Globalization;
using System.Configuration;
using Zentity.Web.UI.ToolKit;
using System.Web.UI.HtmlControls;

public partial class Master : System.Web.UI.MasterPage
{
    /// <summary>
    /// Gets left navigation menu.
    /// </summary>
    public ZentityBase SelectedBrowseTreeView
    {
        get
        {
            return NavigationMenu1.SeletedBrowTreeView;
        }
    }

    /// <summary>
    /// Get left navigation menu panel.
    /// </summary>
    public HtmlGenericControl NavigationMenuContainer
    {
        get
        {
            return LeftNavigationMenu;
        }
    }

    /// <summary>
    /// Handles page load event.
    /// </summary>
    /// <param name="sender">The object that raised this event.</param>
    /// <param name="e">Event arguments.</param>
    protected void Page_Load(object sender, EventArgs e)
    {
        // Required for resolving URL to script file.
        Page.Header.DataBind();
    }
}