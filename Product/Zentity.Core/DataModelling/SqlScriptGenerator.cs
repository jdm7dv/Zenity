// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Transactions;
using System.Data.SqlClient;
using System.Data;
using System.Threading;
using System.Data.EntityClient;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Collections;
using Zentity.Core;

namespace Zentity.Core
{
    /// <summary>
    /// Helper class to generate scripts for SQL Server 2K8 that upgrade a model to a newer model.
    /// </summary>
    internal static class SqlScriptGenerator
    {
        /// <summary>
        /// Generates the scripts.
        /// </summary>
        /// <param name="originalModel">The original model.</param>
        /// <param name="newModel">The new model.</param>
        /// <param name="originalTableMappings">The original table mappings.</param>
        /// <param name="originalAssociationMappings">The original association mappings.</param>
        /// <param name="originalDiscriminators">The original discriminators.</param>
        /// <returns>Collection of queries</returns>
        internal static StringCollection GenerateScripts(DataModel originalModel, DataModel newModel, TableMappingCollection originalTableMappings, Dictionary<Guid, string> originalAssociationMappings, Dictionary<Guid, int> originalDiscriminators)
        {
            TableMappingCollection clonedTableMappings = originalTableMappings.Clone();

            // TODO: Ensure that all the scalar properties and appropriate navigation properties
            // specified in the original model has one and only one column mapping in the 
            // originalMappings structure.

            // We Process the changes in two passes.
            // 1. DDL Processing: In this pass we generate the DDL statements for the changes.
            // 2. DML Processing: In this pass we generate the DML statements to update the 
            //                    metadata information in the database.
            // NOTE: We update the passed in mappings to reflect the latest changes.

            // Structure to hold the generated scripts.
            StringCollection queries = new StringCollection();

            // Pass1 - DDL Processing.
            GenerateDDLs(originalModel, newModel, originalTableMappings,
                originalAssociationMappings, originalDiscriminators, queries);

            // Pass2 - DML Processing.
            // Note: We cannot reuse the changes for DDLs here. It is modified to facilitate
            // DDL generation. We re-compute the differences while generation the DMLs. The
            // changes provided by the DDL generator might be inconsistent with actual changes
            // done on the data model and if used, might cause incorrect information to be 
            // exposed by the change history module. For example, if an association is changed 
            // from ManyToMany to OneToMany, without affecting its navigation properties, the 
            // changes returned by the DDL generation procedure will have those navigation 
            // property recreated. But we never touched them! So, if we use these changes in DML
            // generation, we would delete and then insert rows in NavigationProperty table that
            // will generate incorrect information in change history module.
            GenerateDMLs(originalModel, newModel, clonedTableMappings, originalTableMappings,
                originalAssociationMappings, originalDiscriminators, queries);

            // Add AfterSchemaChangesHandler to the scripts. This gives a chance to other
            // modules (e.g. Change History Logging) to respond to schema changes.
            queries.Add(DataModellingResources.ExecAfterSchemaChanges);

            return queries;
        }

        /// <summary>
        /// Adjusts the changes for special associations and navigation properties.
        /// </summary>
        /// <param name="newModel">The new model.</param>
        /// <param name="changes">The changes.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static void AdjustChangesForSpecialAssociationsAndNavigationProperties(DataModel newModel, ModuleCollectionChange changes)
        {
            Dictionary<Association, Association> specialAssociations =
                new Dictionary<Association, Association>();
            Dictionary<NavigationProperty, NavigationProperty> specialNavigationProperties =
                new Dictionary<NavigationProperty, NavigationProperty>();
            foreach (var kvp in changes.UpdatedAssociations)
            {
                Association previousAssociation = kvp.Key;
                Association nextAssociation = kvp.Value;

                // Take note that we do an Id based comparisons here.
                // All associations that undergo navigation property changes will be treated
                // as if they are dropped from the earlier data model and recreated in the new
                // model.
                if (previousAssociation.SubjectNavigationProperty.Id !=
                    nextAssociation.SubjectNavigationProperty.Id ||
                    previousAssociation.ObjectNavigationProperty.Id !=
                    nextAssociation.ObjectNavigationProperty.Id ||
                    !IsCompatibleCardinalityConversion(previousAssociation, nextAssociation))
                {
                    specialAssociations.Add(previousAssociation, nextAssociation);
                }
            }

            // Each special association is treated as if the association is dropped from the 
            // original model and recreated in the new model. Also, propagate the re-create
            // logic to the end navigation properties.
            foreach (var kvp in specialAssociations)
            {
                Association previousAssociation = kvp.Key;
                Association nextAssociation = kvp.Value;

                changes.UpdatedAssociations.Remove(previousAssociation);
                changes.DeletedAssociations.Add(previousAssociation);
                changes.AddedAssociations.Add(nextAssociation);

                // Also update the NavigationProperty lists.
                NavigationProperty previousSubject = previousAssociation.SubjectNavigationProperty;
                NavigationProperty previousObject = previousAssociation.ObjectNavigationProperty;
                NavigationProperty nextSubject = nextAssociation.SubjectNavigationProperty;
                NavigationProperty nextObject = nextAssociation.ObjectNavigationProperty;

                if (!changes.DeletedNavigationProperties.Contains(previousSubject))
                    changes.DeletedNavigationProperties.Add(previousSubject);

                if (!changes.DeletedNavigationProperties.Contains(previousObject))
                    changes.DeletedNavigationProperties.Add(previousObject);

                if (!changes.AddedNavigationProperties.Contains(nextSubject))
                    changes.AddedNavigationProperties.Add(nextSubject);

                if (!changes.AddedNavigationProperties.Contains(nextObject))
                    changes.AddedNavigationProperties.Add(nextObject);
            }

            // Remove all the navigation properties for deleted associations. The logic here
            // comes handy when the association is deleted without deleting the navigation
            // properties. We need to nullify all the FK information in this case.
            // Assumption here is that there are no One-To-One associations in the data model.
            foreach (Association deletedAssociation in changes.DeletedAssociations)
            {
                if (deletedAssociation.SubjectMultiplicity == AssociationEndMultiplicity.One)
                {
                    if (!changes.DeletedNavigationProperties.Contains(
                        deletedAssociation.ObjectNavigationProperty))
                        changes.DeletedNavigationProperties.Add(
                            deletedAssociation.ObjectNavigationProperty);
                }

                if (deletedAssociation.ObjectMultiplicity == AssociationEndMultiplicity.One)
                {
                    if (!changes.DeletedNavigationProperties.Contains(
                        deletedAssociation.SubjectNavigationProperty))
                        changes.DeletedNavigationProperties.Add(
                            deletedAssociation.SubjectNavigationProperty);
                }
            }

            // Add navigation properties for new associations. Again this logic is the inverse
            // of above and comes handy when an association is added without affecting navigation
            // properties in the model.
            // Assumption here is that there are no One-To-One associations in the data model.
            foreach (Association addedAssociation in changes.AddedAssociations)
            {
                if (addedAssociation.SubjectMultiplicity == AssociationEndMultiplicity.One)
                {
                    if (!changes.AddedNavigationProperties.Contains(
                        addedAssociation.ObjectNavigationProperty))
                        changes.AddedNavigationProperties.Add(
                            addedAssociation.ObjectNavigationProperty);
                }

                if (addedAssociation.ObjectMultiplicity == AssociationEndMultiplicity.One)
                {
                    if (!changes.AddedNavigationProperties.Contains(
                        addedAssociation.SubjectNavigationProperty))
                        changes.AddedNavigationProperties.Add(
                            addedAssociation.SubjectNavigationProperty);
                }
            }

            foreach (var kvp in changes.UpdatedNavigationProperties)
            {
                NavigationProperty previousProperty = kvp.Key;
                NavigationProperty nextProperty = kvp.Value;

                if (previousProperty.Parent.Id != nextProperty.Parent.Id)
                {
                    specialNavigationProperties.Add(previousProperty, nextProperty);
                }
            }

            // Each special navigation property is treated as if the property is dropped from the
            // original model and recreated in the new model. Also, propagate the re-create logic 
            // to the attached association.
            foreach (var kvp in specialNavigationProperties)
            {
                NavigationProperty previousProperty = kvp.Key;
                NavigationProperty nextProperty = kvp.Value;
                Association previousAssociation = previousProperty.Association;
                Association nextAssociation = nextProperty.Association;

                // Locate the other end of associations.
                //
                // IMPORTANT:Note that the previous other end is from the earlier graph
                // and the new other end is from the new graph. DO NOT make a mistake of
                // adding the previous other end to changes.AddedNavigationProperties or 
                // adding the next other end to changes.DeletedNavigationProperties.
                NavigationProperty previousOtherEnd =
                    previousProperty.Direction == AssociationEndType.Subject ?
                    previousAssociation.ObjectNavigationProperty :
                    previousAssociation.SubjectNavigationProperty;

                changes.UpdatedNavigationProperties.Remove(previousProperty);

                if (!changes.DeletedNavigationProperties.Contains(previousProperty))
                    changes.DeletedNavigationProperties.Add(previousProperty);

                if (!changes.AddedNavigationProperties.Contains(nextProperty))
                    changes.AddedNavigationProperties.Add(nextProperty);

                // Include the association in processing list.
                if (!changes.DeletedAssociations.Contains(previousAssociation))
                    changes.DeletedAssociations.Add(previousAssociation);

                if (!changes.AddedAssociations.Contains(nextAssociation))
                    changes.AddedAssociations.Add(nextAssociation);

                // Delete the previous other end.
                if (!changes.DeletedNavigationProperties.Contains(previousOtherEnd))
                    changes.DeletedNavigationProperties.Add(previousOtherEnd);

                // If the previous other end is also present in the new model, again add
                // it to the added list. Example,
                // Previous Model:
                // RT1.Nav1  <--Assoc1-->  RTX.NavAlpha
                // RT1.Nav2  <--Assoc2-->  RTX.NavBeta
                // New Model (Nav1 now moved to RT2):
                // RT2.Nav1  <--Assoc1-->  RTX.NavAlpha
                // RT1.Nav2  <--Assoc2-->  RTX.NavBeta
                //
                // The previous statement will add NavAlpha to deleted list but it is still
                // present in the new data model. So, we need to again add it. Take care in
                // locating the property in the new graph for the earlier property.
                NavigationProperty previousOtherEndInNewModel = newModel.Modules.
                    SelectMany(module => module.ResourceTypes).
                    SelectMany(type => type.NavigationProperties).
                    Where(property => property.Id == previousOtherEnd.Id).FirstOrDefault();
                if (previousOtherEndInNewModel != null &&
                    !changes.AddedNavigationProperties.Contains(previousOtherEndInNewModel))
                    changes.AddedNavigationProperties.Add(previousOtherEndInNewModel);

                // We don't have to care about the next other end. If its a new end, it will
                // be there already in the AddedNavigationProperties. If it comes from previous
                // model, above association change and navigation property change handling logic
                // will automatically place it in both DeletedNavigationProperties and the 
                // AddedNavigationProperties list.
            }
        }

