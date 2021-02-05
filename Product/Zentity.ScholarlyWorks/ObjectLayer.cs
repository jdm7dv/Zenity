// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

[assembly: global::System.Data.Objects.DataClasses.EdmSchemaAttribute()]
[assembly: global::System.Data.Objects.DataClasses.EdmRelationshipAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkItemHasTag", "ScholarlyWorkItem", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.ScholarlyWorkItem), "Tag", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.Tag))]
[assembly: global::System.Data.Objects.DataClasses.EdmRelationshipAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsAssociatedWithDownload", "ScholarlyWork", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.ScholarlyWork), "Download", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.Download))]
[assembly: global::System.Data.Objects.DataClasses.EdmRelationshipAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkContainerContainsWorks", "ScholarlyWorkContainer", global::System.Data.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(Zentity.ScholarlyWorks.ScholarlyWorkContainer), "ScholarlyWork", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.ScholarlyWork))]
[assembly: global::System.Data.Objects.DataClasses.EdmRelationshipAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkHasContributionBy", "ScholarlyWork", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.ScholarlyWork), "Contact", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.Contact))]
[assembly: global::System.Data.Objects.DataClasses.EdmRelationshipAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsPresentedBy", "ScholarlyWork", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.ScholarlyWork), "Contact", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.Contact))]
[assembly: global::System.Data.Objects.DataClasses.EdmRelationshipAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsEditedBy", "ScholarlyWork", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.ScholarlyWork), "Contact", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.Contact))]
[assembly: global::System.Data.Objects.DataClasses.EdmRelationshipAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkItemIsAddedBy", "ScholarlyWorkItem", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.ScholarlyWorkItem), "Contact", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.Contact))]
[assembly: global::System.Data.Objects.DataClasses.EdmRelationshipAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsAuthoredBy", "ScholarlyWork", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.ScholarlyWork), "Contact", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.Contact))]
[assembly: global::System.Data.Objects.DataClasses.EdmRelationshipAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsAssociatedWithElectronicSource", "ScholarlyWork", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.ScholarlyWork), "ElectronicSource", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.ElectronicSource))]
[assembly: global::System.Data.Objects.DataClasses.EdmRelationshipAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsAssociatedWithMedia", "ScholarlyWork", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.ScholarlyWork), "Media", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.Media))]
[assembly: global::System.Data.Objects.DataClasses.EdmRelationshipAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkHasVersion", "ScholarlyWork1", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.ScholarlyWork), "ScholarlyWork2", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.ScholarlyWork))]
[assembly: global::System.Data.Objects.DataClasses.EdmRelationshipAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsCitedBy", "ScholarlyWork1", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.ScholarlyWork), "ScholarlyWork2", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.ScholarlyWork))]
[assembly: global::System.Data.Objects.DataClasses.EdmRelationshipAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkHasRepresentation", "ScholarlyWork1", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.ScholarlyWork), "ScholarlyWork2", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.ScholarlyWork))]
[assembly: global::System.Data.Objects.DataClasses.EdmRelationshipAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsAssociatedWithPersonalCommunication", "ScholarlyWork", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.ScholarlyWork), "PersonalCommunication", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.PersonalCommunication))]
[assembly: global::System.Data.Objects.DataClasses.EdmRelationshipAttribute("Zentity.ScholarlyWorks", "CategoryNodeHasScholarlyWorkItem", "CategoryNode", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.CategoryNode), "ScholarlyWorkItem", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.ScholarlyWorkItem))]
[assembly: global::System.Data.Objects.DataClasses.EdmRelationshipAttribute("Zentity.ScholarlyWorks", "CategoryNodeHasChildren", "CategoryNode1", global::System.Data.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(Zentity.ScholarlyWorks.CategoryNode), "CategoryNode2", global::System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.CategoryNode))]

// Original file name: 03ed13ba490d43629d48b7c14178ba2b.cs
// Generation date: 4/9/2009 10:06:54 AM
namespace Zentity.ScholarlyWorks
{
    
    /// <summary>
    /// Represents a letter.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Letter")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Letter : PersonalCommunication
    {
        /// <summary>
        /// Create a new Letter object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Letter CreateLetter(global::System.Guid id)
        {
            Letter letter = new Letter();
            letter.Id = id;
            return letter;
        }
    }
    /// <summary>
    /// Represents an image.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Image")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Image : Media
    {
        /// <summary>
        /// Create a new Image object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Image CreateImage(global::System.Guid id)
        {
            Image image = new Image();
            image.Id = id;
            return image;
        }
    }
    /// <summary>
    /// Represents some binary data.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Data")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Data : Download
    {
        /// <summary>
        /// Create a new Data object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Data CreateData(global::System.Guid id)
        {
            Data data = new Data();
            data.Id = id;
            return data;
        }
    }
    /// <summary>
    /// Represents an organization.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Organization")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Organization : Contact
    {
        /// <summary>
        /// Create a new Organization object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Organization CreateOrganization(global::System.Guid id)
        {
            Organization organization = new Organization();
            organization.Id = id;
            return organization;
        }
    }
    /// <summary>
    /// Represents a person.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Person")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Person : Contact
    {
        /// <summary>
        /// Create a new Person object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Person CreatePerson(global::System.Guid id)
        {
            Person person = new Person();
            person.Id = id;
            return person;
        }
        /// <summary>
        /// Gets or sets the last name of this person.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string LastName
        {
            get
            {
                return this._LastName;
            }
            set
            {
                this.OnLastNameChanging(value);
                this.ReportPropertyChanging("LastName");
                this._LastName = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("LastName");
                this.OnLastNameChanged();
            }
        }
        private string _LastName;
        partial void OnLastNameChanging(string value);
        partial void OnLastNameChanged();
        /// <summary>
        /// Gets or sets the first name of this person.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string FirstName
        {
            get
            {
                return this._FirstName;
            }
            set
            {
                this.OnFirstNameChanging(value);
                this.ReportPropertyChanging("FirstName");
                this._FirstName = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("FirstName");
                this.OnFirstNameChanged();
            }
        }
        private string _FirstName;
        partial void OnFirstNameChanging(string value);
        partial void OnFirstNameChanged();
        /// <summary>
        /// Gets or sets the middle name of this person.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string MiddleName
        {
            get
            {
                return this._MiddleName;
            }
            set
            {
                this.OnMiddleNameChanging(value);
                this.ReportPropertyChanging("MiddleName");
                this._MiddleName = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("MiddleName");
                this.OnMiddleNameChanged();
            }
        }
        private string _MiddleName;
        partial void OnMiddleNameChanging(string value);
        partial void OnMiddleNameChanged();
    }
    /// <summary>
    /// Represents a book.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Book")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Book : Publication
    {
        /// <summary>
        /// Create a new Book object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Book CreateBook(global::System.Guid id)
        {
            Book book = new Book();
            book.Id = id;
            return book;
        }
        /// <summary>
        /// Gets or sets the change history of this book. E.g. the datetime information of when this book was created, edited etc.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string ChangeHistory
        {
            get
            {
                return this._ChangeHistory;
            }
            set
            {
                this.OnChangeHistoryChanging(value);
                this.ReportPropertyChanging("ChangeHistory");
                this._ChangeHistory = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("ChangeHistory");
                this.OnChangeHistoryChanged();
            }
        }
        private string _ChangeHistory;
        partial void OnChangeHistoryChanging(string value);
        partial void OnChangeHistoryChanged();
        /// <summary>
        /// Gets or sets the International Standard Book Number of this book.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string ISBN
        {
            get
            {
                return this._ISBN;
            }
            set
            {
                this.OnISBNChanging(value);
                this.ReportPropertyChanging("ISBN");
                this._ISBN = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("ISBN");
                this.OnISBNChanged();
            }
        }
        private string _ISBN;
        partial void OnISBNChanging(string value);
        partial void OnISBNChanged();
    }
    /// <summary>
    /// Represents a software.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Software")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Software : Download
    {
        /// <summary>
        /// Create a new Software object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Software CreateSoftware(global::System.Guid id)
        {
            Software software = new Software();
            software.Id = id;
            return software;
        }
    }
    /// <summary>
    /// Represents a PhD thesis.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="ThesisPhD")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class ThesisPhD : Thesis
    {
        /// <summary>
        /// Create a new ThesisPhD object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static ThesisPhD CreateThesisPhD(global::System.Guid id)
        {
            ThesisPhD thesisPhD = new ThesisPhD();
            thesisPhD.Id = id;
            return thesisPhD;
        }
    }
    /// <summary>
    /// Represents a tag.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Tag")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Tag : ScholarlyWorkItem
    {
        /// <summary>
        /// Create a new Tag object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        /// <param name="name">Initial value of Name.</param>
        public static Tag CreateTag(global::System.Guid id, string name)
        {
            Tag tag = new Tag();
            tag.Id = id;
            tag.Name = name;
            return tag;
        }
        /// <summary>
        /// Gets or sets the name of this tag.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute(IsNullable=false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this.OnNameChanging(value);
                this.ReportPropertyChanging("Name");
                this._Name = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, false);
                this.ReportPropertyChanged("Name");
                this.OnNameChanged();
            }
        }
        private string _Name;
        partial void OnNameChanging(string value);
        partial void OnNameChanged();
        /// <summary>
        /// Gets a collection of related ScholarlyWorkItem objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkItemHasTag", "ScholarlyWorkItem")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ScholarlyWorkItem> ScholarlyWorkItems
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ScholarlyWorkItem>("Zentity.ScholarlyWorks.ScholarlyWorkItemHasTag", "ScholarlyWorkItem");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ScholarlyWorkItem>("Zentity.ScholarlyWorks.ScholarlyWorkItemHasTag", "ScholarlyWorkItem", value);
                }
            }
        }
    }
    /// <summary>
    /// Represents an e-mail.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Email")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Email : PersonalCommunication
    {
        /// <summary>
        /// Create a new Email object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Email CreateEmail(global::System.Guid id)
        {
            Email email = new Email();
            email.Id = id;
            return email;
        }
        /// <summary>
        /// Gets or sets the subject of this e-mail.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Subject
        {
            get
            {
                return this._Subject;
            }
            set
            {
                this.OnSubjectChanging(value);
                this.ReportPropertyChanging("Subject");
                this._Subject = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Subject");
                this.OnSubjectChanged();
            }
        }
        private string _Subject;
        partial void OnSubjectChanging(string value);
        partial void OnSubjectChanged();
    }
    /// <summary>
    /// Represents an audio resource.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Audio")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Audio : Media
    {
        /// <summary>
        /// Create a new Audio object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Audio CreateAudio(global::System.Guid id)
        {
            Audio audio = new Audio();
            audio.Id = id;
            return audio;
        }
        /// <summary>
        /// Gets or sets the codec information for the audio.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Codec
        {
            get
            {
                return this._Codec;
            }
            set
            {
                this.OnCodecChanging(value);
                this.ReportPropertyChanging("Codec");
                this._Codec = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Codec");
                this.OnCodecChanged();
            }
        }
        private string _Codec;
        partial void OnCodecChanging(string value);
        partial void OnCodecChanged();
        /// <summary>
        /// Gets or sets the number of bits transmitted or received per second. E.g. 1.4 Mbit/s for CD quality.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<int> BitRate
        {
            get
            {
                return this._BitRate;
            }
            set
            {
                this.OnBitRateChanging(value);
                this.ReportPropertyChanging("BitRate");
                this._BitRate = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("BitRate");
                this.OnBitRateChanged();
            }
        }
        private global::System.Nullable<int> _BitRate;
        partial void OnBitRateChanging(global::System.Nullable<int> value);
        partial void OnBitRateChanged();
        /// <summary>
        /// Gets or sets the mode of this audio. E.g stereo, 5.1 audio etc.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Mode
        {
            get
            {
                return this._Mode;
            }
            set
            {
                this.OnModeChanging(value);
                this.ReportPropertyChanging("Mode");
                this._Mode = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Mode");
                this.OnModeChanged();
            }
        }
        private string _Mode;
        partial void OnModeChanging(string value);
        partial void OnModeChanged();
    }
    /// <summary>
    /// Represents a thesis.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Thesis")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.ThesisPhD))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.ThesisMsc))]
    public partial class Thesis : Publication
    {
        /// <summary>
        /// Create a new Thesis object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Thesis CreateThesis(global::System.Guid id)
        {
            Thesis thesis = new Thesis();
            thesis.Id = id;
            return thesis;
        }
    }
    /// <summary>
    /// Represents a download.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Download")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Data))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Software))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Code))]
    public partial class Download : ScholarlyWorkItem
    {
        /// <summary>
        /// Create a new Download object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Download CreateDownload(global::System.Guid id)
        {
            Download download = new Download();
            download.Id = id;
            return download;
        }
        /// <summary>
        /// Gets or sets the version information of this download.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string VersionInformation
        {
            get
            {
                return this._VersionInformation;
            }
            set
            {
                this.OnVersionInformationChanging(value);
                this.ReportPropertyChanging("VersionInformation");
                this._VersionInformation = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("VersionInformation");
                this.OnVersionInformationChanged();
            }
        }
        private string _VersionInformation;
        partial void OnVersionInformationChanging(string value);
        partial void OnVersionInformationChanged();
        /// <summary>
        /// Gets or sets the copyright of this download.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Copyright
        {
            get
            {
                return this._Copyright;
            }
            set
            {
                this.OnCopyrightChanging(value);
                this.ReportPropertyChanging("Copyright");
                this._Copyright = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Copyright");
                this.OnCopyrightChanged();
            }
        }
        private string _Copyright;
        partial void OnCopyrightChanging(string value);
        partial void OnCopyrightChanged();
        /// <summary>
        /// Gets or sets the hardware requirements of this download.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string HardwareRequirements
        {
            get
            {
                return this._HardwareRequirements;
            }
            set
            {
                this.OnHardwareRequirementsChanging(value);
                this.ReportPropertyChanging("HardwareRequirements");
                this._HardwareRequirements = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("HardwareRequirements");
                this.OnHardwareRequirementsChanged();
            }
        }
        private string _HardwareRequirements;
        partial void OnHardwareRequirementsChanging(string value);
        partial void OnHardwareRequirementsChanged();
        /// <summary>
        /// Gets or sets the end user license agreement for this download.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string EULA
        {
            get
            {
                return this._EULA;
            }
            set
            {
                this.OnEULAChanging(value);
                this.ReportPropertyChanging("EULA");
                this._EULA = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("EULA");
                this.OnEULAChanged();
            }
        }
        private string _EULA;
        partial void OnEULAChanging(string value);
        partial void OnEULAChanged();
        /// <summary>
        /// Gets or sets the operating system requirements for this download.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string OperatingSystem
        {
            get
            {
                return this._OperatingSystem;
            }
            set
            {
                this.OnOperatingSystemChanging(value);
                this.ReportPropertyChanging("OperatingSystem");
                this._OperatingSystem = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("OperatingSystem");
                this.OnOperatingSystemChanged();
            }
        }
        private string _OperatingSystem;
        partial void OnOperatingSystemChanging(string value);
        partial void OnOperatingSystemChanged();
        /// <summary>
        /// Gets or sets other requirements for this download.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string DownloadRequirements
        {
            get
            {
                return this._DownloadRequirements;
            }
            set
            {
                this.OnDownloadRequirementsChanging(value);
                this.ReportPropertyChanging("DownloadRequirements");
                this._DownloadRequirements = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("DownloadRequirements");
                this.OnDownloadRequirementsChanged();
            }
        }
        private string _DownloadRequirements;
        partial void OnDownloadRequirementsChanging(string value);
        partial void OnDownloadRequirementsChanged();
        /// <summary>
        /// Gets or sets the environment details for this download.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string SystemRequirements
        {
            get
            {
                return this._SystemRequirements;
            }
            set
            {
                this.OnSystemRequirementsChanging(value);
                this.ReportPropertyChanging("SystemRequirements");
                this._SystemRequirements = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("SystemRequirements");
                this.OnSystemRequirementsChanged();
            }
        }
        private string _SystemRequirements;
        partial void OnSystemRequirementsChanging(string value);
        partial void OnSystemRequirementsChanged();
        /// <summary>
        /// Gets or sets the language of this download.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Language
        {
            get
            {
                return this._Language;
            }
            set
            {
                this.OnLanguageChanging(value);
                this.ReportPropertyChanging("Language");
                this._Language = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Language");
                this.OnLanguageChanged();
            }
        }
        private string _Language;
        partial void OnLanguageChanging(string value);
        partial void OnLanguageChanged();
        /// <summary>
        /// Gets or sets the license of this download.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string License
        {
            get
            {
                return this._License;
            }
            set
            {
                this.OnLicenseChanging(value);
                this.ReportPropertyChanging("License");
                this._License = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("License");
                this.OnLicenseChanged();
            }
        }
        private string _License;
        partial void OnLicenseChanging(string value);
        partial void OnLicenseChanged();
        /// <summary>
        /// Gets or sets the copyright date for this download.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<global::System.DateTime> DateCopyrighted
        {
            get
            {
                return this._DateCopyrighted;
            }
            set
            {
                this.OnDateCopyrightedChanging(value);
                this.ReportPropertyChanging("DateCopyrighted");
                this._DateCopyrighted = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("DateCopyrighted");
                this.OnDateCopyrightedChanged();
            }
        }
        private global::System.Nullable<global::System.DateTime> _DateCopyrighted;
        partial void OnDateCopyrightedChanging(global::System.Nullable<global::System.DateTime> value);
        partial void OnDateCopyrightedChanged();
        /// <summary>
        /// Gets a collection of related ScholarlyWork objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsAssociatedWithDownload", "ScholarlyWork")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ScholarlyWork> ScholarlyWorks
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkIsAssociatedWithDownload", "ScholarlyWork");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkIsAssociatedWithDownload", "ScholarlyWork", value);
                }
            }
        }
    }
    /// <summary>
    /// Represents a booklet.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Booklet")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Booklet : Publication
    {
        /// <summary>
        /// Create a new Booklet object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Booklet CreateBooklet(global::System.Guid id)
        {
            Booklet booklet = new Booklet();
            booklet.Id = id;
            return booklet;
        }
    }
    /// <summary>
    /// Represents a chapter.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Chapter")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Chapter : Publication
    {
        /// <summary>
        /// Create a new Chapter object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Chapter CreateChapter(global::System.Guid id)
        {
            Chapter chapter = new Chapter();
            chapter.Id = id;
            return chapter;
        }
    }
    /// <summary>
    /// Represents a piece of code.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Code")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Code : Download
    {
        /// <summary>
        /// Create a new Code object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Code CreateCode(global::System.Guid id)
        {
            Code code = new Code();
            code.Id = id;
            return code;
        }
        /// <summary>
        /// Gets or sets the programming language for this piece of code.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string ProgrammingLanguage
        {
            get
            {
                return this._ProgrammingLanguage;
            }
            set
            {
                this.OnProgrammingLanguageChanging(value);
                this.ReportPropertyChanging("ProgrammingLanguage");
                this._ProgrammingLanguage = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("ProgrammingLanguage");
                this.OnProgrammingLanguageChanged();
            }
        }
        private string _ProgrammingLanguage;
        partial void OnProgrammingLanguageChanging(string value);
        partial void OnProgrammingLanguageChanged();
        /// <summary>
        /// Gets or sets the technology required for this code.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Technology
        {
            get
            {
                return this._Technology;
            }
            set
            {
                this.OnTechnologyChanging(value);
                this.ReportPropertyChanging("Technology");
                this._Technology = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Technology");
                this.OnTechnologyChanged();
            }
        }
        private string _Technology;
        partial void OnTechnologyChanging(string value);
        partial void OnTechnologyChanged();
    }
    /// <summary>
    /// Represents a lecture.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Lecture")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Lecture : ScholarlyWork
    {
        /// <summary>
        /// Create a new Lecture object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Lecture CreateLecture(global::System.Guid id)
        {
            Lecture lecture = new Lecture();
            lecture.Id = id;
            return lecture;
        }
        /// <summary>
        /// Gets or sets the lecture series.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Series
        {
            get
            {
                return this._Series;
            }
            set
            {
                this.OnSeriesChanging(value);
                this.ReportPropertyChanging("Series");
                this._Series = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Series");
                this.OnSeriesChanged();
            }
        }
        private string _Series;
        partial void OnSeriesChanging(string value);
        partial void OnSeriesChanged();
        /// <summary>
        /// Gets or sets an image associated with the lecture.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public byte[] Image
        {
            get
            {
                return global::System.Data.Objects.DataClasses.StructuralObject.GetValidValue(this._Image);
            }
            set
            {
                this.OnImageChanging(value);
                this.ReportPropertyChanging("Image");
                this._Image = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Image");
                this.OnImageChanged();
            }
        }
        private byte[] _Image;
        partial void OnImageChanging(byte[] value);
        partial void OnImageChanged();
        /// <summary>
        /// Gets or sets the lecture audience.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Audience
        {
            get
            {
                return this._Audience;
            }
            set
            {
                this.OnAudienceChanging(value);
                this.ReportPropertyChanging("Audience");
                this._Audience = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Audience");
                this.OnAudienceChanged();
            }
        }
        private string _Audience;
        partial void OnAudienceChanging(string value);
        partial void OnAudienceChanged();
        /// <summary>
        /// Gets or sets the lecture venue.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Venue
        {
            get
            {
                return this._Venue;
            }
            set
            {
                this.OnVenueChanging(value);
                this.ReportPropertyChanging("Venue");
                this._Venue = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Venue");
                this.OnVenueChanged();
            }
        }
        private string _Venue;
        partial void OnVenueChanging(string value);
        partial void OnVenueChanged();
        /// <summary>
        /// Gets or sets the end datetime of the lecture.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<global::System.DateTime> DateEnd
        {
            get
            {
                return this._DateEnd;
            }
            set
            {
                this.OnDateEndChanging(value);
                this.ReportPropertyChanging("DateEnd");
                this._DateEnd = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("DateEnd");
                this.OnDateEndChanged();
            }
        }
        private global::System.Nullable<global::System.DateTime> _DateEnd;
        partial void OnDateEndChanging(global::System.Nullable<global::System.DateTime> value);
        partial void OnDateEndChanged();
        /// <summary>
        /// Gets or sets the start datetime of the lecture.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<global::System.DateTime> DateStart
        {
            get
            {
                return this._DateStart;
            }
            set
            {
                this.OnDateStartChanging(value);
                this.ReportPropertyChanging("DateStart");
                this._DateStart = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("DateStart");
                this.OnDateStartChanged();
            }
        }
        private global::System.Nullable<global::System.DateTime> _DateStart;
        partial void OnDateStartChanging(global::System.Nullable<global::System.DateTime> value);
        partial void OnDateStartChanged();
    }
    /// <summary>
    /// Represents a video.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Video")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Video : Media
    {
        /// <summary>
        /// Create a new Video object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Video CreateVideo(global::System.Guid id)
        {
            Video video = new Video();
            video.Id = id;
            return video;
        }
        /// <summary>
        /// Gets or sets the stereoscopic capabilities of this video. It is a technique of recording three-dimensional visual information.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<bool> Stereoscopic
        {
            get
            {
                return this._Stereoscopic;
            }
            set
            {
                this.OnStereoscopicChanging(value);
                this.ReportPropertyChanging("Stereoscopic");
                this._Stereoscopic = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("Stereoscopic");
                this.OnStereoscopicChanged();
            }
        }
        private global::System.Nullable<bool> _Stereoscopic;
        partial void OnStereoscopicChanging(global::System.Nullable<bool> value);
        partial void OnStereoscopicChanged();
        /// <summary>
        /// Gets or sets the resolution of this video generally expressed in pixels, horizontal and vertical scan lines, voxels etc.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Resolution
        {
            get
            {
                return this._Resolution;
            }
            set
            {
                this.OnResolutionChanging(value);
                this.ReportPropertyChanging("Resolution");
                this._Resolution = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Resolution");
                this.OnResolutionChanged();
            }
        }
        private string _Resolution;
        partial void OnResolutionChanging(string value);
        partial void OnResolutionChanged();
        /// <summary>
        /// Gets or sets the peak signal-to-noise ratio. This is used to judge the video quality.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<decimal> PSNR
        {
            get
            {
                return this._PSNR;
            }
            set
            {
                this.OnPSNRChanging(value);
                this.ReportPropertyChanging("PSNR");
                this._PSNR = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("PSNR");
                this.OnPSNRChanged();
            }
        }
        private global::System.Nullable<decimal> _PSNR;
        partial void OnPSNRChanging(global::System.Nullable<decimal> value);
        partial void OnPSNRChanged();
        /// <summary>
        /// Gets or sets the director of this video.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Director
        {
            get
            {
                return this._Director;
            }
            set
            {
                this.OnDirectorChanging(value);
                this.ReportPropertyChanging("Director");
                this._Director = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Director");
                this.OnDirectorChanged();
            }
        }
        private string _Director;
        partial void OnDirectorChanging(string value);
        partial void OnDirectorChanged();
        /// <summary>
        /// Gets or sets the codec information of this video.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Codec
        {
            get
            {
                return this._Codec;
            }
            set
            {
                this.OnCodecChanging(value);
                this.ReportPropertyChanging("Codec");
                this._Codec = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Codec");
                this.OnCodecChanged();
            }
        }
        private string _Codec;
        partial void OnCodecChanging(string value);
        partial void OnCodecChanged();
        /// <summary>
        /// Gets or sets the rate of information content in a video stream. For example VideoCD, with a bit rate of about 1 Mbit/s, is lower quality than DVD, with a bit rate of about 5 Mbit/s.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<int> BitRate
        {
            get
            {
                return this._BitRate;
            }
            set
            {
                this.OnBitRateChanging(value);
                this.ReportPropertyChanging("BitRate");
                this._BitRate = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("BitRate");
                this.OnBitRateChanged();
            }
        }
        private global::System.Nullable<int> _BitRate;
        partial void OnBitRateChanging(global::System.Nullable<int> value);
        partial void OnBitRateChanged();
        /// <summary>
        /// Gets or sets the aspect ratio of this video.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string AspectRatio
        {
            get
            {
                return this._AspectRatio;
            }
            set
            {
                this.OnAspectRatioChanging(value);
                this.ReportPropertyChanging("AspectRatio");
                this._AspectRatio = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("AspectRatio");
                this.OnAspectRatioChanged();
            }
        }
        private string _AspectRatio;
        partial void OnAspectRatioChanging(string value);
        partial void OnAspectRatioChanged();
        /// <summary>
        /// Gets or sets the frame width of the video stream.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<int> FrameWidth
        {
            get
            {
                return this._FrameWidth;
            }
            set
            {
                this.OnFrameWidthChanging(value);
                this.ReportPropertyChanging("FrameWidth");
                this._FrameWidth = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("FrameWidth");
                this.OnFrameWidthChanged();
            }
        }
        private global::System.Nullable<int> _FrameWidth;
        partial void OnFrameWidthChanging(global::System.Nullable<int> value);
        partial void OnFrameWidthChanged();
        /// <summary>
        /// Gets or sets the frame height of the video stream.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<int> FrameHeight
        {
            get
            {
                return this._FrameHeight;
            }
            set
            {
                this.OnFrameHeightChanging(value);
                this.ReportPropertyChanging("FrameHeight");
                this._FrameHeight = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("FrameHeight");
                this.OnFrameHeightChanged();
            }
        }
        private global::System.Nullable<int> _FrameHeight;
        partial void OnFrameHeightChanging(global::System.Nullable<int> value);
        partial void OnFrameHeightChanged();
        /// <summary>
        /// Gets or sets the number of still pictures per second of video.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<short> FramesPerSecond
        {
            get
            {
                return this._FramesPerSecond;
            }
            set
            {
                this.OnFramesPerSecondChanging(value);
                this.ReportPropertyChanging("FramesPerSecond");
                this._FramesPerSecond = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("FramesPerSecond");
                this.OnFramesPerSecondChanged();
            }
        }
        private global::System.Nullable<short> _FramesPerSecond;
        partial void OnFramesPerSecondChanging(global::System.Nullable<short> value);
        partial void OnFramesPerSecondChanged();
        /// <summary>
        /// Gets or sets the scanning method of the video stream. E.g. Interlaced or Progressive etc.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string ScanningMethod
        {
            get
            {
                return this._ScanningMethod;
            }
            set
            {
                this.OnScanningMethodChanging(value);
                this.ReportPropertyChanging("ScanningMethod");
                this._ScanningMethod = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("ScanningMethod");
                this.OnScanningMethodChanged();
            }
        }
        private string _ScanningMethod;
        partial void OnScanningMethodChanging(string value);
        partial void OnScanningMethodChanged();
        /// <summary>
        /// Gets or sets the number of bits per pixel.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<short> BitsPerPixel
        {
            get
            {
                return this._BitsPerPixel;
            }
            set
            {
                this.OnBitsPerPixelChanging(value);
                this.ReportPropertyChanging("BitsPerPixel");
                this._BitsPerPixel = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("BitsPerPixel");
                this.OnBitsPerPixelChanged();
            }
        }
        private global::System.Nullable<short> _BitsPerPixel;
        partial void OnBitsPerPixelChanging(global::System.Nullable<short> value);
        partial void OnBitsPerPixelChanged();
        /// <summary>
        /// Gets or sets the video color representation. E.g. YIQ, YUV etc.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string ColorModel
        {
            get
            {
                return this._ColorModel;
            }
            set
            {
                this.OnColorModelChanging(value);
                this.ReportPropertyChanging("ColorModel");
                this._ColorModel = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("ColorModel");
                this.OnColorModelChanged();
            }
        }
        private string _ColorModel;
        partial void OnColorModelChanging(string value);
        partial void OnColorModelChanged();
    }
    /// <summary>
    /// Represents a container for scholarly works.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="ScholarlyWorkContainer")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class ScholarlyWorkContainer : ScholarlyWork
    {
        /// <summary>
        /// Create a new ScholarlyWorkContainer object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static ScholarlyWorkContainer CreateScholarlyWorkContainer(global::System.Guid id)
        {
            ScholarlyWorkContainer scholarlyWorkContainer = new ScholarlyWorkContainer();
            scholarlyWorkContainer.Id = id;
            return scholarlyWorkContainer;
        }
        /// <summary>
        /// Gets or sets the count of contained items.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<int> Count
        {
            get
            {
                return this._Count;
            }
            set
            {
                this.OnCountChanging(value);
                this.ReportPropertyChanging("Count");
                this._Count = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("Count");
                this.OnCountChanged();
            }
        }
        private global::System.Nullable<int> _Count;
        partial void OnCountChanging(global::System.Nullable<int> value);
        partial void OnCountChanged();
        /// <summary>
        /// Gets a collection of contained ScholarlyWork objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkContainerContainsWorks", "ScholarlyWork")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ScholarlyWork> ContainedWorks
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkContainerContainsWorks", "ScholarlyWork");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkContainerContainsWorks", "ScholarlyWork", value);
                }
            }
        }
    }
    /// <summary>
    /// Represents a contact.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Contact")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Organization))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Person))]
    public partial class Contact : ScholarlyWorkItem
    {
        /// <summary>
        /// Create a new Contact object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Contact CreateContact(global::System.Guid id)
        {
            Contact contact = new Contact();
            contact.Id = id;
            return contact;
        }
        /// <summary>
        /// Gets or sets the e-mail of this contact.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Email
        {
            get
            {
                return this._Email;
            }
            set
            {
                this.OnEmailChanging(value);
                this.ReportPropertyChanging("Email");
                this._Email = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Email");
                this.OnEmailChanged();
            }
        }
        private string _Email;
        partial void OnEmailChanging(string value);
        partial void OnEmailChanged();
        /// <summary>
        /// Gets a collection of related ScholarlyWork objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkHasContributionBy", "ScholarlyWork")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ScholarlyWork> ContributionInWorks
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkHasContributionBy", "ScholarlyWork");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkHasContributionBy", "ScholarlyWork", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related ScholarlyWork objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsPresentedBy", "ScholarlyWork")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ScholarlyWork> PresentedWorks
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkIsPresentedBy", "ScholarlyWork");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkIsPresentedBy", "ScholarlyWork", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related ScholarlyWork objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsEditedBy", "ScholarlyWork")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ScholarlyWork> EditedWorks
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkIsEditedBy", "ScholarlyWork");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkIsEditedBy", "ScholarlyWork", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related ScholarlyWorkItem objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkItemIsAddedBy", "ScholarlyWorkItem")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ScholarlyWorkItem> AddedItems
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ScholarlyWorkItem>("Zentity.ScholarlyWorks.ScholarlyWorkItemIsAddedBy", "ScholarlyWorkItem");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ScholarlyWorkItem>("Zentity.ScholarlyWorks.ScholarlyWorkItemIsAddedBy", "ScholarlyWorkItem", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related ScholarlyWork objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsAuthoredBy", "ScholarlyWork")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ScholarlyWork> AuthoredWorks
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkIsAuthoredBy", "ScholarlyWork");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkIsAuthoredBy", "ScholarlyWork", value);
                }
            }
        }
    }
    /// <summary>
    /// Represents a proceedings article.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="ProceedingsArticle")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class ProceedingsArticle : Proceedings
    {
        /// <summary>
        /// Create a new ProceedingsArticle object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static ProceedingsArticle CreateProceedingsArticle(global::System.Guid id)
        {
            ProceedingsArticle proceedingsArticle = new ProceedingsArticle();
            proceedingsArticle.Id = id;
            return proceedingsArticle;
        }
    }
    /// <summary>
    /// Represents an Msc thesis.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="ThesisMsc")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class ThesisMsc : Thesis
    {
        /// <summary>
        /// Create a new ThesisMsc object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static ThesisMsc CreateThesisMsc(global::System.Guid id)
        {
            ThesisMsc thesisMsc = new ThesisMsc();
            thesisMsc.Id = id;
            return thesisMsc;
        }
    }
    /// <summary>
    /// Represents a journal.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Journal")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Journal : Publication
    {
        /// <summary>
        /// Create a new Journal object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Journal CreateJournal(global::System.Guid id)
        {
            Journal journal = new Journal();
            journal.Id = id;
            return journal;
        }
        /// <summary>
        /// Gets or sets the journal name of this journal.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string JournalName
        {
            get
            {
                return this._JournalName;
            }
            set
            {
                this.OnJournalNameChanging(value);
                this.ReportPropertyChanging("JournalName");
                this._JournalName = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("JournalName");
                this.OnJournalNameChanged();
            }
        }
        private string _JournalName;
        partial void OnJournalNameChanging(string value);
        partial void OnJournalNameChanged();
    }
    /// <summary>
    /// Represents a scholarly work.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="ScholarlyWork")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Publication))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Lecture))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.ScholarlyWorkContainer))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Experiment))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Tutorial))]
    public partial class ScholarlyWork : ScholarlyWorkItem
    {
        /// <summary>
        /// Create a new ScholarlyWork object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static ScholarlyWork CreateScholarlyWork(global::System.Guid id)
        {
            ScholarlyWork scholarlyWork = new ScholarlyWork();
            scholarlyWork.Id = id;
            return scholarlyWork;
        }
        /// <summary>
        /// Gets or sets the license information of this work.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string License
        {
            get
            {
                return this._License;
            }
            set
            {
                this.OnLicenseChanging(value);
                this.ReportPropertyChanging("License");
                this._License = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("License");
                this.OnLicenseChanged();
            }
        }
        private string _License;
        partial void OnLicenseChanging(string value);
        partial void OnLicenseChanged();
        /// <summary>
        /// Gets or sets the date until this work is valid.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<global::System.DateTime> DateValidUntil
        {
            get
            {
                return this._DateValidUntil;
            }
            set
            {
                this.OnDateValidUntilChanging(value);
                this.ReportPropertyChanging("DateValidUntil");
                this._DateValidUntil = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("DateValidUntil");
                this.OnDateValidUntilChanged();
            }
        }
        private global::System.Nullable<global::System.DateTime> _DateValidUntil;
        partial void OnDateValidUntilChanging(global::System.Nullable<global::System.DateTime> value);
        partial void OnDateValidUntilChanged();
        /// <summary>
        /// Gets or sets additional notes on the scholarly work.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Notes
        {
            get
            {
                return this._Notes;
            }
            set
            {
                this.OnNotesChanging(value);
                this.ReportPropertyChanging("Notes");
                this._Notes = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Notes");
                this.OnNotesChanged();
            }
        }
        private string _Notes;
        partial void OnNotesChanging(string value);
        partial void OnNotesChanged();
        /// <summary>
        /// Gets or sets the abstract of this work.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Abstract
        {
            get
            {
                return this._Abstract;
            }
            set
            {
                this.OnAbstractChanging(value);
                this.ReportPropertyChanging("Abstract");
                this._Abstract = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Abstract");
                this.OnAbstractChanged();
            }
        }
        private string _Abstract;
        partial void OnAbstractChanging(string value);
        partial void OnAbstractChanged();
        /// <summary>
        /// Gets or sets the copyright information of this work.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Copyright
        {
            get
            {
                return this._Copyright;
            }
            set
            {
                this.OnCopyrightChanging(value);
                this.ReportPropertyChanging("Copyright");
                this._Copyright = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Copyright");
                this.OnCopyrightChanged();
            }
        }
        private string _Copyright;
        partial void OnCopyrightChanging(string value);
        partial void OnCopyrightChanged();
        /// <summary>
        /// Gets or sets the date from which this work is valid.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<global::System.DateTime> DateValidFrom
        {
            get
            {
                return this._DateValidFrom;
            }
            set
            {
                this.OnDateValidFromChanging(value);
                this.ReportPropertyChanging("DateValidFrom");
                this._DateValidFrom = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("DateValidFrom");
                this.OnDateValidFromChanged();
            }
        }
        private global::System.Nullable<global::System.DateTime> _DateValidFrom;
        partial void OnDateValidFromChanging(global::System.Nullable<global::System.DateTime> value);
        partial void OnDateValidFromChanged();
        /// <summary>
        /// Gets or sets the language of this work.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Language
        {
            get
            {
                return this._Language;
            }
            set
            {
                this.OnLanguageChanging(value);
                this.ReportPropertyChanging("Language");
                this._Language = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Language");
                this.OnLanguageChanged();
            }
        }
        private string _Language;
        partial void OnLanguageChanging(string value);
        partial void OnLanguageChanged();
        /// <summary>
        /// Gets or sets the date until this resource is available.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<global::System.DateTime> DateAvailableUntil
        {
            get
            {
                return this._DateAvailableUntil;
            }
            set
            {
                this.OnDateAvailableUntilChanging(value);
                this.ReportPropertyChanging("DateAvailableUntil");
                this._DateAvailableUntil = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("DateAvailableUntil");
                this.OnDateAvailableUntilChanged();
            }
        }
        private global::System.Nullable<global::System.DateTime> _DateAvailableUntil;
        partial void OnDateAvailableUntilChanging(global::System.Nullable<global::System.DateTime> value);
        partial void OnDateAvailableUntilChanged();
        /// <summary>
        /// Gets or sets the date from which this work is available.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<global::System.DateTime> DateAvailableFrom
        {
            get
            {
                return this._DateAvailableFrom;
            }
            set
            {
                this.OnDateAvailableFromChanging(value);
                this.ReportPropertyChanging("DateAvailableFrom");
                this._DateAvailableFrom = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("DateAvailableFrom");
                this.OnDateAvailableFromChanged();
            }
        }
        private global::System.Nullable<global::System.DateTime> _DateAvailableFrom;
        partial void OnDateAvailableFromChanging(global::System.Nullable<global::System.DateTime> value);
        partial void OnDateAvailableFromChanged();
        /// <summary>
        /// Gets or sets the copyright date of this work.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<global::System.DateTime> DateCopyrighted
        {
            get
            {
                return this._DateCopyrighted;
            }
            set
            {
                this.OnDateCopyrightedChanging(value);
                this.ReportPropertyChanging("DateCopyrighted");
                this._DateCopyrighted = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("DateCopyrighted");
                this.OnDateCopyrightedChanged();
            }
        }
        private global::System.Nullable<global::System.DateTime> _DateCopyrighted;
        partial void OnDateCopyrightedChanging(global::System.Nullable<global::System.DateTime> value);
        partial void OnDateCopyrightedChanged();
        /// <summary>
        /// Gets a collection of related ElectronicSource objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsAssociatedWithElectronicSource", "ElectronicSource")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ElectronicSource> ElectronicSources
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ElectronicSource>("Zentity.ScholarlyWorks.ScholarlyWorkIsAssociatedWithElectronicSource", "ElectronicSource");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ElectronicSource>("Zentity.ScholarlyWorks.ScholarlyWorkIsAssociatedWithElectronicSource", "ElectronicSource", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related Media objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsAssociatedWithMedia", "Media")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<Media> Medias
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<Media>("Zentity.ScholarlyWorks.ScholarlyWorkIsAssociatedWithMedia", "Media");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<Media>("Zentity.ScholarlyWorks.ScholarlyWorkIsAssociatedWithMedia", "Media", value);
                }
            }
        }
        /// <summary>
        /// Gets or sets the container of this scholarly work.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkContainerContainsWorks", "ScholarlyWorkContainer")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public ScholarlyWorkContainer Container
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedReference<ScholarlyWorkContainer>("Zentity.ScholarlyWorks.ScholarlyWorkContainerContainsWorks", "ScholarlyWorkContainer").Value;
            }
            set
            {
                ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedReference<ScholarlyWorkContainer>("Zentity.ScholarlyWorks.ScholarlyWorkContainerContainsWorks", "ScholarlyWorkContainer").Value = value;
            }
        }
        /// <summary>
        /// Gets or sets the container of this scholarly work.
        /// </summary>
        [global::System.ComponentModel.BrowsableAttribute(false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityReference<ScholarlyWorkContainer> ContainerReference
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedReference<ScholarlyWorkContainer>("Zentity.ScholarlyWorks.ScholarlyWorkContainerContainsWorks", "ScholarlyWorkContainer");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedReference<ScholarlyWorkContainer>("Zentity.ScholarlyWorks.ScholarlyWorkContainerContainsWorks", "ScholarlyWorkContainer", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related Contact objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsEditedBy", "Contact")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<Contact> Editors
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<Contact>("Zentity.ScholarlyWorks.ScholarlyWorkIsEditedBy", "Contact");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<Contact>("Zentity.ScholarlyWorks.ScholarlyWorkIsEditedBy", "Contact", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related ScholarlyWork objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkHasVersion", "ScholarlyWork1")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ScholarlyWork> VersionOf
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkHasVersion", "ScholarlyWork1");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkHasVersion", "ScholarlyWork1", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related Contact objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkHasContributionBy", "Contact")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<Contact> Contributors
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<Contact>("Zentity.ScholarlyWorks.ScholarlyWorkHasContributionBy", "Contact");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<Contact>("Zentity.ScholarlyWorks.ScholarlyWorkHasContributionBy", "Contact", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related ScholarlyWork objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsCitedBy", "ScholarlyWork1")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ScholarlyWork> Cites
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkIsCitedBy", "ScholarlyWork1");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkIsCitedBy", "ScholarlyWork1", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related ScholarlyWork objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsCitedBy", "ScholarlyWork2")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ScholarlyWork> CitedBy
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkIsCitedBy", "ScholarlyWork2");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkIsCitedBy", "ScholarlyWork2", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related ScholarlyWork objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkHasVersion", "ScholarlyWork2")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ScholarlyWork> Versions
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkHasVersion", "ScholarlyWork2");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkHasVersion", "ScholarlyWork2", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related Download objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsAssociatedWithDownload", "Download")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<Download> Downloads
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<Download>("Zentity.ScholarlyWorks.ScholarlyWorkIsAssociatedWithDownload", "Download");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<Download>("Zentity.ScholarlyWorks.ScholarlyWorkIsAssociatedWithDownload", "Download", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related ScholarlyWork objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkHasRepresentation", "ScholarlyWork1")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ScholarlyWork> RepresentationOf
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkHasRepresentation", "ScholarlyWork1");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkHasRepresentation", "ScholarlyWork1", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related Contact objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsAuthoredBy", "Contact")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<Contact> Authors
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<Contact>("Zentity.ScholarlyWorks.ScholarlyWorkIsAuthoredBy", "Contact");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<Contact>("Zentity.ScholarlyWorks.ScholarlyWorkIsAuthoredBy", "Contact", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related PersonalCommunication objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsAssociatedWithPersonalCommunication", "PersonalCommunication")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<PersonalCommunication> PersonalCommunications
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<PersonalCommunication>("Zentity.ScholarlyWorks.ScholarlyWorkIsAssociatedWithPersonalCommunication", "PersonalCommunication");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<PersonalCommunication>("Zentity.ScholarlyWorks.ScholarlyWorkIsAssociatedWithPersonalCommunication", "PersonalCommunication", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related ScholarlyWork objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkHasRepresentation", "ScholarlyWork2")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ScholarlyWork> Representations
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkHasRepresentation", "ScholarlyWork2");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkHasRepresentation", "ScholarlyWork2", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related Contact objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsPresentedBy", "Contact")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<Contact> Presenters
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<Contact>("Zentity.ScholarlyWorks.ScholarlyWorkIsPresentedBy", "Contact");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<Contact>("Zentity.ScholarlyWorks.ScholarlyWorkIsPresentedBy", "Contact", value);
                }
            }
        }
    }
    /// <summary>
    /// Represents a proceedings.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Proceedings")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.ProceedingsArticle))]
    public partial class Proceedings : Publication
    {
        /// <summary>
        /// Create a new Proceedings object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Proceedings CreateProceedings(global::System.Guid id)
        {
            Proceedings proceedings = new Proceedings();
            proceedings.Id = id;
            return proceedings;
        }
        /// <summary>
        /// Gets or sets the event name of this proceedings.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string EventName
        {
            get
            {
                return this._EventName;
            }
            set
            {
                this.OnEventNameChanging(value);
                this.ReportPropertyChanging("EventName");
                this._EventName = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("EventName");
                this.OnEventNameChanged();
            }
        }
        private string _EventName;
        partial void OnEventNameChanging(string value);
        partial void OnEventNameChanged();
    }
    /// <summary>
    /// Represents a patent.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Patent")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Patent : Publication
    {
        /// <summary>
        /// Create a new Patent object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Patent CreatePatent(global::System.Guid id)
        {
            Patent patent = new Patent();
            patent.Id = id;
            return patent;
        }
        /// <summary>
        /// Gets or sets the date on which this patent was approved.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<global::System.DateTime> DateApproved
        {
            get
            {
                return this._DateApproved;
            }
            set
            {
                this.OnDateApprovedChanging(value);
                this.ReportPropertyChanging("DateApproved");
                this._DateApproved = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("DateApproved");
                this.OnDateApprovedChanged();
            }
        }
        private global::System.Nullable<global::System.DateTime> _DateApproved;
        partial void OnDateApprovedChanging(global::System.Nullable<global::System.DateTime> value);
        partial void OnDateApprovedChanged();
        /// <summary>
        /// Gets or sets the date on which this patent was rejected.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<global::System.DateTime> DateRejected
        {
            get
            {
                return this._DateRejected;
            }
            set
            {
                this.OnDateRejectedChanging(value);
                this.ReportPropertyChanging("DateRejected");
                this._DateRejected = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("DateRejected");
                this.OnDateRejectedChanged();
            }
        }
        private global::System.Nullable<global::System.DateTime> _DateRejected;
        partial void OnDateRejectedChanging(global::System.Nullable<global::System.DateTime> value);
        partial void OnDateRejectedChanged();
    }
    /// <summary>
    /// Represents a personal communication.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="PersonalCommunication")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Letter))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Email))]
    public partial class PersonalCommunication : ScholarlyWorkItem
    {
        /// <summary>
        /// Create a new PersonalCommunication object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static PersonalCommunication CreatePersonalCommunication(global::System.Guid id)
        {
            PersonalCommunication personalCommunication = new PersonalCommunication();
            personalCommunication.Id = id;
            return personalCommunication;
        }
        /// <summary>
        /// Gets or sets the source of the communication.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string From
        {
            get
            {
                return this._From;
            }
            set
            {
                this.OnFromChanging(value);
                this.ReportPropertyChanging("From");
                this._From = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("From");
                this.OnFromChanged();
            }
        }
        private string _From;
        partial void OnFromChanging(string value);
        partial void OnFromChanged();
        /// <summary>
        /// Gets or sets the datetime of communication exchange.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<global::System.DateTime> DateExchanged
        {
            get
            {
                return this._DateExchanged;
            }
            set
            {
                this.OnDateExchangedChanging(value);
                this.ReportPropertyChanging("DateExchanged");
                this._DateExchanged = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("DateExchanged");
                this.OnDateExchangedChanged();
            }
        }
        private global::System.Nullable<global::System.DateTime> _DateExchanged;
        partial void OnDateExchangedChanging(global::System.Nullable<global::System.DateTime> value);
        partial void OnDateExchangedChanged();
        /// <summary>
        /// Gets or sets the target of the communication.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string To
        {
            get
            {
                return this._To;
            }
            set
            {
                this.OnToChanging(value);
                this.ReportPropertyChanging("To");
                this._To = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("To");
                this.OnToChanged();
            }
        }
        private string _To;
        partial void OnToChanging(string value);
        partial void OnToChanged();
        /// <summary>
        /// Gets a collection of related ScholarlyWork objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsAssociatedWithPersonalCommunication", "ScholarlyWork")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ScholarlyWork> ScholarlyWorks
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkIsAssociatedWithPersonalCommunication", "ScholarlyWork");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkIsAssociatedWithPersonalCommunication", "ScholarlyWork", value);
                }
            }
        }
    }
    /// <summary>
    /// Represents the base resource type for all the types in ScholarlyWorks module.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="ScholarlyWorkItem")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.PersonalCommunication))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Media))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Download))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Contact))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.ScholarlyWork))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Tag))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.ElectronicSource))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.CategoryNode))]
    public partial class ScholarlyWorkItem : Zentity.Core.Resource
    {
        /// <summary>
        /// Create a new ScholarlyWorkItem object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static ScholarlyWorkItem CreateScholarlyWorkItem(global::System.Guid id)
        {
            ScholarlyWorkItem scholarlyWorkItem = new ScholarlyWorkItem();
            scholarlyWorkItem.Id = id;
            return scholarlyWorkItem;
        }
        /// <summary>
        /// Gets or sets the scope of this item. E.g. internal, external, public etc.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Scope
        {
            get
            {
                return this._Scope;
            }
            set
            {
                this.OnScopeChanging(value);
                this.ReportPropertyChanging("Scope");
                this._Scope = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Scope");
                this.OnScopeChanged();
            }
        }
        private string _Scope;
        partial void OnScopeChanging(string value);
        partial void OnScopeChanged();
        /// <summary>
        /// Gets a collection of related CategoryNode objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "CategoryNodeHasScholarlyWorkItem", "CategoryNode")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<CategoryNode> CategoryNodes
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<CategoryNode>("Zentity.ScholarlyWorks.CategoryNodeHasScholarlyWorkItem", "CategoryNode");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<CategoryNode>("Zentity.ScholarlyWorks.CategoryNodeHasScholarlyWorkItem", "CategoryNode", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related Tag objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkItemHasTag", "Tag")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<Tag> Tags
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<Tag>("Zentity.ScholarlyWorks.ScholarlyWorkItemHasTag", "Tag");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<Tag>("Zentity.ScholarlyWorks.ScholarlyWorkItemHasTag", "Tag", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related Contact objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkItemIsAddedBy", "Contact")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<Contact> AddedBy
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<Contact>("Zentity.ScholarlyWorks.ScholarlyWorkItemIsAddedBy", "Contact");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<Contact>("Zentity.ScholarlyWorks.ScholarlyWorkItemIsAddedBy", "Contact", value);
                }
            }
        }
    }
    /// <summary>
    /// Represents an experiment.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Experiment")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Experiment : ScholarlyWork
    {
        /// <summary>
        /// Create a new Experiment object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Experiment CreateExperiment(global::System.Guid id)
        {
            Experiment experiment = new Experiment();
            experiment.Id = id;
            return experiment;
        }
        /// <summary>
        /// Gets or sets the experiment report.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Report
        {
            get
            {
                return this._Report;
            }
            set
            {
                this.OnReportChanging(value);
                this.ReportPropertyChanging("Report");
                this._Report = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Report");
                this.OnReportChanged();
            }
        }
        private string _Report;
        partial void OnReportChanging(string value);
        partial void OnReportChanged();
        /// <summary>
        /// Gets or sets the status of this experiment.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Status
        {
            get
            {
                return this._Status;
            }
            set
            {
                this.OnStatusChanging(value);
                this.ReportPropertyChanging("Status");
                this._Status = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Status");
                this.OnStatusChanged();
            }
        }
        private string _Status;
        partial void OnStatusChanging(string value);
        partial void OnStatusChanged();
        /// <summary>
        /// Gets or sets the experiment plan.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Plan
        {
            get
            {
                return this._Plan;
            }
            set
            {
                this.OnPlanChanging(value);
                this.ReportPropertyChanging("Plan");
                this._Plan = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Plan");
                this.OnPlanChanged();
            }
        }
        private string _Plan;
        partial void OnPlanChanging(string value);
        partial void OnPlanChanged();
        /// <summary>
        /// Gets or sets the experiment name.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this.OnNameChanging(value);
                this.ReportPropertyChanging("Name");
                this._Name = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Name");
                this.OnNameChanged();
            }
        }
        private string _Name;
        partial void OnNameChanging(string value);
        partial void OnNameChanged();
    }
    /// <summary>
    /// Represents an elecronic source external to the repository.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="ElectronicSource")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class ElectronicSource : ScholarlyWorkItem
    {
        /// <summary>
        /// Create a new ElectronicSource object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static ElectronicSource CreateElectronicSource(global::System.Guid id)
        {
            ElectronicSource electronicSource = new ElectronicSource();
            electronicSource.Id = id;
            return electronicSource;
        }
        /// <summary>
        /// Gets or sets the reference (e.g. URL) of the electronic source.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Reference
        {
            get
            {
                return this._Reference;
            }
            set
            {
                this.OnReferenceChanging(value);
                this.ReportPropertyChanging("Reference");
                this._Reference = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Reference");
                this.OnReferenceChanged();
            }
        }
        private string _Reference;
        partial void OnReferenceChanging(string value);
        partial void OnReferenceChanged();
        /// <summary>
        /// Gets a collection of related ScholarlyWork objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsAssociatedWithElectronicSource", "ScholarlyWork")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ScholarlyWork> ScholarlyWorks
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkIsAssociatedWithElectronicSource", "ScholarlyWork");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkIsAssociatedWithElectronicSource", "ScholarlyWork", value);
                }
            }
        }
    }
    /// <summary>
    /// Represents an unpublished work.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Unpublished")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Unpublished : Publication
    {
        /// <summary>
        /// Create a new Unpublished object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Unpublished CreateUnpublished(global::System.Guid id)
        {
            Unpublished unpublished = new Unpublished();
            unpublished.Id = id;
            return unpublished;
        }
    }
    /// <summary>
    /// Represents a media.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Media")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Image))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Audio))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Video))]
    public partial class Media : ScholarlyWorkItem
    {
        /// <summary>
        /// Create a new Media object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Media CreateMedia(global::System.Guid id)
        {
            Media media = new Media();
            media.Id = id;
            return media;
        }
        /// <summary>
        /// Gets or sets the license information of the media.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string License
        {
            get
            {
                return this._License;
            }
            set
            {
                this.OnLicenseChanging(value);
                this.ReportPropertyChanging("License");
                this._License = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("License");
                this.OnLicenseChanged();
            }
        }
        private string _License;
        partial void OnLicenseChanging(string value);
        partial void OnLicenseChanged();
        /// <summary>
        /// Gets or sets the copyright information of this media.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Copyright
        {
            get
            {
                return this._Copyright;
            }
            set
            {
                this.OnCopyrightChanging(value);
                this.ReportPropertyChanging("Copyright");
                this._Copyright = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Copyright");
                this.OnCopyrightChanged();
            }
        }
        private string _Copyright;
        partial void OnCopyrightChanging(string value);
        partial void OnCopyrightChanged();
        /// <summary>
        /// Gets or sets the copyright datetime of this media.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<global::System.DateTime> DateCopyrighted
        {
            get
            {
                return this._DateCopyrighted;
            }
            set
            {
                this.OnDateCopyrightedChanging(value);
                this.ReportPropertyChanging("DateCopyrighted");
                this._DateCopyrighted = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("DateCopyrighted");
                this.OnDateCopyrightedChanged();
            }
        }
        private global::System.Nullable<global::System.DateTime> _DateCopyrighted;
        partial void OnDateCopyrightedChanging(global::System.Nullable<global::System.DateTime> value);
        partial void OnDateCopyrightedChanged();
        /// <summary>
        /// Gets or sets the duration of this media.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<int> Duration
        {
            get
            {
                return this._Duration;
            }
            set
            {
                this.OnDurationChanging(value);
                this.ReportPropertyChanging("Duration");
                this._Duration = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("Duration");
                this.OnDurationChanged();
            }
        }
        private global::System.Nullable<int> _Duration;
        partial void OnDurationChanging(global::System.Nullable<int> value);
        partial void OnDurationChanged();
        /// <summary>
        /// Gets or sets the language of this media.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Language
        {
            get
            {
                return this._Language;
            }
            set
            {
                this.OnLanguageChanging(value);
                this.ReportPropertyChanging("Language");
                this._Language = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Language");
                this.OnLanguageChanged();
            }
        }
        private string _Language;
        partial void OnLanguageChanging(string value);
        partial void OnLanguageChanged();
        /// <summary>
        /// Gets a collection of related ScholarlyWork objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "ScholarlyWorkIsAssociatedWithMedia", "ScholarlyWork")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ScholarlyWork> ScholarlyWorks
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkIsAssociatedWithMedia", "ScholarlyWork");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ScholarlyWork>("Zentity.ScholarlyWorks.ScholarlyWorkIsAssociatedWithMedia", "ScholarlyWork", value);
                }
            }
        }
    }
    /// <summary>
    /// Represents a category node. Nodes can be used to formulate hierarchies based on subject area, departments etc.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="CategoryNode")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class CategoryNode : ScholarlyWorkItem
    {
        /// <summary>
        /// Create a new CategoryNode object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static CategoryNode CreateCategoryNode(global::System.Guid id)
        {
            CategoryNode categoryNode = new CategoryNode();
            categoryNode.Id = id;
            return categoryNode;
        }
        /// <summary>
        /// Gets a collection of related ScholarlyWorkItem objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "CategoryNodeHasScholarlyWorkItem", "ScholarlyWorkItem")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<ScholarlyWorkItem> ScholarlyWorkItems
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<ScholarlyWorkItem>("Zentity.ScholarlyWorks.CategoryNodeHasScholarlyWorkItem", "ScholarlyWorkItem");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<ScholarlyWorkItem>("Zentity.ScholarlyWorks.CategoryNodeHasScholarlyWorkItem", "ScholarlyWorkItem", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related CategoryNode objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "CategoryNodeHasChildren", "CategoryNode1")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public CategoryNode Parent
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedReference<CategoryNode>("Zentity.ScholarlyWorks.CategoryNodeHasChildren", "CategoryNode1").Value;
            }
            set
            {
                ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedReference<CategoryNode>("Zentity.ScholarlyWorks.CategoryNodeHasChildren", "CategoryNode1").Value = value;
            }
        }
        /// <summary>
        /// Gets a collection of related CategoryNode objects.
        /// </summary>
        [global::System.ComponentModel.BrowsableAttribute(false)]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityReference<CategoryNode> ParentReference
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedReference<CategoryNode>("Zentity.ScholarlyWorks.CategoryNodeHasChildren", "CategoryNode1");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedReference<CategoryNode>("Zentity.ScholarlyWorks.CategoryNodeHasChildren", "CategoryNode1", value);
                }
            }
        }
        /// <summary>
        /// Gets a collection of related CategoryNode objects.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute("Zentity.ScholarlyWorks", "CategoryNodeHasChildren", "CategoryNode2")]
        [global::System.Xml.Serialization.XmlIgnoreAttribute()]
        [global::System.Xml.Serialization.SoapIgnoreAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Data.Objects.DataClasses.EntityCollection<CategoryNode> Children
        {
            get
            {
                return ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.GetRelatedCollection<CategoryNode>("Zentity.ScholarlyWorks.CategoryNodeHasChildren", "CategoryNode2");
            }
            set
            {
                if ((value != null))
                {
                    ((global::System.Data.Objects.DataClasses.IEntityWithRelationships)(this)).RelationshipManager.InitializeRelatedCollection<CategoryNode>("Zentity.ScholarlyWorks.CategoryNodeHasChildren", "CategoryNode2", value);
                }
            }
        }
    }
    /// <summary>
    /// Represents a journal article.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="JournalArticle")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class JournalArticle : Publication
    {
        /// <summary>
        /// Create a new JournalArticle object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static JournalArticle CreateJournalArticle(global::System.Guid id)
        {
            JournalArticle journalArticle = new JournalArticle();
            journalArticle.Id = id;
            return journalArticle;
        }
        /// <summary>
        /// Gets or sets the journal hosting this article.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Journal
        {
            get
            {
                return this._Journal;
            }
            set
            {
                this.OnJournalChanging(value);
                this.ReportPropertyChanging("Journal");
                this._Journal = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Journal");
                this.OnJournalChanged();
            }
        }
        private string _Journal;
        partial void OnJournalChanging(string value);
        partial void OnJournalChanged();
    }
    /// <summary>
    /// Represents a publication.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Publication")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Book))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Thesis))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Booklet))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Chapter))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Proceedings))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Journal))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Patent))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Unpublished))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.JournalArticle))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.TechnicalReport))]
    [global::System.Runtime.Serialization.KnownTypeAttribute(typeof(global::Zentity.ScholarlyWorks.Manual))]
    public partial class Publication : ScholarlyWork
    {
        /// <summary>
        /// Create a new Publication object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Publication CreatePublication(global::System.Guid id)
        {
            Publication publication = new Publication();
            publication.Id = id;
            return publication;
        }
        /// <summary>
        /// Gets or sets the date on which this resource was published.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<global::System.DateTime> DatePublished
        {
            get
            {
                return this._DatePublished;
            }
            set
            {
                this.OnDatePublishedChanging(value);
                this.ReportPropertyChanging("DatePublished");
                this._DatePublished = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("DatePublished");
                this.OnDatePublishedChanged();
            }
        }
        private global::System.Nullable<global::System.DateTime> _DatePublished;
        partial void OnDatePublishedChanging(global::System.Nullable<global::System.DateTime> value);
        partial void OnDatePublishedChanged();
        /// <summary>
        /// Gets or sets the permanent digital object identifier (e.g. issued by CrossRef) of this publication.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string DOI
        {
            get
            {
                return this._DOI;
            }
            set
            {
                this.OnDOIChanging(value);
                this.ReportPropertyChanging("DOI");
                this._DOI = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("DOI");
                this.OnDOIChanged();
            }
        }
        private string _DOI;
        partial void OnDOIChanging(string value);
        partial void OnDOIChanged();
        /// <summary>
        /// Gets or sets the page numbers. E.g. 234-400.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Pages
        {
            get
            {
                return this._Pages;
            }
            set
            {
                this.OnPagesChanging(value);
                this.ReportPropertyChanging("Pages");
                this._Pages = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Pages");
                this.OnPagesChanged();
            }
        }
        private string _Pages;
        partial void OnPagesChanging(string value);
        partial void OnPagesChanged();
        /// <summary>
        /// Gets or sets the institution that was involved in publishing, but not necessarily the publisher.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Institution
        {
            get
            {
                return this._Institution;
            }
            set
            {
                this.OnInstitutionChanging(value);
                this.ReportPropertyChanging("Institution");
                this._Institution = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Institution");
                this.OnInstitutionChanged();
            }
        }
        private string _Institution;
        partial void OnInstitutionChanging(string value);
        partial void OnInstitutionChanged();
        /// <summary>
        /// Gets or sets the date part of DatePublished.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<int> DayPublished
        {
            get
            {
                return this._DayPublished;
            }
            set
            {
                this.OnDayPublishedChanging(value);
                this.ReportPropertyChanging("DayPublished");
                this._DayPublished = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("DayPublished");
                this.OnDayPublishedChanged();
            }
        }
        private global::System.Nullable<int> _DayPublished;
        partial void OnDayPublishedChanging(global::System.Nullable<int> value);
        partial void OnDayPublishedChanged();
        /// <summary>
        /// Gets or sets the publisher&apos;s address.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string PublisherAddress
        {
            get
            {
                return this._PublisherAddress;
            }
            set
            {
                this.OnPublisherAddressChanging(value);
                this.ReportPropertyChanging("PublisherAddress");
                this._PublisherAddress = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("PublisherAddress");
                this.OnPublisherAddressChanged();
            }
        }
        private string _PublisherAddress;
        partial void OnPublisherAddressChanging(string value);
        partial void OnPublisherAddressChanged();
        /// <summary>
        /// Gets or sets the location where the resource is published.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Location
        {
            get
            {
                return this._Location;
            }
            set
            {
                this.OnLocationChanging(value);
                this.ReportPropertyChanging("Location");
                this._Location = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Location");
                this.OnLocationChanged();
            }
        }
        private string _Location;
        partial void OnLocationChanging(string value);
        partial void OnLocationChanged();
        /// <summary>
        /// Gets or sets the title of this publication.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string BookTitle
        {
            get
            {
                return this._BookTitle;
            }
            set
            {
                this.OnBookTitleChanging(value);
                this.ReportPropertyChanging("BookTitle");
                this._BookTitle = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("BookTitle");
                this.OnBookTitleChanged();
            }
        }
        private string _BookTitle;
        partial void OnBookTitleChanging(string value);
        partial void OnBookTitleChanged();
        /// <summary>
        /// Gets or sets the volume of a journal or multi-volume book etc.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Volume
        {
            get
            {
                return this._Volume;
            }
            set
            {
                this.OnVolumeChanging(value);
                this.ReportPropertyChanging("Volume");
                this._Volume = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Volume");
                this.OnVolumeChanged();
            }
        }
        private string _Volume;
        partial void OnVolumeChanging(string value);
        partial void OnVolumeChanged();
        /// <summary>
        /// Gets or sets the library of congress catalog number.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string CatalogNumber
        {
            get
            {
                return this._CatalogNumber;
            }
            set
            {
                this.OnCatalogNumberChanging(value);
                this.ReportPropertyChanging("CatalogNumber");
                this._CatalogNumber = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("CatalogNumber");
                this.OnCatalogNumberChanged();
            }
        }
        private string _CatalogNumber;
        partial void OnCatalogNumberChanging(string value);
        partial void OnCatalogNumberChanged();
        /// <summary>
        /// Gets or sets an identifier to locate the publisher, generally the web-site URL.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string PublisherUri
        {
            get
            {
                return this._PublisherUri;
            }
            set
            {
                this.OnPublisherUriChanging(value);
                this.ReportPropertyChanging("PublisherUri");
                this._PublisherUri = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("PublisherUri");
                this.OnPublisherUriChanged();
            }
        }
        private string _PublisherUri;
        partial void OnPublisherUriChanging(string value);
        partial void OnPublisherUriChanged();
        /// <summary>
        /// Gets or sets the submission date of this publication.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<global::System.DateTime> DateSubmitted
        {
            get
            {
                return this._DateSubmitted;
            }
            set
            {
                this.OnDateSubmittedChanging(value);
                this.ReportPropertyChanging("DateSubmitted");
                this._DateSubmitted = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("DateSubmitted");
                this.OnDateSubmittedChanged();
            }
        }
        private global::System.Nullable<global::System.DateTime> _DateSubmitted;
        partial void OnDateSubmittedChanging(global::System.Nullable<global::System.DateTime> value);
        partial void OnDateSubmittedChanged();
        /// <summary>
        /// Gets or sets the chapter number.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Chapter
        {
            get
            {
                return this._Chapter;
            }
            set
            {
                this.OnChapterChanging(value);
                this.ReportPropertyChanging("Chapter");
                this._Chapter = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Chapter");
                this.OnChapterChanged();
            }
        }
        private string _Chapter;
        partial void OnChapterChanging(string value);
        partial void OnChapterChanged();
        /// <summary>
        /// Gets or sets the number of a journal, magazine, tech-report etc.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Number
        {
            get
            {
                return this._Number;
            }
            set
            {
                this.OnNumberChanging(value);
                this.ReportPropertyChanging("Number");
                this._Number = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Number");
                this.OnNumberChanged();
            }
        }
        private string _Number;
        partial void OnNumberChanging(string value);
        partial void OnNumberChanged();
        /// <summary>
        /// Gets or sets the date of acceptance of the publication.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<global::System.DateTime> DateAccepted
        {
            get
            {
                return this._DateAccepted;
            }
            set
            {
                this.OnDateAcceptedChanging(value);
                this.ReportPropertyChanging("DateAccepted");
                this._DateAccepted = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("DateAccepted");
                this.OnDateAcceptedChanged();
            }
        }
        private global::System.Nullable<global::System.DateTime> _DateAccepted;
        partial void OnDateAcceptedChanging(global::System.Nullable<global::System.DateTime> value);
        partial void OnDateAcceptedChanged();
        /// <summary>
        /// Gets or sets the publisher’s name.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Publisher
        {
            get
            {
                return this._Publisher;
            }
            set
            {
                this.OnPublisherChanging(value);
                this.ReportPropertyChanging("Publisher");
                this._Publisher = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Publisher");
                this.OnPublisherChanged();
            }
        }
        private string _Publisher;
        partial void OnPublisherChanging(string value);
        partial void OnPublisherChanged();
        /// <summary>
        /// Gets or sets the edition of this publication.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Edition
        {
            get
            {
                return this._Edition;
            }
            set
            {
                this.OnEditionChanging(value);
                this.ReportPropertyChanging("Edition");
                this._Edition = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Edition");
                this.OnEditionChanged();
            }
        }
        private string _Edition;
        partial void OnEditionChanging(string value);
        partial void OnEditionChanged();
        /// <summary>
        /// Gets or sets the month part of DatePublished.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<int> MonthPublished
        {
            get
            {
                return this._MonthPublished;
            }
            set
            {
                this.OnMonthPublishedChanging(value);
                this.ReportPropertyChanging("MonthPublished");
                this._MonthPublished = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("MonthPublished");
                this.OnMonthPublishedChanged();
            }
        }
        private global::System.Nullable<int> _MonthPublished;
        partial void OnMonthPublishedChanging(global::System.Nullable<int> value);
        partial void OnMonthPublishedChanged();
        /// <summary>
        /// Gets or sets the year part of DatePublished.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<int> YearPublished
        {
            get
            {
                return this._YearPublished;
            }
            set
            {
                this.OnYearPublishedChanging(value);
                this.ReportPropertyChanging("YearPublished");
                this._YearPublished = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("YearPublished");
                this.OnYearPublishedChanged();
            }
        }
        private global::System.Nullable<int> _YearPublished;
        partial void OnYearPublishedChanging(global::System.Nullable<int> value);
        partial void OnYearPublishedChanged();
        /// <summary>
        /// Gets or sets the publication sponsor information.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Organization
        {
            get
            {
                return this._Organization;
            }
            set
            {
                this.OnOrganizationChanging(value);
                this.ReportPropertyChanging("Organization");
                this._Organization = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Organization");
                this.OnOrganizationChanged();
            }
        }
        private string _Organization;
        partial void OnOrganizationChanging(string value);
        partial void OnOrganizationChanged();
        /// <summary>
        /// Gets or sets the publication series.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Series
        {
            get
            {
                return this._Series;
            }
            set
            {
                this.OnSeriesChanging(value);
                this.ReportPropertyChanging("Series");
                this._Series = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Series");
                this.OnSeriesChanged();
            }
        }
        private string _Series;
        partial void OnSeriesChanging(string value);
        partial void OnSeriesChanged();
    }
    /// <summary>
    /// Represents a technical report.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="TechnicalReport")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class TechnicalReport : Publication
    {
        /// <summary>
        /// Create a new TechnicalReport object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static TechnicalReport CreateTechnicalReport(global::System.Guid id)
        {
            TechnicalReport technicalReport = new TechnicalReport();
            technicalReport.Id = id;
            return technicalReport;
        }
        /// <summary>
        /// Gets or sets the category of this report.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Category
        {
            get
            {
                return this._Category;
            }
            set
            {
                this.OnCategoryChanging(value);
                this.ReportPropertyChanging("Category");
                this._Category = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value, true);
                this.ReportPropertyChanged("Category");
                this.OnCategoryChanged();
            }
        }
        private string _Category;
        partial void OnCategoryChanging(string value);
        partial void OnCategoryChanged();
    }
    /// <summary>
    /// Represents a manual.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Manual")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Manual : Publication
    {
        /// <summary>
        /// Create a new Manual object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Manual CreateManual(global::System.Guid id)
        {
            Manual manual = new Manual();
            manual.Id = id;
            return manual;
        }
    }
    /// <summary>
    /// Represents a tutorial.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::System.Data.Objects.DataClasses.EdmEntityTypeAttribute(NamespaceName="Zentity.ScholarlyWorks", Name="Tutorial")]
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference=true)]
    [global::System.Serializable()]
    public partial class Tutorial : ScholarlyWork
    {
        /// <summary>
        /// Create a new Tutorial object.
        /// </summary>
        /// <param name="id">Initial value of Id.</param>
        public static Tutorial CreateTutorial(global::System.Guid id)
        {
            Tutorial tutorial = new Tutorial();
            tutorial.Id = id;
            return tutorial;
        }
        /// <summary>
        /// Gets or sets the date when this tutorial was presented.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<global::System.DateTime> DatePresented
        {
            get
            {
                return this._DatePresented;
            }
            set
            {
                this.OnDatePresentedChanging(value);
                this.ReportPropertyChanging("DatePresented");
                this._DatePresented = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("DatePresented");
                this.OnDatePresentedChanged();
            }
        }
        private global::System.Nullable<global::System.DateTime> _DatePresented;
        partial void OnDatePresentedChanging(global::System.Nullable<global::System.DateTime> value);
        partial void OnDatePresentedChanged();
        /// <summary>
        /// Gets or sets the length of the tutorial.
        /// </summary>
        [global::System.Data.Objects.DataClasses.EdmScalarPropertyAttribute()]
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public global::System.Nullable<long> Length
        {
            get
            {
                return this._Length;
            }
            set
            {
                this.OnLengthChanging(value);
                this.ReportPropertyChanging("Length");
                this._Length = global::System.Data.Objects.DataClasses.StructuralObject.SetValidValue(value);
                this.ReportPropertyChanged("Length");
                this.OnLengthChanged();
            }
        }
        private global::System.Nullable<long> _Length;
        partial void OnLengthChanging(global::System.Nullable<long> value);
        partial void OnLengthChanged();
    }
}

