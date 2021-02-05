// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Concepts
{
    using System;
    using System.Globalization;

    /// <summary>
    /// This class represents an RDF typed literal 
    /// (http://www.w3.org/TR/2004/REC-rdf-concepts-20040210/#dfn-typed-literal). 
    /// Typed literals have a lexical form and a datatype URI being an RDF URI reference.
    /// </summary>
    /// <example>Example below shows parsing of an RDF/XML document with a typed literal.
    /// <code>
    ///using Zentity.Rdf.Xml;
    ///using Zentity.Rdf.Concepts;
    ///using System.IO;
    ///using System;
    ///
    ///namespace ZentitySamples
    ///{
    ///    public class Program_RdfConceptsTypedLiteral
    ///    {
    ///        public static void Main_RdfConceptsTypedLiteral(string[] args)
    ///        {
    ///            string rdfXmlInstance =
    ///            @&quot;&lt;?xml version=&quot;&quot;1.0&quot;&quot;?&gt;
    ///            &lt;rdf:RDF xmlns:rdf=&quot;&quot;http://www.w3.org/1999/02/22-rdf-syntax-ns#&quot;&quot;
    ///                     xmlns:zentity=&quot;&quot;http://www.contoso.com/&quot;&quot;&gt;
    ///              &lt;rdf:Description rdf:about=&quot;&quot;http://www.contoso.com/Publication/1234&quot;&quot;&gt;
    ///                &lt;zentity:DateAdded rdf:datatype=&quot;&quot;http://www.w3.org/2001/XMLSchema#dateTime&quot;&quot;&gt;2009-03-31&lt;/zentity:DateAdded&gt;
    ///              &lt;/rdf:Description&gt;
    ///            &lt;/rdf:RDF&gt;
    ///            &quot;;
    ///
    ///            RdfXmlParser parser = new RdfXmlParser(new StringReader(rdfXmlInstance));
    ///            Graph graph = parser.Parse(true);
    ///
    ///            foreach (Triple triple in graph)
    ///            {
    ///                Console.WriteLine(triple.ToString());
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public sealed class TypedLiteral : Literal
    {
        #region Member Veriables

        #region Private
        RDFUriReference datatypeUri;
        static RDFUriReference xmlLiteralDataTypeUri;
        #endregion

        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets the datatype uri for the typed literal.
        /// </summary>
        public RDFUriReference DataTypeUri
        {
            get { return datatypeUri; }
        }

        /// <summary>
        /// Gets uri for the only pre-defined data type in RDF, XMLLiteral.
        /// </summary>
        public static RDFUriReference XmlLiteralDataTypeUri
        {
            get 
            {
                if (xmlLiteralDataTypeUri == null)
                    xmlLiteralDataTypeUri =
                        new RDFUriReference(Constants.RdfNamespace + Constants.XMLLiteral);
                return xmlLiteralDataTypeUri;
            }

        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the TypedLiteral class. This constructor initializes itself using input parameter.
        /// </summary>
        /// <param name="lexicalForm">Lexical form.</param>
        /// <param name="dataTypeUri">Datatype Uri.</param>
        public TypedLiteral(string lexicalForm, RDFUriReference dataTypeUri)
            : base(lexicalForm)
        {
            if(dataTypeUri == null)
                throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, Resources.MsgNullArgument, "dataTypeUri"));

            datatypeUri = dataTypeUri;
        }

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Returns concatinated string of lexical form and Datatype uri in N-Triple format.
        /// </summary>
        /// <returns>Concatinated string of lexical form and datatype uri in N-Triple format.</returns>
        public override string ToString()
        {
            return "\"" + this.LexicalForm + "\"^^" + DataTypeUri.ToString();
        }

        #region ICloneable Members

        /// <summary>
        /// Creates a new TypedLiteral object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new TypedLiteral object that is a copy of the current instance.</returns>
        public override object Clone()
        {
            return new TypedLiteral(this.LexicalForm, 
                this.DataTypeUri.Clone() as RDFUriReference);
        }

        #endregion

        #endregion

        #endregion
    }
}
