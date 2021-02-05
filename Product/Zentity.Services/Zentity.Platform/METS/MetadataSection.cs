// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    /// <summary>
    /// Class which represents the administrative metadata and descriptive metadata for a file.
    /// </summary>
    public class MetadataSection
    {
        private Metadata descriptiveMetadata;

        #region .ctor

        private Metadata administrativeMetadata;
        private string fileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataSection"/> class.
        /// </summary>
        internal MetadataSection()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets file name to which metadata is associated.
        /// </summary>
        public string FileName
        {
            get { return fileName; }
            internal set { fileName = value; }
        }

        /// <summary>
        /// Gets the descriptive metadata for a specific file.
        /// </summary>
        public Metadata DescriptiveMetadata
        {
            get { return descriptiveMetadata; }
            internal set { descriptiveMetadata = value; }
        }

        /// <summary>
        /// Gets the administrative section (Rights section) metadata for a specific file.
        /// </summary>
        public Metadata AdministrativeMetadata
        {
            get { return administrativeMetadata; }
            internal set { administrativeMetadata = value; }
        }

        #endregion
    }
}
