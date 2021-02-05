// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Web.Data
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    /// <summary>
    /// IDataModelService
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.NotAllowed)]
    public interface IDataModelService
    {
        /// <summary>
        /// Will create a new data model if the namespace is not present.
        /// </summary>
        /// <param name="rdfsXml">RDFS Xml document string</param>
        /// <param name="schemaXml">Schema Xml document string</param>
        /// <param name="modelNamespace">Namespace of the model</param>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        void CreateDataModel(string rdfsXml, string schemaXml, string modelNamespace);

        /// <summary>
        /// Will delete the existing data model and related resources within the Zentity store with the  specified namespace.
        /// </summary>
        /// <param name="modelNamespace">Namespace of the model</param>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        void DeleteDataModel(string modelNamespace);

        /// <summary>
        /// This will stream the raw data of the assembly in a Hierarchical pattern for the particular data model with the specified namespace.
        /// </summary>
        /// <param name="outputAssemblyName">Namespace of the model</param>
        /// <param name="metadataModelNamespaces">List of Metadata Namespaces</param>
        /// <param name="assemblyModelNamespaces">List of Model Namespaces</param>
        /// <param name="storageLocation">Storage Location of the generated assemblies.</param>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        void GenerateHierarchicalAssemblies(string outputAssemblyName, string[] metadataModelNamespaces, string[] assemblyModelNamespaces, string storageLocation);

        /// <summary>
        /// This will stream the raw data of the assembly in a Flattened pattern for the particular data model with the specified namespace.
        /// </summary>
        /// <param name="outputAssemblyName">Namespace of the model</param>
        /// <param name="metadataModelNamespaces">List of Metadata Namespaces</param>
        /// <param name="assemblyModelNamespaces">List of Model Namespaces</param>
        /// <param name="storageLocation">Storage Location of the generated assemblies.</param>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        void GenerateFlattenedAssemblies(string outputAssemblyName, string[] metadataModelNamespaces, string[] assemblyModelNamespaces, string storageLocation);

        /// <summary>
        /// Gets all the Data models in the DB
        /// </summary>
        /// <returns>Collection of Data Models</returns>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        string[] GetAllDataModels();

        /// <summary>
        /// This will stream the raw data of the metadata assembly in a Hierarchical pattern.
        /// </summary>
        /// <param name="metadataModelNamespaces">List of Metadata Namespaces</param>
        /// <param name="storageLocation">Storage Location of the generated assemblies.</param>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        void GenerateHierarchicalMetadataAssembly(string[] metadataModelNamespaces, string storageLocation);

        /// <summary>
        /// This will stream the raw data of the assembly in a Flattened pattern.
        /// </summary>
        /// <param name="metadataModelNamespaces">List of Metadata Namespaces</param>
        /// <param name="storageLocation">Storage Location of the generated assemblies.</param>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        void GenerateFlattenedMetadataAssembly(string[] metadataModelNamespaces, string storageLocation);
    }
}
