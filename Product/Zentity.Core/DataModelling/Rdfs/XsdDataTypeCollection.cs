// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Schema;
using Zentity.Rdf.Concepts;
using Zentity.Core;
using System.Globalization;
using System.Xml.Linq;
using System.Xml;

namespace Zentity.Core
{
    /// <summary>
    /// This class parses xsd file and maintains collection of XsdDataType object.
    /// </summary>
    internal class XsdDataTypeCollection : IEnumerable<XsdDataType>
    {
        #region Variables

        #region Private
        private List<XsdDataType> dataTypeColleciton = new List<XsdDataType>();
        private static List<RDFUriReference> supportedDataTypes;
        private static RDFUriReference integerUri = new RDFUriReference(DataModellingResources.xsdNameSpace + "integer");
        private static RDFUriReference hexBinaryUri = new RDFUriReference(DataModellingResources.xsdNameSpace + "hexBinary");
        private static RDFUriReference booleanUri = new RDFUriReference(DataModellingResources.xsdNameSpace + "boolean");
        private static RDFUriReference byteUri = new RDFUriReference(DataModellingResources.xsdNameSpace + "byte");
        private static RDFUriReference dateTimeUri = new RDFUriReference(DataModellingResources.xsdNameSpace + "dateTime");
        private static RDFUriReference decimalUri = new RDFUriReference(DataModellingResources.xsdNameSpace + "decimal");
        private static RDFUriReference doubleUri = new RDFUriReference(DataModellingResources.xsdNameSpace + "double");
        private static RDFUriReference shortUri = new RDFUriReference(DataModellingResources.xsdNameSpace + "short");
        private static RDFUriReference longUri = new RDFUriReference(DataModellingResources.xsdNameSpace + "long");
        private static RDFUriReference stringUri = new RDFUriReference(DataModellingResources.xsdNameSpace + "string");
        #endregion

        #endregion

        #region Propetries

        #region Internal
        /// <summary>
        /// Gets list of supported xsd data types.
        /// </summary>
        internal static IEnumerable<RDFUriReference> SupportedDataTypes
        {
            get
            {
                if (supportedDataTypes == null)
                {
                    supportedDataTypes = new List<RDFUriReference>();
                    supportedDataTypes.Add(integerUri);
                    supportedDataTypes.Add(hexBinaryUri);
                    supportedDataTypes.Add(booleanUri);
                    supportedDataTypes.Add(byteUri);
                    supportedDataTypes.Add(dateTimeUri);
                    supportedDataTypes.Add(decimalUri);
                    supportedDataTypes.Add(doubleUri);
                    supportedDataTypes.Add(shortUri);
                    supportedDataTypes.Add(longUri);
                    supportedDataTypes.Add(stringUri);
                }

                return supportedDataTypes;
            }
        }
        #endregion

        #endregion

        #region Constructors

        #region Internal

        /// <summary>
        /// Initializes a new instance of the XsdDataTypeCollection class.
        /// </summary>
        internal XsdDataTypeCollection()
        {
            this.AddSupportedXsdDataTypes();
        }

        /// <summary>
        /// Initializes a new instance of the XsdDataTypeCollection class with 
        /// a specified XDocument intance.
        /// </summary>
        /// <param name="xsdDocument">Instance of XDocument class.</param>
        internal XsdDataTypeCollection(XDocument xsdDocument)
        {
            this.AddSupportedXsdDataTypes();
            this.AddUserDefinedXsdDataTypes(xsdDocument.CreateReader());
        }

        #endregion

        #endregion

        #region Methods

        #region Private

        /// <summary>
        /// Adds the supported XSD data types.
        /// </summary>
        private void AddSupportedXsdDataTypes()
        {
            //For each supported datatype, create new instance of XsdDataType 
            //and add to datatype colleciton.
            foreach (RDFUriReference supportType in SupportedDataTypes)
            {
                XsdDataType xsdType = new XsdDataType
                {
                    Name = supportType,
                    BaseType = GetDataType(supportType)
                };
                dataTypeColleciton.Add(xsdType);
            }
        }

