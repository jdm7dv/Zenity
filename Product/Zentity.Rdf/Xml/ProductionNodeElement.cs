// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using Zentity.Rdf.Concepts;

    /// <summary>
    /// This class represents the 
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#nodeElement production.
    /// </summary>
    internal sealed class ProductionNodeElement : Production
    {
        #region Member Variables

        #region Private
        EventElement innerElement;
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        ///  Initializes a new instance of the ProductionNodeElement class. 
        ///  The constructor takes an EventElement to initialize itself.
        /// </summary>
        /// <param name="nodeElement">EventElement object to process.</param>
        internal ProductionNodeElement(EventElement nodeElement)
        {
            if (nodeElement == null)
                throw new ArgumentNullException(
                    string.Format(CultureInfo.InvariantCulture,
                    Resources.MsgNullArgument, "nodeElement"));

            this.innerElement = nodeElement;
        }
        #endregion

        #endregion

        #region Methods

        #region Internal
        /// <summary>
        /// validates node element based on following syntax rules and create triples, 
        /// <br/>
        /// start-element(URI == nodeElementURIs
        /// attributes == set((idAttr | nodeIdAttr | aboutAttr )?, propertyAttr*))
        /// propertyEltList
        /// </summary>
        /// <param name="outputGraph">Graph to add the generated triples.</param>
        internal override void Match(Graph outputGraph)
        {
            try
            {
                // Validate uri.
                ProductionNodeElementUris productionNodeElementUris =
                    new ProductionNodeElementUris(innerElement.Uri);
                productionNodeElementUris.Match(outputGraph);
            }
            catch (RdfXmlParserException ex)
            {
                throw new RdfXmlParserException(ex.ErrorMessageId, innerElement.LineInfo);
            }

            Collection<Production> set = new Collection<Production>();
            foreach (EventAttribute attribute in innerElement.Attributes)
            {
                set.Add(GetProductionAttribute(attribute));
            }

            // Validate attributes.
            // attributes == set((idAttr | nodeIdAttr | aboutAttr )?, propertyAttr*))
            if (set.Where(tuple => tuple is ProductionIdAttribute ||
                tuple is ProductionNodeIdAttribute ||
                tuple is ProductionAboutAttribute).Count() > 1)
            {
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgNodeElementNotComplyToRule, innerElement.LineInfo);
            }

            foreach (Production attribute in set)
            {
                attribute.Match(outputGraph);
            }

            //Generate triples.
            GenerateTriples(outputGraph);

            //* Handle the propertyEltList children events in document order.
            ProductionPropertyElementList propertyElementList =
                new ProductionPropertyElementList(innerElement.Children);
            propertyElementList.Match(outputGraph);

        }
        #endregion

        #region Private

        /// <summary>
        /// Generates the triples.
        /// </summary>
        /// <param name="outputGraph">The Rdf graph.</param>
        private void GenerateTriples(Graph outputGraph)
        {
            //If e.subject is empty, then e.subject := bnodeid(identifier := generated-blank-node-id()).
            //The following can then be performed in any order:
            //    * If e.URI != rdf:Description then the following statement is added to the graph:
            //      e.subject.string-value <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> e.URI-string-value .
            //    * If there is an attribute a in propertyAttr with a.URI == rdf:type then u:=uri(identifier:=resolve(a.string-value)) and the following tiple is added to the graph:
            //      e.subject.string-value <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> u.string-value .
            //    * For each attribute a matching propertyAttr (and not rdf:type), the Unicode string a.string-value SHOULD be in Normal Form C[NFC], o := literal(literal-value := a.string-value, literal-language := e.language) and the following statement is added to the graph:
            //      e.subject.string-value a.URI-string-value o.string-value .

            Subject subject = GetSubject();

            if (subject != null)
                innerElement.Subject = subject;

            if (innerElement.Subject == null)
            {
                innerElement.Subject = new BlankNode();
            }

            if (innerElement.Uri != DescriptionUri)
                AddTriple(outputGraph, innerElement.Subject, TypeUri, innerElement.Uri);

            //propertyAttr = anyURI - ( coreSyntaxTerms | rdf:Description | rdf:li | oldTerms ) 
            IEnumerable<EventAttribute> propertyAttr =
                innerElement.Attributes.Where(tuple => tuple.Uri != DescriptionUri
                    && tuple.Uri != LiUri
                    && Production.CoreSyntaxTerms.Where(attr => attr == tuple.Uri).Count() == 0
                    && Production.OldTerms.Where(attr => attr == tuple.Uri).Count() == 0);

            EventAttribute a = propertyAttr.
                Where(tuple => tuple.Uri == TypeUri).FirstOrDefault();
            if (a != null)
            {
                RDFUriReference u = Production.Resolve(innerElement, a.StringValue);
                AddTriple(outputGraph, innerElement.Subject, TypeUri, u);
            }

            foreach (EventAttribute attr in
                propertyAttr.Where(tuple => tuple.Uri != TypeUri))
            {
                PlainLiteral o = new PlainLiteral(attr.StringValue, innerElement.Language);
                AddTriple(outputGraph, innerElement.Subject, attr.Uri, o);
            }
        }

        /// <summary>
        /// Gets the production attribute.
        /// </summary>
        /// <param name="attribute">The Event attribute.</param>
        /// <returns>Returns a production object.</returns>
        private static Production GetProductionAttribute(EventAttribute attribute)
        {
            if (attribute.Uri == IDUri)
                return new ProductionIdAttribute(attribute);
            else if (attribute.Uri == NodeIDUri)
                return new ProductionNodeIdAttribute(attribute);
            else if (attribute.Uri == AboutUri)
                return new ProductionAboutAttribute(attribute);
            else
                return new ProductionPropertyAttribute(attribute);
        }

        /// <summary>
        /// Gets the subject.
        /// </summary>
        /// <returns>Returns a subject object.</returns>
        private Subject GetSubject()
        {
            //For node element e, the processing of some of the attributes has to be done before other work such as dealing with children events or other attributes. These can be processed in any order:
            //    * If there is an attribute a with a.URI == rdf:ID, then e.subject := uri(identifier := resolve(e, concat("#", a.string-value))).
            //    * If there is an attribute a with a.URI == rdf:nodeID, then e.subject := bnodeid(identifier:=a.string-value).
            //    * If there is an attribute a with a.URI == rdf:about then e.subject := uri(identifier := resolve(e, a.string-value)).

            EventAttribute idAttribute = innerElement.Attributes.
                Where(tuple => tuple.Uri == IDUri).FirstOrDefault();
            if (idAttribute != null)
                return Production.Resolve(innerElement, "#" + idAttribute.StringValue);

            EventAttribute nodeIdAttribute = innerElement.Attributes.
                Where(tuple => tuple.Uri == NodeIDUri).FirstOrDefault();
            if (nodeIdAttribute != null)
            {
                return new BlankNode(nodeIdAttribute.StringValue);
            }

            EventAttribute aboutAttribute = innerElement.Attributes.
                Where(tuple => tuple.Uri == AboutUri).FirstOrDefault();
            if (aboutAttribute != null)
                return Production.Resolve(innerElement, aboutAttribute.StringValue);

            return null;
        }

        #endregion

        #endregion

    }
}
