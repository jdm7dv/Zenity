// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;
using System.Collections.Generic;
using System.Linq;
using Zentity.Rdf.Concepts;
using Zentity.Core;
using System.Globalization;

namespace Zentity.Core
{
    /// <summary>
    /// This class defines abstract class for Interpreter classes.
    /// </summary>
    internal abstract class RdfsInterpreter
    {
        #region Variables

        #region Private
        Context context;
        private static RDFUriReference typeUri = new RDFUriReference(DataModellingResources.RdfNamespace + "type");
        private static RDFUriReference subClassOfUri = new RDFUriReference(DataModellingResources.RdfsNamespace + "subClassOf");
        private static RDFUriReference domainUri = new RDFUriReference(DataModellingResources.RdfsNamespace + "domain");
        private static RDFUriReference rangeUri = new RDFUriReference(DataModellingResources.RdfsNamespace + "range");
        private static RDFUriReference classUri = new RDFUriReference(DataModellingResources.RdfsNamespace + "Class");
        private static RDFUriReference propertyUri = new RDFUriReference(DataModellingResources.RdfNamespace + "Property");
        #endregion

        #endregion

        #region Properties

        #region Protected

        /// <summary>
        /// Gets current context.
        /// </summary>
        protected Context Context
        {
            get { return context; }
        }

        /// <summary>
        /// Gets rdf:type Uri.
        /// </summary>
        protected static RDFUriReference TypeUri
        {
            get { return typeUri; }
        }

        /// <summary>
        /// Gets rdfs:subClassOf Uri.
        /// </summary>
        protected static RDFUriReference SubClassOfUri
        {
            get { return subClassOfUri; }
        }

        /// <summary>
        /// Gets rdfs:domain Uri.
        /// </summary>
        protected static RDFUriReference DomainUri
        {
            get { return domainUri; }
        }

        /// <summary>
        /// Gets rdfs:range Uri.
        /// </summary>
        protected static RDFUriReference RangeUri
        {
            get { return rangeUri; }
        }

        /// <summary>
        /// Gets rdfs:Class Uri.
        /// </summary>
        protected static RDFUriReference ClassUri
        {
            get { return classUri; }
        }

        /// <summary>
        /// Gets rdfs:Property Uri.
        /// </summary>
        protected static RDFUriReference PropertyUri
        {
            get { return propertyUri; }
        }

        /// <summary>
        /// Gets rdf:type triples.
        /// </summary>
        protected IEnumerable<Triple> TypeTriples
        {
            get
            {
                return Context.Graph.Where(tuple => tuple.TriplePredicate == TypeUri);
            }
        }

        /// <summary>
        /// Gets rdfs:Class triples.
        /// </summary>
        protected IEnumerable<Triple> ClassTriples
        {
            get
            {
                return Context.Graph.Where(tuple =>
                  tuple.TriplePredicate == TypeUri && tuple.TripleObject == ClassUri);
            }
        }

        /// <summary>
        /// Gets rdfs:Property triples.
        /// </summary>
        protected IEnumerable<Triple> PropertyTriples
        {
            get
            {
                return Context.Graph.Where(tuple =>
                  tuple.TriplePredicate == TypeUri && tuple.TripleObject == PropertyUri);
            }
        }

        /// <summary>
        /// Gets rdfs:subClassOf triples.
        /// </summary>
        protected IEnumerable<Triple> SubClassOfTriples
        {
            get
            {
                return Context.Graph.Where(tuple => tuple.TriplePredicate == SubClassOfUri);
            }
        }

        /// <summary>
        /// Gets rdfs:domain triples.
        /// </summary>
        protected IEnumerable<Triple> DomainTriples
        {
            get
            {
                return Context.Graph.Where(tuple => tuple.TriplePredicate == DomainUri);
            }
        }

        /// <summary>
        /// Gets rdfs:range triples.
        /// </summary>
        protected IEnumerable<Triple> RangeTriples
        {
            get
            {
                return Context.Graph.Where(tuple => tuple.TriplePredicate == RangeUri);
            }
        }

        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of RdfsInterpreter class with 
        /// a specified context.
        /// </summary>
        /// <param name="context">The Rdfs context</param>
        internal RdfsInterpreter(Context context)
        {
            this.context = context;
        }
        #endregion

        #endregion

        #region Methods

        #region Internal
        /// <summary>
        /// When overriden by derived classes interpets the triples in the graph.
        /// </summary>
        /// <param name="dataModelModule">Instance of DataModelModule class to be updated.</param>
        internal abstract void Interpret(DataModelModule dataModelModule);
        #endregion

        #region Protected
        /// <summary>
        /// Returns local Name of a specified Uri.
        /// </summary>
        /// <param name="uri">The reference Uri.</param>
        /// <returns>Local Name of a specified Uri.</returns>
        protected static string GetLocalName(RDFUriReference uri)
        {
            if (uri != null)
            {
                if (!string.IsNullOrEmpty(uri.InnerUri.Fragment))
                {
                    return uri.InnerUri.Fragment.Replace("#", string.Empty);
                }
                else
                {
                    int index = uri.InnerUri.AbsoluteUri.LastIndexOf("/", StringComparison.Ordinal);
                    if (index >= 0)
                        return uri.InnerUri.AbsoluteUri.Substring(index + 1);
                }
            }

            return string.Empty;
        }
        #endregion 

        #endregion
    }
}
