// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Web.Data
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.Xml.Linq;
    using Zentity.Core;

    /// <summary>
    /// Data Model Service
    /// </summary>
    public class DataModelService : IDataModelService
    {
        /// <summary>
        /// Will create a new data model if the namespace is not present.
        /// </summary>
        /// <param name="rdfsXml">RDFS Xml document string</param>
        /// <param name="schemaXml">Schema Xml document string</param>
        /// <param name="modelNamespace">Namespace of the model</param>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public void CreateDataModel(string rdfsXml, string schemaXml, string modelNamespace)
        {
            #region Validating Parameters

            if (string.IsNullOrEmpty(rdfsXml) || string.IsNullOrWhiteSpace(rdfsXml) ||
                string.IsNullOrEmpty(schemaXml) || string.IsNullOrWhiteSpace(schemaXml) ||
                string.IsNullOrEmpty(modelNamespace) || string.IsNullOrWhiteSpace(modelNamespace))
            {
                throw new FaultException(new FaultReason(Properties.Messages.NullEmptyMessage));
                //throw new FaultException<ArgumentNullException>(new ArgumentNullException(Properties.Messages.NullEmptyTitle), new FaultReason(Properties.Messages.NullEmptyMessage));
            }

            XDocument rdfsDocument;
            XDocument schemaDocument;
            try
            {
                rdfsDocument = XDocument.Parse(rdfsXml);
                schemaDocument = XDocument.Parse(schemaXml);
            }
            catch (Exception)
            {
                throw new FaultException(new FaultReason(Properties.Messages.XmlParseFaultReason));
                //throw new FaultException<System.Xml.XmlException>(new System.Xml.XmlException(), new FaultReason(Properties.Messages.XmlParseFaultReason));
            }

            #endregion

            DataModel dataModel;
            DataModelModule dataModelModule;
            ResourceTypeCollection resourceTypes;
            try
            {
                using (ZentityContext zentityContext = new ZentityContext())
                {
                    dataModel = zentityContext.DataModel;
                    if (dataModel != null)
                    {
                        DataModelModule dataModelModuleExists = dataModel.Modules.FirstOrDefault(m => m.NameSpace.Equals(modelNamespace));
                        if (dataModelModuleExists == null)
                        {
                            dataModelModule = dataModel.Modules[Properties.Resources.ZentityCore];
                            if (dataModelModule != null)
                            {
                                resourceTypes = dataModelModule.ResourceTypes;
                                if (resourceTypes != null && resourceTypes.Count > 0)
                                {
                                    ResourceType baseResourceType = resourceTypes[Properties.Resources.ResourceName];
                                    if (baseResourceType != null)
                                    {
                                        DataModelModule newDataModelModule = DataModelModule.CreateFromRdfs(baseResourceType, rdfsDocument, schemaDocument, modelNamespace, false);
                                        zentityContext.DataModel.Modules.Add(newDataModelModule);
                                        zentityContext.DataModel.Synchronize();
                                    }
                                    else
                                    {
                                        throw new FaultException(new FaultReason(Properties.Messages.BaseResourceTypeFaultReason));
                                        //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(string.Format(Properties.Messages.ResourceTypeFormat, Properties.Resources.ResourceName)), new FaultReason(Properties.Messages.BaseResourceTypeFaultReason));
                                    }
                                }
                                else
                                {
                                    throw new FaultException(new FaultReason(Properties.Messages.NullResourceTypesFaultReason));
                                    //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(Properties.Resources.ResourceTypeName), new FaultReason(Properties.Messages.NullResourceTypesFaultReason));
                                }
                            }
                            else
                            {
                                throw new FaultException(new FaultReason(Properties.Messages.NullModelConfigurationMessage));
                                //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(string.Format(Properties.Messages.DataModelFormat, Properties.Resources.ZentityCore)), new FaultReason(Properties.Messages.NullModelConfigurationMessage));
                            }
                        }
                        else
                        {
                            throw new FaultException(new FaultReason(Properties.Messages.DuplicateDataModelFaultReason));
                            //throw new FaultException<DuplicateNameException>(new DuplicateNameException(Properties.Resources.DataModelName), new FaultReason(Properties.Messages.DuplicateDataModelFaultReason));
                        }
                    }
                    else
                    {
                        throw new FaultException(new FaultReason(Properties.Messages.ContextFaultReason));
                        //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(Properties.Messages.ZentityContext), new FaultReason(Properties.Messages.ContextFaultReason));
                    }
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new FaultException(new FaultReason(exception.ToString()));
                //throw new FaultException<Exception>(exception, new FaultReason(Properties.Messages.ExceptionFaultReason));
            }
        }

        /// <summary>
        /// Will delete the existing data model and related resources within the Zentity store with the  specified namespace.
        /// </summary>
        /// <param name="modelNamespace">Namespace of the model</param>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public void DeleteDataModel(string modelNamespace)
        {
            #region Validating Parameters

            if (string.IsNullOrEmpty(modelNamespace) || string.IsNullOrWhiteSpace(modelNamespace))
            {
                throw new FaultException(new FaultReason(Properties.Messages.NullEmptyMessage));
            }

            #endregion

            DataModel dataModel;
            DataModelModule dataModelModule;
            try
            {
                using (ZentityContext zentityContext = new ZentityContext())
                {
                    dataModel = zentityContext.DataModel;
                    if (dataModel != null)
                    {
                        dataModelModule = dataModel.Modules[modelNamespace];
                        if (dataModelModule != null)
                        {
                            dataModel.Modules.Remove(dataModelModule);
                        }
                        else
                        {
                            throw new FaultException(new FaultReason(Properties.Messages.InvalidDataModelFaultReason));
                            //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(string.Format(Properties.Messages.DataModelFormat, modelNamespace)), new FaultReason(Properties.Messages.InvalidDataModelFaultReason));
                        }
                    }
                    else
                    {
                        throw new FaultException(new FaultReason(Properties.Messages.ContextFaultReason));
                        //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(Properties.Messages.ZentityContext), new FaultReason(Properties.Messages.ContextFaultReason));
                    }

                    dataModel.Synchronize();
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new FaultException(new FaultReason(exception.ToString()));
                //throw new FaultException<Exception>(exception, new FaultReason(Properties.Messages.ExceptionFaultReason));
            }
        }

        /// <summary>
        /// This will stream the raw data of the assembly in a Hierarchical pattern for the particular data model with the specified namespace.
        /// </summary>
        /// <param name="outputAssemblyName">Namespace of the model</param>
        /// <param name="metadataModelNamespaces">List of Metadata Namespaces</param>
        /// <param name="assemblyModelNamespaces">List of Model Namespaces</param>
        /// <param name="storageLocation">Storage Location of the generated assemblies.</param>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public void GenerateHierarchicalAssemblies(string outputAssemblyName, string[] metadataModelNamespaces, string[] assemblyModelNamespaces, string storageLocation)
        {
            #region Validating Parameters

            if (string.IsNullOrWhiteSpace(outputAssemblyName) || string.IsNullOrEmpty(storageLocation))
            {
                throw new FaultException(new FaultReason(Properties.Messages.NullEmptyMessage));
                //throw new FaultException<ArgumentNullException>(new ArgumentNullException(Properties.Messages.NullEmptyTitle), new FaultReason(Properties.Messages.NullEmptyMessage));
            }

            if (!System.IO.Directory.Exists(storageLocation))
            {
                throw new FaultException(new FaultReason(Properties.Messages.DestinationAssemblyFolderPathFaultReason));
                //throw new FaultException<DirectoryNotFoundException>(new DirectoryNotFoundException(), new FaultReason(Properties.Messages.ExceptionFaultReason));
            }

            try { outputAssemblyName = outputAssemblyName.Replace(Properties.Resources.DllExtension, string.Empty); }
            catch (ArgumentException)
            {
                throw new FaultException(new FaultReason(Properties.Messages.InvalidFileNameFaultReason));
            }

            #endregion

            try
            {
                using (ZentityContext zentityContext = new ZentityContext())
                {
                    var validAssemblyModules = from module in zentityContext.DataModel.Modules
                                               where assemblyModelNamespaces.Contains(module.NameSpace, StringComparer.OrdinalIgnoreCase)
                                               select module.NameSpace;

                    var missingModules = assemblyModelNamespaces.Except(validAssemblyModules);
                    if (missingModules.Count() > 0)
                    {
                        throw new FaultException(new FaultReason(Properties.Messages.InvalidModelNamespaceFaultReason));
                        //throw new FaultException<ArgumentException>(new ArgumentException(), new FaultReason(""));
                    }

                    using (FileStream fout = new FileStream(Path.Combine(storageLocation, outputAssemblyName + Properties.Resources.DllExtension), FileMode.Create, FileAccess.Write))
                    {
                        byte[] rawAssembly = zentityContext.DataModel.GenerateExtensionsAssembly(outputAssemblyName, false, null, assemblyModelNamespaces, null);
                        if (rawAssembly != null)
                        {
                            fout.Write(rawAssembly, 0, rawAssembly.Length);
                        }
                        else
                        {
                            throw new FaultException(new FaultReason(Properties.Messages.GenerateAssemblyFailedFaultReason));
                            //throw new FaultException<InvalidOperationException>(new InvalidOperationException(), new FaultReason("Unable to generate assembly"));
                        }
                    }

                    if (metadataModelNamespaces == null || metadataModelNamespaces.Count() == 0)
                    {
                        return;
                    }

                    GenerateHierarchicalMetadataAssembly(metadataModelNamespaces, storageLocation);
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new FaultException(new FaultReason(exception.ToString()));
                //throw;
            }
        }

        /// <summary>
        /// This will stream the raw data of the assembly in a Flattened pattern for the particular data model with the specified namespace.
        /// </summary>
        /// <param name="outputAssemblyName">Namespace of the model</param>
        /// <param name="metadataModelNamespaces">List of Metadata Namespaces</param>
        /// <param name="assemblyModelNamespaces">List of Model Namespaces</param>
        /// <param name="storageLocation">Storage Location of the generated assemblies.</param>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public void GenerateFlattenedAssemblies(string outputAssemblyName, string[] metadataModelNamespaces, string[] assemblyModelNamespaces, string storageLocation)
        {
            #region Validating Parameters

            if (string.IsNullOrWhiteSpace(outputAssemblyName) || string.IsNullOrEmpty(storageLocation))
            {
                throw new FaultException(new FaultReason(Properties.Messages.NullEmptyMessage));
            }

            if (!System.IO.Directory.Exists(storageLocation))
            {
                throw new FaultException(new FaultReason(Properties.Messages.DestinationAssemblyFolderPathFaultReason));
            }

            try { outputAssemblyName = outputAssemblyName.Replace(Properties.Resources.DllExtension, string.Empty); }
            catch (ArgumentException)
            {
                throw new FaultException(new FaultReason(Properties.Messages.InvalidFileNameFaultReason));
            }

            #endregion

            try
            {
                using (ZentityContext zentityContext = new ZentityContext())
                {
                    var validAssemblyModules = from module in zentityContext.DataModel.Modules
                                               where assemblyModelNamespaces.Contains(module.NameSpace, StringComparer.OrdinalIgnoreCase)
                                               select module.NameSpace;

                    var missingModules = assemblyModelNamespaces.Except(validAssemblyModules);
                    if (missingModules.Count() > 0)
                    {
                        throw new FaultException(new FaultReason(Properties.Messages.InvalidModelNamespaceFaultReason));
                    }

                    using (FileStream fout = new FileStream(Path.Combine(storageLocation, outputAssemblyName + Properties.Resources.DllExtension), FileMode.Create, FileAccess.Write))
                    {
                        byte[] rawAssembly = zentityContext.DataModel.GenerateFlattenedExtensionsAssembly(outputAssemblyName, false, null, assemblyModelNamespaces, null);
                        if (rawAssembly != null)
                        {
                            fout.Write(rawAssembly, 0, rawAssembly.Length);
                        }
                        else
                        {
                            throw new FaultException(new FaultReason(Properties.Messages.GenerateAssemblyFailedFaultReason));
                        }
                    }

                    if (metadataModelNamespaces == null || metadataModelNamespaces.Count() == 0)
                    {
                        return;
                    }

                    GenerateFlattenedMetadataAssembly(metadataModelNamespaces, storageLocation);
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new FaultException(new FaultReason(exception.ToString()));
            }
        }

        /// <summary>
        /// This will stream the raw data of the metadata assembly in a Hierarchical pattern.
        /// </summary>
        /// <param name="metadataModelNamespaces">List of Metadata Namespaces</param>
        /// <param name="storageLocation">Storage Location of the generated assemblies.</param>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public void GenerateHierarchicalMetadataAssembly(string[] metadataModelNamespaces, string storageLocation)
        {
            try
            {
                using (ZentityContext zentityContext = new ZentityContext())
                {
                    var validMetadataModules = from module in zentityContext.DataModel.Modules
                                               where metadataModelNamespaces.Contains(module.NameSpace, StringComparer.OrdinalIgnoreCase)
                                               select module.NameSpace;
                    var missingModules = metadataModelNamespaces.Except(validMetadataModules);
                    if (missingModules.Count() > 0)
                    {
                        throw new FaultException(new FaultReason(Properties.Messages.InvalidMetadataNamespaceFaultReason));
                        //throw new FaultException<ArgumentException>(new ArgumentException(), new FaultReason(""));
                    }

                    using (FileStream fout = new FileStream(Path.Combine(storageLocation, Properties.Resources.MetadataDllName + Properties.Resources.DllExtension), FileMode.Create, FileAccess.Write))
                    {
                        byte[] rawAssembly = zentityContext.DataModel.GenerateExtensionsAssembly(Properties.Resources.MetadataDllName, true, metadataModelNamespaces, null, null);
                        if (rawAssembly != null)
                        {
                            fout.Write(rawAssembly, 0, rawAssembly.Length);
                        }
                        else
                        {
                            throw new FaultException(new FaultReason(Properties.Messages.GenerateAssemblyFailedFaultReason));
                            //throw new FaultException<InvalidOperationException>(new InvalidOperationException(), new FaultReason("Unable to generate metadata assembly"));
                        }
                    }
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new FaultException(new FaultReason(exception.ToString()));
                //throw;
            }
        }

        /// <summary>
        /// This will stream the raw data of the assembly in a Flattened pattern.
        /// </summary>
        /// <param name="metadataModelNamespaces">List of Metadata Namespaces</param>
        /// <param name="storageLocation">Storage Location of the generated assemblies.</param>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public void GenerateFlattenedMetadataAssembly(string[] metadataModelNamespaces, string storageLocation)
        {
            try
            {
                using (ZentityContext zentityContext = new ZentityContext())
                {
                    var validMetadataModules = from module in zentityContext.DataModel.Modules
                                               where metadataModelNamespaces.Contains(module.NameSpace, StringComparer.OrdinalIgnoreCase)
                                               select module.NameSpace;

                    var missingModules = metadataModelNamespaces.Except(validMetadataModules);
                    if (missingModules.Count() > 0)
                    {
                        throw new FaultException(new FaultReason(Properties.Messages.InvalidMetadataNamespaceFaultReason));
                    }

                    using (FileStream fout = new FileStream(Path.Combine(storageLocation, Properties.Resources.MetadataDllName + Properties.Resources.DllExtension), FileMode.Create, FileAccess.Write))
                    {
                        byte[] rawAssembly = zentityContext.DataModel.GenerateFlattenedExtensionsAssembly( Properties.Resources.MetadataDllName, true, metadataModelNamespaces, null, null);
                        if (rawAssembly != null)
                        {
                            fout.Write(rawAssembly, 0, rawAssembly.Length);
                        }
                        else
                        {
                            throw new FaultException( new FaultReason(Properties.Messages.GenerateAssemblyFailedFaultReason));
                        }
                    }
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new FaultException(new FaultReason(exception.ToString()));
            }
        }

        /// <summary>
        /// Gets all the Data models in the DB
        /// </summary>
        /// <returns>Collection of Data Models</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public string[] GetAllDataModels()
        {
            DataModel dataModel;
            try
            {
                using (ZentityContext zentityContext = new ZentityContext())
                {
                    dataModel = zentityContext.DataModel;
                    if (dataModel != null)
                    {
                        DataModelModuleCollection dataModelModuleCollection = dataModel.Modules;
                        if (dataModelModuleCollection != null)
                        {
                            string[] dataModules = dataModelModuleCollection.Select(d => d.NameSpace).ToArray();
                            return dataModules;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        throw new FaultException(new FaultReason(Properties.Messages.ContextFaultReason));
                        //throw new FaultException<ObjectNotFoundException>(new ObjectNotFoundException(Properties.Messages.ZentityContext), new FaultReason(Properties.Messages.ContextFaultReason));
                    }
                }
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new FaultException(new FaultReason(exception.ToString()));
                //throw new FaultException<Exception>(exception, new FaultReason(Properties.Messages.ExceptionFaultReason));
            }
        }
    }
}
