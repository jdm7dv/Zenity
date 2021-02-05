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
using System.Xml.Linq;

#endregion

#region Custom Namespace Imports

using Zentity.Core;
using Zentity.Platform.Properties;

#endregion

namespace Zentity.Platform
{
    /// <summary>
    /// Represents a predicate token.
    /// </summary>
    internal class PredicateToken
    {
        #region Properties

        /// <summary>
        /// Gets the predicate token.
        /// </summary>
        public string Token { get; private set; }

        /// <summary>
        /// Gets the predicate name.
        /// </summary>
        public string PredicateName { get; private set; }

        /// <summary>
        /// Gets whether the direction of the relationship.
        /// </summary>
        public bool ReverseRelation { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Fetches the specified predicate token.
        /// </summary>
        /// <param name="token">Predicate token.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch data with.</param>
        /// <returns>Predicate mapping for the specified token.</returns>
        public static IEnumerable<PredicateToken> FetchPredicateToken(string token, ZentityContext context)
        {
            return FetchPredicateTokens(context)
                .Where(predicateToken => predicateToken.Token.Equals(token, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Fetches predicate tokens.
        /// </summary>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch data with.</param>
        /// <returns>Predicate tokens</returns>
        private static IEnumerable<PredicateToken> FetchPredicateTokens(ZentityContext context)
        {
            XDocument mappings = Utility.ReadXmlConfigurationFile(Resources.SEARCH_PREDICATETOKENS_FILENAME);

            IEnumerable<PredicateToken> xmlTokens;
            if (mappings == null)
            {
                xmlTokens = new PredicateToken[0];
            }
            else
            {
                xmlTokens = mappings.Root
               .Elements(XName.Get(SearchConstants.XML_TOKEN, SearchConstants.XMLNS_NAMESPACE))
               .Select(token => new PredicateToken
               {
                   Token = token.Attribute(SearchConstants.XML_NAME).Value,
                   PredicateName = token.Attribute(SearchConstants.XML_PREDICATE).Value,
                   ReverseRelation = Convert.ToBoolean(
                       token.Attribute(SearchConstants.XML_REVERSERELATION).Value,
                       CultureInfo.CurrentCulture)
               });
            }

            IEnumerable<PredicateToken> databaseTokens = context.Predicates
                    .Select(predicate => new PredicateToken
                    {
                        Token = predicate.Name,
                        PredicateName = predicate.Name,
                        ReverseRelation = false
                    });

            xmlTokens = xmlTokens.Concat(databaseTokens);

            IEnumerable<PredicateToken> excludedPredicates = FetchExcludedPredicateTokens();

            xmlTokens = xmlTokens
                .Where(predicateToken => !excludedPredicates
                    .Select(excludedToken => excludedToken.PredicateName.ToLower())
                    .Contains(predicateToken.PredicateName.ToLower()));

            return xmlTokens;
        }

        /// <summary>
        /// Fetches excluded predicate tokens.
        /// </summary>
        /// <returns>Excluded predicate tokens.</returns>
        private static IEnumerable<PredicateToken> FetchExcludedPredicateTokens()
        {
            XDocument mappings = Utility.ReadXmlConfigurationFile(Resources.SEARCH_EXCLUDEDPREDICATES_FILENAME);

            IEnumerable<PredicateToken> xmlTokens;
            if (mappings == null)
            {
                xmlTokens = new PredicateToken[0];
            }
            else
            {
                xmlTokens = mappings.Root
               .Elements(XName.Get(SearchConstants.XML_PREDICATE, SearchConstants.XMLNS_NAMESPACE))
               .Select(token => new PredicateToken
               {
                   Token = token.Attribute(SearchConstants.XML_NAME).Value,
                   PredicateName = token.Attribute(SearchConstants.XML_NAME).Value,
                   ReverseRelation = false
               });
            }

            return xmlTokens;
        }

        #endregion
    }
}