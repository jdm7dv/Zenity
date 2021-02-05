// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Services.Web
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Class to hold the publishing status updates for the service.
    /// </summary>
    [DataContract]
    public class PublishStatus
    {
        #region Public Properties

        /// <summary>
        /// Gets the Instance Id (Guid)
        /// </summary>
        [DataMember]
        public Guid InstanceId
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the data model namespace.
        /// </summary>
        /// <value>The data model namespace.</value>
        [DataMember]
        public string DataModelNamespace
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the name of the resource type.
        /// </summary>
        /// <value>The name of the resource type.</value>
        [DataMember]
        public string ResourceTypeName
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the total resource items.
        /// </summary>
        /// <value>The total resource items.</value>
        [DataMember]
        public ProgressCounter ResourceItems
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the processed resource items.
        /// </summary>
        /// <value>The processed resource items.</value>
        [DataMember]
        public ProgressCounter Images
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the total deep zoom images.
        /// </summary>
        /// <value>The total deep zoom images.</value>
        [DataMember]
        public ProgressCounter DeepZoomImages
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the start time.
        /// </summary>
        /// <value>The start time.</value>
        [DataMember]
        public DateTime StartTime
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the end time.
        /// </summary>
        /// <value>The end time.</value>
        [DataMember]
        public DateTime EndTime
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the sorted dictionary of each stage start time.
        /// </summary>
        /// <value>The stage start time dictionary.</value>
        [DataMember]
        public IDictionary<PublishStage, DateTime> StageStartTime
        {
            get
            {
                return stageStartTime;
            }
        }

        /// <summary>
        /// Gets the current publishing stage.
        /// </summary>
        /// <value>The current publishing stage.</value>
        [DataMember]
        public PublishStage CurrentStage
        {
            get
            {
                return currentStage;
            }
            internal set
            {
                if (value == PublishStage.Completed ||
                    value == PublishStage.AbortedOnError ||
                    value == PublishStage.AbortedOnDemand)
                {
                    this.EndTime = DateTime.Now;
                }
                else if (value == PublishStage.Initiating)
                {
                    this.StartTime = DateTime.Now;
                }

                if ((currentStage == PublishStage.Completed ||
                     currentStage == PublishStage.AbortedOnError ||
                     currentStage == PublishStage.AbortedOnDemand))
                    return;

                currentStage = value;

                if (!isDeserialization)
                {
                    lock (stageStartTime)
                    {
                        if (stageStartTime.ContainsKey(currentStage))
                        {
                            stageStartTime[currentStage] = DateTime.Now;
                        }
                        else
                        {
                            stageStartTime.Add(currentStage, DateTime.Now);
                        }
                    }
                }

                isDeserialization = false;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishStatus"/> class.
        /// </summary>
        public PublishStatus()
        {
            this.stageStartTime = new SortedDictionary<PublishStage, DateTime>();
            this.CurrentStage = PublishStage.NotStarted;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishStatus"/> class.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        public PublishStatus(Guid instanceId)
            : this()
        {
            this.InstanceId = instanceId;
        }
 
        #endregion

        #region Public Methods

        /// <summary>
        /// Called when the object is being deserialized.
        /// </summary>
        /// <param name="streamingContext">The streaming context.</param>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext streamingContext)
        {
            isDeserialization = true;
            if (this.stageStartTime == null)
            {
                this.stageStartTime = new SortedDictionary<PublishStage, DateTime>();
            }
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Is this instance a deserialized instance.
        /// </summary>
        private bool isDeserialization = false;

        /// <summary>
        /// Current publish stage
        /// </summary>
        private PublishStage currentStage;

        /// <summary>
        /// Dictionary of publish stage and start time.
        /// </summary>
        private IDictionary<PublishStage, DateTime> stageStartTime; 

        #endregion
    }

    /// <summary>
    /// Progress Counter
    /// </summary>
    [DataContract]
    public struct ProgressCounter
    {
        /// <summary>
        /// Gets or sets the Total no of elements.
        /// </summary>
        [DataMember]
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets the no of completed elements.
        /// </summary>
        [DataMember]
        public int Completed { get; set; }
    }
}
