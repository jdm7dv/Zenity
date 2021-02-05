// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System.Xml;
using System.Globalization;
using Zentity.Core;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System;

namespace Zentity.Core
{
    /// <summary>
    /// This class defines the methods that are used in data modelling and building entity framework metadata xml elements.
    /// </summary>
    internal static partial class Utilities
    {
        /// <summary>
        /// Adds the attribute.
        /// </summary>
        /// <param name="xmlElement">The XML element.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        internal static void AddAttribute(XmlElement xmlElement, string attributeName, string attributeValue)
        {
            XmlAttribute xmlAttribute = xmlElement.OwnerDocument.CreateAttribute(attributeName);
            xmlAttribute.Value = attributeValue;
            xmlElement.Attributes.Append(xmlAttribute);
        }

        /// <summary>
        /// Adds the attribute.
        /// </summary>
        /// <param name="xmlElement">The XML element.</param>
        /// <param name="namespaceUri">The namespace URI.</param>
        /// <param name="qualifiedName">The qualified name.</param>
        /// <param name="attributeValue">The attribute value.</param>
        internal static void AddAttribute(XmlElement xmlElement, string namespaceUri, string qualifiedName, string attributeValue)
        {
            XmlAttribute xmlAttribute = xmlElement.OwnerDocument.CreateAttribute(qualifiedName, namespaceUri);
            xmlAttribute.Value = attributeValue;
            xmlElement.Attributes.Append(xmlAttribute);
        }

        /// <summary>
        /// Adds the defining query element.
        /// </summary>
        /// <param name="xmlElement">The XML element.</param>
        /// <param name="definingQueryBody">The defining query body.</param>
        internal static void AddDefiningQueryElement(XmlElement xmlElement, string definingQueryBody)
        {
            XmlElement definingQuery = CreateElement(xmlElement, DataModellingResources.DefiningQuery);
            if (!string.IsNullOrWhiteSpace(definingQueryBody))
            {
                definingQuery.InnerXml = definingQueryBody;
            }
        }

        /// <summary>
        /// Adds the CSDL association referential constraint.
        /// </summary>
        /// <param name="associationElement">The association element.</param>
        /// <param name="principalRole">The principal role.</param>
        /// <param name="principalProperty">The principal property.</param>
        /// <param name="dependentRole">The dependent role.</param>
        /// <param name="dependentProperty">The dependent property.</param>
        internal static void AddCsdlAssociationReferentialConstraint(XmlElement associationElement, string principalRole, string principalProperty, string dependentRole, string dependentProperty)
        {
            // Create AssociationSet element.
            XmlElement refConstraint = CreateElement(associationElement, DataModellingResources.ReferentialConstraint);

            // Create Principal element.
            XmlElement principal = CreateElement(refConstraint, DataModellingResources.Principal);

            // Create Principal Role attribute.
            AddAttribute(principal, DataModellingResources.Role, principalRole);

            // Create the Principal's PropertyRef element
            XmlElement principalPropRef = CreateElement(principal, DataModellingResources.PropertyRef);

            // Create Name attribute on PropertyRef element.
            AddAttribute(principalPropRef, DataModellingResources.Name, principalProperty);

            // Create Dependent element.
            XmlElement dependent = CreateElement(refConstraint, DataModellingResources.Dependent);

            // Create Dependent Role attribute.
            AddAttribute(dependent, DataModellingResources.Role, dependentRole);

            // Create the Dependent's PropertyRef element
            XmlElement dependentPropRef = CreateElement(dependent, DataModellingResources.PropertyRef);

            // Create Name attribute on PropertyRef element.
            AddAttribute(dependentPropRef, DataModellingResources.Name, dependentProperty);
        }

