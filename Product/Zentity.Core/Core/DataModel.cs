// *********************************************************
// 
//     Copyright (c) Microsoft. All rights reserved.
//     This code is licensed under the Apache License, Version 2.0.
//     THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//     ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//     IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//     PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// 
// *********************************************************

using System;
using System.Collections.Specialized;
using System.Xml;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Globalization;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using System.IO;
using System.Data.Entity.Design;
using System.Data.Metadata.Edm;
using System.Transactions;
using System.Threading;
using System.Collections.ObjectModel;
using System.Security.Cryptography;

namespace Zentity.Core
{
    /// <summary>
    /// Represents the Zentity Data Model for a store.
    /// </summary>
    /// <remarks>Zentity data model is a way to define the domain-specific resource types and 
    /// associations in the system. Since Zentity is built on top of Entity Framework, we tried 
    /// to stay as close as possible to the Entity Data Model (EDM). Most of the concepts here 
    /// are very similar in nature to the Entity Data Model. Table below shows the correspondence 
    /// between the two models.
    /// <br/>
    /// <table border="1">
    /// <tr><td>Zentity Data Model (ZDM) Construct</td><td>Entity Data Model (EDM) Construct</td></tr>
    /// <tr><td>Resource Type</td><td>Entity</td></tr>
    /// <tr><td>Scalar Property</td><td>Scalar Property</td></tr>
    /// <tr><td>Navigation Property</td><td>Navigation Property</td></tr>
    /// <tr><td>Association</td><td>Association</td></tr>
    /// </table>
    /// <br/>
    /// Methods in this class work on ZDM constructs and generate the EDM constructs in addition 
    /// to other items. The generated EDM, has all its entities deriving directly or indirectly 
    /// from ‘Resource’ entity in the Zentity Core EDM. There is a single ZDM associated with each 
    /// store. Multiple data model modules can be present in a ZDM. Figure below shows the Zentity
    /// Data Model elements.
    /// <br/>
    /// <img src="ZDM.bmp"/>
    /// <br/>
    /// </remarks>
    /// <example>
    /// <code>
    ///using Zentity.Core;
    ///using System;
    ///using System.Data.EntityClient;
    ///
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        public static void Main(string[] args)
    ///        {
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                PrintModelElements(context);
    ///
    ///                // Update the model.
    ///                DataModelModule module = new DataModelModule { NameSpace = &quot;Namespace&quot; + Guid.NewGuid().ToString(&quot;N&quot;) };
    ///                context.DataModel.Modules.Add(module);
    ///
    ///                ResourceType resourceTypeResource = context.DataModel.Modules[&quot;Zentity.Core&quot;].ResourceTypes[&quot;Resource&quot;];
    ///                ResourceType resourceTypeScholarlyWork = new ResourceType { Name = &quot;ScholarlyWork&quot;, BaseType = resourceTypeResource };
    ///                module.ResourceTypes.Add(resourceTypeScholarlyWork);
    ///
    ///                ScalarProperty copyRight = new ScalarProperty { Name = &quot;CopyRight&quot;, DataType = DataTypes.String, MaxLength = 4000 };
    ///                resourceTypeScholarlyWork.ScalarProperties.Add(copyRight);
    ///
    ///                // This method sometimes takes a few minutes to complete depending on the actions
    ///                // taken by other modules (such as change history logging) in response to schema
    ///                // changes. Everything happens in a single transaction. Make sure that the timeout 
    ///                // values are set correct for the command and transaction. Transaction timeout is 
    ///                // controlled from App.Config, Web.Config and machine.config configuration files.
    ///                context.CommandTimeout = 300;
    ///                context.DataModel.Synchronize();
    ///
    ///                // Print the model again.
    ///                PrintModelElements(context);
    ///            }
    ///        }
    ///
    ///        private static void PrintModelElements(ZentityContext context)
    ///        {
    ///            Console.WriteLine(&quot;Data Model details for store [{0}\\{1}], Version [{2}]&quot;,
    ///                ((EntityConnection)context.Connection).StoreConnection.DataSource,
    ///                ((EntityConnection)context.Connection).StoreConnection.Database,
    ///                context.GetConfiguration(&quot;ZentityVersion&quot;));
    ///
    ///            foreach (DataModelModule module in context.DataModel.Modules)
    ///            {
    ///                Console.WriteLine(&quot;Module: [{0}]&quot;, module.NameSpace);
    ///                foreach (ResourceType resourceType in module.ResourceTypes)
    ///                {
    ///                    Console.WriteLine(&quot;\tResourceType: [{0}]&quot;, resourceType.Name);
    ///                    foreach (ScalarProperty scalarProperty in resourceType.ScalarProperties)
    ///                    {
    ///                        Console.WriteLine(&quot;\t\tScalarProperty: [{0}]&quot;, scalarProperty.Name);
    ///                    }
    ///                    foreach (NavigationProperty navProperty in resourceType.NavigationProperties)
    ///                    {
    ///                        Console.WriteLine(&quot;\t\tScalarProperty: [{0}]&quot;, navProperty.Name);
    ///                    }
    ///                }
    ///                foreach (Association assoc in module.Associations)
    ///                {
    ///                    Console.WriteLine(&quot;\tAssociation: [{0}]&quot;, assoc.Name);
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public sealed partial class DataModel
    {
        #region Fields

        DataModelModuleCollection modules;
        ZentityContext parent;
        int maxDiscriminator;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collection of modules that form the data model.
        /// </summary>
        public DataModelModuleCollection Modules
        {
            get
            {
                if (modules == null)
                {
                    this.Refresh();
                }

                return modules;
            }
        }

        /// <summary>
        /// Gets the parent context.
        /// </summary>
        public ZentityContext Parent
        {
            get { return parent; }
        }

        internal int MaxDiscriminator
        {
            get { return maxDiscriminator; }
            set { maxDiscriminator = value; }
        }

        #endregion

        #region Constructors

        internal DataModel(ZentityContext parent)
        {
            this.parent = parent;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the TSQL script to synchronize the in-memory data model with the backend.
        /// It is strongly recommended that you take a backup of your database before executing 
        /// these scripts and run the scripts in a transaction. Also, consider executing the
        /// <see cref="Zentity.Core.DataModel.Refresh()"/> method after executing the scripts to
        /// reload the mapping information from the backend.
        /// </summary>
        /// <returns>TSQL script to synchronize the in-memory data model with the backend.
        /// </returns>
        /// <example> The example below shows how to generate the scripts that synchronize the
        /// in-memory data model with the backend store in a transaction. 
        /// <code>
        ///using Zentity.Core;
        ///using System.Xml;
        ///using System.IO;
        ///using System.Text;
        ///using System.Collections.Specialized;
        ///using System.Transactions;
        ///using System.Data.EntityClient;
        ///using System.Configuration;
        ///using System.Data.SqlClient;
        ///using System.Data;
        ///using System;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        public static void Main(string[] args)
        ///        {
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                string namespaceName = &quot;Namespace&quot; + Guid.NewGuid().ToString(&quot;N&quot;);
        ///
        ///                // Create a new module.
        ///                DataModelModule module = new DataModelModule { NameSpace = namespaceName };
        ///                context.DataModel.Modules.Add(module);
        ///
        ///                // Create the ScholarlyWork type.
        ///                ResourceType resourceTypeResource = context.DataModel.Modules[&quot;Zentity.Core&quot;].
        ///                    ResourceTypes[&quot;Resource&quot;];
        ///                ResourceType resourceTypeScholarlyWork = new ResourceType
        ///                {
        ///                    Name = &quot;ScholarlyWork&quot;,
        ///                    BaseType = resourceTypeResource
        ///                };
        ///                module.ResourceTypes.Add(resourceTypeScholarlyWork);
        ///
        ///                // Create some Scalar Properties.
        ///                ScalarProperty copyRight = new ScalarProperty { Name = &quot;CopyRight&quot;, DataType = DataTypes.String, MaxLength = 4000 };
        ///                resourceTypeScholarlyWork.ScalarProperties.Add(copyRight);
        ///
        ///                // Create some Navigation Properties.
        ///                NavigationProperty authors = new NavigationProperty { Name = &quot;Authors&quot; };
        ///                resourceTypeScholarlyWork.NavigationProperties.Add(authors);
        ///
        ///                // Create the Contact type.
        ///                ResourceType resourceTypeContact = new ResourceType { Name = &quot;Contact&quot;, BaseType = resourceTypeResource };
        ///                module.ResourceTypes.Add(resourceTypeContact);
        ///                ScalarProperty email = new ScalarProperty { Name = &quot;Email&quot;, DataType = DataTypes.String, MaxLength = 1024 };
        ///                resourceTypeContact.ScalarProperties.Add(email);
        ///                NavigationProperty authoredWorks = new NavigationProperty { Name = &quot;AuthoredWorks&quot; };
        ///                resourceTypeContact.NavigationProperties.Add(authoredWorks);
        ///
        ///                // Add SamplesScholarlyWorkAuthoredByContact association.
        ///                // Association names should be unique across all the modules in the data model.
        ///                Association association = new Association
        ///                {
        ///                    Name = namespaceName + &quot;_ScholarlyWorkAuthoredByContact&quot;,
        ///                    SubjectNavigationProperty = authors,
        ///                    ObjectNavigationProperty = authoredWorks,
        ///                    SubjectMultiplicity = AssociationEndMultiplicity.Many,
        ///                    ObjectMultiplicity = AssociationEndMultiplicity.Many
        ///                };
        ///
        ///                // Get the synchronization scripts.
        ///                StringCollection scripts = context.DataModel.GetSynchronizationScripts();
        ///
        ///                // Generate script.
        ///                using (StreamWriter writer = new StreamWriter(&quot;Zentity.Samples.sql&quot;))
        ///                {
        ///                    foreach (string command in scripts)
        ///                    {
        ///                        writer.WriteLine(command);
        ///                        writer.WriteLine(&quot;GO&quot;);
        ///                    }
        ///                    writer.Close();
        ///                }
        ///
        ///                // IMPORTANT: If you intend to use the generated script, it is highly recommended 
        ///                // that you take a backup of your database and run these scripts in a transaction.
        ///                using (TransactionScope scope = new TransactionScope())
        ///                {
        ///                    EntityConnectionStringBuilder entityBuilder =
        ///                        new EntityConnectionStringBuilder(context.Connection.ConnectionString);
        ///
        ///                    if (string.IsNullOrEmpty(entityBuilder.ProviderConnectionString))
        ///                    {
        ///                        string entityConnectionString =
        ///                            ConfigurationManager.ConnectionStrings[entityBuilder.Name].ConnectionString;
        ///                        entityBuilder = new EntityConnectionStringBuilder(entityConnectionString);
        ///                    }
        ///
        ///                    using (SqlConnection storeConnection = new SqlConnection(entityBuilder.ProviderConnectionString))
        ///                    {
        ///                        storeConnection.Open();
        ///                        foreach (string str in scripts)
        ///                        {
        ///                            using (SqlCommand cmd = new SqlCommand())
        ///                            {
        ///                                cmd.Connection = storeConnection;
        ///                                cmd.CommandTimeout = 120;
        ///                                cmd.CommandText = &quot;sp_executesql&quot;;
        ///                                cmd.CommandType = CommandType.StoredProcedure;
        ///
        ///                                SqlParameter param = cmd.CreateParameter();
        ///                                param.DbType = DbType.String;
        ///                                param.Direction = ParameterDirection.Input;
        ///                                param.ParameterName = &quot;Cmd&quot;;
        ///                                param.Size = -1;
        ///                                param.Value = str;
        ///                                cmd.Parameters.Add(param);
        ///                                cmd.ExecuteNonQuery();
        ///                            }
        ///                        }
        ///                    }
        ///                    scope.Complete();
        ///                }
        ///            }
        ///        }
        ///    }
        ///}
        /// </code>
        /// </example>
        public StringCollection GetSynchronizationScripts()
        {
            TableMappingCollection tableMappings;
            Dictionary<Guid, int> discriminators;
            return GetSynchronizationScripts(out tableMappings, out discriminators);
        }

        /// <summary>
        /// Synchronizes the in-memory data model with the backend and saves the data model 
        /// information in the backend. It is recommended that you take a backup of your database 
        /// before invoking this method.
        /// </summary>
        /// <remarks> This method sometimes takes few minutes to complete. Stop SQL Agent service 
        /// if you are experiencing too long wait times. Also, consider increasing the 
        /// CommandTimeout of ZentityContext and the Transaction timeout in the application 
        /// configuration file.
        /// <br/>
        /// During synchronization, following types of database objects are created/updated/deleted.
        /// <ul>
        /// <li>Resource Tables - These tables host the actual data for the resources. Each resource is 
        /// an instance of a resource type. New columns are added to resource table with the introduction 
        /// of new scalar properties in the data model. A resource table is designed to have a maximum of 
        /// around 290 columns. Once, a resource table reaches its maximum limit a new resource table is 
        /// created in the database. Data model metadata tables store the mapping information between 
        /// resource table columns and scalar properties. Each resource table has ‘Id’ and ‘Discriminator’ 
        /// columns.</li>
        /// <li>Association Views - These views are created on Core.Relationship table for each of the 
        /// defined association in the data model. A predicate is used as a filter for the view definition. 
        /// For example, the view definition for association ‘ResourceHasFile’ looks like the following
        /// <code lang="T-SQL">
        ///CREATE VIEW [Core].[ResourceHasFile]
        ///WITH SCHEMABINDING
        ///AS
        ///	SELECT [SubjectResourceId], [ObjectResourceId]
        ///	FROM [Core].[Relationship] T
        ///	WHERE [T].[PredicateId] = '818A93F5-25A9-4149-A8D2-19104A352DA0';
        /// </code>
        /// These views are mapped to associations in the data model.</li>
        /// <li>CUD Procedures - Each resource type in the data model has one procedure each for creating, 
        /// updating and deleting the resources of that type from the repository. Likewise, Many-To-Many, 
        /// Many-To-ZeroOrOne, ZeroOrOne-To-Many and ZeroOrOne-To-ZeroOrOne associations also have these 
        /// CUD procedures to manipulate the relationships in the repository.</li>
        /// <li>Data Model Metadata Tables - These table are updated after altering the database schema to
        /// reflect the latest data model information.</li>
        /// </ul>
        /// <br/>
        /// Figure below shows the overall flow for creating custom types and associations in the system.
        /// <br/>
        /// <img src="OverallFlow.bmp"/>
        /// </remarks>
        /// <example> The following example shows how to make changes to the in-memory data
        /// model and then sychronize the changes with the backend.
        /// <code>
        ///using Zentity.Core;
        ///using System;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        public static void Main(string[] args)
        ///        {
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                string namespaceName = &quot;Namespace&quot; + Guid.NewGuid().ToString(&quot;N&quot;);
        ///
        ///                // Create a new module.
        ///                DataModelModule module = new DataModelModule { NameSpace = namespaceName };
        ///                context.DataModel.Modules.Add(module);
        ///
        ///                // Create the ScholarlyWork type.
        ///                ResourceType resourceTypeResource = context.DataModel.Modules[&quot;Zentity.Core&quot;].
        ///                    ResourceTypes[&quot;Resource&quot;];
        ///                ResourceType resourceTypeScholarlyWork = new ResourceType
        ///                {
        ///                    Name = &quot;ScholarlyWork&quot;,
        ///                    BaseType = resourceTypeResource
        ///                };
        ///                module.ResourceTypes.Add(resourceTypeScholarlyWork);
        ///
        ///                // Create some Scalar Properties.
        ///                ScalarProperty copyRight = new ScalarProperty { Name = &quot;CopyRight&quot;, DataType = DataTypes.String, MaxLength = 4000 };
        ///                resourceTypeScholarlyWork.ScalarProperties.Add(copyRight);
        ///
        ///                // Create some Navigation Properties.
        ///                NavigationProperty authors = new NavigationProperty { Name = &quot;Authors&quot; };
        ///                resourceTypeScholarlyWork.NavigationProperties.Add(authors);
        ///
        ///                // Create the Contact type.
        ///                ResourceType resourceTypeContact = new ResourceType { Name = &quot;Contact&quot;, BaseType = resourceTypeResource };
        ///                module.ResourceTypes.Add(resourceTypeContact);
        ///                ScalarProperty email = new ScalarProperty { Name = &quot;Email&quot;, DataType = DataTypes.String, MaxLength = 1024 };
        ///                resourceTypeContact.ScalarProperties.Add(email);
        ///                NavigationProperty authoredWorks = new NavigationProperty { Name = &quot;AuthoredWorks&quot; };
        ///                resourceTypeContact.NavigationProperties.Add(authoredWorks);
        ///
        ///                // Add SamplesScholarlyWorkAuthoredByContact association.
        ///                // Association names should be unique across all the modules in the data model.
        ///                Association association = new Association
        ///                {
        ///                    Name = namespaceName + &quot;_ScholarlyWorkAuthoredByContact&quot;,
        ///                    SubjectNavigationProperty = authors,
        ///                    ObjectNavigationProperty = authoredWorks,
        ///                    SubjectMultiplicity = AssociationEndMultiplicity.Many,
        ///                    ObjectMultiplicity = AssociationEndMultiplicity.Many
        ///                };
        ///
        ///                // Synchronize the in-memory data model with the backend store.
        ///                // Database objects are created/updated/deleted during synchronization.
        ///                // This method sometimes takes a few minutes to complete depending on the actions
        ///                // taken by other modules (such as change history logging) in response to schema
        ///                // changes. Everything happens in a single transaction. Make sure that the timeout 
        ///                // values are set correct for the command and transaction. Transaction timeout is 
        ///                // controlled from App.Config, Web.Config and machine.config configuration files.
        ///                context.CommandTimeout = 300;
        ///                context.DataModel.Synchronize();
        ///            }
        ///        }
        ///    }
        ///}
        /// </code>
        /// </example>
        public void Synchronize()
        {
            TableMappingCollection tableMappings;
            Dictionary<Guid, int> discriminators;
            StringCollection queries = GetSynchronizationScripts(out tableMappings, out discriminators);

            // Apply changes in a transaction.
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required))
            {
                // Create a new connection for synchronization. We are not using the ZentityContext
                // connection because the transaction management code required does not look elegant.
                // We used to do a BeginTransaction in try catch to figure out if the connection was 
                // participating in a transaction.
                int commandTimeout = this.Parent.OperationTimeout;
                using (SqlConnection storeConnection =
                    new SqlConnection(this.Parent.StoreConnectionString))
                {
                    storeConnection.Open();
                    foreach (string str in queries)
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = storeConnection;
                            cmd.CommandTimeout = commandTimeout;
                            cmd.CommandText = CoreResources.Core_SpExecuteSql;
                            cmd.CommandType = CommandType.StoredProcedure;

                            SqlParameter param = cmd.CreateParameter();
                            param.DbType = DbType.String;
                            param.Direction = ParameterDirection.Input;
                            param.ParameterName = CoreResources.Cmd;
                            param.Size = -1;
                            param.Value = str;
                            cmd.Parameters.Add(param);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                scope.Complete();
            }

            // Update mappings for scalar properties.
            // NOTE: The mappings are not updated by the SqlScriptGenerator. It is not
            // suppose to cause any side effects to the passed models. It however, 
            // updates the passed in mappings to point to newer values.
            foreach (ScalarProperty scalarProperty in this.Modules.
                SelectMany(tuple => tuple.ResourceTypes).
                SelectMany(tuple => tuple.ScalarProperties))
            {
                ColumnMapping columnMapping = tableMappings.GetColumnMappingByPropertyId(
                    scalarProperty.Id);
                scalarProperty.TableName = columnMapping.Parent.TableName;
                scalarProperty.ColumnName = columnMapping.ColumnName;
            }

            // Update mappings for navigation properties.
            foreach (NavigationProperty navigationProperty in this.Modules.
                SelectMany(tuple => tuple.ResourceTypes).
                SelectMany(tuple => tuple.NavigationProperties))
            {
                ColumnMapping columnMapping = tableMappings.GetColumnMappingByPropertyId(
                    navigationProperty.Id);
                if (columnMapping != null)
                {
                    navigationProperty.TableName = columnMapping.Parent.TableName;
                    navigationProperty.ColumnName = columnMapping.ColumnName;
                }
            }

            // Update Discriminators.
            foreach (ResourceType type in this.modules.
                SelectMany(tuple => tuple.ResourceTypes))
            {
                type.Discriminator = discriminators[type.Id];
            }

            // NOTE: DO NOT Refresh() the data model here. It may break the clients that have
            // cached the data model items. For example, if an application has reference to
            // Publication resource type and we refresh the model here, the cached reference 
            // on client side becomes stale and might result into errors if used further.
        }

        /// <summary>
        /// Returns the Entity Framework artifacts for a set of modules. Artifacts can be generated
        /// only for synchronized data model modules.
        /// </summary>
        /// <param name="modulesToInclude">Namespaces of the modules to include in the generated 
        /// artifacts. If this parameter is null, all synchronized modules of the data model are 
        /// included in the list.</param>
        /// <returns>Entity Framework artifacts for a set of synchronized modules.</returns>
        /// <example> The example below shows how to generate the Entity Framework artifacts for a
        /// custom data model module.
        /// <code>
        ///using Zentity.Core;
        ///using System.Xml;
        ///using System;
        ///using System.Linq;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        public static void Main(string[] args)
        ///        {
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                string namespaceName = &quot;Namespace&quot; + Guid.NewGuid().ToString(&quot;N&quot;);
        ///
        ///                // Create a new module.
        ///                DataModelModule module = new DataModelModule { NameSpace = namespaceName };
        ///                context.DataModel.Modules.Add(module);
        ///
        ///                // Create the ScholarlyWork type.
        ///                ResourceType resourceTypeResource = context.DataModel.Modules[&quot;Zentity.Core&quot;].ResourceTypes[&quot;Resource&quot;];
        ///                ResourceType resourceTypeScholarlyWork = new ResourceType { Name = &quot;ScholarlyWork&quot;, BaseType = resourceTypeResource };
        ///                module.ResourceTypes.Add(resourceTypeScholarlyWork);
        ///
        ///                // Create some Scalar Properties.
        ///                ScalarProperty copyRight = new ScalarProperty { Name = &quot;CopyRight&quot;, DataType = DataTypes.String, MaxLength = 4000 };
        ///                resourceTypeScholarlyWork.ScalarProperties.Add(copyRight);
        ///
        ///                // Create some Navigation Properties.
        ///                NavigationProperty authors = new NavigationProperty { Name = &quot;Authors&quot; };
        ///                resourceTypeScholarlyWork.NavigationProperties.Add(authors);
        ///
        ///                // Create the Contact type.
        ///                ResourceType resourceTypeContact = new ResourceType { Name = &quot;Contact&quot;, BaseType = resourceTypeResource };
        ///                module.ResourceTypes.Add(resourceTypeContact);
        ///                ScalarProperty email = new ScalarProperty { Name = &quot;Email&quot;, DataType = DataTypes.String, MaxLength = 1024 };
        ///                resourceTypeContact.ScalarProperties.Add(email);
        ///                NavigationProperty authoredWorks = new NavigationProperty { Name = &quot;AuthoredWorks&quot; };
        ///                resourceTypeContact.NavigationProperties.Add(authoredWorks);
        ///
        ///                // Add SamplesScholarlyWorkAuthoredByContact association.
        ///                // Association names should be unique across all the modules in the data model.
        ///                Association association = new Association
        ///                {
        ///                    Name = namespaceName + &quot;_ScholarlyWorkAuthoredByContact&quot;,
        ///                    SubjectNavigationProperty = authors,
        ///                    ObjectNavigationProperty = authoredWorks,
        ///                    SubjectMultiplicity = AssociationEndMultiplicity.Many,
        ///                    ObjectMultiplicity = AssociationEndMultiplicity.Many
        ///                };
        ///
        ///
        ///                // Synchronize to alter the database schema.
        ///                // It is important to synchronize before we can generate the Entity Framework
        ///                // artifacts. The table and column mappings are assigned while synchronizing and
        ///                // cannot be computed in advance.
        ///                // This method sometimes takes a few minutes to complete depending on the actions
        ///                // taken by other modules (such as change history logging) in response to schema
        ///                // changes. Everything happens in a single transaction. Make sure that the timeout 
        ///                // values are set correct for the command and transaction. Transaction timeout is 
        ///                // controlled from App.Config, Web.Config and machine.config configuration files.
        ///                context.CommandTimeout = 300;
        ///                context.DataModel.Synchronize();
        ///
        ///                // Generate the Entity Framework artifacts for all the modules in the model.
        ///                EFArtifactGenerationResults results = context.DataModel.GenerateEFArtifacts(null);
        ///
        ///                // Dump the CSDLs.
        ///                foreach (var kvp in results.Csdls)
        ///                    kvp.Value.Save(kvp.Key + &quot;.csdl&quot;);
        ///
        ///                // Dump the consolidated SSDL.
        ///                results.Ssdl.Save(&quot;Consolidated.ssdl&quot;);
        ///
        ///                // Dump the consolidated MSL.
        ///                results.Msl.Save(&quot;Consolidated.msl&quot;);
        ///
        ///                // Generate the Entity Framework artifacts that can be used with just Core and 
        ///                // this module.
        ///                results = context.DataModel.GenerateEFArtifacts(namespaceName);
        ///
        ///                // Dump the CSDLs.
        ///                XmlDocument extendedCoreCsdl = results.Csdls.
        ///                    Where(tuple =&gt; tuple.Key == &quot;Zentity.Core&quot;).First().Value;
        ///                XmlDocument moduleCsdl = results.Csdls.
        ///                    Where(tuple =&gt; tuple.Key == namespaceName).First().Value;
        ///                extendedCoreCsdl.Save(&quot;ExtendedCore.csdl&quot;);
        ///                moduleCsdl.Save(&quot;Module.csdl&quot;);
        ///
        ///                // Dump the consolidated SSDL.
        ///                results.Ssdl.Save(&quot;Consolidated1.ssdl&quot;);
        ///
        ///                // Dump the consolidated MSL.
        ///                results.Msl.Save(&quot;Consolidated1.msl&quot;);
        ///            }
        ///        }
        ///    }
        ///}
        /// </code>
        /// <br/>
        /// The generated CSDL for Zentity.Core namespace is an extended CSDL file that contains 
        /// additional AssociationSet elements for all the associations in the data model 
        /// irrespective of their modules. The snippet below shows the AssociationSet element for 
        /// the association between ScholarlyWork and Contact in the example above.
        /// <code lang="xml">
        ///&lt;AssociationSet Name=&quot;Namespace459f3787f18d4c45abe76d3f261344fb_ScholarlyWorkAuthoredByContact&quot; Association=&quot;Namespace459f3787f18d4c45abe76d3f261344fb.Namespace459f3787f18d4c45abe76d3f261344fb_ScholarlyWorkAuthoredByContact&quot;&gt;
        ///  &lt;End Role=&quot;ScholarlyWork&quot; EntitySet=&quot;Resources&quot; /&gt;
        ///  &lt;End Role=&quot;Contact&quot; EntitySet=&quot;Resources&quot; /&gt;
        ///&lt;/AssociationSet&gt;
        /// </code>
        /// <br/>
        /// The generated CSDL for the custom module is shown below.
        /// <code lang="xml">
        ///&lt;Schema Namespace=&quot;Namespace459f3787f18d4c45abe76d3f261344fb&quot; Alias=&quot;Self&quot; xmlns=&quot;http://schemas.microsoft.com/ado/2006/04/edm&quot;&gt;
        ///  &lt;EntityType Name=&quot;ScholarlyWork&quot; BaseType=&quot;Zentity.Core.Resource&quot;&gt;
        ///    &lt;Property Name=&quot;CopyRight&quot; Type=&quot;String&quot; Nullable=&quot;true&quot; Unicode=&quot;true&quot; MaxLength=&quot;4000&quot; FixedLength=&quot;false&quot; /&gt;
        ///    &lt;NavigationProperty Name=&quot;Authors&quot; Relationship=&quot;Namespace459f3787f18d4c45abe76d3f261344fb.Namespace459f3787f18d4c45abe76d3f261344fb_ScholarlyWorkAuthoredByContact&quot; FromRole=&quot;ScholarlyWork&quot; ToRole=&quot;Contact&quot; /&gt;
        ///  &lt;/EntityType&gt;
        ///  &lt;EntityType Name=&quot;Contact&quot; BaseType=&quot;Zentity.Core.Resource&quot;&gt;
        ///    &lt;Property Name=&quot;Email&quot; Type=&quot;String&quot; Nullable=&quot;true&quot; Unicode=&quot;true&quot; MaxLength=&quot;1024&quot; FixedLength=&quot;false&quot; /&gt;
        ///    &lt;NavigationProperty Name=&quot;AuthoredWorks&quot; Relationship=&quot;Namespace459f3787f18d4c45abe76d3f261344fb.Namespace459f3787f18d4c45abe76d3f261344fb_ScholarlyWorkAuthoredByContact&quot; FromRole=&quot;Contact&quot; ToRole=&quot;ScholarlyWork&quot; /&gt;
        ///  &lt;/EntityType&gt;
        ///  &lt;Association Name=&quot;Namespace459f3787f18d4c45abe76d3f261344fb_ScholarlyWorkAuthoredByContact&quot;&gt;
        ///    &lt;End Role=&quot;ScholarlyWork&quot; Type=&quot;Namespace459f3787f18d4c45abe76d3f261344fb.ScholarlyWork&quot; Multiplicity=&quot;*&quot; /&gt;
        ///    &lt;End Role=&quot;Contact&quot; Type=&quot;Namespace459f3787f18d4c45abe76d3f261344fb.Contact&quot; Multiplicity=&quot;*&quot; /&gt;
        ///  &lt;/Association&gt;
        ///&lt;/Schema&gt;
        /// </code>
        /// </example>
        /// <remarks>
        /// This method ignores the Core module if specified in the parameter list. Input modules
        /// are sorted in the order of their dependency before artifact generation.
        /// <br/>
        /// The result includes a list of CSDL documents. The CSDL corresponding to Core is 
        /// enhanced to also include AssociationSet elements for all associations present in the 
        /// input module list. Rest of the items in CSDL list contain a CSDL per input module. 
        /// Finally, the results include consolidated SSDL and MSL documents for the input modules.
        /// </remarks>
        public EFArtifactGenerationResults GenerateEFArtifacts(params string[] modulesToInclude)
        {
            // Validate the model.
            this.Validate();

            // Verify all the modules to include are present in the model.
            List<string> inputNamespaces = (modulesToInclude == null ||
                modulesToInclude != null && modulesToInclude.Length == 0) ?
                this.Modules.Select(tuple => tuple.NameSpace).ToList() :
                modulesToInclude.ToList();

            var nameSpacesInGraph = this.Modules.Select(tuple => tuple.NameSpace);
            var absentModules = inputNamespaces.Except(nameSpacesInGraph);
            if (absentModules.Count() > 0)
            {
                var absentModule = absentModules.First();
                throw new ZentityException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ExceptionAbsentModuleNamespace, absentModule));
            }

