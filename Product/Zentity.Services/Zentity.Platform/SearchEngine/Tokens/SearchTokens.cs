// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using Zentity.Core;

    /// <summary>
    /// Represents search tokens.
    /// </summary>
    internal class SearchTokens
    {
        #region Private Fields

        private ZentityContext context;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the context to use for fetching data.
        /// </summary>
        public string SqlConnectionString
        {
            get;
            private set;
        }

        #endregion 

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTokens"/> class.
        /// </summary>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch data with.</param>
        public SearchTokens(ZentityContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            this.context = context;
            SqlConnectionString = context.StoreConnectionString;
        }

        #endregion 

        #region Public Methods

        /// <summary>
        /// Compares two specified resource types and returns an integer that indicates 
        /// their relationship to one another in the inheritance hierarchy.
        /// </summary>
        /// <param name="resourceTypeFullNameA">The first resource type full name.</param>
        /// <param name="resourceTypeFullNameB">The second resource type full name.</param>
        /// <returns>A 32-bit signed integer indicating the relationship between the two
        /// Value Condition Less than zero resourceTypeFullNameB is derived from resourceTypeFullNameA.
        /// Value Condition Greater than zero resourceTypeFullNameA is derived from resourceTypeFullNameB.
        /// Zero, resourceTypeFullNameA and resourceTypeFullNameB have no relation.
        /// </returns>
        public int CompareResourceTypes(string resourceTypeFullNameA, string resourceTypeFullNameB)
        {
            return ResourceTypeHelper.CompareResourceTypes(
                resourceTypeFullNameA, resourceTypeFullNameB, context);
        }

        /// <summary>
        /// Fetches data type of the specified token.
        /// </summary>
        /// <param name="token">A token.</param>
        /// <returns>Token data type.</returns>
        public static DataTypes FetchSpecialTokenDataType(string token)
        {
            return SpecialToken.FetchTokenDataType(token);
        }

        /// <summary>
        /// Fetches the implicit properties.
        /// </summary>
        /// <param name="resourceTypeFullName">Full name of resource type.</param>
        /// <returns>Implicit properties.</returns>
        public IEnumerable<ScalarProperty> FetchImplicitProperties(string resourceTypeFullName)
        {
            return ImplicitProperties.FetchImplicitProperties(resourceTypeFullName, context);
        }

        /// <summary>
        /// Fetches the specified predicate token.
        /// </summary>
        /// <param name="token">Predicate token.</param>
        /// <returns>Predicate mapping for the specified token.</returns>
        public IEnumerable<PredicateToken> FetchPredicateToken(string token)
        {
            return PredicateToken.FetchPredicateToken(token, context);
        }

        /// <summary>
        /// Fetches the specified property token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>Property token.</returns>
        /// <exception cref="SearchException">Thrown when an ambiguous token is passed.</exception>
        public ScalarProperty FetchPropertyToken(string token)
        {
            return PropertyTokens.FetchPropertyToken(token, context);
        }

        /// <summary>
        /// Fetches the specified property token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="resourceTypeFullName">Full name of resource type.</param>
        /// <returns>Property token.</returns>
        public ScalarProperty FetchPropertyToken(string token, string resourceTypeFullName)
        {
            return PropertyTokens.FetchPropertyToken(token, resourceTypeFullName, context);
        }

        /// <summary>
        /// Fetches the specified special token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="resourceTypeFullName">Full name of resource type.</param>
        /// <returns>Special token.</returns>
        public SpecialToken FetchSpecialToken(string token, string resourceTypeFullName)
        {
            return SpecialToken.FetchToken(token, resourceTypeFullName, context);
        }

        /// <summary>
        /// Fetches the full name of resource type.
        /// </summary>
        /// <param name="resourceType">Resource type.</param>
        /// <returns>Full name of resource type.</returns>
        public string FetchResourceTypeFullName(string resourceType)
        {
            return ResourceTypeHelper.FetchResourceTypeFullName(resourceType, context);
        }

        /// <summary>
        /// Fetches the specified resource type.
        /// </summary>
        /// <param name="resourceTypeFullName">Full name of resource type.</param>
        /// <returns>Resource type.</returns>
        public ResourceType FetchResourceType(string resourceTypeFullName)
        {
            return ResourceTypeHelper.FetchResourceType(resourceTypeFullName, context);
        }

        /// <summary>
        /// Fetches the derived types of the specified resource type.
        /// </summary>
        /// <param name="resourceType">Resource type.</param>
        /// <returns>Resource types.</returns>
        public IEnumerable<ResourceType> FetchResourceDerivedTypes(ResourceType resourceType)
        {
            return ResourceTypeHelper.FetchResourceDerivedTypes(resourceType, context);
        }

        /// <summary>
        /// Indicates whether the specified token is a special token.
        /// </summary>
        /// <param name="token">A token.</param>
        /// <returns>true if the token is a special token; otherwise, false.</returns>
        public static bool IsSpecialToken(string token)
        {
            return SpecialToken.IsSpecialToken(token);
        }

        /// <summary>
        /// Fetches excluded resource types from an XML file.
        /// </summary>
        /// <returns>Excluded resource type full names.</returns>
        public static IEnumerable<string> FetchExcludedResourceTypeFullNames()
        {
            return ResourceTypeHelper.FetchExcludedResourceTypeFullNames();
        }

        #endregion 
    }
}
