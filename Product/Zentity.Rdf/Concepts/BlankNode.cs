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
    /// This class represents a blank node in RDF graph 
    /// (http://www.w3.org/TR/2004/REC-rdf-concepts-20040210/#section-blank-nodes). 
    /// </summary>
    /// <example>The example below shows the triples generated after parsing an RDF/XML
    /// document having blank nodes.
    /// <code>
    ///using Zentity.Rdf.Xml;
    ///using Zentity.Rdf.Concepts;
    ///using System.IO;
    ///using System;
    ///
    ///namespace ZentitySamples
    ///{
    ///    public class Program_RdfConceptsBlankNode
    ///    {
    ///        public static void Main_RdfConceptsBlankNode(string[] args)
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
    public sealed class BlankNode : Subject
    {
        #region Member Variables

        #region Private
        string localIdentifier;
        const string GenIdstr = "_:genid{0}";
        const string GenNodeIdstr = "_:{0}";
        #endregion

        #endregion

        #region Properties

        #region Public
        /// <summary>
        /// Gets the local identifier of the blank node with respect to the graph.
        /// </summary>
        public string LocalIdentifier
        {
            get { return localIdentifier; }
        }
        #endregion

        #endregion

        #region Constructors

        #region Public

        /// <summary>
        /// Initializes a new instance of the BlankNode class. This constructor Initializes LocalIdentifier property.
        /// </summary>
        public BlankNode()
        {
            localIdentifier = GenerateId();
        }

        /// <summary>
        /// Initializes a new instance of the BlankNode class. This constructor Initializes LocalIdentifier property using passed parameter.
        /// </summary>
        /// <param name="localIdentifier">LocalIdentifier value.</param>
        public BlankNode(string localIdentifier)
        {
            if (string.IsNullOrEmpty(localIdentifier))
                throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, 
                    Resources.MsgNullArgument, "localIdentifier"));

            localIdentifier = GenerateId(localIdentifier);
        }

        #endregion

        #endregion

        #region Methods

        #region Internal

        /// <summary>
        /// Generates local identifiers for blank nodes. The generated id is 
        /// of the form _:genid&lt;guid-value&gt;.
        /// </summary>
        /// <returns>Generated local identifier.</returns>
        internal static string GenerateId()
        {
            return string.Format(CultureInfo.InvariantCulture, GenIdstr, Guid.NewGuid().ToString().Replace("-", string.Empty));
        }

        /// <summary>
        /// Generates local identifiers for blank nodes. The generated id is 
        /// of the form _:&lt;identifier&gt;.
        /// </summary>
        /// <param name="identifier">local identifier value.</param>
        /// <returns>Generated local identifier.</returns>
        internal static string GenerateId(string identifier)
        {
            return string.Format(CultureInfo.InvariantCulture, GenNodeIdstr, identifier);
        }

        #endregion

        #region Public

        /// <summary>
        /// Returns LocalIdentifier value. 
        /// </summary>
        /// <returns>LocalIdentifier value</returns>
        public override string ToString()
        {
            return localIdentifier;
        }

        #region ICloneable Members

        /// <summary>
        /// Creates a new BlankNode object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new BlankNode object that is a copy of the current instance.</returns>
        public override object Clone()
        {
            BlankNode cloneBlankNode = new BlankNode();
            cloneBlankNode.localIdentifier = this.localIdentifier;

            return cloneBlankNode;
        }

        #endregion

        #endregion

        #endregion
    }
}
