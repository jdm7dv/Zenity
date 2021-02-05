// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using Zentity.Services.Web;

namespace Zentity.Services.ServiceHost
{
    /// <summary>
    /// Web service host.
    /// </summary>
    partial class WebServiceHost : ServiceBase
    {
        private System.ServiceModel.ServiceHost adminConfigServiceHost;
        private System.ServiceModel.ServiceHost configurationServiceHost;
        private System.ServiceModel.ServiceHost publishingServiceHost;
        private System.ServiceModel.ServiceHost dataModelServiceHost;
        private System.ServiceModel.ServiceHost resourceTypeServiceHost;
        private System.ServiceModel.ServiceHost publishingProgressServiceHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceHost"/> class.
        /// </summary>
        public WebServiceHost()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Starts this instance. Only used for interactive debug purposes.
        /// </summary>
        public void Start()
        {
            this.OnStart(null);
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            this.OnStop(false);
            KillAllCreatorApplications();

            // Create a ServiceHost for the each service type and provide the base address.
            adminConfigServiceHost = new System.ServiceModel.ServiceHost(typeof(Web.Admin.ConfigurationService));
            configurationServiceHost = new System.ServiceModel.ServiceHost(typeof(Web.Pivot.ConfigurationService));
            publishingServiceHost = new System.ServiceModel.ServiceHost(typeof(Web.Pivot.PublishingService));
            dataModelServiceHost = new System.ServiceModel.ServiceHost(typeof(Web.Data.DataModelService));
            resourceTypeServiceHost = new System.ServiceModel.ServiceHost(typeof(Web.Data.ResourceTypeService));
            publishingProgressServiceHost = new System.ServiceModel.ServiceHost(typeof(Web.Pivot.PublishingProgressService));

            // Open the ServiceHostBase to create listeners and start 
            // listening for messages.
            adminConfigServiceHost.Open();
            configurationServiceHost.Open();
            publishingServiceHost.Open();
            dataModelServiceHost.Open();
            resourceTypeServiceHost.Open();
            publishingProgressServiceHost.Open();

            Thread initThread = new Thread(InitializeService) {IsBackground = true};
            initThread.Start();
        }
        
        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            this.OnStop(true);
        }

        /// <summary>
        /// Called when the service is stopped.
        /// </summary>
        /// <param name="killRequests">if set to <c>true</c> [kill requests].</param>
        private void OnStop(bool killRequests)
        {
            if (killRequests)
            {
                KillAllPublishRequests();
            }

            CloseServiceHost(adminConfigServiceHost);
            CloseServiceHost(configurationServiceHost);
            CloseServiceHost(publishingServiceHost);
            CloseServiceHost(dataModelServiceHost);
            CloseServiceHost(resourceTypeServiceHost);
            CloseServiceHost(publishingProgressServiceHost);

            adminConfigServiceHost = null;
            configurationServiceHost = null;
            publishingServiceHost = null;
            dataModelServiceHost = null;
            resourceTypeServiceHost = null;
            publishingProgressServiceHost = null;
        }

        /// <summary>
        /// Initializes the service.
        /// </summary>
        private static void InitializeService()
        {
            PublishQueueProcessor.Initialize();
        }

        /// <summary>
        /// Kills all publish requests.
        /// </summary>
        private static void KillAllPublishRequests()
        {
            // Get PublishStatus from RequestTracker where the status is not AbortedOnDemand, AbortedOnError, Completed, NotStarted, PerformingCleanup
            IEnumerable<PublishRequest> publishRequests =
                PublishQueueProcessor.RequestTracker.Where(request => request.CurrentStatus.CurrentStage != PublishStage.AbortedOnDemand ||
                                                                      request.CurrentStatus.CurrentStage != PublishStage.AbortedOnError ||
                                                                      request.CurrentStatus.CurrentStage != PublishStage.Completed ||
                                                                      request.CurrentStatus.CurrentStage != PublishStage.NotStarted ||
                                                                      request.CurrentStatus.CurrentStage != PublishStage.PerformingCleanup);
            if (publishRequests != null && publishRequests.Count() > 0)
            {
                foreach (PublishRequest request in publishRequests)
                {
                    try
                    {
                        if (request.CollectionCreatorProcessId != 0)
                        {
                            try
                            {
                                Process collectionCreator = Process.GetProcessById(request.CollectionCreatorProcessId);
                                collectionCreator.Kill();
                            }
                            catch (ArgumentException)
                            {}
                        }

                        if (request.DeepZoomCreatorProcessId != 0)
                        {
                            try
                            {
                                Process deepZoomCreator = Process.GetProcessById(request.DeepZoomCreatorProcessId);
                                deepZoomCreator.Kill();
                            }
                            catch (ArgumentException)
                            { }
                        }
                    }
                    catch (Exception ex)
                    {
                        Globals.TraceMessage(TraceEventType.Error, ex.ToString(), "Exception during Killing of Running Processes during Shutdown of SerivceHost.");
                    }
                }
            }
        }

        /// <summary>
        /// Kills all running creator applications.
        /// </summary>
        private static void KillAllCreatorApplications()
        {
            foreach (var creatorAppName in Enum.GetNames(typeof(CreatorApplication)))
            {
                IEnumerable<Process> creatorAppProcessList = Process.GetProcessesByName("Zentity.Pivot." + creatorAppName);
                foreach (var creatorAppProcess in creatorAppProcessList)
                {
                    try
                    {
                        creatorAppProcess.Kill();
                    }
                    catch
                    { }
                }
            }

        }

        /// <summary>
        /// Closes the service host.
        /// </summary>
        /// <param name="serviceHost">The service host.</param>
        private static void CloseServiceHost(System.ServiceModel.ServiceHost serviceHost)
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
            }
        }
    }
}
