// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Configuration;

    #region Settings class

    internal static class PlatformSettings
    {
        #region Internal member variables

        private static int maximumHarvestCount = 100;

        private static int resumptionTokenExpirationTime = 30;

        private static string copyrights = "@Copyrights";

        private static string authorName = "Your Name";

        private static string authorEmail = "email@domain.com";

        private static ushort maximumFeedCount = 30;

        private static int cacheExpirationTime = 30;

        private static bool cachingEnabled;

        private static string databaseConnectionString = string.Empty;

        private static int timeoutInterval;

        #endregion

        #region Internal Properties

        #region OAI-PMH

        /// <summary>
        /// Gets or sets the maximum harvest count associated with the OAI-PMH protocol
        /// </summary>
        internal static int MaximumHarvestCount
        {
            get
            {
                return PlatformSettings.maximumHarvestCount;
            }
            set
            {
                PlatformSettings.maximumHarvestCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the resumption token associated with the OAI-PMH protocol
        /// </summary>
        internal static int ResumptionTokenExpirationTimeSpan
        {
            get
            {
                return PlatformSettings.resumptionTokenExpirationTime;
            }
            set
            {
                PlatformSettings.resumptionTokenExpirationTime = value;
            }
        }

        #endregion

        #region Syndication Service

        /// <summary>
        /// Gets or sets the copyrights message associated with the Syndication Service.
        /// </summary>
        internal static string Copyrights
        {
            get
            {
                return PlatformSettings.copyrights;
            }
            set
            {
                PlatformSettings.copyrights = value;
            }
        }

        /// <summary>
        /// Gets or sets the Author Name associated with the Syndication Service.
        /// </summary>
        internal static string AuthorName
        {
            get
            {
                return PlatformSettings.authorName;
            }
            set
            {
                PlatformSettings.authorName = value;
            }
        }

        /// <summary>
        /// Gets or sets the Author Email associated with the Syndication Service.
        /// </summary>
        internal static string AuthorEmail
        {
            get
            {
                return PlatformSettings.authorEmail;
            }
            set
            {
                PlatformSettings.authorEmail = value;
            }
        }

        /// <summary>
        /// Gets or sets the Maximum Feed Count associated with the Syndication Service.
        /// </summary>
        internal static ushort MaximumFeedCount
        {
            get
            {
                return PlatformSettings.maximumFeedCount;
            }
            set
            {
                PlatformSettings.maximumFeedCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the Cache Expiration Time associated with the Syndication Service.
        /// </summary>
        internal static int CacheExpirationTime
        {
            get
            {
                return PlatformSettings.cacheExpirationTime;
            }
            set
            {
                PlatformSettings.cacheExpirationTime = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the caching is enabled or disabled.
        /// </summary>
        internal static bool CachingEnabled
        {
            get
            {
                return PlatformSettings.cachingEnabled;
            }
            set
            {
                PlatformSettings.cachingEnabled = value;
            }
        }

        #endregion

        #region Search Service

        /// <summary>
        /// Gets the ConnectionString associated with the Search Service API's.
        /// </summary>
        internal static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(PlatformSettings.databaseConnectionString))
                {
                    string actualConnectionString = string.Empty;
                    ConnectionStringSettingsCollection collection = ConfigurationManager.ConnectionStrings;
                    foreach (ConnectionStringSettings connection in collection)
                    {
                        if ((!connection.Name.Equals("LocalSqlServer", StringComparison.Ordinal)) && connection.ProviderName.Equals("System.Data.EntityClient", StringComparison.Ordinal))
                        {
                            actualConnectionString = connection.ConnectionString.Split(new string[] { "provider connection string=" }, StringSplitOptions.RemoveEmptyEntries).Last();
                            actualConnectionString = actualConnectionString.Trim('\'').Trim('\"').Trim();
                            break;
                        }
                    }
                    PlatformSettings.databaseConnectionString = actualConnectionString;
                }
                return PlatformSettings.databaseConnectionString;
            }
        }

        /// <summary>
        /// Gets the connection timeout.
        /// </summary>
        /// <value>The connection timeout.</value>
        internal static int ConnectionTimeout
        {
            get
            {
                if (PlatformSettings.timeoutInterval <= 0)
                {
                    bool timeoutSpecified = false;
                    if (PlatformSettings.ConnectionString.Contains("Connection Timeout") ||
                        PlatformSettings.ConnectionString.Contains("Connect Timeout"))
                    {
                        timeoutSpecified = true;
                    }

                    using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(PlatformSettings.ConnectionString))
                    {
                        PlatformSettings.timeoutInterval = connection.ConnectionTimeout;
                        if (!timeoutSpecified && PlatformSettings.timeoutInterval <= 30)
                        {
                            PlatformSettings.timeoutInterval = 300;
                        }
                    }
                }
                return PlatformSettings.timeoutInterval;
            }
        }

        #endregion

        #endregion
    }

    #endregion
}
