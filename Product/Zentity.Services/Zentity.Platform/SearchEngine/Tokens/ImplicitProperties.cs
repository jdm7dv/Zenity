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
    /// Provides methods for fetching implicit properties.
    /// </summary>
    internal class ImplicitProperties
    {
        #region Public Methods

        /// <summary>
        /// Fetches the implicit properties.
        /// </summary>
        /// <param name="resourceTypeFullName">Full name of resource type.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch data with.</param>
        /// <returns>Implicit scalar properties.</returns>
        public static IEnumerable<ScalarProperty> FetchImplicitProperties(string resourceTypeFullName,
            ZentityContext context)
        {
            if (String.IsNullOrEmpty(resourceTypeFullName))
            {
                throw new ArgumentNullException("resourceTypeFullName");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            XDocument implicitPropertiesDocument =
                Utility.ReadXmlConfigurationFile(Resources.SEARCH_IMPLICITPROPERTIES_FILENAME);

            if (implicitPropertiesDocument == null)
            {
                return Utility.CreateEmptyEnumerable<ScalarProperty>();
            }

            ResourceType type = ResourceTypeHelper.FetchResourceType(resourceTypeFullName, context);

            if (type == null)
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture,
                    Resources.SEARCH_INVALID_RESOURCETYPE, resourceTypeFullName));
            }
            return FetchImplicitProperties(implicitPropertiesDocument, type, context);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Fetches the property tokens of the specified resource type and base types.
        /// </summary>
        /// <param name="implicitPropertiesDocument">Implicit properties document.</param>
        /// <param name="type">Resource type.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch data with.</param>
        /// <returns>Implicit scalar properties.</returns>
        private static IEnumerable<ScalarProperty> FetchImplicitProperties(
            XDocument implicitPropertiesDocument, ResourceType type,
            ZentityContext context)
        {
            IEnumerable<XElement> xmlProperties =
                    implicitPropertiesDocument.Root
                    .Elements(XName.Get(SearchConstants.XML_MODULE, SearchConstants.XMLNS_NAMESPACE))
                    .Where(xmlModule => xmlModule.Attribute(SearchConstants.XML_NAMESPACE).Value == type.Parent.NameSpace)
                    .Elements(XName.Get(SearchConstants.XML_RESOURCETYPE, SearchConstants.XMLNS_NAMESPACE))
                    .Where(xmlResourceType => xmlResourceType.Attribute(SearchConstants.XML_NAME).Value == type.Name)
                    .Elements(XName.Get(SearchConstants.XML_PROPERTY, SearchConstants.XMLNS_NAMESPACE));

            IEnumerable<ScalarProperty> scalarProperties =
                PropertyTokens.FetchPropertyTokens(type, context);

            IEnumerable<ScalarProperty> implicitProperties =
                scalarProperties.Join<ScalarProperty, XElement, string, ScalarProperty>(xmlProperties,
                scalarProperty => scalarProperty.Name,
                xmlProperty => xmlProperty.Value,
                (scalarPropertyName, xmlPropertyName) => scalarPropertyName);

            if (type.BaseType != null)
            {
                implicitProperties =
                    implicitProperties.Concat(
                    FetchImplicitProperties(implicitPropertiesDocument, type.BaseType, context));
            }
            return implicitProperties;
        }

        #endregion
    }
}