        /// <summary>
        /// Adds the CSDL association set.
        /// </summary>
        /// <param name="entityContainerElement">The entity container element.</param>
        /// <param name="associationSetName">Name of the association set.</param>
        /// <param name="associationFullName">Full name of the association.</param>
        /// <param name="end1Role">The first end role.</param>
        /// <param name="end1EntitySet">The first end entity set.</param>
        /// <param name="end2Role">The second end role.</param>
        /// <param name="end2EntitySet">The second end entity set.</param>
        /// <returns>The xml element with the added item</returns>
        internal static XmlElement AddCsdlAssociationSet(XmlElement entityContainerElement, string associationSetName, string associationFullName, string end1Role, string end1EntitySet, string end2Role, string end2EntitySet)
        {
            // Create AssociationSet element.
            XmlElement associationSetElement = CreateElement(entityContainerElement, DataModellingResources.AssociationSet);

            // Create Name attribute.
            AddAttribute(associationSetElement, DataModellingResources.Name, associationSetName);

            // Create Association attribute.
            AddAttribute(associationSetElement, DataModellingResources.Association, associationFullName);

            // Create first End element.
            XmlElement end1 = CreateElement(associationSetElement, DataModellingResources.End);

            // Create first Role attribute.
            AddAttribute(end1, DataModellingResources.Role, end1Role);

            // Create first EntitySet attribute.
            AddAttribute(end1, DataModellingResources.EntitySet, end1EntitySet);

            // Create second End element.
            XmlElement end2 = CreateElement(associationSetElement, DataModellingResources.End);

            // Create second Role attribute.
            AddAttribute(end2, DataModellingResources.Role, end2Role);

            // Create second EntitySet attribute.
            AddAttribute(end2, DataModellingResources.EntitySet, end2EntitySet);

            return associationSetElement;
        }

        /// <summary>
        /// Adds the CSDL entity set.
        /// </summary>
        /// <param name="entityContainerElement">The entity container element.</param>
        /// <param name="entitySetName">Name of the entity set.</param>
        /// <param name="entityTypeName">Name of the entity type.</param>
        internal static void AddCsdlEntitySet(XmlElement entityContainerElement, string entitySetName, string entityTypeName)
        {
            // Create EntitySet element.
            XmlElement entitySetElement = CreateElement(entityContainerElement, DataModellingResources.EntitySet);
            AddAttribute(entitySetElement, DataModellingResources.Name, entitySetName);
            AddAttribute(entitySetElement, DataModellingResources.EntityType, entityTypeName);

            entityContainerElement.AppendChild(entitySetElement);
        }

        /// <summary>
        /// Creates a CSDL FunctionImport element.
        /// </summary>
        /// <param name="entityContainerElement">The entity container element</param>
        /// <param name="functionImportName">The function import name</param>
        /// <param name="parameterDetails">String triples of {Name, ManagerType, Mode}</param>
        /// <returns>The xml element with the added item</returns>
        internal static XmlElement AddCsdlFunctionImport(XmlElement entityContainerElement, string functionImportName, params string[] parameterDetails)
        {
            // Create FunctionImport element.
            XmlElement functionImportElement = CreateElement(entityContainerElement, DataModellingResources.FunctionImport);
            AddAttribute(functionImportElement, DataModellingResources.Name, functionImportName);

            // Create Parameter elements.
            if (parameterDetails != null)
            {
                for (int i = 0; i < parameterDetails.Length; i += 3)
                {
                    XmlElement parameterElement = CreateElement(functionImportElement, DataModellingResources.Parameter);
                    AddAttribute(parameterElement, DataModellingResources.Name, parameterDetails[i]);
                    AddAttribute(parameterElement, DataModellingResources.Type, parameterDetails[i + 1]);
                    AddAttribute(parameterElement, DataModellingResources.Mode, parameterDetails[i + 2]);
                }
            }

            return functionImportElement;
        }

        /// <summary>
        /// Adds the MSL FunctionImport mapping element.
        /// </summary>
        /// <param name="entityContainerMappingElement">The entity container mapping element.</param>
        /// <param name="functionImportName">Name of the function import.</param>
        /// <param name="functionName">Name of the function.</param>
        internal static void AddMslFunctionImportMapping(XmlElement entityContainerMappingElement, string functionImportName, string functionName)
        {
            XmlElement eFunctionImportMapping = Utilities.CreateElement(
                entityContainerMappingElement, DataModellingResources.FunctionImportMapping);

            Utilities.AddAttribute(eFunctionImportMapping,
                DataModellingResources.FunctionImportName, functionImportName);

            Utilities.AddAttribute(eFunctionImportMapping,
                DataModellingResources.FunctionName, functionName);
        }

