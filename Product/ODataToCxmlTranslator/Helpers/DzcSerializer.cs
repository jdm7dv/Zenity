// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace ODataToCxmlTranslator
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// Serialize a Collection's image data into Deep Zoom Collection XML.
    /// Reference: http://www.getpivot.com/developer-info/image-content.aspx
    /// </summary>
    internal class DzcSerializer
    {
        #region Constructors, Finalizer and Dispose

        private DzcSerializer(Collection collection)
        {
            m_collection = collection;

            this.MaxLevel = DefaultMaxLevel;
            this.TileDimension = DefaultTilePixelDimension;
        }

        #endregion

        #region Public Properties

        public const int DefaultMaxLevel = 8;
        public const int DefaultTilePixelDimension = 256;

        int MaxLevel { get; set; }
        int TileDimension { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Write a collection's image data as a DZC into an XmlWriter.
        /// </summary>
        public static void Serialize(XmlWriter xmlWriter, Collection collection)
        {
            if (null == collection)
            {
                throw new ArgumentNullException("collection");
            }

            DzcSerializer s = new DzcSerializer(collection);
            s.Write(xmlWriter);
        }

        /// <summary>
        /// Write a collection's image data as a DZC into a TextWriter.
        /// </summary>
        public static void Serialize(TextWriter textWriter, Collection collection)
        {
            using (XmlWriter xmlWriter = XmlWriter.Create(textWriter))
            {
                Serialize(xmlWriter, collection);
            }
        }

        #endregion

        #region Private Properties

        private XNamespace Xmlns
        {
            get { return "http://schemas.microsoft.com/deepzoom/2008"; }
        }

        #endregion

        #region Private Methods

        private void Write(XmlWriter outputWriter)
        {
            outputWriter.WriteStartDocument();

            XStreamingElement root = MakeDzcTree();
            root.WriteTo(outputWriter);
        }

        private XStreamingElement MakeDzcTree()
        {
            XStreamingElement root = new XStreamingElement(Xmlns + "Collection", MakeCollectionContent());
            return root;
        }

        private IEnumerable<object> MakeCollectionContent()
        {
            yield return new XAttribute("MaxLevel", this.MaxLevel);
            yield return new XAttribute("TileSize", this.TileDimension);
            yield return new XAttribute("Format", "jpg");
            yield return new XAttribute("NextItemId", m_collection.Items.Count);

            yield return MakeItems();
        }

        private XStreamingElement MakeItems()
        {
            int itemId = 0;
            var xmlItems = m_collection.Items.Select(
                item => new XStreamingElement(Xmlns + "I", MakeItemContent(item, itemId++))
                );
            XStreamingElement items = new XStreamingElement(Xmlns + "Items", xmlItems);
            return items;
        }

        private IEnumerable<object> MakeItemContent(CollectionItem item, int id)
        {
            yield return new XAttribute("Id", id);
            yield return new XAttribute("N", id); //N is the Morton number of this item.
            yield return new XAttribute("IsPath", 1);
            yield return new XAttribute("Source", string.Empty);

            if (null != item.ImageProvider)
            {
                Size size = item.ImageProvider.Size;
                yield return new XStreamingElement(Xmlns + "Size",
                    new XAttribute("Width", size.Width), new XAttribute("Height", size.Height));
            }
        }

        #endregion

        #region Private Fields
        
        Collection m_collection;

        #endregion
    }
}
