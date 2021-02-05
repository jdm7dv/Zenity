// *********************************************************
// 
//     Copyright (c) Microsoft. All rights reserved.
//     This code is licensed under the Apache License, Version 2.0.
//     THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//     ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//     IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//     PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// 
// *********************************************************

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

        XmlDocument ssdl;
        XmlDocument msl;
        List<KeyValuePair<string, XmlDocument>> csdls;

        #endregion

        #region Properties

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
        /// A list of CSDL documents generated per data model module. CSDL document corresponding
        /// to Core is the enhanced Core CSDL with the additional information of all custom 
        /// AssociationSets. Rest of the documents define the custom Resource Type and Association 
        /// for each data model module.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<KeyValuePair<string, XmlDocument>> Csdls
        {
            get { return csdls; }
        }

        #endregion

        #region Constructors

        internal EFArtifactGenerationResults()
        {
            csdls = new List<KeyValuePair<string, XmlDocument>>();
        }

        #endregion
    }
}