        /// <summary>
        /// Adds the SSDL association.
        /// </summary>
        /// <param name="schemaElement">The schema element.</param>
        /// <param name="associationName">Name of the association.</param>
        /// <param name="end1Role">The first end role.</param>
        /// <param name="end1Type">The first end type.</param>
        /// <param name="end1Multiplicity">The first end multiplicity.</param>
        /// <param name="end2Role">The second end role.</param>
        /// <param name="end2Type">The second end type .</param>
        /// <param name="end2Multiplicity">The second end multiplicity.</param>
        /// <param name="principalRole">The principal role.</param>
        /// <param name="principalKeyColumns">The principal key columns.</param>
        /// <param name="dependentRole">The dependent role.</param>
        /// <param name="dependentKeyColumns">The dependent key columns.</param>
        /// <returns>The xml element with the added item</returns>
        internal static XmlElement AddSsdlAssociation(XmlElement schemaElement, string associationName, string end1Role, string end1Type, string end1Multiplicity, string end2Role, string end2Type, string end2Multiplicity, string principalRole, string[] principalKeyColumns, string dependentRole, string[] dependentKeyColumns)
        {
            // Create Association element.
            XmlElement associationElement = CreateElement(schemaElement, DataModellingResources.Association);
            AddAttribute(associationElement, DataModellingResources.Name, associationName);

            // Add first End element.
            XmlElement end1 = CreateElement(associationElement, DataModellingResources.End);
            AddAttribute(end1, DataModellingResources.Role, end1Role);
            AddAttribute(end1, DataModellingResources.Type, end1Type);
            AddAttribute(end1, DataModellingResources.Multiplicity, end1Multiplicity);

            // Add second End element.
            XmlElement end2 = CreateElement(associationElement, DataModellingResources.End);
            AddAttribute(end2, DataModellingResources.Role, end2Role);
            AddAttribute(end2, DataModellingResources.Type, end2Type);
            AddAttribute(end2, DataModellingResources.Multiplicity, end2Multiplicity);

            // Add ReferentialConstraint element.
            XmlElement referentialConstraintElement = CreateElement(associationElement, DataModellingResources.ReferentialConstraint);

            // Add Principal element.
            XmlElement principalElement = CreateElement(referentialConstraintElement, DataModellingResources.Principal);
            AddAttribute(principalElement, DataModellingResources.Role, principalRole);

            // Add PropertyRef elements for principal.
            foreach (string keyColumn in principalKeyColumns)
            {
                XmlElement propertyRefElement = CreateElement(principalElement, DataModellingResources.PropertyRef);
                AddAttribute(propertyRefElement, DataModellingResources.Name, keyColumn);
            }

            // Add Dependent element.
            XmlElement dependentElement = CreateElement(referentialConstraintElement, DataModellingResources.Dependent);
            AddAttribute(dependentElement, DataModellingResources.Role, dependentRole);

            // Add PropertyRef elements for dependent.
            foreach (string keyColumn in dependentKeyColumns)
            {
                XmlElement propertyRefElement = CreateElement(dependentElement, DataModellingResources.PropertyRef);
                AddAttribute(propertyRefElement, DataModellingResources.Name, keyColumn);
            }

            return associationElement;
        }

        /// <summary>
        /// Adds the SSDL association set.
        /// </summary>
        /// <param name="entityContainerElement">The entity container element.</param>
        /// <param name="associationSetName">Name of the association set.</param>
        /// <param name="associationFullName">Full name of the association.</param>
        /// <param name="end1Role">The first end role.</param>
        /// <param name="end1EntitySet">The first end entity set.</param>
        /// <param name="end2Role">The second end role.</param>
        /// <param name="end2EntitySet">The second entity set.</param>
        /// <returns>The xml element with the added item</returns>
        internal static XmlElement AddSsdlAssociationSet(XmlElement entityContainerElement, string associationSetName, string associationFullName, string end1Role, string end1EntitySet, string end2Role, string end2EntitySet)
        {
            // Create AssociationSet element.
            XmlElement associationSetElement = CreateElement(entityContainerElement, DataModellingResources.AssociationSet);

            // Create Name attribute.
            AddAttribute(associationSetElement, DataModellingResources.Name, associationSetName);

            // Create Association attribute.
            AddAttribute(associationSetElement, DataModellingResources.Association, associationFullName);

            // Create first End element.
            XmlElement end1 = CreateElement(associationSetElement, DataModellingResources.End);

            // Create first Role attribute.
            AddAttribute(end1, DataModellingResources.Role, end1Role);

            // Create first EntitySet attribute.
            AddAttribute(end1, DataModellingResources.EntitySet, end1EntitySet);

            // Create second End element.
            XmlElement end2 = CreateElement(associationSetElement, DataModellingResources.End);

            // Create second Role attribute.
            AddAttribute(end2, DataModellingResources.Role, end2Role);

            // Create second EntitySet attribute.
            AddAttribute(end2, DataModellingResources.EntitySet, end2EntitySet);

            return associationSetElement;
        }