            // Validate that the modules in input list are synchronized.
            DataModel synchronizedModel;
            TableMappingCollection tableMappings;
            Dictionary<Guid, int> discriminators;
            GetSynchronizedModelAndMappings(out synchronizedModel, out tableMappings,
                out discriminators);

            // We can generate artifacts only for the synchronized modules.
            DetectUnsynchronizedModules(synchronizedModel, this, inputNamespaces);

            EFArtifactGenerationResults results = new EFArtifactGenerationResults();
            Assembly zentityAssembly = typeof(Resource).Assembly;

            // Load initial values.
            results.Ssdl = PrepareXmlDocumentFromManifestResourceStream(zentityAssembly,
                CoreResources.SSDLManifestResourceName);

            results.Csdls.Add(new KeyValuePair<string, XmlDocument>(CoreResources.ZentityCore,
                PrepareXmlDocumentFromManifestResourceStream(zentityAssembly,
                CoreResources.CSDLManifestResourceName)));

            results.Msl = PrepareXmlDocumentFromManifestResourceStream(zentityAssembly,
                CoreResources.MSLManifestResourceName);

            // Translate from namespaces to modules.
            List<DataModelModule> inputModules = this.Modules.
                Where(tuple => inputNamespaces.Contains(tuple.NameSpace)).ToList();

