// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Zentity.Rdf.Concepts;

    /// <summary>
    /// This class defines an abstract base class for all Production classes.
    /// </summary>
    internal abstract class Production
    {
        #region Member Variables

        #region Private Static

        static RDFUriReference rdfUri = new RDFUriReference(Constants.RdfNamespace + Constants.RDF);
        static RDFUriReference idUri = new RDFUriReference(Constants.RdfNamespace + Constants.ID);
        static RDFUriReference resourceUri = new RDFUriReference(Constants.RdfNamespace + Constants.Resource);
        static RDFUriReference nodeIDUri = new RDFUriReference(Constants.RdfNamespace + Constants.NodeID);
        static RDFUriReference aboutUri = new RDFUriReference(Constants.RdfNamespace + Constants.About);
        static RDFUriReference descriptionUri = new RDFUriReference(Constants.RdfNamespace + Constants.Description);
        static RDFUriReference liUri = new RDFUriReference(Constants.RdfNamespace + Constants.Li);
        static RDFUriReference typeUri = new RDFUriReference(Constants.RdfNamespace + Constants.Type);
        static RDFUriReference parseTypeUri = new RDFUriReference(Constants.RdfNamespace + Constants.ParseType);
        static RDFUriReference datatypeUri = new RDFUriReference(Constants.RdfNamespace + Constants.Datatype);
        static RDFUriReference firstUri = new RDFUriReference(Constants.RdfNamespace + Constants.First);
        static RDFUriReference restUri = new RDFUriReference(Constants.RdfNamespace + Constants.Rest);
        static RDFUriReference nilUri = new RDFUriReference(Constants.RdfNamespace + Constants.Nil);
        static RDFUriReference statementUri = new RDFUriReference(Constants.RdfNamespace + Constants.Statement);
        static RDFUriReference subjectUri = new RDFUriReference(Constants.RdfNamespace + Constants.Subject);
        static RDFUriReference predicateUri = new RDFUriReference(Constants.RdfNamespace + Constants.Predicate);
        static RDFUriReference objectUri = new RDFUriReference(Constants.RdfNamespace + Constants.Object);
        static RDFUriReference aboutEachUri = new RDFUriReference(Constants.RdfNamespace + Constants.AboutEach);
        static RDFUriReference aboutEachPrefixUri = new RDFUriReference(Constants.RdfNamespace + Constants.AboutEachPrefix);
        static RDFUriReference bagIDUri = new RDFUriReference(Constants.RdfNamespace + Constants.BagID);

        static RDFUriReference propertyUri = new RDFUriReference(Constants.RdfNamespace + Constants.Property);
        static RDFUriReference listUri = new RDFUriReference(Constants.RdfNamespace + Constants.List);
        static RDFUriReference bagUri = new RDFUriReference(Constants.RdfNamespace + Constants.Bag);
        static RDFUriReference seqUri = new RDFUriReference(Constants.RdfNamespace + Constants.Seq);
        static RDFUriReference altUri = new RDFUriReference(Constants.RdfNamespace + Constants.Alt);
        static RDFUriReference valueUri = new RDFUriReference(Constants.RdfNamespace + Constants.Value);
        static RDFUriReference xmlLiteralUri = new RDFUriReference(Constants.RdfNamespace + Constants.XMLLiteral);

        static IEnumerable<RDFUriReference> coreSyntaxTerms;
        static IEnumerable<RDFUriReference> oldTerms;
        static IEnumerable<RDFUriReference> allTerms;
        #endregion

        #endregion

        #region Properties

        #region Internal Static

        /// <summary>
        /// Gets core syntax terms.
        /// </summary>
        internal static IEnumerable<RDFUriReference> CoreSyntaxTerms
        {
            get
            {
                if (coreSyntaxTerms == null)
                    coreSyntaxTerms = GetCoreSyntaxTerms();
                return coreSyntaxTerms;
            }
        }

        /// <summary>
        /// Gets old terms.
        /// </summary>
        internal static IEnumerable<RDFUriReference> OldTerms
        {
            get
            {
                if (oldTerms == null)
                    oldTerms = GetOldTerms();
                return oldTerms;
            }
        }

        /// <summary>
        /// Gets all RDF terms.
        /// </summary>
        internal static IEnumerable<RDFUriReference> AllRdfTerms
        {
            get
            {
                if (allTerms == null)
                    allTerms = GetAllRdfTerms();
                return allTerms;
            }
        }

        /// <summary>
        /// Gets rdf:RDF uri reference.
        /// </summary>
        internal static RDFUriReference RDFUri
        {
            get { return rdfUri; }
        }

        /// <summary>
        /// Gets rdf:ID uri reference.
        /// </summary>
        internal static RDFUriReference IDUri
        {
            get { return idUri; }
        }

        /// <summary>
        /// Gets rdf:resource uri reference.
        /// </summary>
        internal static RDFUriReference ResourceUri
        {
            get { return resourceUri; }
        }

        /// <summary>
        /// Gets rdf:nodeID uri reference.
        /// </summary>
        internal static RDFUriReference NodeIDUri
        {
            get { return nodeIDUri; }
        }

        /// <summary>
        /// Gets rdf:about uri reference.
        /// </summary>
        internal static RDFUriReference AboutUri
        {
            get { return aboutUri; }
        }

        /// <summary>
        /// Gets rdf:Description uri reference.
        /// </summary>
        internal static RDFUriReference DescriptionUri
        {
            get { return descriptionUri; }
        }

        /// <summary>
        /// Gets rdf:li uri reference.
        /// </summary>
        internal static RDFUriReference LiUri
        {
            get { return liUri; }
        }

        /// <summary>
        /// Gets rdf:type uri reference.
        /// </summary>
        internal static RDFUriReference TypeUri
        {
            get { return typeUri; }
        }

        /// <summary>
        /// Gets rdf:parseType uri reference.
        /// </summary>
        internal static RDFUriReference ParseTypeUri
        {
            get { return parseTypeUri; }
        }

        /// <summary>
        /// Gets rdf:datatype uri reference.
        /// </summary>
        internal static RDFUriReference DatatypeUri
        {
            get { return datatypeUri; }
        }

        /// <summary>
        /// Gets rdf:first uri reference.
        /// </summary>
        internal static RDFUriReference FirstUri
        {
            get { return firstUri; }
        }

        /// <summary>
        /// Gets rdf:rest uri reference.
        /// </summary>
        internal static RDFUriReference RestUri
        {
            get { return restUri; }
        }

        /// <summary>
        /// Gets rdf:nil uri reference.
        /// </summary>
        internal static RDFUriReference NilUri
        {
            get { return nilUri; }
        }

        /// <summary>
        /// Gets rdf:Statement uri reference.
        /// </summary>
        internal static RDFUriReference StatementUri
        {
            get { return statementUri; }
        }

        /// <summary>
        /// Gets rdf:subject uri reference.
        /// </summary>
        internal static RDFUriReference SubjectUri
        {
            get { return subjectUri; }
        }

        /// <summary>
        /// Gets rdf:predicate uri reference.
        /// </summary>
        internal static RDFUriReference PredicateUri
        {
            get { return predicateUri; }
        }

        /// <summary>
        /// Gets rdf:object uri reference.
        /// </summary>
        internal static RDFUriReference ObjectUri
        {
            get { return objectUri; }
        }

        /// <summary>
        /// Gets rdf:aboutEach uri reference.
        /// </summary>
        internal static RDFUriReference AboutEachUri
        {
            get { return aboutEachUri; }
        }

        /// <summary>
        /// Gets rdf:aboutEachPrefix uri reference.
        /// </summary>
        internal static RDFUriReference AboutEachPrefixUri
        {
            get { return aboutEachPrefixUri; }
        }

        /// <summary>
        /// Gets rdf:bagID uri reference.
        /// </summary>
        internal static RDFUriReference BagIDUri
        {
            get { return bagIDUri; }
        }

        /// <summary>
        /// Gets rdf:Property uri reference.
        /// </summary>
        internal static RDFUriReference PropertyUri
        {
            get { return propertyUri; }
        }

        /// <summary>
        /// Gets rdf:List uri reference.
        /// </summary>
        internal static RDFUriReference ListUri
        {
            get { return listUri; }
        }

        /// <summary>
        /// Gets rdf:Bag uri reference.
        /// </summary>
        internal static RDFUriReference BagUri
        {
            get { return bagUri; }
        }

        /// <summary>
        /// Gets rdf:Seq uri reference.
        /// </summary>
        internal static RDFUriReference SeqUri
        {
            get { return seqUri; }
        }

        /// <summary>
        /// Gets rdf:Alt uri reference.
        /// </summary>
        internal static RDFUriReference AltUri
        {
            get { return altUri; }
        }

        /// <summary>
        /// Gets rdf:value uri reference.
        /// </summary>
        internal static RDFUriReference ValueUri
        {
            get { return valueUri; }
        }

        /// <summary>
        /// Gets rdf:XMLLiteral uri reference.
        /// </summary>
        internal static RDFUriReference XMLLiteralUri
        {
            get { return xmlLiteralUri; }
        }

        #endregion

        #endregion

        #region Methods

        #region Interal

        /// <summary>
        /// This function when overridden by a child class validates a 
        /// portion of input RDF document and based on certain rules, may add 
        /// triples to the input graph.
        /// </summary>
        /// <param name="outputGraph">instance of output Graph.</param>
        internal abstract void Match(Graph outputGraph);

        /// <summary>
        /// Applies reification rule to given statement and create Triplets.
        /// </summary>
        /// <param name="s">Subject Node</param>
        /// <param name="p">Predicate Node</param>
        /// <param name="o">Node object</param>
        /// <param name="r">RDF Uri Reference</param>
        /// <param name="outputGraph">Graph to add the generated triples.</param>
         internal static void Reify(Node s, Node p, Node o, RDFUriReference r, Graph outputGraph)
        {
            if (s == null)
                throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, Resources.MsgNullArgument, "s"));
             if(p == null)
                 throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, Resources.MsgNullArgument, "p"));
             if (o == null)
                 throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, Resources.MsgNullArgument, "o"));
             if (r == null)
                 throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, Resources.MsgNullArgument, "r"));
             if (outputGraph == null)
                 throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, Resources.MsgNullArgument, "outputGraph"));

            //For the given URI reference event r and the statement with terms s, p and o corresponding to the N-Triples:
            //s p o . 
            //add the following statements to the graph:
            //r.string-value <http://www.w3.org/1999/02/22-rdf-syntax-ns#subject> s .
            //r.string-value <http://www.w3.org/1999/02/22-rdf-syntax-ns#predicate> p .
            //r.string-value <http://www.w3.org/1999/02/22-rdf-syntax-ns#object> o .
            //r.string-value <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/1999/02/22-rdf-syntax-ns#Statement> .

            Triple statmentTriple = new Triple{ TripleSubject = r,
                TriplePredicate = TypeUri, TripleObject = StatementUri };
            outputGraph.Add(statmentTriple);

            Triple subjectTriple = new Triple { TripleSubject = r,
                TriplePredicate = SubjectUri, TripleObject = s };
            outputGraph.Add(subjectTriple);

            Triple predicateTriple = new Triple { TripleSubject = r,
                TriplePredicate = PredicateUri, TripleObject = p };
            outputGraph.Add(predicateTriple);

            Triple objectTriple = new Triple { TripleSubject = r, 
                TriplePredicate = ObjectUri, TripleObject = o };
            outputGraph.Add(objectTriple);

        }

        /// <summary>
         /// Resolve given string by by interpreting string as a relative URI reference 
         /// to the •base-uri• accessor of element as defined in W3C specification, 
         /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#section-baseURIs.
        /// </summary>
        /// <param name="e">Event element.</param>
        /// <param name="a">Relative string to resolve.</param>
        /// <returns>Resoved RDFUriReference object.</returns>
         /// <exception cref="Zentity.Rdf.Xml.RdfXmlParserException">Invalid uri reference.</exception>
        internal static RDFUriReference Resolve(EventElement e, string a)
        {
            try
            {
                //TODO: Handle Possible Test Cases.
                //TODO: Case To Handle. a is empty and base Uri is default base Uri.
                Uri baseUri = new Uri(e.BaseUri);
                if (string.IsNullOrEmpty(a) && !string.IsNullOrEmpty(baseUri.Fragment))
                {
                    baseUri = new Uri(baseUri.AbsoluteUri.Replace(baseUri.Fragment, string.Empty));
                }
                return new RDFUriReference(baseUri, a);
            }
            catch (UriFormatException ex)
            {
                throw new RdfXmlParserException(ex, Constants.ErrorMessageIds.MsgEventElementInvalidUri, e.LineInfo);
            }
        }

        #endregion

        #region Protected Static

        /// <summary>
        /// Creates amd adds Triple object to Graph.
        /// </summary>
        /// <param name="outputGraph">Output Graph object.</param>
        /// <param name="s">Isubject object.</param>
        /// <param name="p">RDFUriReference object.</param>
        /// <param name="o">INode object.</param>
        protected static void AddTriple(Graph outputGraph, Subject s, RDFUriReference p, Node o)
        {
            Triple trpl = new Triple
            {
                TripleSubject = s,
                TriplePredicate = p,
                TripleObject = o
            };

            outputGraph.Add(trpl);
        }

        #endregion

        #region Private Static

        /// <summary>
        /// Returns list of core syntax terms.
        /// rdf:RDF | rdf:ID | rdf:about | rdf:parseType | rdf:resource | rdf:nodeID | rdf:datatype
        /// </summary>
        /// <returns>List of core syntax terms.</returns>
        private static List<RDFUriReference> GetCoreSyntaxTerms()
        {
            List<RDFUriReference> coreSyntaxTerms = new List<RDFUriReference>();
            coreSyntaxTerms.Add(RDFUri);
            coreSyntaxTerms.Add(IDUri);
            coreSyntaxTerms.Add(AboutUri);
            coreSyntaxTerms.Add(ParseTypeUri);
            coreSyntaxTerms.Add(ResourceUri);
            coreSyntaxTerms.Add(NodeIDUri);
            coreSyntaxTerms.Add(DatatypeUri);
            return coreSyntaxTerms;
        }

        /// <summary>
        /// Returns list of systax terms.
        /// coreSyntaxTerms | rdf:Description | rdf:li
        /// </summary>
        /// <returns>List of syntax terms.</returns>
        private static List<RDFUriReference> GetSyntaxTerms()
        {
            List<RDFUriReference> syntaxTerms = new List<RDFUriReference>();
            syntaxTerms.AddRange(GetCoreSyntaxTerms());
            syntaxTerms.Add(DescriptionUri);
            syntaxTerms.Add(LiUri);

            return syntaxTerms;
        }

        /// <summary>
        /// Returns list of class terms.
        /// Seq | Bag | Alt | Statement | Property | XMLLiteral | List 
        /// </summary>
        /// <returns>List of class terms.</returns>
        private static List<RDFUriReference> GetClassTerms()
        {
            List<RDFUriReference> classTerms = new List<RDFUriReference>();
            classTerms.Add(PropertyUri);
            classTerms.Add(StatementUri);
            classTerms.Add(ListUri);
            classTerms.Add(BagUri);
            classTerms.Add(SeqUri);
            classTerms.Add(AltUri);
            classTerms.Add(XMLLiteralUri);

            return classTerms;
        }

        /// <summary>
        /// Returns list of property terms.
        /// subject | predicate | object | type | value | first | rest | _n
        /// where n is a decimal integer greater than zero with no leading zeros.  
        /// </summary>
        /// <returns>List of property terms.</returns>
        private static List<RDFUriReference> GetPropertyTerms()
        {
            List<RDFUriReference> propertyTerms = new List<RDFUriReference>();

            propertyTerms.Add(TypeUri);
            propertyTerms.Add(SubjectUri);
            propertyTerms.Add(PredicateUri);
            propertyTerms.Add(ObjectUri);
            propertyTerms.Add(ValueUri);
            propertyTerms.Add(NilUri);
            propertyTerms.Add(FirstUri);
            propertyTerms.Add(RestUri);

            return propertyTerms;
        }

        /// <summary>
        /// Returns list of old syntax terms.
        /// rdf:aboutEach | rdf:aboutEachPrefix | rdf:bagID
        /// </summary>
        /// <returns>List of old syntax terms.</returns>
        private static List<RDFUriReference> GetOldTerms()
        {
            List<RDFUriReference> oldTerms = new List<RDFUriReference>();
            oldTerms.Add(AboutEachUri);
            oldTerms.Add(AboutEachPrefixUri);
            oldTerms.Add(BagIDUri);
            return oldTerms;
        }

        /// <summary>
        /// Returns list of all RDF terms.
        /// </summary>
        /// <returns>List of rdf terms.</returns>
        private static List<RDFUriReference> GetAllRdfTerms()
        {
            List<RDFUriReference> terms = new List<RDFUriReference>();

            terms.AddRange(GetCoreSyntaxTerms());
            terms.AddRange(GetSyntaxTerms());
            terms.AddRange(GetOldTerms());
            terms.AddRange(GetClassTerms());
            terms.AddRange(GetPropertyTerms());

            return terms;
        }

        #endregion

        #endregion
    }
}
