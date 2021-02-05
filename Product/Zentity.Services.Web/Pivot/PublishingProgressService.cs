// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Web.Pivot
{
    using System;
    using System.Linq;
    using System.ServiceModel;

    public sealed class PublishingProgressService : IPublishingProgressService
    {
        #region IPublishingProgressService Members

        /// <summary>
        /// Reports the start of an operation by an external executable.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="appType">Type of the application.</param>
        /// <param name="processId">The process id.</param>
        void IPublishingProgressService.ReportStart(Guid instanceId, CreatorApplication appType, int processId)
        {
            try
            {
                var publishRequest = PublishQueueProcessor.RequestTracker.Where(request => request.InstanceId == instanceId).FirstOrDefault();
                if (publishRequest != default(PublishRequest))
                {
                    switch (appType)
                    {
                        case CreatorApplication.CollectionCreator:
                            publishRequest.PublishingCallback = OperationContext.Current.GetCallbackChannel<IPublishingCallback>();
                            publishRequest.CollectionCreatorProcessId = processId;
                            break;

                        case CreatorApplication.DeepZoomCreator:
                            publishRequest.DeepZoomCreatorProcessId = processId;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Globals.TraceMessage(System.Diagnostics.TraceEventType.Error, e.ToString(), string.Format(Properties.Messages.ExceptionInReportStart, instanceId, appType));
            }
        }

        /// <summary>
        /// Reports the progress of operation and stage completed by an external executable.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="publishStage">The publish stage.</param>
        /// <param name="progressCounter">The progress counter.</param>
        void IPublishingProgressService.ReportProgress(Guid instanceId, PublishStage publishStage, ProgressCounter progressCounter)
        {
            try
            {
                var publishRequest = PublishQueueProcessor.RequestTracker.Where(request => request.InstanceId == instanceId).FirstOrDefault();
                if (publishRequest != default(PublishRequest))
                {
                    switch (publishStage)
                    {
                        case PublishStage.NotStarted:
                            break;
                        case PublishStage.Initiating:
                            break;
                        case PublishStage.FetchingResourceItems:
                            break;
                        case PublishStage.ProcessingResourceItems:
                            publishRequest.CurrentStatus.ResourceItems = progressCounter;
                            break;
                        case PublishStage.CreatingImages:
                            publishRequest.CurrentStatus.Images = progressCounter;
                            break;
                        case PublishStage.CreatingDeepZoomImages:
                            publishRequest.CurrentStatus.DeepZoomImages = progressCounter;
                            break;
                        case PublishStage.CreatingDeepZoomCollection:
                            break;
                        case PublishStage.DeletingExistingCollection:
                            break;
                        case PublishStage.CopyingNewCollection:
                            break;
                        case PublishStage.CopyingExistingCollection:
                            break;
                        case PublishStage.PerformingCleanup:
                            break;
                        case PublishStage.Completed:
                            publishRequest.PublishingCallback = null;
                            break;
                        case PublishStage.AbortedOnError:
                            publishRequest.PublishingCallback = null;
                            break;
                        case PublishStage.AbortedOnDemand:
                            publishRequest.PublishingCallback = null;
                            break;
                    }

                    if (publishRequest.CurrentStatus.CurrentStage != publishStage)
                    {
                        publishRequest.CurrentStatus.CurrentStage = publishStage;
                        publishRequest.SaveToDatabase(false);
                    }
                }
            }
            catch (Exception e)
            {
                Globals.TraceMessage(System.Diagnostics.TraceEventType.Error, e.ToString(), string.Format(Properties.Messages.ExceptionInReportProgress, instanceId, publishStage));
            }
        }

        #endregion
    }
}
