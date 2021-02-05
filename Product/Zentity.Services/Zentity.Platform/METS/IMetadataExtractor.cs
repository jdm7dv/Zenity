// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System.IO;

    /// <summary>
    /// Contains the contracts for extracting the metadata.
    /// </summary>
    interface IMetadataExtractor
    {
        /// <summary>
        /// Extracts the metadata.
        /// </summary>
        /// <param name="inputStream">The input stream.</param>
        /// <returns>The <see cref="Metadata"/>.</returns>
        Metadata ExtractMetadata(Stream inputStream);
    }
}