            // This method need not work on sorted modules. The EF artifacts can be
            // generated even if the modules are not sorted. But callers of this method
            // usually have the "sorted modules" requirement. So, we sort the modules here.
            List<DataModelModule> sortedModules = SortModules(inputModules);

            for (int i = 0; i < sortedModules.Count(); i++)
            {
                DataModelModule item = sortedModules[i];

                // Ignore Core module.
                if (item.NameSpace == CoreResources.ZentityCore)
                    continue;

                // Update the SSDL.
                item.UpdateSsdl(results.Ssdl, tableMappings);

                // Create a new CSDL and add it to results.
                XmlDocument csdlDocument = new XmlDocument();
                XmlElement schemaElement = csdlDocument.CreateElement(CoreResources.Schema,
                    CoreResources.CSDLSchemaNameSpace);
                csdlDocument.AppendChild(schemaElement);
                Utilities.AddAttribute(schemaElement, CoreResources.Namespace, item.NameSpace);
                Utilities.AddAttribute(schemaElement, CoreResources.Alias, CoreResources.Self);
                results.Csdls.Add(new KeyValuePair<string, XmlDocument>(item.NameSpace,
                    csdlDocument));

                // Update the Core CSDL and the module specific CSDL.
                item.UpdateCsdls(results.Csdls.Where(tuple =>
                    tuple.Key == CoreResources.ZentityCore).First().Value, csdlDocument);

                // Update MSL.
                // NOTE: We are passing the tableMappings and the discriminators retrieved
                // from the synchronized model. We cannot rely on the in-memory graph here
                // since it might still not be refreshed. For example, if a user creates an
                // in memory model and generates sql scripts for it and then applies those
                // scripts to the database, the backend column mappings are updated but the
                // in memory model still does not have any mapping information. Thus, we pass
                // the mappings information that we got from the backend to MSL update procedures.
                string storageSchemaName = results.Ssdl[CoreResources.Schema].
                    Attributes[CoreResources.Namespace].Value;
                item.UpdateMsl(results.Msl, tableMappings, discriminators, storageSchemaName);
            }

