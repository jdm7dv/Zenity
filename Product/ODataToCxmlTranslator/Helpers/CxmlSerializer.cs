// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace ODataToCxmlTranslator
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// Serialize a Collection object into CXML.
    /// Reference: http://www.getpivot.com/developer-info/xml-schema.aspx
    /// </summary>
    internal class CxmlSerializer
    {
        #region Constructors, Finalizer and Dispose

        private CxmlSerializer(Collection collection)
        {
            m_collection = collection;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Write a collection object as CXML into a TextWriter.
        /// </summary>
        /// <param name="textWriter">The text writer.</param>
        /// <param name="collection">The collection.</param>
        public static void Serialize(TextWriter textWriter, Collection collection)
        {
            using (XmlWriter writer = XmlWriter.Create(textWriter))
            {
                Serialize(writer, collection);
            }
        }

        /// <summary>
        /// Write a collection object as CXML into an XmlWriter.
        /// </summary>
        /// <param name="outputWriter">The output writer.</param>
        /// <param name="collection">The collection.</param>
        public static void Serialize(XmlWriter outputWriter, Collection collection)
        {
            CxmlSerializer s = new CxmlSerializer(collection);
            s.Write(outputWriter);
        }

        /// <summary>
        /// Write a collection object as CXML into the given stream.
        /// </summary>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="collection">The collection.</param>
        public static void Serialize(Stream outputStream, Collection collection)
        {
            CxmlSerializer s = new CxmlSerializer(collection);
            using (XmlWriter writer = XmlWriter.Create(outputStream))
            {
                s.Write(writer);
            }
        }

        /// <summary>
        /// Write the collection object as CXML to a string.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns>The collection object as string.</returns>
        public static string Serialize(Collection collection)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                Serialize(memStream, collection);
                memStream.Position = 0; //Rewind the stream, ready for reading.

                using (StreamReader reader = new StreamReader(memStream))
                {
                    string text = reader.ReadToEnd();
                    return text;
                }
            }
        }

        #endregion

        #region Private Properties

        private XNamespace Xmlns
        {
            get { return "http://schemas.microsoft.com/collection/metadata/2009"; }
        }

        private XNamespace PivotXmlns
        {
            get { return "http://schemas.microsoft.com/livelabs/pivot/collection/2009"; }
        }

        #endregion

        #region Private Methods

        private void Write(XmlWriter outputWriter)
        {
            outputWriter.WriteStartDocument(); //Write the XML declaration

            XStreamingElement root = MakeCxmlTree();
            root.WriteTo(outputWriter);
        }

        private XStreamingElement MakeCxmlTree()
        {
            XStreamingElement root = new XStreamingElement(Xmlns + "Collection", MakeCollectionContent());
            return root;
        }

        private IEnumerable<object> MakeCollectionContent()
        {
            yield return new XAttribute("xmlns", Xmlns);
            yield return new XAttribute(XNamespace.Xmlns + "ui", PivotXmlns);

            yield return new XAttribute("SchemaVersion", m_collection.SchemaVersion.ToString());

            if (!string.IsNullOrEmpty(m_collection.Name))
            {
                yield return new XAttribute("Name", m_collection.Name);
            }

            if (null != m_collection.IconUrl)
            {
                yield return new XAttribute(PivotXmlns + "Icon", m_collection.IconUrl.AbsoluteUri);
            }

            if (null != m_collection.Culture)
            {
                string language = m_collection.Culture.Name;
                if (!string.IsNullOrEmpty(language))
                {
                    yield return new XAttribute(XNamespace.Xml + "lang", language);
                }
            }

            if (!m_collection.EnableInfoPaneBingSearch)
            {
                yield return new XAttribute(PivotXmlns + "AdditionalSearchText", "__block");
            }

            if (m_collection.HasFacets)
            {
                yield return MakeFacetCategories();
            }
            yield return MakeItems();
        }

        private XStreamingElement MakeFacetCategories()
        {
            XStreamingElement facetCats = new XStreamingElement(Xmlns + "FacetCategories", MakeFacetCategoriesContent());
            return facetCats;
        }

        private IEnumerable<XStreamingElement> MakeFacetCategoriesContent()
        {
            foreach (var cat in m_collection.FacetCategories)
            {
                yield return new XStreamingElement(Xmlns + "FacetCategory", MakeFacetCategoryContent(cat));
            }
        }

        private IEnumerable<XAttribute> MakeFacetCategoryContent(FacetCategory cat)
        {
            yield return new XAttribute("Name", cat.Name);
            yield return new XAttribute("Type", FacetTypeText(cat.FacetType));

            if (!string.IsNullOrEmpty(cat.DisplayFormat))
            {
                yield return new XAttribute("Format", cat.DisplayFormat);
            }

            if (!cat.IsShowInFacetPane)
            {
                yield return new XAttribute(PivotXmlns + "IsFilterVisible", false);
            }

            if (!cat.IsShowInInfoPane)
            {
                yield return new XAttribute(PivotXmlns + "IsMetaDataVisible", false);
            }

            if (!cat.IsTextFilter)
            {
                yield return new XAttribute(PivotXmlns + "IsWordWheelVisible", false);
            }
        }

        private string FacetTypeText(FacetType facetType)
        {
            switch (facetType)
            {
                default: //default falls through to Text
                case FacetType.Text:
                    return "String";

                case FacetType.Number:
                    return "Number";

                case FacetType.DateTime:
                    return "DateTime";

                case FacetType.Link:
                    return "Link";
            }
        }

        private XStreamingElement MakeItems()
        {
            XStreamingElement items = new XStreamingElement(Xmlns + "Items", MakeItemsContent());
            return items;
        }

        private IEnumerable<object> MakeItemsContent()
        {
            if (null != m_collection.ImgBaseName)
            {
                yield return new XAttribute("ImgBase", m_collection.ImgBaseName);
            }

            if (!string.IsNullOrEmpty(m_collection.HrefBase))
            {
                yield return new XAttribute("HrefBase", m_collection.HrefBase);
            }

            int itemId = 0;
            foreach (var item in m_collection.Items)
            {
                yield return new XStreamingElement(Xmlns + "Item", MakeItemContent(item, itemId++));
            }
        }

        private IEnumerable<object> MakeItemContent(CollectionItem item, int id)
        {
            if(item.Id == null)
                yield return new XAttribute("Id", id);
            else
                yield return new XAttribute("Id", item.Id);
            
            yield return new XAttribute("Img", "#" + id.ToString());
            if (!string.IsNullOrEmpty(item.Name))
            {
                yield return new XAttribute("Name", item.Name);
            }

            if (!string.IsNullOrEmpty(item.Url))
            {
                yield return new XAttribute("Href", item.Url);
            }

            if ((null != item.FacetValues) && (item.FacetValues.Count > 0))
            {
                //Note, Pivot does not accept an empty Facets element under Item.
                yield return new XStreamingElement(Xmlns + "Facets", MakeItemFacets(item.FacetValues));
            }
        }

        private IEnumerable<XStreamingElement> MakeItemFacets(IEnumerable<Facet> facets)
        {
            foreach (Facet facet in facets)
            {
                if (facet.IsTags)
                {
                    XStreamingElement[] tagElements = MakeItemFacetTags(facet).ToArray();
                    //Only emit a Facet element if there is content inside it.
                    if (tagElements.Length > 0)
                    {
                        yield return new XStreamingElement(Xmlns + "Facet", new XAttribute("Name", facet.Category),
                            tagElements);
                    }
                }
            }
        }

        private IEnumerable<XStreamingElement> MakeItemFacetTags(Facet facet)
        {
            if (facet.DataType == FacetType.Link)
            {
                foreach (FacetHyperlink value in facet.Tags)
                {
                    yield return new XStreamingElement(Xmlns + FacetTypeText(facet.DataType),
                        new XAttribute("Name", value.Name), new XAttribute("Href", value.Url));
                }
            }
            else
            {
                foreach (var value in facet.EnumerateNonEmptyTags())
                {
                    yield return new XStreamingElement(Xmlns + FacetTypeText(facet.DataType),
                        new XAttribute("Value", value));
                }
            }
        }

        #endregion

        #region Private Fields

        Collection m_collection;

        #endregion
    }
}
