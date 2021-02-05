// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Concepts
{
    using System;

    /// <summary>
    /// This abastract class represents a node in the RDF graph 
    /// (http://www.w3.org/TR/2004/REC-rdf-concepts-20040210/#section-Graph-Node).
    /// </summary>
    /// <example>
    /// <code>
    ///using Zentity.Rdf.Xml;
    ///using Zentity.Rdf.Concepts;
    ///using System.IO;
    ///using System;
    ///using System.Collections.Generic;
    ///using System.Linq;
    ///
    ///namespace ZentitySamples
    ///{
    ///    public class Program_RdfConceptsNode
    ///    {
    ///        public static void Main_RdfConceptsNode(string[] args)
    ///        {
    ///            string rdfXmlInstance =
    ///            @&quot;&lt;?xml version=&quot;&quot;1.0&quot;&quot;?&gt;
    ///            &lt;rdf:RDF xmlns:rdf=&quot;&quot;http://www.w3.org/1999/02/22-rdf-syntax-ns#&quot;&quot;
    ///              xmlns:zentity=&quot;&quot;http://www.contoso.com/&quot;&quot;&gt;
    ///              &lt;rdf:Description rdf:about=&quot;&quot;http://www.contoso.com/Publication/1234&quot;&quot;
    ///	            zentity:Title=&quot;&quot;Test Doc&quot;&quot;&gt;
    ///                &lt;zentity:Author&gt;
    ///                  &lt;rdf:Description zentity:FirstName=&quot;&quot;Foo&quot;&quot;/&gt;
    ///                &lt;/zentity:Author&gt;
    ///              &lt;/rdf:Description&gt;
    ///            &lt;/rdf:RDF&gt;
    ///            &quot;;
    ///
    ///            RdfXmlParser parser = new RdfXmlParser(new StringReader(rdfXmlInstance));
    ///            Graph graph = parser.Parse(true);
    ///
    ///            Console.WriteLine(&quot;Graph nodes:--&quot;);
    ///
    ///            foreach (Node n in graph.Select(triple =&gt; triple.TripleSubject as Node).
    ///                Union(graph.Select(triple =&gt; triple.TripleObject)))
    ///            {
    ///                Console.WriteLine(n.ToString());
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public abstract class Node : IComparable<Node>, ICloneable
    {
        #region Methods

        #region Public

        /// <summary>
        /// Chacks whether specified Node objects are equal.
        /// </summary>
        /// <param name="first">First Node object.</param>
        /// <param name="second">Second Node object.</param>
        /// <returns>true if two instances are equal, else false.</returns>
        public static bool operator ==(Node first, Node second)
        {
            if (object.Equals(first, second))
                return true;

            if (object.Equals(first, null) && !object.Equals(second, null))
                return false;

            if (first.CompareTo(second) == 0)
                return true;

            return false;
        }

        /// <summary>
        /// Chacks whether specified Node object are not eqal.
        /// </summary>
        /// <param name="first">First Node object.</param>
        /// <param name="second">Second Node object.</param>
        /// <returns>true if two instances are not equal, else false.</returns>
        public static bool operator !=(Node first, Node second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Chacks whether the first Node object has precedence over second 
        /// Node object.
        /// </summary>
        /// <param name="first">First Node object.</param>
        /// <param name="second">Second Node object.</param>
        /// <returns>true if first Node object has precedence over second 
        /// Node object, else false.
        /// </returns>
        public static bool operator <(Node first, Node second)
        {
            if (first == null && second == null)
                return false;

            if (first == null && second != null)
                return true;

            if (first.CompareTo(second) < 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Chacks whether the second Node object has precedence over first 
        /// Node object.
        /// </summary>
        /// <param name="first">First Node object.</param>
        /// <param name="second">Second Node object.</param>
        /// <returns>true if second Node object has precedence over first 
        /// Node object, else false.
        /// </returns>
        public static bool operator >(Node first, Node second)
        {
            if (first == null && second == null)
                return false;

            if (first == null && second != null)
                return false;

            if (first.CompareTo(second) > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current instance.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal 
        /// to a specified Subject object.
        /// </summary>
        /// <param name="obj">A Subject object to compare to this instance.</param>
        /// <returns>true if obj has the same reference as this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        #region IComparable<Node> Members

        /// <summary>
        /// Compares this instance with a specified Node instance.
        /// </summary>
        /// <param name="other">Instance of Node to compare.</param>
        /// <returns> 
        /// value &gt; 0 if this instance precedes other instance, 
        /// value = 0 if this instance equals to other instance,
        /// value &lt; 0 if other instance precedes this instance.
        /// </returns>
        public int CompareTo(Node other)
        {
            if (other == null)
                return 1;

            return string.Compare(this.ToString(), other.ToString(), StringComparison.Ordinal);
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Overriden in Derived class.
        /// </summary>
        /// <returns>Cloned instance of Node class.</returns>
        public abstract object Clone();

        #endregion

        #endregion

        #endregion
    }
}
