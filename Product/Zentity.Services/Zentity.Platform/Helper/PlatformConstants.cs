// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************





using System;
using System.Configuration;

namespace Zentity.Platform
{
    internal static class PlatformConstants
    {
        #region Fields

        internal const string GetRequestType = "GET";
        internal const string PostRequestType = "POST";
        internal const string PutRequestType = "PUT";
        internal const string DeleteRequestType = "DELETE";

        internal const string ZentityAtomPubStoreReader = "ZentityAtomPubStoreReader";
        internal const string ZentityAtomPubStoreWriter = "ZentityAtomPubStoreWriter";

        internal const string ZentitySwordStoreWriter = "ZentitySwordStoreWriter";
        

        #endregion

        /// <summary>
        /// Gets the service host name from the configuration file.
        /// </summary>
        /// <returns>Service host name for the service.</returns>
        internal static string GetServiceHostName()
        {
            string serviceHost = ConfigurationManager.AppSettings["ServiceHost"];
            // If base uri does not contain "/" at the end, then add it.
            if (!serviceHost.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                serviceHost += "/";
            }
            return serviceHost.ToLowerInvariant();
        }
    }
}
