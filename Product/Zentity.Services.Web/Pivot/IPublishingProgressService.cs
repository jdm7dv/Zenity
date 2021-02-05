// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Web.Pivot
{
    using System;
    using System.ServiceModel;

    [ServiceContract(CallbackContract=typeof(IPublishingCallback))]
    public interface IPublishingProgressService
    {
        /// <summary>
        /// Reports the start of an operation by an external executable.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="appType">Type of the application.</param>
        /// <param name="processId">The process id.</param>
        [OperationContract(IsOneWay=true)]
        void ReportStart(Guid instanceId, CreatorApplication appType, int processId);

        /// <summary>
        /// Reports the progress of operation and stage completed by an external executable.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="publishStage">The publish stage.</param>
        /// <param name="progressCounter">The progress counter.</param>
        [OperationContract(IsOneWay = true)]
        void ReportProgress(Guid instanceId, PublishStage publishStage, ProgressCounter progressCounter);
    }
}
