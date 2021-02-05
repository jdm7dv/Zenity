// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Zentity.Web.UI
{
    /// <summary>
    /// Summary description for Constants
    /// </summary>
    public sealed class Constants
    {
        private Constants()
        {
        }

        public const string PermissionRequiredForAssociation = UserResourcePermissions.Read;

        public const string SessionThemeName = "SessionThemeName";

        public const string TagFullName = "Zentity.ScholarlyWorks.Tag";
        public const string CategoryNodeFullName = "Zentity.ScholarlyWorks.CategoryNode";

        public const string PageSizeQueryStringParameter = "PageSize";
        public const string ContentSearchQueryStringParameter = "Content";
        public const string UriBasicSearch = "~/ResourceManagement/BasicSearch.aspx?SearchText=";
        public const string UriBasicSearchPage = "BasicSearch.aspx?SearchText={0}&PageSize={1}&Content={2}";
        public const string UriDisplayResourceDetailWithId = "~/ResourceManagement/ManageResource.aspx?ActiveTab=Summary&Id={0}";
        public const string UriManageResource = "ManageResource.aspx?Id={0}&ActiveTab={1}";

        public const string Id = "Id";

        public const string PropertyName = "Name";
        public const string Contact = "Contact";
        public const string Organization = "Organization";
        public const string Person = "Person";
        public const string NamespaceZentityCore = "Zentity.Core";
        public const string ResourceTypeInfo = "ResourceTypeInfo";

        public const string ResourceEntityType = "Resource";
        public const string TagEntityType = "Tag";
        public const string CategoryEntityType = "CategoryNode";
        public const string TypeName = "TypeName";

        public const string HtmlSpace = "&nbsp; " + "&nbsp; ";
        public const string CssStyleClass = "class";
        public const string RowStyle = "rStyle";
        public const string AlternateRowStyle = "arStyle";
        public const string Colon = ":";
        public const string Dot = ".";
        public const string Comma = ",";
        public const string Space = " ";
        public const string DoubleQuotes = "\"";
        public const string EscapedDoubleQuotes = "\\\"";

        public const string ValueTextBoxId = "ValueTextBox";
        public const string DateTimeTextBoxID = "DateTimeTextBox";
        public const string DateTimeRangeValidatorID = "dateTimeRange";
        public const string NumberRangeValidatorId = "NumberRangeValidator";
        public const string RequiredInputValidatorId = "RequiredInputValidator";
        public const string OnblurEvent = "onblur";
        public const string CauseValidationFunction = "javascript: CauseValidations('{0}')";

        public const string PageNotFoundQueryParam = "PageNotFound";
        public const string ErrorQueryParam = "Error";
        public const string ResourceNotFoundQueryParam = "ResourceNotFound";
        public const string TagNotFoundQueryParam = "TagNotFound";
        public const string CategoryNotFoundQueryParam = "CategoryNotFound";
        public const string ConcurrentAccessQueryParam = "ConcurrentAccess";
        public const string TimeoutQueryParam = "Timeout";
        public const string UnauthorizedQueryParam = "Unauthorized";
        public const string ExceptionMessageForCustomError = "ExceptionMessageForCustomError";
        public const string FormatExceptionQueryParam = "InvalidFormat";

        public static string AuthenticationTokenKey = "AuthenticatedToken";
        public static string URL = "url";
        public static string QueryStringSelectedPredicate = "SelectedPredicate";

        #region NavigationalConstants

        public const string MetadataTab = "Metadata";
        public const string AssociationTab = "Associations";
        public const string CategoriesTab = "Categories";
        public const string TagsTab = "Tags";
        public const string SummaryTab = "Summary";
        public const string ChangeHistoryTab = "ChangeHistory";
        public const string ResourcePermissionsTab = "ResourcePermissions";

        #endregion

    }

    public static class UserResourcePermissions
    {
        public const string Create = "Create";
        public const string Read = "Read";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string Owner = "Owner";
    }
}
