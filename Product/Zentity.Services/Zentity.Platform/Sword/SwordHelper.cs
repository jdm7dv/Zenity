// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Configuration;

    internal class SwordHelper
    {
        /// <summary>
        /// Gets the base uri from the configuration file.
        /// </summary>
        /// <returns>Base uri for the service.</returns>
        internal static string GetBaseUri()
        {
            string baseAddress = ConfigurationManager.AppSettings["SwordBaseUri"];
            // If base uri does not contain "/" at the end, then add it.
            if (!baseAddress.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                baseAddress += "/";
            }

            return PlatformConstants.GetServiceHostName() + baseAddress.ToLowerInvariant();
        }        
    }
}
