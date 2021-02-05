// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Xml
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Zentity.Rdf.Concepts;

    /// <summary>
    /// This class represents the 
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#propertyElementURIs production.
    /// </summary>
    internal sealed class ProductionPropertyElementUris : Production
    {
        #region Member Variables

        #region Private
        RDFUriReference innerName;
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionPropertyElementUris class.
        /// The constructor takes a RDFUriReference to initialize itself.
        /// </summary>
        /// <param name="uri">RDFUriReference object to process.</param>
        internal ProductionPropertyElementUris(RDFUriReference uri)
        {
            if (uri == null)
                throw new ArgumentNullException(
                    string.Format(CultureInfo.InvariantCulture,
                    Resources.MsgNullArgument, "uri"));

            innerName = uri;
        }
        #endregion

        #endregion

        #region Methods

        #region Internal
        /// <summary>
        /// Validates property element uri based on following syntax rules, 
        /// <br/>
        /// anyURI - ( coreSyntaxTerms | rdf:Description | oldTerms ) 
        /// </summary>
        /// <param name="outputGraph">Graph to add the generated triples.</param>
        internal override void Match(Graph outputGraph)
        {
            if (Production.CoreSyntaxTerms.
                Where(tuple => tuple == innerName).Count() > 0)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgPropertyElementUrisCoreTermsNotAllowed);

            if (innerName == DescriptionUri)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgPropertyElementUrisDescriptionNotAllowed);

            if (Production.OldTerms.
                Where(tuple => tuple == innerName).Count() > 0)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgPropertyElementUrisOldTermsNotAllowed);

            //Handling the case where propertyElementUris = rdf:_n
            //where n is a decimal integer greater than zero with no leading zeros.
            if (innerName.InnerUri.AbsoluteUri.StartsWith(Constants.RdfNamespace + "_", StringComparison.Ordinal))
            {
                string n = innerName.InnerUri.AbsoluteUri.Split('_')[1];
                int i = 0;
                //If parse operation failed or parsed value is less than or equal to zero
                //then throw exception.
                if (n.StartsWith("0", StringComparison.Ordinal) || !int.TryParse(n, out i) || i <= 0)
                {
                    throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgPropertyElementUrisInvalidNumber);
                }
            }
            else if (innerName.InnerUri.AbsoluteUri.StartsWith(Constants.RdfNamespace, StringComparison.Ordinal) &&
                Production.AllRdfTerms.Where(tuple => tuple == innerName).Count() == 0)
                throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgPropertyAttributeUrisInvalidTerm);
        }
        #endregion

        #endregion
    }
}
