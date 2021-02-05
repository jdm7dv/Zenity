// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System;
using System.Reflection;
using System.Text;
using System.Data.Objects;
using System.Data;
using System.Linq;
using System.Globalization;
using System.Data.SqlClient;

namespace Zentity.Core
{
    /// <summary>
    /// Contains the private class that handles the SavingChanges event and performs validations for the ZentityContext instance.
    /// </summary>
    public partial class ZentityContext
    {
        /// <summary>
        /// Handles the SavingChanges event and performs validations for the ZentityContext instance.
        /// </summary>
        private class SavingChangesHandler
        {
            /// <summary>
            /// Local copy of the ZentityContext instance.
            /// </summary>
            readonly ZentityContext parent;
            
            /// <summary>
            /// Initializes a new instance of the <see cref="SavingChangesHandler"/> class.
            /// </summary>
            /// <param name="context">The context.</param>
            internal SavingChangesHandler(ZentityContext context)
            {
                parent = context;
            }

            /// <summary>
            /// Handles the SavingChanges event raised by the ZentityContext instance.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
            internal void Handle(object sender, EventArgs e)
            {
                // Validate the Id property for each entity that is added into Zentity. 
                // Valid Id should not be Guid.Empty.
                ValidateIdForEntities();

                // Set the DateCreated and DateModified on the entities.
                UpdateDateCreatedAndDateModified();

                // TODO: For now we do not allow explicit Relationship updates for OneToZeroOrOne 
                // and ZeroOrOneToOne updates, but we can ensure the consistency here and allow it
                // in future. To understand better, let's consider a OneToZeroOrOne association
                // with these relationships:
                // A <--> 1, B <--> 2.
                // Updating these relationships to reach a state where 'A' is related
                // to '2' and 'B' is related to '1' requires the following steps:
                // Step1: Relate A with 2, i.e. update B <--> 2 to A <--> 2
                // Step2: Relate B with 1, i.e. update A <--> 1 to B <--> 1
                // Now after step 1, we have A associated with more than one entities,
                // thus we cannot create a unique constraint on [ObjectResourceId] on the view 
                // created for the association. In the absence of such a constraint, it would be
                // possible to insert incorrect rows in Relationship table. We prevent this by 
                // adding a check in [Core].[UpdateRelationship] procedure. We do not allow 
                // explicit updates of OneToZeroOrOne and ZeroOrOneToOne associations.

                // Ensure that OneToXXX relationships and XXX side resources are deleted together.
                ValidateOneToXxxRelationshipDeletions();
            }

            /// <summary>
            /// Validates the Id for entities. The Id for the entity must be be Guid.Empty
            /// </summary>
            private void ValidateIdForEntities()
            {
                // Validate the Id property of the entities being Added.
                foreach (ObjectStateEntry entry in parent.ObjectStateManager.GetObjectStateEntries(EntityState.Added))
                {
                    // Check if the entry has a valid entity.
                    if (entry.Entity == null)
                        continue;

                    // Reflect on the Id property.
                    Type entityType = entry.Entity.GetType();
                    PropertyInfo propId = entityType.GetProperty(CoreResources.Id);

                    if (propId != null)
                    {
                        // Get the property value through reflection and check if the Id value is not Guid.Empty else
                        // throw an exception.
                        Guid entityId = (Guid) propId.GetValue(entry.Entity, null);
                        if (entityId == Guid.Empty)
                        {
                            throw new InvalidOperationException(CoreResources.InvalidIdForEntity);
                        }
                    }
                }
            }

