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
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#emptyPropertyElt production.
    /// </summary>
    internal sealed class ProductionEmptyPropertyElement : Production
    {
        #region Member Variables

        #region Private
        private EventElement innerElement;
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionEmptyPropertyElement class. 
        /// The constructor takes an EventElement to initialize itself.
        /// </summary>
        /// <param name="propertyElement">EventElement object.</param>
        internal ProductionEmptyPropertyElement(EventElement propertyElement)
        {
            if (propertyElement == null)
                throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, Resources.MsgNullArgument, "propertyElement"));

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
        /// attributes == set(idAttr?, ( resourceAttr | nodeIdAttr )?, propertyAttr*))
        /// end-element() 
        /// </summary>
        /// <param name="outputGraph">Graph to add the generated triples.</param>
        internal override void Match(Graph outputGraph)
        {
            try
            {
                //start-element(URI == propertyElementURIs ),
                ProductionPropertyElementUris propertyElementUris =
                    new ProductionPropertyElementUris(innerElement.Uri);
                propertyElementUris.Match(outputGraph);
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
            //attributes == set(idAttr?, ( resourceAttr | nodeIdAttr )?, propertyAttr*))
            if (set.Where(tuple => tuple is ProductionIdAttribute).Count() > 1)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgEmptyPropertyElementMoreThanOneIDAttr, innerElement.LineInfo);

            if (set.Where(tuple => tuple is ProductionResourceAttribute
                || tuple is ProductionNodeIdAttribute).Count() > 1)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgEmptyPropertyElementNotComplyToRule, innerElement.LineInfo);

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
            //If there are no attributes or only the optional rdf:ID attribute i then 
            //o := literal(literal-value:="", literal-language := e.language) and 
            //the following statement is added to the graph:
            //e.parent.subject.string-value e.URI-string-value o.string-value . 
            EventAttribute i = innerElement.Attributes.
            Where(tuple => tuple.Uri == IDUri).FirstOrDefault();

            if (innerElement.Attributes.Count() == 0 ||
                (innerElement.Attributes.Count() == 1 && i != null))
            {
                Subject s = innerElement.Parent.Subject;
                RDFUriReference p = innerElement.Uri;
                Node o = new PlainLiteral(string.Empty, innerElement.Language);

                AddTriple(outputGraph, s, p, o);

                //and then if i is given, the above statement is reified with 
                //uri(identifier := resolve(e, concat("#", i.string-value))) 
                //using the reification rules 
                if (i != null)
                {
                    RDFUriReference elementUri = Production.Resolve(innerElement, ("#" + i.StringValue));
                    Production.Reify(s, p, o, elementUri, outputGraph);
                }
            }
            else
            {
                Subject r = GetSubject();

                //For all propertyAttr attributes a (in any order)
                //If a.URI == rdf:type then u:=uri(identifier:=resolve(a.string-value)) and the following triple is added to the graph:
                //r.string-value <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> u.string-value . 
                //Otherwise Unicode string a.string-value SHOULD be in Normal Form C[NFC], 
                //o := literal(literal-value := a.string-value, literal-language := e.language) and the following statement is added to the graph:
                //r.string-value a.URI-string-value o.string-value .

                //propertyAttr = anyURI - ( coreSyntaxTerms | rdf:Description | rdf:li | oldTerms ) 
                IEnumerable<EventAttribute> propertyAttr = innerElement.Attributes.
                Where(tuple => tuple.Uri != DescriptionUri
                    && tuple.Uri != LiUri
                    && (Production.CoreSyntaxTerms.Where(attr => attr == tuple.Uri).Count() == 0)
                    && (Production.OldTerms.Where(attr => attr == tuple.Uri).Count() == 0));

                foreach (EventAttribute a in propertyAttr)
                {
                    if (a.Uri == TypeUri)
                    {
                        RDFUriReference u = Production.Resolve(innerElement, a.StringValue);
                        AddTriple(outputGraph, r, TypeUri, u);
                    }
                    //Attributes except rdf:resource and rdf:nodeID
                    else if (!(a.Uri == ResourceUri || a.Uri == NodeIDUri))
                    {
                        Node literal = new PlainLiteral(a.StringValue, innerElement.Language);
                        AddTriple(outputGraph, r, a.Uri, literal);
                    }
                }


                //Add the following statement to the graph:
                //e.parent.subject.string-value e.URI-string-value r.string-value . 
                //and then if rdf:ID attribute i is given, the above statement is reified with uri(identifier := resolve(e, concat("#", i.string-value))) using the reification rules 
                Subject s = innerElement.Parent.Subject;
                RDFUriReference p = innerElement.Uri;
                Node o = r;
                AddTriple(outputGraph, s, p, o);

                if (i != null)
                {
                    RDFUriReference elementUri = Production.Resolve(innerElement, ("#" + i.StringValue));
                    Production.Reify(s, p, o, elementUri, outputGraph);
                }
            }
        }

        /// <summary>
        /// Gets the subject.
        /// </summary>
        /// <returns>Returns a node which is a subject of a triple.s</returns>
        private Subject GetSubject()
        {
            //If rdf:resource attribute i is present, then r := uri(identifier := resolve(e, i.string-value)) 
            //If rdf:nodeID attribute i is present, then r := bnodeid(identifier := i.string-value) 
            //If neither, r := bnodeid(identifier := generated-blank-node-id()) 
            Subject r = null;
            EventAttribute resourceAttr = innerElement.Attributes.
            Where(tuple => tuple.Uri == ResourceUri).FirstOrDefault();
            if (resourceAttr != null)
            {
                r = Production.Resolve(innerElement, resourceAttr.StringValue);
            }

            if (r == null)
            {
                EventAttribute nodeIDAttr = innerElement.Attributes.
                   Where(tuple => tuple.Uri == NodeIDUri).FirstOrDefault();
                if (nodeIDAttr != null)
                {
                    r = new BlankNode(nodeIDAttr.StringValue);
                }
            }

            if (r == null)
            {
                r = new BlankNode();
            }
            return r;
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
            else if (attribute.Uri == ResourceUri)
                return new ProductionResourceAttribute(attribute);
            else if (attribute.Uri == NodeIDUri)
                return new ProductionNodeIdAttribute(attribute);
            else
                return new ProductionPropertyAttribute(attribute);
        }

        #endregion

        #endregion



        

    }
}
