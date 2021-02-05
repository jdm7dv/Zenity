// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

    internal static class PropertyMapper
    {
        #region Fields

        private static Dictionary<string, string> propertyMappings;
        private const string PropertyMappingXPath = "/Mapping/Proeprty";

        #endregion

        #region Properties

        /// <summary>
        /// Gets the resource property mapping collection.
        /// </summary>
        public static Dictionary<string, string> ResourceProperties
        {
            get
            {
                if (null == propertyMappings)
                {
                    LoadResourcePropertyMappings();
                }
                return propertyMappings;
            }
        }       

        #endregion

        #region Methods

        /// <summary>
        /// Catches the mapped property names for metada properties.
        /// </summary>
        private static void LoadResourcePropertyMappings()
        {
            propertyMappings = new Dictionary<string, string>();
            XmlDocument mappingDoc = new XmlDocument();
            mappingDoc.LoadXml(Properties.Resources.ResourcePropertyMapping);
            foreach (XmlNode propertyNode in mappingDoc.SelectNodes(PropertyMappingXPath))
            {
                XmlNode valueNode = propertyNode.FirstChild;
                if (null != valueNode)
                {
                    propertyMappings.Add(
                        propertyNode.Attributes["Name"].InnerText,
                        valueNode.InnerText);
                }
            }
        }
        
        #endregion
    }
}
