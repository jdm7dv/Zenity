// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zentity.Core;
using System.Configuration;
using Zentity.SchWorksAndAuthArtifactGenerator.Properties;

namespace Zentity.SchWorksAndAuthArtifactGenerator
{
    /// <summary>
    /// Generates consolidated artifacts for Scholarly Works and Authorization data models.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine(Messages.RemovingNamespace, Resources.ScholarlyWorksAuthorizationNamespace);
                Artifacts.RemoveModule(Resources.ScholarlyWorksAuthorizationNamespace);
                Console.WriteLine(Messages.RemovedNamespace, Resources.ScholarlyWorksAuthorizationNamespace);
                Console.WriteLine();

                Console.WriteLine(Messages.GeneratingArtifacts);
                string[] assemblyNames = { Resources.SecurityAuthorizationAssembly, Resources.ScholarlyWorksAssembly };
                Artifacts.GenerateConsolidatedArtifacts(Resources.ScholarlyWorksAuthorizationNamespace, assemblyNames);
                Console.WriteLine(Messages.GeneratedArtifacts);
                Console.WriteLine();
            }
            catch (Exception exception)
            {
                Console.WriteLine(Messages.GeneralErrorMessage + exception.Message);
                Console.WriteLine();
            }

            Console.WriteLine(Messages.PressKeyToExit);
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Generates artifacts together for 2 or more data models.
    /// </summary>
    internal static class Artifacts
    {
        /// <summary>
        /// Generates and saves the artifact files for the extensions assemblies at the path mentioned in 
        /// application configuration file.
        /// </summary>
        /// <param name="nameSpace">Namespace of the consolidated artifacts.</param>
        /// <param name="assemblyNames">Array of assembly names. The assemblies must be available
        /// to the application.</param>
        internal static void GenerateConsolidatedArtifacts(string nameSpace, string[] assemblyNames)
        {
            using (ZentityContext context
                = new ZentityContext(ConfigurationManager.ConnectionStrings[Resources.ZentityContext].ToString()))
            {
                DataModel model = context.DataModel;
                DataModelModule module = new DataModelModule();
                module.NameSpace = nameSpace;
                model.Modules.Add(module);

                model.Synchronize();

                EFArtifactGenerationResults results = context.DataModel.GenerateEFArtifacts(assemblyNames);

                results.Csdls.Where(tuple => tuple.Key == Resources.ZentityCore).First().
                    Value.Save(Resources.ExtendedCoreCsdl);

                for (int i = 0; i < assemblyNames.Length; i++)
                {
                    results.Csdls.Where(tuple => tuple.Key == assemblyNames[i]).
                        First().Value.Save(assemblyNames[i] + Resources.FileExtensionCsdl);
                }

                results.Msl.Save(Resources.ConsolidatedMsl);
                results.Ssdl.Save(Resources.ConsolidatedSsdl);                
            }
        }

        /// <summary>
        /// Remvoes the module from the system.
        /// </summary>
        /// <param name="nameSpace">Namespace to be removed.</param>
        internal static void RemoveModule(string nameSpace)
        {
            using (ZentityContext context
                = new ZentityContext(ConfigurationManager.ConnectionStrings[Resources.ZentityContext].ToString()))
            {
                if (context.DataModel.Modules.Where(tuple => tuple.NameSpace == nameSpace)
                    .FirstOrDefault() != null)
                {
                    DataModelModule module = context.DataModel.Modules[nameSpace];
                    context.DataModel.Modules.Remove(module);
                    context.DataModel.Synchronize();
                }
            }
        }
    }
}
