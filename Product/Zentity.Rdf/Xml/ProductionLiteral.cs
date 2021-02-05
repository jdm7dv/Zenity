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
    ///  This class represents the 
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#literal production.
    /// </summary>
    internal sealed class ProductionLiteral : Production
    {
        #region Memeber Variables

        #region Private
        string literal;
        const string ValidXml = "<ValidNode>{0}</ValidNode>";
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionLiteral class. 
        /// The constructor takes a literal value to initialize itself.
        /// </summary>
        /// <param name="stringValue">Literal value.</param>
        internal ProductionLiteral(string stringValue)
        {
            literal = stringValue;
        }
        #endregion

        #endregion

        #region Methods

        #region Internal
        /// <summary>
        /// Validates literal value.
        /// </summary>
        /// <param name="outputGraph">Graph to add the generated triples.</param>
        internal override void Match(Graph outputGraph)
        {
            try
            {
                using (StringReader reader = new StringReader(string.Format(CultureInfo.InvariantCulture, ValidXml, literal)))
                {
                    XDocument.Load(reader);
                }
            }
            catch (System.Xml.XmlException ex)
            {
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgLiteralInvalidValue, ex);
            }
        }
        #endregion

        #endregion
    }
}
