// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.CollectionCreator.PublishingProgressService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="PublishingProgressService.IPublishingProgressService", CallbackContract=typeof(Zentity.CollectionCreator.PublishingProgressService.IPublishingProgressServiceCallback))]
    public interface IPublishingProgressService {
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IPublishingProgressService/ReportStart")]
        void ReportStart(System.Guid instanceId, Zentity.Services.Web.CreatorApplication appType, int processId);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IPublishingProgressService/ReportProgress")]
        void ReportProgress(System.Guid instanceId, Zentity.Services.Web.PublishStage publishStage, Zentity.Services.Web.ProgressCounter progressCounter);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IPublishingProgressServiceCallback {
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IPublishingProgressService/CancelOperation")]
        void CancelOperation();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IPublishingProgressServiceChannel : Zentity.CollectionCreator.PublishingProgressService.IPublishingProgressService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class PublishingProgressServiceClient : System.ServiceModel.DuplexClientBase<Zentity.CollectionCreator.PublishingProgressService.IPublishingProgressService>, Zentity.CollectionCreator.PublishingProgressService.IPublishingProgressService {
        
        public PublishingProgressServiceClient(System.ServiceModel.InstanceContext callbackInstance) : 
                base(callbackInstance) {
        }
        
        public PublishingProgressServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : 
                base(callbackInstance, endpointConfigurationName) {
        }
        
        public PublishingProgressServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public PublishingProgressServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public PublishingProgressServiceClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, binding, remoteAddress) {
        }
        
        public void ReportStart(System.Guid instanceId, Zentity.Services.Web.CreatorApplication appType, int processId) {
            base.Channel.ReportStart(instanceId, appType, processId);
        }
        
        public void ReportProgress(System.Guid instanceId, Zentity.Services.Web.PublishStage publishStage, Zentity.Services.Web.ProgressCounter progressCounter) {
            base.Channel.ReportProgress(instanceId, publishStage, progressCounter);
        }
    }
}
