// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Concepts
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// This class represents an RDF graph 
    /// (http://www.w3.org/TR/2004/REC-rdf-concepts-20040210/#section-rdf-graph). 
    /// An RDF graph is a set of RDF triples. 
    /// </summary>
    /// <example>Example below shows how to enumerate the graph elements.
    /// <code>
    ///using Zentity.Rdf.Xml;
    ///using Zentity.Rdf.Concepts;
    ///using System.IO;
    ///using System;
    ///
    ///namespace ZentitySamples
    ///{
    ///    public class Program_RdfConceptsGraph
    ///    {
    ///        public static void Main_RdfConceptsGraph(string[] args)
    ///        {
    ///            string rdfXmlInstance =
    ///            @&quot;&lt;?xml version=&quot;&quot;1.0&quot;&quot;?&gt;
    ///            &lt;rdf:RDF xmlns:rdf=&quot;&quot;http://www.w3.org/1999/02/22-rdf-syntax-ns#&quot;&quot;
    ///                     xmlns:zentity=&quot;&quot;http://www.contoso.com/&quot;&quot;&gt;
    ///
    ///              &lt;zentity:Person rdf:about=&quot;&quot;http://www.contoso.com/Persons/1234&quot;&quot;&gt;
    ///                &lt;zentity:FirstName&gt;Foo Bar&lt;/zentity:FirstName&gt;
    ///                &lt;zentity:Email rdf:resource=&quot;&quot;mailto:foobar@contoso.com&quot;&quot;/&gt;
    ///              &lt;/zentity:Person&gt;
    ///
    ///            &lt;/rdf:RDF&gt;
    ///            &quot;;
    ///
    ///            RdfXmlParser parser = new RdfXmlParser(new StringReader(rdfXmlInstance));
    ///            Graph graph = parser.Parse(true);
    ///
    ///            foreach (Triple triple in graph)
    ///            {
    ///                Console.WriteLine(&quot;Triple details:--&quot;);
    ///                Console.WriteLine(&quot;Subject:{0}&quot;, triple.TripleSubject);
    ///                Console.WriteLine(&quot;Predicate:{0}&quot;, triple.TriplePredicate);
    ///                Console.WriteLine(&quot;Object:{0}&quot;, triple.TripleObject);
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public sealed class Graph : ICollection<Triple>
    {
        #region Member Variables

        #region Private
        Collection<Triple> tripleCollection = new Collection<Triple>();
        #endregion

        #endregion

        #region Properties

        #region Public


        /// <summary>
        /// Gets the number of Triples contained in the Graph.
        /// </summary>
        public int Count
        {
            get { return tripleCollection.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the Graph is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Creates a clone of the RDF graph with duplicates triples removed. Object references 
        /// are not shared amongst the existing and the generated graph.
        /// </summary>
        /// <returns>Clone of the RDF Graph</returns>
        public Graph GetDeDuplicatedGraph()
        {
            Graph duplicateGraph = new Graph();

            foreach (Triple triple in this)
            {
                if (duplicateGraph.Where(
                    tuple => tuple.ToString().Equals(triple.ToString())).Count() == 0)
                {
                    duplicateGraph.Add(triple.Clone() as Triple);
                }
            }

            return duplicateGraph;
        }

        #region Public

        #region ICollection<Triple> Members

        /// <summary>
        /// Adds a Triple to the Graph.
        /// </summary>
        /// <param name="item">The Triple to add to the Graph.</param>
        /// <exception cref="System.NotSupportedException">The Graph is read-only.</exception> 
        public void Add(Triple item)
        {
            tripleCollection.Add(item);
        }

        /// <summary>
        /// Removes all Triples from the Graph.
        /// </summary>
        /// <exception cref="System.NotSupportedException">The Graph is read-only.</exception>
        public void Clear()
        {
            tripleCollection.Clear();
        }

        /// <summary>
        /// Determines whether the Graph contains a specific value.
        /// </summary>
        /// <param name="item">Rdf Triple</param>
        /// <returns>True if the Graph contains the Triple, else false.</returns>
        public bool Contains(Triple item)
        {
            return tripleCollection.Contains(item);
        }

        /// <summary>
        /// Copies the Triples of Graph to an System.Array, starting at a particular System.Array index.
        /// </summary>
        /// <param name="array">Array of Triple</param>
        /// <param name="arrayIndex">Array Index</param>
        /// <exception cref="System.ArgumentNullException"> array is null</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        /// <exception cref="System.ArgumentException">
        /// array is multidimensional.  -or- arrayIndex is equal to or greater than the
        /// length of array.  -or- The number of elements in the source System.Collections.Generic.ICollection&lt;T&gt;
        /// is greater than the available space from arrayIndex to the end of the destination
        /// array.  -or- Type T cannot be cast automatically to the type of the destination
        /// array.
        /// </exception>
        public void CopyTo(Triple[] array, int arrayIndex)
        {
            tripleCollection.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of a specific Triple from the Graph.
        /// </summary>
        /// <param name="item">The Triple to remove from the Triple.</param>
        /// <returns>
        /// true if item was successfully removed from the System.Collections.Generic.ICollection&lt;T&gt;
        /// otherwise, false. This method also returns false if item is not found in
        /// the original System.Collections.Generic.ICollection&lt;T&gt;.
        /// </returns>
        public bool Remove(Triple item)
        {
            return tripleCollection.Remove(item);
        }

        #endregion

        #region IEnumerable<Triple> Members

        /// <summary>
        /// Returns an enumerator that iterates through the Graph.
        /// </summary>
        /// <returns> A System.Collections.Generic.IEnumerator&lt;Triple&gt; 
        /// that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<Triple> GetEnumerator()
        {
            return tripleCollection.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a Graph.
        /// </summary>
        /// <returns>
        /// An System.Collections.IEnumerator Triple that can be used to iterate through 
        /// the Graph.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return tripleCollection.GetEnumerator();
        }

        #endregion

        #endregion

        #endregion
    }
}
