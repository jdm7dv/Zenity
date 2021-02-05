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
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#parseTypeLiteralPropertyElt production.
    /// </summary>
    internal sealed class ProductionParseTypeLiteralPropertyElement : Production
    {
        #region Member Variables

        #region Private
        private EventElement innerElement;
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionParseTypeLiteralPropertyElement class. 
        /// The constructor takes an EventElement to initialize itself.
        /// </summary>
        /// <param name="propertyElement">EventElement object to process.</param>
        internal ProductionParseTypeLiteralPropertyElement(EventElement propertyElement)
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
        /// attributes == set(idAttr?, parseLiteral))
        /// literal
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

            // Validate attributes.
            //attributes == set(idAttr?, parseLiteral))
            Collection<Production> set = new Collection<Production>();
            foreach (EventAttribute attribute in innerElement.Attributes)
            {
                Production production = GetProductionAttribute(attribute);
                if (production != null)
                    set.Add(production);
                else
                    throw new RdfXmlParserException(
                        Constants.ErrorMessageIds.MsgParseTypeLiteralPropertyElementNoOtherAttrs,
                        innerElement.LineInfo);
            }

            if (set.Where(tuple => tuple is ProductionIdAttribute).Count() > 1)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgParseTypeLiteralPropertyElementMoreThanOneIdAttr, innerElement.LineInfo);
            if (set.Where(tuple => tuple is ProductionParseLiteral).Count() != 1)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgParseTypeLiteralPropertyElementSingleParseLiteralAttr, innerElement.LineInfo);

            foreach (Production attribute in set)
            {
                attribute.Match(outputGraph);
            }


            try
            {
                //Validate literal value.
                ProductionLiteral literal = new ProductionLiteral(innerElement.StringValue);
                literal.Match(outputGraph);
            }
            catch (RdfXmlParserException ex)
            {
                throw new RdfXmlParserException(ex.InnerException, ex.ErrorMessageId, innerElement.LineInfo);
            }

            //Generate triples.
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

            //For element e and the literal l that is the rdf:parseType="Literal" content. l is not transformed by the syntax data model mapping into events (as noted in 6 Syntax Data Model) but remains an XML Infoset of XML Information items.
            //l is transformed into the lexical form of an XML literal in the RDF graph x (a Unicode string) by the following algorithm. This does not mandate any implementation method — any other method that gives the same result may be used.
            //Use l to construct an XPath[XPATH] node-set (a document subset) 
            //Apply Exclusive XML Canonicalization [XML-XC14N]) with comments and with empty InclusiveNamespaces PrefixList to this node-set to give a sequence of octets s 
            //This sequence of octets s can be considered to be a UTF-8 encoding of some Unicode string x (sequence of Unicode characters) 
            //The Unicode string x is used as the lexical form of l 
            //This Unicode string x SHOULD be in NFC Normal Form C[NFC] 
            //Then o := typed-literal(literal-value := x, literal-datatype := http://www.w3.org/1999/02/22-rdf-syntax-ns#XMLLiteral ) and the following statement is added to the graph:
            //e.parent.subject.string-value e.URI-string-value o.string-value . 

            //TODO: Check for XML Encoding/Decoding.
            string x = innerElement.StringValue;

            Subject s = innerElement.Parent.Subject;
            RDFUriReference p = innerElement.Uri;
            Node o = new TypedLiteral(x, TypedLiteral.XmlLiteralDataTypeUri);

            AddTriple(outputGraph, s, p, o);

            //If the rdf:ID attribute a is given, the above statement is reified with 
            //i := uri(identifier := resolve(e, concat("#", a.string-value))) 
            //using the reification rules and e.subject := i.
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
        /// <param name="attribute">The Event attribute.</param>
        /// <returns>Returns a production Object.</returns>
        private static Production GetProductionAttribute(EventAttribute attribute)
        {
            if (attribute.Uri == IDUri)
                return new ProductionIdAttribute(attribute);
            else if (attribute.Uri == ParseTypeUri)
                return new ProductionParseLiteral(attribute);

            return null;
        }

        #endregion

        #endregion
    }
}
