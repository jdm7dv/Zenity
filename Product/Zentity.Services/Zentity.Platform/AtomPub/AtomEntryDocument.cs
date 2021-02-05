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
    using System.ServiceModel.Syndication;
    using System.Xml;
    using System.Xml.Linq;
    using Zentity.Platform.Properties;

    /// <summary>
    /// Represents an in-memory Atom Entry Document.
    /// </summary>
    public class AtomEntryDocument
    {
        #region Fields

        //  Atom Entry Document is generating by using SyndicationItem object
        private SyndicationItem atomDocument;
        private string source;
        private List<string> xmlLinks;

        #endregion

        #region .ctor

        /// <summary>
        /// Initializes a new instance of the AtomEntryDocument class.
        /// </summary>        
        public AtomEntryDocument()
        {
            AtomDocument = new SyndicationItem();
            this.XmlLinks = new List<string>();
            this.Title = new TextSyndicationContent(string.Empty);
            this.Content = new TextSyndicationContent(string.Empty);
        }

        /// <summary>
        /// Initializes a new instance of the AtomEntryDocument class.
        /// </summary>
        /// <param name="xmlFragment">String containing Atom XML fragment.</param>
        public AtomEntryDocument(string xmlFragment)
            : this()
        {
            if(string.IsNullOrEmpty(xmlFragment))
            {
                throw new ArgumentNullException("xmlFragment");
            }

            XmlTextReader atomXmlReader = null;

            try
            {
                atomXmlReader = new XmlTextReader(xmlFragment, XmlNodeType.Document, null);
                CreateAtomEntryDocument(atomXmlReader);
            }
            finally
            {
                if(null != atomXmlReader)
                {
                    atomXmlReader.Close();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the AtomEntryDocument class.
        /// </summary>
        /// <param name="inputStream">A stream containing Atom Entry document</param>
        public AtomEntryDocument(Stream inputStream)
            : this()
        {
            if(null == inputStream)
            {
                throw new ArgumentNullException("inputStream");
            }

            XmlReader atomXmlReader = null;

            try
            {
                atomXmlReader = XmlReader.Create(inputStream);
                CreateAtomEntryDocument(atomXmlReader);
            }
            finally
            {
                if(null != atomXmlReader)
                {
                    atomXmlReader.Close();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the AtomEntryDocument class.
        /// </summary>
        /// <param name="item">SyndicationItem object which represents AtomEntryDocument.</param>
        internal AtomEntryDocument(SyndicationItem item)
            : this()
        {
            if(null == item)
            {
                throw new ArgumentNullException("item");
            }

            AtomDocument = item;
            SetProperties();
        }


        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets title of the Atom Entry Document.
        /// </summary>
        public TextSyndicationContent Title
        {
            get
            {
                return AtomDocument.Title;
            }
            set
            {
                if(null == value)
                {
                    throw new ArgumentNullException("value");
                }

                AtomDocument.Title = value;
            }
        }

        /// <summary>
        /// Gets or sets id of the Atom Entry Document.
        /// </summary>
        public string Id
        {
            get
            {
                return AtomDocument.Id;
            }
            set
            {
                if(string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                AtomDocument.Id = value;
            }
        }

        /// <summary>
        /// Gets or sets updated date of the Atom Entry Document.
        /// </summary>
        public DateTimeOffset LastUpdatedTime
        {
            get
            {
                return AtomDocument.LastUpdatedTime;
            }
            set
            {
                AtomDocument.LastUpdatedTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the copyright value.
        /// </summary>
        public TextSyndicationContent Copyright
        {
            get
            {
                return this.AtomDocument.Copyright;
            }
            set
            {
                this.AtomDocument.Copyright = value;
            }
        }

        /// <summary>
        /// Gets or sets published date.
        /// </summary>
        public DateTimeOffset PublishDate
        {
            get
            {
                return AtomDocument.PublishDate;
            }
            set
            {
                AtomDocument.PublishDate = value;
            }
        }

        /// <summary>
        /// Gets contributors of the Atom Entry Document.
        /// </summary>
        public Collection<SyndicationPerson> Contributors
        {
            get
            {
                return this.AtomDocument.Contributors;
            }
        }

        /// <summary>
        /// Gets or sets content of the Atom Entry Document.
        /// </summary>
        public SyndicationContent Content
        {
            get
            {
                return AtomDocument.Content;
            }
            set
            {
                if(null == value)
                {
                    throw new ArgumentNullException("value");
                }

                AtomDocument.Content = value;
            }
        }

        /// <summary>
        /// Gets or sets source of the Atom Entry Document.
        /// </summary>
        public TextSyndicationContent Summary
        {
            get
            {
                return AtomDocument.Summary;
            }
            set
            {
                AtomDocument.Summary = value;
            }
        }

        /// <summary>
        /// Gets link values of the Atom Entry Document..
        /// </summary>
        public Collection<SyndicationLink> Links
        {
            get
            {
                return AtomDocument.Links;
            }
        }

        /// <summary>
        /// Gets author values of the Atom Entry Document.
        /// </summary>
        public Collection<SyndicationPerson> Authors
        {
            get
            {
                return AtomDocument.Authors;
            }
        }

        /// <summary>
        /// Gets Atom Entry Document XML.
        /// </summary>
        public string AtomEntry
        {
            get
            {
                return GetAtomEntryData();
            }
        }

        /// <summary>
        /// Gets the atom:link elements in xml string
        /// </summary>
        internal List<string> XmlLinks
        {
            get
            {
                return xmlLinks;
            }
            private set
            {
                xmlLinks = value;
            }
        }

        /// <summary>
        /// Gets or sets atom:source element in xml string
        /// </summary>
        internal string Source
        {
            get
            {
                return source;
            }
            set
            {
                source = value;
            }
        }

        /// <summary>
        /// Gets or sets the SyndicationItem.
        /// </summary>
        private SyndicationItem AtomDocument
        {
            get
            {
                return atomDocument;
            }
            set
            {
                atomDocument = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the entry stream.
        /// </summary>
        /// <returns>The entry <see cref="Stream"/>.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Stream will be closed by the calling method.")]
        private Stream GetEntryStream()
        {
            XmlWriter writer = null;
            MemoryStream entryStream = new MemoryStream();

            try
            {
                writer = XmlWriter.Create(entryStream);
                Atom10ItemFormatter atomFormatter = AtomDocument.GetAtom10Formatter();
                atomFormatter.WriteTo(writer);
                writer.Close();
                entryStream.Seek(0, SeekOrigin.Begin);
            }
            finally
            {
                if(null != writer)
                {
                    writer.Close();
                }
            }

            return entryStream;
        }

        /// <summary>
        /// Get Atom entry xml data.
        /// </summary>
        /// <returns>Atom entry xml data string.</returns>
        private string GetAtomEntryData()
        {
            SetSyndicationProperties();

            using (Stream entryStream = GetEntryStream())
            {
                entryStream.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(entryStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Sets the syndication properties.
        /// </summary>
        private void SetSyndicationProperties()
        {
            bool setSource = false;

            if(string.IsNullOrEmpty(this.Source))
            {
                AtomDocument.SourceFeed = null;
                setSource = true;
            }

            XElement entryElement = GetXmlEntry();

            if(!setSource)
            {
                try
                {
                    XElement source = XElement.Parse(this.Source);
                    entryElement.Add(source);
                }
                catch(XmlException)
                {
                    // Do nothing
                }
            }

            IEnumerable<string> xLinks = entryElement.Elements(AtomPubConstants.AtomEntryNamespace + AtomPubConstants.AtomLink)
                                                     .Select(link => link.ToString());

            IEnumerable<string> addedLinks = this.XmlLinks.Where(link => !xLinks.Contains(link));

            foreach(string xmlLink in addedLinks)
            {
                if(xLinks.Contains(xmlLink))
                {
                    continue;
                }

                XElement link = null;

                try
                {
                    link = XElement.Parse(xmlLink);
                }
                catch(XmlException)
                {
                    continue;
                }

                entryElement.Add(link);
            }

            XmlReader entryReader = new XmlTextReader(entryElement.ToString(), XmlNodeType.Document, null);
            AtomDocument = SyndicationItem.Load(entryReader);
        }

        /// <summary>
        /// Sets the properties.
        /// </summary>
        private void SetProperties()
        {
            XElement entryElement = GetXmlEntry();
            this.XmlLinks = entryElement.Elements(AtomPubConstants.AtomEntryNamespace + AtomPubConstants.AtomLink)
                                                     .Where(link => link.Attribute(AtomPubConstants.AtomRelative).Value != AtomPubConstants.Edit
                                                                && link.Attribute(AtomPubConstants.AtomRelative).Value != AtomPubConstants.EditMedia)
                                                     .Select(link => link.ToString())
                                                     .ToList();
            XElement sourceElement = entryElement.Element(AtomPubConstants.AtomEntryNamespace + AtomPubConstants.AtomSource);

            this.Source = (null != sourceElement) ? sourceElement.ToString() : null;
        }

        /// <summary>
        /// Creates the atom entry document.
        /// </summary>
        /// <param name="atomXmlReader">The atom XML reader.</param>
        private void CreateAtomEntryDocument(XmlReader atomXmlReader)
        {
            Atom10ItemFormatter formatter = new Atom10ItemFormatter();
            bool isValidAtomEntry = formatter.CanRead(atomXmlReader);

            if(isValidAtomEntry)
            {
                AtomDocument = SyndicationItem.Load(atomXmlReader);
                bool isValidAtomPubEntry = null != AtomDocument.Id &&
                                       DateTimeOffset.MinValue != AtomDocument.LastUpdatedTime &&
                                       null != AtomDocument.Title &&
                                       null != AtomDocument.Authors &&
                                       0 < AtomDocument.Authors.Count;

                if(isValidAtomPubEntry)
                {
                    if(null == AtomDocument.Content)
                    {
                        isValidAtomPubEntry = AtomDocument.Links.Where(atomLink => string.Compare(atomLink.RelationshipType, "alternate", StringComparison.OrdinalIgnoreCase) == 0)
                                                          .Count() == 1;
                    }
                    else if(!(AtomDocument.Content is TextSyndicationContent))
                    {
                        isValidAtomPubEntry = null != AtomDocument.Summary;
                    }
                }

                if(!isValidAtomPubEntry)
                {
                    throw new ArgumentException(Resources.ATOMPUB_INVALID_ATOMENTRYDOCUMENT, "atomXmlReader");
                }
                else
                {
                    SetProperties();
                }
            }
            else
            {
                throw new ArgumentException(Resources.ATOMPUB_INVALID_ATOMENTRYDOCUMENT, "atomXmlReader");
            }
        }

        /// <summary>
        /// Gets the XML entry.
        /// </summary>
        /// <returns>The <see cref="XElement"/> with the xml entry.</returns>
        private XElement GetXmlEntry()
        {
            Stream entryStream = GetEntryStream();
            XmlReader reader = XmlReader.Create(entryStream);
            XElement entryElement = XElement.Load(reader);
            return entryElement;
        }

        #endregion
    }
}

