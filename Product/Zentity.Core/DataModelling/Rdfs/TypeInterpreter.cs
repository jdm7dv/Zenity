// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Zentity.Rdf.Concepts;

namespace Zentity.Core
{
    /// <summary>
    /// Defines the class for Rdfs Types.
    /// </summary>
    internal class TypeInterpreter : RdfsInterpreter
    {
        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the TypeInterpreter class with 
        /// a specified context.
        /// </summary>
        /// <param name="context">Instance of a Context class.</param>
        internal TypeInterpreter(Context context) : base(context) { }
        #endregion

        #endregion

        #region Methods

        #region Internal
        /// <summary>
        /// Validates graph with the rules applicable to ManagerType triples and 
        /// interprets ManagerType triples. 
        /// </summary>
        /// <param name="dataModelModule">Instance of DataModelModule class to be updated.</param>
        internal override void Interpret(DataModelModule dataModelModule)
        {
            //Validate Graph
            Validate();

            foreach (Triple triple in ClassTriples)
            {
                ResourceType newResRype = new ResourceType(
                    GetLocalName(triple.TripleSubject as RDFUriReference),
                    Context.BaseResourceType);

                newResRype.Uri = (triple.TripleSubject as RDFUriReference).InnerUri.AbsoluteUri;

                dataModelModule.ResourceTypes.Add(newResRype);
            }
        }
        #endregion

        #region Private

        /// <summary>
        /// Validates this instance.
        /// </summary>
        private void Validate()
        {
            //Validation 1. In ‘Strict’ mode, the importer MUST raise errors 
            //if the object of rdf:type triples are other than rdfs:Class or 
            //rdf:Property. 
            List<Triple> restrictedTypes = TypeTriples.Where(tuple =>
                !(tuple.TripleObject == ClassUri ||
                    tuple.TripleObject == PropertyUri)).ToList();

            if (restrictedTypes.Count() > 0)
            {
                if (Context.ExecutionMode == ExecutionMode.Strict)
                {
                    throw new RdfsException(DataModellingResources.MsgTypeInterpreterNotSupportedObject);
                }
                else
                {
                    //In ‘Loose’ mode, the importer removes the rdf:type triples that have object 
                    //other than rdfs:Class or rdf:Property from the graph.
                    foreach (Triple triple in restrictedTypes)
                    {
                        Context.Graph.Remove(triple);
                    }
                }
            }


            //Validation 2. Importer MUST NOT allow multiple rdf:type triples in the graph 
            //having the same subject. 
            foreach (Triple triple in TypeTriples)
            {
                int dupSubjectCount = TypeTriples.Where(tuple =>
                tuple.TripleSubject == triple.TripleSubject).Count();
                if (dupSubjectCount > 1)
                {
                    throw new RdfsException(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.MsgTypeInterpreterDuplicateSubject,
                        triple.ToString()));
                }
            }

            ValidatePropertyTriples();
        }

        /// <summary>
        /// Validates the property triples.
        /// </summary>
        private void ValidatePropertyTriples()
        {
            //Validation 3. In ‘Strict’ mode, the importer MUST raise errors if there is a property 
            //defined in the graph, but the count of rdf:domain and rdf:range triples for 
            //this property present in the graph is not exactly one. 
            foreach (Triple triple in PropertyTriples.ToList())
            {
                List<Triple> propDomains = DomainTriples.Where(tuple =>
                    tuple.TripleSubject == triple.TripleSubject).ToList();
                List<Triple> propRanges = RangeTriples.Where(tuple =>
                    tuple.TripleSubject == triple.TripleSubject).ToList();

                if (Context.ExecutionMode == ExecutionMode.Strict)
                {
                    if (propDomains.Count() > 1)
                        throw new RdfsException(string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.MsgTypeInterpreterMoreDomains,
                            triple.ToString()));

                    if (propRanges.Count() > 1)
                        throw new RdfsException(string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.MsgTypeInterpreterMoreRanges,
                            triple.ToString()));

                    if (propDomains.Count() == 0)
                        throw new RdfsException(string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.MsgTypeInterpreterNoDomains,
                            triple.ToString()));

                    if (propRanges.Count() == 0)
                        throw new RdfsException(string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.MsgTypeInterpreterNoRanges,
                            triple.ToString()));
                }
                else
                {
                    //In ‘Loose’ mode, if the rdf:domain triple for the property is not present, 
                    //the importer removes the rdf:type triple along with all the rdf:range 
                    //triples in the graph.
                    if (propDomains.Count() != 1 || propRanges.Count() != 1)
                    {
                        Context.Graph.Remove(triple);
                        foreach (Triple domainTriple in propDomains)
                            Context.Graph.Remove(domainTriple);
                        foreach (Triple rangeTriple in propRanges)
                            Context.Graph.Remove(rangeTriple);
                    }
                }

            }
        }

        #endregion

        #endregion
    }
}