        /// <summary>
        /// Adds the SSDL entity set with defining query.
        /// </summary>
        /// <param name="entityContainerElement">The entity container element.</param>
        /// <param name="entitySetName">Name of the entity set.</param>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="definingQueryBody">The defining query body.</param>
        /// <returns>The xml element with the added item</returns>
        internal static XmlElement AddSsdlEntitySetWithDefiningQuery(XmlElement entityContainerElement, string entitySetName, string entityType, string definingQueryBody)
        {
            // Create EntitySet element.
            XmlElement entitySetElement = CreateElement(entityContainerElement, DataModellingResources.EntitySet);

            // Create Name attribute.
            AddAttribute(entitySetElement, DataModellingResources.Name, entitySetName);

            // Create EntityType attribute.
            AddAttribute(entitySetElement, DataModellingResources.EntityType, entityType);

            // Create the Defining Query element
            AddDefiningQueryElement(entitySetElement, definingQueryBody);

            return entitySetElement;
        }

        /// <summary>
        /// Adds the SSDL entity set for tables.
        /// </summary>
        /// <param name="entityContainerElement">The entity container element.</param>
        /// <param name="entitySetName">Name of the entity set.</param>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="schema">The schema.</param>
        /// <returns>The xml element with the added item</returns>
        internal static XmlElement AddSsdlEntitySetForTables(XmlElement entityContainerElement, string entitySetName, string entityType, string schema)
        {
            // Create EntitySet element.
            XmlElement entitySetElement = CreateElement(entityContainerElement, DataModellingResources.EntitySet);

            // Create Name attribute.
            AddAttribute(entitySetElement, DataModellingResources.Name, entitySetName);

            // Create EntityType attribute.
            AddAttribute(entitySetElement, DataModellingResources.EntityType, entityType);

            // Add store:ManagerType attribute.
            AddAttribute(entitySetElement, "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator",
                "store:ManagerType", "Tables");

            // Add Schema attribute.
            AddAttribute(entitySetElement, DataModellingResources.Schema, schema);

            return entitySetElement;
        }

        /// <summary>
        /// Adds the SSDL entity set for views.
        /// </summary>
        /// <param name="entityContainerElement">The entity container element.</param>
        /// <param name="entitySetName">Name of the entity set.</param>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="storeSchema">The store schema.</param>
        /// <param name="storeName">Name of the store.</param>
        /// <returns>The xml element with the added item</returns>
        internal static XmlElement AddSsdlEntitySetForViews(XmlElement entityContainerElement, string entitySetName, string entityType, string storeSchema, string storeName)
        {
            // Create EntitySet element.
            XmlElement entitySetElement = CreateElement(entityContainerElement, DataModellingResources.EntitySet);

            // Create Name attribute.
            AddAttribute(entitySetElement, DataModellingResources.Name, entitySetName);

            // Create EntityType attribute.
            AddAttribute(entitySetElement, DataModellingResources.EntityType, entityType);

            // Add store:Type attribute.
            AddAttribute(entitySetElement, "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator",
                "store:Type", "Views");

            // Add store:Schema attribute.
            AddAttribute(entitySetElement, "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator",
                "store:Schema", storeSchema);

            // Add store:Name attribute.
            AddAttribute(entitySetElement, "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator",
                "store:Name", storeName);

            return entitySetElement;
        }

