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
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#resourcePropertyElt production.
    /// </summary>
    internal sealed class ProductionResourcePropertyElement : Production
    {
        #region Member Variables

        #region Private
        private EventElement innerElement;
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionResourcePropertyElement class.
        /// The constructor takes an EventElement to initialize itself.
        /// </summary>
        /// <param name="propertyElement">EventElement object to process.</param>
        internal ProductionResourcePropertyElement(EventElement propertyElement)
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
        /// validates resource property element based on following syntax rules and creates triples, 
        /// <br/>
        /// start-element(URI == propertyElementURIs ),
        /// attributes == set(idAttr?))
        /// ws* nodeElement ws*
        /// end-element() 
        /// </summary>
        /// <param name="outputGraph">Graph to add the generated triples.</param>
        internal override void Match(Graph outputGraph)
        {
            try
            {
                //Validate uri.
                //start-element(URI == propertyElementURIs )
                ProductionPropertyElementUris propertyElementUris =
                    new ProductionPropertyElementUris(innerElement.Uri);
                propertyElementUris.Match(outputGraph);
            }
            catch (RdfXmlParserException ex)
            {
                throw new RdfXmlParserException(ex.ErrorMessageId, innerElement.LineInfo);
            }

            //Validate attributes.
            Collection<Production> idAttrSet = new Collection<Production>();
            foreach (EventAttribute attribute in innerElement.Attributes)
            {
                if (attribute.Uri == IDUri)
                    idAttrSet.Add(new ProductionIdAttribute(attribute));
                else
                    throw new RdfXmlParserException(
                        Constants.ErrorMessageIds.MsgResourcePropertyElementNoOtherAttr,
                        innerElement.LineInfo);
            }

            //attributes == set(idAttr?))
            if (idAttrSet.Count > 1)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgResourcePropertyElementMoreThanOneIdAttr, innerElement.LineInfo);

            //ws* nodeElement ws*
            if (innerElement.Children.Count() != 1)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgResourcePropertyElementSingleChildElement, innerElement.LineInfo);

            foreach (Production attribute in idAttrSet)
            {
                attribute.Match(outputGraph);
            }

            //Generate Triples;
            GenerateTriples(outputGraph, idAttrSet.Count);

        }
        #endregion

        #region Private
        /// <summary>
        /// Generates the triples.
        /// </summary>
        /// <param name="outputGraph">The output graph.</param>
        /// <param name="idAttributeCount">The attribute count.</param>
        private void GenerateTriples(Graph outputGraph, int idAttributeCount)
        {

            //For element e, and the single contained nodeElement n, first n 
            //must be processed using production nodeElement. Then the following 
            //statement is added to the graph:
            //e.parent.subject.string-value e.URI-string-value n.subject.string-value . 
            EventElement n = innerElement.Children.First();
            ProductionNodeElement nodeElement =
                new ProductionNodeElement(n);
            nodeElement.Match(outputGraph);

            Subject subjectUri = innerElement.Parent.Subject;
            RDFUriReference predicateUri = innerElement.Uri;
            Node objectUri = n.Subject;
            AddTriple(outputGraph, subjectUri, predicateUri, objectUri);

            //If the rdf:ID attribute a is given, the above statement is reified with 
            //i := uri(identifier := resolve(e, concat("#", a.string-value))) 
            //using the reification rules and e.subject := i
            if (idAttributeCount == 1)
            {
                EventAttribute idAttr = innerElement.Attributes.First();
                RDFUriReference elementUri = Production.Resolve(innerElement, ("#" + idAttr.StringValue));
                Production.Reify(subjectUri, predicateUri, objectUri, elementUri, outputGraph);
            }
        }
        #endregion

        #endregion
    }
}
