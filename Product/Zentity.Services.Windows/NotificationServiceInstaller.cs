// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;

namespace Zentity.Services.Windows
{
    /// <summary>
    /// Notification service installer.
    /// </summary>
    [RunInstaller(true)]
    public partial class NotificationServiceInstaller : System.Configuration.Install.Installer
    {
        private System.ServiceProcess.ServiceProcessInstaller zentityServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller zentityServiceInstaller;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationServiceInstaller"/> class.
        /// </summary>
        public NotificationServiceInstaller()
        {
            InitializeComponent();
            this.InitalizeServiceInstaller();
        }

        /// <summary>
        /// Initalizes the service installer.
        /// </summary>
        private void InitalizeServiceInstaller()
        {
            this.zentityServiceProcessInstaller = new ServiceProcessInstaller();
            this.zentityServiceInstaller = new  ServiceInstaller();
            // 
            // zentityServiceProcessInstaller
            // 
            this.zentityServiceProcessInstaller.Account = ServiceAccount.User;
            this.zentityServiceProcessInstaller.Password = null;
            this.zentityServiceProcessInstaller.Username = null;
            // 
            // zentityServiceInstaller
            // 
            this.zentityServiceInstaller.ServiceName = "Zentity Notification Service";
            this.zentityServiceInstaller.Description = "Zentity Notification Service is responsible for publishing Pivot collections from Zentity when there are updates to the database.";
            this.zentityServiceInstaller.StartType =  ServiceStartMode.Automatic;
            // 
            // Project Installer
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] 
            { 
                this.zentityServiceProcessInstaller,
                this.zentityServiceInstaller 
            });

        }
    }
}
