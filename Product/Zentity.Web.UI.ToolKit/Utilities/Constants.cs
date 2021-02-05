// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace Zentity.Web.UI.ToolKit
{
    #region Namespace

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    #endregion

    /// <summary>
    /// Common constants.
    /// </summary>
    internal static class Constants
    {
        internal const string PermissionRequiredForAssociation = UserResourcePermissions.Read;
        internal const string EmptyDivTag = "<div></div>";
        internal const string Hash = "#";
        internal const string Comma = ",";
        internal const string HtmlSpace = "&nbsp;";
        internal const string Space = " ";
        internal const string DoubleQuotes = "\"";
        internal const string EscapedDoubleQuotes = "\\\"";
        internal const string Colon = ":";
        internal const string Dot = ".";
        internal const int DesignTimeDummyDataRowCount = 5;

        internal const string AddedByPredicateName = "ScholarlyWorkItemIsAddedBy";
        internal const string EditedByPredicateName = "ScholarlyWorkIsEditedBy";

        internal const string ResourceFullName = "Zentity.Core.Resource";
        internal const string TagFullName = "Zentity.ScholarlyWorks.Tag";
        internal const string ScholarlyWorkItemFullName = "Zentity.ScholarlyWorks.ScholarlyWorkItem";
        internal const string CategoryNodeFullName = "Zentity.ScholarlyWorks.CategoryNode";

        internal const string OtherAuthors = "Others";
        internal const string A_EAuthors = "A-E";
        internal const string F_JAuthors = "F-J";
        internal const string K_OAuthors = "K-O";
        internal const string P_TAuthors = "P-T";
        internal const string U_ZAuthors = "U-Z";

        internal const string ResponseHeaderContentDispositionValue = "attachment; filename=";
        internal const string ResponseContentType = "application/octet-stream";
        internal const string ResponseHeaderContentDispositionName = "Content-Disposition";
        internal const string ResponseHeaderContentLengthName = "Content-Length";
    }

    internal static class UserResourcePermissions
    {
        internal const string Create = "Create";
        internal const string Read = "Read";
        internal const string Update = "Update";
        internal const string Delete = "Delete";
        internal const string Owner = "Owner";
    }
}