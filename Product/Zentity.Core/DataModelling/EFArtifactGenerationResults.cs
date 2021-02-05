// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System.Xml;
using System.Collections.Generic;

namespace Zentity.Core
{
    /// <summary>
    /// Represents the results of Entity Framework artifact generation.
    /// </summary>
    public sealed class EFArtifactGenerationResults
    {
        #region Fields

        List<KeyValuePair<string, XmlDocument>> csdls;
        XmlDocument msl;
        XmlDocument ssdl;

        #endregion

        #region Properties

        /// <summary>
        /// A list of CSDL documents generated per data model module. CSDL document corresponding
        /// to Core is the enhanced Core CSDL with the additional information of all custom 
        /// AssociationSets. Rest of the documents define the custom Resource ManagerType and Association 
        /// for each data model module.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<KeyValuePair<string, XmlDocument>> Csdls
        {
            get { return csdls; }
        }

        /// <summary>
        /// The consolidated MSL document containing the mapping information for the complete
        /// application. This includes the Core mappings (e.g. mapping for Core.Resource table) 
        /// and mappings for all the database objects created for custom Resource Types and 
        /// Associations.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        public XmlDocument Msl
        {
            get { return msl; }
            internal set { msl = value; }
        }

        /// <summary>
        /// The consolidated SSDL document containing the database information for the complete
        /// application. This includes the Core database objects (e.g. Core.Resource table) and
        /// all the database objects created for custom Resource Types and Associations.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        public XmlDocument Ssdl
        {
            get { return ssdl; }
            internal set { ssdl = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EFArtifactGenerationResults"/> class.
        /// </summary>
        internal EFArtifactGenerationResults()
        {
            csdls = new List<KeyValuePair<string, XmlDocument>>();
        }

        #endregion
    }
}