            /// <summary>
            /// Validates the one to XXX relationship deletions.
            /// </summary>
            private void ValidateOneToXxxRelationshipDeletions()
            {
                // NOTE: Rather than relying on object state entries here, we ask the store to do
                // the verification. So, we don't have to worry about the related ends being loaded
                // in memory for the Relationship object under consideration. However, this is a 
                // performance hit. 
                StringBuilder csvDeletedRelationshipIds = new StringBuilder();
                foreach (ObjectStateEntry entry in parent.ObjectStateManager.GetObjectStateEntries(
                    EntityState.Deleted).Where(tuple => tuple.Entity is Relationship))
                    // DO NOT append comma with spaces.
                    csvDeletedRelationshipIds.Append("," + (entry.Entity as Relationship).Id.ToString());

                if (csvDeletedRelationshipIds.Length > 0)
                    csvDeletedRelationshipIds = csvDeletedRelationshipIds.Remove(0, 1);
                else
                    return;

                StringBuilder csvDeletedResourceIds = new StringBuilder();
                foreach (ObjectStateEntry entry in parent.ObjectStateManager.GetObjectStateEntries(
                    EntityState.Deleted).Where(tuple => tuple.Entity is Resource))
                    // DO NOT append comma with spaces.
                    csvDeletedResourceIds.Append("," + (entry.Entity as Resource).Id.ToString());

                if (csvDeletedResourceIds.Length > 0)
                    csvDeletedResourceIds = csvDeletedResourceIds.Remove(0, 1);

                // Use a separate connection for validations that doesn't block regular operations
                // other threads are performing using EntityConnection. Don't directly lock the
                // SqlConnection here, else you'll get CA2002:DoNotLockOnObjectsWithWeakIdentity.
                // Locking here is necessary for multi-threaded scenarios. For example, 
                // two threads entering this block at the same time and first one opens entity 
                // connection, second gets an opened connection. Now first one is finished and 
                // closes the connection since it opened it, but second one is still not finished. 
                // Since the connection is closed, second thread will fail. 
                lock (parent.validationConnectionLock)
                {
                    bool isConnectionOpenedHere = false;
                    if (parent.validationConnection.State == ConnectionState.Closed)
                    {
                        parent.validationConnection.Open();
                        isConnectionOpenedHere = true;
                    }
                    try
                    {
                        using (SqlCommand cmd = parent.validationConnection.CreateCommand())
                        {
                            cmd.CommandText = CoreResources.Core_ValidateOneToXxxDeletion;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandTimeout = parent.OperationTimeout;

                            cmd.Parameters.Add(CoreResources.CsvDeletedRelationshipIds,
                                SqlDbType.NVarChar, -1);
                            cmd.Parameters[CoreResources.CsvDeletedRelationshipIds].Value =
                                csvDeletedRelationshipIds.ToString();

                            cmd.Parameters.Add(CoreResources.CsvDeletedResourceIds,
                                SqlDbType.NVarChar, -1);
                            cmd.Parameters[CoreResources.CsvDeletedResourceIds].Value =
                                csvDeletedResourceIds.ToString();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    finally
                    {
                        if (isConnectionOpenedHere)
                            parent.validationConnection.Close();
                    }
                }
            }

            /// <summary>
            /// Updates the date created and date modified for entities.
            /// </summary>
            private void UpdateDateCreatedAndDateModified()
            {
                DateTime now = DateTime.Now;
                foreach (ObjectStateEntry entry in parent.ObjectStateManager.GetObjectStateEntries(
                    EntityState.Added).Where(tuple => tuple.Entity is Resource ||
                    tuple.Entity is Relationship))
                {
                    Resource resource = entry.Entity as Resource;
                    if (resource != null)
                    {
                        if (resource.DateAdded == null)
                            resource.DateAdded = now;
                        if (resource.DateModified == null)
                            resource.DateModified = now;
                        continue;
                    }

                    Relationship relationship = entry.Entity as Relationship;
                    if (relationship != null && relationship.DateAdded == null)
                    {
                        relationship.DateAdded = now;
                    }
                }

                foreach (ObjectStateEntry entry in parent.ObjectStateManager.GetObjectStateEntries(
                    EntityState.Modified).Where(tuple => tuple.Entity is Resource))
                {
                    Resource resource = entry.Entity as Resource;
                    if (resource.DateModified == null ||
                        Convert.ToDateTime(entry.OriginalValues[CoreResources.DateModified],
                        CultureInfo.InvariantCulture) ==
                        Convert.ToDateTime(entry.CurrentValues[CoreResources.DateModified],
                        CultureInfo.InvariantCulture))
                        resource.DateModified = now;
                }
            }
        }
    }
}
