// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml.Linq;
    using Zentity.Rdf.Concepts;

    /// <summary>
    /// Represents http://www.w3.org/TR/rdf-syntax-grammar/#doc.
    /// </summary>
    internal sealed class ProductionDoc : Production
    {
        #region Member Variables

        #region Private
        XDocument innerDocument;
        bool isRDFElementPresent = true;
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionDoc class. 
        /// The constructor takes an rdfXmlDocument and isRDFElementPresent flag to initialize itself.
        /// </summary>
        /// <param name="rdfXmlDocument">RDf XML Docment.</param>
        /// <param name="isRDFElementPresent">flag to indicate whether RDF root element is present or not.</param>
        internal ProductionDoc(XDocument rdfXmlDocument, bool isRDFElementPresent)
        {
            if (rdfXmlDocument == null)
                throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, Resources.MsgNullArgument, "rdfXmlDocument"));

            innerDocument = rdfXmlDocument;
            this.isRDFElementPresent = isRDFElementPresent;
        }
        #endregion

        #endregion

        #region Methods

        #region Internal
        /// <summary>
        /// Matches the production.
        /// <br/>
        /// root(document-element == RDF,children == list(RDF)) 
        /// </summary>
        /// <param name="outputGraph">Graph to add the generated triples.</param>
        internal override void Match(Graph outputGraph)
        {
            if (isRDFElementPresent)
            {
                List<EventElement> rootElements = new List<EventElement>();
                if (FindRootRDFElements(innerDocument.Root, rootElements))
                {
                    foreach (EventElement element in rootElements)
                    {
                        ProductionRdf rootProduction = new ProductionRdf(element);
                        rootProduction.Match(outputGraph);
                    }
                }
                else
                {
                    throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgDocNoRDFElement);
                }
            }
            else
            {
                List<EventElement> elementList = new List<EventElement>();
                
                foreach (XElement xmlElement in innerDocument.Elements())
                    elementList.Add(new EventElement(xmlElement));

                ProductionNodeElementList nodeElementList =
                    new ProductionNodeElementList(elementList);

                nodeElementList.Match(outputGraph);
            }
        }
        #endregion

        #region Private

        /// <summary>
        /// Finds all root RDF root elements in the document recursively.
        /// </summary>
        /// <param name="element">Current node element.</param>
        /// <param name="rootElements">List of root RDF elements found.</param>
        /// <returns>true, if one or more root RDF element found, else false.</returns>
        private bool FindRootRDFElements(XElement element, List<EventElement> rootElements)
        {
            
            bool found = false;

            if (RDFUri.InnerUri.AbsoluteUri.Equals(element.Name.NamespaceName + element.Name.LocalName))
            {
                rootElements.Add(new EventElement(element));

                found = true;
            }
            
            if(!found)
            {
                //Find RDF element in child elements
                foreach (XElement childEelement in element.Elements())
                {
                    if(FindRootRDFElements(childEelement, rootElements))
                    {
                        found = true;
                    }
                }
            }

            return found;
        }

        #endregion
        #endregion
    } 
}
