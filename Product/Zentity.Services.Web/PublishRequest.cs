// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Web
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading;
    using Zentity.Services.Web.Pivot;

    /// <summary>
    /// Publish Request
    /// </summary>
    [DataContract]
    public class PublishRequest
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets Instance Id (Guid)
        /// </summary>
        [DataMember]
        public Guid InstanceId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Data model Namespace
        /// </summary>
        [DataMember]
        public string DataModelNamespace
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Resource type name
        /// </summary>
        [DataMember]
        public string ResourceTypeName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Publish Operation (Create, Update)
        /// </summary>
        [DataMember]
        public PublishOperation Operation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets CollectionCreator Process Id
        /// </summary>
        public int CollectionCreatorProcessId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets DeepZoom Creator Process Id
        /// </summary>
        public int DeepZoomCreatorProcessId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Status of cxml creation
        /// </summary>
        [DataMember]
        public PublishStatus CurrentStatus
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Change message file path
        /// </summary>
        [DataMember]
        public string ChangeMessageFilePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether a cancellation is requested for the thread.
        /// </summary>
        [DataMember]
        public bool IsCancellationRequested
        {
            get
            {
                return this.cancelTokenSource.IsCancellationRequested;
            }
            private set
            {
                if (value)
                {
                    this.cancelTokenSource.Cancel();
                }
            }
        }

        /// <summary>
        /// Gets or sets Publishing Callback used to report progress and cancel cxml creation
        /// </summary>
        internal IPublishingCallback PublishingCallback
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the PublishRequest class.
        /// Creates a publish request handeled by the publishing service.
        /// </summary>
        /// <param name="instanceId">Instance Id</param>
        public PublishRequest(Guid instanceId)
        {
            if (instanceId == default(Guid))
            {
                throw new ArgumentNullException("Instance Id", Properties.Messages.InstanceIdCannotBeGuidEmpty);
            }

            this.InstanceId = instanceId;
            this.cancelTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Initializes a new instance of the PublishRequest class.
        /// Creates a publish requst handeled by the publishing service.
        /// </summary>
        /// <param name="instanceId">Instance Id</param>
        /// <param name="dataModelNamespace">DataModel Namespace</param>
        /// <param name="resourceTypeName">ResourceType Name</param>
        public PublishRequest(Guid instanceId, string dataModelNamespace, string resourceTypeName)
            : this(instanceId)
        {
            this.DataModelNamespace = dataModelNamespace;
            this.ResourceTypeName = resourceTypeName;
        }

        #endregion

        #region Private Readonly Memebers

        /// <summary>
        /// Cancellation token to cancel the execution of the thread.
        /// </summary>
        private CancellationTokenSource cancelTokenSource;

        #endregion

        #region Internal Methods

        /// <summary>
        /// Cancel Request
        /// </summary>
        internal void CancelRequest()
        {
            this.cancelTokenSource.Cancel();
            this.CurrentStatus.CurrentStage = PublishStage.AbortedOnDemand;
            this.SaveToDatabase(true);
        }

        /// <summary>
        /// Saves the PublishRequest into database for recovery.
        /// </summary>
        /// <param name="isQueuedRequest">if set to <c>true</c> [is queued request].</param>
        /// <returns>True if successful otherwise false.</returns>
        internal bool SaveToDatabase(bool isQueuedRequest)
        {
            try
            {
                using (var databaseContext = new ZentityAdministrationDataContext(Utilities.ZentityConnectionString))
                {
                    var existingRequestDB = databaseContext.PublishRequestRecoveries.Where(request => request.InstanceId == this.InstanceId).FirstOrDefault();
                    if (existingRequestDB == default(PublishRequestRecovery))
                    {
                        // If this is a queued request then fetch the highest QueueOrder number. 
                        int currentQueueOrder = 0;
                        if (isQueuedRequest)
                        {
                            var queueOrderList = databaseContext.PublishRequestRecoveries.Where(request => request.IsQueuedRequest).Select(request => request.QueueOrder);
                            if (queueOrderList.Count() > 0)
                            {
                                currentQueueOrder = queueOrderList.Max();
                            }
                        }

                        // Create the new PublishRequestRecovery object and add to the database
                        PublishRequestRecovery newPublishRequestDB = new PublishRequestRecovery
                        {
                            InstanceId = this.InstanceId,
                            DataModelNamespace = this.DataModelNamespace,
                            ResourceTypeName = this.ResourceTypeName,
                            IsQueuedRequest = isQueuedRequest,
                            QueueOrder = currentQueueOrder + 1,
                            ObjectData = this.SerializeToXElement()
                        };

                        databaseContext.PublishRequestRecoveries.InsertOnSubmit(newPublishRequestDB);
                    }
                    else
                    {
                        if (existingRequestDB.IsQueuedRequest && isQueuedRequest == false)
                        {
                            existingRequestDB.QueueOrder = 0;
                        }
                        existingRequestDB.DataModelNamespace = this.DataModelNamespace;
                        existingRequestDB.ResourceTypeName = this.ResourceTypeName;
                        existingRequestDB.IsQueuedRequest = isQueuedRequest;
                        existingRequestDB.ObjectData = this.SerializeToXElement();
                    }

                    databaseContext.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);
                }
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Deletes the PublishRequest from database.
        /// </summary>
        /// <returns>True if successful otherwise false.</returns>
        internal bool DeleteFromDatabase()
        {
            try
            {
                using (var databaseContext = new ZentityAdministrationDataContext(Utilities.ZentityConnectionString))
                {
                    var existingRequestDB = databaseContext.PublishRequestRecoveries.Where(request => request.InstanceId == this.InstanceId).FirstOrDefault();
                    if (existingRequestDB != default(PublishRequestRecovery))
                    {
                        databaseContext.PublishRequestRecoveries.DeleteOnSubmit(existingRequestDB);
                    }
                    databaseContext.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
            }

            return false;
        }


        /// <summary>
        /// Deletes the PublishRequest from database.
        /// </summary>
        /// <param name="isQueuedRequest">if set to <c>true</c> if it is a queued request.</param>
        /// <param name="deleteAllRows">if set to <c>true</c> deletes all rows.</param>
        /// <returns>True if successful otherwise false.</returns>
        internal bool DeleteFromDatabase(bool isQueuedRequest, bool deleteAllRows)
        {
            try
            {
                if (!deleteAllRows)
                {
                    return DeleteFromDatabase();
                }

                using (var databaseContext = new ZentityAdministrationDataContext(Utilities.ZentityConnectionString))
                {
                    IEnumerable<PublishRequestRecovery> existingRequestsInDB = 
                               databaseContext.PublishRequestRecoveries.Where(request => request.IsQueuedRequest == isQueuedRequest &&
                                                                                         request.DataModelNamespace == this.DataModelNamespace &&
                                                                                         request.ResourceTypeName == this.ResourceTypeName).ToList();
                    if (existingRequestsInDB.Count() > 0)
                    {
                        databaseContext.PublishRequestRecoveries.DeleteAllOnSubmit(existingRequestsInDB);
                        databaseContext.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
            }

            return false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            const string StringFormat = " InstanceId   : {0}\r\n" +
                                        " Operation    : {1}\r\n" +
                                        " DataModel    : {2}\r\n" +
                                        " ResourceType : {3}\r\n" +
                                        " CurrentStage : {4}\r\n";
            return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                 StringFormat,
                                 this.InstanceId,
                                 this.Operation,
                                 this.DataModelNamespace,
                                 this.ResourceTypeName,
                                 this.CurrentStatus.CurrentStage);
        }

        /// <summary>
        /// Called when a serialized object is deserialized.
        /// </summary>
        /// <param name="streamingContext">The streaming context.</param>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext streamingContext)
        {
            if (this.cancelTokenSource == null)
            {
                this.cancelTokenSource = new CancellationTokenSource();
            }
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Saves the collection of PublishRequests into the database for recovery.
        /// </summary>
        /// <param name="requestCollection">The request collection.</param>
        /// <param name="isQueuedRequest">if set to <c>true</c> [is queued request].</param>
        /// <returns>True if successful otherwise false.</returns>
        public static bool SaveToDatabase(IEnumerable<PublishRequest> requestCollection, bool isQueuedRequest)
        {
            if (requestCollection.Count() > 0)
            {
                return false;
            }

            try
            {
                using (var databaseContext = new ZentityAdministrationDataContext(Utilities.ZentityConnectionString))
                {
                    List<PublishRequestRecovery> requestCollectionDB = new List<PublishRequestRecovery>();
                    int loopIndexNew = 0;
                    int loopIndexOld = 0;

                    foreach (var sourceRequest in requestCollection)
                    {
                        var existingRequestDB = databaseContext.PublishRequestRecoveries.Where(request => request.InstanceId == sourceRequest.InstanceId).FirstOrDefault();
                        if (existingRequestDB == default(PublishRequestRecovery))
                        {
                            PublishRequestRecovery newRequestDB = new PublishRequestRecovery
                            {
                                InstanceId = sourceRequest.InstanceId,
                                DataModelNamespace = sourceRequest.DataModelNamespace,
                                ResourceTypeName = sourceRequest.ResourceTypeName,
                                IsQueuedRequest = isQueuedRequest,
                                QueueOrder = isQueuedRequest ? loopIndexNew++ : 0,
                                ObjectData = sourceRequest.SerializeToXElement()
                            };
                            requestCollectionDB.Add(newRequestDB);
                        }
                        else
                        {
                            existingRequestDB.IsQueuedRequest = isQueuedRequest;
                            existingRequestDB.QueueOrder = loopIndexOld++;
                            existingRequestDB.ObjectData = sourceRequest.SerializeToXElement();
                            databaseContext.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);
                        }
                    }

                    if (requestCollectionDB.Count > 0)
                    {
                        databaseContext.PublishRequestRecoveries.InsertAllOnSubmit(requestCollectionDB);
                    }
                    
                    databaseContext.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
            }
            
            return false;
        }

        /// <summary>
        /// Loads the collection of PublishRequests from database.
        /// </summary>
        /// <param name="isQueuedRequest">if set to <c>true</c> [get queued request].</param>
        /// <returns>The collection of PublishRequests from database</returns>
        public static IEnumerable<PublishRequest> LoadFromDatabase(bool isQueuedRequest)
        {
            List<PublishRequest> requestCollection = new List<PublishRequest>();

            try
            {
                using (var databaseContext = new ZentityAdministrationDataContext(Utilities.ZentityConnectionString))
                {
                    databaseContext.ObjectTrackingEnabled = false;
                    var requestsFromDB = databaseContext.PublishRequestRecoveries.Where(request => request.IsQueuedRequest == isQueuedRequest).OrderBy(request => request.QueueOrder);
                    foreach (PublishRequestRecovery requestFromDB in requestsFromDB)
                    {
                        PublishRequest requestDeserialized = requestFromDB.DeserializeFromXElement();
                        if (requestDeserialized != null)
                        {
                            requestCollection.Add(requestDeserialized);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
            }
            return requestCollection;
        }

        #endregion
    }

    /// <summary>
    /// Publish Request Comparer
    /// </summary>
    public class PublishRequestComparer : IEqualityComparer<PublishRequest>
    {
        #region IEqualityComparer<PublishRequest> Members

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type T to compare.</param>
        /// <param name="y">The second object of type T to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(PublishRequest x, PublishRequest y)
        {
            return x.InstanceId == y.InstanceId;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="requestObject">The request object.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public int GetHashCode(PublishRequest requestObject)
        {
            return requestObject.InstanceId.GetHashCode();
        }

        #endregion
    }
}
