// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Concepts
{
    using System;

    /// <summary>
    /// This class represents an RDF triple (http://www.w3.org/TR/2004/REC-rdf-concepts-20040210/#section-triples).
    /// An RDF triple contains three components:
    /// <ul>
    /// <li>the subject, which is an RDF URI reference or a blank node</li>
    /// <li>the predicate, which is an RDF URI reference</li>
    /// <li>the object, which is an RDF URI reference, a literal or a blank node</li>
    /// </ul>
    /// </summary>
    public sealed class Triple : ICloneable
    {
        #region Member Variables

        #region Private
        Subject tripleSubject;
        Node tripleObject;
        RDFUriReference triplePredicate;
        #endregion

        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets the subject node of the triple. Take note that Literals 
        /// are not allowed to be the subject of a triple.
        /// </summary>
        public Subject TripleSubject
        {
            get { return tripleSubject; }
            set { tripleSubject = value; }
        }

        /// <summary>
        /// Gets or sets the object node of the triple. Object of a triple could 
        /// be a URI reference, Literal or Blank Node.
        /// </summary>
        public Node TripleObject
        {
            get { return tripleObject; }
            set { tripleObject = value; }
        }

        /// <summary>
        /// Gets or sets the predicate of the triple. A predicate is a URI reference.
        /// </summary>
        public RDFUriReference TriplePredicate
        {
            get { return triplePredicate; }
            set { triplePredicate = value; }
        }

        #endregion

        #endregion

        #region Methods

        #region Public
        /// <summary>
        /// Serializes Triple object in N-Triple format.
        /// </summary>
        /// <returns>Triple object in N-Triple format.</returns>
        public override string ToString()
        {
            if (tripleSubject != null && triplePredicate != null && tripleObject != null)
            {
                return tripleSubject.ToString() + " " +
                    triplePredicate.ToString() + " " + tripleObject.ToString() + " .";
            }
            else
            {
                return base.ToString();
            }
        }

        #region ICloneable Members

        /// <summary>
        /// Creates a new Triple object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Triple object that is a copy of the current instance.</returns>
        public object Clone()
        {
            Triple clonedTriple = new Triple();

            if (this.TripleSubject != null)
                clonedTriple.TripleSubject = this.TripleSubject.Clone() as Subject;
            if (this.TriplePredicate != null)
                clonedTriple.TriplePredicate = this.TriplePredicate.Clone() as RDFUriReference;
            if (this.TripleObject != null)
                clonedTriple.TripleObject = this.TripleObject.Clone() as Node;

            return clonedTriple;
        }

        #endregion

        #endregion

        #endregion
    }
}
