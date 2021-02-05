// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Xml
{
    using System;
    using System.Globalization;
    using System.Xml;
    using System.Xml.Linq;
    using Zentity.Rdf.Concepts;

    /// <summary>
    /// Represents the http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#section-attribute-node event.
    /// </summary>
    internal class EventAttribute
    {
        #region Member Variables

        #region Private
        XAttribute innerAttribute;
        RDFUriReference uri;
        EventElement parent;
        IXmlLineInfo lineInfo;
        #endregion

        #endregion

        #region Constructors

        #region Internal

        /// <summary>
        /// Initializes a new instance of the EventAttribute class. 
        /// The constructor takes an XAttribute to initialize itself. 
        /// The XAttribute is the implementation of “attribute information item” 
        /// referred by the W3C specification.
        /// </summary>
        /// <param name="attribute">Xml Attribute.</param>
        /// <param name="parent">Parent EventElement.</param>
        /// <exception cref="System.UriFormatException">Format of uriString is invalid.</exception>
        /// <exception cref="System.ArgumentNullException">Given XAttribute object is nill.</exception>
        internal EventAttribute(XAttribute attribute, EventElement parent)
        {
            if(attribute == null)
                throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, Resources.MsgNullArgument, "attribute"));

            innerAttribute = attribute;
            this.parent = parent;
            lineInfo = attribute as IXmlLineInfo;

            try
            {
                uri = new RDFUriReference(innerAttribute.Name.NamespaceName + innerAttribute.Name.LocalName);

                if (!uri.InnerUri.IsWellFormedOriginalString())
                    throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgEventAttributeInvalidUri, LineInfo);
            }
            catch (UriFormatException ex)
            {
                throw new RdfXmlParserException(ex, Constants.ErrorMessageIds.MsgEventAttributeInvalidUri, LineInfo);
            }
        }

        #endregion

        #endregion

        #region Properties

        #region Internal

        /// <summary>
        /// Gets Uri of the Attribute.
        /// </summary>
        internal RDFUriReference Uri
        {
            get
            {
                return uri;
            }
        }

        /// <summary>
        /// Gets value set to the Attribute.
        /// </summary>
        internal string StringValue
        {
            get
            {
                return innerAttribute.Value;
            }
        }

        /// <summary>
        /// Gets parent element of the attribute.
        /// </summary>
        internal EventElement Parent
        {
            get { return parent;}
        }

        /// <summary>
        /// Gets line information of the element in the RDF Xml.
        /// </summary>
        internal string LineInfo
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, Resources.LineInfo, lineInfo.LineNumber, lineInfo.LinePosition);
            }
        }
        #endregion

        #endregion
    }
}
