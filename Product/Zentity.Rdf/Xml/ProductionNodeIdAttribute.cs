// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Xml
{
    using System;
    using System.Globalization;
    using Zentity.Rdf.Concepts;

    /// <summary>
    /// This class represents the 
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#nodeIdAttr production.
    /// </summary>
    internal sealed class ProductionNodeIdAttribute : Production
    {
        #region Member Variables

        #region Private
        EventAttribute innerAttribute;
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionNodeIdAttribute class. 
        /// The constructor takes an EventAttribute to initialize itself.
        /// </summary>
        /// <param name="attribute">EventAttribute object to process.</param>
        internal ProductionNodeIdAttribute(EventAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(
                    string.Format(CultureInfo.InvariantCulture,
                    Resources.MsgNullArgument, "attribute"));

            this.innerAttribute = attribute;
        }
        #endregion

        #endregion

        #region Methods

        #region Internal
        /// <summary>
        /// Validates nodeID attribute based on following syntax rules, 
        /// <br/>
        /// attribute(URI == rdf:nodeID, string-value == rdf-id) 
        /// </summary>
        /// <param name="outputGraph">Graph to add the generated triples.</param>
        internal override void Match(Graph outputGraph)
        {
            if (innerAttribute.Uri != NodeIDUri)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgNodeIdAttributeUriNotMatch,innerAttribute.LineInfo);

            try
            {
                ProductionRdfId rdfId = new ProductionRdfId(innerAttribute.StringValue);
                rdfId.Match(outputGraph);
            }
            catch (RdfXmlParserException ex)
            {
                throw new RdfXmlParserException(ex.InnerException, ex.ErrorMessageId, innerAttribute.LineInfo);
            }
        }
        #endregion

        #endregion
    }
}