        /// <summary>
        /// Adds the user defined XSD data types.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        private void AddUserDefinedXsdDataTypes(XmlReader xmlReader)
        {
            //Read xsd file.
            XmlSchema xsd = XmlSchema.Read(xmlReader, null);

            //Validate xml schema.
            ValidateXmlSchema(xsd);

            foreach (XmlSchemaSimpleType xsdtypes in xsd.Items.OfType<XmlSchemaSimpleType>())
            {
                XsdDataType xsdType = null;

                string namespaceScope = xsd.TargetNamespace;

                foreach (XmlQualifiedName localNsDefinition in xsdtypes.Namespaces.ToArray())
                {
                    if (localNsDefinition.Name == string.Empty)
                    {
                        namespaceScope = localNsDefinition.Namespace;
                    }
                }

                //Create instance of XsdDataType.
                xsdType = new XsdDataType(
                    new RDFUriReference(namespaceScope + xsdtypes.Name));

                XmlSchemaSimpleTypeRestriction restType = xsdtypes.Content as XmlSchemaSimpleTypeRestriction;
                if (restType != null)
                {
                    //Set BaseType property
                    xsdType.BaseType = GetDataType(new RDFUriReference(
                        restType.BaseTypeName.Namespace + "#" + restType.BaseTypeName.Name));
                    //Set MaxLength property
                    XmlSchemaFacet facet = restType.Facets.OfType<XmlSchemaMaxLengthFacet>()
                        .FirstOrDefault();
                    if (facet != null && !string.IsNullOrEmpty(facet.Value))
                        xsdType.MaxLength = Convert.ToInt32(facet.Value,
                            CultureInfo.InvariantCulture);
                    //Set Precistion property.                    
                    facet = restType.Facets.OfType<XmlSchemaTotalDigitsFacet>().FirstOrDefault();
                    if (facet != null && !string.IsNullOrEmpty(facet.Value))
                        xsdType.Precision = Convert.ToInt32(facet.Value,
                            CultureInfo.InvariantCulture);
                    //Set Scale property.                    
                    facet = restType.Facets.OfType<XmlSchemaFractionDigitsFacet>().FirstOrDefault();
                    if (facet != null && !string.IsNullOrEmpty(facet.Value))
                        xsdType.Scale = Convert.ToInt32(facet.Value, CultureInfo.InvariantCulture);

                    dataTypeColleciton.Add(xsdType);
                }
            }
        }

        /// <summary>
        /// Validates the XML schema.
        /// </summary>
        /// <param name="xsd">The XSD schema.</param>
        private static void ValidateXmlSchema(XmlSchema xsd)
        {
            if (xsd != null)
            {
                if (xsd.Items.OfType<XmlSchemaObject>().Where(tuple =>
                    tuple.GetType() != typeof(XmlSchemaSimpleType)).Count() > 0)
                    throw new RdfsException(DataModellingResources.MsgXsdDataTypeSimpleTypeSupported);

                foreach (XmlSchemaSimpleType xsdTypes in xsd.Items.OfType<XmlSchemaSimpleType>())
                {
                    XmlSchemaSimpleTypeRestriction restType =
                        xsdTypes.Content as XmlSchemaSimpleTypeRestriction;
                    if (restType != null)
                    {
                        //Validate facets
                        if (restType.Facets.OfType<XmlSchemaFacet>().Where(tuple =>
                            tuple.GetType() != typeof(XmlSchemaMaxLengthFacet) &&
                            tuple.GetType() != typeof(XmlSchemaFractionDigitsFacet) &&
                            tuple.GetType() != typeof(XmlSchemaTotalDigitsFacet)).Count() > 0)
                            throw new RdfsException(DataModellingResources.MsgXsdDataTypeFacetNotSupported);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the type of the data.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        /// <returns>The DataType enum value</returns>
        private static DataTypes GetDataType(RDFUriReference dataType)
        {
            DataTypes type = DataTypes.String;

            if (dataType == integerUri) type = DataTypes.Int32;
            else if (dataType == hexBinaryUri) type = DataTypes.Binary;
            else if (dataType == booleanUri) type = DataTypes.Boolean;
            else if (dataType == byteUri) type = DataTypes.Byte;
            else if (dataType == dateTimeUri) type = DataTypes.DateTime;
            else if (dataType == decimalUri) type = DataTypes.Decimal;
            else if (dataType == doubleUri) type = DataTypes.Double;
            else if (dataType == shortUri) type = DataTypes.Int16;
            else if (dataType == longUri) type = DataTypes.Int64;
            else if (dataType == stringUri) type = DataTypes.String;

            return type;
        }

        #endregion

        #endregion

        #region IEnumerable<XsdDataType> Members

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The XsdDataType enumerator.</returns>
        public IEnumerator<XsdDataType> GetEnumerator()
        {
            return dataTypeColleciton.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return dataTypeColleciton.GetEnumerator();
        }

        #endregion
    }
}
