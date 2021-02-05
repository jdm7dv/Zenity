// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Zentity.Services.ServiceHost
{
    /// <summary>
    /// Web service host installer.
    /// </summary>
    [RunInstaller(true)]
    public partial class WebServiceHostInstaller : Installer
    {
        private ServiceProcessInstaller zentityServiceProcessInstaller;
        private ServiceInstaller zentityServiceInstaller;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceHostInstaller"/> class.
        /// </summary>
        public WebServiceHostInstaller()
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
            this.zentityServiceInstaller = new ServiceInstaller();
            // 
            // zentityServiceProcessInstaller
            // 
            this.zentityServiceProcessInstaller.Account = ServiceAccount.User;
            this.zentityServiceProcessInstaller.Password = null;
            this.zentityServiceProcessInstaller.Username = null;
            // 
            // zentityServiceInstaller
            // 
            this.zentityServiceInstaller.ServiceName = "Zentity Services Host";
            this.zentityServiceInstaller.Description = "Zentity Services Host is the hosting service for all Zentity related WCF services.";
            this.zentityServiceInstaller.StartType = ServiceStartMode.Automatic;
            // 
            // Project Installer
            // 
            this.Installers.AddRange(new Installer[] 
            { 
                this.zentityServiceProcessInstaller,
                this.zentityServiceInstaller 
            });
        }
    }
}
