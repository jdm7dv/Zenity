﻿// *******************************************************
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
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#parseOther production.
    /// </summary>
    internal sealed class ProductionParseOther : Production
    {
        #region Member Variables

        #region Private
        EventAttribute innerAttribute;
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionParseOther class. 
        /// The constructor takes an EventAttribute to initialize itself.
        /// </summary>
        /// <param name="attribute">EventAttribute object to process.</param>
        internal ProductionParseOther(EventAttribute attribute)
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
        /// Validates ParseOther attribute based on following syntax rules, 
        /// <br/>
        /// attribute(URI == rdf:parseType,
        /// string-value == anyString - ("Resource" | "Literal" | "Collection") ) 
        /// </summary>
        /// <param name="outputGraph">Graph to add the generated triples.</param>
        internal override void Match(Graph outputGraph)
        {
            if (innerAttribute.Uri != ParseTypeUri ||
                innerAttribute.StringValue.Equals(Constants.ParseTypeResource) ||
                innerAttribute.StringValue.Equals(Constants.ParseTypeLiteral) ||
                innerAttribute.StringValue.Equals(Constants.ParseTypeCollection))
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgParseOtherNotComplyToRule, innerAttribute.LineInfo);
        }
        #endregion

        #endregion
    }
}
