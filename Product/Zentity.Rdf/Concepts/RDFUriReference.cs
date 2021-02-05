// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Concepts
{
    using System;

    /// <summary>
    /// This class represents an RDF URI Reference 
    /// (http://www.w3.org/TR/2004/REC-rdf-concepts-20040210/#section-Graph-URIref). 
    /// </summary>
    public sealed class RDFUriReference : Subject
    {
        #region Member Variables

        #region private
        Uri innerUri;

        #endregion

        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets inner Uri.
        /// </summary>
        public Uri InnerUri
        {
            get { return innerUri; }
        }

        #endregion

        #endregion

        #region Constructors

        #region Public

        /// <summary>
        /// Initializes a new instance of the RDFUriReference class with the specified URI.
        /// </summary>
        /// <param name="uriString">Uri String.</param>
        /// <exception cref="System.ArgumentNullException">uriString is null.</exception>
        /// <exception cref="System.UriFormatException">Format of uriString is invalid.</exception>
        public RDFUriReference(string uriString)
        {
            innerUri = new Uri(uriString);
        }

        /// <summary>
        /// Initializes a new instance of the RDFUriReference class based on the specified 
        /// base URI and relative URI string.
        /// </summary>
        /// <param name="baseUri">Base Uniform resource identifier.</param>
        /// <param name="relativeUri">Relative Uri string.</param>
        /// <exception cref="System.ArgumentNullException">uriString is null.</exception>
        /// <exception cref="System.UriFormatException">Format of uriString is invalid.</exception>
        public RDFUriReference(Uri baseUri, string relativeUri)
        {
            innerUri = new Uri(baseUri, relativeUri);
        }

        #endregion

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Return absolut Uri string in N-Triple format.
        /// </summary>
        /// <returns>absolute Uri string in N-Triple format</returns>
        public override string ToString()
        {
            return "<" + innerUri.ToString() + ">";
        }

        #region ICloneable Members

        /// <summary>
        /// Creates a new RDFUriReference object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new RDFUriReference object that is a copy of the current instance.</returns>
        public override object Clone()
        {
            RDFUriReference clonedReference = new RDFUriReference(this.innerUri.AbsoluteUri);
            return clonedReference;
        }

        #endregion

        #endregion

        #endregion
    }
}
