// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Windows.Pivot {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ResourceChangeMessage", Namespace="http://schemas.datacontract.org/2004/07/Zentity.Services.Web.Pivot")]
    [System.SerializableAttribute()]
    public partial class ResourceChangeMessage : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private Zentity.Services.Windows.Pivot.ResourceChangeType ChangeTypeField;
        
        private string DataModelNamespaceField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Nullable<System.DateTime> DateAddedField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Nullable<System.DateTime> DateModifiedField;
        
        private System.Guid ResourceIdField;
        
        private System.Guid ResourceTypeIdField;
        
        private string ResourceTypeNameField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public Zentity.Services.Windows.Pivot.ResourceChangeType ChangeType {
            get {
                return this.ChangeTypeField;
            }
            set {
                if ((this.ChangeTypeField.Equals(value) != true)) {
                    this.ChangeTypeField = value;
                    this.RaisePropertyChanged("ChangeType");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public string DataModelNamespace {
            get {
                return this.DataModelNamespaceField;
            }
            set {
                if ((object.ReferenceEquals(this.DataModelNamespaceField, value) != true)) {
                    this.DataModelNamespaceField = value;
                    this.RaisePropertyChanged("DataModelNamespace");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<System.DateTime> DateAdded {
            get {
                return this.DateAddedField;
            }
            set {
                if ((this.DateAddedField.Equals(value) != true)) {
                    this.DateAddedField = value;
                    this.RaisePropertyChanged("DateAdded");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<System.DateTime> DateModified {
            get {
                return this.DateModifiedField;
            }
            set {
                if ((this.DateModifiedField.Equals(value) != true)) {
                    this.DateModifiedField = value;
                    this.RaisePropertyChanged("DateModified");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public System.Guid ResourceId {
            get {
                return this.ResourceIdField;
            }
            set {
                if ((this.ResourceIdField.Equals(value) != true)) {
                    this.ResourceIdField = value;
                    this.RaisePropertyChanged("ResourceId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public System.Guid ResourceTypeId {
            get {
                return this.ResourceTypeIdField;
            }
            set {
                if ((this.ResourceTypeIdField.Equals(value) != true)) {
                    this.ResourceTypeIdField = value;
                    this.RaisePropertyChanged("ResourceTypeId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public string ResourceTypeName {
            get {
                return this.ResourceTypeNameField;
            }
            set {
                if ((object.ReferenceEquals(this.ResourceTypeNameField, value) != true)) {
                    this.ResourceTypeNameField = value;
                    this.RaisePropertyChanged("ResourceTypeName");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ResourceChangeType", Namespace="http://schemas.datacontract.org/2004/07/Zentity.Services.Web.Pivot")]
    public enum ResourceChangeType : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Added = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Updated = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Deleted = 2,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PublishStatus", Namespace="http://schemas.datacontract.org/2004/07/Zentity.Services.Web")]
    [System.SerializableAttribute()]
    public partial class PublishStatus : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private Zentity.Services.Windows.Pivot.PublishStage CurrentStageField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string DataModelNamespaceField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private Zentity.Services.Windows.Pivot.ProgressCounter DeepZoomImagesField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.DateTime EndTimeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private Zentity.Services.Windows.Pivot.ProgressCounter ImagesField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Guid InstanceIdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private Zentity.Services.Windows.Pivot.ProgressCounter ResourceItemsField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ResourceTypeNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Collections.Generic.Dictionary<Zentity.Services.Windows.Pivot.PublishStage, System.DateTime> StageStartTimeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.DateTime StartTimeField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public Zentity.Services.Windows.Pivot.PublishStage CurrentStage {
            get {
                return this.CurrentStageField;
            }
            set {
                if ((this.CurrentStageField.Equals(value) != true)) {
                    this.CurrentStageField = value;
                    this.RaisePropertyChanged("CurrentStage");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string DataModelNamespace {
            get {
                return this.DataModelNamespaceField;
            }
            set {
                if ((object.ReferenceEquals(this.DataModelNamespaceField, value) != true)) {
                    this.DataModelNamespaceField = value;
                    this.RaisePropertyChanged("DataModelNamespace");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public Zentity.Services.Windows.Pivot.ProgressCounter DeepZoomImages {
            get {
                return this.DeepZoomImagesField;
            }
            set {
                if ((this.DeepZoomImagesField.Equals(value) != true)) {
                    this.DeepZoomImagesField = value;
                    this.RaisePropertyChanged("DeepZoomImages");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.DateTime EndTime {
            get {
                return this.EndTimeField;
            }
            set {
                if ((this.EndTimeField.Equals(value) != true)) {
                    this.EndTimeField = value;
                    this.RaisePropertyChanged("EndTime");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public Zentity.Services.Windows.Pivot.ProgressCounter Images {
            get {
                return this.ImagesField;
            }
            set {
                if ((this.ImagesField.Equals(value) != true)) {
                    this.ImagesField = value;
                    this.RaisePropertyChanged("Images");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Guid InstanceId {
            get {
                return this.InstanceIdField;
            }
            set {
                if ((this.InstanceIdField.Equals(value) != true)) {
                    this.InstanceIdField = value;
                    this.RaisePropertyChanged("InstanceId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public Zentity.Services.Windows.Pivot.ProgressCounter ResourceItems {
            get {
                return this.ResourceItemsField;
            }
            set {
                if ((this.ResourceItemsField.Equals(value) != true)) {
                    this.ResourceItemsField = value;
                    this.RaisePropertyChanged("ResourceItems");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ResourceTypeName {
            get {
                return this.ResourceTypeNameField;
            }
            set {
                if ((object.ReferenceEquals(this.ResourceTypeNameField, value) != true)) {
                    this.ResourceTypeNameField = value;
                    this.RaisePropertyChanged("ResourceTypeName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Collections.Generic.Dictionary<Zentity.Services.Windows.Pivot.PublishStage, System.DateTime> StageStartTime {
            get {
                return this.StageStartTimeField;
            }
            set {
                if ((object.ReferenceEquals(this.StageStartTimeField, value) != true)) {
                    this.StageStartTimeField = value;
                    this.RaisePropertyChanged("StageStartTime");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.DateTime StartTime {
            get {
                return this.StartTimeField;
            }
            set {
                if ((this.StartTimeField.Equals(value) != true)) {
                    this.StartTimeField = value;
                    this.RaisePropertyChanged("StartTime");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ProgressCounter", Namespace="http://schemas.datacontract.org/2004/07/Zentity.Services.Web")]
    [System.SerializableAttribute()]
    public partial struct ProgressCounter : System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int CompletedField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int TotalField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Completed {
            get {
                return this.CompletedField;
            }
            set {
                if ((this.CompletedField.Equals(value) != true)) {
                    this.CompletedField = value;
                    this.RaisePropertyChanged("Completed");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Total {
            get {
                return this.TotalField;
            }
            set {
                if ((this.TotalField.Equals(value) != true)) {
                    this.TotalField = value;
                    this.RaisePropertyChanged("Total");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PublishStage", Namespace="http://schemas.datacontract.org/2004/07/Zentity.Services.Web")]
    public enum PublishStage : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        NotStarted = -1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Initiating = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        FetchingResourceItems = 10,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        ProcessingResourceItems = 15,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        PublishIntermediateCollection = 17,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        CreatingImages = 20,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        CreatingDeepZoomImages = 25,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        CreatingDeepZoomCollection = 26,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        DeletingExistingCollection = 30,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        CopyingNewCollection = 35,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        CopyingExistingCollection = 11,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        PerformingCleanup = 50,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Completed = 100,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        AbortedOnError = 105,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        AbortedOnDemand = 110,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="Pivot.IPublishingService", SessionMode=System.ServiceModel.SessionMode.NotAllowed)]
    public interface IPublishingService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPublishingService/CreateCollection", ReplyAction="http://tempuri.org/IPublishingService/CreateCollectionResponse")]
        System.Guid CreateCollection(string modelNamespace, string resourceType);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPublishingService/UpdateCollection", ReplyAction="http://tempuri.org/IPublishingService/UpdateCollectionResponse")]
        System.Guid UpdateCollection(string resourceChangeMessageFilePath, string modelNamespace, string resourceType, Zentity.Services.Windows.Pivot.ResourceChangeMessage changeMessage);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IPublishingService/DeletePublishedCollection")]
        void DeletePublishedCollection(string modelNamespace, string resourceType);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPublishingService/CancelPublishRequestByInstanceID", ReplyAction="http://tempuri.org/IPublishingService/CancelPublishRequestByInstanceIDResponse")]
        bool CancelPublishRequestByInstanceID(System.Guid instanceId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPublishingService/CancelPublishRequestByResourceType", ReplyAction="http://tempuri.org/IPublishingService/CancelPublishRequestByResourceTypeResponse")]
        bool CancelPublishRequestByResourceType(string modelNamespace, string resourceType);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPublishingService/GetAllPublishingStatus", ReplyAction="http://tempuri.org/IPublishingService/GetAllPublishingStatusResponse")]
        System.Collections.Generic.List<Zentity.Services.Windows.Pivot.PublishStatus> GetAllPublishingStatus();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPublishingService/GetPublishingStatusByInstanceID", ReplyAction="http://tempuri.org/IPublishingService/GetPublishingStatusByInstanceIDResponse")]
        Zentity.Services.Windows.Pivot.PublishStatus GetPublishingStatusByInstanceID(System.Guid instanceId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPublishingService/GetPublishingStatusByResourceType", ReplyAction="http://tempuri.org/IPublishingService/GetPublishingStatusByResourceTypeResponse")]
        Zentity.Services.Windows.Pivot.PublishStatus GetPublishingStatusByResourceType(string modelNamespace, string resourceType);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPublishingService/GetAllQueuedRequests", ReplyAction="http://tempuri.org/IPublishingService/GetAllQueuedRequestsResponse")]
        System.Collections.Generic.List<Zentity.Services.Windows.Pivot.PublishStatus> GetAllQueuedRequests();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPublishingService/GetQueuedRequestByInstanceID", ReplyAction="http://tempuri.org/IPublishingService/GetQueuedRequestByInstanceIDResponse")]
        Zentity.Services.Windows.Pivot.PublishStatus GetQueuedRequestByInstanceID(System.Guid instanceId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPublishingService/GetQueuedRequestsByResourceType", ReplyAction="http://tempuri.org/IPublishingService/GetQueuedRequestsByResourceTypeResponse")]
        System.Collections.Generic.List<Zentity.Services.Windows.Pivot.PublishStatus> GetQueuedRequestsByResourceType(string modelNamespace, string resourceType);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPublishingService/GetQueuedPositionByInstanceID", ReplyAction="http://tempuri.org/IPublishingService/GetQueuedPositionByInstanceIDResponse")]
        int GetQueuedPositionByInstanceID(System.Guid instanceId);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IPublishingServiceChannel : Zentity.Services.Windows.Pivot.IPublishingService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class PublishingServiceClient : System.ServiceModel.ClientBase<Zentity.Services.Windows.Pivot.IPublishingService>, Zentity.Services.Windows.Pivot.IPublishingService {
        
        public PublishingServiceClient() {
        }
        
        public PublishingServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public PublishingServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public PublishingServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public PublishingServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.Guid CreateCollection(string modelNamespace, string resourceType) {
            return base.Channel.CreateCollection(modelNamespace, resourceType);
        }
        
        public System.Guid UpdateCollection(string resourceChangeMessageFilePath, string modelNamespace, string resourceType, Zentity.Services.Windows.Pivot.ResourceChangeMessage changeMessage) {
            return base.Channel.UpdateCollection(resourceChangeMessageFilePath, modelNamespace, resourceType, changeMessage);
        }
        
        public void DeletePublishedCollection(string modelNamespace, string resourceType) {
            base.Channel.DeletePublishedCollection(modelNamespace, resourceType);
        }
        
        public bool CancelPublishRequestByInstanceID(System.Guid instanceId) {
            return base.Channel.CancelPublishRequestByInstanceID(instanceId);
        }
        
        public bool CancelPublishRequestByResourceType(string modelNamespace, string resourceType) {
            return base.Channel.CancelPublishRequestByResourceType(modelNamespace, resourceType);
        }
        
        public System.Collections.Generic.List<Zentity.Services.Windows.Pivot.PublishStatus> GetAllPublishingStatus() {
            return base.Channel.GetAllPublishingStatus();
        }
        
        public Zentity.Services.Windows.Pivot.PublishStatus GetPublishingStatusByInstanceID(System.Guid instanceId) {
            return base.Channel.GetPublishingStatusByInstanceID(instanceId);
        }
        
        public Zentity.Services.Windows.Pivot.PublishStatus GetPublishingStatusByResourceType(string modelNamespace, string resourceType) {
            return base.Channel.GetPublishingStatusByResourceType(modelNamespace, resourceType);
        }
        
        public System.Collections.Generic.List<Zentity.Services.Windows.Pivot.PublishStatus> GetAllQueuedRequests() {
            return base.Channel.GetAllQueuedRequests();
        }
        
        public Zentity.Services.Windows.Pivot.PublishStatus GetQueuedRequestByInstanceID(System.Guid instanceId) {
            return base.Channel.GetQueuedRequestByInstanceID(instanceId);
        }
        
        public System.Collections.Generic.List<Zentity.Services.Windows.Pivot.PublishStatus> GetQueuedRequestsByResourceType(string modelNamespace, string resourceType) {
            return base.Channel.GetQueuedRequestsByResourceType(modelNamespace, resourceType);
        }
        
        public int GetQueuedPositionByInstanceID(System.Guid instanceId) {
            return base.Channel.GetQueuedPositionByInstanceID(instanceId);
        }
    }
}
