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
using System.IO;
using Zentity.Web.UI;

public partial class Help_Help : ZentityBasePage
{
    #region Event Handlers

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            ThemeRadioButtonList.DataSource = GetThemes();
            ThemeRadioButtonList.DataBind();
            ThemeRadioButtonList.SelectedValue = Page.StyleSheetTheme;
        }
    }

    protected void ThemeRadioButtonList_SelectedIndexChanged(object sender, EventArgs e)
    {
        Session.Add(Constants.SessionThemeName, ThemeRadioButtonList.SelectedValue);
        Server.Transfer(Request.FilePath);
    }

    #endregion Event Handlers

    #region Methods

    private static List<string> GetThemes()
    {
        DirectoryInfo dInfo = new DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath("~/App_Themes"));
        DirectoryInfo[] themeDirectories = dInfo.GetDirectories();
        return themeDirectories.Select(directory => directory.Name)
            .ToList();
    }

    #endregion Methods
}
