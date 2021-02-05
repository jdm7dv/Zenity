// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Web
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;

    public class PublishQueueProcessor
    {
        #region Private Static ReadOnly Members

        /// <summary>
        /// Singleton instance of PublishQueueProcessor
        /// </summary>
        private static readonly PublishQueueProcessor SingletonQueueProcessor;

        /// <summary>
        /// Hash set of PublishRequest
        /// </summary>
        private static readonly HashSet<PublishRequest> PublishRequestTracker;

        #endregion

        #region Public Constants

        /// <summary>
        /// Minimum instances to spawn per core to do the processing.
        /// </summary>
        public const int MinInstancesPerCore = 1;

        /// <summary>
        /// Maximum instances to spawn per core to do the processing.
        /// </summary>
        public const int MaxInstancesPerCore = 5;

        #endregion

        #region Private Fields

        private string tempFolder;

        private readonly object lockObject;

        private int threadCount;

        private readonly List<Thread> threadPool;

        private ConcurrentQueue<PublishRequest> publishRequestQueue;

        private volatile bool stopRequested;

        private volatile int itemCount;

        private volatile int processedItemCount;

        private readonly AutoResetEvent waitHandle;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes static members of the <see cref="PublishQueueProcessor"/> class.
        /// </summary>
        static PublishQueueProcessor()
        {
            // Initialize the Request Tracker for processed requests
            if (PublishRequestTracker == null)
            {
                PublishRequestTracker = new HashSet<PublishRequest>(new PublishRequestComparer());
            }

            // Initialize the QueueProcessor singleton object for queued request handling and processing
            if (SingletonQueueProcessor == null)
            {
                SingletonQueueProcessor = new PublishQueueProcessor();
            }
            
            // Recover Request Tracker from Recovery Database
            LoadTrackerFromDatabase();
            // Recover Request Queue from Recovery Database
            LoadQueueFromDatabase();
            // Signal threaded queue processing to start
            SingletonQueueProcessor.Start();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="PublishQueueProcessor"/> class from being created.
        /// </summary>
        private PublishQueueProcessor()
        {
            tempFolder = Path.GetTempPath();
            waitHandle = new AutoResetEvent(false);
            lockObject = new object();
            threadPool = new List<Thread>();
            threadCount = Environment.ProcessorCount * MinInstancesPerCore;
            string instancesPerCore = System.Configuration.ConfigurationManager.AppSettings["InstancesPerCore"];
            if (!string.IsNullOrWhiteSpace(instancesPerCore))
            {
                int instancePerCore;
                if (int.TryParse(instancesPerCore, out instancePerCore))
                {
                    if (instancePerCore >= MinInstancesPerCore && instancePerCore <= MaxInstancesPerCore)
                    {
                        threadCount = Environment.ProcessorCount * instancePerCore;
                    }
                }
            }
            publishRequestQueue = new ConcurrentQueue<PublishRequest>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>The current.</value>
        public static PublishQueueProcessor Current
        {
            get
            {
                return SingletonQueueProcessor;
            }
        }

        /// <summary>
        /// Gets the request tracker.
        /// </summary>
        /// <value>The request tracker.</value>
        public static IEnumerable<PublishRequest> RequestTracker
        {
            get
            {
                return PublishRequestTracker;
            }
        }

        /// <summary>
        /// Gets or sets the thread count.
        /// </summary>
        /// <value>The thread count.</value>
        public int ThreadCount
        {
            get
            {
                return threadCount;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentException(string.Format(Properties.Messages.ThreadValueMustBeAtleastOne, value));
                }

                threadCount = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public static void Initialize()
        {
            // Method to invode the static constructor.    
        }

        /// <summary>
        /// Submits the specified item id.
        /// </summary>
        /// <param name="publishRequest">The publish request.</param>
        public void Submit(PublishRequest publishRequest)
        {
            if (publishRequest == null || publishRequest.InstanceId == Guid.Empty)
                return;

            if (publishRequestQueue.Contains(publishRequest, new PublishRequestComparer()))
            {
                return;
            }

            publishRequestQueue.Enqueue(publishRequest);
            publishRequest.SaveToDatabase(true);
            itemCount++;
            waitHandle.Set();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads the tracker from database.
        /// </summary>
        private static void LoadTrackerFromDatabase()
        {
            IEnumerable<PublishRequest> requestsFromDatabase = PublishRequest.LoadFromDatabase(false);
            foreach (var requestFromDataBase in requestsFromDatabase)
            {
                if (requestFromDataBase.CurrentStatus.CurrentStage == PublishStage.Completed ||
                    requestFromDataBase.CurrentStatus.CurrentStage == PublishStage.AbortedOnError ||
                    requestFromDataBase.CurrentStatus.CurrentStage == PublishStage.AbortedOnDemand ||
                    requestFromDataBase.CurrentStatus.CurrentStage == PublishStage.PerformingCleanup)
                {
                    if (requestFromDataBase.CurrentStatus.CurrentStage == PublishStage.PerformingCleanup)
                    {
                        requestFromDataBase.CurrentStatus.CurrentStage = PublishStage.Completed;
                    }
                    PublishRequestTracker.Add(requestFromDataBase);
                }
                else
                {
                    PublishStatus newStatus = new PublishStatus(requestFromDataBase.InstanceId) { ResourceTypeName = requestFromDataBase.ResourceTypeName, DataModelNamespace = requestFromDataBase.DataModelNamespace };
                    requestFromDataBase.CurrentStatus = newStatus;
                    SingletonQueueProcessor.publishRequestQueue.Enqueue(requestFromDataBase);
                }
            }
        }

        /// <summary>
        /// Loads the queue from database.
        /// </summary>
        private static void LoadQueueFromDatabase()
        {
            IEnumerable<PublishRequest> requestsFromDatabase = PublishRequest.LoadFromDatabase(true);
            foreach (var requestFromDataBase in requestsFromDatabase)
            {
                SingletonQueueProcessor.publishRequestQueue.Enqueue(requestFromDataBase);
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        private void Start()
        {
            lock (threadPool)
            {
                if (threadPool.Count > 0) return;

                stopRequested = false;
                while (threadPool.Count < threadCount)
                {
                    Thread newThread = new Thread(OnThreadRun)
                    {
                        Name = "PublishQueueProcessor-" + threadPool.Count,
                        IsBackground = true
                    };
                    newThread.Start();
                    threadPool.Add(newThread);
                }
            }

            Globals.TraceMessage(TraceEventType.Information, string.Empty, string.Format(Properties.Messages.CreatedThreadPoolForPublishQueueProcessor, threadPool.Count));
        }

        /// <summary>
        /// Joins this instance.
        /// </summary>
        private void Join()
        {
            int lastReportedSize = int.MaxValue;
            while (true)
            {
                lock (lockObject)
                {
                    int countRemaining = itemCount - processedItemCount;
                    if (countRemaining == 0) break;

                    if ((lastReportedSize - countRemaining) > 10)
                    {
                        lastReportedSize = countRemaining;
                    }
                    Monitor.Wait(lockObject);
                }
            }

            stopRequested = true;
            lock (publishRequestQueue) { Monitor.PulseAll(publishRequestQueue); }
            lock (threadPool)
            {
                lastReportedSize = int.MaxValue;
                while (threadPool.Count > 0)
                {
                    if ((lastReportedSize - threadPool.Count) > 10)
                    {
                        lastReportedSize = threadPool.Count;
                    }
                    Monitor.Wait(threadPool);
                }
            }
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        private void Reset()
        {
            this.Join();
            threadPool.Clear();
            publishRequestQueue = new ConcurrentQueue<PublishRequest>();
            stopRequested = false;
        }


        /// <summary>
        /// Called when [thread run].
        /// </summary>
        private void OnThreadRun()
        {
            bool shouldResign = false;
            while ((stopRequested == false) && (shouldResign == false))
            {
                PublishRequest currentRequest = null;
                try
                {
                    if (publishRequestQueue.IsEmpty)
                    {
                        waitHandle.WaitOne();
                        continue;
                    }

                    // Check whether the peeked resource is running in PublishRequestTracker
                    // => If running then set all the requests to add it to the end
                    // => If not then dequeue and process
                    lock (lockObject)
                    {
                        if (!RearrangePublishRequestQueue())
                        {
                            Monitor.Pulse(lockObject);
                            continue;
                        }
                    }

                    if (publishRequestQueue.TryDequeue(out currentRequest))
                    {
                        this.ProcessPublishRequest(currentRequest);
                    }
                }
                catch (OutOfMemoryException)
                {
                    Globals.TraceMessage(TraceEventType.Error, string.Empty, Properties.Messages.OutOfMemoryOnThreadRun);
                    lock (threadPool)
                    {
                        if (threadPool.Count > 1)
                        {
                            shouldResign = true;
                        }
                    }

                    if (currentRequest != null)
                    {
                        publishRequestQueue.Enqueue(currentRequest);
                    }

                    lock (lockObject) { Monitor.Pulse(lockObject); }
                }
                catch (Exception e)
                {
                    Globals.TraceMessage(TraceEventType.Error, e.ToString(), Properties.Messages.ExceptionOnThreadRun);
                    lock (lockObject) { Monitor.Pulse(lockObject); }
                }
            }

            lock (threadPool)
            {
                threadPool.Remove(Thread.CurrentThread);
                Monitor.Pulse(threadPool);
            }
        }

        /// <summary>
        /// Rearranges the publish request queue.
        /// </summary>
        /// <returns><c>True</c> if succesfull, <c>false</c> otherwise.</returns>
        private bool RearrangePublishRequestQueue()
        {
            // Peek on the first PublishRequest in the queue
            PublishRequest tempRequest;
            if (!publishRequestQueue.TryPeek(out tempRequest))
            {
                return false;
            }

            string dataModelNamespace = tempRequest.DataModelNamespace;
            string resourceTypeName = tempRequest.ResourceTypeName;
            int sameResourceTypeInProgressCount = PublishRequestTracker.Where(request => request.DataModelNamespace.Equals(dataModelNamespace, StringComparison.OrdinalIgnoreCase) &&
                                                                                         request.ResourceTypeName.Equals(resourceTypeName, StringComparison.OrdinalIgnoreCase) &&
                                                                                         request.CurrentStatus.CurrentStage != PublishStage.AbortedOnError &&
                                                                                         request.CurrentStatus.CurrentStage != PublishStage.AbortedOnDemand &&
                                                                                         request.CurrentStatus.CurrentStage != PublishStage.Completed).Count();
            if (sameResourceTypeInProgressCount > 0)
            {
                // Rearrange the queue so that all PublishRequests for the same resource type are queued in the back.
                // This will ensure the same resource type is not processed immediately.
                Queue<PublishRequest> sameResouceTypeRequestQueue = new Queue<PublishRequest>();
                Queue<PublishRequest> otherResouceTypeRequestQueue = new Queue<PublishRequest>();

                while (!publishRequestQueue.IsEmpty)
                {
                    PublishRequest firstRequest;
                    if (!publishRequestQueue.TryDequeue(out firstRequest))
                        continue;

                    if (firstRequest.DataModelNamespace.Equals(dataModelNamespace, StringComparison.OrdinalIgnoreCase) &&
                        firstRequest.ResourceTypeName.Equals(resourceTypeName, StringComparison.OrdinalIgnoreCase))
                    {
                        sameResouceTypeRequestQueue.Enqueue(firstRequest);
                    }
                    else
                    {
                        otherResouceTypeRequestQueue.Enqueue(firstRequest);
                    }
                }

                foreach (PublishRequest requestItem in otherResouceTypeRequestQueue)
                {
                    publishRequestQueue.Enqueue(requestItem);
                }

                foreach (PublishRequest requestItem in sameResouceTypeRequestQueue)
                {
                    publishRequestQueue.Enqueue(requestItem);
                }

                PublishRequest.SaveToDatabase(publishRequestQueue, true);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Processes the publish request.
        /// </summary>
        /// <param name="currentPublishRequest">The current publish request.</param>
        private void ProcessPublishRequest(PublishRequest currentPublishRequest)
        {
            Globals.TraceMessage(TraceEventType.Information, currentPublishRequest.ToString(), string.Format(Properties.Messages.StartedProcessingQueuedPublishRequest, currentPublishRequest.InstanceId));
            
            // Add the item in the Request Tracker
            // Remove old requests of the same resourcetype from the Request Tracker
            lock (PublishRequestTracker)
            {
                PublishRequestTracker.RemoveWhere(request => request.DataModelNamespace.Equals(currentPublishRequest.DataModelNamespace, StringComparison.OrdinalIgnoreCase) &&
                                                             request.ResourceTypeName.Equals(currentPublishRequest.ResourceTypeName, StringComparison.OrdinalIgnoreCase));
                PublishRequestTracker.Add(currentPublishRequest);
                currentPublishRequest.DeleteFromDatabase(false, true);
                currentPublishRequest.SaveToDatabase(false);
            }

            // If the request is a cancelled request then mark it as AbortedOnDemand and save to database.
            if (currentPublishRequest.IsCancellationRequested)
            {
                currentPublishRequest.CurrentStatus.CurrentStage = PublishStage.AbortedOnDemand;
                currentPublishRequest.SaveToDatabase(false);
                return;
            }

            try
            {
                using (Process zentityCollectionCreator = new Process())
                {
                    zentityCollectionCreator.StartInfo = new ProcessStartInfo()
                    {
                        // Create the Zentity.Pivot.CollectionCreator Process
                        FileName = Path.Combine(Utilities.AssemblyLocation, "Zentity.Pivot.CollectionCreator.exe"),
                        // Pass the specific arguments to the process
                        Arguments = string.Format("/operation:{0} /instanceId:{1} /modelNamespace:{2} /resourceType:{3} {4}",
                                                    currentPublishRequest.Operation,
                                                    currentPublishRequest.InstanceId,
                                                    currentPublishRequest.DataModelNamespace,
                                                    currentPublishRequest.ResourceTypeName,
                                                    currentPublishRequest.Operation == PublishOperation.UpdateCollection
                                                    ? string.Format("/changemessagefilepath:\"{0}\"", currentPublishRequest.ChangeMessageFilePath)
                                                    : string.Empty),
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    };

                    currentPublishRequest.CurrentStatus.CurrentStage = PublishStage.Initiating;
                    zentityCollectionCreator.Start();
                    // Read the output stream first and then wait.
                    string output = zentityCollectionCreator.StandardOutput.ReadToEnd();
                    // Thread waits till the process exits
                    zentityCollectionCreator.WaitForExit();
                    
                    // Analyze the ExitCode and throw the correct execption.
                    switch (zentityCollectionCreator.ExitCode)
                    {
                        // CollectionCreator has exited with a ReturnCode
                        case (int) ReturnCode.Error:
                        case (int) ReturnCode.Invalid:
                            throw new ApplicationException(output.Replace("\n", "\r\n"));

                        // User has ended the CollectionCreator process from Task Manager etc.
                        case 1:
                            throw new InvalidOperationException(Properties.Messages.CollectionCreatorProcessKilled);
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), string.Format(Properties.Messages.ExceptionProcessingPublishRequest, currentPublishRequest.InstanceId));
                currentPublishRequest.CurrentStatus.CurrentStage = PublishStage.AbortedOnError;
                currentPublishRequest.PublishingCallback = null;
            }

            // Do cleanup, if the CollectionCreator process has not done itself.
            try
            {
                string directoryPath = Path.Combine(Path.GetTempPath(), currentPublishRequest.InstanceId.ToString());
                string imagesPath = Path.Combine(Path.GetTempPath(), string.Format("{0}_Images", currentPublishRequest.InstanceId));
                if (Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, true);
                }
                if (Directory.Exists(imagesPath))
                {
                    Directory.Delete(imagesPath, true);
                }
            }
            catch { }

            // If the Collection creator has not been able to report progress (highly unlikely). 
            // Assume that the operation completed successfully.
            if (currentPublishRequest.CurrentStatus.CurrentStage == PublishStage.NotStarted ||
                currentPublishRequest.CurrentStatus.CurrentStage == PublishStage.Initiating)
            {
                Globals.TraceMessage(TraceEventType.Information, string.Empty, string.Format(Properties.Messages.CollectionCreatorProgressReportFailure, currentPublishRequest.InstanceId));
                currentPublishRequest.CurrentStatus.CurrentStage = PublishStage.Completed;
            }

            currentPublishRequest.SaveToDatabase(false);
            lock (lockObject) { processedItemCount++; Monitor.Pulse(lockObject); }

            Globals.TraceMessage(TraceEventType.Information, currentPublishRequest.ToString(), string.Format(Properties.Messages.FinishedProcessingQueuedPublishRequest, currentPublishRequest.InstanceId));
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Updates the Request Queue by Cancelling any existing update,create operations for the PublishRequest provided and adds the new one.
        /// </summary>
        /// <param name="publishRequest">PublishRequest containing DataModelNamespace and ResouceTypeName</param>
        internal void UpdateRequestQueue(PublishRequest publishRequest)
        {
            Globals.TraceMessage(TraceEventType.Verbose, publishRequest.ToString(), Properties.Messages.StartedFunctionMessage);

            lock (lockObject)
            {
                IEnumerable<PublishRequest> updatePublishRequest = publishRequestQueue.Where(p =>
                                                p.DataModelNamespace.Equals(publishRequest.DataModelNamespace, StringComparison.OrdinalIgnoreCase) &&
                                                p.ResourceTypeName.Equals(publishRequest.ResourceTypeName, StringComparison.OrdinalIgnoreCase));

                if (updatePublishRequest != null && updatePublishRequest.Count() > 0)
                {
                    foreach (PublishRequest request in updatePublishRequest)
                    {
                        request.CancelRequest();
                    }
                }
            }

            // Add the new publish request by calling submit
            this.Submit(publishRequest);

            Globals.TraceMessage(TraceEventType.Verbose, publishRequest.ToString(), Properties.Messages.FinishedFunctionMessage);
        }

        /// <summary>
        /// Cancels the CXML creation.
        /// </summary>
        /// <param name="modelNamespace">The model namespace containing the resource type.</param>
        /// <param name="resourceType">The resource type for which cxml is to be deleted.</param>
        /// <returns>True if Cancelation was successful or else false.</returns>
        internal bool CancelPublishRequest(string modelNamespace, string resourceType)
        {
            Globals.TraceMessage(TraceEventType.Information, string.Empty, string.Format(Properties.Messages.CancellationRequestedByUser, modelNamespace, resourceType));

            bool returnFlag = false, returnFlag1 = false, exceptionFlag = false;
            lock (lockObject)
            {
                IEnumerable<PublishRequest> publishRequestFromQueue = publishRequestQueue.Where(request => request.DataModelNamespace.Equals(modelNamespace, StringComparison.OrdinalIgnoreCase) &&
                                                                                                               request.ResourceTypeName.Equals(resourceType, StringComparison.OrdinalIgnoreCase) &&
                                                                                                               !request.IsCancellationRequested);
                IEnumerable<PublishRequest> publishRequestFromTracker = PublishRequestTracker.Where(request => request.DataModelNamespace.Equals(modelNamespace, StringComparison.OrdinalIgnoreCase) &&
                                                                                                               request.ResourceTypeName.Equals(resourceType, StringComparison.OrdinalIgnoreCase) &&
                                                                                                               !request.IsCancellationRequested);

                if (publishRequestFromQueue != null && publishRequestFromQueue.Count() > 0)
                {
                    foreach (PublishRequest request in publishRequestFromQueue)
                    {
                        try
                        {
                            request.CancelRequest();
                            returnFlag = true;
                        }
                        catch (Exception ex)
                        {
                            Globals.TraceMessage(TraceEventType.Error, ex.ToString(), string.Format(Properties.Messages.ExceptionCancellingPublishRequest, modelNamespace, resourceType));
                            exceptionFlag = true;
                        }
                    }
                }

                if (publishRequestFromTracker != null && publishRequestFromTracker.Count() > 0)
                {
                    foreach (PublishRequest request in publishRequestFromTracker)
                    {
                        if (request.PublishingCallback != null)
                        {
                            try
                            {
                                request.PublishingCallback.CancelOperation();
                                returnFlag1 = true;
                            }
                            catch (Exception ex)
                            {
                                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), string.Format(Properties.Messages.ExceptionCancellingPublishRequest, modelNamespace, resourceType));
                                exceptionFlag = true;
                            }
                        }
                    }
                }
            }

            return exceptionFlag ? false : (returnFlag || returnFlag1);
        }

        /// <summary>
        /// Cancels the CXML creation.
        /// </summary>
        /// <param name="instanceId">Instance Id of the publish operation.</param>
        /// <returns>True if Cancelation was successful or else false.</returns>
        internal bool CancelPublishRequest(Guid instanceId)
        {
            Globals.TraceMessage(TraceEventType.Information, string.Empty, string.Format(Properties.Messages.CancellationRequestedByUserForInstance, instanceId));

            bool returnFlag = false, returnFlag1 = false, exceptionFlag = false;
            lock (lockObject)
            {
                PublishRequest publishRequestFromQueue = publishRequestQueue.Where(request => request.InstanceId.Equals(instanceId) && !request.IsCancellationRequested).FirstOrDefault();
                PublishRequest publishRequestFromTracker = PublishRequestTracker.Where(request => request.InstanceId.Equals(instanceId) && !request.IsCancellationRequested).FirstOrDefault();

                if (publishRequestFromQueue != null)
                {
                    try
                    {
                        publishRequestFromQueue.CancelRequest();
                        returnFlag = true;
                    }
                    catch (Exception ex)
                    {
                        Globals.TraceMessage(TraceEventType.Error, ex.ToString(), string.Format(Properties.Messages.ExceptionCancellingPublishRequestForInstance, instanceId));
                        exceptionFlag = true;
                    }
                }

                if (publishRequestFromTracker != null)
                {
                    if (publishRequestFromTracker.PublishingCallback != null)
                    {
                        try
                        {
                            publishRequestFromTracker.PublishingCallback.CancelOperation();
                            returnFlag1 = true;
                        }
                        catch (Exception ex)
                        {
                            Globals.TraceMessage(TraceEventType.Error, ex.ToString(), string.Format(Properties.Messages.ExceptionCancellingPublishRequestForInstance, instanceId));
                            exceptionFlag = true;
                        }
                    }
                }
            }

            return exceptionFlag ? false : (returnFlag || returnFlag1);
        }

        /// <summary>
        /// Gets the status of CXML creation in the queue.
        /// </summary>
        /// <returns>
        /// Enumeration of PublishRequest for all the resource types or else null
        /// </returns>
        internal IEnumerable<PublishRequest> GetPublishStatusFromQueue()
        {
            IEnumerable<PublishRequest> queuedRequests;
            lock (lockObject)
            {
                queuedRequests = this.publishRequestQueue.ToList();
            }
            return queuedRequests;
        }

        #endregion
    }
}
