// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace Zentity.Web.UI
{
    // This  Enum for selecting various string comparison options    
    public enum ResourceStringComparison
    {
        All,
        Equals,
        StartsWith,
        EndsWith,
        Contains,
        NotEqual
    }
}
