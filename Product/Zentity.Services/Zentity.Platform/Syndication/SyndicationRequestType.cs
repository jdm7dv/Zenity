// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    /// <summary>
    /// Syndication request type.
    /// </summary>
    internal enum SyndicationRequestType
    {
        /// <summary>
        /// Default search.
        /// </summary>
        DefaultSearch,

        /// <summary>
        /// Default search with page number.
        /// </summary>
        DefaultSearchWithPageNo,

        /// <summary>
        /// RSS search.
        /// </summary>
        RSSSearch,

        /// <summary>
        /// RSS search with page number.
        /// </summary>
        RSSSearchWithPageNo,

        /// <summary>
        /// ATOM search.
        /// </summary>
        /// 
        ATOMSearch,
        /// <summary>
        /// ATOM search.
        /// </summary>
        ATOMSearchWithPageNo,

        /// <summary>
        /// Help request.
        /// </summary>
        Help,

        /// <summary>
        /// Unknown request type.
        /// </summary>
        Unknown
    }
}
