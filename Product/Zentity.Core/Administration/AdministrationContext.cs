// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Administration
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.Data.EntityClient;

    /// <summary>Adds functionality to the Administration entity model context class</summary>
    /// <example> This example demonstrates simple usage of change history classes. 
    /// Use <see cref="Zentity.Administration.AdministrationContext.EnableChangeHistory"/> 
    /// to enable the change history feature before running this sample. 
    /// <br/>
    /// If you are using the parameterless constructor of AdministrationContext class, make sure
    /// that the configuration file (App.Config, Web.Config etc) contains a connection string
    /// with the name 'AdministrationContext'. Following is an example.
    /// <code lang="xml">
    ///&lt;connectionStrings&gt;
    ///&lt;add name=&quot;ZentityContext&quot; 
    ///     connectionString=&quot;provider=System.Data.SqlClient;
    ///     metadata=res://Zentity.ScholarlyWorks;
    ///     provider connection string='Data Source=.;
    ///     Initial Catalog=Zentity;Integrated Security=True;
    ///     MultipleActiveResultSets=True'&quot;
    ///     providerName=&quot;System.Data.EntityClient&quot; /&gt;
    ///&lt;add name=&quot;AdministrationContext&quot;
    ///     connectionString=&quot;provider=System.Data.SqlClient;
    ///     metadata=res://Zentity.Core;
    ///     provider connection string='Data Source=.;
    ///     Initial Catalog=Zentity;Integrated Security=True;
    ///     MultipleActiveResultSets=True'&quot;
    ///     providerName=&quot;System.Data.EntityClient&quot; /&gt;
    ///&lt;/connectionStrings&gt;
    ///</code>
    /// <code>
    ///using System;
    ///using System.Linq;
    ///using Zentity.Administration;
    ///using Zentity.Core;
    ///using System.Threading;
    ///
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        public static void Main(string[] args)
    ///        {
    ///            Guid resourceId = Guid.Empty;
    ///
    ///            // Use ZentityContext to update repository content.
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                // Create.
    ///                Resource pub = new Resource
    ///                {
    ///                    Title = &quot;sample Resource&quot;,
    ///                    Uri = &quot;urn:zentity-samples:pub1&quot;
    ///                };
    ///                context.AddToResources(pub);
    ///                context.SaveChanges();
    ///                resourceId = pub.Id;
    ///
    ///                // Update.
    ///                pub.Title = &quot;new title&quot;;
    ///                context.SaveChanges();
    ///
    ///                // Delete.
    ///                context.DeleteObject(pub);
    ///                context.SaveChanges();
    ///            }
    ///
    ///            // Give some time to the background job to process these changes.
    ///            Thread.Sleep(new TimeSpan(0, 0, 20));
    ///
    ///            // Retrieve all changes for the above Resource. 
    ///            // Use the default connection string provided in the configuration file.
    ///            using (AdministrationContext context = new AdministrationContext())
    ///            {
    ///                foreach (ResourceChange rc in context.ResourceChanges.
    ///                    Include(&quot;ChangeSet&quot;).Include(&quot;Operation&quot;).
    ///                    Where(tuple =&gt; tuple.ResourceId == resourceId))
    ///                {
    ///                    Console.WriteLine(&quot;Changeset: [{0}] created on [{1}].&quot;, 
    ///                        rc.ChangeSet.Id, rc.ChangeSet.DateCreated);
    ///                    Console.WriteLine(&quot;ResourceChange Operation:[{0}], Details:\n{1}&quot;, 
    ///                        rc.Operation.Name, rc.PropertyChanges);
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class AdministrationContext
    {
        /// <summary>
        /// Gets the operation timeout.
        /// </summary>
        /// <value>The operation timeout.</value>
        internal int OperationTimeout
        {
            get
            {
                return this.CommandTimeout.HasValue ? (int)this.CommandTimeout : 120;
            }
        }

        /// <summary>
        /// Gets a connection string to the backend SQL Server 2008 database.
        /// </summary>
        public string StoreConnectionString
        {
            get
            {
                // Compute the store connection string from context connection string.
                EntityConnectionStringBuilder entityBuilder =
                    new EntityConnectionStringBuilder(this.Connection.ConnectionString);

                if (string.IsNullOrEmpty(entityBuilder.ProviderConnectionString))
                {
                    string entityConnectionString =
                        ConfigurationManager.ConnectionStrings[entityBuilder.Name].ConnectionString;
                    entityBuilder = new EntityConnectionStringBuilder(entityConnectionString);
                }

                return entityBuilder.ProviderConnectionString;
            }
        }

        /// <summary>
        /// Enables change history logging feature on the repository. 
        /// </summary>
        /// <param name="changeHistoryFilePath">File path to store change history data. Multiple 
        /// invocations of this method would add new files to the change history filegroup.</param>
        /// <example>This example shows how to enable and disable change history feature. On Vista or 
        /// Windows Server 2008, you may have to run this sample as an administrator.
        /// <code>
        ///using System;
        ///using System.Linq;
        ///using Zentity.Administration;
        ///using Zentity.Core;
        ///using System.Threading;
        ///using System.ServiceProcess;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        public static void Main(string[] args)
        ///        {
        ///            ZentityContext regularContext = new ZentityContext();
        ///            AdministrationContext adminContext = new AdministrationContext();
        ///
        ///            // Enabling and disabling change history is lengthy operation. Set a sufficiently
        ///            // large timeout for the context commands.
        ///            adminContext.CommandTimeout = 300;
        ///
        ///            // Disable SQL Server Agent process before enabling or disabling the change history.
        ///            ServiceController[] scServices;
        ///            scServices = ServiceController.GetServices();
        ///            var v = scServices.Where
        ///                (tuple =&gt; tuple.DisplayName == &quot;SQL Server Agent (MSSQLSERVER)&quot;).FirstOrDefault();
        ///            if (v != null &amp;&amp; v.Status == ServiceControllerStatus.Running)
        ///                v.Stop();
        ///            Thread.Sleep(5000);
        ///
        ///            bool isChangeHistoryEnabled =
        ///                Convert.ToBoolean(regularContext.GetConfiguration(&quot;IsChangeHistoryEnabled&quot;));
        ///
        ///            Console.WriteLine(&quot;IsChangeHistoryEnabled: [{0}]&quot;, isChangeHistoryEnabled);
        ///
        ///            if (isChangeHistoryEnabled)
        ///            {
        ///                Console.WriteLine(&quot;Disabling change history...&quot;);
        ///                adminContext.DisableChangeHistory();
        ///            }
        ///
        ///            Console.WriteLine(&quot;Enabling change history...&quot;);
        ///            adminContext.EnableChangeHistory(@&quot;C:\ChangeHistory.ndf&quot;);
        ///
        ///            // Restart the SQL Agent.
        ///            if (v != null)
        ///                v.Start();
        ///        }
        ///    }
        ///}
        /// </code>
        /// </example>
        /// <remarks>
        /// Zentity change history logging relies on the 'Change Data Capture' feature of SQL 
        /// Server 2008 and is thus available only on Developer, Enterprise and Enterprise 
        /// Evaluation editions.
        /// <para>
        /// This method alters the backend database to create new filegroups and files if they
        /// are not already created. Since ALTER DATABASE statement is not allowed within 
        /// multi-statement transaction, including this method in a TransactionScope, for example,
        /// may raise Exceptions.
        /// </para>
        /// <para>
        /// Also, if you are seeing any Transaction deadlock errors, turn off SQL Server Agent
        /// service, invoke this method and then turn on the agent again.
        /// </para>
        /// While enabling change history logging, each major table in Zentity database is enabled 
        /// for change data capture. SQL Server automatically creates two jobs during this process,
        /// 1. to populate capture instances and 2. to periodically clean up the capture instances. 
        /// Zentity derives its change history data from these capture instances. A background job, 
        /// ProcessNextLSN, pulls data from the capture instances and populates a separate set of 
        /// ‘Coupling’ tables. These coupling tables allow us to retain the historical data even 
        /// after the capture instances are cleaned up. 'Coupling' tables are then mapped to the 
        /// conceptual model of Zentity Change History Logging. The public API is generated by 
        /// Entity Framework from the conceptual model. Figure below presents an overall picture.
        /// <br/>
        /// <img src="ChangeHistory.bmp"/>
        /// <para>
        /// <b>Data Loss Scenarios</b>
        /// <br/>
        /// Since Zentity processes CDC capture instances to retrieve the change history 
        /// information, there is a possibility of data loss if the source capture instance is 
        /// deleted before it is completely processed. This might happen if Change Data Capture is 
        /// disabled on the database before all the changesets are processed. To print a list of 
        /// changesets yet to be processed, execute a query similar to the following.
        /// <code language="SQL">
        ///SELECT [start_lsn], [tran_end_time] 
        ///FROM [cdc].lsn_time_mapping 
        ///WHERE tran_id &lt;&gt; 0x00
        ///EXCEPT
        ///SELECT Administration.fn_hexstrtovarbin([Id]) [start_lsn], [DateCreated] [tran_end_time] 
        ///FROM [Administration].ChangeSet
        /// </code>
        /// Another scenario where there is a possibility of data loss is while altering the schema 
        /// of Core.Resource table during Zentity Data Model updates. A new capture instance is 
        /// created for Core.Resource table in response to the schema change. Since, SQL Server 
        /// allows a maximum of two capture instances per table, an earlier capture instance is 
        /// dropped if the capture instance count increases two. All data present in the dropped 
        /// capture instance is thus lost.
        /// </para>
        /// <para>
        /// <b>Backup and Restore Scenario</b>
        /// <br/>
        /// During backup and restore of Zentity database, all configuration values and capture 
        /// instances are restored. However, the jobs to populate and cleanup capture instances and
        /// 'ProcessNextLSN' are not restored on the target server. So, even though sys.databases 
        /// and sys.tables show that the database and table are configured for change data capture,
        /// no entries are created in the capture instances and hence the Administration tables.
        /// To fix this, disable change history logging and then re-enable it again using either 
        /// the stored procedures Administration.DisableChangeHistory and 
        /// Administration.EnableChangeHistory or using the methods 
        /// <see cref="Zentity.Administration.AdministrationContext.DisableChangeHistory"/> and
        /// <see cref="Zentity.Administration.AdministrationContext.EnableChangeHistory(string)"/>.
        /// </para>
        /// </remarks>
        public void EnableChangeHistory(string changeHistoryFilePath)
        {
            if (string.IsNullOrEmpty(changeHistoryFilePath))
                throw new ArgumentException(
                    AdministrationResources.InvalidFilePath,
                    "changeHistoryFilePath");

            using (EntityCommand cmd = (EntityCommand)this.Connection.CreateCommand())
            {
                bool isConnectionOpenedHere = false;
                if (this.Connection.State == ConnectionState.Closed)
                {
                    this.Connection.Open();
                    isConnectionOpenedHere = true;
                }

                cmd.CommandText = "AdministrationContext.EnableChangeHistory";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = this.OperationTimeout;

                EntityParameter param = cmd.CreateParameter();
                param.DbType = DbType.String;
                param.ParameterName = "ChangeHistoryFilePath";
                param.Size = 512;
                param.Value = changeHistoryFilePath;
                cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();

                if (isConnectionOpenedHere)
                    this.Connection.Close();
            }
        }

        /// <summary>
        /// Disables change history logging on the repository.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Zentity change history logging is built on top of SQL Server 'Change Data Capture' 
        /// feature. A background SQL job copies the change data from capture instances to 
        /// Administration namespace tables. This method disables 'Change Data Capture' on Zentity 
        /// database which deletes all the capture instances. This method does not delete the data 
        /// that has been moved to the Administration tables.
        /// </para>
        /// <para>
        /// It is possible that some data might still be present in the capture instances when 
        /// this method is called. In such a case, that data is lost since the capture instance 
        /// is dropped. To print a list of changesets yet to be processed, execute a query similar 
        /// to the following.
        /// <code language="SQL">
        ///SELECT [start_lsn], [tran_end_time] 
        ///FROM [cdc].lsn_time_mapping 
        ///WHERE tran_id &lt;&gt; 0x00
        ///EXCEPT
        ///SELECT Administration.fn_hexstrtovarbin([Id]) [start_lsn], [DateCreated] [tran_end_time] 
        ///FROM [Administration].ChangeSet
        /// </code>
        /// </para>
        /// <para>
        /// Turn off the SQL Server Agent service before invoking this method if you are seeing any 
        /// transaction deadlocks.
        /// </para>
        /// </remarks>
        public void DisableChangeHistory()
        {
            using (EntityCommand cmd = (EntityCommand)this.Connection.CreateCommand())
            {
                bool isConnectionOpenedHere = false;
                if (this.Connection.State == ConnectionState.Closed)
                {
                    this.Connection.Open();
                    isConnectionOpenedHere = true;
                }

                cmd.CommandText = "AdministrationContext.DisableChangeHistory";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = this.OperationTimeout;

                cmd.ExecuteNonQuery();
                if (isConnectionOpenedHere)
                    this.Connection.Close();
            }
        }

        /// <summary>
        /// Enable full text indexes on backend tables.
        /// </summary>
        /// <remarks>
        /// This method creates full text filegroup, file, catalog and indexes on backend table 
        /// if they are not present already. Also, it creates a background SQL job that 
        /// incrementatly populates the full text indexes on various tables.
        /// <br/>
        /// Enabling full text search requires Full Text Search service to be installed 
        /// on the current instance of SQL Server. If SQL Server Agent service is not present on
        /// the deployment box, you have to manually update the full text indexes or you can
        /// schedule a custom job to update full text indexes using Windows Scheduler.
        /// </remarks>
        /// <param name="fullTextCatalogFilePath">Path to the secondary file to 
        /// keep all full text indexes.</param>
        public void EnableFullTextSearch(string fullTextCatalogFilePath)
        {
            if (string.IsNullOrEmpty(fullTextCatalogFilePath))
                throw new ArgumentException(
                    AdministrationResources.InvalidFilePath,
                    "fullTextCatalogFilePath");

            using (EntityCommand cmd = (EntityCommand)this.Connection.CreateCommand())
            {
                bool isConnectionOpenedHere = false;
                if (this.Connection.State == ConnectionState.Closed)
                {
                    this.Connection.Open();
                    isConnectionOpenedHere = true;
                }

                cmd.CommandText = "AdministrationContext.EnableFullTextSearch";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = this.OperationTimeout;

                EntityParameter param = cmd.CreateParameter();
                param.DbType = DbType.String;
                param.ParameterName = "FullTextCatalogFilePath";
                param.Size = 512;
                param.Value = fullTextCatalogFilePath;
                cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();
                if (isConnectionOpenedHere)
                    this.Connection.Close();
            }
        }

        /// <summary>
        /// Disables full text indexing on backend tables.
        /// </summary>
        /// <remarks>
        /// This method removes the SQL jobs that populate the full text indexes.
        /// Full text indexes are not dropped, so you can query any existing full
        /// text index data.
        /// </remarks>
        public void DisableFullTextSearch()
        {
            using (EntityCommand cmd = (EntityCommand)this.Connection.CreateCommand())
            {
                bool isConnectionOpenedHere = false;
                if (this.Connection.State == ConnectionState.Closed)
                {
                    this.Connection.Open();
                    isConnectionOpenedHere = true;
                }
                cmd.CommandText = "AdministrationContext.DisableFullTextSearch";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = this.OperationTimeout;

                cmd.ExecuteNonQuery();
                if (isConnectionOpenedHere)
                    this.Connection.Close();
            }
        }
    }
}
