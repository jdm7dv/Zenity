// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Windows
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Pivot;

    /// <summary>
    /// Publishes the broker message to the webservice.
    /// </summary>
    internal class MessagePublisher : IDisposable
    {
        private const string MessagesElement = "Messages";
        private const string ZentityMessageElement = "ZentityMsg";
        private const string OperationElement = "Operation";
        private const string ChangedDataElement = "ChangedData";
        private const string ResourceElement = "Resource";
        private const string ResourceIdAttribute = "ResourceID";
        private const string NameSpaceAttribute = "Namespace";
        private const string DateAddedAttribute = "DateAdded";
        private const string DateModifiedAttribute = "DateModified";
        private const string ResourceNameAttribute = "Name";
        private const string ResourceTypeIdAttribute = "ResourceTypeId";
        private const int MaxRetryCount = 10;

        private readonly BackgroundWorker publisherThread;
        private readonly AutoResetEvent resetEvent;

        private bool disposed;
        private PublishingServiceClient publishingProxy;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePublisher"/> class.
        /// </summary>
        internal MessagePublisher()
        {
            resetEvent = new AutoResetEvent(false);
            publishingProxy = new PublishingServiceClient("WSHttpBinding_IPublishingService");
            publisherThread = new BackgroundWorker();
            publisherThread.WorkerSupportsCancellation = true;
            publisherThread.DoWork += this.WorkerThreadDoWork;
            publisherThread.RunWorkerAsync();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePublisher"/> class.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        internal void PublishMessage(string message)
        {
            // Add the message into the queue.
            //this.messageQueue.Enqueue(message);
            try
            {
                using (var databaseContext = new ZentityAdministrationDataContext(Globals.ZentityConnectionString))
                {
                    MessageQueueRecovery newMessage = new MessageQueueRecovery { Id = Guid.NewGuid(), RawMessage = message };
                    databaseContext.MessageQueueRecoveries.InsertOnSubmit(newMessage);
                    databaseContext.SubmitChanges();
                    this.resetEvent.Set();
                }
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), TraceMessages.ExceptionAddingBrokerMessageToQueue);
            }
        }

        /// <summary>
        /// Performs the cleanup operations.
        /// </summary>
        internal void OnClose()
        {
            this.publisherThread.CancelAsync();
            this.publishingProxy.Close();
        }

        /// <summary>
        /// Event handler for DoWork event of BackgroundWorker thread.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="e">DoWorkEventArgs args.</param>
        private void WorkerThreadDoWork(object sender, DoWorkEventArgs e)
        {
            const int QueueMaxRetry = 1;
            
            while (e.Cancel != true)
            {
                int currentMessageQueueCount, currentChangeMessageCount;
                int queueRetryCount = 0;

                while (true)
                {
                    // Check the message counts in the MessageQueue and ChangeMessage tables from the database.
                    using (var databaseContext = new ZentityAdministrationDataContext(Globals.ZentityConnectionString))
                    {
                        databaseContext.ObjectTrackingEnabled = false;
                        currentMessageQueueCount = databaseContext.MessageQueueRecoveries.Count();
                        currentChangeMessageCount = databaseContext.ChangeMessageRecoveries.Count();
                    }

                    if (currentMessageQueueCount != 0 || queueRetryCount >= QueueMaxRetry)
                    {
                        break;
                    }
                    
                    if (queueRetryCount < QueueMaxRetry)
                    {
                        // Wait for some interval before new messages are pumped into the queue.
                        this.resetEvent.Reset();
                        queueRetryCount = this.resetEvent.WaitOne(Worker.RetryInterval) ? 0 : queueRetryCount + 1;
                    }
                }

                // If the top message in the queue is not null then process it, otherwise 
                // push the processed messages to the publishing service.
                if (currentMessageQueueCount > 0)
                {
                    Globals.TraceMessage(TraceEventType.Verbose, string.Empty, string.Format(TraceMessages.ParsingMessageInInternalQueue,currentMessageQueueCount));

                    // Parsing the message from the internal queue
                    int messagesParsed = ParseMessages(currentMessageQueueCount);

                    Globals.TraceMessage(TraceEventType.Verbose, string.Empty, string.Format(TraceMessages.ParsedMessageInInternalQueue, messagesParsed));
                }
                else if (currentChangeMessageCount > 0)
                {
                    try
                    {
                        Globals.TraceMessage(TraceEventType.Verbose, string.Empty, string.Format(TraceMessages.PushingMessagesFromProcessedListToPublishingService, currentChangeMessageCount));
                            
                        // Push the messages into publishing service
                        this.PushMessages(currentChangeMessageCount);

                        Globals.TraceMessage(TraceEventType.Verbose, string.Empty, TraceMessages.PushedMessagesFromProcessedListToPublishingService);
                    }
                    catch (Exception ex)
                    {
                        Globals.TraceMessage(TraceEventType.Error, ex.ToString(), string.Format(TraceMessages.ExceptionPushingMessage, currentChangeMessageCount));
                    }
                }
            }
        }

        /// <summary>
        /// Pushes the messages to the publishing service.
        /// </summary>
        /// <param name="currentChangeMessageCount">The current change message count.</param>
        private void PushMessages(int currentChangeMessageCount)
        {
            if (currentChangeMessageCount <= 0) return;

            using (var databaseContext = new ZentityAdministrationDataContext(Globals.ZentityConnectionString))
            {
                // Fetch the distinct list of resource type names
                //var distinctResourceTypes = databaseContext.ChangeMessageRecoveries.Select(changeMessage => new {changeMessage.DataModelNamespace, changeMessage.ResourceTypeName}).Distinct().ToArray();
                var distinctResourceTypes = (from changeMessage in databaseContext.ChangeMessageRecoveries
                                             group changeMessage by new
                                                                        {
                                                                            changeMessage.DataModelNamespace, 
                                                                            changeMessage.ResourceTypeName
                                                                        }
                                             into groupResType
                                             orderby groupResType.Count()
                                             select groupResType.Key).Distinct().ToArray();

                // Iterate on the resource type names and push the messages for that resource type 
                // to the publishing service.
                foreach (var resourceType in distinctResourceTypes)
                {
                    // Keep retrying to send the messages in the list till it is successfully sent to the publishing service
                    bool flagSucceeded = false;
                    int retryCounter = 0;
                    IQueryable<ChangeMessageRecovery> changeMessageListForResourceType = null;

                    do
                    {
                        try
                        {
                            PublishStatus resourceTypePublishStatus = this.publishingProxy.GetPublishingStatusByResourceType(resourceType.DataModelNamespace, resourceType.ResourceTypeName);
                            int messagesSent;

                            if (resourceTypePublishStatus == null ||
                                resourceTypePublishStatus.CurrentStage == PublishStage.Completed ||
                                resourceTypePublishStatus.CurrentStage == PublishStage.AbortedOnError ||
                                resourceTypePublishStatus.CurrentStage == PublishStage.AbortedOnDemand)
                            {
                                // Fetch all messages of a particular resource type
                                changeMessageListForResourceType = databaseContext.ChangeMessageRecoveries.Where(changeMessage => changeMessage.ResourceTypeName == resourceType.ResourceTypeName &&
                                                                                                                                  changeMessage.DataModelNamespace == resourceType.DataModelNamespace);

                                // Convert the change message list from database into resource change messages for the publishing service
                                var resChangeMessageListForResourceType = changeMessageListForResourceType.ToResourceChangeMessageList();
                                messagesSent = resChangeMessageListForResourceType.Count();

                                // Call the publishing service with the resource change message list.
                                string serializedFilePath = SerializeResourceChangeMessageList(resChangeMessageListForResourceType);
                                if (!string.IsNullOrWhiteSpace(serializedFilePath))
                                {
                                    this.publishingProxy.UpdateCollection(serializedFilePath, resourceType.DataModelNamespace, resourceType.ResourceTypeName, null);
                                    flagSucceeded = true;
                                }
                            }
                            else
                            {
                                Globals.TraceMessage(TraceEventType.Information, string.Empty, string.Format(TraceMessages.PublishingServiceBusyMessagesWillNotBeSent, resourceType));
                                break;
                            }

                            Globals.TraceMessage(TraceEventType.Information, string.Empty, string.Format(TraceMessages.MessagesSentToPublishingService, resourceType, messagesSent));
                        }
                        catch (EndpointNotFoundException ex)
                        {
                            Globals.TraceMessage(
                                TraceEventType.Error, ex.Message, string.Format("{0} {1}",
                                                string.Format(TraceMessages.PublishingServiceEndpointUnavailableWithSuggestion, Worker.RetryInterval.Minutes), 
                                                string.Format(TraceMessages.RetryCount,retryCounter)));
                        }
                        catch (Exception ex)
                        {
                            Globals.TraceMessage(
                               TraceEventType.Error, ex.Message, string.Format("{0} {1}",
                                               string.Format(TraceMessages.ExceptionThrownByPublishingService, Worker.RetryInterval.Minutes),
                                               string.Format(TraceMessages.RetryCount, retryCounter)));
                        }

                        // If the message couldnt be sent to the publishing service then wait and retry.
                        if (!flagSucceeded)
                        {
                            if (retryCounter < MaxRetryCount)
                            {
                                this.publishingProxy = Worker.RecreatePublishingProxy(this.publishingProxy);
                                retryCounter++;
                            }
                            else
                            {
                                // Break the loop for this ResourceType as the max retry count has been reached.
                                // The messages for this ResourceType will not be deleted and will be retried next time.
                                break;
                            }
                        }

                    } while (flagSucceeded == false);

                    // Remove the messages of a particular resource type from the message list.
                    if (flagSucceeded)
                    {
                        databaseContext.ChangeMessageRecoveries.DeleteAllOnSubmit(changeMessageListForResourceType);
                        databaseContext.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);
                    }
                }
            }
        }

        /// <summary>
        /// Parses the message.
        /// </summary>
        /// <param name="currentMessageQueueCount">The current message queue count.</param>
        /// <returns>Number of messages parsed.</returns>
        private static int ParseMessages(int currentMessageQueueCount)
        {
            int messagesParsed = 0;
            if (currentMessageQueueCount == 0)
            {
                return messagesParsed;
            }

            try
            {
                using (var databaseContext = new ZentityAdministrationDataContext(Globals.ZentityConnectionString))
                {
                    var messagesCurrentlyInQueue = databaseContext.MessageQueueRecoveries.Where(message => message.DateTimeStamp <= DateTime.Now).ToList();
                    
                    // Iterate on each raw message from the queue and parse them into ChangeMessage records
                    foreach (var messageFromQueue in messagesCurrentlyInQueue)
                    {
                        if (string.IsNullOrWhiteSpace(messageFromQueue.RawMessage))
                        {
                            continue;
                        }

                        try
                        {
                            // Make it a proper xml. 
                            StringBuilder messageBuilder = new StringBuilder();
                            messageBuilder.Append("<");
                            messageBuilder.Append(MessagesElement);
                            messageBuilder.Append(">");
                            messageBuilder.Append(messageFromQueue.RawMessage);
                            messageBuilder.Append("</");
                            messageBuilder.Append(MessagesElement);
                            messageBuilder.Append(">");
                            XElement messages = XElement.Parse(messageBuilder.ToString());

                            // Create a list of ChangeMessages for the current raw message in the iteration
                            List<ChangeMessageRecovery> changeMessageList = new List<ChangeMessageRecovery>();

                            // Parse each raw message in and add them to the ChangeMessage list.
                            Parallel.ForEach(messages.Elements(ZentityMessageElement), message =>
                            {
                                IEnumerable<ChangeMessageRecovery> parsedChangeMessageList = ParseMessage(message);
                                lock (changeMessageList)
                                {
                                    changeMessageList.AddRange(parsedChangeMessageList);
                                    messagesParsed++;
                                }
                            });

                            if (changeMessageList.Count > 0)
                            {
                                databaseContext.ChangeMessageRecoveries.InsertAllOnSubmit(changeMessageList);
                            }
                        }
                        catch (Exception ex)
                        {
                            Globals.TraceMessage(TraceEventType.Error, ex.ToString(), TraceMessages.ExceptionParsingMessage);
                        }
                    }

                    databaseContext.MessageQueueRecoveries.DeleteAllOnSubmit(messagesCurrentlyInQueue);
                    databaseContext.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);
                }
            }
            catch (Exception exception)
            {
                Globals.TraceMessage(TraceEventType.Error, exception.ToString(), exception.Message);
            }

            return messagesParsed;
        }

        /// <summary>
        /// Parses the raw message.
        /// </summary>
        /// <param name="message">The raw message as string.</param>
        /// <returns>List of <see cref="ChangeMessageRecovery"/>.</returns>
        private static IEnumerable<ChangeMessageRecovery> ParseMessage(XElement message)
        {
            List<ChangeMessageRecovery> changeMessageList = new List<ChangeMessageRecovery>();
            if (message != null)
            {
                string resourceType = message.Element(OperationElement).Value;

                // Retrieve values from the XML.
                foreach (XElement resourceNode in message.Element(ChangedDataElement).Elements(ResourceElement))
                {
                    string resourceId = GetAttributeValue(resourceNode, ResourceIdAttribute);
                    string dataNamespace = GetAttributeValue(resourceNode, NameSpaceAttribute);
                    string dateAddedString = GetAttributeValue(resourceNode, DateAddedAttribute);
                    string dateModifiedString = GetAttributeValue(resourceNode, DateModifiedAttribute);
                    string resourceName = GetAttributeValue(resourceNode, ResourceNameAttribute);
                    string resourceTypeId = GetAttributeValue(resourceNode, ResourceTypeIdAttribute);

                    // Parse the string values to get the required datatype.
                    Guid resourceGuid;
                    Guid.TryParse(resourceId, out resourceGuid);
                    Guid resourceTypeGuid;
                    Guid.TryParse(resourceTypeId, out resourceTypeGuid);
                    DateTime addedDateTime;
                    DateTime.TryParse(dateAddedString, out addedDateTime);
                    DateTime modifiedDateTime;
                    DateTime.TryParse(dateModifiedString, out modifiedDateTime);

                    // Create the model to be saved into the database.
                    ChangeMessageRecovery changeMessage = new ChangeMessageRecovery
                                                              {
                                                                  Id = Guid.NewGuid(),
                                                                  ChangeType = (short) GetChangeType(resourceType),
                                                                  DataModelNamespace = dataNamespace,
                                                                  ResourceId = resourceGuid,
                                                                  DateAdded = addedDateTime,
                                                                  DateModified = modifiedDateTime,
                                                                  ResourceTypeId = resourceTypeGuid,
                                                                  ResourceTypeName = resourceName
                                                              };

                    if (changeMessage.ResourceId != Guid.Empty)
                        changeMessageList.Add(changeMessage);
                }
            }

            return changeMessageList;
        }

        /// <summary>
        /// Get the value of the attribute.
        /// </summary>
        /// <param name="element">The element which contains the attribute.</param>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <returns>The attribute value.</returns>
        private static string GetAttributeValue(XElement element, string attributeName)
        {
            string result = string.Empty;
            XAttribute attribute = element.Attribute(attributeName);
            if (attribute != null)
            {
                result = attribute.Value;
            }

            return result;
        }

        /// <summary>
        /// Gets the database modification type.
        /// </summary>
        /// <param name="type">The type as a string.</param>
        /// <returns>The type as an enum.</returns>
        private static ResourceChangeType GetChangeType(string type)
        {
            ResourceChangeType finalResult = ResourceChangeType.Added;
            switch (type)
            {
                case "D":
                    finalResult = ResourceChangeType.Deleted;
                    break;
                case "U":
                    finalResult = ResourceChangeType.Updated;
                    break;
                default:
                    break;
            }

            return finalResult;
        }

        /// <summary>
        /// Serializes the resource change message list into a temporary file.
        /// </summary>
        /// <param name="resourceChangeMessageList">The resource change message list.</param>
        /// <returns>The serialized file path</returns>
        private static string SerializeResourceChangeMessageList(IEnumerable<ResourceChangeMessage> resourceChangeMessageList)
        {
            try
            {
                string messagesFolder = Path.Combine(Globals.AssemblyLocation, "MessageStorage");
                if (!Directory.Exists(messagesFolder))
                {
                    Directory.CreateDirectory(messagesFolder);
                }
                string serializedFilePath = Path.Combine(messagesFolder, Guid.NewGuid() + ".dat");

                using (FileStream resourceChangeMessageStream = new FileStream(serializedFilePath, FileMode.Create, FileAccess.Write))
                {
                    DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(IEnumerable<ResourceChangeMessage>));
                    dataContractSerializer.WriteObject(resourceChangeMessageStream, resourceChangeMessageList);
                    return serializedFilePath;
                }
            }
            catch (Exception exception)
            {
                Globals.TraceMessage(TraceEventType.Error, exception.InnerException != null ? exception.InnerException.ToString() : exception.ToString(), exception.Message);
            }

            return string.Empty;
        }

        #region IDisposable Members

        /// <summary>
        /// Dispose the publisher.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.publisherThread.Dispose();
                    this.publishingProxy.Close();
                    this.resetEvent.Dispose();
                }
            }

            this.disposed = true;
        }
        #endregion
    }
}
