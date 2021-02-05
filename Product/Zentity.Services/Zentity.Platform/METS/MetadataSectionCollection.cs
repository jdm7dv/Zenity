// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// This class represents the collection of MetadataSections.
    /// </summary>
    public class MetadataSectionCollection : IEnumerable<MetadataSection>
    {
        #region Fields

        private MetsDocument metsXmlDocument;
        private string[] files;

        #endregion

        #region .ctor

        /// <summary>
        /// Prevents a default instance of the <see cref="MetadataSectionCollection"/> class from being created.
        /// </summary>
        private MetadataSectionCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataSectionCollection"/> class.
        /// </summary>
        /// <param name="metsDocument">METS document instance.</param>
        internal MetadataSectionCollection(MetsDocument metsDocument)
        {
            this.MetsXmlDocument = metsDocument;
            // Catches all file names from the given MetsDocument.
            if (null != this.MetsXmlDocument.MetadataPointer)
            {
                Files = this.MetsXmlDocument.MetadataPointer.Select(ptr => ptr.FileName).Distinct().ToArray();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets file names in the METS xml.
        /// </summary>
        private string[] Files
        {
            get { return files; }
            set { files = value; }
        }

        /// <summary>
        /// Gets the number of files exists in the METS xml.
        /// </summary>
        public int Count
        {
            get { return this.Files.Count(); }
        }

        /// <summary>
        /// Gets or sets the MetsDocument related to MetadataCollection.
        /// </summary>
        private MetsDocument MetsXmlDocument
        {
            get { return metsXmlDocument; }
            set { metsXmlDocument = value; }
        }

        /// <summary>
        /// Gets the metadata for a file.
        /// </summary>
        /// <param name="fileName">Name of the file to get metadata.</param>
        /// <returns>Metadata for a given file.</returns>
        public MetadataSection this[string fileName]
        {
            get
            {
                return this.MetsXmlDocument.GetMetadata(fileName);
            }
        }

        #endregion

        #region Methods

        #region IEnumerable<MetadataSection> Members

        /// <summary>
        /// Get the enumerator for metadata.
        /// </summary>
        /// <returns>Metadata for all files in the METS xml.</returns>
        public IEnumerator<MetadataSection> GetEnumerator()
        {
            foreach (string fileName in Files)
            {
                yield return this[fileName];
            }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Get the enumerator for metadata.
        /// </summary>
        /// <returns>Metadata for all files in the METS xml.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IEnumerable<MetadataSection> Members

        /// <summary>
        /// Get the enumerator for metadata.
        /// </summary>
        /// <returns>Metadata for all files in the METS xml.</returns>
        IEnumerator<MetadataSection> IEnumerable<MetadataSection>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #endregion
    }
}
