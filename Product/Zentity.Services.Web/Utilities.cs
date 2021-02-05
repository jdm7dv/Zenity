// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Web
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Core;

    /// <summary>
    /// Contains properties and methods for global use across the project
    /// </summary>
    public static class Utilities
    {
        private static string connectionString;

        /// <summary>
        /// Gets the folder path of the currently executing assembly.
        /// </summary>
        /// <value>The assembly folder path.</value>
        public static string AssemblyLocation
        {
            get
            {
                return new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)).LocalPath;
            }
        }

        /// <summary>
        /// Gets the zentity database sql server's connection string.
        /// </summary>
        /// <value>The zentity connection string.</value>
        public static string ZentityConnectionString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    using (ZentityContext context = new ZentityContext())
                    {
                        connectionString = context.StoreConnectionString;
                    }
                }
                return connectionString;
            }
        }

        /// <summary>
        /// Gets the supported image MIME types.
        /// </summary>
        /// <value>The image MIME types collection.</value>
        public static IEnumerable<string> ImageMimeTypes
        {
            get
            {
                return Properties.Resources.ImageMimeTypes.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).AsEnumerable();
            }
        }

        /// <summary>
        /// Creates a default instance of ZentityContext and loads the metadata from the entity type assemblies.
        /// </summary>
        /// <returns>An instance of ZentityContext</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The ZentityContext instance is supposed to be disposed by the calling method. This method only initializes the instance.")]
        public static ZentityContext CreateZentityContext()
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            ZentityContext context = new ZentityContext();
            context.CommandTimeout = context.Connection.ConnectionTimeout;

            try
            {
                foreach (var assemblyCacheEntry in AssemblyCache.Current)
                {
                    context.MetadataWorkspace.LoadFromAssembly(assemblyCacheEntry.Value);
                }
            }
            catch (Exception exception)
            {
                Globals.TraceMessage(TraceEventType.Error, exception.Message, Properties.Messages.FailLoadAssemblyTitle);
            }
            
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);
            return context; 
        }

        /// <summary>
        /// Gets a list of Core.ResourceType instances from the ZentityContext.
        /// </summary>
        /// <param name="modelNamespace">The model namespace.</param>
        /// <returns>List of Core.ResourceType instances</returns>
        public static IEnumerable<ResourceType> GetResourceTypes(string modelNamespace)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            using (ZentityContext context = Utilities.CreateZentityContext())
            {
                try
                {
                    var dataModuleResourceTypes = (from DataModelModule dm in context.DataModel.Modules
                                                   where dm.NameSpace.Equals(modelNamespace, StringComparison.OrdinalIgnoreCase)
                                                   select dm.ResourceTypes).FirstOrDefault();


                    Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);
                    return dataModuleResourceTypes;
                }
                catch (Exception exception)
                {
                    Globals.TraceMessage(TraceEventType.Error, exception.Message, Properties.Messages.FailLoadAssemblyTitle);
                    return null;
                }
            }
        }
    }
}