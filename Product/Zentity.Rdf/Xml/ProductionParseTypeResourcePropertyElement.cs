// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Xml
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using Zentity.Rdf.Concepts;

    /// <summary>
    /// This class represents the 
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#parseTypeResourcePropertyElt production.
    /// </summary>
    internal sealed class ProductionParseTypeResourcePropertyElement : Production
    {
        #region Member Variables

        #region Private
        private EventElement innerElement;
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionParseTypeResourcePropertyElement class. 
        /// The constructor takes an EventElement to initialize itself.
        /// </summary>
        /// <param name="propertyElement">EventElement object to process.</param>
        internal ProductionParseTypeResourcePropertyElement(EventElement propertyElement)
        {
            if (propertyElement == null)
                throw new ArgumentNullException(
                    string.Format(CultureInfo.InvariantCulture,
                    Resources.MsgNullArgument, "propertyElement"));

            innerElement = propertyElement;
        }
        #endregion

        #endregion

        #region Methods

        #region Internal
        /// <summary>
        /// Validates property element based on following syntax rules and creates triples, 
        /// <br/>
        /// start-element(URI == propertyElementURIs ),
        /// attributes == set(idAttr?, parseResource))
        /// propertyEltList
        /// end-element() 
        /// </summary>
        /// <param name="outputGraph">Graph to add the generated triples.</param>
        internal override void Match(Graph outputGraph)
        {
            try
            {
                ProductionPropertyElementUris propertyElementUris =
                    new ProductionPropertyElementUris(innerElement.Uri);
                propertyElementUris.Match(outputGraph);
            }
            catch (RdfXmlParserException ex)
            {
                throw new RdfXmlParserException(ex.ErrorMessageId, innerElement.LineInfo);
            }

            // Validate attributes.
            //attributes == set(idAttr?, parseResource))
            Collection<Production> set = new Collection<Production>();
            foreach (EventAttribute attribute in innerElement.Attributes)
            {
                Production production = GetProductionAttribute(attribute);
                if (production != null)
                    set.Add(production);
                else
                    throw new RdfXmlParserException(
                        Constants.ErrorMessageIds.MsgParseTypeResourcePropertyElementNoOtherAttrs,
                        innerElement.LineInfo);
            }

            if (set.Where(tuple => tuple is ProductionIdAttribute).Count() > 1)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgParseTypeResourcePropertyElementMoreThanOneIdAttr, innerElement.LineInfo);
            if (set.Where(tuple => tuple is ProductionParseResource).Count() != 1)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgParseTypeResourcePropertyElementSingleParseResourceAttr, innerElement.LineInfo);

            foreach (Production attribute in set)
            {
                attribute.Match(outputGraph);
            }

            //Generate Triples.
            BlankNode n = GenerateTriples(outputGraph);

            //If the element content c is not empty, then use event 
            //n to create a new sequence of events as follows:
            //start-element(URI := rdf:Description,
            //subject := n,
            //attributes := set())
            //c
            //end-element() 
            //Then process the resulting sequence using production nodeElement.
            if (innerElement.Children.Count() > 0)
            {
                EventElement element = new EventElement(Constants.Description,
                        Constants.RdfNamespace, innerElement.Children);
                element.Subject = n;

                ProductionNodeElement nodeElement = new ProductionNodeElement(element);
                nodeElement.Match(outputGraph);
            }
        }
        #endregion

        #region Private

        /// <summary>
        /// Generates the triples.
        /// </summary>
        /// <param name="outputGraph">The Rdf graph.</param>
        /// <returns>REturns a Blank Node.</returns>
        private BlankNode GenerateTriples(Graph outputGraph)
        {
            //For element e with possibly empty element content c.
            //n := bnodeid(identifier := generated-blank-node-id()).
            //Add the following statement to the graph: 
            //e.parent.subject.string-value  e.URI-string-value   n.string-value . 
            BlankNode n = new BlankNode();
            Subject s = innerElement.Parent.Subject;
            RDFUriReference p = innerElement.Uri;
            AddTriple(outputGraph, s, p, n);

            //If the rdf:ID attribute a is given, the statement above is reified 
            //with i := uri(identifier := resolve(e, concat("#", a.string-value))) 
            //using the reification rules and e.subject := i.
            EventAttribute a = innerElement.Attributes.
               Where(tuple => tuple.Uri == IDUri).FirstOrDefault();

            if (a != null)
            {
                RDFUriReference i = Production.Resolve(innerElement, ("#" + a.StringValue));
                Production.Reify(s, p, n, i, outputGraph);
            }

            return n;

        }

        /// <summary>
        /// Gets the production attribute.
        /// </summary>
        /// <param name="attribute">The Event attribute.</param>
        /// <returns>Returns a Production Object.</returns>
        private static Production GetProductionAttribute(EventAttribute attribute)
        {
            if (attribute.Uri == IDUri)
                return new ProductionIdAttribute(attribute);
            else if (attribute.Uri == ParseTypeUri)
                return new ProductionParseResource(attribute);

            return null;
        }

        #endregion

        #endregion
    }
}
