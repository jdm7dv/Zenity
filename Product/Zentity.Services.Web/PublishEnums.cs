// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Services.Web
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Publish Operation
    /// </summary>
    [DataContract]
    public enum PublishOperation
    {
        /// <summary>
        /// Create Collection operation.
        /// </summary>
        [EnumMember]
        CreateCollection,

        /// <summary>
        /// Update Collection operation.
        /// </summary>
        [EnumMember]
        UpdateCollection
    }

    /// <summary>
    /// Creator Application
    /// </summary>
    [DataContract]
    public enum CreatorApplication
    {
        /// <summary>
        /// Collection Creator Application
        /// </summary>
        [EnumMember]
        CollectionCreator,

        /// <summary>
        /// DeepZoom Creator Application
        /// </summary>
        [EnumMember]
        DeepZoomCreator
    }

    /// <summary>
    /// Enum values to determine the publishing stage.
    /// </summary>
    [DataContract]
    public enum PublishStage
    {
        /// <summary>
        /// Denotes that the publishing operation has not started
        /// </summary>
        [EnumMember]
        NotStarted = -1,

        /// <summary>
        /// Denotes that publishing operation is in its initial state
        /// </summary>
        [EnumMember]
        Initiating = 0,

        /// <summary>
        /// Denotes that publishing operation is fetching resource records from the database
        /// </summary>
        [EnumMember]
        FetchingResourceItems = 10,

        /// <summary>
        /// Denotes that publishing operation has started processing the resource records.
        /// 1) Create Cxml Item objects and add them to a collection
        /// 2) Create the respective images based on the configuration
        /// </summary>
        [EnumMember]
        ProcessingResourceItems = 15,

        /// <summary>
        /// Publish the intermediate collection with default deepzoom image
        /// </summary>
        [EnumMember]
        PublishIntermediateCollection = 17,

        /// <summary>
        /// Create the images for the collection items. 
        /// </summary>
        [EnumMember]
        CreatingImages = 20,

        /// <summary>
        /// Denotes that publishing operation has spawned a Zentity.Pivot.DeepZoomCreator.exe for the creation of DeepZoom images
        /// </summary>
        [EnumMember]
        CreatingDeepZoomImages = 25,

        /// <summary>
        /// Denotes that publishing operation has started creating the DeepZoom collection
        /// </summary>
        [EnumMember]
        CreatingDeepZoomCollection = 26,

        /// <summary>
        /// Denotes that publishing operation is deleting an existing collection from output folder
        /// </summary>
        [EnumMember]
        DeletingExistingCollection = 30,

        /// <summary>
        /// Denotes that publishing operation is copying the newly generated collection to the output folder
        /// </summary>
        [EnumMember]
        CopyingNewCollection = 35,

        /// <summary>
        /// Denotes that publishing operation is copying the existing collection to the working folder
        /// </summary>
        [EnumMember]
        CopyingExistingCollection = 11,

        /// <summary>
        /// Denotes that publishing operation is deleting the working folders for collection and generated images
        /// </summary>
        [EnumMember]
        PerformingCleanup = 50,

        /// <summary>
        /// Denotes that publishing operation is completed
        /// </summary>
        [EnumMember]
        Completed = 100,

        /// <summary>
        /// Denotes that publishing operation encountered some error that is logged into the service logs
        /// </summary>
        [EnumMember]
        AbortedOnError = 105,

        /// <summary>
        /// Denotes that the publishing operation was cancelled/aborted by the user.
        /// </summary>
        [EnumMember]
        AbortedOnDemand = 110
    }
}
