// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Xml
{
    using System.Globalization;
    using System.IO;
    using System.Xml.Linq;
    using Zentity.Rdf.Concepts;

    /// <summary>
    /// This class represents the 
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#rdf-id production.
    /// </summary>
    internal sealed class ProductionRdfId : Production
    {

        #region Memeber Variables

        #region Private
        string rdfIdString;
        const string ValidXml = "<ValidRoot  xmlns:{0}=\"http:a-valid-url\"/>";
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionRdfId class.
        /// The constructor takes a Rdf Id value to initialize itself.
        /// </summary>
        /// <param name="stringValue">Rdf Id value</param>
        internal ProductionRdfId(string stringValue)
        {
            rdfIdString = stringValue;
        }
        #endregion

        #endregion

        #region Methods

        #region Internal
        /// <summary>
        /// Validates Rdf Id value.
        /// </summary>
        /// <param name="outputGraph">Graph to add the generated triples.</param>
        internal override void Match(Graph outputGraph)
        {
            try
            {
                string testXml = string.Format(CultureInfo.InvariantCulture, ValidXml, rdfIdString);
                using (StringReader reader = new StringReader(testXml))
                {
                    XDocument.Load(reader);
                }
            }
            catch (System.Xml.XmlException ex)
            {
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgRdfIdInvalidId, ex);
            }
        }
        #endregion

        #endregion

    }
}
