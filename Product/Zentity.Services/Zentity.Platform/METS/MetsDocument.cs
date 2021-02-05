// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using Zentity.Platform.Properties;

    /// <summary>
    /// Class which process METS xml data.
    /// </summary>
    public class MetsDocument
    {
        #region Fields

        private MetadataSectionCollection files;
        private FileMetadataPointer[] metadaPointer;
        private XmlSchema simpleDCSchema;
        private XElement metsElement;
        private XNamespace metsNamespace = MetsConstants.MetsNamespace;
        private XNamespace xlinkNamespace = MetsConstants.XlinkNamespace;

        #endregion

        #region .ctor

        /// <summary>
        /// Prevents a default instance of the Zentity.Platform.MetsDocument class from being created.
        /// </summary>
        private MetsDocument()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetsDocument"/> class.
        /// </summary>
        /// <param name="metsFileName">File path for METS xml.</param>
        public MetsDocument(string metsFileName)
        {
            InitializeDocument(metsFileName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetsDocument"/> class.
        /// </summary>
        /// <param name="metsStream">File stream for METS xml.</param>
        public MetsDocument(Stream metsStream)
        {
            InitializeDocument(metsStream);
        }

        #endregion

        #region Properties

        #region Private

        /// <summary>
        /// Gets or sets the xml schema for Dublin Core xml data used in METS xml.
        /// </summary>
        private XmlSchema DublinCoreSchema
        {
            get
            {
                if(null == simpleDCSchema)
                {
                    LoadDcSchema();
                }
                return simpleDCSchema;
            }
            set
            {
                simpleDCSchema = value;
            }
        }

        /// <summary>
        /// Gets or sets the xml schema for METS xml.
        /// </summary>
        private XElement MetsElement
        {
            get
            {
                return metsElement;
            }
            set
            {
                metsElement = value;
            }
        }

        /// <summary>
        /// Gets the xml namespace used for METS xml tags.
        /// </summary>
        private XNamespace MetsNamespace
        {
            get
            {
                return metsNamespace;
            }
        }

        /// <summary>
        /// Gets the xml namespace used for Xlink xml tags.
        /// </summary>
        private XNamespace XlinkNamespace
        {
            get
            {
                return xlinkNamespace;
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets the metadata pointers for all files in the METS xml.
        /// </summary>
        internal FileMetadataPointer[] MetadataPointer
        {
            get
            {
                return metadaPointer;
            }
            set
            {
                metadaPointer = value;
            }
        }

        /// <summary>
        /// Gets the metadata for files in the METS xml.
        /// </summary>
        public MetadataSectionCollection Files
        {
            get
            {
                return files;
            }
            private set
            {
                files = value;
            }
        }

        #endregion

        #region Methods

        #region Private

        /// <summary>
        /// Initialize the MetsDocument instance.
        /// </summary>
        /// <param name="metsFileName">File path for METS xml.</param>
        private void InitializeDocument(string metsFileName)
        {
            if(string.IsNullOrEmpty(metsFileName.Trim()))
            {
                throw new ArgumentNullException("metsFileName");
            }

            Stream metsStream = null;
            try
            {
                metsStream = File.OpenRead(metsFileName);
                InitializeDocument(metsStream);
            }
            finally
            {
                if(null != metsStream)
                {
                    metsStream.Close();
                }
            }
        }

        /// <summary>
        /// Validate and catches the metadata id for all files in the METS xml.
        /// </summary>
        /// <param name="metsStream">File stream for METS xml.</param>
        private void InitializeDocument(Stream metsStream)
        {
            if(null == metsStream)
            {
                throw new ArgumentNullException("metsStream");
            }
            XmlReader metsXmlreader = XmlReader.Create(metsStream);
            MetsElement = XElement.Load(metsXmlreader);
            ValidateMetsDocument(metsStream);
            LoadFileMetadataPointer();
            Files = new MetadataSectionCollection(this);
        }

        /// <summary>
        /// Validate the METS document against METS schema.
        /// </summary>
        /// <param name="metsStream">File stream of a METS document to be validated.</param>
        /// <exception cref="MetsException">Throws exception if the given METS xml contains wrong data.</exception>
        private void ValidateMetsDocument(Stream metsStream)
        {
            XmlReaderSettings xmlRdSetting = new XmlReaderSettings();
            XmlSchema metsSchema = GetMetsSchema();
            xmlRdSetting.Schemas.Add(metsSchema);
            xmlRdSetting.ValidationType = ValidationType.Schema;
            metsStream.Seek(0, SeekOrigin.Begin);

            XmlReader validatorReader = null;
            try
            {
                validatorReader = XmlReader.Create(metsStream, xmlRdSetting);
                while(validatorReader.Read())
                    ;

                metsStream.Seek(0, SeekOrigin.Begin);
                ValidDublinCoreContent();
            }
            catch(XmlSchemaValidationException ex)
            {
                throw new MetsException(Resources.INVALID_METS_DOCUMENT, ex);
            }
            finally
            {
                if(null != validatorReader)
                {
                    validatorReader.Close();
                }
            }
        }

        /// <summary>
        /// Validate Dublin Core xml data against Dublin Core xml schema.
        /// </summary>
        private void ValidDublinCoreContent()
        {
            var dublinCoreElements = MetsElement.Descendants(this.MetsNamespace + MetsConstants.MdWrap)
                             .Where(wrap => wrap.Attribute(MetsConstants.MimeType) != null
                                            && wrap.Attribute(MetsConstants.MimeType).Value == MetsConstants.TextXml
                                            && wrap.Attribute(MetsConstants.MdType).Value == MetsConstants.DublinCorePrefix)
                             .Select(wrap => wrap.Element(this.MetsNamespace + MetsConstants.XmlData));
            foreach(XElement dublinCoreXml in dublinCoreElements)
            {
                XmlReaderSettings xmlRdSetting = new XmlReaderSettings();
                xmlRdSetting.ValidationType = ValidationType.Schema;
                xmlRdSetting.ConformanceLevel = ConformanceLevel.Auto;
                xmlRdSetting.Schemas.Add(this.DublinCoreSchema);
                XmlTextReader dublinCoreData = new XmlTextReader(
                                    dublinCoreXml.ToString(),
                                    XmlNodeType.Element,
                                    null);
                XmlReader validatorReader = null;

                try
                {
                    validatorReader = XmlReader.Create(dublinCoreData, xmlRdSetting);
                    while(validatorReader.Read())
                        ;
                }
                finally
                {
                    if(null != validatorReader)
                    {
                        validatorReader.Close();
                    }
                    if(null != dublinCoreData)
                    {
                        dublinCoreData.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Get the METS xml schema from resource file.
        /// </summary>
        /// <returns>METS xml schema</returns>
        private static XmlSchema GetMetsSchema()
        {
            XmlTextReader metsSchemaReader = null;
            XmlTextReader xlinkSchemaReader = null;

            try
            {
                metsSchemaReader = new XmlTextReader(Resources.Mets_v1_7_Schema,
                                     XmlNodeType.Document,
                                     null);
                XmlSchema metsSchema = XmlSchema.Read(metsSchemaReader, null);

                xlinkSchemaReader = new XmlTextReader(Resources.XlinkSchema,
                                    XmlNodeType.Document,
                                    null);
                XmlSchema xlinkSchema = XmlSchema.Read(xlinkSchemaReader, null);

                XmlSchemaImport simpleDcInclude = new XmlSchemaImport();
                simpleDcInclude.Namespace = MetsConstants.XlinkNamespace;
                simpleDcInclude.Schema = xlinkSchema;
                metsSchema.Includes.Add(simpleDcInclude);

                return metsSchema;
            }
            finally
            {
                if(null != metsSchemaReader)
                {
                    metsSchemaReader.Close();
                }
                if(null != xlinkSchemaReader)
                {
                    xlinkSchemaReader.Close();
                }
            }

        }

        /// <summary>
        /// Load Dublin Core xml schema from resource files.
        /// </summary>
        private void LoadDcSchema()
        {
            XmlTextReader simpleDCSchemaReader = null;
            XmlTextReader dublinCoreSchemaReader = null;
            XmlTextReader xmlSchemaReader = null;

            try
            {
                // Load Schema which describes "mets:xmlData" can contain "dc:elementContainer" types.
                simpleDCSchemaReader = new XmlTextReader(
                                                        Resources.SimpleDcSchema,
                                                        XmlNodeType.Document,
                                                        null);
                DublinCoreSchema = XmlSchema.Read(simpleDCSchemaReader, null);

                // Load Dublin Core xml Schema with "dc:elementContainer" types at namespace "http://purl.org/dc/elements/1.1/" .
                dublinCoreSchemaReader = new XmlTextReader(
                                                        Resources.DcSchema,
                                                        XmlNodeType.Document,
                                                        null);
                XmlSchema dublinCoreSchema = XmlSchema.Read(dublinCoreSchemaReader, null);

                // Load xml schema for namespace "http://www.w3.org/XML/1998/namespace" which is imported in Dublin Core schema.
                xmlSchemaReader = new XmlTextReader(
                                                Resources.XmlSchema,
                                                XmlNodeType.Document,
                                                null);
                XmlSchema xmlSchema = XmlSchema.Read(xmlSchemaReader, null);


                // Import "http://www.w3.org/XML/1998/namespace" namespace in Dublin Core xml schema.
                XmlSchemaImport dublinCoreInclude = new XmlSchemaImport();
                dublinCoreInclude.Namespace = MetsConstants.XmlNamespace;
                dublinCoreInclude.Schema = xmlSchema;
                dublinCoreSchema.Includes.Add(dublinCoreInclude);

                // Import "http://purl.org/dc/elements/1.1/" namespace in simple Dublin Core xml schema.                
                XmlSchemaImport simpleDcInclude = new XmlSchemaImport();
                simpleDcInclude.Namespace = MetsConstants.DublinCoreNamespace;
                simpleDcInclude.Schema = dublinCoreSchema;
                DublinCoreSchema.Includes.Add(simpleDcInclude);

            }
            finally
            {
                if(null != simpleDCSchemaReader)
                {
                    simpleDCSchemaReader.Close();
                }
                if(null != dublinCoreSchemaReader)
                {
                    dublinCoreSchemaReader.Close();
                }
                if(null != xmlSchemaReader)
                {
                    xmlSchemaReader.Close();
                }
            }
        }

        /// <summary>
        /// Combines two metadata values.
        /// </summary>
        /// <param name="sectionData">Target metadata to hold the combined result.</param>
        /// <param name="data">New metadata to combine the data.</param>
        private static void MergeMetadata(ref Metadata sectionData, Metadata data)
        {
            if(null == data)
            {
                throw new ArgumentNullException("data");
            }

            if(null == sectionData)
            {
                sectionData = new Metadata();
            }
            PropertyInfo[] properties = data.GetType().GetProperties();
            foreach(PropertyInfo dataProperty in properties.Where(pro => pro.Name != MetsConstants.Item))
            {
                object sourceData = dataProperty.GetValue(data, null);
                if(null == sourceData)
                {
                    continue;
                }
                object targetData = dataProperty.GetValue(sectionData, null);
                if(dataProperty.PropertyType == typeof(ReadOnlyCollection<string>))
                {
                    ReadOnlyCollection<string> sourceArray = sourceData as ReadOnlyCollection<string>;
                    ReadOnlyCollection<string> targetArray = targetData as ReadOnlyCollection<string>;
                    if(null == targetArray)
                    {
                        dataProperty.SetValue(sectionData, sourceArray, null);
                    }
                    else
                    {
                        dataProperty.SetValue(sectionData,
                                            new ReadOnlyCollection<string>(sourceArray.Union(targetArray).ToArray()),
                                            null);
                    }
                }
            }
        }

        /// <summary>
        /// Catches descriptive metadata id and administrative metadata id (Right section) for all files in the METS xml document.
        /// </summary>
        private void LoadFileMetadataPointer()
        {
            MetadataPointer = MetsElement.Descendants(this.MetsNamespace + MetsConstants.File)
                       .Where(fl => fl.Element(this.MetsNamespace + MetsConstants.Flocat) != null
                          && fl.Element(this.MetsNamespace + MetsConstants.Flocat).Attribute(this.XlinkNamespace + MetsConstants.Href) != null)
                       .Select(fl => new FileMetadataPointer
                       {
                           Id = fl.Attribute(MetsConstants.Id).Value,
                           FileName = fl.Element(this.MetsNamespace + MetsConstants.Flocat).Attribute(this.XlinkNamespace + MetsConstants.Href).Value,
                           RightsIds = (null == fl.Attribute(MetsConstants.AdmId)) ? new string[] { } :
                                        fl.Attribute(MetsConstants.AdmId).Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries),
                           DescriptiveIds = (null == fl.Attribute(MetsConstants.DmdId)) ? new string[] { } :
                                        fl.Attribute(MetsConstants.DmdId).Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                       })
                       .ToArray();
        }

        /// <summary>
        /// Get xmlData section for a file in METS xml.
        /// </summary>
        /// <param name="fileName">Name of file to get XML data sections.</param>
        /// <returns>Array of XML data section.</returns>
        private XmlMetadata[] GetXmlData(string fileName)
        {
            IEnumerable<FileMetadataPointer> filePointers = MetadataPointer.Where(flmdptr => flmdptr.FileName
                                                                            .Equals(fileName, StringComparison.OrdinalIgnoreCase));

            if(0 == filePointers.Count())
            {
                return null;
            }

            var descriptiveData = filePointers.SelectMany(file => file.DescriptiveIds)
                                    .Join(MetsElement.Descendants(this.MetsNamespace + MetsConstants.DmdSec),
                                        dmdId => dmdId,
                                        md => md.Attribute(MetsConstants.Id).Value,
                                        (dmdId, md) => new
                                        {
                                            MdWrap = md.Descendants(this.MetsNamespace + MetsConstants.MdWrap).FirstOrDefault(),
                                            IsDiscriptive = true
                                        });

            var rightsData = filePointers.SelectMany(file => file.RightsIds)
                                    .Join(MetsElement.Descendants(this.MetsNamespace + MetsConstants.RightsMD),
                                        admId => admId,
                                        md => md.Attribute(MetsConstants.Id).Value,
                                        (admId, md) => new
                                        {
                                            MdWrap = md.Descendants(this.MetsNamespace + MetsConstants.MdWrap).FirstOrDefault(),
                                            IsDiscriptive = false
                                        });

            XmlMetadata[] fileMetadata = descriptiveData.Union(rightsData)
                                         .Where(wp => wp.MdWrap != null
                                                    && wp.MdWrap.Attribute(MetsConstants.MimeType) != null
                                                    && wp.MdWrap.Attribute(MetsConstants.MimeType).Value.Equals(
                                                                       MetsConstants.TextXml, StringComparison.OrdinalIgnoreCase))
                                         .Select(wp => new XmlMetadata
                                           {
                                               Metadata = wp.MdWrap.Descendants(this.MetsNamespace + MetsConstants.XmlData)
                                                                   .FirstOrDefault(),
                                               MdType = wp.MdWrap.Attribute(MetsConstants.MdType).Value,
                                               IsDiscriptive = wp.IsDiscriptive
                                           })
                                           .ToArray();

            return fileMetadata;
        }

        #endregion

        /// <summary>
        /// Get the metadata for files in the METS xml.
        /// </summary>
        /// <param name="fileName">Name of the file to get metadata.</param>
        /// <returns>Metadata for the given file.</returns>
        internal MetadataSection GetMetadata(string fileName)
        {
            if(string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            XmlMetadata[] fileMetadata = GetXmlData(fileName);

            if(null == fileMetadata)
            {
                return null;
            }

            Metadata descriptiveData = null;
            Metadata administrativeData = null;

            foreach(XmlMetadata metadataXml in fileMetadata)
            {
                if(null == metadataXml.Metadata)
                {
                    continue;
                }
                IMetadataExtractor extractor = null;
                Metadata data = null;
                Stream xmlDataStream = new MemoryStream();
                StreamWriter dataWriter = new StreamWriter(xmlDataStream);

                dataWriter.Write(metadataXml.Metadata.ToString());
                dataWriter.Flush();
                xmlDataStream.Position = 0;

                if(MetsConstants.DublinCorePrefix == metadataXml.MdType)
                {
                    extractor = new DublinCoreMetadataExtractor();
                }

                data = extractor.ExtractMetadata(xmlDataStream);

                if(null == data)
                {
                    continue;
                }

                if(metadataXml.IsDiscriptive)
                {
                    MergeMetadata(ref descriptiveData, data);
                }
                else
                {
                    MergeMetadata(ref administrativeData, data);
                }
            }
            if(null == administrativeData && null == descriptiveData)
            {
                return null;
            }
            return new MetadataSection
            {
                FileName = fileName,
                AdministrativeMetadata = administrativeData,
                DescriptiveMetadata = descriptiveData
            };
        }

        #endregion
    }
}
