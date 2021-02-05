// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Xml;

    #region SerializeRDFXML Class
    class SerializeRDFXML<URI> : ISerializeORE
    {
        #region Private Members
        private Memento<URI> state;
        #endregion

        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializeRDFXML&lt;URI&gt;"/> class.
        /// </summary>
        /// <param name="state">memento state</param>
        public SerializeRDFXML(Memento<URI> state)
        {
            this.state = state;
        }
        #endregion

        #region Public methods

        /// <summary>
        /// Gets the URL.
        /// </summary>
        /// <param name="deployedAt">The deployed at.</param>
        /// <returns>string deployed value</returns>
        public string GetURL(string deployedAt)
        {

            return deployedAt + state.ResourceMapUri.ToString() + ".rdf";
        }

        /// <summary>
        /// Serializes the resource map
        /// </summary>
        /// <param name="deployedAt">deployed at string</param>
        /// <returns>serialized string</returns>
        public string Serialize(string deployedAt)
        {
            StringBuilder builder = new StringBuilder();
            StringWriterWithEncoding sw = new StringWriterWithEncoding(builder, System.Text.Encoding.UTF8);
            XmlTextWriter xmlWriter = new XmlTextWriter(sw);
            //xmlWriter.Formatting = Formatting.Indented;

            WriteHeaders(xmlWriter);

            WriteDescriptionHeader(deployedAt, xmlWriter);

            SerializeAggregateResource(deployedAt, xmlWriter);

            xmlWriter.WriteEndDocument();
            xmlWriter.Flush();
            xmlWriter.Close();
            sw.Flush();
            return sw.ToString();
        }

        /// <summary>
        /// Serializes aggregated resource
        /// </summary>
        /// <param name="deployedAt">deployed string</param>
        /// <param name="writer">xml writer</param>
        public void SerializeAggregateResource(string deployedAt, XmlWriter writer)
        {
            SerializeAggregations(writer);
            SerializeMetadata(writer);
            SerializeAggregateByResource(deployedAt, writer);
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Writes the description header.
        /// </summary>
        /// <param name="deployedAt">The deployed at.</param>
        /// <param name="xmlWriter">The XML writer.</param>
        private void WriteDescriptionHeader(string deployedAt, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            xmlWriter.WriteStartAttribute("rdf", "about", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            xmlWriter.WriteValue(GetURL(deployedAt));
            xmlWriter.WriteEndAttribute();
            xmlWriter.WriteStartElement("ore:describes", "");
            xmlWriter.WriteStartAttribute("rdf:resource", "");
            xmlWriter.WriteValue(deployedAt + state.ResourceMapUri + "#aggregation");
            xmlWriter.WriteEndAttribute();//rdf:resource
            xmlWriter.WriteEndElement(); //ore :describes

            SerializeCreator("describes", xmlWriter);
            SerializeDateModified(xmlWriter);

            xmlWriter.WriteEndElement();//rdf desc

            xmlWriter.WriteStartElement("rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            xmlWriter.WriteStartAttribute("rdf", "about", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            xmlWriter.WriteValue(deployedAt + state.ResourceMapUri + "#aggregation");
            xmlWriter.WriteEndAttribute();
            xmlWriter.WriteStartElement("ore:isDescribedBy", "");
            xmlWriter.WriteStartAttribute("rdf:resource", "");
            xmlWriter.WriteValue(GetURL(deployedAt));
            xmlWriter.WriteEndAttribute();
            xmlWriter.WriteEndElement(); //ore:described by

            if(!string.IsNullOrEmpty(state.ResourceCreator))
                SerializeCreator("describedBy", xmlWriter);
        }

        /// <summary>
        /// Serializes the creator.
        /// </summary>
        /// <param name="mapping">The mapping.</param>
        /// <param name="xmlWriter">The XML writer.</param>
        private void SerializeCreator(string mapping, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("dcterms:creator", "");

            if(mapping == "describes")
            {
                xmlWriter.WriteStartAttribute("rdf:resource", "");
                xmlWriter.WriteValue(state.ResourceMapCreator);
                xmlWriter.WriteEndAttribute(); //rdf resource
            }
            else
            {
                xmlWriter.WriteStartAttribute("rdf", "parseType", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                xmlWriter.WriteValue("Resource");
                xmlWriter.WriteEndAttribute();
                xmlWriter.WriteStartElement("foaf:name", "");
                xmlWriter.WriteValue(state.ResourceCreator);
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();
            return;
        }

        /// <summary>
        /// serializes the date modified
        /// </summary>
        /// <param name="xmlWriter">xml writer</param>
        private void SerializeDateModified(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("dcterms:modified", "");
            xmlWriter.WriteStartAttribute("rdf:datatype", "");
            xmlWriter.WriteValue("http://www.w3.org/2001/XMLSchema#date");
            xmlWriter.WriteEndAttribute();//data type
            xmlWriter.WriteValue(state.ResourceMapDateModified.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            xmlWriter.WriteEndElement(); //modified
            return;
        }

        /// <summary>
        /// writes the xml document headers
        /// </summary>
        /// <param name="xmlWriter">xml writer</param>
        private static void WriteHeaders(XmlTextWriter xmlWriter)
        {
            Namespaces ns = new Namespaces();
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("rdf", "RDF", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");

            xmlWriter.WriteStartAttribute("xmlns:ore");
            xmlWriter.WriteValue("http://www.openarchives.org/ore/terms/");
            xmlWriter.WriteEndAttribute();

            xmlWriter.WriteStartAttribute("xmlns:" + ns._zentityTerms.TermsNamespace);
            xmlWriter.WriteValue(ns._zentityTerms.NamespaceUrl);
            xmlWriter.WriteEndAttribute();

            xmlWriter.WriteStartAttribute("xmlns:" + ns._foafterms.TermsNamespace);
            xmlWriter.WriteValue(ns._foafterms.NamespaceUrl);
            xmlWriter.WriteEndAttribute();

            xmlWriter.WriteStartAttribute("xmlns:" + ns._dcterms.TermsNamespace);
            xmlWriter.WriteValue(ns._dcterms.NamespaceUrl);
            xmlWriter.WriteEndAttribute();

            xmlWriter.WriteStartAttribute("xmlns:" + ns._dciterms.TermsNamespace);
            xmlWriter.WriteValue(ns._dciterms.NamespaceUrl);
            xmlWriter.WriteEndAttribute();

            xmlWriter.WriteStartAttribute("xmlns:" + ns._dcmitypes.TermsNamespace);
            xmlWriter.WriteValue(ns._dcmitypes.NamespaceUrl);
            xmlWriter.WriteEndAttribute();

            return;
        }

        /// <summary>
        /// Determines whether the string has a special relation.
        /// </summary>
        /// <param name="relation">The relation.</param>
        /// <returns>
        /// 	<c>true</c> if the string has a special relation; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsSpecialRelation(string relation)
        {
            return relation.EndsWith("ScholarlyWorkIsCitedBy", StringComparison.OrdinalIgnoreCase) ||
                   relation.EndsWith("ScholarlyWorkHasVersion", StringComparison.OrdinalIgnoreCase) ||
                   relation.EndsWith("ScholarlyWorkHasRepresentation", StringComparison.OrdinalIgnoreCase) ||
                relation.EndsWith("ScholarlyWorkItemIsAddedBy", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// serializes the aggregations
        /// </summary>
        /// <param name="writer">xml writer</param>
        private void SerializeAggregations(XmlWriter writer)
        {
            //<ore:aggregates rdf:resource=resourceId />
            int i = 0;
            foreach(AbstractAggregatedResource<URI> aggregateResource in state.AggreagtedResources)
            {

                writer.WriteStartElement("ore:aggregates");
                writer.WriteStartAttribute("rdf", "resource", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                writer.WriteValue(aggregateResource.ResourceUri.ToString());
                writer.WriteEndAttribute();
                writer.WriteEndElement();
            }

            for(i = 0; i < state.RelationUris.Count(); i++)
            {
                if(IsSpecialRelation(state.RelationUris[i]))
                {
                    continue;
                }
                writer.WriteStartElement("ore:aggregates");
                writer.WriteStartAttribute("rdf", "resource", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                writer.WriteValue(state.ObjectResourceIds[i].ToString());
                writer.WriteEndAttribute();

                writer.WriteEndElement();
            }
            writer.WriteStartElement("rdf:type", "");
            writer.WriteStartAttribute("rdf:resource", "");
            writer.WriteValue(TermsMgr.GetInstance().GetStandardTerms("type:" + state.ResourceType.ToString()));
            writer.WriteEndAttribute();
            writer.WriteEndElement();

        }

        /// <summary>
        /// serializes the metadata
        /// </summary>
        /// <param name="writer">xml writer</param>
        private void SerializeMetadata(XmlWriter writer)
        {
            SerializeTagRelations(writer);
            SerializeResourceRelations(writer);
            SerializeResourceMetaData(writer);
        }

        /// <summary>
        /// serializes resource metadata
        /// </summary>
        /// <param name="writer">xml writer</param>
        private void SerializeResourceMetaData(XmlWriter writer)
        {
            foreach(PropertyInformation info in state.ScalarProperties.Where(info => info.PropertyValue != ""))
            {
                if(info.PropertyName.ToUpperInvariant() == "ID" ||
                    info.PropertyName.ToUpperInvariant() == "TITLE" ||
                    info.PropertyName.ToUpperInvariant() == "IMAGE")
                {
                    continue;
                }
                writer.WriteStartElement(TermsMgr.GetInstance().GetStandardTerms("field:" + info.PropertyName.ToString()));
                writer.WriteStartAttribute("rdf", "datatype", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                info.PropertyType = GetPropertyType(info.PropertyType);
                writer.WriteValue(info.PropertyType);
                writer.WriteEndAttribute();
                writer.WriteValue(info.PropertyValue);
                writer.WriteEndElement();

            }

            PropertyInformation pinfo = state.ScalarProperties.Where(p => p.PropertyName.ToLower() == "title").FirstOrDefault();
            if(pinfo != null)
            {
                writer.WriteStartElement(TermsMgr.GetInstance().GetStandardTerms("field:Title"));
                writer.WriteValue(pinfo.PropertyValue);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();//rdf desc
        }

        /// <summary>
        /// Maps the property type to rdf datatype
        /// </summary>
        /// <param name="propertyType">property type</param>
        /// <returns>value of rdf property</returns>
        private static string GetPropertyType(string propertyType)
        {
            switch(propertyType)
            {
                case "System.Nullable`1[System.DateTime]":
                    propertyType = "xs:dateTime";
                    break;

                case "System.String":
                    propertyType = "xs:string";
                    break;

                case "System.Nullable`1[System.Int32]":
                    propertyType = "xs:integer";
                    break;

                case "System.Nullable`1[System.Int16]":
                    propertyType = "xs:short";
                    break;

                case "System.Nullable`1[System.Int64]":
                    propertyType = "xs:long";
                    break;

                case "System.Nullable`1[System.Decimal]":
                    propertyType = "xs:decimal";
                    break;
                case "System.Nullable`1[System.Boolean]":
                    propertyType = "xs:boolean";
                    break;
                default:
                    break;

            }
            return propertyType;
        }


        /// <summary>
        /// serializes tag and categories
        /// </summary>
        /// <param name="writer">The xml writer.</param>
        private void SerializeTagRelations(XmlWriter writer)
        {
            string stdTerm = TermsMgr.GetInstance().GetStandardTerms("Tag");
            for(int i = 0; i < state.TagNames.Count(); i++)
            {
                writer.WriteStartElement(stdTerm);
                writer.WriteStartAttribute(TermsMgr.GetInstance().GetStandardTerms("field:Title"));
                writer.WriteValue(state.TagNames[i]);
                writer.WriteEndAttribute();
                writer.WriteEndElement();
            }
            stdTerm = TermsMgr.GetInstance().GetStandardTerms("Category");
            for(int i = 0; i < state.CategoryNames.Count(); i++)
            {
                writer.WriteStartElement(stdTerm);
                writer.WriteStartAttribute(TermsMgr.GetInstance().GetStandardTerms("field:Title"));
                writer.WriteValue(state.CategoryNames[i]);
                writer.WriteEndAttribute();
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// serialize resource relations
        /// </summary>
        /// <param name="writer">xml writer</param>
        private void SerializeResourceRelations(XmlWriter writer)
        {
            //TODO :what to do for Custom Predicates 
            for(int i = 0; i < state.RelationUris.Count(); i++)
            {
                if(IsSpecialRelation(state.RelationUris[i]))
                {
                    writer.WriteStartElement(TermsMgr.GetInstance().GetStandardTerms(state.RelationUris[i]));
                    writer.WriteStartAttribute("rdf", "resource", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                    writer.WriteValue(state.ObjectResourceIds[i].ToString());
                    writer.WriteEndAttribute();
                    writer.WriteStartAttribute("rdf", "type", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                    writer.WriteValue(TermsMgr.GetInstance().GetStandardTerms("type:" + state.ObjectTypes[i].ToString()));
                    writer.WriteEndAttribute();
                    writer.WriteEndElement();
                }
            }
        }

        /// <summary>
        /// serialize aggregated by resource
        /// </summary>
        /// <param name="deployedAt">deployed at string</param>
        /// <param name="writer">xml writer</param>
        private void SerializeAggregateByResource(string deployedAt, XmlWriter writer)
        {
            int j = 0;
            int i = 0;

            foreach(AbstractAggregatedResource<URI> aggregateResource in state.AggreagtedResources)
            {
                writer.WriteStartElement("rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                writer.WriteStartAttribute("rdf", "about", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                writer.WriteValue(aggregateResource.ResourceUri.ToString());
                writer.WriteEndAttribute();

                writer.WriteStartElement("rdf:type", "");
                writer.WriteStartAttribute("rdf:resource", "");
                writer.WriteValue(TermsMgr.GetInstance().GetStandardTerms("type:" + state.AggregateTypes[j++].ToString()));
                writer.WriteEndAttribute();
                writer.WriteEndElement(); //rdf:type

                writer.WriteStartElement("ore:isAggregatedBy");
                writer.WriteStartAttribute("rdf", "resource", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                writer.WriteValue(deployedAt + aggregateResource.ResourceUri.ToString() + "#aggregation");
                writer.WriteEndAttribute();
                writer.WriteEndElement(); //aggregated by

                writer.WriteEndElement(); //rdf description

            }
            for(i = 0; i < state.RelationUris.Count(); i++)
            {
                if(IsSpecialRelation(state.RelationUris[i]))
                {
                    continue;
                }
                writer.WriteStartElement("rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                writer.WriteStartAttribute("rdf", "about", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                writer.WriteValue(state.ObjectResourceIds[i].ToString());
                writer.WriteEndAttribute();

                writer.WriteStartElement("rdf:type", "");
                writer.WriteStartAttribute("rdf:resource", "");
                writer.WriteValue(TermsMgr.GetInstance().GetStandardTerms(state.RelationUris[i]));
                writer.WriteEndAttribute();
                writer.WriteEndElement(); //rdf:type

                writer.WriteStartElement("ore:isAggregatedBy");
                writer.WriteStartAttribute("rdf", "resource", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                writer.WriteValue(deployedAt + state.ObjectResourceIds[i].ToString() + "#aggregation");
                writer.WriteEndAttribute();
                writer.WriteEndElement(); //aggregated by

                writer.WriteEndElement();//rdf description
            }
        }

        #endregion
    }
    #endregion
}
