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
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#datatypeAttr production.
    /// </summary>
    internal sealed class ProductionDataTypeAttribute : Production
    {
        #region Member Variables

        #region Private
        EventAttribute innerAttribute;
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionDataTypeAttribute class. 
        /// The constructor takes an EventAttribute to initialize itself.
        /// </summary>
        /// <param name="attribute">EventAttribute object.</param>
        internal ProductionDataTypeAttribute(EventAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, Resources.MsgNullArgument, "attribute"));

            this.innerAttribute = attribute;
        }
        #endregion

        #endregion

        #region Methods

        #region Internal
        /// <summary>
        /// Validates datatype attribute based on following syntax rules, 
        /// <br/>
        /// attribute(URI == rdf:datatype, string-value == URI-reference) 
        /// </summary>
        /// <param name="outputGraph">Graph to add the generated triples.</param>
        internal override void Match(Graph outputGraph)
        {
            if (innerAttribute.Uri != DatatypeUri)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgDataTypeAttributeUriNotMatch, innerAttribute.LineInfo);

            //This function call raises RdfXmlParserException if string value is 
            //invalid Uri reference.
            Production.Resolve(innerAttribute.Parent, innerAttribute.StringValue);
        }
        #endregion

        #endregion
        


 
    }
}