        /// <summary>
        /// Adds the type of the SSDL entity.
        /// </summary>
        /// <param name="schemaElement">The schema element.</param>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="keyProperties">The key properties.</param>
        /// <returns>The xml element with the added item</returns>
        internal static XmlElement AddSsdlEntityType(XmlElement schemaElement, string entityTypeName, params string[] keyProperties)
        {
            // NOTE: We do not add Property elements here.

            // Create EntityType element.
            XmlElement entityTypeElement = CreateElement(schemaElement, DataModellingResources.EntityType);

            // Add Name attribute.
            AddAttribute(entityTypeElement, DataModellingResources.Name, entityTypeName);

            // Add Key element.
            XmlElement keyElement = CreateElement(entityTypeElement, DataModellingResources.Key);

            // Add PropertyRef elements.
            foreach (string keyPropertyName in keyProperties)
            {
                XmlElement propertyRefElement = CreateElement(keyElement, DataModellingResources.PropertyRef);
                AddAttribute(propertyRefElement, DataModellingResources.Name, keyPropertyName);
            }

            return entityTypeElement;
        }

        /// <summary>
        /// Adds the SSDL entity type property.
        /// </summary>
        /// <param name="entityTypeElement">The entity type element.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="nullable">if set to <c>true</c> [nullable].</param>
        /// <returns>The xml element with the added item</returns>
        internal static XmlElement AddSsdlEntityTypeProperty(XmlElement entityTypeElement, string name, string type, bool nullable)
        {
            // Create Property element.
            XmlElement propertyElement = CreateElement(entityTypeElement, DataModellingResources.Property);

            // Add Name attribute.
            AddAttribute(propertyElement, DataModellingResources.Name, name);

            // Add ManagerType attribute.
            AddAttribute(propertyElement, DataModellingResources.Type, type);
            // Add Nullable attribute.
            AddAttribute(propertyElement, DataModellingResources.Nullable, nullable.ToString().ToLowerInvariant());

            return propertyElement;
        }

        /// <summary>
        /// Adds the SSDL entity type property.
        /// </summary>
        /// <param name="entityTypeElement">The entity type element.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="nullable">if set to <c>true</c> [nullable].</param>
        /// <param name="maxLength">Length of the max.</param>
        /// <returns>The xml element with the added item</returns>
        internal static XmlElement AddSsdlEntityTypeProperty(XmlElement entityTypeElement, string name, string type, bool nullable, int maxLength)
        {
            XmlElement propertyElement = AddSsdlEntityTypeProperty(entityTypeElement, name, type,
                nullable);

            // Add MaxLength attribute.
            AddAttribute(propertyElement, DataModellingResources.MaxLength,
                maxLength.ToString(CultureInfo.InvariantCulture));

            return propertyElement;
        }

        /// <summary>
        /// Adds the SSDL entity type property.
        /// </summary>
        /// <param name="entityTypeElement">The entity type element.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="nullable">if set to <c>true</c> [nullable].</param>
        /// <param name="precision">The precision.</param>
        /// <param name="scale">The scale.</param>
        /// <returns>The xml element with the added item</returns>
        internal static XmlElement AddSsdlEntityTypeProperty(XmlElement entityTypeElement, string name, string type, bool nullable, int precision, int scale)
        {
            XmlElement propertyElement = AddSsdlEntityTypeProperty(entityTypeElement, name,
                type, nullable);

            // Add Precision attribute.
            AddAttribute(propertyElement, DataModellingResources.Precision,
                precision.ToString(CultureInfo.InvariantCulture));

            // Add Scale attribute.
            AddAttribute(propertyElement, DataModellingResources.Scale,
                scale.ToString(CultureInfo.InvariantCulture));

            return propertyElement;
        }

