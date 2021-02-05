// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.VisualExplorer.VisualExplorerService {
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="VisualExplorerGraph", Namespace="http://schemas.datacontract.org/2004/07/Zentity.Services.Explorer")]
    public partial class VisualExplorerGraph : object, System.ComponentModel.INotifyPropertyChanged {
        
        private GuanxiUI.JSONGraph JSONGraphField;
        
        private System.Collections.Generic.Dictionary<System.Guid, string> ResourceMapField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public GuanxiUI.JSONGraph JSONGraph {
            get {
                return this.JSONGraphField;
            }
            set {
                if ((object.ReferenceEquals(this.JSONGraphField, value) != true)) {
                    this.JSONGraphField = value;
                    this.RaisePropertyChanged("JSONGraph");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Collections.Generic.Dictionary<System.Guid, string> ResourceMap {
            get {
                return this.ResourceMapField;
            }
            set {
                if ((object.ReferenceEquals(this.ResourceMapField, value) != true)) {
                    this.ResourceMapField = value;
                    this.RaisePropertyChanged("ResourceMap");
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
    [System.Runtime.Serialization.DataContractAttribute(Name="VisualExplorerFilterList", Namespace="http://schemas.datacontract.org/2004/07/Zentity.Services.Explorer")]
    public partial class VisualExplorerFilterList : object, System.ComponentModel.INotifyPropertyChanged {
        
        private System.Collections.ObjectModel.ObservableCollection<string> RelationShipTypesField;
        
        private System.Collections.ObjectModel.ObservableCollection<string> ResourceTypesField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Collections.ObjectModel.ObservableCollection<string> RelationShipTypes {
            get {
                return this.RelationShipTypesField;
            }
            set {
                if ((object.ReferenceEquals(this.RelationShipTypesField, value) != true)) {
                    this.RelationShipTypesField = value;
                    this.RaisePropertyChanged("RelationShipTypes");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Collections.ObjectModel.ObservableCollection<string> ResourceTypes {
            get {
                return this.ResourceTypesField;
            }
            set {
                if ((object.ReferenceEquals(this.ResourceTypesField, value) != true)) {
                    this.ResourceTypesField = value;
                    this.RaisePropertyChanged("ResourceTypes");
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
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="VisualExplorerService.IVisualExplorerService")]
    public interface IVisualExplorerService {
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/IVisualExplorerService/GetVisualExplorerGraphBySearchKeyword", ReplyAction="http://tempuri.org/IVisualExplorerService/GetVisualExplorerGraphBySearchKeywordRe" +
            "sponse")]
        System.IAsyncResult BeginGetVisualExplorerGraphBySearchKeyword(string keyword, System.AsyncCallback callback, object asyncState);
        
        Zentity.VisualExplorer.VisualExplorerService.VisualExplorerGraph EndGetVisualExplorerGraphBySearchKeyword(System.IAsyncResult result);
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/IVisualExplorerService/GetVisualExplorerGraphByResourceId", ReplyAction="http://tempuri.org/IVisualExplorerService/GetVisualExplorerGraphByResourceIdRespo" +
            "nse")]
        System.IAsyncResult BeginGetVisualExplorerGraphByResourceId(string resourceId, System.AsyncCallback callback, object asyncState);
        
        Zentity.VisualExplorer.VisualExplorerService.VisualExplorerGraph EndGetVisualExplorerGraphByResourceId(System.IAsyncResult result);
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/IVisualExplorerService/GetResourceMetadataByResourceId", ReplyAction="http://tempuri.org/IVisualExplorerService/GetResourceMetadataByResourceIdResponse" +
            "")]
        System.IAsyncResult BeginGetResourceMetadataByResourceId(string resourceId, System.AsyncCallback callback, object asyncState);
        
        string EndGetResourceMetadataByResourceId(System.IAsyncResult result);
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/IVisualExplorerService/GetResourceRelationByResourceId", ReplyAction="http://tempuri.org/IVisualExplorerService/GetResourceRelationByResourceIdResponse" +
            "")]
        System.IAsyncResult BeginGetResourceRelationByResourceId(string subjectResourceId, string objectResourceId, System.AsyncCallback callback, object asyncState);
        
        string EndGetResourceRelationByResourceId(System.IAsyncResult result);
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/IVisualExplorerService/GetResourcesByKeyword", ReplyAction="http://tempuri.org/IVisualExplorerService/GetResourcesByKeywordResponse")]
        System.IAsyncResult BeginGetResourcesByKeyword(string keyword, System.AsyncCallback callback, object asyncState);
        
        string EndGetResourcesByKeyword(System.IAsyncResult result);
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/IVisualExplorerService/GetVisualExplorerFilterList", ReplyAction="http://tempuri.org/IVisualExplorerService/GetVisualExplorerFilterListResponse")]
        System.IAsyncResult BeginGetVisualExplorerFilterList(System.AsyncCallback callback, object asyncState);
        
        Zentity.VisualExplorer.VisualExplorerService.VisualExplorerFilterList EndGetVisualExplorerFilterList(System.IAsyncResult result);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IVisualExplorerServiceChannel : Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GetVisualExplorerGraphBySearchKeywordCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public GetVisualExplorerGraphBySearchKeywordCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public Zentity.VisualExplorer.VisualExplorerService.VisualExplorerGraph Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((Zentity.VisualExplorer.VisualExplorerService.VisualExplorerGraph)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GetVisualExplorerGraphByResourceIdCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public GetVisualExplorerGraphByResourceIdCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public Zentity.VisualExplorer.VisualExplorerService.VisualExplorerGraph Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((Zentity.VisualExplorer.VisualExplorerService.VisualExplorerGraph)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GetResourceMetadataByResourceIdCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public GetResourceMetadataByResourceIdCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public string Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GetResourceRelationByResourceIdCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public GetResourceRelationByResourceIdCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public string Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GetResourcesByKeywordCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public GetResourcesByKeywordCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public string Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GetVisualExplorerFilterListCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public GetVisualExplorerFilterListCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public Zentity.VisualExplorer.VisualExplorerService.VisualExplorerFilterList Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((Zentity.VisualExplorer.VisualExplorerService.VisualExplorerFilterList)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class VisualExplorerServiceClient : System.ServiceModel.ClientBase<Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService>, Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService {
        
        private BeginOperationDelegate onBeginGetVisualExplorerGraphBySearchKeywordDelegate;
        
        private EndOperationDelegate onEndGetVisualExplorerGraphBySearchKeywordDelegate;
        
        private System.Threading.SendOrPostCallback onGetVisualExplorerGraphBySearchKeywordCompletedDelegate;
        
        private BeginOperationDelegate onBeginGetVisualExplorerGraphByResourceIdDelegate;
        
        private EndOperationDelegate onEndGetVisualExplorerGraphByResourceIdDelegate;
        
        private System.Threading.SendOrPostCallback onGetVisualExplorerGraphByResourceIdCompletedDelegate;
        
        private BeginOperationDelegate onBeginGetResourceMetadataByResourceIdDelegate;
        
        private EndOperationDelegate onEndGetResourceMetadataByResourceIdDelegate;
        
        private System.Threading.SendOrPostCallback onGetResourceMetadataByResourceIdCompletedDelegate;
        
        private BeginOperationDelegate onBeginGetResourceRelationByResourceIdDelegate;
        
        private EndOperationDelegate onEndGetResourceRelationByResourceIdDelegate;
        
        private System.Threading.SendOrPostCallback onGetResourceRelationByResourceIdCompletedDelegate;
        
        private BeginOperationDelegate onBeginGetResourcesByKeywordDelegate;
        
        private EndOperationDelegate onEndGetResourcesByKeywordDelegate;
        
        private System.Threading.SendOrPostCallback onGetResourcesByKeywordCompletedDelegate;
        
        private BeginOperationDelegate onBeginGetVisualExplorerFilterListDelegate;
        
        private EndOperationDelegate onEndGetVisualExplorerFilterListDelegate;
        
        private System.Threading.SendOrPostCallback onGetVisualExplorerFilterListCompletedDelegate;
        
        private BeginOperationDelegate onBeginOpenDelegate;
        
        private EndOperationDelegate onEndOpenDelegate;
        
        private System.Threading.SendOrPostCallback onOpenCompletedDelegate;
        
        private BeginOperationDelegate onBeginCloseDelegate;
        
        private EndOperationDelegate onEndCloseDelegate;
        
        private System.Threading.SendOrPostCallback onCloseCompletedDelegate;
        
        public VisualExplorerServiceClient() {
        }
        
        public VisualExplorerServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public VisualExplorerServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public VisualExplorerServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public VisualExplorerServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.Net.CookieContainer CookieContainer {
            get {
                System.ServiceModel.Channels.IHttpCookieContainerManager httpCookieContainerManager = this.InnerChannel.GetProperty<System.ServiceModel.Channels.IHttpCookieContainerManager>();
                if ((httpCookieContainerManager != null)) {
                    return httpCookieContainerManager.CookieContainer;
                }
                else {
                    return null;
                }
            }
            set {
                System.ServiceModel.Channels.IHttpCookieContainerManager httpCookieContainerManager = this.InnerChannel.GetProperty<System.ServiceModel.Channels.IHttpCookieContainerManager>();
                if ((httpCookieContainerManager != null)) {
                    httpCookieContainerManager.CookieContainer = value;
                }
                else {
                    throw new System.InvalidOperationException("Unable to set the CookieContainer. Please make sure the binding contains an HttpC" +
                            "ookieContainerBindingElement.");
                }
            }
        }
        
        public event System.EventHandler<GetVisualExplorerGraphBySearchKeywordCompletedEventArgs> GetVisualExplorerGraphBySearchKeywordCompleted;
        
        public event System.EventHandler<GetVisualExplorerGraphByResourceIdCompletedEventArgs> GetVisualExplorerGraphByResourceIdCompleted;
        
        public event System.EventHandler<GetResourceMetadataByResourceIdCompletedEventArgs> GetResourceMetadataByResourceIdCompleted;
        
        public event System.EventHandler<GetResourceRelationByResourceIdCompletedEventArgs> GetResourceRelationByResourceIdCompleted;
        
        public event System.EventHandler<GetResourcesByKeywordCompletedEventArgs> GetResourcesByKeywordCompleted;
        
        public event System.EventHandler<GetVisualExplorerFilterListCompletedEventArgs> GetVisualExplorerFilterListCompleted;
        
        public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> OpenCompleted;
        
        public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> CloseCompleted;
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.IAsyncResult Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService.BeginGetVisualExplorerGraphBySearchKeyword(string keyword, System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginGetVisualExplorerGraphBySearchKeyword(keyword, callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        Zentity.VisualExplorer.VisualExplorerService.VisualExplorerGraph Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService.EndGetVisualExplorerGraphBySearchKeyword(System.IAsyncResult result) {
            return base.Channel.EndGetVisualExplorerGraphBySearchKeyword(result);
        }
        
        private System.IAsyncResult OnBeginGetVisualExplorerGraphBySearchKeyword(object[] inValues, System.AsyncCallback callback, object asyncState) {
            string keyword = ((string)(inValues[0]));
            return ((Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService)(this)).BeginGetVisualExplorerGraphBySearchKeyword(keyword, callback, asyncState);
        }
        
        private object[] OnEndGetVisualExplorerGraphBySearchKeyword(System.IAsyncResult result) {
            Zentity.VisualExplorer.VisualExplorerService.VisualExplorerGraph retVal = ((Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService)(this)).EndGetVisualExplorerGraphBySearchKeyword(result);
            return new object[] {
                    retVal};
        }
        
        private void OnGetVisualExplorerGraphBySearchKeywordCompleted(object state) {
            if ((this.GetVisualExplorerGraphBySearchKeywordCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.GetVisualExplorerGraphBySearchKeywordCompleted(this, new GetVisualExplorerGraphBySearchKeywordCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void GetVisualExplorerGraphBySearchKeywordAsync(string keyword) {
            this.GetVisualExplorerGraphBySearchKeywordAsync(keyword, null);
        }
        
        public void GetVisualExplorerGraphBySearchKeywordAsync(string keyword, object userState) {
            if ((this.onBeginGetVisualExplorerGraphBySearchKeywordDelegate == null)) {
                this.onBeginGetVisualExplorerGraphBySearchKeywordDelegate = new BeginOperationDelegate(this.OnBeginGetVisualExplorerGraphBySearchKeyword);
            }
            if ((this.onEndGetVisualExplorerGraphBySearchKeywordDelegate == null)) {
                this.onEndGetVisualExplorerGraphBySearchKeywordDelegate = new EndOperationDelegate(this.OnEndGetVisualExplorerGraphBySearchKeyword);
            }
            if ((this.onGetVisualExplorerGraphBySearchKeywordCompletedDelegate == null)) {
                this.onGetVisualExplorerGraphBySearchKeywordCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetVisualExplorerGraphBySearchKeywordCompleted);
            }
            base.InvokeAsync(this.onBeginGetVisualExplorerGraphBySearchKeywordDelegate, new object[] {
                        keyword}, this.onEndGetVisualExplorerGraphBySearchKeywordDelegate, this.onGetVisualExplorerGraphBySearchKeywordCompletedDelegate, userState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.IAsyncResult Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService.BeginGetVisualExplorerGraphByResourceId(string resourceId, System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginGetVisualExplorerGraphByResourceId(resourceId, callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        Zentity.VisualExplorer.VisualExplorerService.VisualExplorerGraph Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService.EndGetVisualExplorerGraphByResourceId(System.IAsyncResult result) {
            return base.Channel.EndGetVisualExplorerGraphByResourceId(result);
        }
        
        private System.IAsyncResult OnBeginGetVisualExplorerGraphByResourceId(object[] inValues, System.AsyncCallback callback, object asyncState) {
            string resourceId = ((string)(inValues[0]));
            return ((Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService)(this)).BeginGetVisualExplorerGraphByResourceId(resourceId, callback, asyncState);
        }
        
        private object[] OnEndGetVisualExplorerGraphByResourceId(System.IAsyncResult result) {
            Zentity.VisualExplorer.VisualExplorerService.VisualExplorerGraph retVal = ((Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService)(this)).EndGetVisualExplorerGraphByResourceId(result);
            return new object[] {
                    retVal};
        }
        
        private void OnGetVisualExplorerGraphByResourceIdCompleted(object state) {
            if ((this.GetVisualExplorerGraphByResourceIdCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.GetVisualExplorerGraphByResourceIdCompleted(this, new GetVisualExplorerGraphByResourceIdCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void GetVisualExplorerGraphByResourceIdAsync(string resourceId) {
            this.GetVisualExplorerGraphByResourceIdAsync(resourceId, null);
        }
        
        public void GetVisualExplorerGraphByResourceIdAsync(string resourceId, object userState) {
            if ((this.onBeginGetVisualExplorerGraphByResourceIdDelegate == null)) {
                this.onBeginGetVisualExplorerGraphByResourceIdDelegate = new BeginOperationDelegate(this.OnBeginGetVisualExplorerGraphByResourceId);
            }
            if ((this.onEndGetVisualExplorerGraphByResourceIdDelegate == null)) {
                this.onEndGetVisualExplorerGraphByResourceIdDelegate = new EndOperationDelegate(this.OnEndGetVisualExplorerGraphByResourceId);
            }
            if ((this.onGetVisualExplorerGraphByResourceIdCompletedDelegate == null)) {
                this.onGetVisualExplorerGraphByResourceIdCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetVisualExplorerGraphByResourceIdCompleted);
            }
            base.InvokeAsync(this.onBeginGetVisualExplorerGraphByResourceIdDelegate, new object[] {
                        resourceId}, this.onEndGetVisualExplorerGraphByResourceIdDelegate, this.onGetVisualExplorerGraphByResourceIdCompletedDelegate, userState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.IAsyncResult Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService.BeginGetResourceMetadataByResourceId(string resourceId, System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginGetResourceMetadataByResourceId(resourceId, callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        string Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService.EndGetResourceMetadataByResourceId(System.IAsyncResult result) {
            return base.Channel.EndGetResourceMetadataByResourceId(result);
        }
        
        private System.IAsyncResult OnBeginGetResourceMetadataByResourceId(object[] inValues, System.AsyncCallback callback, object asyncState) {
            string resourceId = ((string)(inValues[0]));
            return ((Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService)(this)).BeginGetResourceMetadataByResourceId(resourceId, callback, asyncState);
        }
        
        private object[] OnEndGetResourceMetadataByResourceId(System.IAsyncResult result) {
            string retVal = ((Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService)(this)).EndGetResourceMetadataByResourceId(result);
            return new object[] {
                    retVal};
        }
        
        private void OnGetResourceMetadataByResourceIdCompleted(object state) {
            if ((this.GetResourceMetadataByResourceIdCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.GetResourceMetadataByResourceIdCompleted(this, new GetResourceMetadataByResourceIdCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void GetResourceMetadataByResourceIdAsync(string resourceId) {
            this.GetResourceMetadataByResourceIdAsync(resourceId, null);
        }
        
        public void GetResourceMetadataByResourceIdAsync(string resourceId, object userState) {
            if ((this.onBeginGetResourceMetadataByResourceIdDelegate == null)) {
                this.onBeginGetResourceMetadataByResourceIdDelegate = new BeginOperationDelegate(this.OnBeginGetResourceMetadataByResourceId);
            }
            if ((this.onEndGetResourceMetadataByResourceIdDelegate == null)) {
                this.onEndGetResourceMetadataByResourceIdDelegate = new EndOperationDelegate(this.OnEndGetResourceMetadataByResourceId);
            }
            if ((this.onGetResourceMetadataByResourceIdCompletedDelegate == null)) {
                this.onGetResourceMetadataByResourceIdCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetResourceMetadataByResourceIdCompleted);
            }
            base.InvokeAsync(this.onBeginGetResourceMetadataByResourceIdDelegate, new object[] {
                        resourceId}, this.onEndGetResourceMetadataByResourceIdDelegate, this.onGetResourceMetadataByResourceIdCompletedDelegate, userState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.IAsyncResult Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService.BeginGetResourceRelationByResourceId(string subjectResourceId, string objectResourceId, System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginGetResourceRelationByResourceId(subjectResourceId, objectResourceId, callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        string Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService.EndGetResourceRelationByResourceId(System.IAsyncResult result) {
            return base.Channel.EndGetResourceRelationByResourceId(result);
        }
        
        private System.IAsyncResult OnBeginGetResourceRelationByResourceId(object[] inValues, System.AsyncCallback callback, object asyncState) {
            string subjectResourceId = ((string)(inValues[0]));
            string objectResourceId = ((string)(inValues[1]));
            return ((Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService)(this)).BeginGetResourceRelationByResourceId(subjectResourceId, objectResourceId, callback, asyncState);
        }
        
        private object[] OnEndGetResourceRelationByResourceId(System.IAsyncResult result) {
            string retVal = ((Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService)(this)).EndGetResourceRelationByResourceId(result);
            return new object[] {
                    retVal};
        }
        
        private void OnGetResourceRelationByResourceIdCompleted(object state) {
            if ((this.GetResourceRelationByResourceIdCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.GetResourceRelationByResourceIdCompleted(this, new GetResourceRelationByResourceIdCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void GetResourceRelationByResourceIdAsync(string subjectResourceId, string objectResourceId) {
            this.GetResourceRelationByResourceIdAsync(subjectResourceId, objectResourceId, null);
        }
        
        public void GetResourceRelationByResourceIdAsync(string subjectResourceId, string objectResourceId, object userState) {
            if ((this.onBeginGetResourceRelationByResourceIdDelegate == null)) {
                this.onBeginGetResourceRelationByResourceIdDelegate = new BeginOperationDelegate(this.OnBeginGetResourceRelationByResourceId);
            }
            if ((this.onEndGetResourceRelationByResourceIdDelegate == null)) {
                this.onEndGetResourceRelationByResourceIdDelegate = new EndOperationDelegate(this.OnEndGetResourceRelationByResourceId);
            }
            if ((this.onGetResourceRelationByResourceIdCompletedDelegate == null)) {
                this.onGetResourceRelationByResourceIdCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetResourceRelationByResourceIdCompleted);
            }
            base.InvokeAsync(this.onBeginGetResourceRelationByResourceIdDelegate, new object[] {
                        subjectResourceId,
                        objectResourceId}, this.onEndGetResourceRelationByResourceIdDelegate, this.onGetResourceRelationByResourceIdCompletedDelegate, userState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.IAsyncResult Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService.BeginGetResourcesByKeyword(string keyword, System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginGetResourcesByKeyword(keyword, callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        string Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService.EndGetResourcesByKeyword(System.IAsyncResult result) {
            return base.Channel.EndGetResourcesByKeyword(result);
        }
        
        private System.IAsyncResult OnBeginGetResourcesByKeyword(object[] inValues, System.AsyncCallback callback, object asyncState) {
            string keyword = ((string)(inValues[0]));
            return ((Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService)(this)).BeginGetResourcesByKeyword(keyword, callback, asyncState);
        }
        
        private object[] OnEndGetResourcesByKeyword(System.IAsyncResult result) {
            string retVal = ((Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService)(this)).EndGetResourcesByKeyword(result);
            return new object[] {
                    retVal};
        }
        
        private void OnGetResourcesByKeywordCompleted(object state) {
            if ((this.GetResourcesByKeywordCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.GetResourcesByKeywordCompleted(this, new GetResourcesByKeywordCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void GetResourcesByKeywordAsync(string keyword) {
            this.GetResourcesByKeywordAsync(keyword, null);
        }
        
        public void GetResourcesByKeywordAsync(string keyword, object userState) {
            if ((this.onBeginGetResourcesByKeywordDelegate == null)) {
                this.onBeginGetResourcesByKeywordDelegate = new BeginOperationDelegate(this.OnBeginGetResourcesByKeyword);
            }
            if ((this.onEndGetResourcesByKeywordDelegate == null)) {
                this.onEndGetResourcesByKeywordDelegate = new EndOperationDelegate(this.OnEndGetResourcesByKeyword);
            }
            if ((this.onGetResourcesByKeywordCompletedDelegate == null)) {
                this.onGetResourcesByKeywordCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetResourcesByKeywordCompleted);
            }
            base.InvokeAsync(this.onBeginGetResourcesByKeywordDelegate, new object[] {
                        keyword}, this.onEndGetResourcesByKeywordDelegate, this.onGetResourcesByKeywordCompletedDelegate, userState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.IAsyncResult Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService.BeginGetVisualExplorerFilterList(System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginGetVisualExplorerFilterList(callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        Zentity.VisualExplorer.VisualExplorerService.VisualExplorerFilterList Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService.EndGetVisualExplorerFilterList(System.IAsyncResult result) {
            return base.Channel.EndGetVisualExplorerFilterList(result);
        }
        
        private System.IAsyncResult OnBeginGetVisualExplorerFilterList(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService)(this)).BeginGetVisualExplorerFilterList(callback, asyncState);
        }
        
        private object[] OnEndGetVisualExplorerFilterList(System.IAsyncResult result) {
            Zentity.VisualExplorer.VisualExplorerService.VisualExplorerFilterList retVal = ((Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService)(this)).EndGetVisualExplorerFilterList(result);
            return new object[] {
                    retVal};
        }
        
        private void OnGetVisualExplorerFilterListCompleted(object state) {
            if ((this.GetVisualExplorerFilterListCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.GetVisualExplorerFilterListCompleted(this, new GetVisualExplorerFilterListCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void GetVisualExplorerFilterListAsync() {
            this.GetVisualExplorerFilterListAsync(null);
        }
        
        public void GetVisualExplorerFilterListAsync(object userState) {
            if ((this.onBeginGetVisualExplorerFilterListDelegate == null)) {
                this.onBeginGetVisualExplorerFilterListDelegate = new BeginOperationDelegate(this.OnBeginGetVisualExplorerFilterList);
            }
            if ((this.onEndGetVisualExplorerFilterListDelegate == null)) {
                this.onEndGetVisualExplorerFilterListDelegate = new EndOperationDelegate(this.OnEndGetVisualExplorerFilterList);
            }
            if ((this.onGetVisualExplorerFilterListCompletedDelegate == null)) {
                this.onGetVisualExplorerFilterListCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetVisualExplorerFilterListCompleted);
            }
            base.InvokeAsync(this.onBeginGetVisualExplorerFilterListDelegate, null, this.onEndGetVisualExplorerFilterListDelegate, this.onGetVisualExplorerFilterListCompletedDelegate, userState);
        }
        
        private System.IAsyncResult OnBeginOpen(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(callback, asyncState);
        }
        
        private object[] OnEndOpen(System.IAsyncResult result) {
            ((System.ServiceModel.ICommunicationObject)(this)).EndOpen(result);
            return null;
        }
        
        private void OnOpenCompleted(object state) {
            if ((this.OpenCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.OpenCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void OpenAsync() {
            this.OpenAsync(null);
        }
        
        public void OpenAsync(object userState) {
            if ((this.onBeginOpenDelegate == null)) {
                this.onBeginOpenDelegate = new BeginOperationDelegate(this.OnBeginOpen);
            }
            if ((this.onEndOpenDelegate == null)) {
                this.onEndOpenDelegate = new EndOperationDelegate(this.OnEndOpen);
            }
            if ((this.onOpenCompletedDelegate == null)) {
                this.onOpenCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnOpenCompleted);
            }
            base.InvokeAsync(this.onBeginOpenDelegate, null, this.onEndOpenDelegate, this.onOpenCompletedDelegate, userState);
        }
        
        private System.IAsyncResult OnBeginClose(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((System.ServiceModel.ICommunicationObject)(this)).BeginClose(callback, asyncState);
        }
        
        private object[] OnEndClose(System.IAsyncResult result) {
            ((System.ServiceModel.ICommunicationObject)(this)).EndClose(result);
            return null;
        }
        
        private void OnCloseCompleted(object state) {
            if ((this.CloseCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.CloseCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void CloseAsync() {
            this.CloseAsync(null);
        }
        
        public void CloseAsync(object userState) {
            if ((this.onBeginCloseDelegate == null)) {
                this.onBeginCloseDelegate = new BeginOperationDelegate(this.OnBeginClose);
            }
            if ((this.onEndCloseDelegate == null)) {
                this.onEndCloseDelegate = new EndOperationDelegate(this.OnEndClose);
            }
            if ((this.onCloseCompletedDelegate == null)) {
                this.onCloseCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnCloseCompleted);
            }
            base.InvokeAsync(this.onBeginCloseDelegate, null, this.onEndCloseDelegate, this.onCloseCompletedDelegate, userState);
        }
        
        protected override Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService CreateChannel() {
            return new VisualExplorerServiceClientChannel(this);
        }
        
        private class VisualExplorerServiceClientChannel : ChannelBase<Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService>, Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService {
            
            public VisualExplorerServiceClientChannel(System.ServiceModel.ClientBase<Zentity.VisualExplorer.VisualExplorerService.IVisualExplorerService> client) : 
                    base(client) {
            }
            
            public System.IAsyncResult BeginGetVisualExplorerGraphBySearchKeyword(string keyword, System.AsyncCallback callback, object asyncState) {
                object[] _args = new object[1];
                _args[0] = keyword;
                System.IAsyncResult _result = base.BeginInvoke("GetVisualExplorerGraphBySearchKeyword", _args, callback, asyncState);
                return _result;
            }
            
            public Zentity.VisualExplorer.VisualExplorerService.VisualExplorerGraph EndGetVisualExplorerGraphBySearchKeyword(System.IAsyncResult result) {
                object[] _args = new object[0];
                Zentity.VisualExplorer.VisualExplorerService.VisualExplorerGraph _result = ((Zentity.VisualExplorer.VisualExplorerService.VisualExplorerGraph)(base.EndInvoke("GetVisualExplorerGraphBySearchKeyword", _args, result)));
                return _result;
            }
            
            public System.IAsyncResult BeginGetVisualExplorerGraphByResourceId(string resourceId, System.AsyncCallback callback, object asyncState) {
                object[] _args = new object[1];
                _args[0] = resourceId;
                System.IAsyncResult _result = base.BeginInvoke("GetVisualExplorerGraphByResourceId", _args, callback, asyncState);
                return _result;
            }
            
            public Zentity.VisualExplorer.VisualExplorerService.VisualExplorerGraph EndGetVisualExplorerGraphByResourceId(System.IAsyncResult result) {
                object[] _args = new object[0];
                Zentity.VisualExplorer.VisualExplorerService.VisualExplorerGraph _result = ((Zentity.VisualExplorer.VisualExplorerService.VisualExplorerGraph)(base.EndInvoke("GetVisualExplorerGraphByResourceId", _args, result)));
                return _result;
            }
            
            public System.IAsyncResult BeginGetResourceMetadataByResourceId(string resourceId, System.AsyncCallback callback, object asyncState) {
                object[] _args = new object[1];
                _args[0] = resourceId;
                System.IAsyncResult _result = base.BeginInvoke("GetResourceMetadataByResourceId", _args, callback, asyncState);
                return _result;
            }
            
            public string EndGetResourceMetadataByResourceId(System.IAsyncResult result) {
                object[] _args = new object[0];
                string _result = ((string)(base.EndInvoke("GetResourceMetadataByResourceId", _args, result)));
                return _result;
            }
            
            public System.IAsyncResult BeginGetResourceRelationByResourceId(string subjectResourceId, string objectResourceId, System.AsyncCallback callback, object asyncState) {
                object[] _args = new object[2];
                _args[0] = subjectResourceId;
                _args[1] = objectResourceId;
                System.IAsyncResult _result = base.BeginInvoke("GetResourceRelationByResourceId", _args, callback, asyncState);
                return _result;
            }
            
            public string EndGetResourceRelationByResourceId(System.IAsyncResult result) {
                object[] _args = new object[0];
                string _result = ((string)(base.EndInvoke("GetResourceRelationByResourceId", _args, result)));
                return _result;
            }
            
            public System.IAsyncResult BeginGetResourcesByKeyword(string keyword, System.AsyncCallback callback, object asyncState) {
                object[] _args = new object[1];
                _args[0] = keyword;
                System.IAsyncResult _result = base.BeginInvoke("GetResourcesByKeyword", _args, callback, asyncState);
                return _result;
            }
            
            public string EndGetResourcesByKeyword(System.IAsyncResult result) {
                object[] _args = new object[0];
                string _result = ((string)(base.EndInvoke("GetResourcesByKeyword", _args, result)));
                return _result;
            }
            
            public System.IAsyncResult BeginGetVisualExplorerFilterList(System.AsyncCallback callback, object asyncState) {
                object[] _args = new object[0];
                System.IAsyncResult _result = base.BeginInvoke("GetVisualExplorerFilterList", _args, callback, asyncState);
                return _result;
            }
            
            public Zentity.VisualExplorer.VisualExplorerService.VisualExplorerFilterList EndGetVisualExplorerFilterList(System.IAsyncResult result) {
                object[] _args = new object[0];
                Zentity.VisualExplorer.VisualExplorerService.VisualExplorerFilterList _result = ((Zentity.VisualExplorer.VisualExplorerService.VisualExplorerFilterList)(base.EndInvoke("GetVisualExplorerFilterList", _args, result)));
                return _result;
            }
        }
    }
}
