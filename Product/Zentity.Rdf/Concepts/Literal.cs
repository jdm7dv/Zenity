// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Concepts
{
    /// <summary>
    /// This abstract class represent an RDF literal 
    /// (http://www.w3.org/TR/2004/REC-rdf-concepts-20040210/#section-Graph-Literal).
    /// </summary>
    /// <example>Example below shows the parsing of a simple RDF/XML document with a plain literal.
    /// <code>
    ///using Zentity.Rdf.Xml;
    ///using Zentity.Rdf.Concepts;
    ///using System.IO;
    ///using System;
    ///
    ///namespace ZentitySamples
    ///{
    ///    public class Program_RdfConceptsLiteral
    ///    {
    ///        public static void Main_RdfConceptsLiteral(string[] args)
    ///        {
    ///            string rdfXmlInstance =
    ///            @&quot;&lt;?xml version=&quot;&quot;1.0&quot;&quot;?&gt;
    ///            &lt;rdf:RDF xmlns:rdf=&quot;&quot;http://www.w3.org/1999/02/22-rdf-syntax-ns#&quot;&quot;
    ///                     xmlns:zentity=&quot;&quot;http://www.contoso.com/&quot;&quot;&gt;
    ///
    ///              &lt;rdf:Description rdf:about=&quot;&quot;http://www.contoso.com/Resource/1234&quot;&quot;&gt;
    ///                &lt;zentity:Title&gt;Foo Bar&lt;/zentity:Title&gt;
    ///              &lt;/rdf:Description&gt;
    ///
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
    public abstract class Literal : Node
    {
        #region Member Variables

        #region Private 
        private string lexicalForm;
        #endregion

        #endregion

        #region Properties

        #region Public
        /// <summary>
        /// Gets the lexical form of the literal which is a unicode string.
        /// </summary>
        public string LexicalForm
        {
            get { return lexicalForm; }
        }
        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the Literal class. This constructor initializes LexicalForm property using input parameter.
        /// </summary>
        /// <param name="lexicalForm">LexicalForm string value.</param>
        protected Literal(string lexicalForm)
        {
            this.lexicalForm = lexicalForm;
        }

        #endregion
    }
}
