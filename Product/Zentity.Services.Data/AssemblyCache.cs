// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System.Diagnostics;

namespace Zentity.Services.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Objects.DataClasses;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A strongly typed class to hold a dictionary of core and custom entity assemblies
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "This class is used internally and is never serialized.")]
    public class AssemblyCache : Dictionary<string, Assembly>
    {
        /// <summary>
        /// An object instance used to lock critical code blocks related to the singleton AssemblyCache instance.
        /// </summary>
        private static readonly object LockObject = new object();

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static AssemblyCache currentInstance;

        /// <summary>
        /// Prevents a default instance of the <see cref="AssemblyCache"/> class from being created.
        /// </summary>
        private AssemblyCache()
        {
            this.LoadAssemblyCache();
        }

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
            this.LoadAssemblyCache();
        }

        /// <summary>
        /// Loads the assembly cache.
        /// </summary>
        private void LoadAssemblyCache()
        {
            // Locking the current instance before making changes
            lock (AssemblyCache.LockObject)
            {
                // Clearing the dictionary holding the assemblies
                base.Clear();

                // Fetching the list of assemblies in the assembly folder
                string[] assemblyFileList = Directory.GetFiles(AssemblyCache.AssemblyLocation, "*.dll");
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
                                var resourceTypes = currentAssembly.GetExportedTypes().Where(type => type.IsSubclassOf(typeof(EntityObject)));
                                if (resourceTypes != null && resourceTypes.Count() > 0)
                                {
                                    if (currentAssembly.FullName != null)
                                    {
                                        if (!this.ContainsKey(currentAssembly.FullName))
                                        {
                                            this.Add(currentAssembly.FullName, currentAssembly);
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
        }
    }
}