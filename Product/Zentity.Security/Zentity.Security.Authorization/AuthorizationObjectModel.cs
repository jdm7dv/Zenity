// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

[assembly: global::System.Data.Objects.DataClasses.EdmSchemaAttribute()]
[assembly: global::System.Data.Objects.DataClasses.EdmRelationshipAttribute("Zentity.Security.Authorization", "IdentityBelongsToGroups", "Identity", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.Security.Authorization.Identity), "Group", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.Security.Authorization.Group))]

// Original file name: eb64f76af22e4cb1811a61041e3a3bb2.cs
// Generation date: 4/14/2009 6:23:51 PM
namespace Zentity.Security.Authorization
{
    
    /// <summary>
    /// Represents an Identity.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.Security.Authorization", Name="Identity")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Identity : Zentity.Core.Resource
    {
        /// <summary>
        /// Create a new Identity object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Identity CreateIdentity(global::System.Guid id)
        {
            Identity identity = new Identity();
            identity.Id = id;
            return identity;
        }
        /// <summary>
        /// There are no comments for Property IdentityName in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string IdentityName
        {
            get
            {
                return this._IdentityName;
            }
            set
            {
                this.OnIdentityNameChanging(value);
                this.ReportPropertyChanging("IdentityName");
                this._IdentityName = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("IdentityName");
                this.OnIdentityNameChanged();
            }
        }
        private string _IdentityName;
        partial void OnIdentityNameChanging(string value);
        partial void OnIdentityNameChanged();
        /// <summary>
        /// Gets a collection of related Group objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.Security.Authorization", "IdentityBelongsToGroups", "Group")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<Group> Groups
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<Group>("Zentity.Security.Authorization.IdentityBelongsToGroups", "Group");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<Group>("Zentity.Security.Authorization.IdentityBelongsToGroups", "Group", value);
                }
            }
        }
    }
    /// <summary>
    /// Represents a Group.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.Security.Authorization", Name="Group")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Group : Zentity.Core.Resource
    {
        /// <summary>
        /// Create a new Group object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Group CreateGroup(global::System.Guid id)
        {
            Group group = new Group();
            group.Id = id;
            return group;
        }
        /// <summary>
        /// There are no comments for Property GroupName in the schema.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string GroupName
        {
            get
            {
                return this._GroupName;
            }
            set
            {
                this.OnGroupNameChanging(value);
                this.ReportPropertyChanging("GroupName");
                this._GroupName = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("GroupName");
                this.OnGroupNameChanged();
            }
        }
        private string _GroupName;
        partial void OnGroupNameChanging(string value);
        partial void OnGroupNameChanged();
        /// <summary>
        /// Gets a collection of related Identity objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.Security.Authorization", "IdentityBelongsToGroups", "Identity")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<Identity> Identities
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<Identity>("Zentity.Security.Authorization.IdentityBelongsToGroups", "Identity");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<Identity>("Zentity.Security.Authorization.IdentityBelongsToGroups", "Identity", value);
                }
            }
        }
    }
}

