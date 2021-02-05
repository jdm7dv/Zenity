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
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#idAttr production.
    /// </summary>
    internal sealed class ProductionIdAttribute : Production
    {
        #region Member Variables

        #region Private
        EventAttribute innerAttribute;
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionIdAttribute class. 
        /// The constructor takes an EventAttribute to initialize itself.
        /// </summary>
        /// <param name="attribute">EventAttribute object.</param>
        internal ProductionIdAttribute(EventAttribute attribute)
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
        /// Validates Id attribute based on following syntax rules, 
        /// <br/>
        /// attribute(URI == rdf:ID, string-value == rdf-id)
        /// </summary>
        /// <param name="outputGraph">Graph to add the generated triples.</param>
        internal override void Match(Graph outputGraph)
        {
            if (innerAttribute.Uri != IDUri)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgIdAttributeUriNotMatch, innerAttribute.LineInfo);

            try
            {
                ProductionRdfId rdfId = new ProductionRdfId(innerAttribute.StringValue);
                rdfId.Match(outputGraph);
            }
            catch (RdfXmlParserException ex)
            {
                throw new RdfXmlParserException(ex.InnerException, ex.ErrorMessageId, innerAttribute.LineInfo);
            }

            //Constraint:: constraint-id applies to the values of rdf:ID attributes
            //Each application of production idAttr matches an attribute. The pair formed by 
            //the ·string-value· accessor of the matched attribute and the ·base-uri· 
            //accessor of the matched attribute is unique within a single RDF/XML document.
            if (IsIdAlreadyDefined(outputGraph))
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgIdAttributeIdDefinedMoreThanOnce, innerAttribute.LineInfo);
        }
        #endregion

        #region Private
        /// <summary>
        /// Determines whether id is already defined in specified output graph.
        /// </summary>
        /// <param name="outputGraph">The output graph.</param>
        /// <returns>
        /// 	<c>true</c> if id is already defined in the specified output graph; otherwise, <c>false</c>.
        /// </returns>
        private bool IsIdAlreadyDefined(Graph outputGraph)
        {
            //TODO: Revisit the logic
            RDFUriReference idUri = Production.Resolve(innerAttribute.Parent, "#" + innerAttribute.StringValue);

            if (outputGraph.Where(tuple => (tuple.TripleSubject as RDFUriReference) == idUri).Count() > 0)
                return true;
            else
                return false;
        }
        #endregion

        #endregion
    }
}
