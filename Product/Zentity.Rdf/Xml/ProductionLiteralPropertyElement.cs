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
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#literalPropertyElt production.
    /// </summary>
    internal sealed class ProductionLiteralPropertyElement : Production
    {
        #region Member Variables

        #region Private
        private EventElement innerElement;
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionLiteralPropertyElement class. 
        /// The constructor takes an EventElement to initialize itself.
        /// </summary>
        /// <param name="propertyElement">EventElement object to process.</param>
        internal ProductionLiteralPropertyElement(EventElement propertyElement)
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
        /// validates property element based on following syntax rules and create triples, 
        /// <br/>
        /// start-element(URI == propertyElementURIs ),    
        /// attributes == set(idAttr?, datatypeAttr?))
        /// text()
        /// end-element() 
        /// </summary>
        /// <param name="outputGraph">Graph to add the generated triples.</param>
        internal override void Match(Graph outputGraph)
        {
            try
            {
                //Validate uri.
                ProductionPropertyElementUris propertyElementUris =
                    new ProductionPropertyElementUris(innerElement.Uri);
                propertyElementUris.Match(outputGraph);
            }
            catch (RdfXmlParserException ex)
            {
                throw new RdfXmlParserException(ex.ErrorMessageId, innerElement.LineInfo);
            }

            // Validate attributes.
            // attributes == set(idAttr?, datatypeAttr?))
            Collection<Production> set = new Collection<Production>();
            foreach (EventAttribute attribute in innerElement.Attributes)
            {

                Production production = GetProductionAttribute(attribute);
                if (production != null)
                    set.Add(production);
                else
                    throw new RdfXmlParserException(
                        Constants.ErrorMessageIds.MsgLiteralPropertyElementIdOrDataTypeAttr,
                        innerElement.LineInfo);
            }

            if (set.Where(tuple => tuple is ProductionIdAttribute).Count() > 1)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgLiteralPropertyElementMoreThanOneIdAttr, innerElement.LineInfo);

            if (set.Where(tuple => tuple is ProductionDataTypeAttribute).Count() > 1)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgLiteralPropertyElementMoreThanOneDataTypeAttr, innerElement.LineInfo);

            if (innerElement.Children.Count() > 0)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgLiteralPropertyElementNoChildElements, innerElement.LineInfo);

            foreach (Production attribute in set)
            {
                attribute.Match(outputGraph);
            }

            //Generate Triples.
            GenerateTriples(outputGraph);

        }
        #endregion

        #region Private

        /// <summary>
        /// Generates the triples.
        /// </summary>
        /// <param name="outputGraph">The Rdf graph.</param>
        private void GenerateTriples(Graph outputGraph)
        {
            //For element e, and the text event t. The Unicode string t.string-value SHOULD 
            //be in Normal Form C[NFC]. 
            //If the rdf:datatype attribute d is given then o := 
            //typed-literal(literal-value := t.string-value, literal-datatype := d.string-value) 
            //otherwise o := literal(literal-value := t.string-value, literal-language := e.language) 
            //and the following statement is added to the graph:
            //e.parent.subject.string-value e.URI-string-value o.string-value . 
            //  If the rdf:ID attribute a is given, the above statement is reified 
            //with i := uri(identifier := resolve(e, concat("#", a.string-value))) 
            //using the reification rules in section 7.3 and e.subject := i.
            Subject s = innerElement.Parent.Subject;
            RDFUriReference p = innerElement.Uri;
            Node o = null;

            EventAttribute d = innerElement.Attributes.
                Where(tuple => tuple.Uri == DatatypeUri).FirstOrDefault();
            if (d != null)
            {
                o = new TypedLiteral(innerElement.StringValue,
                    Production.Resolve(innerElement, d.StringValue));
            }
            else
            {
                o = new PlainLiteral(innerElement.StringValue, innerElement.Language);
            }

            AddTriple(outputGraph, s, p, o);

            EventAttribute a = innerElement.Attributes.
                Where(tuple => tuple.Uri == IDUri).FirstOrDefault();

            if (a != null)
            {
                RDFUriReference i = Production.Resolve(innerElement, ("#" + a.StringValue));

                Production.Reify(s, p, o, i, outputGraph);
            }
        }

        /// <summary>
        /// Gets the production attribute.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <returns>Returns a production object.</returns>
        private static Production GetProductionAttribute(EventAttribute attribute)
        {
            if (attribute.Uri == IDUri)
                return new ProductionIdAttribute(attribute);
            else if(attribute.Uri == DatatypeUri)
                return new ProductionDataTypeAttribute(attribute);

            return null;
        }

        #endregion

        #endregion
    }
}
