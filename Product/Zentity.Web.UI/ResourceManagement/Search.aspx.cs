// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Resources;

public partial class ResourceManagement_Search : ZentityBasePage
{
    #region Event Handlers

    protected void Page_Load(object sender, EventArgs e)
    {
        LocalizePage();
    }

    #endregion

    #region Methods

    private void LocalizePage()
    {
        searchLabel.Text = Resources.Resources.ZentitySearch;
        basicSearchLabel.Text = Resources.Resources.BasicSearch;
        basicSearchDescriptionLabel.Text = Resources.Resources.Search_BasicSearchDesc;
    }

    #endregion
}
