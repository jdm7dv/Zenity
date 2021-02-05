// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Web.UI.Explorer.DataModelingServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="DataModelingServiceReference.IDataModelService", SessionMode=System.ServiceModel.SessionMode.NotAllowed)]
    public interface IDataModelService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDataModelService/CreateDataModel", ReplyAction="http://tempuri.org/IDataModelService/CreateDataModelResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(System.Exception), Action="http://tempuri.org/IDataModelService/CreateDataModelExceptionFault", Name="Exception", Namespace="http://schemas.datacontract.org/2004/07/System")]
        void CreateDataModel(string rdfsXml, string schemaXml, string modelNamespace);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDataModelService/DeleteDataModel", ReplyAction="http://tempuri.org/IDataModelService/DeleteDataModelResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(System.Exception), Action="http://tempuri.org/IDataModelService/DeleteDataModelExceptionFault", Name="Exception", Namespace="http://schemas.datacontract.org/2004/07/System")]
        void DeleteDataModel(string modelNamespace);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDataModelService/GenerateHierarchicalAssemblies", ReplyAction="http://tempuri.org/IDataModelService/GenerateHierarchicalAssembliesResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(System.Exception), Action="http://tempuri.org/IDataModelService/GenerateHierarchicalAssembliesExceptionFault" +
            "", Name="Exception", Namespace="http://schemas.datacontract.org/2004/07/System")]
        void GenerateHierarchicalAssemblies(string outputAssemblyName, System.Collections.Generic.List<string> metadataModelNamespaces, System.Collections.Generic.List<string> assemblyModelNamespaces, string storageLocation);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDataModelService/GenerateFlattenedAssemblies", ReplyAction="http://tempuri.org/IDataModelService/GenerateFlattenedAssembliesResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(System.Exception), Action="http://tempuri.org/IDataModelService/GenerateFlattenedAssembliesExceptionFault", Name="Exception", Namespace="http://schemas.datacontract.org/2004/07/System")]
        void GenerateFlattenedAssemblies(string outputAssemblyName, System.Collections.Generic.List<string> metadataModelNamespaces, System.Collections.Generic.List<string> assemblyModelNamespaces, string storageLocation);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDataModelService/GetAllDataModels", ReplyAction="http://tempuri.org/IDataModelService/GetAllDataModelsResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(System.Exception), Action="http://tempuri.org/IDataModelService/GetAllDataModelsExceptionFault", Name="Exception", Namespace="http://schemas.datacontract.org/2004/07/System")]
        System.Collections.Generic.List<string> GetAllDataModels();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDataModelService/GenerateHierarchicalMetadataAssembly", ReplyAction="http://tempuri.org/IDataModelService/GenerateHierarchicalMetadataAssemblyResponse" +
            "")]
        [System.ServiceModel.FaultContractAttribute(typeof(System.Exception), Action="http://tempuri.org/IDataModelService/GenerateHierarchicalMetadataAssemblyExceptio" +
            "nFault", Name="Exception", Namespace="http://schemas.datacontract.org/2004/07/System")]
        void GenerateHierarchicalMetadataAssembly(System.Collections.Generic.List<string> metadataModelNamespaces, string storageLocation);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDataModelService/GenerateFlattenedMetadataAssembly", ReplyAction="http://tempuri.org/IDataModelService/GenerateFlattenedMetadataAssemblyResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(System.Exception), Action="http://tempuri.org/IDataModelService/GenerateFlattenedMetadataAssemblyExceptionFa" +
            "ult", Name="Exception", Namespace="http://schemas.datacontract.org/2004/07/System")]
        void GenerateFlattenedMetadataAssembly(System.Collections.Generic.List<string> metadataModelNamespaces, string storageLocation);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IDataModelServiceChannel : Zentity.Web.UI.Explorer.DataModelingServiceReference.IDataModelService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class DataModelServiceClient : System.ServiceModel.ClientBase<Zentity.Web.UI.Explorer.DataModelingServiceReference.IDataModelService>, Zentity.Web.UI.Explorer.DataModelingServiceReference.IDataModelService {
        
        public DataModelServiceClient() {
        }
        
        public DataModelServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public DataModelServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DataModelServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DataModelServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public void CreateDataModel(string rdfsXml, string schemaXml, string modelNamespace) {
            base.Channel.CreateDataModel(rdfsXml, schemaXml, modelNamespace);
        }
        
        public void DeleteDataModel(string modelNamespace) {
            base.Channel.DeleteDataModel(modelNamespace);
        }
        
        public void GenerateHierarchicalAssemblies(string outputAssemblyName, System.Collections.Generic.List<string> metadataModelNamespaces, System.Collections.Generic.List<string> assemblyModelNamespaces, string storageLocation) {
            base.Channel.GenerateHierarchicalAssemblies(outputAssemblyName, metadataModelNamespaces, assemblyModelNamespaces, storageLocation);
        }
        
        public void GenerateFlattenedAssemblies(string outputAssemblyName, System.Collections.Generic.List<string> metadataModelNamespaces, System.Collections.Generic.List<string> assemblyModelNamespaces, string storageLocation) {
            base.Channel.GenerateFlattenedAssemblies(outputAssemblyName, metadataModelNamespaces, assemblyModelNamespaces, storageLocation);
        }
        
        public System.Collections.Generic.List<string> GetAllDataModels() {
            return base.Channel.GetAllDataModels();
        }
        
        public void GenerateHierarchicalMetadataAssembly(System.Collections.Generic.List<string> metadataModelNamespaces, string storageLocation) {
            base.Channel.GenerateHierarchicalMetadataAssembly(metadataModelNamespaces, storageLocation);
        }
        
        public void GenerateFlattenedMetadataAssembly(System.Collections.Generic.List<string> metadataModelNamespaces, string storageLocation) {
            base.Channel.GenerateFlattenedMetadataAssembly(metadataModelNamespaces, storageLocation);
        }
    }
}
