// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Rdf
{
    /// <summary>
    /// Constants class
    /// </summary>
    internal static class Constants
    {
        internal const string XmlBase = "base";
        internal const string XmlLang = "lang";
        internal const string ParseTypeCollection = "Collection";
        internal const string ParseTypeLiteral = "Literal";
        internal const string ParseTypeResource = "Resource";

        internal const string Description = "Description";
        internal const string RDF = "RDF";
        internal const string ID = "ID";
        internal const string Resource = "resource";
        internal const string NodeID = "nodeID";
        internal const string About = "about";
        internal const string Li = "li";
        internal const string Type = "type";
        internal const string ParseType = "parseType";
        internal const string Datatype = "datatype";
        internal const string First = "first";
        internal const string Rest = "rest";
        internal const string Nil = "nil";
        internal const string Statement = "Statement";
        internal const string Subject = "subject";
        internal const string Predicate = "predicate";
        internal const string Object = "object";
        internal const string AboutEach = "aboutEach";
        internal const string AboutEachPrefix = "aboutEachPrefix";
        internal const string BagID = "bagID";
        internal const string Property = "Property";
        internal const string List = "List";
        internal const string Bag = "Bag";
        internal const string Seq = "Seq";
        internal const string Alt = "Alt";
        internal const string Value = "value";
        internal const string XMLLiteral = "XMLLiteral";

        internal const string RdfNamespace = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        internal const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";
        internal const string DefaultBaseUriKey = "DefaultBaseUri";
        internal const string DefaultBaseUri = "http://www.Zentity.org/";

        #region Nested type: ErrorMessageIds

        /// <summary>
        /// Error Message Ids
        /// </summary>
        internal static class ErrorMessageIds
        {
            internal const string MsgDocNoRDFElement = "MsgDocNoRDFElement";
            internal const string MsgAboutAttributeUriNotMatch = "MsgAboutAttributeUriNotMatch";
            internal const string MsgDataTypeAttributeUriNotMatch = "MsgDataTypeAttributeUriNotMatch";
            internal const string MsgEmptyPropertyElementMoreThanOneIDAttr = "MsgEmptyPropertyElementMoreThanOneIDAttr";
            internal const string MsgEmptyPropertyElementNotComplyToRule = "MsgEmptyPropertyElementNotComplyToRule";
            internal const string MsgEventAttributeInvalidUri = "MsgEventAttributeInvalidUri";
            internal const string MsgEventElementInvalidUri = "MsgEventElementInvalidUri";
            internal const string MsgIdAttributeIdDefinedMoreThanOnce = "MsgIdAttributeIdDefinedMoreThanOnce";
            internal const string MsgIdAttributeUriNotMatch = "MsgIdAttributeUriNotMatch";
            internal const string MsgLiteralInvalidValue = "MsgLiteralInvalidValue";
            internal const string MsgLiteralPropertyElementIdOrDataTypeAttr = "MsgLiteralPropertyElementIdOrDataTypeAttr";
            internal const string MsgLiteralPropertyElementMoreThanOneDataTypeAttr = "MsgLiteralPropertyElementMoreThanOneDataTypeAttr";
            internal const string MsgLiteralPropertyElementMoreThanOneIdAttr = "MsgLiteralPropertyElementMoreThanOneIdAttr";
            internal const string MsgLiteralPropertyElementNoChildElements = "MsgLiteralPropertyElementNoChildElements";
            internal const string MsgNodeElementNotComplyToRule = "MsgNodeElementNotComplyToRule";
            internal const string MsgNodeElementUrisCoreTermsNotAllowed = "MsgNodeElementUrisCoreTermsNotAllowed";
            internal const string MsgNodeElementUrisInvalidTerm = "MsgNodeElementUrisInvalidTerm";
            internal const string MsgNodeElementUrisLiNotAllowed = "MsgNodeElementUrisLiNotAllowed";
            internal const string MsgNodeElementUrisOldTermsNotAllowed = "MsgNodeElementUrisOldTermsNotAllowed";
            internal const string MsgNodeElementUrisInvalidNumber = "MsgNodeElementUrisInvalidNumber";
            internal const string MsgNodeIdAttributeUriNotMatch = "MsgNodeIdAttributeUriNotMatch";
            internal const string MsgParseCollectionNotComplyToRule = "MsgParseCollectionNotComplyToRule";
            internal const string MsgParseLiteralNotComplyToRule = "MsgParseLiteralNotComplyToRule";
            internal const string MsgParseOtherNotComplyToRule = "MsgParseOtherNotComplyToRule";
            internal const string MsgParseResourceNotComplyToRule = "MsgParseResourceNotComplyToRule";
            internal const string MsgParseTypeCollectionPropertyElementMoreThanOneIdAttr = "MsgParseTypeCollectionPropertyElementMoreThanOneIdAttr";
            internal const string MsgParseTypeCollectionPropertyElementNoOtherAttrs = "MsgParseTypeCollectionPropertyElementNoOtherAttrs";
            internal const string MsgParseTypeCollectionPropertyElementSingleCollectionAttr = "MsgParseTypeCollectionPropertyElementSingleCollectionAttr";
            internal const string MsgParseTypeLiteralPropertyElementMoreThanOneIdAttr = "MsgParseTypeLiteralPropertyElementMoreThanOneIdAttr";
            internal const string MsgParseTypeLiteralPropertyElementNoOtherAttrs = "MsgParseTypeLiteralPropertyElementNoOtherAttrs";
            internal const string MsgParseTypeLiteralPropertyElementSingleParseLiteralAttr = "MsgParseTypeLiteralPropertyElementSingleParseLiteralAttr";
            internal const string MsgParseTypeOtherPropertyElementMoreThanOneIdAttr = "MsgParseTypeOtherPropertyElementMoreThanOneIdAttr";
            internal const string MsgParseTypeOtherPropertyElementNoOtherAttrs = "MsgParseTypeOtherPropertyElementNoOtherAttrs";
            internal const string MsgParseTypeOtherPropertyElementSingleParseOtherAttr = "MsgParseTypeOtherPropertyElementSingleParseOtherAttr";
            internal const string MsgParseTypeResourcePropertyElementMoreThanOneIdAttr = "MsgParseTypeResourcePropertyElementMoreThanOneIdAttr";
            internal const string MsgParseTypeResourcePropertyElementNoOtherAttrs = "MsgParseTypeResourcePropertyElementNoOtherAttrs";
            internal const string MsgParseTypeResourcePropertyElementSingleParseResourceAttr = "MsgParseTypeResourcePropertyElementSingleParseResourceAttr";
            internal const string MsgPropertyAttributeUrisCoreTermsNotAllowed = "MsgPropertyAttributeUrisCoreTermsNotAllowed";
            internal const string MsgPropertyAttributeUrisDescriptionNotAllowed = "MsgPropertyAttributeUrisDescriptionNotAllowed";
            internal const string MsgPropertyAttributeUrisInvalidNumber = "MsgPropertyAttributeUrisInvalidNumber";
            internal const string MsgPropertyAttributeUrisInvalidTerm = "MsgPropertyAttributeUrisInvalidTerm";
            internal const string MsgPropertyAttributeUrisLiNotAllowed = "MsgPropertyAttributeUrisLiNotAllowed";
            internal const string MsgPropertyAttributeUrisOldTermsNotAllowed = "MsgPropertyAttributeUrisOldTermsNotAllowed";
            internal const string MsgPropertyElementUrisCoreTermsNotAllowed = "MsgPropertyElementUrisCoreTermsNotAllowed";
            internal const string MsgPropertyElementUrisDescriptionNotAllowed = "MsgPropertyElementUrisDescriptionNotAllowed";
            internal const string MsgPropertyElementUrisInvalidNumber = "MsgPropertyElementUrisInvalidNumber";
            internal const string MsgPropertyElementUrisInvalidTerm = "MsgPropertyElementUrisInvalidTerm";
            internal const string MsgPropertyElementUrisOldTermsNotAllowed = "MsgPropertyElementUrisOldTermsNotAllowed";
            internal const string MsgRdfIdInvalidId = "MsgRdfIdInvalidId";
            internal const string MsgRdfInvalidRootElement = "MsgRdfInvalidRootElement";
            internal const string MsgRdfNoAttrAllowed = "MsgRdfNoAttrAllowed";
            internal const string MsgResourceAttributeUriNotMatch = "MsgResourceAttributeUriNotMatch";
            internal const string MsgResourcePropertyElementMoreThanOneIdAttr = "MsgResourcePropertyElementMoreThanOneIdAttr";
            internal const string MsgResourcePropertyElementNoOtherAttr = "MsgResourcePropertyElementNoOtherAttr";
            internal const string MsgResourcePropertyElementSingleChildElement = "MsgResourcePropertyElementSingleChildElement";
        }

        #endregion
    }
}