// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Data
{
    using System;
    using System.Configuration;
    using System.Data.Services;
    using System.Data.Services.Common;
    using System.Data.Services.Providers;
    using System.Diagnostics;
    using Zentity.Core;

    /// <summary>
    /// The ZentityDataService class is the custom WCF Data Service class that exposes Zentity
    /// data as an OData compliant data service.
    /// </summary>
    public class ZentityDataService : DataService<ZentityContext>, IServiceProvider
    {
        private const int DefaultPageSize = 50;

        /// <summary>
        /// This method is called only once to initialize service-wide policies.
        /// </summary>
        /// <param name="config">The data service configurations.</param>
        public static void InitializeService(DataServiceConfiguration config)
        {
            // Examples:
            // config.SetEntitySetAccessRule("MyEntityset", EntitySetRights.AllRead);
            // config.SetServiceOperationAccessRule("MyServiceOperation", ServiceOperationRights.All);

            config.UseVerboseErrors = true;
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
            config.SetEntitySetAccessRule("*", EntitySetRights.All);
            int entityPageSize;
            if (!Int32.TryParse(ConfigurationManager.AppSettings["EntityPageSize"], out entityPageSize))
            {
                entityPageSize = DefaultPageSize;
            }
            config.SetEntitySetPageSize("*", entityPageSize);
        }

        #region IServiceProvider Members

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>
        /// A service object of type <paramref name="serviceType"/>.-or- null if there is no service object of type <paramref name="serviceType"/>.
        /// </returns>
        public object GetService(Type serviceType)
        {
            // Handle only custom service handler requests for Streaming Provider
            if (serviceType == typeof(IDataServiceStreamProvider))
            {
                // Create an instance of the stream provider to return to the data service.
                ZentityFileStreamProvider provider = new ZentityFileStreamProvider(this.CurrentDataSource);

                // Register the processed changeset event for the pipeline events.
                this.ProcessingPipeline.ProcessedChangeset += provider.SaveStreamToDatabase;

                return provider;
            }

            return null;
        }

        #endregion

        /// <summary>
        /// Creates the data source.
        /// </summary>
        /// <returns>The data context instance</returns>
        protected override ZentityContext CreateDataSource()
        {
            ZentityContext context = base.CreateDataSource();
            context.CommandTimeout = context.Connection.ConnectionTimeout;

            // Load the metadata and classes from all the Entity assemblies
            foreach (var assemblyCacheEntry in AssemblyCache.Current)
            {
                context.MetadataWorkspace.LoadFromAssembly(assemblyCacheEntry.Value);
            }
            
            context.ContextOptions.LazyLoadingEnabled = false;
            return context;
        }

        /// <summary>
        /// Handles the exception that has occured in data service and log it in trace file.
        /// </summary>
        /// <param name="args">The arguments.</param>
        protected override void HandleException(HandleExceptionArgs args)
        {
            Exception dataServiceException = args.Exception.InnerException ?? args.Exception;
            if (dataServiceException != null)
            {
                Globals.TraceMessage(TraceEventType.Error, dataServiceException.ToString(), dataServiceException.Message);
            }
        }
    }
}
