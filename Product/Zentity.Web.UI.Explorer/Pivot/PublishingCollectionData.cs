// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Web.UI.Explorer.Pivot
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    /// <summary>
    /// Collection of PublishingCollectionItem
    /// </summary>
    public class PublishingCollectionItems : Collection<PublishingCollectionItem>
    { 
    }

    /// <summary>
    /// Represents one row in Zentity Dashboard grid
    /// </summary>
    public class PublishingCollectionItem
    {
        /// <summary>
        /// Gets or sets Name of the ResourceType
        /// </summary>
        public string ResourceType { get; set; }

        /// <summary>
        /// Gets or sets Total number of Resources for specified ResourceType in Zentity store
        /// </summary>
        public int NumberOfResources { get; set; }

        /// <summary>
        /// Gets or sets Publishing status of the collection file
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets DataModel of the ResourceType
        /// </summary>
        public string DataModel { get; set; }

        /// <summary>
        /// Gets or sets Collection of Pivot collection files associated with the ResourceType
        /// </summary>
        public List<CollectionData> Collection { get; set; }

        /// <summary>
        /// Gets or sets Last updated timestamp of Published collection
        /// </summary>
        public string LastUpdated { get; set; }

        /// <summary>
        /// Gets or sets Total number of Resources in Zentity store
        /// </summary>
        public int TotalNoOfResources { get; set; }

        /// <summary>
        /// Gets or sets Total number of elements available in published collections for specified ResourceType
        /// </summary>
        public int TotalNoOfElements { get; set; }
    }

    /// <summary>
    /// Represents one Pivot collection file
    /// </summary>
    public class CollectionData
    {
        /// <summary>
        /// Gets or sets Name of the file
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Number of elements within the collection file
        /// </summary>
        public int NumberOfElements { get; set; }

        /// <summary>
        /// Gets or sets Path of the collection file
        /// </summary>
        public string Path { get; set; }
    }
}