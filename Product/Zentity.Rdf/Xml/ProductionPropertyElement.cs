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
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#propertyElt production.
    /// </summary>
    internal class ProductionPropertyElement : Production
    {
        #region Member Variables

        #region Private
        protected EventElement innerElement;
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionPropertyElement class.
        /// The constructor takes an EventElement to initialize itself.
        /// </summary>
        /// <param name="nodeElement">EventElement object to process.</param>
        internal ProductionPropertyElement(EventElement nodeElement)
        {
            if (nodeElement == null)
                throw new ArgumentNullException(
                    string.Format(CultureInfo.InvariantCulture,
                    Resources.MsgNullArgument, "nodeElement"));

            this.innerElement = nodeElement;
        }
        #endregion

        #endregion

        #region Methods

        #region Internal
        /// <summary>
        /// Instantiate Property element object as per syntax rule and 
        /// calls Match method on it.
        /// </summary>
        /// <param name="outputGraph">Graph to add the generated triples.</param>
        internal override void Match(Graph outputGraph)
        {
            GetAlternativeProduction().Match(outputGraph);
        }
        #endregion

        #region Private
        /// <summary>
        /// resourcePropertyElt | literalPropertyElt | parseTypeLiteralPropertyElt | 
        /// parseTypeResourcePropertyElt | parseTypeCollectionPropertyElt | 
        /// parseTypeOtherPropertyElt | emptyPropertyElt
        /// </summary>
        /// <returns>Returns a Production object.</returns>
        private Production GetAlternativeProduction()
        {
            //If element e has e.URI = rdf:li then apply the list expansion rules 
            //on element e.parent to give a new URI u and e.URI := u. 
            if (innerElement.Uri == LiUri)
                innerElement.Uri = innerElement.Parent.ExpandList();

            if (innerElement.Attributes.Where(tuple => tuple.Uri == ParseTypeUri
                && tuple.StringValue == Constants.ParseTypeLiteral).Count() > 0)
                return new ProductionParseTypeLiteralPropertyElement(innerElement);

            else if (innerElement.Attributes.Where(tuple => tuple.Uri == ParseTypeUri
                && tuple.StringValue == Constants.ParseTypeResource).Count() > 0)
                return new ProductionParseTypeResourcePropertyElement(innerElement);

            else if (innerElement.Attributes.Where(tuple => tuple.Uri == ParseTypeUri
                && tuple.StringValue == Constants.ParseTypeCollection).Count() > 0)
                return new ProductionParseTypeCollectionPropertyElement(innerElement);

            else if (innerElement.Attributes.Where(tuple => tuple.Uri == ParseTypeUri
                && tuple.StringValue != Constants.ParseTypeResource && tuple.StringValue != Constants.ParseTypeLiteral
                && tuple.StringValue != Constants.ParseTypeCollection).Count() > 0)
                return new ProductionParseTypeOtherPropertyElement(innerElement);

            else if (innerElement.Children.Count() > 0)
                return new ProductionResourcePropertyElement(innerElement);

            else if (innerElement.Attributes.Where(tuple => tuple.Uri == DatatypeUri).Count() > 0
                || !string.IsNullOrEmpty(innerElement.StringValue.Trim()))
                return new ProductionLiteralPropertyElement(innerElement);

            else
                return new ProductionEmptyPropertyElement(innerElement);
        }
        #endregion

        #endregion
    }
}
