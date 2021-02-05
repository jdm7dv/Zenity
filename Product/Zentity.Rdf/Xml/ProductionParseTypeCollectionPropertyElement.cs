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
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#parseTypeCollectionPropertyElt production.
    /// </summary>
    internal sealed class ProductionParseTypeCollectionPropertyElement : Production
    {
        #region Member Variables

        #region Private
        private EventElement innerElement;
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionParseTypeCollectionPropertyElement class. 
        /// The constructor takes an EventElement to initialize itself.
        /// </summary>
        /// <param name="propertyElement">EventElement object to process.</param>
        internal ProductionParseTypeCollectionPropertyElement(EventElement propertyElement)
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
        /// attributes == set(idAttr?, parseCollection))
        /// nodeElementList
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
            // attributes == set(idAttr?, parseCollection))
            Collection<Production> set = new Collection<Production>();
            foreach (EventAttribute attribute in innerElement.Attributes)
            {
                Production production = GetProductionAttribute(attribute);
                if (production != null)
                    set.Add(production);
                else
                    throw new RdfXmlParserException(
                        Constants.ErrorMessageIds.MsgParseTypeCollectionPropertyElementNoOtherAttrs,
                        innerElement.LineInfo);
            }
            if (set.Where(tuple => tuple is ProductionIdAttribute).Count() > 1)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgParseTypeCollectionPropertyElementMoreThanOneIdAttr, innerElement.LineInfo);
            if (set.Where(tuple => tuple is ProductionParseCollection).Count() != 1)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgParseTypeCollectionPropertyElementSingleCollectionAttr, innerElement.LineInfo);

            foreach (Production attribute in set)
            {
                attribute.Match(outputGraph);
            }

            ProductionNodeElementList nodeElementList = new ProductionNodeElementList(innerElement.Children);
            nodeElementList.Match(outputGraph);

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
            //For element event e with possibly empty nodeElementList l. Set s:=list().
            //For each element event f in l, n := bnodeid(identifier := generated-blank-node-id()) and append n to s to give a sequence of events.
            //If s is not empty, n is the first event identifier in s and the following statement is added to the graph:
            //e.parent.subject.string-value e.URI-string-value n.string-value . 
            //otherwise the following statement is added to the graph:
            //e.parent.subject.string-value e.URI-string-value <http://www.w3.org/1999/02/22-rdf-syntax-ns#nil> . 
            List<Subject> s = GetBlankNodeList();
            Subject subject = innerElement.Parent.Subject;
            RDFUriReference predicate = innerElement.Uri;
            Node o = s.Count == 0 ? NilUri : s[0];

            AddTriple(outputGraph, subject, predicate, o);

            //If the rdf:ID attribute a is given, either of the the above statements is 
            //reified with i := uri(identifier := resolve(e, concat("#", a.string-value))) 
            //using the reification rules
            EventAttribute a = innerElement.Attributes.
               Where(tuple => tuple.Uri == IDUri).FirstOrDefault();
            if (a != null)
            {
                RDFUriReference i = Production.Resolve(innerElement, ("#" + a.StringValue));
                Production.Reify(subject, predicate, o, i, outputGraph);
            }


            //For each event n in s and the corresponding element event f in l, the following statement is added to the graph:
            //n.string-value <http://www.w3.org/1999/02/22-rdf-syntax-ns#first> f.string-value . 
            //For each consecutive and overlapping pair of events (n, o) in s, the following statement is added to the graph:
            //n.string-value <http://www.w3.org/1999/02/22-rdf-syntax-ns#rest> o.string-value . 
            //If s is not empty, n is the last event identifier in s, the following statement is added to the graph:
            //n.string-value <http://www.w3.org/1999/02/22-rdf-syntax-ns#rest> <http://www.w3.org/1999/02/22-rdf-syntax-ns#nil> . 
            int index = 0;
            foreach (EventElement f in innerElement.Children)
            {
                AddTriple(outputGraph, s[index], FirstUri, f.Subject);

                //If next element is available
                if (index < (s.Count - 1))
                    AddTriple(outputGraph, s[index], RestUri, s[index + 1]);
                else
                    AddTriple(outputGraph, s[index], RestUri, NilUri);

                index++;
            }
        }

        /// <summary>
        /// Gets the blank node list.
        /// </summary>
        /// <returns>Returns a list of Subject objects.</returns>
        private List<Subject> GetBlankNodeList()
        {
            List<Subject> s = new List<Subject>();

            for(int i=0; i < innerElement.Children.Count(); i++)
            {
                s.Add(new BlankNode());
            }

            return s;
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
            else if(attribute.Uri == ParseTypeUri)
                return new ProductionParseCollection(attribute);

            return null;
        }
        #endregion

        #endregion
    }
}
