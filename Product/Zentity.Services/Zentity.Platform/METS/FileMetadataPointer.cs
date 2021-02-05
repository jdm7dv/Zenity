// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    /// <summary>
    /// Class which holds the metadata id for a file in METS xml.
    /// </summary>
    internal class FileMetadataPointer
    {
        #region Fields

        private string id;
        private string fileName;
        private string[] rightsIds;
        private string[] descriptiveIds;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the metadata id for a file.
        /// </summary>
        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        /// <summary>
        /// Gets or sets the file name to which metadata id is associated.
        /// </summary>
        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
            }
        }

        /// <summary>
        /// Gets or sets mets:rightsMD ids of specified file.
        /// </summary>
        public string[] RightsIds
        {
            get
            {
                return rightsIds;
            }
            set
            {
                rightsIds = value;
            }
        }

        /// <summary>
        /// Gets or sets mets:dmdSec ids of specified file.
        /// </summary>
        public string[] DescriptiveIds
        {
            get
            {
                return descriptiveIds;
            }
            set
            {
                descriptiveIds = value;
            }
        }

        #endregion
    }
}
