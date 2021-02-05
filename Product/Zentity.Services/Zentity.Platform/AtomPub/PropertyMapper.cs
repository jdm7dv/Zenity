//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Famulus.Platform
{
    internal static class PropertyMapper
    {
        #region Fields

        private static Dictionary<string, string> _propertyMappings;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the resource property mapping collection.
        /// </summary>
        public static Dictionary<string, string> ResourceProperties
        {
            get
            {
                if (null == _propertyMappings)
                {
                    LoadResourcePropertyMappings();
                }
                return _propertyMappings;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Catches the mapped property names for metada properties.
        /// </summary>
        private static void LoadResourcePropertyMappings()
        {
            _propertyMappings = new Dictionary<string, string>();
            XmlDocument mappingDoc = new XmlDocument();
            mappingDoc.LoadXml(Microsoft.Famulus.Platform.Properties.Resources.ResourcePropertyMapping);
            foreach (XmlNode propertyNode in mappingDoc.SelectNodes("/Mapping/Proeprty"))
            {
                XmlNode valueNode = propertyNode.FirstChild;
                if (null != valueNode)
                {
                    _propertyMappings.Add(
                        propertyNode.Attributes["Name"].InnerText,
                        valueNode.InnerText);
                }
            }
        }

        #endregion
    }
}