        /// <summary>
        /// Adds the SSDL function.
        /// </summary>
        /// <param name="schemaElement">The schema element.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="aggregate">if set to <c>true</c> the function is aggregate.</param>
        /// <param name="builtIn">if set to <c>true</c> the function is built in.</param>
        /// <param name="niladic">if set to <c>true</c> the function is niladic.</param>
        /// <param name="composable">if set to <c>true</c> the function is composable.</param>
        /// <param name="parameterTypeSemantics">The parameter type semantics.</param>
        /// <param name="schema">The schema.</param>
        /// <param name="parameterDetails">The parameter details.</param>
        /// <returns>The xml element with the added item</returns>
        internal static XmlElement AddSsdlFunction(XmlElement schemaElement, string functionName, bool aggregate, bool builtIn, bool niladic, bool composable, string parameterTypeSemantics, string schema, params string[] parameterDetails)
        {
            // Create Function element.
            XmlElement functionElement = CreateElement(schemaElement, DataModellingResources.Function);
            AddAttribute(functionElement, DataModellingResources.Name, functionName);
            AddAttribute(functionElement, DataModellingResources.Aggregate, aggregate.ToString().ToLowerInvariant());
            AddAttribute(functionElement, DataModellingResources.BuiltIn, builtIn.ToString().ToLowerInvariant());
            AddAttribute(functionElement, DataModellingResources.NiladicFunction, niladic.ToString().ToLowerInvariant());
            AddAttribute(functionElement, DataModellingResources.IsComposable, composable.ToString().ToLowerInvariant());
            AddAttribute(functionElement, DataModellingResources.ParameterTypeSemantics, parameterTypeSemantics);
            AddAttribute(functionElement, DataModellingResources.Schema, schema);

            // Create Parameter elements.
            if (parameterDetails != null)
            {
                for (int i = 0; i < parameterDetails.Length; i += 3)
                {
                    XmlElement parameterElement = CreateElement(functionElement, DataModellingResources.Parameter);
                    AddAttribute(parameterElement, DataModellingResources.Name, parameterDetails[i]);
                    AddAttribute(parameterElement, DataModellingResources.Type, parameterDetails[i + 1]);
                    AddAttribute(parameterElement, DataModellingResources.Mode, parameterDetails[i + 2]);
                }
            }

            return functionElement;
        }

        /// <summary>
        /// Creates the element.
        /// </summary>
        /// <param name="parentXmlElement">The parent XML element.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <returns>The xml element with the added item</returns>
        internal static XmlElement CreateElement(XmlElement parentXmlElement, string elementName)
        {
            XmlElement xmlElement = parentXmlElement.OwnerDocument.
                CreateElement(elementName, parentXmlElement.NamespaceURI);
            parentXmlElement.AppendChild(xmlElement);
            return xmlElement;
        }

        /// <summary>
        /// Gets the edmx multiplicity value.
        /// </summary>
        /// <param name="multiplicity">The multiplicity.</param>
        /// <returns>The edmx multiplicity</returns>
        internal static string GetEdmxMultiplicityValue(AssociationEndMultiplicity multiplicity)
        {
            switch (multiplicity)
            {
                case AssociationEndMultiplicity.Many:
                    return DataModellingResources.Many;
                case AssociationEndMultiplicity.One:
                    return DataModellingResources.One;
                case AssociationEndMultiplicity.ZeroOrOne:
                    return DataModellingResources.ZeroOrOne;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the GUID string for edmx.
        /// </summary>
        /// <returns>The guid string for edmx</returns>
        internal static string GetGuidString()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Gets the type of the SQL.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        /// <returns>The string with the type of data</returns>
        internal static string GetSQLType(DataTypes dataType)
        {
            switch (dataType)
            {
                case DataTypes.Binary:
                    return DataModellingResources.DataTypeVarbinary;
                case DataTypes.Boolean:
                    return DataModellingResources.DataTypeBit;
                case DataTypes.Byte:
                    return DataModellingResources.DataTypeTinyInt;
                case DataTypes.DateTime:
                    return DataModellingResources.DataTypeDatetime;
                case DataTypes.Decimal:
                    return DataModellingResources.DataTypeDecimal;
                case DataTypes.Double:
                    return DataModellingResources.DataTypeFloat;
                case DataTypes.Guid:
                    return DataModellingResources.DataTypeUniqueidentifier;
                case DataTypes.Int16:
                    return DataModellingResources.DataTypeSmallint;
                case DataTypes.Int32:
                    return DataModellingResources.DataTypeInt;
                case DataTypes.Int64:
                    return DataModellingResources.DataTypeBigint;
                case DataTypes.Single:
                    return DataModellingResources.DataTypeReal;
                case DataTypes.String:
                    return DataModellingResources.DataTypeNvarchar;
                default:
                    throw new ZentityException(string.Format(CultureInfo.InvariantCulture, DataModellingResources.UnknownDatatype, dataType.ToString()));
            }
        }

        /// <summary>
        /// Merges the sub names.
        /// </summary>
        /// <param name="firstSubName">First name of the sub.</param>
        /// <param name="secondSubName">Name of the second sub.</param>
        /// <returns>Returns the merged names</returns>
        internal static string MergeSubNames(string firstSubName, string secondSubName)
        {
            return firstSubName + "." + secondSubName;
        }
    }
}
