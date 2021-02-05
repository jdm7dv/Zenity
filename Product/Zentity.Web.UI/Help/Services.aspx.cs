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
using System.Configuration;

public partial class Help_Services : ZentityBasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        SetEndPoint(Resources.Resources.EndPointAtomPub, HyperLinkAtomPub);
        SetEndPoint(Resources.Resources.EndPointSWORD, HyperLinkSword);
        SetEndPoint(Resources.Resources.EndPointSyndication, HyperLinkSyndication);
        SetEndPoint(Resources.Resources.EndPointOaiPmh, HyperLinkOaiPmh);
        SetEndPoint(Resources.Resources.EndPointOaiOre, HyperLinkOaiOre);
    }

    private static void SetEndPoint(string atomPubEndPointKey, HyperLink hyperLink)
    {
        string atomPubEndPoint = ConfigurationManager.AppSettings[atomPubEndPointKey];
        if (string.IsNullOrEmpty(atomPubEndPoint))
        {
            hyperLink.Text = Resources.Resources.ComponentNotInstalled;
            hyperLink.NavigateUrl = string.Empty;
        }
        else
        {
            hyperLink.Text = atomPubEndPoint;
            hyperLink.NavigateUrl = atomPubEndPoint;
            hyperLink.Target = "_blank";
        }
    }
}
