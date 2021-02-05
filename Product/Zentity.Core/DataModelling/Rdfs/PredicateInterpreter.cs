// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System.Collections.Generic;
using System.Linq;
using Zentity.Rdf.Concepts;
using Zentity.Core;
using System.Globalization;

namespace Zentity.Core
{
    /// <summary>
    /// This class interprets predicate triples in the Graph.
    /// </summary>
    internal class PredicateInterpreter : RdfsInterpreter
    {
        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the PredicateInterpreter class with 
        /// a specified context.
        /// </summary>
        /// <param name="context">Instance of Context class.</param>
        internal PredicateInterpreter(Context context) : base(context) { }
        #endregion

        #endregion

        #region Methods

        #region Internal
        /// <summary>
        /// Validates Graph with the rules applicable to predicates. 
        /// </summary>
        /// <param name="dataModelModule">Instance of DataModelModule class to be updated.</param>
        internal override void Interpret(DataModelModule dataModelModule)
        {
            Validate();
        }
        #endregion

        #region Private
        /// <summary>
        /// Validates this instance.
        /// </summary>
        private void Validate()
        {
            //Validation 1. For ‘Strict’ validation, the importer MUST hold the 
            //input graph to be in error if duplicate triples are present in the 
            //graph. For ‘Loose’ validation, duplicate triples are ignored and a 
            //dedup operation is performed before any further processing.
            Graph dedupGraph = Context.Graph.GetDeDuplicatedGraph();
            if (Context.ExecutionMode == ExecutionMode.Strict
                & Context.Graph.Count != dedupGraph.Count)
            {
                throw new RdfsException(DataModellingResources.MsgPredicateInterpreterDuplicateTriples);
            }
            else
            {
                Context.Graph = dedupGraph;
            }

            //Validation 2. For ‘Strict’ validation, the importer MUST hold the input 
            //graph to be in error if it contains triples having predicates other than 
            //those described in this section. In the case of ‘Loose’ validation, triples 
            //having predicates other than those defined in this document are removed 
            //before any further processing.
            List<Triple> restrictedTriples =
                Context.Graph.Where(tuple =>
                    tuple.TriplePredicate != TypeUri
                    && tuple.TriplePredicate != SubClassOfUri
                    && tuple.TriplePredicate != DomainUri
                    && tuple.TriplePredicate != RangeUri).ToList();

            if (restrictedTriples.Count() > 0)
            {
                if (Context.ExecutionMode == ExecutionMode.Strict)
                {
                    throw new RdfsException(DataModellingResources.MsgPredicateInterpreterNotSupportedPredicates);
                }
                else
                {
                    foreach (Triple triple in restrictedTriples)
                    {
                        Context.Graph.Remove(triple);
                    }
                }
            }
        }
        #endregion

        #endregion
    }
}