            return results;
        }

        /// <summary>
        /// Generates the C# source code for the specified modules.
        /// </summary>
        /// <param name="modulesToInclude">A list of module namespaces to generate source code 
        /// for. If this parameter is null, all modules of the data model are included in 
        /// the list.</param>
        /// <returns>Source code for each module defined in the input parameter.</returns>
        /// <example> The example below shows how to generate the source code for a custom data
        /// model module.
        /// <code>
        ///using Zentity.Core;
        ///using System.Xml;
        ///using System.IO;
        ///using System.Text;
        ///using System.Collections.Specialized;
        ///using System;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        public static void Main(string[] args)
        ///        {
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                string namespaceName = &quot;Namespace&quot; + Guid.NewGuid().ToString(&quot;N&quot;);
        ///
        ///                // Create a new module.
        ///                DataModelModule module = new DataModelModule { NameSpace = namespaceName };
        ///                context.DataModel.Modules.Add(module);
        ///
        ///                // Create the ScholarlyWork type.
        ///                ResourceType resourceTypeResource = context.DataModel.Modules[&quot;Zentity.Core&quot;].ResourceTypes[&quot;Resource&quot;];
        ///                ResourceType resourceTypeScholarlyWork = new ResourceType { Name = &quot;ScholarlyWork&quot;, BaseType = resourceTypeResource };
        ///                module.ResourceTypes.Add(resourceTypeScholarlyWork);
        ///
        ///                // Create some Scalar Properties.
        ///                ScalarProperty copyRight = new ScalarProperty { Name = &quot;CopyRight&quot;, DataType = DataTypes.String, MaxLength = 4000 };
        ///                resourceTypeScholarlyWork.ScalarProperties.Add(copyRight);
        ///
        ///                // Create some Navigation Properties.
        ///                NavigationProperty authors = new NavigationProperty { Name = &quot;Authors&quot; };
        ///                resourceTypeScholarlyWork.NavigationProperties.Add(authors);
        ///
        ///                // Create the Contact type.
        ///                ResourceType resourceTypeContact = new ResourceType { Name = &quot;Contact&quot;, BaseType = resourceTypeResource };
        ///                module.ResourceTypes.Add(resourceTypeContact);
        ///                ScalarProperty email = new ScalarProperty { Name = &quot;Email&quot;, DataType = DataTypes.String, MaxLength = 1024 };
        ///                resourceTypeContact.ScalarProperties.Add(email);
        ///                NavigationProperty authoredWorks = new NavigationProperty { Name = &quot;AuthoredWorks&quot; };
        ///                resourceTypeContact.NavigationProperties.Add(authoredWorks);
        ///
        ///                // Add SamplesScholarlyWorkAuthoredByContact association.
        ///                // Association names should be unique across all the modules in the data model.
        ///                Association association = new Association
        ///                {
        ///                    Name = namespaceName + &quot;_ScholarlyWorkAuthoredByContact&quot;,
        ///                    SubjectNavigationProperty = authors,
        ///                    ObjectNavigationProperty = authoredWorks,
        ///                    SubjectMultiplicity = AssociationEndMultiplicity.Many,
        ///                    ObjectMultiplicity = AssociationEndMultiplicity.Many
        ///                };
        ///
        ///                // Synchronize to alter the database schema.
        ///                // It is important to synchronize before we can generate the Entity Framework
        ///                // artifacts. The table and column mappings are assigned while synchronizing and
        ///                // cannot be computed in advance.
        ///                // This method sometimes takes a few minutes to complete depending on the actions
        ///                // taken by other modules (such as change history logging) in response to schema
        ///                // changes. Everything happens in a single transaction. Make sure that the timeout 
        ///                // values are set correct for the command and transaction. Transaction timeout is 
        ///                // controlled from App.Config, Web.Config and machine.config configuration files.
        ///                context.CommandTimeout = 300;
        ///                context.DataModel.Synchronize();
        ///
        ///                // Generate source code for just a single module.
        ///                using (StreamWriter writer = new StreamWriter(namespaceName + &quot;.cs&quot;))
        ///                {
        ///                    foreach (string str in context.DataModel.GenerateSourceCode(namespaceName))
        ///                        writer.WriteLine(str);
        ///                    writer.Close();
        ///                }
        ///
        ///                // Generate source code for all modules (except MsShipped).
        ///                using (StreamWriter writer = new StreamWriter(&quot;AllModules.cs&quot;))
        ///                {
        ///                    foreach (string str in context.DataModel.GenerateSourceCode(null))
        ///                        writer.WriteLine(str);
        ///                    writer.Close();
        ///                }
        ///            }
        ///        }
        ///    }
        ///}
        /// </code>
        /// <br/>
        /// The snippet below presents an idea of the generated output.
        /// <code>
        ///...
        ///namespace Namespace7577e96f4c0c4540a679fe75340e76f1
        ///{
        ///    ...
        ///    public partial class ScholarlyWork : Zentity.Core.Resource
        ///    {
        ///        public static ScholarlyWork CreateScholarlyWork(global::System.Guid id)
        ///        {
        ///            ...
        ///        }
        ///        public string CopyRight
        ///        {
        ///            get
        ///            {
        ///                ...
        ///            }
        ///            set
        ///            {
        ///                ...
        ///            }
        ///        }
        ///        public global::System.Data.Objects.DataClasses.EntityCollection&lt;Contact&gt; Authors
        ///        {
        ///            get
        ///            {
        ///                ...
        ///            }
        ///            set
        ///            {
        ///                ...
        ///            }
        ///        }
        ///        ...
        ///    }
        ///
        ///    ...
        ///    public partial class Contact : Zentity.Core.Resource
        ///    {
        ///        public static Contact CreateContact(global::System.Guid id)
        ///        {
        ///            ...
        ///        }
        ///
        ///        public string Email
        ///        {
        ///            ...
        ///        }
        ///
        ///        public global::System.Data.Objects.DataClasses.EntityCollection&lt;ScholarlyWork&gt; AuthoredWorks
        ///        {
        ///            ...
        ///        }
        ///        ...
        ///    }
        ///}
        /// </code>
        /// </example>
        public StringCollection GenerateSourceCode(params string[] modulesToInclude)
        {
            // No need to Validate here. GenerateEFArtifacts does it for us.
            List<string> inputNamespaces = (modulesToInclude == null ||
                modulesToInclude != null && modulesToInclude.Length == 0) ?
                this.Modules.Select(tuple => tuple.NameSpace).ToList() :
                modulesToInclude.ToList();

            // Validate the input list.
            var nameSpaces = this.Modules.Select(tuple => tuple.NameSpace);
            var absentModules = inputNamespaces.Except(nameSpaces);
            if (absentModules.Count() > 0)
            {
                var absentModule = absentModules.First();
                throw new ZentityException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ExceptionAbsentModuleNamespace, absentModule));
            }

            // Generate artifacts for all the modules in the model. This saves us from missing CSDL
            // references while generating source code. We filter out unwanted namespaces later.
            Dictionary<string, string> sourceCodeTracker = new Dictionary<string, string>();

            // Prepare a reference list of CSDLs.
            List<string> refCSDLs = new List<string>();
            // Add Core CSDL.
            refCSDLs.Add(Utilities.GetGuidString() + CoreResources.DotCsdl);

            // Create generator.
            EntityClassGenerator generator =
                new EntityClassGenerator(LanguageOption.GenerateCSharpCode);

            List<DataModelModule> sortedModules = SortModules(this.Modules.ToList());
            // Remove the Core module from sorted list. We cannot assume that the Core module is
            // the first module in sorted list. For example, if a module has zero resource types
            // it is independent of Core and thus can be sorted before Core.
            var coreModule = sortedModules.
                Where(tuple => tuple.NameSpace == CoreResources.ZentityCore).FirstOrDefault();
            if (coreModule != null)
                sortedModules.Remove(coreModule);

            try
            {
                for (int i = 0; i < sortedModules.Count(); i++)
                {
                    DataModelModule module = sortedModules[i];

                    // Generate CSDL for this module. We cannot move this artifact generation 
                    // outside this loop since, the consolidated CSDL changes with the inclusion 
                    // of every module. For example, if the new module has some associations, 
                    // the Core CSDL will have to host new AssociationSet elements. We generate 
                    // and update the new Core CSDL in each iteration.
                    EFArtifactGenerationResults results = this.GenerateEFArtifacts(
                        module.NameSpace);

                    // Update Core CSDL.
                    results.Csdls.Where(tuple => tuple.Key == CoreResources.ZentityCore).
                        First().Value.Save(refCSDLs[0]);

                    // Dump the CSDL.
                    string csdlFileName = Utilities.GetGuidString() + CoreResources.DotCsdl;
                    results.Csdls.Where(tuple => tuple.Key == module.NameSpace).
                        First().Value.Save(csdlFileName);

                    // Create an output file name.
                    string outputFileName = Utilities.GetGuidString() + CoreResources.DotCs;

                    // Generate class.
                    IList<EdmSchemaError> errors = generator.GenerateCode(csdlFileName, outputFileName, refCSDLs);

                    if (errors.Count > 0)
                    {
                        StringBuilder error = new StringBuilder();
                        foreach (EdmSchemaError e in errors)
                            error.Append(e.Message);

                        throw new ZentityException(error.ToString());
                    }

                    // Add this csdl to reference csdl list.
                    refCSDLs.Add(csdlFileName);

                    // Add the code to object layer.
                    using (StreamReader rdr = new StreamReader(outputFileName))
                    {
                        sourceCodeTracker.Add(module.NameSpace, rdr.ReadToEnd());
                    }

                    // Delete the output file.
                    System.IO.File.Delete(outputFileName);
                }

                StringCollection objectLayer = new StringCollection();

                // Strip off additional source code.
                foreach (KeyValuePair<string, string> kvp in sourceCodeTracker)
                {
                    if (inputNamespaces.Contains(kvp.Key) &&
                        !this.Modules.Where(module => module.NameSpace == kvp.Key).First().IsMsShipped)
                        objectLayer.Add(kvp.Value);
                }

                return objectLayer;
            }
            finally
            {
                // Cleanup - delete all referenced csdl files.
                foreach (string refCSDL in refCSDLs)
                    if (System.IO.File.Exists(refCSDL))
                        System.IO.File.Delete(refCSDL);
            }
        }

        /// <summary>
        /// Generates a .NET assembly containing the types defined in the input modules. 
        /// </summary>
        /// <param name="outputAssemblyName">The security identity of the output assembly.</param>
        /// <param name="embedMetadataFilesAsResources">Whether to embed the Entity Framework 
        /// artifacts into the assembly.</param>
        /// <param name="modulesToEmbed">Namespaces of modules for which the Entity Framework 
        /// artifacts are to be embedded in the generated assembly. The method embeds artifacts
        /// for all modules if this parameter is null.</param>
        /// <param name="modulesToCompile">Namespaces of modules to compile. The method generates
        /// source code for all non-MsShipped modules if this parameter is null.</param>
        /// <param name="referencedAssemblies">The assemblies to be referenced for compilation.
        /// It is not required to reference the 'Zentity.Core' dll explicitly. The semantics of 
        /// this parameter are similar to ReferencedAssemblies property of 
        /// System.CodeDom.Compiler.CompilerParameters class.</param>
        /// <returns>The generated .NET assembly for custom resource types.</returns>
        /// <example> The example below shows how to generate and use an Extensions assembly. 
        /// Though the example uses reflection, we recommend compiling the client programs with
        /// references to the generated assembly to avoid reflection based code. Also, notice
        /// that the Entity Framework artifacts generated to work with the assembly are embedded
        /// in the assembly itself. While working with a number of active data model modules, it
        /// is advised to generate the EF artifacts explicitly using 
        /// <see cref="Zentity.Core.DataModel.GenerateEFArtifacts"/> and provide their paths in the 
        /// ZentityContext constructor.
        /// <code>
        ///using Zentity.Core;
        ///using System.Xml;
        ///using System.Reflection;
        ///using System;
        ///using System.Collections;
        ///using System.Linq;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        static string extensionsNamespace = &quot;Namespace&quot; + Guid.NewGuid().ToString(&quot;N&quot;);
        ///        const string connectionStringFormat = @&quot;provider=System.Data.SqlClient;
        ///                metadata=res://{0}; provider connection string='Data Source=.;
        ///                Initial Catalog=Zentity;Integrated Security=True;MultipleActiveResultSets=True'&quot;;
        ///        const string extensionsAssemblyName = &quot;Extensions&quot;;
        ///
        ///        public static void Main(string[] args)
        ///        {
        ///            using (ZentityContext context = new ZentityContext(
        ///                string.Format(connectionStringFormat, &quot;Zentity.Core&quot;)))
        ///            {
        ///                // Create a new module.
        ///                DataModelModule module = new DataModelModule { NameSpace = extensionsNamespace };
        ///                context.DataModel.Modules.Add(module);
        ///
        ///                // Create the ScholarlyWork type.
        ///                ResourceType resourceTypeResource = context.DataModel.Modules[&quot;Zentity.Core&quot;].ResourceTypes[&quot;Resource&quot;];
        ///                ResourceType resourceTypeScholarlyWork = new ResourceType { Name = &quot;ScholarlyWork&quot;, BaseType = resourceTypeResource };
        ///                module.ResourceTypes.Add(resourceTypeScholarlyWork);
        ///
        ///                // Create some Scalar Properties.
        ///                ScalarProperty copyRight = new ScalarProperty { Name = &quot;CopyRight&quot;, DataType = DataTypes.String, MaxLength = 4000 };
        ///                resourceTypeScholarlyWork.ScalarProperties.Add(copyRight);
        ///
        ///                // Synchronize to alter the database schema.
        ///                // It is important to synchronize before we can generate the Entity Framework
        ///                // artifacts. The table and column mappings are assigned while synchronizing and
        ///                // cannot be computed in advance.
        ///                // This method sometimes takes a few minutes to complete depending on the actions
        ///                // taken by other modules (such as change history logging) in response to schema
        ///                // changes. Everything happens in a single transaction. Make sure that the timeout 
        ///                // values are set correct for the command and transaction. Transaction timeout is 
        ///                // controlled from App.Config, Web.Config and machine.config configuration files.
        ///                context.CommandTimeout = 300;
        ///                context.DataModel.Synchronize();
        ///
        ///                // Generate Extensions Assembly.
        ///                // Take note that we have embedded the generated Entity Framework artifacts in the assembly itself.
        ///                byte[] rawAssembly = context.DataModel.GenerateExtensionsAssembly(
        ///                    extensionsAssemblyName, true, new string[] { extensionsNamespace },
        ///                    new string[] { extensionsNamespace }, null);
        ///
        ///                Assembly extensions = Assembly.Load(rawAssembly);
        ///
        ///                // Create some repository items using the generated assembly.
        ///                CreateRepositoryItems(extensions);
        ///
        ///                // Retrieve the created repository items.
        ///                FetchRepositoryItems(extensions);
        ///            }
        ///        }
        ///
        ///        private static void FetchRepositoryItems(Assembly extensionsAssembly)
        ///        {
        ///            using (ZentityContext context = new ZentityContext(
        ///                string.Format(connectionStringFormat, extensionsAssemblyName)))
        ///            {
        ///                Console.WriteLine(&quot;Getting ScholarlyWorks...&quot;);
        ///                Type resourceTypeScholarlyWork = extensionsAssembly.GetType(extensionsNamespace + &quot;.ScholarlyWork&quot;);
        ///                PropertyInfo pi = resourceTypeScholarlyWork.GetProperty(&quot;CopyRight&quot;);
        ///                MethodInfo ofTypeMethod = context.Resources.GetType().GetMethod(&quot;OfType&quot;).
        ///                    MakeGenericMethod(resourceTypeScholarlyWork);
        ///                var customTypeInstances = ofTypeMethod.Invoke(context.Resources, null);
        ///                foreach (Resource scholarlyWork in (IEnumerable)customTypeInstances)
        ///                {
        ///                    Console.WriteLine(&quot;Id:[{0}], CopyRight:[{1}]&quot;, scholarlyWork.Id,
        ///                        pi.GetValue(scholarlyWork, null));
        ///                }
        ///            }
        ///        }
        ///
        ///        private static void CreateRepositoryItems(Assembly extensionsAssembly)
        ///        {
        ///            using (ZentityContext context = new ZentityContext(
        ///                string.Format(connectionStringFormat, extensionsAssemblyName)))
        ///            {
        ///                Type resourceTypeScholarlyWork = extensionsAssembly.GetType(extensionsNamespace + &quot;.ScholarlyWork&quot;);
        ///                var aScholarlyWork = Activator.CreateInstance(resourceTypeScholarlyWork);
        ///                PropertyInfo pi = resourceTypeScholarlyWork.GetProperty(&quot;CopyRight&quot;);
        ///                pi.SetValue(aScholarlyWork, &quot;A copyright value.&quot;, null);
        ///
        ///                // Save the items to repository. 
        ///                context.AddToResources((Resource)aScholarlyWork);
        ///                context.SaveChanges();
        ///            }
        ///        }
        ///
        ///    }
        ///}
        /// </code>
        /// </example>
        /// <remarks>
        /// The method creates some intermediate files on disk and thus the application should have
        /// write access on current directory. A temporary assembly file is created on disk using 
        /// the outputAssemblyName parameter. Try reducing the length of this parameter if you are
        /// seeing 'path too long' exceptions.
        /// <br/>
        /// Importance of referencedAssemblies parameter:
        /// <br/>
        /// Consider the inheritance hieararchy Core.Resource &lt;-- M1.Type1 &lt;-- M2.Type2.
        /// Let's say the user has already generated source code for M1, did some enhancements
        /// and compiled it into an assembly. Now while compiling the assembly for M2, we have 
        /// a choice to either use the enhanced assembly or generate a fresh one that includes 
        /// all the types from M1 and M2. To avoid any confusions, assemblies to be referenced 
        /// are explicitly passed as parameter to this method.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile")]
        public byte[] GenerateExtensionsAssembly(string outputAssemblyName, bool embedMetadataFilesAsResources, string[] modulesToEmbed, string[] modulesToCompile, string[] referencedAssemblies)
        {
            CSharpCodeProvider codeProvider = null;
            CompilerParameters compilerParameters = null;
            CompilerResults results = null;
            string notNullOutputAssemblyName = string.IsNullOrEmpty(outputAssemblyName) ?
                Utilities.GetGuidString() : outputAssemblyName;
            string temporaryDirectoryName = Utilities.GetGuidString();

            try
            {
                // Create a provider.
                codeProvider = new CSharpCodeProvider(new Dictionary<string, string>() 
                {                 
                    {                     
                        CoreResources.CompilerVersion,                     
                        CoreResources.v3_5                     
                    } 
                });

                // Create compiler parameters.
                compilerParameters = new CompilerParameters();
                Assembly coreAssembly = typeof(Resource).Assembly;
                // Set the list of assemblies to reference.
                compilerParameters.ReferencedAssemblies.Add(Assembly.Load(
                    CoreResources.DllSystem).Location);
                compilerParameters.ReferencedAssemblies.Add(Assembly.Load(
                    CoreResources.DllSystemDataEntity).Location);
                compilerParameters.ReferencedAssemblies.Add(Assembly.Load(
                    CoreResources.DllSystemRuntimeSerialization).Location);
                compilerParameters.ReferencedAssemblies.Add(coreAssembly.Location);
                compilerParameters.ReferencedAssemblies.Add(Assembly.Load(
                    CoreResources.DllSystemXml).Location);

                // Add reference to other assemblies.
                if (referencedAssemblies != null)
                    foreach (String assemblyLocation in referencedAssemblies)
                    {
                        // Case sensitive comparison.
                        if (!Assembly.LoadFile(assemblyLocation).FullName.
                            Equals(coreAssembly.FullName))
                            compilerParameters.ReferencedAssemblies.Add(assemblyLocation);
                    }
                // Set other options.
                compilerParameters.GenerateExecutable = false;
                compilerParameters.TreatWarningsAsErrors = false;
                compilerParameters.GenerateInMemory = false;
                compilerParameters.OutputAssembly = Path.Combine(temporaryDirectoryName,
                    notNullOutputAssemblyName + CoreResources.DotDll);

                // Dump the resources on disk and include them in the embedded resources list.
                // To avoid System.IO.PathTooLongException for the resource names, use a hash 
                // instead of actual names.
                Directory.CreateDirectory(temporaryDirectoryName);
                if (embedMetadataFilesAsResources)
                {
                    EFArtifactGenerationResults efArtifacts = new EFArtifactGenerationResults();
                    efArtifacts = GenerateEFArtifacts(modulesToEmbed);

                    // Embed all the CSDLs.
                    for (int i = 0; i < efArtifacts.Csdls.Count; i++)
                    {
                        XmlDocument xCSDL = efArtifacts.Csdls[i].Value;
                        string csdlFileName = Path.Combine(temporaryDirectoryName,
                            ComputeHash(efArtifacts.Csdls[i].Key) +
                            CoreResources.DotCsdl);
                        xCSDL.Save(csdlFileName);
                        compilerParameters.EmbeddedResources.Add(csdlFileName);
                    }

                    // Embed SSDL.
                    string ssdlFileName = Path.Combine(temporaryDirectoryName,
                        ComputeHash(notNullOutputAssemblyName) +
                        CoreResources.DotSsdl);
                    efArtifacts.Ssdl.Save(ssdlFileName);
                    compilerParameters.EmbeddedResources.Add(ssdlFileName);

                    // Embed MSL.
                    string mslFileName = Path.Combine(temporaryDirectoryName,
                        ComputeHash(notNullOutputAssemblyName) +
                        CoreResources.DotMsl);
                    efArtifacts.Msl.Save(mslFileName);
                    compilerParameters.EmbeddedResources.Add(mslFileName);
                }

                StringCollection sourceCode = GenerateSourceCode(modulesToCompile);

                // There could be no source code to process sometimes. This happens when there is no 
                // custom type in the resource types collection.
                if (sourceCode.Count == 0)
                    return null;

                string[] sourceCodeArray = new string[sourceCode.Count];
                sourceCode.CopyTo(sourceCodeArray, 0);
                results = codeProvider.CompileAssemblyFromSource(compilerParameters, sourceCodeArray);
                if (results.Errors.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (CompilerError error in results.Errors)
                        sb.Append(error.ErrorText);

                    throw new ZentityException(sb.ToString());
                }

                // Create byte array from generated assembly.
                byte[] generatedAssembly = null;
                using (FileStream fs = new FileStream(results.PathToAssembly, FileMode.Open,
                    FileAccess.Read))
                {
                    generatedAssembly = new byte[fs.Length];
                    fs.Read(generatedAssembly, 0, (int)fs.Length);
                    fs.Close();
                }

                return generatedAssembly;
            }
            finally
            {
                // Cleanup.
                if (Directory.Exists(temporaryDirectoryName))
                    Directory.Delete(temporaryDirectoryName, true);
            }
        }

        /// <summary>
        /// Reloads the data model information from backend. The 
        /// <see cref="Zentity.Core.DataModel.Modules"/> property is re-initialized with a new 
        /// instance of <see cref="Zentity.Core.DataModelModuleCollection"/> object.
        /// </summary>
        public void Refresh()
        {

            // We open a new connection here and do not reuse context connection. Using context 
            // connection might raise errors if there is an explicit transaction opened on it by 
            // a client. In that case, the ExecuteNonQuery here should use the same client 
            // transaction and it is difficult to get hold of the client initiated transaction here.
            using (SqlConnection storeConnection =
                new SqlConnection(this.Parent.StoreConnectionString))
            {
                Refresh(storeConnection);
            }
        }

        #endregion

        #region Metadata Loading Helpers

        private void Refresh(SqlConnection storeConnection)
        {
            // Save off the modules collection reference.
            DataModelModuleCollection savedModules = this.modules;

            try
            {
                // Pull custom resource type information from backend.
                XmlDocument xDataModel = new XmlDocument();

                if (storeConnection.State == ConnectionState.Closed)
                    storeConnection.Open();

                using (DbCommand cmd = storeConnection.CreateCommand())
                {
                    cmd.CommandText = CoreResources.Core_GetDataModelModules;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = this.Parent.OperationTimeout;

                    DbParameter param = cmd.CreateParameter();
                    param.DbType = DbType.String;
                    param.Direction = ParameterDirection.Output;
                    param.ParameterName = CoreResources.DataModelModules;
                    param.Size = -1;
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();

                    xDataModel.LoadXml(param.Value.ToString());
                    // TODO: Validate the xml against a schema.
                }

                LoadDataModel(this, xDataModel);
            }
            // TODO: Catch a more specific exception.
            catch
            {
                // Restore the saved modules collection, if anything goes wrong.
                this.modules = savedModules;
                throw;
            }
        }

        private static void LoadDataModel(DataModel dataModel, XmlDocument xDataModel)
        {
            dataModel.modules = new DataModelModuleCollection(dataModel);

            // Load data model modules.
            LoadDataModelModuleCollection(dataModel.Modules, xDataModel);

            // Assign the MaxDiscriminator.
            XmlElement eDataModel = xDataModel[CoreResources.DataModel];
            if (eDataModel.HasAttribute(CoreResources.MaxDiscriminator))
                dataModel.MaxDiscriminator = Convert.ToInt32(xDataModel[CoreResources.DataModel].
                    Attributes[CoreResources.MaxDiscriminator].Value, CultureInfo.InvariantCulture);

            // Validate the model graph.
            dataModel.Validate();
        }

        private static void LoadDataModelModuleCollection(DataModelModuleCollection inputModules, XmlDocument xDataModel)
        {
            // We load the module collection in three passes.
            // Pass1 - Create all resource types, scalar and navigation properties but do not 
            // assign base types to the resource types. This allows us to process the derived
            // types before the base types. Otherwise, we might run into scenarios where we do
            // not have a base type reference while processing a derived type.
            // Pass2 - Assign base types to all resource types.
            // Pass3 - Create associations between various navigation properties.

            // Pass1 - Create all resource types, scalar and navigation properties but do not 
            // assign base types to the resource types.
            foreach (XmlNode xModule in xDataModel.SelectNodes(CoreResources.XPathDataModelModule))
            {
                DataModelModule module = new DataModelModule();
                inputModules.Add(module);
                LoadDataModelModule(module, xModule);
            }

            // Pass2 - Assign base types to all resource types.
            foreach (XmlNode xResourceType in
                xDataModel.SelectNodes(CoreResources.XPathResourceType))
            {
                XmlElement eResourceType = xResourceType as XmlElement;
                Guid id = new Guid(eResourceType.Attributes[CoreResources.Id].Value);

                // Assign BaseType.
                if (eResourceType.HasAttribute(CoreResources.BaseTypeId))
                {
                    Guid baseResourceTypeId =
                        new Guid(eResourceType.Attributes[CoreResources.BaseTypeId].Value);

                    ResourceType derivedType = GetResourceTypeById(inputModules, id);
                    derivedType.BaseType = GetResourceTypeById(inputModules, baseResourceTypeId);
                }
            }

            // Pass3 - Create associations between various navigation properties.
            foreach (XmlNode xAssociation in
                xDataModel.SelectNodes(CoreResources.XPathAssociation))
            {
                XmlElement eAssociation = xAssociation as XmlElement;
                Association association = new Association();

                // Assign Id.
                association.Id = new Guid(eAssociation.Attributes[CoreResources.Id].Value);

                // Assign Name.
                association.Name = eAssociation.Attributes[CoreResources.Name].Value;

                // Assign Uri.
                association.Uri = eAssociation.HasAttribute(CoreResources.Uri) ?
                    eAssociation.Attributes[CoreResources.Uri].Value : null;

                // Assign subject navigation property.
                Guid subjectNavigationPropertyId = new Guid(eAssociation.
                    Attributes[CoreResources.SubjectNavigationPropertyId].Value);
                association.SubjectNavigationProperty = GetNavigationPropertyById(inputModules,
                    subjectNavigationPropertyId);

                // Assign object navigation property.
                Guid objectNavigationPropertyId = new Guid(eAssociation.
                    Attributes[CoreResources.ObjectNavigationPropertyId].Value);
                association.ObjectNavigationProperty = GetNavigationPropertyById(inputModules,
                    objectNavigationPropertyId);

                // Assign predicate id.
                association.PredicateId = new Guid(eAssociation.
                    Attributes[CoreResources.PredicateId].Value);

                // Assign subject multiplicity.
                string subjectMultiplicity =
                    eAssociation.Attributes[CoreResources.SubjectMultiplicity].Value;
                association.SubjectMultiplicity = (AssociationEndMultiplicity)Enum.
                    Parse(typeof(AssociationEndMultiplicity), subjectMultiplicity);

                // Assign object multiplicity.
                string objectMultiplicity =
                    eAssociation.Attributes[CoreResources.ObjectMultiplicity].Value;
                association.ObjectMultiplicity = (AssociationEndMultiplicity)Enum.
                    Parse(typeof(AssociationEndMultiplicity), objectMultiplicity);
            }
        }

        private static void LoadDataModelModule(DataModelModule module, XmlNode xdataModelModule)
        {
            XmlElement eDataModelModule = xdataModelModule as XmlElement;

            // Assign Id.
            module.Id = new Guid(eDataModelModule.Attributes[CoreResources.Id].Value);

            // Assign Namespace.
            module.NameSpace = eDataModelModule.Attributes[CoreResources.Namespace].Value;

            // Assign Uri.
            module.Uri = eDataModelModule.HasAttribute(CoreResources.Uri) ?
                eDataModelModule.Attributes[CoreResources.Uri].Value : null;

            // Assign Description.
            module.Description = eDataModelModule.HasAttribute(CoreResources.Description) ?
                eDataModelModule.Attributes[CoreResources.Description].Value : null;

            // Clear internal lists.
            module.ResourceTypes.Clear();

            // Load resource types before associations.
            LoadResourceTypeCollection(module.ResourceTypes,
                xdataModelModule.SelectNodes(CoreResources.XPathRelativeResourceType));

            // Assign IsMsShipped in the end.
            module.IsMsShipped = Convert.ToInt32(xdataModelModule.
                Attributes[CoreResources.IsMsShipped].Value, CultureInfo.InvariantCulture) == 1 ?
                true : false;
        }

        private static void LoadResourceTypeCollection(ResourceTypeCollection resourceTypes, XmlNodeList xResourceTypes)
        {
            foreach (XmlNode xResourceType in xResourceTypes)
            {
                ResourceType resourceType = new ResourceType();
                resourceTypes.Add(resourceType);
                LoadResourceType(resourceType, xResourceType);
            }
        }

        private static void LoadResourceType(ResourceType resourceType, XmlNode xResourceType)
        {
            XmlElement eResourceType = xResourceType as XmlElement;

            // Assign Id.
            resourceType.Id = new Guid(eResourceType.Attributes[CoreResources.Id].Value);

            // Assign Discriminator.
            resourceType.Discriminator = int.Parse(
                eResourceType.Attributes[CoreResources.Discriminator].Value,
                CultureInfo.InvariantCulture);

            // Assign Name.
            resourceType.Name = eResourceType.Attributes[CoreResources.Name].Value;

            // Assign Uri.
            if (eResourceType.HasAttribute(CoreResources.Uri))
                resourceType.Uri = eResourceType.Attributes[CoreResources.Uri].Value;

            // Assign Description.
            if (eResourceType.HasAttribute(CoreResources.Description))
                resourceType.Description = eResourceType.Attributes[CoreResources.Description].Value;

            // Clear internal lists.
            resourceType.ScalarProperties.Clear();
            resourceType.NavigationProperties.Clear();

            // Load scalar properties.
            LoadScalarPropertyCollection(resourceType.ScalarProperties,
                eResourceType.SelectNodes(CoreResources.XPathRelativeScalarProperty));

            // Load navigation properties.
            LoadNavigationPropertyCollection(resourceType.NavigationProperties,
                eResourceType.SelectNodes(CoreResources.XPathRelativeNavigationProperty));
        }

        private static void LoadNavigationPropertyCollection(NavigationPropertyCollection navigationProperties, XmlNodeList xNavigationProperties)
        {
            foreach (XmlNode xNavigationProperty in xNavigationProperties)
            {
                NavigationProperty navigationProperty = new NavigationProperty();
                navigationProperties.Add(navigationProperty);
                LoadNavigationProperty(navigationProperty, xNavigationProperty);
            }
        }

        private static void LoadNavigationProperty(NavigationProperty navigationProperty, XmlNode xNavigationProperty)
        {
            XmlElement eNavigationProperty = xNavigationProperty as XmlElement;

            // Assign Id.
            navigationProperty.Id =
                new Guid(eNavigationProperty.Attributes[CoreResources.Id].Value);

            // Assign Name.
            navigationProperty.Name = eNavigationProperty.Attributes[CoreResources.Name].Value;

            // Assign Uri.
            if (eNavigationProperty.HasAttribute(CoreResources.Uri))
                navigationProperty.Uri = eNavigationProperty.Attributes[CoreResources.Uri].Value;

            // Assign Description.
            if (eNavigationProperty.HasAttribute(CoreResources.Description))
                navigationProperty.Description =
                    eNavigationProperty.Attributes[CoreResources.Description].Value;

            // Assign table and column mappings.
            if (eNavigationProperty.HasAttribute(CoreResources.TableName))
            {
                navigationProperty.TableName =
                    eNavigationProperty.Attributes[CoreResources.TableName].Value;

                navigationProperty.ColumnName =
                    eNavigationProperty.Attributes[CoreResources.ColumnName].Value;
            }
        }

        private static void LoadScalarPropertyCollection(ScalarPropertyCollection scalarProperties, XmlNodeList xScalarProperties)
        {
            foreach (XmlNode xScalarProperty in xScalarProperties)
            {
                ScalarProperty scalarProperty = new ScalarProperty();
                scalarProperties.Add(scalarProperty);
                LoadScalarProperty(scalarProperty, xScalarProperty);
            }
        }

        private static void LoadScalarProperty(ScalarProperty scalarProperty, XmlNode xScalarProperty)
        {
            XmlElement eScalarProperty = xScalarProperty as XmlElement;

            // Assign Id.
            scalarProperty.Id = new Guid(eScalarProperty.Attributes[CoreResources.Id].Value);

            // Assign Name.
            scalarProperty.Name = eScalarProperty.Attributes[CoreResources.Name].Value;

            // Assign Nullable.
            scalarProperty.Nullable =
                Convert.ToInt32(eScalarProperty.Attributes[CoreResources.Nullable].Value,
                CultureInfo.InvariantCulture) == 1 ? true : false;

            // Assign Uri.
            if (eScalarProperty.HasAttribute(CoreResources.Uri))
                scalarProperty.Uri = eScalarProperty.Attributes[CoreResources.Uri].Value;

            // Assign Description.
            if (eScalarProperty.HasAttribute(CoreResources.Description))
                scalarProperty.Description = eScalarProperty.Attributes[CoreResources.Description].Value;

            // Assign DataType.
            string dataType = eScalarProperty.Attributes[CoreResources.DataType].Value;
            scalarProperty.DataType = (DataTypes)Enum.Parse(typeof(DataTypes), dataType);

            // Assign MaxLength.
            if (eScalarProperty.HasAttribute(CoreResources.MaxLength))
                scalarProperty.MaxLength = Convert.ToInt32(eScalarProperty.
                    Attributes[CoreResources.MaxLength].Value, CultureInfo.InvariantCulture);

            // Assign Scale.
            if (eScalarProperty.HasAttribute(CoreResources.Scale))
                scalarProperty.Scale = Convert.ToInt32(eScalarProperty.
                    Attributes[CoreResources.Scale].Value, CultureInfo.InvariantCulture);

            // Assign Precision.
            if (eScalarProperty.HasAttribute(CoreResources.Precision))
                scalarProperty.Precision = Convert.ToInt32(eScalarProperty.
                    Attributes[CoreResources.Precision].Value, CultureInfo.InvariantCulture);

            // Assign TableName.
            scalarProperty.TableName = eScalarProperty.Attributes[CoreResources.TableName].Value;

            // Assign ColumnName.
            scalarProperty.ColumnName = eScalarProperty.Attributes[CoreResources.ColumnName].Value;

            // Assign IsFullTextIndexed.
            scalarProperty.IsFullTextIndexed = Convert.ToInt32(eScalarProperty.
                Attributes[CoreResources.IsFullTextIndexed].Value,
                CultureInfo.InvariantCulture) == 1 ? true : false;
        }

        private static ResourceType GetResourceTypeById(DataModelModuleCollection inputModules, Guid id)
        {
            var resourceTypes = inputModules.SelectMany(tuple => tuple.ResourceTypes).
                Where(tuple => tuple.Id == id);

            if (resourceTypes.Count() > 1)
                throw new ZentityException(CoreResources.ExceptionMultipleResourceTypeDetected);
            else
                return resourceTypes.First();
        }

        private static NavigationProperty GetNavigationPropertyById(DataModelModuleCollection inputModules, Guid id)
        {
            var navigationProperties = inputModules.SelectMany(tuple => tuple.ResourceTypes).
                SelectMany(tuple => tuple.NavigationProperties).
                Where(tuple => tuple.Id == id);

            if (navigationProperties.Count() > 1)
                throw new ZentityException(CoreResources.ExceptionMultipleNavigationPropertyDetected);
            else
                return navigationProperties.First();
        }

        #endregion

        #region Misc Helpers

        internal static List<ResourceType> SortByHierarchy(List<ResourceType> list)
        {
            List<Guid> visitedList = new List<Guid>();
            List<Guid> idList = new List<Guid>();
            idList.AddRange(list.Select(tuple => tuple.Id));

            List<ResourceType> sortedList = new List<ResourceType>();

            // Get the roots. Root elements are those whose base types are not present in the list.
            sortedList.AddRange(list.Where(tuple => tuple.BaseType == null ||
                tuple.BaseType != null && !idList.Contains(tuple.BaseType.Id)));

            // Process each entry in the list and queue up the derived types in the end of list. 
            // Basically, we are traversing the inheritance tree in depth first order.
            for (int i = 0; i < sortedList.Count; i++)
            {
                ResourceType resourceType = sortedList[i];

                // Check for cycles in the inheritance hierarchy.
                if (visitedList.Contains(resourceType.Id))
                    throw new ZentityException(CoreResources.InvalidResourceTypesCyclicInheritance);

                // Enqueue derived types.
                foreach (ResourceType derivedType in list.Where(tuple => tuple.BaseType != null &&
                    tuple.BaseType.Id == resourceType.Id))
                    sortedList.Add(derivedType);

                // Mark as visited.
                visitedList.Add(resourceType.Id);
            }

            // If the final count of sorted list is not equal to the input list then it means that
            // there are some elements missed that have their base type present in the collection and 
            // which cannot be reached by the root nodes. This is only possible if there are cylces
            // of resource types in the list.
            if (sortedList.Count != list.Count)
                throw new ZentityException(CoreResources.InvalidResourceTypesCyclicInheritance);

            return sortedList;
        }

        private static List<DataModelModule> SortModules(List<DataModelModule> moduleList)
        {
            // Do a topological sort of modules (http://en.wikipedia.org/wiki/Topological_sort).

            // Prepare a directed graph. Each node in the dictionary holds the Id of a module
            // and the list of Ids of modules that depend on this module, basically a list of
            // incoming edges.
            Dictionary<Guid, List<Guid>> directedGraph = new Dictionary<Guid, List<Guid>>();

            // Pass1: Populate modules.
            foreach (DataModelModule module in moduleList)
                directedGraph.Add(module.Id, new List<Guid>());

            // Pass2: Set dependencies.
            //                Core
            //                ^  ^
            //                |  |
            //    ScholarlyWorks |
            //                ^  |
            //                |  |
            //               Museum
            foreach (DataModelModule dependentModule in moduleList)
            {
                Guid dependentModuleId = dependentModule.Id;
                foreach (ResourceType resourceType in dependentModule.ResourceTypes)
                {
                    // BaseType might be null. e.g. for Core.Resource. Also, ignore all the 
                    // resource types whose parent modules are not set.
                    if (resourceType.BaseType != null && resourceType.BaseType.Parent != null &&
                        resourceType.BaseType.Parent.Id != dependentModuleId)
                    {
                        Guid baseModuleId = resourceType.BaseType.Parent.Id;
                        // Ignore modules that are not present in the input list.
                        if (directedGraph.Keys.Contains(baseModuleId))
                        {
                            List<Guid> dependentModuleIds = directedGraph[baseModuleId];
                            // The module id might already be added to the dependency list.
                            if (!dependentModuleIds.Contains(dependentModuleId))
                                dependentModuleIds.Add(dependentModuleId);
                        }
                    }
                }
            }

            List<Guid> L = TopologicalSort(directedGraph);

            List<DataModelModule> sortedModules = new List<DataModelModule>();
            for (int i = L.Count - 1; i >= 0; i--)
            {
                sortedModules.Add(moduleList.Where(tuple => tuple.Id == L[i]).First());
            }

            return sortedModules;
        }

        private static List<Guid> TopologicalSort(Dictionary<Guid, List<Guid>> directedGraph)
        {
            List<Guid> L = new List<Guid>();
            Queue<Guid> S = new Queue<Guid>();

            foreach (KeyValuePair<Guid, List<Guid>> kvp in directedGraph)
            {
                if (kvp.Value.Count == 0)
                    S.Enqueue(kvp.Key);
            }

            while (S.Count > 0)
            {
                Guid n = S.Dequeue();
                L.Add(n);
                //for each node m with an edge e from n to m do
                //    remove edge e from the graph
                //    if m has no other incoming edges then
                //        insert m into S
                List<Guid> baseModuleIds = directedGraph.Where(tuple => tuple.Value.Contains(n)).
                    Select(tuple => tuple.Key).ToList();
                foreach (Guid m in baseModuleIds)
                {
                    List<Guid> dependentModuleIds = directedGraph[m];
                    dependentModuleIds.Remove(n);
                    if (dependentModuleIds.Count == 0)
                        S.Enqueue(m);
                }
                // Removal of the edge includes removing entry from the dependents list
                // of all the nodes as well as removing the node itself.
                directedGraph.Remove(n);
            }

            if (directedGraph.Count > 0)
                throw new ZentityException(CoreResources.ExceptionCycleInDependencyGraph);
            return L;
        }

        private static string ComputeHash(string inputString)
        {
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
            byte[] buffer = Encoding.Unicode.GetBytes(inputString);
            buffer = provider.ComputeHash(buffer);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in buffer)
                sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            return sb.ToString();
        }

        private StringCollection GetSynchronizationScripts(out TableMappingCollection tableMappings, out Dictionary<Guid, int> discriminators)
        {
            // Validate the in-memory data model graph.
            Validate();

            DataModel synchronizedModel;
            GetSynchronizedModelAndMappings(out synchronizedModel, out tableMappings,
                out discriminators);

            // Verify that the MsShipped modules are not changed.
            ValidateMsShippedModuleChanges(synchronizedModel, this);

            // Generate synchronization script for the changes. The scripts alter the database 
            // schema and also update the data model information in the backend tables.
            StringCollection scripts = SqlScriptGenerator.GenerateScripts(
                synchronizedModel, this, tableMappings, discriminators);

            return scripts;
        }

        private static XmlDocument PrepareXmlDocumentFromManifestResourceStream(Assembly assembly, string manifestResourceName)
        {
            XmlDocument resource;
            using (Stream resourceStream = assembly.GetManifestResourceStream(manifestResourceName))
            {
                resource = new XmlDocument();
                resource.Load(resourceStream);
            }
            return resource;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static void DetectUnsynchronizedModules(DataModel originalModel, DataModel newModel, List<string> inputNamespaces)
        {
            ModuleCollectionChange changes = ComputeDifferences(originalModel, newModel);

            // Again the logic below is based on IDs since the namespaces might have changed for
            // the modules. Once we have all the module IDs that have undergone some change, we
            // locate those modules in the new graph and prepare a list of namespaces from the
            // new model. In the end, we check if there are any namespaces in the prepared list
            // that are also present in the input list. If yes, this means that some of the input
            // namespaces have some pending changes and thus we cannot successfully generate the
            // mapping files for them.
            List<Guid> unsynchronizedModuleIds = new List<Guid>();
            unsynchronizedModuleIds.AddRange(
                changes.AddedAssociations.Select(tuple => tuple.Parent.Id));
            unsynchronizedModuleIds.AddRange(
                changes.AddedDataModelModules.Select(tuple => tuple.Id));
            unsynchronizedModuleIds.AddRange(
                changes.AddedNavigationProperties.Select(tuple => tuple.Parent.Parent.Id));
            unsynchronizedModuleIds.AddRange(
                changes.AddedResourceTypes.Select(tuple => tuple.Parent.Id));
            unsynchronizedModuleIds.AddRange(
                changes.AddedScalarProperties.Select(tuple => tuple.Parent.Parent.Id));

            unsynchronizedModuleIds.AddRange(
                changes.DeletedAssociations.Select(tuple => tuple.Parent.Id));
            unsynchronizedModuleIds.AddRange(
                changes.DeletedDataModelModules.Select(tuple => tuple.Id));
            unsynchronizedModuleIds.AddRange(
                changes.DeletedNavigationProperties.Select(tuple => tuple.Parent.Parent.Id));
            unsynchronizedModuleIds.AddRange(
                changes.DeletedResourceTypes.Select(tuple => tuple.Parent.Id));
            unsynchronizedModuleIds.AddRange(
                changes.DeletedScalarProperties.Select(tuple => tuple.Parent.Parent.Id));

            foreach (var kvp in changes.UpdatedAssociations)
            {
                unsynchronizedModuleIds.Add(kvp.Key.Parent.Id);
                unsynchronizedModuleIds.Add(kvp.Value.Parent.Id);
            }
            foreach (var kvp in changes.UpdatedDataModelModules)
            {
                unsynchronizedModuleIds.Add(kvp.Key.Id);
                unsynchronizedModuleIds.Add(kvp.Value.Id);
            }
            foreach (var kvp in changes.UpdatedNavigationProperties)
            {
                unsynchronizedModuleIds.Add(kvp.Key.Parent.Parent.Id);
                unsynchronizedModuleIds.Add(kvp.Value.Parent.Parent.Id);
            }
            foreach (var kvp in changes.UpdatedResourceTypes)
            {
                unsynchronizedModuleIds.Add(kvp.Key.Parent.Id);
                unsynchronizedModuleIds.Add(kvp.Value.Parent.Id);
            }
            foreach (var kvp in changes.UpdatedScalarProperties)
            {
                unsynchronizedModuleIds.Add(kvp.Key.Parent.Parent.Id);
                unsynchronizedModuleIds.Add(kvp.Value.Parent.Parent.Id);
            }

            List<string> unsynchronizedModuleNamespaces = newModel.Modules.
                Where(tuple => unsynchronizedModuleIds.Contains(tuple.Id)).
                Select(tuple => tuple.NameSpace).ToList();

            var aModule = inputNamespaces.Intersect(unsynchronizedModuleNamespaces).
                FirstOrDefault();
            if (aModule != null)
                throw new ZentityException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ExceptionUnsynchronizedModule, aModule));
        }

        private void GetSynchronizedModelAndMappings(out DataModel synchronizedModel, out TableMappingCollection tableMappings, out Dictionary<Guid, int> discriminators)
        {
            synchronizedModel = new DataModel(this.Parent);
            tableMappings = new TableMappingCollection();
            discriminators = new Dictionary<Guid, int>();
            XmlDocument xDocMappings = new XmlDocument();

            // Create a new model from backend information. Also, get the table mappings for
            // the data model items. We can use the same connection. Wrap these two information 
            // pulls in a transaction so that we get the mapping information that is in sync with
            // the metadata information. For example, if client A pulls the metadata information 
            // then client B updates the information and then client A again pulls the mappings 
            // information, the mapping information will not be consistent with the metadata 
            // information that client A had pulled earlier. This may not be required since this 
            // method will be called very infrequently and it is very unlikely that readers and
            // writers are active at the same time.
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead
                }))
            {
                using (SqlConnection storeConnection =
                    new SqlConnection(this.Parent.StoreConnectionString))
                {
                    synchronizedModel.Refresh(storeConnection);

                    if (storeConnection.State == ConnectionState.Closed)
                        storeConnection.Open();

                    using (DbCommand cmd = storeConnection.CreateCommand())
                    {
                        cmd.CommandText = CoreResources.Core_GetTableModelMap;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = this.Parent.OperationTimeout;

                        DbParameter param = cmd.CreateParameter();
                        param.DbType = DbType.String;
                        param.Direction = ParameterDirection.Output;
                        param.ParameterName = CoreResources.TableModelMap;
                        param.Size = -1;
                        cmd.Parameters.Add(param);
                        cmd.ExecuteNonQuery();

                        xDocMappings.LoadXml(param.Value.ToString());
                    }
                }
                scope.Complete();
            }

            // Create table mappings.
            tableMappings.LoadFromXml(xDocMappings);

            // Create discriminators.
            foreach (ResourceType r in synchronizedModel.Modules.
                SelectMany(module => module.ResourceTypes))
                discriminators.Add(r.Id, r.Discriminator);
        }

        #endregion
    }
}
