// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

#region .NET Framework Class Namespace Imports

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#endregion

#region Custom Namespace Imports

using Zentity.Core;
using Zentity.Platform.Properties;

#endregion

namespace Zentity.Platform
{
    /// <summary>
    /// Provides methods for fetching property tokens.
    /// </summary>
    internal class PropertyTokens
    {
        #region Public Methods

        /// <summary>
        /// Fetches the specified property token.
        /// </summary>
        /// <param name="token">Token.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch data with.</param>
        /// <returns>Property token.</returns>
        /// <exception cref="SearchException">Thrown when an ambiguous token is passed.</exception>
        public static ScalarProperty FetchPropertyToken(string token, ZentityContext context)
        {
            IEnumerable<ScalarProperty> matchingProperties = FetchPropertyTokens(context)
                .Where(property => property.Name.Equals(token, StringComparison.OrdinalIgnoreCase));

            if (matchingProperties.Count() > 1)
            {
                // If more than one match, then token is ambiguous so do not use either.
                throw new SearchException(string.Format(CultureInfo.CurrentCulture,
                    Resources.SEARCH_AMBIGUOUS_PROPERTY_TOKEN, token));
            }
            return matchingProperties.FirstOrDefault();
        }

        /// <summary>
        /// Fetches the specified property token.
        /// </summary>
        /// <param name="token">Token.</param>
        /// <param name="resourceTypeFullName">Full name of resource type.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch data with.</param>
        /// <returns>Property token.</returns>
        public static ScalarProperty FetchPropertyToken(
                                                    string token, 
                                                    string resourceTypeFullName,
                                                    ZentityContext context)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(resourceTypeFullName))
            {
                return null;
            }

            return FetchPropertyTokens(resourceTypeFullName, context)
                   .Where(property => property.Name.Equals(token, StringComparison.OrdinalIgnoreCase))
                   .FirstOrDefault();
        }

        /// <summary>
        /// Fetches the property tokens of the specified resource type and base types.
        /// </summary>
        /// <param name="type">Resource type.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch data with.</param>
        /// <returns>Property tokens.</returns>
        public static IEnumerable<ScalarProperty> FetchPropertyTokens(ResourceType type,
            ZentityContext context)
        {
            IEnumerable<ScalarProperty> properties = ResourceTypeHelper.FetchResourceTypes(context)
                .Where(resourceType => resourceType.Id == type.Id)
                .SelectMany(resourceType => resourceType.ScalarProperties);

            if (type.BaseType != null)
            {
                properties = properties.Concat(FetchPropertyTokens(type.BaseType, context));
            }
            return properties;
        }

        #endregion 

        #region Private Methods 

        /// <summary>
        /// Fetches the property tokens.
        /// </summary>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch data with.</param>
        /// <returns>Property tokens.</returns>
        private static IEnumerable<ScalarProperty> FetchPropertyTokens(ZentityContext context)
        {
            return ResourceTypeHelper.FetchResourceTypes(context)
                .SelectMany(resourceType => resourceType.ScalarProperties);
        }

        /// <summary>
        /// Fetches the property tokens of the specified resource type.
        /// </summary>
        /// <param name="resourceTypeFullName">Full name of resource type.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch data with.</param>
        /// <returns>Property tokens.</returns>
        private static IEnumerable<ScalarProperty> FetchPropertyTokens(string resourceTypeFullName,
            ZentityContext context)
        {
            ResourceType type = ResourceTypeHelper.FetchResourceType(resourceTypeFullName, context);
            return FetchPropertyTokens(type, context);
        }

        #endregion 
    }
}