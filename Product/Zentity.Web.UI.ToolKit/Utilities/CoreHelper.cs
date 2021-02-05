// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
#region Using Namespace
using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Data.Objects;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Collections.Generic;
using Zentity.Core;
using Zentity.ScholarlyWorks;
using System.Collections.ObjectModel;
using System.Web.Caching;
using System.Globalization;
using System.Collections;
using System.Data.Common;
using Zentity.Security.Authorization;
using Zentity.Security.Authentication;
using Zentity.Security.AuthorizationHelper;
#endregion

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This class provide the functionality to fetch the related data from data source 
    /// through  Zentity.Core library.
    /// </summary>
    internal static class CoreHelper
    {
        #region Constants

        #region Private

        private const string _2yearFormatS = "yy";
        private const string _2yearFormatC = "YY";
        private const string _yearChar = "y";
        private const string _space = " ";
        private const string _group = "Group";
        private const string _identity = "Identity";
        private const string _tag = "Tag";
        private const string _categoryNode = "CategoryNode";
        private const string ScalarPropertiesKey = "ScalarProperties";
        private const string NavigationalPropertiesKey = "NavigationalProperties";

        #endregion

        #endregion Constants

        #region Methods

        #region Public

        public static void LoadMetadata(ZentityContext context)
        {
            context.MetadataWorkspace.LoadFromAssembly(System.Reflection.Assembly.GetAssembly
              (typeof(ScholarlyWorkItem)));
            context.MetadataWorkspace.LoadFromAssembly(System.Reflection.Assembly.GetAssembly
              (typeof(Identity)));
        }

        /// <summary>
        /// Updates Resource's title with default value if title is null or empty string.
        /// </summary>
        /// <param name="resources">Collection of resource.</param>
        public static void UpdateResourcesEmptyTitle(IEnumerable<Resource> resources)
        {
            IEnumerable<Resource> emptyTitleResources = resources.Where(res => res.Title == null || string.IsNullOrEmpty(res.Title.Trim())).AsEnumerable();
            foreach (Resource res in emptyTitleResources)
            {
                if (res.Title == null || string.IsNullOrEmpty(res.Title.Trim()))
                {
                    res.Title = Resources.GlobalResource.ResourceEmptyTitle;
                }
            }
        }

        /// <summary>
        /// Updates provided title with default value if title contains null or empty string.
        /// </summary>
        /// <param name="title">Resource's title.</param>
        public static string UpdateEmptyTitle(string title)
        {
            return title == null || string.IsNullOrEmpty(title.Trim()) ? Resources.GlobalResource.ResourceEmptyTitle : title;
        }

        /// <summary>
        /// Returns current date format.
        /// </summary>
        /// <returns>current date format</returns>
        public static string GetDateFormat()
        {
            CultureInfo current = CultureInfo.CurrentCulture;
            string _dateTimeFormat = current.DateTimeFormat.ShortDatePattern;

            // If year format is yy or YY then replace it with yyyy or YYYY respectively.
            int startIndex = _dateTimeFormat.IndexOf(_yearChar, StringComparison.OrdinalIgnoreCase);
            int endIndex = _dateTimeFormat.LastIndexOf(_yearChar, StringComparison.OrdinalIgnoreCase);
            if ((endIndex - startIndex) == 1)
            {
                _dateTimeFormat = _dateTimeFormat.Replace(_2yearFormatS, _2yearFormatS + _2yearFormatS);
                _dateTimeFormat = _dateTimeFormat.Replace(_2yearFormatC, _2yearFormatC + _2yearFormatC);
            }
            return _dateTimeFormat;
        }

        /// <summary>
        /// Truncates a string.
        /// </summary>
        /// <param name="value">String to be truncated.</param>
        /// <param name="maximumChars">Maximum number of characters in truncated string.</param>
        /// <returns>Truncated string.</returns>
        public static string FitString(string value, int maximumChars)
        {
            if (string.IsNullOrEmpty(value) || maximumChars < 1)
            {
                return string.Empty;
            }
            string stringToCompress = value;
            if (stringToCompress.Length > maximumChars)
            {
                string strResult = stringToCompress.Substring(0, maximumChars);
                strResult += "...";
                return (strResult);
            }
            else
            {
                return value;
            }
        }

        public static string GetTitleByResourceType(Resource resource)
        {
            string title = string.Empty;

            if (resource != null)
            {
                title = resource.Title;

                //If resource is of type Person
                Person person = resource as Person;
                if (person != null && (!string.IsNullOrEmpty(person.FirstName) ||
                        !string.IsNullOrEmpty(person.LastName)))
                {
                    title = (person.FirstName + _space + person.LastName).Trim();
                }
                else
                {
                    //If resource is of type Tag
                    Tag tag = resource as Tag;
                    if (tag != null && !string.IsNullOrEmpty(tag.Name))
                    {
                        title = tag.Name;
                    }
                }
            }

            return title;
        }

        public static Collection<ScalarProperty> GetScalarProperties(Cache pageCache, ResourceType type)
        {
            Collection<ScalarProperty> scalarProperties = null;
            if (type != null)
            {
                if (pageCache[type.FullName + ScalarPropertiesKey] == null)
                {
                    scalarProperties = GetScalarProperties(scalarProperties, type);
                }
                else
                {
                    scalarProperties = pageCache[type.FullName + ScalarPropertiesKey] as Collection<ScalarProperty>;
                }
            }
            return scalarProperties;
        }

        public static Collection<NavigationProperty> GetNavigationalProperties(Cache pageCache, ResourceType type)
        {
            Collection<NavigationProperty> navigationalProperties = null;
            if (type != null)
            {
                if (pageCache[type.FullName + NavigationalPropertiesKey] == null)
                {
                    navigationalProperties = GetNavigationalProperties(navigationalProperties, type);
                }
                else
                {
                    navigationalProperties = pageCache[type.FullName + NavigationalPropertiesKey] as Collection<NavigationProperty>;
                }
            }
            return navigationalProperties;
        }

        public static string ExtractNamespace(string className)
        {
            string namespaceName = null;
            if (className.Contains(ResourcePropertyConstants.Dot))
            {
                namespaceName = className.Substring(0, className.LastIndexOf(".", StringComparison.Ordinal));
            }

            return namespaceName;
        }

        public static string ExtractTypeName(string className)
        {
            string typeName = className;
            if (className.Contains(ResourcePropertyConstants.Dot))
            {
                typeName = className.Substring(className.LastIndexOf(".", StringComparison.Ordinal) + 1);
            }

            return typeName;
        }

        public static string GetFileName(string fileTitle, string fileExtension)
        {
            string extension = fileExtension;
            if (!string.IsNullOrEmpty(extension) && !extension.StartsWith(Constants.Dot, StringComparison.Ordinal))
            {
                extension = Constants.Dot + extension;
            }

            return CoreHelper.UpdateEmptyTitle(fileTitle) + extension;
        }

        /// <summary>
        /// Filter security resource types
        /// </summary>
        /// <param name="resourceTypes">All resource types</param>
        /// <returns>Filtered resource types</returns>
        public static IEnumerable<ResourceType> FilterSecurityResourceTypes(IEnumerable<ResourceType> resourceTypes)
        {
            return resourceTypes.Where(tuple => tuple.Name != _group && tuple.Name != _identity);
        }

        /// <summary>
        /// Filter security resource types
        /// </summary>
        /// <param name="resourceTypes">All resource types</param>
        /// <returns>Filtered resource types</returns>
        public static IEnumerable<ResourceType> FilterTagCategoryResourceTypes(IEnumerable<ResourceType> resourceTypes)
        {
            return resourceTypes.Where(tuple => tuple.Name != _tag && tuple.Name != _categoryNode);
        }

        #endregion

        #region Private

        private static Collection<ScalarProperty> GetScalarProperties(Collection<ScalarProperty>
            scalarPropertyCollection, ResourceType type)
        {
            if (type != null)
            {
                if (scalarPropertyCollection == null)
                    scalarPropertyCollection = new Collection<ScalarProperty>();
                foreach (ScalarProperty property in type.ScalarProperties)
                {
                    scalarPropertyCollection.Add((property));
                }
                //If base type is not null then fetch scalar properties of Base class recursively
                if (type.BaseType != null)
                {
                    GetScalarProperties(scalarPropertyCollection, type.BaseType);
                }
            }

            return scalarPropertyCollection;
        }

        private static Collection<NavigationProperty> GetNavigationalProperties(Collection<NavigationProperty>
            navigationalPropertyCollection, ResourceType type)
        {
            if (type != null)
            {
                if (navigationalPropertyCollection == null)
                    navigationalPropertyCollection = new Collection<NavigationProperty>();
                foreach (NavigationProperty property in type.NavigationProperties)
                {
                    navigationalPropertyCollection.Add((property));
                }
                //If base type is not null then fetch navigational properties of Base class recursively
                if (type.BaseType != null)
                {
                    GetNavigationalProperties(navigationalPropertyCollection, type.BaseType);
                }
            }

            return navigationalPropertyCollection;
        }

        #endregion

        #endregion
    }
}