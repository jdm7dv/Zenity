// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System.Xml.Linq;

    /// <summary>
    /// Class which holds XML meta data.
    /// </summary>
    internal class XmlMetadata
    {
        #region Fields

        private XElement metadata;
        private string metadataType;
        private bool isDiscriptive;       

        #endregion        

        #region Properties

        /// <summary>
        /// Gets or sets the XML meta data.
        /// </summary>
        public XElement Metadata
        {
            get { return metadata; }
            set { metadata = value; }
        }

        /// <summary>
        /// Gets or sets the meta data type.
        /// </summary>
        public string MdType
        {
            get { return metadataType; }
            set { metadataType = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the metadata is descriptive.
        /// </summary>
        public bool IsDiscriptive
        {
            get { return isDiscriptive; }
            set { isDiscriptive = value; }
        }

        #endregion        
    }
}
