// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zentity.Rdf.Concepts;
using Zentity.Core;
using System.Globalization;

namespace Zentity.Core
{
    /// <summary>
    /// Defines the class to handle "subClass" element in Rdfs
    /// </summary>
    internal class SubClassOfInterpreter : RdfsInterpreter
    {
        #region Constrcutor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the SubClassOfInterpreter class with 
        /// a specified context.
        /// </summary>
        /// <param name="context">Instance of a Context class.</param>
        internal SubClassOfInterpreter(Context context) : base(context) { }
        #endregion

        #endregion

        #region Methods

        #region Internal
        /// <summary>
        /// Validates graph with the rules applicable to SubClassOf triples and 
        /// interprets SubClassOf triples. 
        /// </summary>
        /// <param name="dataModelModule">Instance of DataModelModule class to be updated.</param>
        internal override void Interpret(DataModelModule dataModelModule)
        {
            Validate();

            foreach (Triple triple in SubClassOfTriples)
            {
                ResourceType childClass =
                    dataModelModule.ResourceTypes[GetLocalName(
                    triple.TripleSubject as RDFUriReference)];
                ResourceType parentClass =
                    dataModelModule.ResourceTypes[GetLocalName(
                    triple.TripleObject as RDFUriReference)];

                childClass.BaseType = parentClass;

            }
        }
        #endregion

        #region Private
        /// <summary>
        /// Validates this instance.
        /// </summary>
        private void Validate()
        {
            //Validation 1. Importer MUST raise an error if multiple rdfs:subClassOf triples 
            //are defined with same subject.
            foreach (Triple triple in SubClassOfTriples)
            {
                if (SubClassOfTriples.Where(tuple =>
                    tuple.TripleSubject == triple.TripleSubject).Count() > 1)
                {
                    throw new RdfsException(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.MsgSubClassOfInterpreterSameSubject,
                        triple.TripleSubject.ToString()));
                }
            }

            //Validation 2. Importer MUST raise errors if an rdf:type (not rdfs:subClassOf) 
            //triple with object = rdf:Class is not defined in the graph for the subject and 
            //object classes of rdfs:subClassOf triple.
            foreach (Triple triple in SubClassOfTriples.ToList())
            {
                List<Triple> subjectTriples = ClassTriples.
                    Where(tuple => tuple.TripleSubject == triple.TripleSubject).ToList();
                List<Triple> objectTriples = ClassTriples.
                    Where(tuple => tuple.TripleSubject == triple.TripleObject).ToList();

                if (subjectTriples.Count() == 0 || objectTriples.Count() == 0)
                {
                    if (subjectTriples.Count() == 0)
                        throw new RdfsException(string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.MsgSubClassOfInterpreterSubjectNotDefined,
                            triple.ToString()));
                    else
                        throw new RdfsException(string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.MsgSubClassOfInterpreterObjectNotDefined,
                            triple.ToString()));
                }
            }

            //Validation 3: Subject and object of SubuClassOf triple should not be same.
            Triple invalidTriple = SubClassOfTriples.Where(tuple =>
                tuple.TripleSubject == tuple.TripleObject).FirstOrDefault();

            if (invalidTriple != null)
                throw new RdfsException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.MsgSubClassOfInterpreterSameSubjectObject,
                    invalidTriple.ToString()));
        }
        #endregion

        #endregion
    }
}
