// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;
using System.Collections.Generic;
using System.Linq;
using Zentity.Core;
using Zentity.Rdf.Concepts;
using System.Globalization;

namespace Zentity.Core
{
    /// <summary>
    /// This class interprets domain and range triples in the Graph.
    /// </summary>
    internal class DomainRangeInterpreter : RdfsInterpreter
    {
        #region Member Variables

        #region Private
        private string propertyNameFormat = "{0}_Inverse";
        private string associationNameFormat = "{0}_{1}_{2}";
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the DomainRangeInterpreter class with 
        /// a specified context.
        /// </summary>
        /// <param name="context">Instance of context class.</param>
        internal DomainRangeInterpreter(Context context) : base(context) { }
        #endregion

        #endregion

        #region Methods

        #region Internal

        /// <summary>
        /// Interprets Domain and Range triples.
        /// </summary>
        /// <param name="dataModelModule">Instance of DataModelModule class to be updated.</param>
        internal override void Interpret(DataModelModule dataModelModule)
        {
            // Validate domain and range triple rules.
            Validate();

            foreach (Triple triple in PropertyTriples)
            {
                //Get range and domain triples.
                Triple rangeTriple = RangeTriples.
                    Where(tuple => tuple.TripleSubject == triple.TripleSubject).First();
                Triple domainTriple = DomainTriples.
                    Where(tuple => tuple.TripleSubject == triple.TripleSubject).First();

                //Add property.
                if (IsScalarProperty(rangeTriple.TripleObject as RDFUriReference))
                {
                    AddScalarProperty(domainTriple, rangeTriple, dataModelModule);
                }
                else
                {
                    AddNavigationProperty(domainTriple, rangeTriple, dataModelModule);

                }

            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Validates this instance.
        /// </summary>
        private void Validate()
        {
            //Validation 1. Each rdfs:domain triple MUST have a corresponding rdfs:type 
            //triple such that subject(rdfs:domain triple) = subject(rdfs:type triple). 
            //The object of rdfs:type triple MUST be rdfs:Property.
            //Validation 2. Each rdfs:domain triple MUST have a corresponding rdfs:type 
            //triple such that object(rdfs:domain triple) = subject(rdfs:type triple). 
            //The object of the rdfs:type triple MUST be rdfs:Class.
            foreach (Triple triple in DomainTriples)
            {
                if (PropertyTriples.Where(tuple =>
                    tuple.TripleSubject == triple.TripleSubject).Count() == 0)
                {
                    throw new RdfsException(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.MsgDomainInterpreterDomainTripleSubjectNotDefined,
                        triple.ToString()));
                }

                if (ClassTriples.Where(tuple =>
                        tuple.TripleSubject == triple.TripleObject).Count() == 0)
                {
                    throw new RdfsException(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.MsgDomainInterpreterDomainTripleObjectNotDefined,
                        triple.ToString()));
                }
            }

            //Validation 3. Each rdfs:range triple MUST have a corresponding rdf:type 
            //triple with the same subject. The object of rdf:type triple MUST be rdfs:Property. 
            //Validation 4. Each rdfs:range triple MAY have a corresponding rdfs:type triple 
            //such that object(rdfs:range triple) = subject(rdfs:type triple). If there exists 
            //such an rdfs:type triple, then the object of the rdfs:type triple MUST be 
            //rdfs:Class. Otherwise, the object of rdfs:range SHOULD be treated as a datatype 
            //reference, XSD or derived.
            foreach (Triple triple in RangeTriples)
            {
                if (PropertyTriples.Where(tuple =>
                    tuple.TripleSubject == triple.TripleSubject).Count() == 0)
                {
                    throw new RdfsException(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.MsgDomainInterpreterRangeTripleSubjectNotDefined,
                        triple.ToString()));
                }

                if (ClassTriples.
                        Where(tuple => tuple.TripleSubject == triple.TripleObject).Count() == 0
                    &&
                    (Context.XsdDataTypeCollection.
                        Where(tuple => tuple.Name == triple.TripleObject).Count() == 0)
                    )
                {
                    throw new RdfsException(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.MsgDomainInterpreterRangeTripleObjectNotDefined,
                        triple.ToString()));
                }
            }

        }

        /// <summary>
        /// Determines whether the Uri reference is a valid scalar property.
        /// </summary>
        /// <param name="propertyUri">The property URI.</param>
        /// <returns>
        /// 	<c>true</c> if Uri reference is a valid scalar property; otherwise, <c>false</c>.
        /// </returns>
        private bool IsScalarProperty(RDFUriReference propertyUri)
        {
            //If property present in XSD datatype collection then it is 
            //a Scalar property.
            if (Context.XsdDataTypeCollection.
                    Where(tuple => tuple.Name == propertyUri).Count() > 0)
                return true;

            return false;
        }

        /// <summary>
        /// Adds the scalar property.
        /// </summary>
        /// <param name="domainTriple">The domain triple.</param>
        /// <param name="rangeTriple">The range triple.</param>
        /// <param name="dataModelModule">The data model module.</param>
        private void AddScalarProperty(Triple domainTriple, Triple rangeTriple, DataModelModule dataModelModule)
        {
            //Get property name.
            string propertyName =
                GetLocalName(domainTriple.TripleSubject as RDFUriReference);
            //Get resource type.
            ResourceType domainResType = dataModelModule.ResourceTypes[
                GetLocalName(domainTriple.TripleObject as RDFUriReference)];
            XsdDataType dataType = Context.XsdDataTypeCollection.
                    Where(tuple => tuple.Name == rangeTriple.TripleObject).First();

            //Create scalar property and add to resource type.
            ScalarProperty property = new ScalarProperty(propertyName, dataType.BaseType);
            property.MaxLength = dataType.MaxLength;
            property.Precision = dataType.Precision;
            property.Scale = dataType.Scale;

            domainResType.ScalarProperties.Add(property);
        }

        /// <summary>
        /// Adds the navigation property.
        /// </summary>
        /// <param name="domainTriple">The domain triple.</param>
        /// <param name="rangeTriple">The range triple.</param>
        /// <param name="dataModelModule">The data model module.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "association")]
        private void AddNavigationProperty(Triple domainTriple, Triple rangeTriple, DataModelModule dataModelModule)
        {
            //Get property name.
            string propertyName =
                GetLocalName(domainTriple.TripleSubject as RDFUriReference);

            //Get subject resource type.
            ResourceType subjectResType = dataModelModule.ResourceTypes[
                    GetLocalName(domainTriple.TripleObject as RDFUriReference)];
            //Create subject navigation property and add to subject resource type.
            NavigationProperty subProperty = new NavigationProperty();
            subProperty.Name = propertyName;
            subProperty.Uri = (rangeTriple.TripleSubject as RDFUriReference).InnerUri.AbsoluteUri;
            subjectResType.NavigationProperties.Add(subProperty);

            //Get object resource type.
            ResourceType objectResType = dataModelModule.ResourceTypes[
                    GetLocalName(rangeTriple.TripleObject as RDFUriReference)];
            //Create object navigation property and add to object resource type.
            NavigationProperty objProperty = new NavigationProperty();
            objProperty.Name = string.Format(CultureInfo.InvariantCulture,
                propertyNameFormat, propertyName);
            objProperty.Uri = string.Format(CultureInfo.InvariantCulture,
                propertyNameFormat,
                (rangeTriple.TripleSubject as RDFUriReference).InnerUri.AbsoluteUri);
            objectResType.NavigationProperties.Add(objProperty);

            //Create Association object.
            string assName = string.Format(CultureInfo.InvariantCulture,
                associationNameFormat, subjectResType.Name, subProperty.Name, objectResType.Name);
            Association association = new Association(assName)
            {
                SubjectNavigationProperty = subProperty,
                ObjectNavigationProperty = objProperty
            };
        }

        #endregion

        #endregion
    }
}
