// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Xml
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Zentity.Rdf.Concepts;

    /// <summary>
    /// This class represents the 
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#RDF production.
    /// </summary>
    internal sealed class ProductionRdf : Production
    {
        #region Member Variables

        #region Private
        EventElement innerElement = null;
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionRdf class.
        /// The constructor takes an EventElement to initialize itself.
        /// </summary>
        /// <param name="rdfElement">EventElement object to process</param>
        internal ProductionRdf(EventElement rdfElement)
        {
            if (rdfElement == null)
                throw new ArgumentNullException(
                    string.Format(CultureInfo.InvariantCulture,
                    Resources.MsgNullArgument, "rdfElement"));

            innerElement = rdfElement;
        }
        #endregion

        #endregion

        #region Methods

        #region Internal
        /// <summary>
        /// Matches production,
        /// <br/>
        /// start-element(URI == rdf:RDF, attributes == set())
        ///    nodeElementList
        /// end-element()
        /// </summary>
        /// <param name="outputGraph">Graph to add the generated triples.</param>
        internal override void Match(Graph outputGraph)
        {
            if (innerElement.Uri != RDFUri)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgRdfInvalidRootElement, innerElement.LineInfo);

            if (innerElement.Attributes.Count() != 0)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgRdfNoAttrAllowed, innerElement.LineInfo);

            ProductionNodeElementList nodeElementList =
                new ProductionNodeElementList(innerElement.Children);

            nodeElementList.Match(outputGraph);

        }

        #endregion

        #endregion

    }
}
