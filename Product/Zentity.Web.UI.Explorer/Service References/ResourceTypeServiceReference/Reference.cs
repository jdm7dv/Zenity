// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Web.UI.Explorer.ResourceTypeServiceReference {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name="ResourceTypeCollection", Namespace="http://schemas.datacontract.org/2004/07/Zentity.Core", ItemName="ResourceType")]
    [System.SerializableAttribute()]
    public class ResourceTypeCollection : System.Collections.Generic.List<Zentity.Core.ResourceType> {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name="NavigationPropertyCollection", Namespace="http://schemas.datacontract.org/2004/07/Zentity.Core", ItemName="NavigationProperty")]
    [System.SerializableAttribute()]
    public class NavigationPropertyCollection : System.Collections.Generic.List<Zentity.Core.NavigationProperty> {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name="ScalarPropertyCollection", Namespace="http://schemas.datacontract.org/2004/07/Zentity.Core", ItemName="ScalarProperty")]
    [System.SerializableAttribute()]
    public class ScalarPropertyCollection : System.Collections.Generic.List<Zentity.Core.ScalarProperty> {
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ResourceTypeServiceReference.IResourceTypeService", SessionMode=System.ServiceModel.SessionMode.NotAllowed)]
    public interface IResourceTypeService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IResourceTypeService/GetAllResourceTypesByNamespace", ReplyAction="http://tempuri.org/IResourceTypeService/GetAllResourceTypesByNamespaceResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(System.Exception), Action="http://tempuri.org/IResourceTypeService/GetAllResourceTypesByNamespaceExceptionFa" +
            "ult", Name="Exception", Namespace="http://schemas.datacontract.org/2004/07/System")]
        Zentity.Web.UI.Explorer.ResourceTypeServiceReference.ResourceTypeCollection GetAllResourceTypesByNamespace(string modelNameSpace);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IResourceTypeService/GetResourceCountForResourceType", ReplyAction="http://tempuri.org/IResourceTypeService/GetResourceCountForResourceTypeResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(System.Exception), Action="http://tempuri.org/IResourceTypeService/GetResourceCountForResourceTypeExceptionF" +
            "ault", Name="Exception", Namespace="http://schemas.datacontract.org/2004/07/System")]
        int GetResourceCountForResourceType(string modelNamespace, string resourceTypeName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IResourceTypeService/GetResourceCountForDataModel", ReplyAction="http://tempuri.org/IResourceTypeService/GetResourceCountForDataModelResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(System.Exception), Action="http://tempuri.org/IResourceTypeService/GetResourceCountForDataModelExceptionFaul" +
            "t", Name="Exception", Namespace="http://schemas.datacontract.org/2004/07/System")]
        System.Collections.Generic.Dictionary<string, int> GetResourceCountForDataModel(out int totalUniqueItemCount, System.Collections.Generic.List<string> dataModelNamesapceList);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IResourceTypeService/GetAllScalarPropertiesForResourceType", ReplyAction="http://tempuri.org/IResourceTypeService/GetAllScalarPropertiesForResourceTypeResp" +
            "onse")]
        [System.ServiceModel.FaultContractAttribute(typeof(System.Exception), Action="http://tempuri.org/IResourceTypeService/GetAllScalarPropertiesForResourceTypeExce" +
            "ptionFault", Name="Exception", Namespace="http://schemas.datacontract.org/2004/07/System")]
        Zentity.Web.UI.Explorer.ResourceTypeServiceReference.ScalarPropertyCollection GetAllScalarPropertiesForResourceType(string modelNamespace, string resourceType);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IResourceTypeService/AddScalarPropertyToResourceType", ReplyAction="http://tempuri.org/IResourceTypeService/AddScalarPropertyToResourceTypeResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(System.Exception), Action="http://tempuri.org/IResourceTypeService/AddScalarPropertyToResourceTypeExceptionF" +
            "ault", Name="Exception", Namespace="http://schemas.datacontract.org/2004/07/System")]
        void AddScalarPropertyToResourceType(string modelNamespace, string resourceType, string propertyName, Zentity.Core.DataTypes dataType, System.Nullable<int> maxLength, System.Nullable<int> precision, System.Nullable<int> scale);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IResourceTypeService/DeleteScalarPropertyOfResourceType", ReplyAction="http://tempuri.org/IResourceTypeService/DeleteScalarPropertyOfResourceTypeRespons" +
            "e")]
        [System.ServiceModel.FaultContractAttribute(typeof(System.Exception), Action="http://tempuri.org/IResourceTypeService/DeleteScalarPropertyOfResourceTypeExcepti" +
            "onFault", Name="Exception", Namespace="http://schemas.datacontract.org/2004/07/System")]
        void DeleteScalarPropertyOfResourceType(string modelNamespace, string resourceType, string propertyName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IResourceTypeService/GetAllNavigationPropertiesForResourceType" +
            "", ReplyAction="http://tempuri.org/IResourceTypeService/GetAllNavigationPropertiesForResourceType" +
            "Response")]
        [System.ServiceModel.FaultContractAttribute(typeof(System.Exception), Action="http://tempuri.org/IResourceTypeService/GetAllNavigationPropertiesForResourceType" +
            "ExceptionFault", Name="Exception", Namespace="http://schemas.datacontract.org/2004/07/System")]
        Zentity.Web.UI.Explorer.ResourceTypeServiceReference.NavigationPropertyCollection GetAllNavigationPropertiesForResourceType(string modelNamespace, string resourceType);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IResourceTypeService/AddNavigationPropertyToResourceType", ReplyAction="http://tempuri.org/IResourceTypeService/AddNavigationPropertyToResourceTypeRespon" +
            "se")]
        [System.ServiceModel.FaultContractAttribute(typeof(System.Exception), Action="http://tempuri.org/IResourceTypeService/AddNavigationPropertyToResourceTypeExcept" +
            "ionFault", Name="Exception", Namespace="http://schemas.datacontract.org/2004/07/System")]
        void AddNavigationPropertyToResourceType(string modelNamespace, string subjectResourceType, string objectResourceType, string subjectNavigationPropertyName, string objectNavigationPropertyName, string associationName, Zentity.Core.AssociationEndMultiplicity subjectMultiplicity, Zentity.Core.AssociationEndMultiplicity objectMultiplicity);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IResourceTypeService/DeleteNavigationPropertyOfResourceType", ReplyAction="http://tempuri.org/IResourceTypeService/DeleteNavigationPropertyOfResourceTypeRes" +
            "ponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(System.Exception), Action="http://tempuri.org/IResourceTypeService/DeleteNavigationPropertyOfResourceTypeExc" +
            "eptionFault", Name="Exception", Namespace="http://schemas.datacontract.org/2004/07/System")]
        void DeleteNavigationPropertyOfResourceType(string modelNamespace, string resourceType, string propertyName);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IResourceTypeServiceChannel : Zentity.Web.UI.Explorer.ResourceTypeServiceReference.IResourceTypeService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ResourceTypeServiceClient : System.ServiceModel.ClientBase<Zentity.Web.UI.Explorer.ResourceTypeServiceReference.IResourceTypeService>, Zentity.Web.UI.Explorer.ResourceTypeServiceReference.IResourceTypeService {
        
        public ResourceTypeServiceClient() {
        }
        
        public ResourceTypeServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ResourceTypeServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ResourceTypeServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ResourceTypeServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public Zentity.Web.UI.Explorer.ResourceTypeServiceReference.ResourceTypeCollection GetAllResourceTypesByNamespace(string modelNameSpace) {
            return base.Channel.GetAllResourceTypesByNamespace(modelNameSpace);
        }
        
        public int GetResourceCountForResourceType(string modelNamespace, string resourceTypeName) {
            return base.Channel.GetResourceCountForResourceType(modelNamespace, resourceTypeName);
        }
        
        public System.Collections.Generic.Dictionary<string, int> GetResourceCountForDataModel(out int totalUniqueItemCount, System.Collections.Generic.List<string> dataModelNamesapceList) {
            return base.Channel.GetResourceCountForDataModel(out totalUniqueItemCount, dataModelNamesapceList);
        }
        
        public Zentity.Web.UI.Explorer.ResourceTypeServiceReference.ScalarPropertyCollection GetAllScalarPropertiesForResourceType(string modelNamespace, string resourceType) {
            return base.Channel.GetAllScalarPropertiesForResourceType(modelNamespace, resourceType);
        }
        
        public void AddScalarPropertyToResourceType(string modelNamespace, string resourceType, string propertyName, Zentity.Core.DataTypes dataType, System.Nullable<int> maxLength, System.Nullable<int> precision, System.Nullable<int> scale) {
            base.Channel.AddScalarPropertyToResourceType(modelNamespace, resourceType, propertyName, dataType, maxLength, precision, scale);
        }
        
        public void DeleteScalarPropertyOfResourceType(string modelNamespace, string resourceType, string propertyName) {
            base.Channel.DeleteScalarPropertyOfResourceType(modelNamespace, resourceType, propertyName);
        }
        
        public Zentity.Web.UI.Explorer.ResourceTypeServiceReference.NavigationPropertyCollection GetAllNavigationPropertiesForResourceType(string modelNamespace, string resourceType) {
            return base.Channel.GetAllNavigationPropertiesForResourceType(modelNamespace, resourceType);
        }
        
        public void AddNavigationPropertyToResourceType(string modelNamespace, string subjectResourceType, string objectResourceType, string subjectNavigationPropertyName, string objectNavigationPropertyName, string associationName, Zentity.Core.AssociationEndMultiplicity subjectMultiplicity, Zentity.Core.AssociationEndMultiplicity objectMultiplicity) {
            base.Channel.AddNavigationPropertyToResourceType(modelNamespace, subjectResourceType, objectResourceType, subjectNavigationPropertyName, objectNavigationPropertyName, associationName, subjectMultiplicity, objectMultiplicity);
        }
        
        public void DeleteNavigationPropertyOfResourceType(string modelNamespace, string resourceType, string propertyName) {
            base.Channel.DeleteNavigationPropertyOfResourceType(modelNamespace, resourceType, propertyName);
        }
    }
}
