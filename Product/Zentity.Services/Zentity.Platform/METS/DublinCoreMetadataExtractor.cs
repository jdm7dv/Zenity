// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using Zentity.Platform.Properties;

    /// <summary>
    /// Class which process Dublin Core xml data.
    /// </summary>
    public class DublinCoreMetadataExtractor : IMetadataExtractor
    {
        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DublinCoreMetadataExtractor"/> class.
        /// </summary>
        public DublinCoreMetadataExtractor() { }

        #endregion

        #region Methods

        #region IMetadataExtractor Members

        /// <summary>
        /// Get the metadata from specified xml data stream.
        /// </summary>
        /// <param name="inputStream">Dublin Core xml meta data stream.</param>
        /// <returns>Dublin Core metadata for the specified xml data.</returns>
        /// <exception cref="ArgumentNullException">Throws exception if input stream is not valid.</exception>
        public Metadata ExtractMetadata(Stream inputStream)
        {
            if (null == inputStream || !inputStream.CanRead)
            {
                throw new ArgumentNullException("inputStream");
            }

            XElement dublinCoreElement;

            XmlTextReader dublinCoreXmlReader = new XmlTextReader(inputStream);
            try
            {
                dublinCoreElement = XElement.Load(dublinCoreXmlReader);
            }
            catch (XmlException ex)
            {
                throw new ExtractorException(Resources.INVALID_METS_DOCUMENT, ex);
            }

            if (0 == dublinCoreElement.Elements().Count())
            {
                return null;
            }

            var nodeValues = dublinCoreElement.Elements()
                        .Select(dt => dt.Name.LocalName)
                        .Distinct()
                        .GroupJoin(dublinCoreElement.Elements(),
                        name => name,
                        element => element.Name.LocalName,
                        (name, element) => new
                        {
                            Name = GetPropertyName(name),
                            Value = element.Nodes().Select(nd => nd.ToString().Trim()).ToArray()
                        })
                        .Where(property => property.Value.Length > 0)
                        .ToArray();

            if (0 == nodeValues.Length)
            {
                return null;
            }

            Metadata dublinCoreData = new Metadata();

            foreach (var data in nodeValues)
            {
                dublinCoreData[data.Name] = new ReadOnlyCollection<string>(data.Value);                
            }

            bool isPropertyUpdated = isPropertyUpdated = dublinCoreData.GetType().GetProperties()
                                    .Where(pr => MetsConstants.Item != pr.Name)
                                    .Select(pr => pr.GetValue(dublinCoreData, null))
                                    .Where(value => null != value)
                                    .Count() > 0;

            return (isPropertyUpdated) ? dublinCoreData : null;
        }

        /// <summary>
        /// Get the mapped metadata property name for a given DC xml tag.
        /// </summary>
        /// <param name="name">Name of the DC xml tag to get mapped property name.</param>
        /// <returns>Mapped metadata property name for a given DC xml tag.</returns>
        private static string GetPropertyName(string name)
        {
            if (MetsConstants.DcProperties.ContainsKey(name))
            {
                return MetsConstants.DcProperties[name];
            }
            else
            {
                return name;
            }
        }

        /// <summary>
        /// Extracts the metadata from the specified xml data.
        /// </summary>
        /// <param name="inputStream">The input stream.</param>
        /// <returns>The <see cref="Metadata"/>.</returns>
        Metadata IMetadataExtractor.ExtractMetadata(Stream inputStream)
        {
            return this.ExtractMetadata(inputStream);
        }

        #endregion

        #endregion
    }
}
