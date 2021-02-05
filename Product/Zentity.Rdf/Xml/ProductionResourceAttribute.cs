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
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#resourceAttr production.
    /// </summary>
    internal sealed class ProductionResourceAttribute : Production
    {
        #region Member Variables

        #region Private
        EventAttribute innerAttribute;
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionResourceAttribute class.
        /// The constructor takes an EventAttribute to initialize itself.
        /// </summary>
        /// <param name="attribute">EventAttribute object to process.</param>
        internal ProductionResourceAttribute(EventAttribute attribute)
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
        /// Validates resource attribute based on following syntax rules, 
        /// <br/>
        /// attribute(URI == rdf:resource, string-value == URI-reference) 
        /// </summary>
        /// <param name="outputGraph">Rdf Graph containing a set of Rdf triples.</param>
        internal override void Match(Graph outputGraph)
        {

            if (innerAttribute.Uri != ResourceUri)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgResourceAttributeUriNotMatch, innerAttribute.LineInfo);

            //This function call raises RdfXmlParserException if string value is 
            //invalid uri reference.
            Production.Resolve(innerAttribute.Parent, innerAttribute.StringValue);
        }
        #endregion

        #endregion

    }
}
