// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Xml
{
    using System;
    using System.Globalization;
    using Zentity.Rdf.Concepts;

    /// <summary>
    /// This class represents the 
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#propertyAttr production.
    /// </summary>
    internal sealed class ProductionPropertyAttribute : Production
    {
        #region Member Variables

        #region Private
        EventAttribute innerAttribute;
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionPropertyAttribute class. 
        /// The constructor takes an EventAttribute to initialize itself.
        /// </summary>
        /// <param name="attribute">EventAttribute object to process.</param>
        internal ProductionPropertyAttribute(EventAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(
                    string.Format(CultureInfo.InvariantCulture,
                    Resources.MsgNullArgument, "attribute"));

            this.innerAttribute = attribute;
        }
        #endregion

        #endregion

        #region Methods

        #region Internal
        /// <summary>
        /// Validates property attribute based on following syntax rules, 
        /// <br/>
        /// attribute(URI == propertyAttributeURIs, string-value == anyString)
        /// </summary>
        /// <param name="outputGraph">Graph to add the generated triples.</param>
        internal override void Match(Graph outputGraph)
        {
            try
            {
                ProductionPropertyAttributeUris attrUris =
                    new ProductionPropertyAttributeUris(innerAttribute.Uri);

                attrUris.Match(outputGraph);
            }
            catch (RdfXmlParserException ex)
            {
                throw new RdfXmlParserException(ex.ErrorMessageId, innerAttribute.LineInfo);
            }
        }
        #endregion

        #endregion

       



    }
}
