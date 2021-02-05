// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Xml
{
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using Zentity.Rdf.Concepts;

    /// <summary>
    /// This class provides functionality to construct RDF Graph from RDF XML.
    /// </summary>
    /// <example>Example below shows simple parsing of an RDF/XML document.
    /// <code>
    ///using Zentity.Rdf.Xml;
    ///using Zentity.Rdf.Concepts;
    ///using System.IO;
    ///using System;
    ///
    ///namespace ZentitySamples
    ///{
    ///    public class Program_RdfXmlParser
    ///    {
    ///        public static void Main_RdfXmlParser(string[] args)
    ///        {
    ///            string rdfXmlInstance =
    ///            @&quot;&lt;?xml version=&quot;&quot;1.0&quot;&quot;?&gt;
    ///            &lt;rdf:RDF xmlns:rdf=&quot;&quot;http://www.w3.org/1999/02/22-rdf-syntax-ns#&quot;&quot;
    ///                        xmlns:dc=&quot;&quot;http://purl.org/dc/elements/1.1/&quot;&quot;&gt;
    ///
    ///              &lt;rdf:Description rdf:about=&quot;&quot;http://www.contoso.com/document1&quot;&quot;&gt;
    ///                &lt;dc:title&gt;A test document.&lt;/dc:title&gt;
    ///                &lt;dc:creator rdf:nodeID=&quot;&quot;abc&quot;&quot;/&gt;
    ///              &lt;/rdf:Description&gt;
    ///
    ///              &lt;rdf:Description rdf:nodeID=&quot;&quot;abc&quot;&quot;&gt;
    ///                &lt;dc:title&gt;Foo Bar&lt;/dc:title&gt;
    ///              &lt;/rdf:Description&gt;
    ///
    ///            &lt;/rdf:RDF&gt;&quot;;
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
    public sealed class RdfXmlParser
    {
        #region Member Variables

        #region Private
        XDocument innerDocument;
        #endregion

        #endregion

        #region Constructors

        #region Public

        /// <summary>
        /// Initializes a new instance of the RdfXmlParser class.
        /// Creates a new RdfXmlParser from a System.IO.TextReader.
        /// </summary>
        /// <param name="textReader">A System.IO.TextReader that contains the content for the RDF document.</param>
        public RdfXmlParser(TextReader textReader)
        {
            innerDocument = XDocument.Load(textReader, LoadOptions.SetLineInfo);
        }

        /// <summary>
        /// Initializes a new instance of the RdfXmlParser class.
        /// Creates a new parser instance and loads the document from a file.
        /// </summary>
        /// <param name="uri">Uri string.</param>
        public RdfXmlParser(string uri)
        {
            innerDocument = XDocument.Load(uri, LoadOptions.SetLineInfo);
        }

        /// <summary>
        /// Initializes a new instance of the RdfXmlParser class.
        /// Creates a new parser instance and loads the document from an System.Xml.XmlReader.
        /// </summary>
        /// <param name="reader">Intance of XmlReader class.</param>
        public RdfXmlParser(XmlReader reader)
        {
          innerDocument = XDocument.Load(reader, LoadOptions.SetLineInfo);
        }

        /// <summary>
        /// Initializes a new instance of the RdfXmlParser class.
        /// Creates a new parser instance and loads the document from a System.IO.TextReader, 
        /// optionally preserving white space, setting the base URI, and retaining line information.
        /// </summary>
        /// <param name="textReader">Instance of TextReader class.</param>
        /// <param name="options">Loading options.</param>
        public RdfXmlParser(TextReader textReader, LoadOptions options)
        {
            innerDocument = XDocument.Load(textReader, options | LoadOptions.SetLineInfo);
        }

        /// <summary>
        /// Initializes a new instance of the RdfXmlParser class.
        /// Creates a new parser instance and loads the document from a System.IO.TextReader, 
        /// optionally preserving white space, setting the base URI, and retaining line information.
        /// </summary>
        /// <param name="uri">Instance of TextReader class.</param>
        /// <param name="options">Loading options.</param>
        public RdfXmlParser(string uri, LoadOptions options)
        {
            innerDocument = XDocument.Load(uri, options | LoadOptions.SetLineInfo);
        }

        /// <summary>
        /// Initializes a new instance of the RdfXmlParser class.
        /// Creates a new parser instance and loads the document from an System.Xml.XmlReader, 
        /// optionally setting the base URI, and retaining line information.
        /// </summary>
        /// <param name="reader">Instance of System.Xml.XmlReader class.</param>
        /// <param name="options">Loading options.</param>
        public RdfXmlParser(XmlReader reader, LoadOptions options)
        {
            innerDocument = XDocument.Load(reader, options | LoadOptions.SetLineInfo);
        }

        /// <summary>
        /// Initializes a new instance of the RdfXmlParser class.
        /// Creates a new parser instance and initiaizes using specified 
        /// System.Xml.Linq.XDocument argument.
        /// </summary>
        /// <param name="doc">Instance of System.Xml.Linq.XDocument class.</param>
        public RdfXmlParser(XDocument doc)
        {
            innerDocument = doc;
        }

        #endregion

        #endregion

        #region Methods

        #region Public
        /// <summary>
        /// Parses RDF XML to RDF Graph.
        /// </summary>
        /// <param name="isRDFElementPresent">
        /// Flag to indicate whether root element RDF is present or not.
        /// </param>
        /// <returns>RDF Graph.</returns>
        public Graph Parse(bool isRDFElementPresent)
        {
            Graph graph = new Graph();
            ProductionDoc documentProduction = new ProductionDoc(innerDocument, isRDFElementPresent);
            documentProduction.Match(graph);
            return graph;
        }
        #endregion

        #endregion
    }
}