        /// <summary>
        /// Appends the delete procedure.
        /// </summary>
        /// <param name="newType">The new type.</param>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="queries">The queries collection.</param>
        private static void AppendDeleteProcedure(ResourceType newType, TableMappingCollection tableMappings, StringCollection queries)
        {
            Dictionary<string, string> deleteStatements = new Dictionary<string, string>();
            StringBuilder parameterList = new StringBuilder();
            StringBuilder deleteFromRelationshipStatements = new StringBuilder();

            parameterList.AppendLine(DataModellingResources.AtIdUniqueIdentifier);

            ResourceType tempType = newType;
            while (tempType != null)
            {
                foreach (ScalarProperty scalarProperty in tempType.ScalarProperties)
                {
                    ColumnMapping columnMapping = tableMappings.GetColumnMappingByPropertyId(
                        scalarProperty.Id);
                    TableMapping tableMapping = columnMapping.Parent;
                    string tableName = tableMapping.TableName;

                    if (!deleteStatements.Keys.Contains(tableMapping.TableName))
                        deleteStatements.Add(tableName,
                            string.Format(CultureInfo.InvariantCulture, DataModellingResources.DeleteFrom,
                            tableName, DataModellingResources.Id, DataModellingResources.AtId));
                }

                foreach (NavigationProperty navigationProperty in tempType.NavigationProperties)
                {
                    ColumnMapping columnMapping = tableMappings.GetColumnMappingByPropertyId(
                        navigationProperty.Id);
                    if (columnMapping != null)
                    {
                        TableMapping tableMapping = columnMapping.Parent;
                        string tableName = tableMapping.TableName;
                        string parameterName = DataModellingResources.At + navigationProperty.Name;

                        parameterList.AppendLine(DataModellingResources.CommaWithTrailingSpace +
                            parameterName + DataModellingResources.Space +
                            DataModellingResources.DataTypeUniqueidentifier);

                        // Get the association that uses this property. The retrieved association
                        // is one of {ManyToOne, OneToMany, OneToZeroOrOne, ZeroOrOneToOne},
                        // basically a OneToXXX or XXXToOne type mapping. The property is always 
                        // mapped to the XXX side. In the logic below, we assume that there are no
                        // OneToOne associations in the data model.
                        Association association = navigationProperty.Association;
                        string paramSubjectResourceId = null;
                        string paramObjectResourceId = null;
                        string paramPredicateId = association.PredicateId.ToString();
                        if (association.SubjectMultiplicity == AssociationEndMultiplicity.One)
                        {
                            // Note the subjectColumnName. For OneToXXX association, the FK column
                            // hosts the Ids of Parent entity and the 'Id' column hosts the Ids of
                            // dependent entity. So to delete a row 
                            // (SubjectId, ObjectId, PredicateId) from Resource, we do something 
                            // like
                            // DELETE FROM [Core].[Relationship]
                            // WHERE [SubjectResourceId] = <FKColumnValue>
                            // AND [ObjectResourceId] = <IdColumnValue>
                            // AND [PredicateId] = <PredicateId>;
                            paramSubjectResourceId = parameterName;
                            paramObjectResourceId = DataModellingResources.AtId;
                        }
                        else
                        {
                            paramSubjectResourceId = DataModellingResources.AtId;
                            paramObjectResourceId = parameterName;
                        }

                        deleteFromRelationshipStatements.AppendLine(string.Format(
                            CultureInfo.InvariantCulture,
                            DataModellingResources.DeleteFromRelationshipFromDeleted,
                            paramSubjectResourceId, paramObjectResourceId,
                            PrepareStringParameter(paramPredicateId)));
                    }
                }

                tempType = tempType.BaseType;
            }

            // Prepare the procedure body.
            StringBuilder body = new StringBuilder();

            // Append Relationship removal statements.
            body.AppendLine(deleteFromRelationshipStatements.ToString());

            // Delete from the 'Resource' table in the end since there are FK constraints from
            // other resource table to it.
            foreach (var kvp in deleteStatements.
                Where(tuple => tuple.Key != DataModellingResources.Resource))
            {
                body.AppendLine(kvp.Value);
            }

            body.AppendLine(deleteStatements.Where(tuple => tuple.Key == DataModellingResources.Resource).
                First().Value);

            queries.Add(string.Format(CultureInfo.InvariantCulture, DataModellingResources.CreateProcedure,
                newType.DeleteProcedureName, parameterList.ToString(), string.Empty, body));
        }

        /// <summary>
        /// Appends the insert procedure.
        /// </summary>
        /// <param name="newType">The new type.</param>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="discriminators">The discriminators.</param>
        /// <param name="queries">The queries collection.</param>
        private static void AppendInsertProcedure(ResourceType newType, TableMappingCollection tableMappings, Dictionary<Guid, int> discriminators, StringCollection queries)
        {
            StringBuilder parameterList = new StringBuilder();
            StringBuilder parameterNotNullCheck = new StringBuilder();
            StringBuilder insertIntoRelationshipStatements = new StringBuilder();

            Dictionary<string, KeyValuePair<StringBuilder, StringBuilder>> insertStatements =
                new Dictionary<string, KeyValuePair<StringBuilder, StringBuilder>>();

            ResourceType tempType = newType;
            while (tempType != null)
            {
                foreach (ScalarProperty scalarProperty in tempType.ScalarProperties)
                {
                    ColumnMapping columnMapping = tableMappings.GetColumnMappingByPropertyId(
                        scalarProperty.Id);
                    TableMapping tableMapping = columnMapping.Parent;
                    string tableName = tableMapping.TableName;
                    string columnName = columnMapping.ColumnName;
                    string parameterName = DataModellingResources.At + scalarProperty.Name;

                    if (!scalarProperty.Nullable)
                        parameterNotNullCheck.AppendLine(
                            string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.ParameterNotNullCheck, scalarProperty.Name));

                    string columnDefinition = GetColumnDefinition(scalarProperty);
                    parameterList.AppendLine(DataModellingResources.CommaWithTrailingSpace +
                        parameterName + DataModellingResources.Space +
                        columnDefinition);

                    if (!insertStatements.Keys.Contains(tableMapping.TableName))
                        insertStatements.Add(tableName, new KeyValuePair<StringBuilder,
                            StringBuilder>(new StringBuilder(), new StringBuilder()));
                    var insertStatement = insertStatements[tableName];

                    insertStatement.Key.AppendLine(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.SquareBracket, columnName) + DataModellingResources.CommaWithTrailingSpace);

                    insertStatement.Value.AppendLine(parameterName + DataModellingResources.CommaWithTrailingSpace);
                }

                foreach (NavigationProperty navigationProperty in tempType.NavigationProperties)
                {
                    ColumnMapping columnMapping = tableMappings.GetColumnMappingByPropertyId(
                        navigationProperty.Id);
                    if (columnMapping != null)
                    {
                        TableMapping tableMapping = columnMapping.Parent;

                        string tableName = tableMapping.TableName;
                        string columnName = columnMapping.ColumnName;
                        string parameterName = DataModellingResources.At + navigationProperty.Name;

                        parameterList.AppendLine(DataModellingResources.CommaWithTrailingSpace + parameterName + DataModellingResources.Space +
                            DataModellingResources.DataTypeUniqueidentifier);

                        parameterNotNullCheck.AppendLine(
                            string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.ParameterNotNullCheck, navigationProperty.Name));

                        if (!insertStatements.Keys.Contains(tableMapping.TableName))
                            insertStatements.Add(tableName, new KeyValuePair<StringBuilder,
                                StringBuilder>(new StringBuilder(), new StringBuilder()));
                        var insertStatement = insertStatements[tableName];

                        insertStatement.Key.AppendLine(string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.SquareBracket, columnName) + DataModellingResources.CommaWithTrailingSpace);

                        insertStatement.Value.AppendLine(parameterName + DataModellingResources.CommaWithTrailingSpace);

                        // Get the association that uses this property. The retrieved association
                        // is one of {ManyToOne, OneToMany, OneToZeroOrOne, ZeroOrOneToOne},
                        // basically a OneToXXX or XXXToOne type mapping. The property is always 
                        // mapped to the XXX side. In the logic below, we assume that there are no
                        // OneToOne associations in the data model.
                        Association association = navigationProperty.Association;
                        string paramSubjectResourceId = null;
                        string paramObjectResourceId = null;
                        string paramPredicateId = association.PredicateId.ToString();
                        if (association.SubjectMultiplicity == AssociationEndMultiplicity.One)
                        {
                            // Note the subjectColumnName. For OneToXXX association, the FK column
                            // hosts the Ids of Parent entity and the 'Id' column hosts the Ids of
                            // dependent entity. So while inserting into 
                            // Relationship(SubjectId,ObjectId) we do something like
                            // SELECT FKColumnName, Id FROM ...
                            paramSubjectResourceId = parameterName;
                            paramObjectResourceId = DataModellingResources.AtId;
                        }
                        else
                        {
                            paramSubjectResourceId = DataModellingResources.AtId;
                            paramObjectResourceId = parameterName;
                        }

