// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using Zentity.DeepZoomCreator.PublishingProgressService;
using Zentity.Services.Web;

namespace Zentity.DeepZoomCreator
{
    /// <summary>
    /// Generates deep zoom images.
    /// </summary>
    internal class ImageGenerator : IPublishingProgressServiceCallback
    {
        #region Private Fields
        
        private Dictionary<Guid, string> deepZoomSourceImageList;
        private readonly string deepZoomFolder;
        private readonly ParallelDeepZoomCreator imageCreator;
        private readonly PublishingProgressServiceClient progressServiceClient;
        
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageGenerator"/> class.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="sourceFile">The source file.</param>
        internal ImageGenerator(Guid instanceId, string sourceFile)
        {
            if (!File.Exists(sourceFile))
            {
                throw new FileNotFoundException(sourceFile);
            }

            // Creating an instance id for the collection helper instance
            this.InstanceId = instanceId;
            Deserialize(sourceFile);

            // Initialize the Publishing Progress Service client
            try
            {
                InstanceContext instanceContext = new InstanceContext(this);
                this.progressServiceClient = new PublishingProgressServiceClient(instanceContext);
                this.progressServiceClient.ReportStart(this.InstanceId, CreatorApplication.DeepZoomCreator, Process.GetCurrentProcess().Id);
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
            }

            deepZoomFolder = Path.Combine(Path.GetDirectoryName(sourceFile), "DeepZoomImages");
            imageCreator = new ParallelDeepZoomCreator
                               {
                                   InstanceId = instanceId,
                                   OutputDirectory = deepZoomFolder, 
                                   ProgressServiceClient = this.progressServiceClient
                               };
        }

        /// <summary>
        /// Gets the instance id.
        /// </summary>
        /// <value>The instance id.</value>
        public Guid InstanceId
        {
            get;
            private set;
        }

        /// <summary>
        /// Generates this instance.
        /// </summary>
        internal void Generate()
        {
            if (deepZoomSourceImageList != null)
            {
                try
                {
                    Globals.TraceMessage(TraceEventType.Information, Properties.Messages.StatusMessage, Properties.Messages.StartingDeepZoomImageProcessing);
                    imageCreator.Reset();
                    imageCreator.Start();
                    imageCreator.TotalImages = deepZoomSourceImageList.Count;

                    foreach(var item in deepZoomSourceImageList)
                    {
                        if (!item.Value.EndsWith(".dzi"))
                            imageCreator.Submit(item.Key, item.Value);
                    }
                 
                    imageCreator.Join();
                    Globals.TraceMessage(TraceEventType.Information, Properties.Messages.StatusMessage, Properties.Messages.CompletedDeepZoomImageProcessing);
                }
                catch (Exception ex)
                {
                    Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
                }
            }
        }

        /// <summary>
        /// Deserializes the specified source file.
        /// </summary>
        /// <param name="sourceFile">The source file.</param>
        private void Deserialize(string sourceFile)
        {
            DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(Dictionary<Guid, string>));
            using (FileStream dictStream = new FileStream(sourceFile, FileMode.Open))
            {
                deepZoomSourceImageList = (Dictionary<Guid, string>) xmlSerializer.ReadObject(dictStream);
            }
        }

        #region IPublishingProgressServiceCallback Members

        /// <summary>
        /// Cancels the operation.
        /// </summary>
        public void CancelOperation()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
