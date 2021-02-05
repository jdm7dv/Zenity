// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

namespace Zentity.Services.ServiceHost
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            if (Environment.UserInteractive)
            {
                using (WebServiceHost webServiceHost = new WebServiceHost())
                {
                    webServiceHost.Start();
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            else
            {
                ServiceBase[] servicesToRun = new ServiceBase[]
                                                  {
                                                      new ServiceHost.WebServiceHost()
                                                  };
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
                ServiceBase.Run(servicesToRun);
            }
        }
    }
}
