// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Web
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Zentity.Core;

    /// <summary>
    /// A strongly typed class to hold a dictionary of core and custom entity assemblies
    /// </summary>
    public class AssemblyCache : IEnumerable<KeyValuePair<string, Assembly>>
    {
        /// <summary>
        /// Internal dictionary to store the cached Assembly objects loaded via reflection
        /// </summary>
        private readonly Dictionary<string, Assembly> assemblyCache;

        /// <summary>
        /// An object instance used to lock critical code blocks related to the singleton AssemblyCache instance.
        /// </summary>
        private static readonly object LockObject = new object();

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static AssemblyCache currentInstance;

        /// <summary>
        /// Prevents a default instance of the AssemblyCache class from being created.
        /// </summary>
        private AssemblyCache()
        {
            this.assemblyCache = new Dictionary<string, Assembly>();
            this.LoadAssemblyCache();
        }

        /// <summary>
        /// Gets the singleton instance of the AssemblyCache.
        /// </summary>
        /// <value>The current instance of AssemblyCache.</value>
        public static AssemblyCache Current
        {
            get
            {
                if (AssemblyCache.currentInstance == null)
                {
                    currentInstance = new AssemblyCache();
                }

                return AssemblyCache.currentInstance;
            }
        }

        /// <summary>
        /// Reloads this AssemblyCache instance.
        /// </summary>
        public void Reload()
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            this.LoadAssemblyCache();
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);
        }

        /// <summary>
        /// Gets the assembly cache item containing the given namespace.
        /// </summary>
        /// <param name="namespaceName">Name of the namespace.</param>
        /// <returns>Assembly cache item or default</returns>
        public KeyValuePair<string, Assembly> GetItemContainingNamespace(string namespaceName)
        {
            if (!string.IsNullOrWhiteSpace(namespaceName))
            {
                return (from cacheItem in this.assemblyCache
                        let namespaceList = cacheItem.Value.GetExportedTypes().Select(type => type.Namespace).Distinct()
                        where namespaceList.Contains(namespaceName, StringComparer.OrdinalIgnoreCase)
                        select cacheItem).FirstOrDefault();
            }

            return default(KeyValuePair<string, Assembly>);
        }

        /// <summary>
        /// Loads the assembly cache.
        /// </summary>
        private void LoadAssemblyCache()
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            // Locking the current instance before making changes
            lock (AssemblyCache.LockObject)
            {
                // Clearing the dictionary holding the assemblies
                this.assemblyCache.Clear();

                // Fetching the list of assemblies in the assembly folder
                string[] assemblyFileList = Directory.GetFiles(Utilities.AssemblyLocation, Properties.Resources.AssemblyExtension);
                if (assemblyFileList.Length > 0)
                {
                    foreach (string assemblyFile in assemblyFileList)
                    {
                        Assembly currentAssembly;
                        try
                        {
                            currentAssembly = Assembly.LoadFrom(assemblyFile);
                            if (currentAssembly != null)
                            {
                                var resourceTypes = currentAssembly.GetExportedTypes().Where(type => type.IsSubclassOf(typeof(Resource)));
                                if (resourceTypes != null && resourceTypes.Count() > 0)
                                {
                                    if (currentAssembly.FullName != null)
                                    {
                                        if (!this.assemblyCache.ContainsKey(currentAssembly.FullName))
                                        {
                                            this.assemblyCache.Add(currentAssembly.FullName, currentAssembly);
                                        }
                                    }
                                }
                            }
                        }
                        catch (System.Exception exception)
                        {
                            Globals.TraceMessage(TraceEventType.Error, exception.InnerException != null ? exception.InnerException.ToString() : exception.ToString(), exception.Message);
                        }
                    }
                }
            }

            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);
        }

        /// <summary>
        /// Gets the Assembly Cache's enumerator.
        /// </summary>
        /// <returns>Returns an IEnumerator of KeyValuePair (string, Assembly)</returns>
        public IEnumerator<KeyValuePair<string, Assembly>> GetEnumerator()
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);
            return this.assemblyCache.GetEnumerator();
        }

        /// <summary>
        /// Gets the Assembly Cache's enumerator.
        /// </summary>
        /// <returns>Returns an IEnumerator of Assembly Cache's Dictionary</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);
            return this.assemblyCache.GetEnumerator();
        }
    }
}
