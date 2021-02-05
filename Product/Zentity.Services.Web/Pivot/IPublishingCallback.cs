// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Services.Web.Pivot
{
    /// <summary>
    /// IPublishing Callback
    /// </summary>
    interface IPublishingCallback
    {
        /// <summary>
        /// Cancel the cxml creation
        /// </summary>
        [System.ServiceModel.OperationContract(IsOneWay = true)]
        void CancelOperation();
    }
}