                        insertIntoRelationshipStatements.AppendLine(string.Format(
                            CultureInfo.InvariantCulture,
                            DataModellingResources.InsertIntoRelationshipFromInserted,
                            paramSubjectResourceId, paramObjectResourceId,
                            PrepareStringParameter(paramPredicateId)));
                    }
                }

                tempType = tempType.BaseType;
            }

            string parameters = parameterList.ToString().Substring(1, parameterList.Length - 1);
            StringBuilder body = new StringBuilder();

            // Prepare the procedure body.
            // Insert into 'Resource' table first.
            var insertIntoResource = insertStatements[DataModellingResources.Resource];

            //Set Resource ManagerType Id
            string paramResourceTypeId = PrepareStringParameter(newType.Id.ToString().
                ToLowerInvariant());
            insertIntoResource.Key.AppendLine(string.Format(CultureInfo.InvariantCulture,
                DataModellingResources.SquareBracket, DataModellingResources.ResourceTypeId) +
                DataModellingResources.CommaWithTrailingSpace);
            insertIntoResource.Value.AppendLine(paramResourceTypeId + DataModellingResources.CommaWithTrailingSpace);

            //Set Discriminator value
            // NOTE: DO NOT use the Discriminator property of ResourceType here. It's not set yet!
            insertIntoResource.Key.AppendLine(string.Format(CultureInfo.InvariantCulture,
                DataModellingResources.SquareBracket, DataModellingResources.Discriminator));
            insertIntoResource.Value.AppendLine(discriminators[newType.Id].
                ToString(CultureInfo.InvariantCulture));

            body.AppendLine(string.Format(CultureInfo.InvariantCulture, DataModellingResources.InsertInto,
                DataModellingResources.Resource,
                insertIntoResource.Key.ToString(),
                insertIntoResource.Value.ToString()));

            // Insert into other tables.
            foreach (var kvp in insertStatements.
                Where(tuple => tuple.Key != DataModellingResources.Resource))
            {
                string tableName = kvp.Key;
                var insertIntoTable = kvp.Value;

                //Set Id
                insertIntoTable.Key.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.SquareBracket, DataModellingResources.Id) +
                    DataModellingResources.CommaWithTrailingSpace);
                insertIntoTable.Value.AppendLine(DataModellingResources.AtId +
                    DataModellingResources.CommaWithTrailingSpace);

                //Set Resource ManagerType Id
                insertIntoTable.Key.AppendLine(string.Format(CultureInfo.InvariantCulture,
                DataModellingResources.SquareBracket, DataModellingResources.ResourceTypeId) +
                DataModellingResources.CommaWithTrailingSpace);
                insertIntoTable.Value.AppendLine(paramResourceTypeId +
                    DataModellingResources.CommaWithTrailingSpace);

                //Set Discriminator value
                // NOTE: DO NOT use the Discriminator property of ResourceType here. 
                // It's not set yet!
                insertIntoTable.Key.AppendLine(string.Format(CultureInfo.InvariantCulture,
                DataModellingResources.SquareBracket, DataModellingResources.Discriminator));
                insertIntoTable.Value.AppendLine(discriminators[newType.Id].
                    ToString(CultureInfo.InvariantCulture));

                body.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.InsertInto, tableName,
                    insertIntoTable.Key.ToString(),
                    insertIntoTable.Value.ToString()));
            }

            // Append Insert Into Core.Relationship statements.
            body.AppendLine(insertIntoRelationshipStatements.ToString());

            queries.Add(string.Format(CultureInfo.InvariantCulture, DataModellingResources.CreateProcedure,
                newType.InsertProcedureName, parameters, parameterNotNullCheck.ToString(), body));
        }

        /// <summary>
        /// Appends the update procedure.
        /// </summary>
        /// <param name="newType">The new type.</param>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="queries">The queries collection.</param>
        private static void AppendUpdateProcedure(ResourceType newType, TableMappingCollection tableMappings, StringCollection queries)
        {
            StringBuilder parameterList = new StringBuilder();
            Dictionary<string, StringBuilder> setStatements =
                new Dictionary<string, StringBuilder>();
            StringBuilder parameterNotNullCheck = new StringBuilder();
            StringBuilder updateRelationshipStatements = new StringBuilder();

            ResourceType tempType = newType;
            while (tempType != null)
            {
                foreach (ScalarProperty scalarProperty in tempType.ScalarProperties)
                {
                    ColumnMapping columnMapping = tableMappings.GetColumnMappingByPropertyId(
                        scalarProperty.Id);
                    TableMapping tableMapping = columnMapping.Parent;
                    string tableName = tableMapping.TableName;
                    string columnName = columnMapping.ColumnName;
                    string parameterName = DataModellingResources.At + scalarProperty.Name;

                    string columnDefinition = GetColumnDefinition(scalarProperty);
                    parameterList.AppendLine(DataModellingResources.CommaWithTrailingSpace +
                        parameterName + DataModellingResources.Space +
                        columnDefinition);

                    if (!scalarProperty.Nullable)
                        parameterNotNullCheck.AppendLine(
                            string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.ParameterNotNullCheck, scalarProperty.Name));

                    if (!setStatements.Keys.Contains(tableMapping.TableName))
                        setStatements.Add(tableName, new StringBuilder());

                    var setStatement = setStatements[tableName];

                    if (scalarProperty.Name != DataModellingResources.Id)
                        setStatement.AppendLine(DataModellingResources.CommaWithTrailingSpace +
                            string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.SquareBracket, columnName) +
                            DataModellingResources.EqualsWithLeadingAndTrailingSpaces + parameterName);
                }

                foreach (NavigationProperty navigationProperty in tempType.NavigationProperties)
                {
                    ColumnMapping columnMapping = tableMappings.GetColumnMappingByPropertyId(
                        navigationProperty.Id);
                    if (columnMapping != null)
                    {
                        TableMapping tableMapping = columnMapping.Parent;
                        string tableName = tableMapping.TableName;
                        string columnName = columnMapping.ColumnName;
                        string parameterName = DataModellingResources.At + navigationProperty.Name;

                        parameterList.AppendLine(DataModellingResources.CommaWithTrailingSpace +
                            parameterName + DataModellingResources.Space +
                            DataModellingResources.DataTypeUniqueidentifier);

                        parameterNotNullCheck.AppendLine(
                                string.Format(CultureInfo.InvariantCulture,
                                DataModellingResources.ParameterNotNullCheck, navigationProperty.Name));

                        if (!setStatements.Keys.Contains(tableMapping.TableName))
                            setStatements.Add(tableName, new StringBuilder());

                        var setStatement = setStatements[tableName];

                        if (navigationProperty.Name != DataModellingResources.Id)
                            setStatement.AppendLine(DataModellingResources.CommaWithTrailingSpace +
                                string.Format(CultureInfo.InvariantCulture,
                                DataModellingResources.SquareBracket, columnName) +
                                DataModellingResources.EqualsWithLeadingAndTrailingSpaces + parameterName);

                        // Get the association that uses this property.
                        Association association = navigationProperty.Association;
                        string paramPreviousFKValue = DataModellingResources.AtPrevious +
                            navigationProperty.Name;
                        string paramPredicateId = association.PredicateId.ToString();
                        if (association.SubjectMultiplicity == AssociationEndMultiplicity.One)
                        {
                            // Note the subjectColumnName. For OneToXXX association, the FK column
                            // hosts the Ids of Parent entity and the 'Id' column hosts the Ids of
                            // dependent entity.
                            string updateRelationshipStatement = string.Format(
                                CultureInfo.InvariantCulture,
                                DataModellingResources.UpdateRelationshipForAssociation,
                                paramPreviousFKValue, EscapeBrackets(columnName),
                                EscapeBrackets(tableName), DataModellingResources.AtId, parameterName,
                                DataModellingResources.AtId, paramPreviousFKValue, DataModellingResources.AtId,
                                PrepareStringParameter(paramPredicateId));
                            updateRelationshipStatements.AppendLine(updateRelationshipStatement);
                        }
                        else
                        {
                            string updateRelationshipStatement = string.Format(
                                CultureInfo.InvariantCulture,
                                DataModellingResources.UpdateRelationshipForAssociation,
                                paramPreviousFKValue, EscapeBrackets(columnName),
                                EscapeBrackets(tableName), DataModellingResources.AtId,
                                DataModellingResources.AtId, parameterName, DataModellingResources.AtId, paramPreviousFKValue,
                                PrepareStringParameter(paramPredicateId));
                            updateRelationshipStatements.AppendLine(updateRelationshipStatement);
                        }
                    }
                }

                tempType = tempType.BaseType;
            }

            // Prepare the procedure body.
            string parameters = parameterList.ToString().Substring(1, parameterList.Length - 1);
            StringBuilder body = new StringBuilder();

            // Append Relationship update statements before resource updates, otherwise the
            // previous FK value is lost.
            body.AppendLine(updateRelationshipStatements.ToString());

            foreach (var kvp in setStatements)
            {
                string tableName = kvp.Key;
                StringBuilder setColumns = kvp.Value;
                body.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.UpdateSet, tableName,
                    setColumns.ToString().Substring(1, setColumns.Length - 1),
                    DataModellingResources.Id, DataModellingResources.AtId));
            }

            queries.Add(string.Format(CultureInfo.InvariantCulture, DataModellingResources.CreateProcedure,
                newType.UpdateProcedureName, parameters, parameterNotNullCheck.ToString(), body));
        }

        /// <summary>
        /// Creates the insert and delete procedures for association.
        /// </summary>
        /// <param name="queries">The queries collection.</param>
        /// <param name="association">The association.</param>
        /// <returns>The predicate Id</returns>
        private static Guid CreateInsertAndDeleteProceduresForAssociation(StringCollection queries, Association association)
        {
            Guid predicateId = association.PredicateId;
            string insertProcedureName = association.InsertProcedureName;
            string deleteProcedureName = association.DeleteProcedureName;

            // Create Insert Procedure.
            queries.Add(string.Format(CultureInfo.InvariantCulture,
                DataModellingResources.CreateInsertProcedureForView,
                EscapeBrackets(insertProcedureName),
                predicateId.ToString()));

            // Create Delete Procedure.
            queries.Add(string.Format(CultureInfo.InvariantCulture,
                DataModellingResources.CreateDeleteProcedureForView,
                EscapeBrackets(deleteProcedureName),
                predicateId.ToString()));
            return predicateId;
        }

        /// <summary>
        /// Creates the view and indexes for association.
        /// </summary>
        /// <param name="queries">The queries collection.</param>
        /// <param name="association">The association.</param>
        /// <param name="associationMappings">The association mappings.</param>
        /// <param name="uniqueClusteredIndexColumns">The unique clustered index columns.</param>
        /// <param name="uniqueNonClusteredIndexColumns">The unique non clustered index columns.</param>
        private static void CreateViewAndIndexesForAssociation(StringCollection queries, Association association, Dictionary<Guid, string> associationMappings, string[] uniqueClusteredIndexColumns, string[] uniqueNonClusteredIndexColumns)
        {
            Guid predicateId = association.PredicateId;
            string viewName = association.Id.ToString("N").ToLowerInvariant();

            // Create View.
            queries.Add(string.Format(CultureInfo.InvariantCulture,
                DataModellingResources.CreateView, EscapeBrackets(viewName),
                predicateId.ToString()));

            // Create unique clustered index.
            if (uniqueClusteredIndexColumns != null && uniqueClusteredIndexColumns.Length > 0)
            {
                StringBuilder sbCommaSeparatedList = new StringBuilder();
                for (int i = 0; i < uniqueClusteredIndexColumns.Length; i++)
                {
                    if (i > 0)
                        sbCommaSeparatedList.Append(DataModellingResources.CommaWithTrailingSpace);

                    sbCommaSeparatedList.Append(string.Format(CultureInfo.InvariantCulture,
                       DataModellingResources.SquareBracket,
                       EscapeBrackets(uniqueClusteredIndexColumns[i])));
                }

                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.CreateUniqueClusteredIndex,
                    EscapeBrackets(viewName),
                    EscapeBrackets(viewName),
                    sbCommaSeparatedList.ToString()));
            }

            // Create unique non-clustered index on separate columns.
            if (uniqueNonClusteredIndexColumns != null && uniqueNonClusteredIndexColumns.Length > 0)
            {
                for (int i = 0; i < uniqueNonClusteredIndexColumns.Length; i++)
                {
                    string columnName = uniqueNonClusteredIndexColumns[i];
                    queries.Add(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.CreateUniqueNonClusteredIndex,
                        EscapeBrackets(columnName),
                        EscapeBrackets(viewName),
                        string.Format(CultureInfo.InvariantCulture, DataModellingResources.SquareBracket,
                        EscapeBrackets(columnName))));
                }
            }

            // Update internal lists.
            associationMappings.Add(association.Id, viewName);
        }

        /// <summary>
        /// Deletes the Create Update Delete procedures.
        /// </summary>
        /// <param name="resourceType">Type of the resource.</param>
        /// <param name="queries">The queries collection.</param>
        private static void DeleteCUDProcedures(ResourceType resourceType, StringCollection queries)
        {
            queries.Add(string.Format(CultureInfo.InvariantCulture, DataModellingResources.DropProcedure,
                EscapeBrackets(resourceType.InsertProcedureName)));
            queries.Add(string.Format(CultureInfo.InvariantCulture, DataModellingResources.DropProcedure,
                EscapeBrackets(resourceType.UpdateProcedureName)));
            queries.Add(string.Format(CultureInfo.InvariantCulture, DataModellingResources.DropProcedure,
                EscapeBrackets(resourceType.DeleteProcedureName)));
        }

        /// <summary>
        /// Drops the Foreign Key for navigation property.
        /// </summary>
        /// <param name="navigationProperty">The navigation property.</param>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="queries">The queries collection.</param>
        private static void DropFKForNavigationProperty(NavigationProperty navigationProperty, TableMappingCollection tableMappings, StringCollection queries)
        {
            ColumnMapping columnMapping = tableMappings.GetColumnMappingByPropertyId(
                navigationProperty.Id);
            string tableName = columnMapping.Parent.TableName;
            string columnName = columnMapping.ColumnName;

            // Drop FK constraint.
            string constraintName = string.Format(CultureInfo.InvariantCulture,
                DataModellingResources.FKConstraintName, columnName);
            queries.Add(string.Format(CultureInfo.InvariantCulture,
                DataModellingResources.DropForeignkey, EscapeBrackets(tableName),
                EscapeBrackets(constraintName)));

            // Drop the foreign key column. 
            queries.Add(string.Format(CultureInfo.InvariantCulture,
                DataModellingResources.AlterTableDropColumn, EscapeBrackets(tableName),
                EscapeBrackets(columnName)));

            // Update internal lists.
            columnMapping.Parent.ColumnMappings.Remove(columnMapping);
        }

        /// <summary>
        /// Modifies the string to escape brackets.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns>The output string</returns>
        private static string EscapeBrackets(string inputString)
        {
            return inputString.Replace("]", "]]");
        }

        /// <summary>
        /// Generates the Data Definition Language scripts.
        /// </summary>
        /// <param name="originalModel">The original model.</param>
        /// <param name="newModel">The new model.</param>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="associationMappings">The association mappings.</param>
        /// <param name="discriminators">The discriminators.</param>
        /// <param name="queries">The queries collection.</param>
        private static void GenerateDDLs(DataModel originalModel, DataModel newModel, TableMappingCollection tableMappings, Dictionary<Guid, string> associationMappings, Dictionary<Guid, int> discriminators, StringCollection queries)
        {
            // Compute differences in data models.
            ModuleCollectionChange changesForDDLs = DataModel.ComputeDifferences(originalModel,
                newModel);

            // THE ORDER OF PROCESSING IS IMPORTANT HERE.

            // Associations that undergo navigation property changes and Navigation Properties
            // that undergo parent changes are treated special. The net DDL is equivalent to 
            // the one generated if these items are dropped from the earlier data model and 
            // re-created in the new data model. The re-create logic is propagated to the end 
            // navigation properties of special associations and to the attached associations 
            // of special navigation properties.
            // Also, if OneToXXX associations are created or dropped without affecting the number
            // of navigation properties in the model, we have to force in DDL generation for 
            // XXX side navigation properties even if they are left untouched to drop or create
            // the foreign key column.
            // Finally, if the associations are undergoing a cardinality update, we check for the
            // cardinality compatibilities. If not compatible, we re-create the association.
            AdjustChangesForSpecialAssociationsAndNavigationProperties(newModel, changesForDDLs);

            ProcessDeletedScalarProperties(changesForDDLs, tableMappings, queries);
            ProcessDeletedAssociations(changesForDDLs, associationMappings, queries);
            ProcessDeletedNavigationProperties(changesForDDLs, tableMappings, queries);
            ProcessDeletedResourceTypes(changesForDDLs, discriminators);

            ProcessAddedResourceTypes(changesForDDLs, discriminators,
                originalModel.MaxDiscriminator);
            ProcessAddedScalarProperties(changesForDDLs, tableMappings, queries);
            ProcessAddedNavigationProperties(changesForDDLs, tableMappings, queries);
            ProcessAddedAssociations(changesForDDLs, associationMappings, queries);

            // Database objects are never renamed during updates. This makes the change history 
            // module less complex. We just update the mappings in metadata information later.
            ProcessUpdatedScalarProperties(changesForDDLs, tableMappings, queries);
            ProcessUpdatedAssociations(changesForDDLs, tableMappings, associationMappings,
                queries);

            // Refresh CUD procedures for resource types.
            // Note: We re-use the DDL changes here. It has the information of actual changes
            // done on database. 
            RefreshCUDProcedures(originalModel, newModel, changesForDDLs, tableMappings,
                discriminators, queries);
        }

        /// <summary>
        /// Generates the Data Modification Language scripts.
        /// </summary>
        /// <param name="originalModel">The original model.</param>
        /// <param name="newModel">The new model.</param>
        /// <param name="originalTableMappings">The original table mappings.</param>
        /// <param name="newTablelMappings">The new tablel mappings.</param>
        /// <param name="newAssociationMappings">The new association mappings.</param>
        /// <param name="discriminators">The discriminators.</param>
        /// <param name="queries">The queries collection.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static void GenerateDMLs(DataModel originalModel, DataModel newModel, TableMappingCollection originalTableMappings, TableMappingCollection newTablelMappings, Dictionary<Guid, string> newAssociationMappings, Dictionary<Guid, int> discriminators, StringCollection queries)
        {
            List<NavigationProperty> processedNavigationProperties = new List<NavigationProperty>();

            // Compute differences in data models.
            ModuleCollectionChange changes = DataModel.ComputeDifferences(originalModel, newModel);

            // THE ORDER OF PROCESSING IS IMPORTANT HERE. 
            // Process added items, then update the earlier items to point to the added items.
            // Finally delete items from the model. This logic might raise errors if the newly
            // added items have same Name/NameSpace as that of the already present items. So,
            // we drop the unique constraints first, add these items and then reapply the 
            // constraints. Simply deleting everything and then creating new items is error prone. 
            // For example, consider the scenario where a navigation property is removed and a 
            // new navigation property is added without removing the association. Now, while 
            // removing the row for navigation property, we will get FK violations.
            // Another alternative is to insert the new items with dummy names, do updates and
            // then update the rows. But that would generate multiple rows for change history
            // module.

            // Drop constraints.
            queries.Add(DataModellingResources.ExecDropUniqueIndexesFromMetadata);

            #region Process all added objects.
            // Follow the order of Module --> ResourceType
            // --> ScalarProperty --> NavigationProperty --> Association to comply with the
            // referential constraints on metadata tables.
            foreach (DataModelModule item in changes.AddedDataModelModules)
            {
                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.CreateOrUpdateDataModelModule,
                    PrepareStringParameter(item.Id.ToString()),
                    PrepareStringParameter(item.NameSpace),
                    PrepareStringParameter(item.Uri),
                    PrepareStringParameter(item.Description)));
            }

            // Since the metadata has referential constraints, process base then derived.
            List<ResourceType> sortedTypes = DataModel.SortByHierarchy(changes.AddedResourceTypes.ToList());
            for (int i = 0; i < sortedTypes.Count; i++)
            {
                ResourceType resourceType = sortedTypes[i];
                string configXml = resourceType.ConfigurationXml == null ? string.Empty : resourceType.ConfigurationXml.ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.CreateOrUpdateResourceType,
                    PrepareStringParameter(resourceType.Id.ToString()),
                    PrepareStringParameter(resourceType.Parent.Id.ToString()),
                    PrepareStringParameter(resourceType.BaseType.Id.ToString()),
                    PrepareStringParameter(resourceType.Name),
                    PrepareStringParameter(resourceType.Uri),
                    PrepareStringParameter(resourceType.Description),
                    discriminators[resourceType.Id].ToString(CultureInfo.InvariantCulture),
                    PrepareStringParameter(configXml)));
            }

            foreach (ScalarProperty scalarProperty in changes.AddedScalarProperties)
            {
                // Get the table and column mappings.
                ColumnMapping columnMapping = newTablelMappings.GetColumnMappingByPropertyId(
                    scalarProperty.Id);

                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.CreateOrUpdateScalarProperty,
                    PrepareStringParameter(scalarProperty.Id.ToString()),
                    PrepareStringParameter(scalarProperty.Parent.Id.ToString()),
                    PrepareStringParameter(scalarProperty.Name),
                    PrepareStringParameter(scalarProperty.Uri),
                    PrepareStringParameter(scalarProperty.Description),
                    PrepareStringParameter(scalarProperty.DataType.ToString()),
                    scalarProperty.Nullable ? 1 : 0,
                    scalarProperty.MaxLength,
                    scalarProperty.Scale,
                    scalarProperty.Precision,
                    PrepareStringParameter(columnMapping.Parent.TableName),
                    PrepareStringParameter(columnMapping.ColumnName)));
            }

            foreach (NavigationProperty property in changes.AddedNavigationProperties)
            {
                // Get the table and column mappings.
                ColumnMapping columnMapping = newTablelMappings.GetColumnMappingByPropertyId(
                    property.Id);
                if (columnMapping != null)
                {
                    queries.Add(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.CreateOrUpdateNavigationProperty,
                        PrepareStringParameter(property.Id.ToString()),
                        PrepareStringParameter(property.Parent.Id.ToString()),
                        PrepareStringParameter(property.Name),
                        PrepareStringParameter(property.Uri),
                        PrepareStringParameter(property.Description),
                        PrepareStringParameter(columnMapping.Parent.TableName),
                        PrepareStringParameter(columnMapping.ColumnName)));
                }
                else
                {
                    queries.Add(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.CreateOrUpdateNavigationProperty,
                        PrepareStringParameter(property.Id.ToString()),
                        PrepareStringParameter(property.Parent.Id.ToString()),
                        PrepareStringParameter(property.Name),
                        PrepareStringParameter(property.Uri),
                        PrepareStringParameter(property.Description),
                        PrepareStringParameter(null), PrepareStringParameter(null)));
                }

                // Add the navigation property to processed list.
                processedNavigationProperties.Add(property);
            }

            foreach (Association association in changes.AddedAssociations)
            {
                // Always create a Predicate for the association. We do not allow sharing of
                // predicates between associations. Also, there are special checks while inserting
                // raw relationships to enforce that if the predicate is used by an association,
                // then the subject and object resources should adhere to the subject and
                // object resource types of the association.
                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.CreatePredicate,
                    PrepareStringParameter(association.PredicateId.ToString()),
                    PrepareStringParameter(association.Name),
                    PrepareStringParameter(association.Uri)));

                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.CreateOrUpdateAssociation,
                    PrepareStringParameter(association.Id.ToString()),
                    PrepareStringParameter(association.Name),
                    PrepareStringParameter(association.Uri),
                    PrepareStringParameter(association.SubjectNavigationProperty.Id.ToString()),
                    PrepareStringParameter(association.ObjectNavigationProperty.Id.ToString()),
                    PrepareStringParameter(association.PredicateId.ToString()),
                    PrepareStringParameter(Enum.GetName(typeof(AssociationEndMultiplicity),
                    association.SubjectMultiplicity)),
                    PrepareStringParameter(Enum.GetName(typeof(AssociationEndMultiplicity),
                    association.ObjectMultiplicity)),
                    PrepareStringParameter(newAssociationMappings[association.Id])));
            }

            #endregion

            #region Process all updated objects.
            // Database objects are never renamed. This makes
            // the change history module less complex. We just update the mappings in metadata
            // information.

            foreach (KeyValuePair<DataModelModule, DataModelModule> kvp in
                changes.UpdatedDataModelModules)
            {
                DataModelModule module = kvp.Value;
                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.CreateOrUpdateDataModelModule,
                    PrepareStringParameter(module.Id.ToString()),
                    PrepareStringParameter(module.NameSpace), PrepareStringParameter(module.Uri),
                    PrepareStringParameter(module.Description)));
            }

            foreach (KeyValuePair<ResourceType, ResourceType> kvp
                in changes.UpdatedResourceTypes)
            {
                ResourceType nextType = kvp.Value;
                string configXml = nextType.ConfigurationXml == null ? string.Empty : nextType.ConfigurationXml.ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.CreateOrUpdateResourceType,
                    PrepareStringParameter(nextType.Id.ToString()),
                    PrepareStringParameter(nextType.Parent.Id.ToString()),
                    PrepareStringParameter(nextType.BaseType.Id.ToString()),
                    PrepareStringParameter(nextType.Name),
                    PrepareStringParameter(nextType.Uri),
                    PrepareStringParameter(nextType.Description),
                    discriminators[nextType.Id].ToString(CultureInfo.InvariantCulture),
                    PrepareStringParameter(configXml)));
            }

            foreach (KeyValuePair<ScalarProperty, ScalarProperty> kvp
                in changes.UpdatedScalarProperties)
            {
                ScalarProperty nextProperty = kvp.Value;

                // Get the table and column mappings.
                ColumnMapping columnMapping = newTablelMappings.GetColumnMappingByPropertyId(
                    nextProperty.Id);

                queries.Add(string.Format(CultureInfo.InvariantCulture,
                 DataModellingResources.CreateOrUpdateScalarProperty,
                 PrepareStringParameter(nextProperty.Id.ToString()),
                 PrepareStringParameter(nextProperty.Parent.Id.ToString()),
                 PrepareStringParameter(nextProperty.Name),
                 PrepareStringParameter(nextProperty.Uri),
                 PrepareStringParameter(nextProperty.Description),
                 PrepareStringParameter(nextProperty.DataType.ToString()),
                 nextProperty.Nullable ? 1 : 0,
                 nextProperty.MaxLength,
                 nextProperty.Scale,
                 nextProperty.Precision,
                 PrepareStringParameter(columnMapping.Parent.TableName),
                 PrepareStringParameter(columnMapping.ColumnName)));
            }

            foreach (KeyValuePair<NavigationProperty, NavigationProperty> kvp
                in changes.UpdatedNavigationProperties)
            {
                NavigationProperty property = kvp.Value;

                ColumnMapping columnMapping = newTablelMappings.GetColumnMappingByPropertyId(
                    property.Id);
                if (columnMapping != null)
                {
                    queries.Add(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.CreateOrUpdateNavigationProperty,
                        PrepareStringParameter(property.Id.ToString()),
                        PrepareStringParameter(property.Parent.Id.ToString()),
                        PrepareStringParameter(property.Name),
                        PrepareStringParameter(property.Uri),
                        PrepareStringParameter(property.Description),
                        PrepareStringParameter(columnMapping.Parent.TableName),
                        PrepareStringParameter(columnMapping.ColumnName)));
                }
                else
                {
                    queries.Add(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.CreateOrUpdateNavigationProperty,
                        PrepareStringParameter(property.Id.ToString()),
                        PrepareStringParameter(property.Parent.Id.ToString()),
                        PrepareStringParameter(property.Name),
                        PrepareStringParameter(property.Uri),
                        PrepareStringParameter(property.Description),
                        PrepareStringParameter(null), PrepareStringParameter(null)));
                }

                // Add property to processed list.
                processedNavigationProperties.Add(property);
            }

            foreach (KeyValuePair<Association, Association> kvp in changes.UpdatedAssociations)
            {
                Association nextAssociation = kvp.Value;
                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.CreateOrUpdateAssociation,
                    PrepareStringParameter(nextAssociation.Id.ToString()),
                    PrepareStringParameter(nextAssociation.Name),
                    PrepareStringParameter(nextAssociation.Uri),
                    PrepareStringParameter(nextAssociation.SubjectNavigationProperty.Id.ToString()),
                    PrepareStringParameter(nextAssociation.ObjectNavigationProperty.Id.ToString()),
                    PrepareStringParameter(nextAssociation.PredicateId.ToString()),
                    PrepareStringParameter(Enum.GetName(typeof(AssociationEndMultiplicity),
                    nextAssociation.SubjectMultiplicity)),
                    PrepareStringParameter(Enum.GetName(typeof(AssociationEndMultiplicity),
                    nextAssociation.ObjectMultiplicity)),
                    PrepareStringParameter(newAssociationMappings[nextAssociation.Id])));
            }

            #endregion

            #region Process all removed objects.
            // Since, the metadata has referential
            // constraints, it is important to remove ScalarProperty, then ResourceType
            // and then DataModelModule.

            foreach (ScalarProperty property in changes.DeletedScalarProperties)
            {
                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.DeleteFromScalarProperty,
                    PrepareStringParameter(property.Id.ToString())));
            }

            foreach (Association association in changes.DeletedAssociations)
            {
                // Follow the order of Association -> Predicate.
                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.DeleteFromAssociation,
                    PrepareStringParameter(association.Id.ToString())));

                // Delete the predicate.
                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.DeleteFromPredicate,
                    PrepareStringParameter(association.PredicateId.ToString())));
            }

            foreach (NavigationProperty property in changes.DeletedNavigationProperties)
            {
                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.DeleteFromNavigationProperty,
                    PrepareStringParameter(property.Id.ToString())));
            }

            // Since the metadata has referential constraints, process derived then base.
            sortedTypes = DataModel.SortByHierarchy(changes.DeletedResourceTypes.ToList());

            for (int i = sortedTypes.Count - 1; i >= 0; i--)
            {
                ResourceType resourceType = sortedTypes[i];
                string resourceTypeId = resourceType.Id.ToString();

                // Each deleted resource type also has all its scalar and navigation properties in
                // the respective deleted lists. All deleted scalar and navigation properties are
                // processed before the resource types. Thus, we need not care about them here.

                // Delete all resources of this type. There is no actual FK constraint between 
                // resource tables in the database. So the order of deletion should not matter.
                foreach (string tableName in newTablelMappings.Select(tuple => tuple.TableName))
                {
                    queries.Add(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.DeleteFromTableBasedOnResourceType,
                        EscapeBrackets(tableName), PrepareStringParameter(resourceTypeId)));
                }

                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.DeleteFromResourceType,
                    PrepareStringParameter(sortedTypes[i].Id.ToString())));
            }

            foreach (DataModelModule deletedModule in changes.DeletedDataModelModules)
            {
                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.DeleteFromDataModelModule,
                    PrepareStringParameter(deletedModule.Id.ToString())));
            }

            #endregion

            // Apply constraints.
            queries.Add(DataModellingResources.ExecCreateUniqueIndexesOnMetadata);

            #region Update the navigation property column mappings.

            // This is necessary for scenarios where navigation properties are not
            // actually updated but since the association multiplicity has changed,
            // the column mappings are now changed.
            // 
            // NOTE: We process only those navigation properties that are not processed
            // before. Updating a row in navigation property table more than once creates
            // multiple rows in the corresponding capture instance. This might success in
            // some errors in change history module.

            // Find all the navigation properties that have not been processed and have
            // undergone a change in mappings.
            foreach (NavigationProperty previousProperty in originalModel.Modules.
                SelectMany(tuple => tuple.ResourceTypes).
                SelectMany(tuple => tuple.NavigationProperties))
            {
                NavigationProperty newProperty = newModel.Modules.
                    SelectMany(tuple => tuple.ResourceTypes).
                    SelectMany(tuple => tuple.NavigationProperties).
                    Where(tuple => tuple.Id == previousProperty.Id).FirstOrDefault();

                // Navigation property exists in both the data models.
                if (newProperty != null)
                {
                    // Continue, if the property is already processed.
                    if (processedNavigationProperties.Contains(newProperty))
                        continue;

                    ColumnMapping previousColumnMapping = originalTableMappings.
                        GetColumnMappingByPropertyId(previousProperty.Id);

                    ColumnMapping nextColumnMapping = newTablelMappings.
                        GetColumnMappingByPropertyId(newProperty.Id);

                    // Mapping updates could be from null to not-null or not-null to null
                    // or not-null to not-null but with changed columns.

                    if (previousColumnMapping == null && nextColumnMapping != null ||
                        previousColumnMapping != null && nextColumnMapping != null &&
                        (!previousColumnMapping.ColumnName.Equals(nextColumnMapping.ColumnName,
                        StringComparison.OrdinalIgnoreCase) ||
                        !previousColumnMapping.Parent.TableName.Equals(nextColumnMapping.Parent.TableName,
                        StringComparison.OrdinalIgnoreCase)))
                    {
                        queries.Add(string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.CreateOrUpdateNavigationProperty,
                            PrepareStringParameter(newProperty.Id.ToString()),
                            PrepareStringParameter(newProperty.Parent.Id.ToString()),
                            PrepareStringParameter(newProperty.Name),
                            PrepareStringParameter(newProperty.Uri),
                            PrepareStringParameter(newProperty.Description),
                            PrepareStringParameter(nextColumnMapping.Parent.TableName),
                            PrepareStringParameter(nextColumnMapping.ColumnName)));
                    }

                    if (previousColumnMapping != null && nextColumnMapping == null)
                    {
                        queries.Add(string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.CreateOrUpdateNavigationProperty,
                            PrepareStringParameter(newProperty.Id.ToString()),
                            PrepareStringParameter(newProperty.Parent.Id.ToString()),
                            PrepareStringParameter(newProperty.Name),
                            PrepareStringParameter(newProperty.Uri),
                            PrepareStringParameter(newProperty.Description),
                            PrepareStringParameter(null), PrepareStringParameter(null)));
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// Gets the column definition.
        /// </summary>
        /// <param name="scalarProperty">The scalar property.</param>
        /// <returns>The column definition string</returns>
        private static string GetColumnDefinition(ScalarProperty scalarProperty)
        {
            string columnDefinition = null;
            switch (scalarProperty.DataType)
            {
                case DataTypes.Binary:
                    if (scalarProperty.MaxLength == -1)
                        columnDefinition = DataModellingResources.DataTypeVarbinary +
                            string.Format(CultureInfo.InvariantCulture, DataModellingResources.Paranthesis,
                            DataModellingResources.Max.ToLowerInvariant());
                    else
                        columnDefinition = DataModellingResources.DataTypeVarbinary +
                            string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.Paranthesis,
                            scalarProperty.MaxLength.ToString(CultureInfo.InvariantCulture));
                    break;
                case DataTypes.Boolean:
                    columnDefinition = DataModellingResources.DataTypeBit;
                    break;
                case DataTypes.Byte:
                    columnDefinition = DataModellingResources.DataTypeTinyInt;
                    break;
                case DataTypes.DateTime:
                    columnDefinition = DataModellingResources.DataTypeDatetime;
                    break;
                case DataTypes.Decimal:
                    columnDefinition = DataModellingResources.DataTypeDecimal +
                        string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.Paranthesis,
                        scalarProperty.Precision.ToString(CultureInfo.InvariantCulture) +
                        DataModellingResources.CommaWithTrailingSpace +
                        scalarProperty.Scale.ToString(CultureInfo.InvariantCulture));
                    break;
                case DataTypes.Double:
                    columnDefinition = DataModellingResources.DataTypeFloat;
                    break;
                case DataTypes.Guid:
                    columnDefinition = DataModellingResources.DataTypeUniqueidentifier;
                    break;
                case DataTypes.Int16:
                    columnDefinition = DataModellingResources.DataTypeSmallint;
                    break;
                case DataTypes.Int32:
                    columnDefinition = DataModellingResources.DataTypeInt;
                    break;
                case DataTypes.Int64:
                    columnDefinition = DataModellingResources.DataTypeBigint;
                    break;
                case DataTypes.Single:
                    columnDefinition = DataModellingResources.DataTypeReal;
                    break;
                case DataTypes.String:
                    if (scalarProperty.MaxLength == -1)
                        columnDefinition = DataModellingResources.DataTypeNvarchar +
                            string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.Paranthesis, DataModellingResources.Max.ToLowerInvariant());
                    else
                        columnDefinition = DataModellingResources.DataTypeNvarchar +
                            string.Format(CultureInfo.InvariantCulture,
                            DataModellingResources.Paranthesis,
                            scalarProperty.MaxLength.ToString(CultureInfo.InvariantCulture));
                    break;
            }
            return columnDefinition;
        }

        /// <summary>
        /// Gets the nullable column definition.
        /// </summary>
        /// <param name="scalarProperty">The scalar property.</param>
        /// <returns>The column definition string</returns>
        private static string GetNullableColumnDefinition(ScalarProperty scalarProperty)
        {
            string columnDefinition = GetColumnDefinition(scalarProperty);

            // NOTE: Due to TPH mappings, we have all the columns nullable, but the 
            // stored procedures that do actual inserts/updates check for null parameters.
            if (!string.IsNullOrEmpty(columnDefinition))
                columnDefinition += DataModellingResources.Space + DataModellingResources.Null;
            return columnDefinition;
        }

        /// <summary>
        /// Determines whether the association end multiplicity conversion is compatible or not.
        /// </summary>
        /// <param name="previousAssociationEndMultiplicity">The previous association end multiplicity.</param>
        /// <param name="nextAssociationEndMultiplicity">The next association end multiplicity.</param>
        /// <returns>
        /// 	<c>true</c> if the association end multiplicity conversion is compatible; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsCompatibleAssociationEndMultiplicityConversion(AssociationEndMultiplicity previousAssociationEndMultiplicity, AssociationEndMultiplicity nextAssociationEndMultiplicity)
        {
            switch (previousAssociationEndMultiplicity)
            {
                case AssociationEndMultiplicity.Many:
                    if (nextAssociationEndMultiplicity == AssociationEndMultiplicity.Many)
                        return true;
                    break;
                case AssociationEndMultiplicity.One:
                    if (nextAssociationEndMultiplicity == AssociationEndMultiplicity.Many ||
                        nextAssociationEndMultiplicity == AssociationEndMultiplicity.One ||
                        nextAssociationEndMultiplicity == AssociationEndMultiplicity.ZeroOrOne)
                        return true;
                    break;
                case AssociationEndMultiplicity.ZeroOrOne:
                    if (nextAssociationEndMultiplicity == AssociationEndMultiplicity.Many ||
                        nextAssociationEndMultiplicity == AssociationEndMultiplicity.ZeroOrOne)
                        return true;
                    break;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the cardinality conversion is compatible.
        /// </summary>
        /// <param name="previousAssociation">The previous association.</param>
        /// <param name="nextAssociation">The next association.</param>
        /// <returns>
        /// 	<c>true</c> if the cardinality conversion is compatible; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsCompatibleCardinalityConversion(Association previousAssociation, Association nextAssociation)
        {
            bool areSubjectsCompatible = IsCompatibleAssociationEndMultiplicityConversion(
                previousAssociation.SubjectMultiplicity, nextAssociation.SubjectMultiplicity);

            bool areObjectsCompatible = IsCompatibleAssociationEndMultiplicityConversion(
                previousAssociation.ObjectMultiplicity, nextAssociation.ObjectMultiplicity);

            if (areSubjectsCompatible && areObjectsCompatible)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Determines whether the data type conversion is compatible.
        /// </summary>
        /// <param name="previousProperty">The previous property.</param>
        /// <param name="nextProperty">The next property.</param>
        /// <returns>
        /// 	<c>true</c> if the data type conversion is compatible; otherwise, <c>false</c>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static bool IsCompatibleDataTypeConversion(ScalarProperty previousProperty, ScalarProperty nextProperty)
        {
            switch (previousProperty.DataType)
            {
                case DataTypes.Binary:
                    if (
                        nextProperty.DataType == DataTypes.Binary &&
                        (nextProperty.MaxLength >= previousProperty.MaxLength || nextProperty.MaxLength == -1)
                       )
                        return true;
                    break;

                case DataTypes.Boolean:
                    if (nextProperty.DataType == DataTypes.Binary ||
                        nextProperty.DataType == DataTypes.Boolean ||
                        nextProperty.DataType == DataTypes.Int16 ||
                        nextProperty.DataType == DataTypes.Int32 ||
                        nextProperty.DataType == DataTypes.Int64 ||
                        nextProperty.DataType == DataTypes.Single ||
                        nextProperty.DataType == DataTypes.String)
                        return true;
                    break;

                case DataTypes.Byte:
                    if (nextProperty.DataType == DataTypes.Byte)
                        return true;
                    break;

                case DataTypes.DateTime:
                    if (nextProperty.DataType == DataTypes.DateTime)
                        return true;
                    break;

                case DataTypes.Decimal:
                    if (
                        nextProperty.DataType == DataTypes.Decimal ||
                        nextProperty.DataType == DataTypes.String &&
                        (nextProperty.MaxLength >= 40 || nextProperty.MaxLength == -1)
                    )
                        return true;
                    break;

                case DataTypes.Double:
                    if (nextProperty.DataType == DataTypes.Double ||
                        nextProperty.DataType == DataTypes.Single ||
                        nextProperty.DataType == DataTypes.String &&
                        (nextProperty.MaxLength >= 40 || nextProperty.MaxLength == -1)
                    )
                        return true;
                    break;

                case DataTypes.Guid:
                    if (nextProperty.DataType == DataTypes.Guid ||
                        nextProperty.DataType == DataTypes.String &&
                        (nextProperty.MaxLength >= 40 || nextProperty.MaxLength == -1)
                    )
                        return true;
                    break;

                case DataTypes.Int16:
                    if (nextProperty.DataType == DataTypes.Int16 ||
                        nextProperty.DataType == DataTypes.Int32 ||
                        nextProperty.DataType == DataTypes.Int64 ||
                        nextProperty.DataType == DataTypes.Double ||
                        nextProperty.DataType == DataTypes.Single ||
                        nextProperty.DataType == DataTypes.String &&
                        (nextProperty.MaxLength >= 40 || nextProperty.MaxLength == -1)
                    )
                        return true;
                    break;

                case DataTypes.Int32:
                    if (nextProperty.DataType == DataTypes.Int32 ||
                        nextProperty.DataType == DataTypes.Int64 ||
                        nextProperty.DataType == DataTypes.Double ||
                        nextProperty.DataType == DataTypes.Single ||
                        nextProperty.DataType == DataTypes.String &&
                        (nextProperty.MaxLength >= 40 || nextProperty.MaxLength == -1)
                    )
                        return true;
                    break;

                case DataTypes.Int64:
                    if (nextProperty.DataType == DataTypes.Int64 ||
                        nextProperty.DataType == DataTypes.Double ||
                        nextProperty.DataType == DataTypes.Single ||
                        nextProperty.DataType == DataTypes.String &&
                        (nextProperty.MaxLength >= 40 || nextProperty.MaxLength == -1)
                    )
                        return true;
                    break;

                case DataTypes.Single:
                    if (nextProperty.DataType == DataTypes.Double ||
                        nextProperty.DataType == DataTypes.Single ||
                        nextProperty.DataType == DataTypes.String &&
                        (nextProperty.MaxLength >= 40 || nextProperty.MaxLength == -1)
                    )
                        return true;
                    break;

                case DataTypes.String:
                    if (nextProperty.DataType == DataTypes.String &&
                        (nextProperty.MaxLength >= previousProperty.MaxLength || nextProperty.MaxLength == -1)
                    )
                        return true;
                    break;
            }
            return false;
        }

        /// <summary>
        /// Prepares the string parameter for Sql query.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns>The escaped string</returns>
        private static string PrepareStringParameter(string inputString)
        {
            if (inputString == null)
                return DataModellingResources.Null;

            return "'" + inputString.Replace("'", "''") + "'";
        }

        /// <summary>
        /// Processes the added association and generate queries.
        /// </summary>
        /// <param name="queries">The queries collection.</param>
        /// <param name="association">The association.</param>
        /// <param name="associationMappings">The association mappings.</param>
        private static void ProcessAddedAssociation(StringCollection queries, Association association, Dictionary<Guid, string> associationMappings)
        {
            // Perform actions depending on the cardinality.
            switch (association.SubjectMultiplicity)
            {
                case AssociationEndMultiplicity.Many:
                    if (association.ObjectMultiplicity == AssociationEndMultiplicity.Many)
                    {
                        CreateViewAndIndexesForAssociation(queries, association,
                            associationMappings, new string[] { DataModellingResources.SubjectResourceId, 
                                DataModellingResources.ObjectResourceId }, null);

                        CreateInsertAndDeleteProceduresForAssociation(queries, association);
                    }
                    else if (association.ObjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne)
                    {
                        CreateViewAndIndexesForAssociation(queries, association,
                            associationMappings, new string[] { DataModellingResources.SubjectResourceId },
                            null);

                        CreateInsertAndDeleteProceduresForAssociation(queries, association);
                    }
                    else if (association.ObjectMultiplicity == AssociationEndMultiplicity.One)
                    {
                        // Create view and FK column.
                        // Creating a schema-bound view with unique index prevents inserting 
                        // relationships into the relationship table that violate the multiplicity
                        // constraints for the association.
                        CreateViewAndIndexesForAssociation(queries, association,
                            associationMappings, new string[] { DataModellingResources.SubjectResourceId },
                            null);
                    }
                    break;
                case AssociationEndMultiplicity.One:
                    if (association.ObjectMultiplicity == AssociationEndMultiplicity.Many)
                    {
                        CreateViewAndIndexesForAssociation(queries, association,
                            associationMappings, new string[] { DataModellingResources.ObjectResourceId },
                            null);
                    }
                    else if (association.ObjectMultiplicity == AssociationEndMultiplicity.One)
                        throw new ZentityException(DataModellingResources.InvalidMultiplicityOneToOne);
                    else if (association.ObjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne)
                    {
                        // Take note that we have not put a unique constraint on the ZeroOrOne
                        // side. This is required to allow updating of One To ZeroOrOne 
                        // relationships. For example, consider the following relationships
                        // A <--> 1, B <--> 2.
                        // Updating these relationships to reach a state where 'A' is related
                        // to '2' and 'B' is related to '1' requires the following steps:
                        // Step1: Relate A with 2, i.e. update B <--> 2 to A <--> 2
                        // Step2: Relate B with 1, i.e. update A <--> 1 to B <--> 1
                        // Now after step 1, we have A associated with more than one entities,
                        // thus we cannot create a unique constraint on the ZeroOrOne side.
                        //
                        // SERIOUS NOTE: So, it will be possible to violate the cardinality 
                        // constraint in Relationship table. We prevent this by adding a check
                        // in [Core].[AfterUpdateOnRelationship] trigger. We do not allow explicit
                        // updates of OneToZeroOrOne and ZeroOrOneToOne associations.
                        CreateViewAndIndexesForAssociation(queries, association,
                            associationMappings, new string[] { DataModellingResources.SubjectResourceId },
                            null);
                    }
                    break;
                case AssociationEndMultiplicity.ZeroOrOne:
                    if (association.ObjectMultiplicity == AssociationEndMultiplicity.Many)
                    {
                        CreateViewAndIndexesForAssociation(queries, association,
                            associationMappings, new string[] { DataModellingResources.ObjectResourceId },
                            null);

                        CreateInsertAndDeleteProceduresForAssociation(queries, association);
                    }
                    else if (association.ObjectMultiplicity == AssociationEndMultiplicity.One)
                    {
                        CreateViewAndIndexesForAssociation(queries, association,
                            associationMappings, new string[] { DataModellingResources.ObjectResourceId },
                            null);
                    }
                    else if (association.ObjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne)
                    {
                        CreateViewAndIndexesForAssociation(queries, association,
                            associationMappings, new string[] { DataModellingResources.SubjectResourceId },
                            new string[] { DataModellingResources.ObjectResourceId });

                        CreateInsertAndDeleteProceduresForAssociation(queries, association);
                    }
                    break;
                default:
                    throw new NotSupportedException(DataModellingResources.ExceptionNotSupportedCardinality);
            }
        }

        /// <summary>
        /// Processes the added associations and generate queries.
        /// </summary>
        /// <param name="changes">The changes collection.</param>
        /// <param name="associationMappings">The association mappings.</param>
        /// <param name="queries">The queries colletion.</param>
        private static void ProcessAddedAssociations(ModuleCollectionChange changes, Dictionary<Guid, string> associationMappings, StringCollection queries)
        {
            foreach (Association association in changes.AddedAssociations)
            {
                ProcessAddedAssociation(queries, association, associationMappings);

                // Adding an association does not necessarily mean adding a navigation property.
                // An earlier naviation property can be reused for new associations. Added
                // navigation properties are handled separately.
            }
        }

        /// <summary>
        /// Processes the added navigation properties and generate queries.
        /// </summary>
        /// <param name="changes">The changes collection.</param>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="queries">The queries collection.</param>
        private static void ProcessAddedNavigationProperties(ModuleCollectionChange changes, TableMappingCollection tableMappings, StringCollection queries)
        {
            // It may seem like we can handle navigation property additions while handling
            // association addition, but this is not entirely correct. It is possible to
            // add/remove navigation properties without affecting associations. For example,
            // ResourceType rt = GetTypeSomehow();
            // NavigationProperty nav1 = rt.NavigationProperties["Nav"];
            // NavigationProperty nav2 = new NavigationProperty;
            // Association assoc = nav1.Association;
            // rt.NavigationProperties.Remove(nav1);    // Removed the navigation property.
            // rt.NavigationProperties.Add(nav2);       // Added a new property.
            // assoc.SubjectNavigationProperty = nav2;  // Should be based on the direction, 
            //                                          // but still shows that navigation 
            //                                          // properties can be added or removed
            //                                          // independent of associations.

            foreach (NavigationProperty property in changes.AddedNavigationProperties)
            {
                // Earlier validations ensure that each navigation property is attached to an 
                // association.
                Association association = property.Association;

                if (association.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                    association.ObjectMultiplicity == AssociationEndMultiplicity.One)
                {
                    throw new ZentityException(DataModellingResources.InvalidMultiplicityOneToOne);
                }

                // If the property belongs to the XXX side of a OneToXXX association, create a 
                // column mapping for it.
                // NOTE: If an entity participates in many OneToXXX associations, we might get 
                // multiple FKs in the same table. These FKs will not be filled always because 
                // with TPH, one table can host data for multiple entities. Thus it is important
                // to make the FK columns nullable.
                // Assumption here is that there are no One-To-One associations in the data model.
                if (property.Direction == AssociationEndType.Subject &&
                    association.ObjectMultiplicity == AssociationEndMultiplicity.One ||
                    property.Direction == AssociationEndType.Object &&
                    association.SubjectMultiplicity == AssociationEndMultiplicity.One)
                {
                    // Earlier we used to create a new table after ~290 columns. But that caused
                    // serious performance hits. So, we always create new columns in Core.Resource
                    // table now.
                    string tableName = DataModellingResources.Resource;
                    string columnName = property.Id.ToString().ToLowerInvariant();
                    string constraintName = string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.FKConstraintName, columnName);

                    queries.Add(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.AlterTableAddForeignKeyColumn, tableName, columnName,
                        DataModellingResources.DataTypeUniqueidentifier, constraintName, DataModellingResources.Resource,
                        DataModellingResources.Id));

                    // Update internal lists.
                    tableMappings[tableName, StringComparison.OrdinalIgnoreCase].
                        ColumnMappings.Add(new ColumnMapping
                        {
                            ColumnName = columnName,
                            IsScalarProperty = false,
                            IsMapped = true,
                            PropertyId = property.Id
                        });
                }
            }
        }

        /// <summary>
        /// Processes the added resource types and generate queries.
        /// </summary>
        /// <param name="changes">The changes collection.</param>
        /// <param name="discriminators">The discriminators.</param>
        /// <param name="maxDiscriminator">The max discriminator.</param>
        private static void ProcessAddedResourceTypes(ModuleCollectionChange changes, Dictionary<Guid, int> discriminators, int maxDiscriminator)
        {
            foreach (ResourceType type in changes.AddedResourceTypes)
            {
                // Generate the new discriminator value to be 1 greater than the maximum of 
                // all the values in the list and the additional integer.
                int discriminator = discriminators.Values.Max();

                if (maxDiscriminator > discriminator)
                    discriminator = maxDiscriminator;

                // Update internal lists.
                discriminators.Add(type.Id, discriminator + 1);
            }
        }

        /// <summary>
        /// Processes the added scalar properties and generate queries.
        /// </summary>
        /// <param name="changes">The changes collection.</param>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="queries">The queries collection.</param>
        private static void ProcessAddedScalarProperties(ModuleCollectionChange changes, TableMappingCollection tableMappings, StringCollection queries)
        {
            foreach (ScalarProperty scalarProperty in changes.AddedScalarProperties)
            {
                // Get next available table.
                // Earlier we used to create a new table after ~290 columns. But that caused
                // serious performance hits. So, we always create new columns in Core.Resource
                // table now.
                string tableName = DataModellingResources.Resource;
                string columnName = scalarProperty.Id.ToString().ToLowerInvariant();

                // Alter table add column.
                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.AlterTableAddColumn,
                    EscapeBrackets(tableName),
                    EscapeBrackets(columnName),
                    GetNullableColumnDefinition(scalarProperty)));

                // Update internal lists.
                tableMappings[tableName, StringComparison.OrdinalIgnoreCase].
                    ColumnMappings.Add(new ColumnMapping
                    {
                        ColumnName = columnName,
                        IsMapped = true,
                        IsScalarProperty = true,
                        PropertyId = scalarProperty.Id
                    });
            }
        }

        /// <summary>
        /// Processes the deleted association and generate queries.
        /// </summary>
        /// <param name="queries">The queries collection.</param>
        /// <param name="association">The association.</param>
        /// <param name="associationMappings">The association mappings.</param>
        private static void ProcessDeletedAssociation(StringCollection queries, Association association, Dictionary<Guid, string> associationMappings)
        {
            // Drop View. This automatically drops the indexes on the view.
            // NOTE: A view is always created irrespective of the cardinality.
            // This is to enforce the cardinality constraints on Relationship
            // table for OneToXXX relationships.
            queries.Add(string.Format(CultureInfo.InvariantCulture,
                DataModellingResources.DropView,
                EscapeBrackets(associationMappings[association.Id])));

            if (association.SubjectMultiplicity != AssociationEndMultiplicity.One &&
                association.ObjectMultiplicity != AssociationEndMultiplicity.One)
            {
                // Drop INSERT/DELETE procedures.
                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.DropProcedure,
                    EscapeBrackets(association.InsertProcedureName)));

                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.DropProcedure,
                    EscapeBrackets(association.DeleteProcedureName)));
            }

            // Remove mapping information.
            associationMappings.Remove(association.Id);
        }

        /// <summary>
        /// Processes the deleted associations and generate queries.
        /// </summary>
        /// <param name="changes">The changes collection.</param>
        /// <param name="associationMappings">The association mappings.</param>
        /// <param name="queries">The queries collection.</param>
        private static void ProcessDeletedAssociations(ModuleCollectionChange changes, Dictionary<Guid, string> associationMappings, StringCollection queries)
        {
            foreach (Association association in changes.DeletedAssociations)
            {
                // Remove entries from the Relationship table for the association.
                // Though it is a DML, we are doing it here. This is because, dropping the
                // view does not delete rows from Relationship table. Ideally, when an 
                // association is dropped, no remains should exist in the Relationship table.

                // NOTE: We do not allow deletion of OneToXXX associations if there
                // are any relationships present for them. The only way to delete 
                // those relationships is to delete the entity on XXX side.
                // Assumption here is that there are no One-To-One associations in the data model.
                if (association.SubjectMultiplicity == AssociationEndMultiplicity.One ||
                    association.ObjectMultiplicity == AssociationEndMultiplicity.One)
                {
                    queries.Add(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.CheckRelationshipsForOneToXXX,
                        association.Id.ToString(),
                        string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.CardinalityDefinition,
                        association.SubjectMultiplicity.ToString(),
                        association.ObjectMultiplicity.ToString()),
                        (association.SubjectMultiplicity == AssociationEndMultiplicity.One) ?
                        association.ObjectMultiplicity.ToString() :
                        association.SubjectMultiplicity.ToString()));
                }

                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.DeleteFromRelationshipForPredicate,
                    PrepareStringParameter(association.PredicateId.ToString())));

                ProcessDeletedAssociation(queries, association, associationMappings);
            }
        }

        /// <summary>
        /// Processes the deleted navigation properties and generate queries.
        /// </summary>
        /// <param name="changes">The changes collection.</param>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="queries">The queries collection.</param>
        private static void ProcessDeletedNavigationProperties(ModuleCollectionChange changes, TableMappingCollection tableMappings, StringCollection queries)
        {
            // It looks like we can remove the navigation properties while removing 
            // associations. But that is not the case. It is possible to delete an 
            // association without deleting the navigation properties. For example,
            // Association assoc1 = GetAssoc();
            // NavigationProperty prop1 = assoc1.SubjectNavigationProperty;
            // NavigationProperty prop2 = assoc2.ObjectNavigationProperty;
            // assoc1.SubjectNavigationProperty = null;
            // assoc1.ObjectNavigationProperty = null;
            // Association assoc2 = new Association();
            // assoc2.SubjectNavigationProperty = prop1;
            // assoc2.ObjectNavigationProperty = prop2;
            // Thus, we need to handle the deletion of navigtion property separate from the
            // deletion of associations.

            foreach (NavigationProperty property in changes.DeletedNavigationProperties)
            {
                Association association = property.Association;

                // If the property belongs to the XXX side of a OneToXXX association, drop the 
                // foreign key column mapped to it. Assumption here is that there are no 
                // One-To-One associations in the data model.
                if (property.Direction == AssociationEndType.Subject &&
                    association.ObjectMultiplicity == AssociationEndMultiplicity.One ||
                    property.Direction == AssociationEndType.Object &&
                    association.SubjectMultiplicity == AssociationEndMultiplicity.One)
                    DropFKForNavigationProperty(property, tableMappings, queries);
            }
        }

        /// <summary>
        /// Processes the deleted resource types and generate queries.
        /// </summary>
        /// <param name="changes">The changes collection.</param>
        /// <param name="discriminators">The discriminators.</param>
        private static void ProcessDeletedResourceTypes(ModuleCollectionChange changes, Dictionary<Guid, int> discriminators)
        {
            foreach (ResourceType type in changes.DeletedResourceTypes)
            {
                // Update internal lists.
                discriminators.Remove(type.Id);
            }
        }

        /// <summary>
        /// Processes the deleted scalar properties and generate queries.
        /// </summary>
        /// <param name="changes">The changes collection.</param>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="queries">The queries collection.</param>
        private static void ProcessDeletedScalarProperties(ModuleCollectionChange changes, TableMappingCollection tableMappings, StringCollection queries)
        {
            foreach (ScalarProperty property in changes.DeletedScalarProperties)
            {
                ColumnMapping columnMapping =
                    tableMappings.GetColumnMappingByPropertyId(property.Id);
                TableMapping tableMapping = columnMapping.Parent;
                string tableName = tableMapping.TableName;
                string columnName = columnMapping.ColumnName;

                // Alter table drop column.
                queries.Add(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.AlterTableDropColumn,
                    EscapeBrackets(tableName),
                    EscapeBrackets(columnName)));

                // Update internal lists.
                tableMapping.ColumnMappings.Remove(columnMapping);
            }
        }

        /// <summary>
        /// Processes the updated associations and generate queries.
        /// </summary>
        /// <param name="changes">The changes collection.</param>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="associationMappings">The association mappings.</param>
        /// <param name="queries">The queries collection.</param>
        private static void ProcessUpdatedAssociations(ModuleCollectionChange changes, TableMappingCollection tableMappings, Dictionary<Guid, string> associationMappings, StringCollection queries)
        {
            // We handle only cardinality updates here. Assumption here is no change other than
            // multiplicities is done on the updated associations. For example, we assume that
            // the association navigation properties are not changed and the navigation property
            // parents are also the same etc. Also, the multiplicity updates are compatible in 
            // nature.
            // 
            // The view corresponding to nextAssociation will be dropped or created with the same
            // name as earlier.

            foreach (KeyValuePair<Association, Association> kvp in changes.UpdatedAssociations)
            {
                Association previousAssociation = kvp.Key;
                Association nextAssociation = kvp.Value;

                // We can assume that the previous data model was validated and the 
                // previous association was not OneToOne. Below we check for next association.
                if (nextAssociation.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                    nextAssociation.ObjectMultiplicity == AssociationEndMultiplicity.One)
                    throw new ZentityException(DataModellingResources.InvalidMultiplicityOneToOne);

                // Handle dropping of FKs for OneToXXX to YYYToZZZ upgrades.
                if (previousAssociation.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                    nextAssociation.SubjectMultiplicity != AssociationEndMultiplicity.One)
                {
                    DropFKForNavigationProperty(previousAssociation.ObjectNavigationProperty,
                        tableMappings, queries);
                }

                // Handle dropping of FKs for XXXToOne to YYYToZZZ upgrades.
                if (previousAssociation.ObjectMultiplicity == AssociationEndMultiplicity.One &&
                    nextAssociation.ObjectMultiplicity != AssociationEndMultiplicity.One)
                {
                    DropFKForNavigationProperty(previousAssociation.SubjectNavigationProperty,
                        tableMappings, queries);
                }

                // Handle dropping of Views.
                ProcessDeletedAssociation(queries, previousAssociation, associationMappings);

                // Create Views and Procedures for new association. Do not create FKs.
                // For example, OneToZeroOrOne to OneToMany upgrade should recreate only 
                // Views and SPs, not the FK. FKs are created while processing added navigation
                // properties.
                if (nextAssociation.SubjectMultiplicity != AssociationEndMultiplicity.One &&
                    nextAssociation.ObjectMultiplicity != AssociationEndMultiplicity.One)
                    ProcessAddedAssociation(queries, nextAssociation, associationMappings);

                if (nextAssociation.SubjectMultiplicity == AssociationEndMultiplicity.Many &&
                    nextAssociation.ObjectMultiplicity == AssociationEndMultiplicity.One)
                    CreateViewAndIndexesForAssociation(queries, nextAssociation,
                        associationMappings, new string[] { DataModellingResources.SubjectResourceId },
                        null);
                if (nextAssociation.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                    nextAssociation.ObjectMultiplicity == AssociationEndMultiplicity.Many)
                    CreateViewAndIndexesForAssociation(queries, nextAssociation,
                        associationMappings, new string[] { DataModellingResources.ObjectResourceId },
                        null);

                if (nextAssociation.SubjectMultiplicity == AssociationEndMultiplicity.One &&
                nextAssociation.ObjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne)
                    CreateViewAndIndexesForAssociation(queries, nextAssociation,
                        associationMappings, new string[] { DataModellingResources.SubjectResourceId },
                        null);

                if (nextAssociation.SubjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne &&
                    nextAssociation.ObjectMultiplicity == AssociationEndMultiplicity.One)
                    CreateViewAndIndexesForAssociation(queries, nextAssociation,
                        associationMappings, new string[] { DataModellingResources.ObjectResourceId },
                        null);
            }
        }

        /// <summary>
        /// Processes the updated scalar properties and generate queries.
        /// </summary>
        /// <param name="changes">The changes collection.</param>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="queries">The queries collection.</param>
        private static void ProcessUpdatedScalarProperties(ModuleCollectionChange changes, TableMappingCollection tableMappings, StringCollection queries)
        {
            foreach (KeyValuePair<ScalarProperty, ScalarProperty> kvp
                in changes.UpdatedScalarProperties)
            {
                ScalarProperty previousProperty = kvp.Key;
                ScalarProperty nextProperty = kvp.Value;

                // The column corresponding to nextProperty will be created or updated within the 
                // same table as that of previousProperty. Also, there will be no change in the
                // name of table column. ScalarProperty renames are handled by changing the mapping
                // information.
                // Keeping the column names intact in the case of scalar property renames, helps in
                // simplifying the change history logic. Otherwise, we had to drop the replicated
                // columns and recreate them. We were observing transaction deadlocks because of this.
                ColumnMapping previousColumnMapping = tableMappings.GetColumnMappingByPropertyId(
                    previousProperty.Id);
                string previousColumnName = previousColumnMapping.ColumnName;
                TableMapping previousTableMapping = previousColumnMapping.Parent;
                string previousTableName = previousTableMapping.TableName;

                // Handle column definition changes.
                if (previousProperty.Parent.Id == nextProperty.Parent.Id &&
                IsCompatibleDataTypeConversion(previousProperty, nextProperty))
                {
                    queries.Add(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.AlterTableAlterColumn,
                        EscapeBrackets(previousTableName),
                        EscapeBrackets(previousColumnName),
                        GetNullableColumnDefinition(nextProperty)));
                }
                // Handle parent changes and data type incompatibilities.
                else
                {
                    // Drop and recreate the column in the same table with the same name.
                    queries.Add(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.AlterTableDropColumn,
                        EscapeBrackets(previousTableName),
                        EscapeBrackets(previousColumnName)));

                    queries.Add(string.Format(CultureInfo.InvariantCulture,
                        DataModellingResources.AlterTableAddColumn,
                        EscapeBrackets(previousTableName),
                        EscapeBrackets(previousColumnName),
                        GetNullableColumnDefinition(nextProperty)));

                    // No need to update internal lists. The column mappings do not change 
                    // even after dropping and recreating the columns.
                }
            }
        }

        /// <summary>
        /// Refreshes the Create Update Delete procedures.
        /// </summary>
        /// <param name="originalModel">The original model.</param>
        /// <param name="newModel">The new model.</param>
        /// <param name="changesForDDLs">The changes for DDLs.</param>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="discriminators">The discriminators.</param>
        /// <param name="queries">The queries colleciton.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static void RefreshCUDProcedures(DataModel originalModel, DataModel newModel, ModuleCollectionChange changesForDDLs, TableMappingCollection tableMappings, Dictionary<Guid, int> discriminators, StringCollection queries)
        {
            // Prepare a list of resource types to refresh the CUD procedures.
            // NOTE: We also create procedures for resource types that don't have any properties.
            List<Guid> updatedTypeIds = changesForDDLs.AddedResourceTypes.Select(type => type.Id).
                ToList();

            updatedTypeIds.AddRange(changesForDDLs.DeletedResourceTypes.Select(type => type.Id).
                Where(id => !updatedTypeIds.Contains(id)));

            // Add the updated types as well. The update could be a base type change in which
            // case the CUD procedures will have to handle scalar properties of new base type.
            updatedTypeIds.AddRange(changesForDDLs.UpdatedResourceTypes.Select(kvp => kvp.Key).
                Select(type => type.Id).Where(id => !updatedTypeIds.Contains(id)));

            foreach (ScalarProperty scalarProperty in changesForDDLs.AddedScalarProperties)
            {
                if (!updatedTypeIds.Contains(scalarProperty.Parent.Id))
                    updatedTypeIds.Add(scalarProperty.Parent.Id);
            }

            foreach (ScalarProperty scalarProperty in changesForDDLs.DeletedScalarProperties)
            {
                if (!updatedTypeIds.Contains(scalarProperty.Parent.Id))
                    updatedTypeIds.Add(scalarProperty.Parent.Id);
            }

            foreach (ScalarProperty scalarProperty in changesForDDLs.UpdatedScalarProperties.
                Select(kvp => kvp.Key))
            {
                if (!updatedTypeIds.Contains(scalarProperty.Parent.Id))
                    updatedTypeIds.Add(scalarProperty.Parent.Id);
            }

            foreach (NavigationProperty navigationProperty in changesForDDLs.
                AddedNavigationProperties)
            {
                if (!updatedTypeIds.Contains(navigationProperty.Parent.Id))
                    updatedTypeIds.Add(navigationProperty.Parent.Id);
            }

            foreach (NavigationProperty navigationProperty in changesForDDLs.
                DeletedNavigationProperties)
            {
                if (!updatedTypeIds.Contains(navigationProperty.Parent.Id))
                    updatedTypeIds.Add(navigationProperty.Parent.Id);
            }

            foreach (NavigationProperty navigationProperty in changesForDDLs.
                UpdatedNavigationProperties.Select(kvp => kvp.Key))
            {
                if (!updatedTypeIds.Contains(navigationProperty.Parent.Id))
                    updatedTypeIds.Add(navigationProperty.Parent.Id);
            }

            // NOTE: We always use the latest definition of resource type to create CUD procedures.
            foreach (Guid updatedTypeId in updatedTypeIds)
            {
                ResourceType earlierType = originalModel.Modules.
                    SelectMany(module => module.ResourceTypes).
                    Where(type => type.Id == updatedTypeId).FirstOrDefault();
                if (earlierType != null)
                    DeleteCUDProcedures(earlierType, queries);
                ResourceType newType = newModel.Modules.
                    SelectMany(module => module.ResourceTypes).
                    Where(type => type.Id == updatedTypeId).FirstOrDefault();
                if (newType != null)
                {
                    AppendInsertProcedure(newType, tableMappings, discriminators, queries);
                    AppendUpdateProcedure(newType, tableMappings, queries);
                    AppendDeleteProcedure(newType, tableMappings, queries);
                }
            }
        }
    }
}
