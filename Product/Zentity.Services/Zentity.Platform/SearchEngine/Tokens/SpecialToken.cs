// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;
    using Zentity.Core;
    using Zentity.Platform.Properties;

    /// <summary>
    /// Represents a special token.
    /// </summary>
    internal class SpecialToken
    {
        #region Properties

        /// <summary>
        /// Gets the special token.
        /// </summary>
        public string Token { get; private set; }

        /// <summary>
        /// Gets the data type.
        /// </summary>
        public DataTypes DataType { get; private set; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        public IEnumerable<ScalarProperty> Properties { get; private set; }

        #endregion 

        #region Public Methods

        /// <summary>
        /// Fetches data type of the specified token.
        /// </summary>
        /// <param name="token">A token.</param>
        /// <returns>Token data type.</returns>
        public static DataTypes FetchTokenDataType(string token)
        {
            if (String.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException("token");
            }
            IEnumerable<XElement> xmlTokens = FetchTokenElements(token);
            if (xmlTokens.Count() == 0)
            {
                throw new SearchException(
                    String.Format(CultureInfo.CurrentCulture, 
                    Resources.SEARCH_INVALID_TOKEN, token));
            }
            string tokenDataType = xmlTokens.First().Attribute(SearchConstants.XML_DATATYPE).Value;

            if (!String.IsNullOrEmpty(tokenDataType))
            {
                DataTypes dataType = (DataTypes)Enum.Parse(typeof(DataTypes), tokenDataType);
                return dataType;
            }
            else //By default treat it as a string 
            { 
                return DataTypes.String;
            }
        }

        /// <summary>
        /// Indicates whether the specified token is a special token.
        /// </summary>
        /// <param name="token">A token.</param>
        /// <returns>true if the token is a special token; otherwise, false.</returns>
        public static bool IsSpecialToken(string token)
        {
            if (String.IsNullOrEmpty(token))
            {
                return false;
            }
            IEnumerable<XElement> xmlTokens = FetchTokenElements(token);

            if (xmlTokens.Count() > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Fetches the specified special token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="resourceTypeFullName">Full name of resource type.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch data with.</param>
        /// <returns>Special token.</returns>
        public static SpecialToken FetchToken(
                                            string token, 
                                            string resourceTypeFullName,
                                            ZentityContext context)
        {
            if (String.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException("token");
            }
            return FetchTokens(resourceTypeFullName, context)
                .Where(property => String.Compare(
                    property.Token, token, StringComparison.OrdinalIgnoreCase) == 0)
                    .FirstOrDefault();
        }

        #endregion 

        #region Private Methods

        /// <summary>
        /// Fetches special tokens.
        /// </summary>
        /// <param name="resourceTypeFullName">Full name of resource type.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch data with.</param>
        /// <returns>Special tokens.</returns>
        private static IEnumerable<SpecialToken> FetchTokens(string resourceTypeFullName, ZentityContext context)
        {
            if (String.IsNullOrEmpty(resourceTypeFullName))
            {
                throw new ArgumentNullException("resourceTypeFullName");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            List<SpecialToken> specialTokens = new List<SpecialToken>();

            XDocument specialTokensDocument =
                 Utility.ReadXmlConfigurationFile(Resources.SEARCH_SPECIALTOKENS_FILENAME);

            if (specialTokensDocument == null)
            {
                return Utility.CreateEmptyEnumerable<SpecialToken>();
            }

            IEnumerable<XElement> xmlTokens =
                specialTokensDocument.Root
                .Elements(XName.Get(SearchConstants.XML_TOKEN, SearchConstants.XMLNS_NAMESPACE));

            ResourceType type =
                ResourceTypeHelper.FetchResourceType(resourceTypeFullName, context);

            if (type == null)
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture,
                    Resources.SEARCH_INVALID_RESOURCETYPE, resourceTypeFullName));
            }

            foreach (XElement xmlToken in xmlTokens)
            {
                SpecialToken specialToken = new SpecialToken();
                specialToken.Token = xmlToken.Attribute(SearchConstants.XML_NAME).Value;
                specialToken.DataType =
                    (DataTypes)Enum.Parse(
                    typeof(DataTypes), xmlToken.Attribute(SearchConstants.XML_DATATYPE).Value);

                specialToken.Properties = FetchProperties(xmlToken, type, context);
                specialTokens.Add(specialToken);
            }

            return specialTokens;
        }

        /// <summary>
        /// Fetches special properties.
        /// </summary>
        /// <param name="xmlToken">Xml token.</param>
        /// <param name="type">Resource type.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch data with.</param>
        /// <returns>Special Properties.</returns>
        private static IEnumerable<ScalarProperty> FetchProperties(
            XElement xmlToken, ResourceType type, ZentityContext context)
        {
            IEnumerable<XElement> xmlProperties = xmlToken
            .Elements(XName.Get(SearchConstants.XML_MODULE, SearchConstants.XMLNS_NAMESPACE))
            .Where(xmlModule => xmlModule.Attribute(SearchConstants.XML_NAMESPACE).Value == type.Parent.NameSpace)
            .Elements(XName.Get(SearchConstants.XML_RESOURCETYPE, SearchConstants.XMLNS_NAMESPACE))
            .Where(xmlResourceType => xmlResourceType.Attribute(SearchConstants.XML_NAME).Value == type.Name)
            .Elements(XName.Get(SearchConstants.XML_PROPERTY, SearchConstants.XMLNS_NAMESPACE));

            IEnumerable<ScalarProperty> scalarProperties = 
                PropertyTokens.FetchPropertyTokens(type, context);

            IEnumerable<ScalarProperty> specialProperties =
                scalarProperties.Join<ScalarProperty, XElement, string, ScalarProperty>(xmlProperties,
                scalarProperty => scalarProperty.Name,
                xmlProperty => xmlProperty.Value,
                (scalarPropertyName, xmlPropertyName) => scalarPropertyName);

            if (type.BaseType != null)
            {
                specialProperties = 
                    specialProperties.Concat(FetchProperties(xmlToken, type.BaseType, context));
            }

            return specialProperties;
        }

        /// <summary>
        /// Fetches the token elements.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>List of <see cref="XElement"/>.</returns>
        private static IEnumerable<XElement> FetchTokenElements(string token)
        {
            XDocument specialTokensDocument =
                  Utility.ReadXmlConfigurationFile(Resources.SEARCH_SPECIALTOKENS_FILENAME);

            if (specialTokensDocument == null)
            {
                return Utility.CreateEmptyEnumerable<XElement>();
            }

            IEnumerable<XElement> xmlTokens =
                specialTokensDocument.Root
                .Elements(XName.Get(SearchConstants.XML_TOKEN, SearchConstants.XMLNS_NAMESPACE))
                .Where(xmlToken => String.Compare(xmlToken.Attribute(SearchConstants.XML_NAME).Value, token, StringComparison.OrdinalIgnoreCase) == 0);
            return xmlTokens;
        }

        #endregion
    }
}