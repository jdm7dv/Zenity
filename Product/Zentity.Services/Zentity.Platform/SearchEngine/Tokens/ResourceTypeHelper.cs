// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

#region .NET Framework Class Namespace Imports

using System;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

#endregion

#region Custom Namespace Imports

using Zentity.Core;
using Zentity.Platform.Properties;

#endregion

namespace Zentity.Platform
{
    /// <summary>
    /// Provides methods for fetching resource types.
    /// </summary>
    internal class ResourceTypeHelper
    {
        #region Public Methods 

        /// <summary>
        /// Compares two specified resource types and returns an integer that indicates 
        /// their relationship to one another in the inheritance heirarchy.
        /// </summary>
        /// <param name="resourceTypeFullNameA">The first resource type full name.</param>
        /// <param name="resourceTypeFullNameB">The second resource type full name.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch data with.</param>
        /// <returns>A 32-bit signed integer indicating the relationship between the two
        /// Value Condition Less than zero resourceTypeFullNameB is derived from resourceTypeFullNameA.
        /// Value Condition Greater than zero resourceTypeFullNameA is derived from resourceTypeFullNameB.
        /// Zero, resourceTypeFullNameA and resourceTypeFullNameB have no relation.
        /// </returns>
        public static int CompareResourceTypes(
            string resourceTypeFullNameA, string resourceTypeFullNameB, ZentityContext context)
        {
            if (String.IsNullOrEmpty(resourceTypeFullNameA))
            {
                throw new ArgumentNullException("resourceTypeFullNameA");
            }
            if (String.IsNullOrEmpty(resourceTypeFullNameB))
            {
                throw new ArgumentNullException("resourceTypeFullNameB");
            }
            if (String.Equals(resourceTypeFullNameA, resourceTypeFullNameB))
            {
                return 1;
            }
            ResourceType typeA = FetchResourceType(resourceTypeFullNameA, context);
            int value = FetchResourceDerivedTypes(typeA, context)
                .Where(resourceType => resourceType.FullName == resourceTypeFullNameB).Count();
            if (value > 0)
            {
                return -1;
            }
            else
            {
                ResourceType typeB = FetchResourceType(resourceTypeFullNameB, context);
                value = FetchResourceDerivedTypes(typeB, context)
                    .Where(resourceType => resourceType.FullName == resourceTypeFullNameA).Count();
                if (value > 0)
                {
                    return 1;
                }
            }
            return 0;
        }

        /// <summary>
        /// Fetches the specified resource type.
        /// </summary>
        /// <param name="resourceTypeFullName">Full name of resource type.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch data with.</param>
        /// <returns>Resource type.</returns>
        public static ResourceType FetchResourceType(
            string resourceTypeFullName, ZentityContext context)
        {
            return FetchResourceTypes(context)
                        .Where(resourceType => resourceType.FullName.Equals(resourceTypeFullName,
                            StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }

        /// <summary>
        /// Fetches all resource types.
        /// </summary>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch data with.</param>
        /// <returns>Resource types.</returns>
        public static IEnumerable<ResourceType> FetchResourceTypes(ZentityContext context)
        {
            // context.DataModel.Modules gives all the modules in database not only 
            // loaded modules.
            // Fetch Resource types only from loaded data modules.
            IEnumerable<GlobalItem> globalItems = context.MetadataWorkspace.GetItemCollection(DataSpace.CSpace)
                .Where(item => item.BuiltInTypeKind == BuiltInTypeKind.EntityType);

            IEnumerable<string> loadedModuleNamespaces = globalItems.Select(globalItem => globalItem.MetadataProperties["NamespaceName"].Value.ToString()).Distinct();

            return context.DataModel.Modules.Where(module => loadedModuleNamespaces.Contains(module.NameSpace))
                        .SelectMany(module => module.ResourceTypes);
        }

        /// <summary>
        /// Fetches the full name of resource type.
        /// </summary>
        /// <param name="resourceType">Resource type.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch data with.</param>
        /// <returns>Full name of resource type.</returns>
        public static string FetchResourceTypeFullName(string resourceType, ZentityContext context)
        {
            if (String.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException("resourceType");
            }
            IEnumerable<ResourceType> types;
            if (resourceType.Contains(SearchConstants.DOT)) //if full resource type name
            {
                types = FetchResourceTypes(context)
                    .Where(rType => String.Compare(rType.FullName, resourceType, StringComparison.OrdinalIgnoreCase) == 0);
            }
            else
            {
                types = FetchResourceTypes(context)
                 .Where(rType => String.Compare(rType.Name, resourceType, StringComparison.OrdinalIgnoreCase) == 0);
            }

            int resourceTypeCount = types.Count();

            if (resourceTypeCount == 0)
            {
                throw new SearchException(
                    String.Format(CultureInfo.CurrentCulture,
                    Resources.SEARCH_INVALID_RESOURCETYPE, resourceType));
            }
            if (resourceTypeCount > 1)
            {
                throw new SearchException(
                    String.Format(CultureInfo.CurrentCulture,
                    Resources.SEARCH_AMBIGUOUS_RESOURCE_TYPE, resourceType));
            }

            return types.First().FullName;
        }

        /// <summary>
        /// Fetches the derived types of the specified resource type.
        /// </summary>
        /// <param name="type">Resource type.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch data with.</param>
        /// <returns>Derived types.</returns>
        public static IEnumerable<ResourceType> FetchResourceDerivedTypes(ResourceType type,
            ZentityContext context)
        {
            IEnumerable<ResourceType> derivedTypes = FetchResourceTypes(context)
                .Where(resourceType => resourceType.BaseType != null && resourceType.BaseType.Id == type.Id);

            foreach (ResourceType derivedType in derivedTypes)
            {
                derivedTypes = 
                    derivedTypes.Concat(FetchResourceDerivedTypes(derivedType, context));
            }

            return derivedTypes;
        }

        /// <summary>
        /// Fetches excluded resource types from an XML file.
        /// </summary>
        /// <returns>Excluded resource type full names.</returns>
        public static IEnumerable<string> FetchExcludedResourceTypeFullNames()
        {
            XDocument excludedResourceTypesDocument =
                Utility.ReadXmlConfigurationFile(Resources.SEARCH_EXCLUDEDRESOURCETYPES_FILENAME);

            if (excludedResourceTypesDocument == null)
            {
                return Utility.CreateEmptyEnumerable<string>();
            }

            IEnumerable<string> excludedResourceTypeFullNames =
                excludedResourceTypesDocument.Root
                .Elements(XName.Get(SearchConstants.XML_MODULE, SearchConstants.XMLNS_NAMESPACE))
                .Elements(XName.Get(SearchConstants.XML_RESOURCETYPE, SearchConstants.XMLNS_NAMESPACE))
                .Select(
                xmlResourceType => (xmlResourceType.Parent.Attribute(SearchConstants.XML_NAMESPACE).Value
                    + SearchConstants.DOT
                    + xmlResourceType.Attribute(SearchConstants.XML_NAME).Value).ToLower());

            if (excludedResourceTypeFullNames == null
                || excludedResourceTypeFullNames.Count() == 0)
            {
                return Utility.CreateEmptyEnumerable<string>();
            }
            else
            {
                return excludedResourceTypeFullNames;
            }
        }

        #endregion
    }
}