// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System;
using System.ServiceProcess;
using System.Threading;

namespace Zentity.Services.Windows
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
                using (QueueListener queueListener = new QueueListener())
                {
                    queueListener.Start();
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            else
            {
                ServiceBase[] servicesToRun = new ServiceBase[]
                                                  {
                                                      new QueueListener()
                                                  };
                ServiceBase.Run(servicesToRun);
            }
        }
    }
}
