// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System.Collections;
    using System.Diagnostics;

    class Namespaces
    {

        public DCTerms _dcterms = new DCTerms("http://purl.org/dc/terms/", "dcterms");
        public FOAFTerms _foafterms = new FOAFTerms("http://xmlns.com/foaf/0.1/", "foaf");
        public ZentityTerms _zentityTerms = new ZentityTerms(OreService.GetBaseUri()+"zentity/terms", "zentity");
        public DCIterms _dciterms = new DCIterms("http://purl.org/eprint/type/", "dciterms");
        public DCMITypes _dcmitypes = new DCMITypes("http://purl.org/dc/dcmitype/", "dcmitype");

        /// <summary>
        /// Initializes a new instance of the <see cref="Namespaces"/> class.
        /// </summary>
        public Namespaces()
        {
        }
    }

    class TermsMgr
    {
        private static TermsMgr termsmgr = new TermsMgr();
        private Hashtable termsMappings = new Hashtable();

        /// <summary>
        /// Prevents a default instance of the <see cref="TermsMgr"/> class from being created.
        /// </summary>
        private TermsMgr()
        {
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <returns>The singleton instance of <see cref="TermsMgr"/>.</returns>
        public static TermsMgr GetInstance()
        {
            return termsmgr;
        }

        /// <summary>
        /// Registers the mapping.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <param name="stdTerm">The STD term.</param>
        /// <returns>
        ///     <c>true</c> if the registration was successful; otherwise <c>false</c>.
        /// </returns>
        public bool RegisterMapping(string term, string stdTerm)
        {

            if (this.termsMappings.Contains(term))
                return false;
            this.termsMappings.Add(term, stdTerm);
            return true;
        }

        /// <summary>
        /// Gets the standard terms.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>The strandard terms.</returns>
        public string GetStandardTerms(string term)
        {

            if (this.termsMappings.Contains(term))
                return this.termsMappings[term] as string;
            return string.Empty;
        }
    }

    class Terms
    {
        public string NamespaceUrl;
        public string TermsNamespace;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terms"/> class.
        /// </summary>
        /// <param name="namespaceUrl">The namespace URL.</param>
        /// <param name="namespacename">The namespacename.</param>
        public Terms(string namespaceUrl, string namespacename)
        {
            this.NamespaceUrl = namespaceUrl;
            this.TermsNamespace = namespacename;
        }

        /// <summary>
        /// Registers the terms.
        /// </summary>
        virtual protected void RegisterTerms()
        {
        }

        /// <summary>
        /// Registers the terms.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <param name="stdTerm">The STD term.</param>
        /// <returns>
        ///     <c>true</c> if the registration was successful; otherwise <c>false</c>.
        /// </returns>
        protected bool RegisterTerms(string term, string stdTerm)
        {
            return TermsMgr.GetInstance().RegisterMapping(term, TermsNamespace + ":" + stdTerm);
        }
    }

    sealed class DCTerms : Terms
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DCTerms"/> class.
        /// </summary>
        /// <param name="namespaceURL">The namespace URL.</param>
        /// <param name="namespacename">The namespacename.</param>
        public DCTerms(string namespaceURL, string namespacename)
            : base(namespaceURL, namespacename)
        {
            this.RegisterTerms();
        }

        /// <summary>
        /// Registers the terms.
        /// </summary>
        override protected void RegisterTerms()
        {
            RegisterTerms("publisher", "publisher");
            RegisterTerms("ScholarlyWorkHasContributionBy", "Contributor");
            RegisterTerms("ScholarlyWorkHasVersion", "isVersionOf");
        }
    }

    sealed class FOAFTerms : Terms
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FOAFTerms"/> class.
        /// </summary>
        /// <param name="namespaceURL">The namespace URL.</param>
        /// <param name="namespacename">The namespacename.</param>
        public FOAFTerms(string namespaceURL, string namespacename)
            : base(namespaceURL, namespacename)
        {
            this.RegisterTerms();
        }

        /// <summary>
        /// Registers the terms.
        /// </summary>
        override protected void RegisterTerms()
        {
            RegisterTerms("type:Zentity.ScholarlyWorks.Person", "Person");
            RegisterTerms("type:Zentity.ScholarlyWorks.Organization", "Organization");
            RegisterTerms("type:Zentity.ScholarlyWorks.Email", "mbox");
            
            RegisterTerms("field:Title", "name");
            RegisterTerms("field:Email", "mbox");
        }
    }

    sealed class ZentityTerms : Terms
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZentityTerms"/> class.
        /// </summary>
        /// <param name="namespaceURL">The namespace URL.</param>
        /// <param name="namespacename">The namespacename.</param>
        public ZentityTerms(string namespaceURL, string namespacename)
            : base(namespaceURL, namespacename)
        {
            this.RegisterTerms();
        }

        /// <summary>
        /// Registers the terms.
        /// </summary>
        override protected void RegisterTerms()
        {
            RegisterTerms("ScholarlyWorkIsEditedBy", "Editor");
            RegisterTerms("ScholarlyWorkIsPresentedBy", "Presenter");
            RegisterTerms("ScholarlyWorkIsAuthoredBy", "Author");
            RegisterTerms("ScholarlyWorkHasRepresentation", "representation");
            RegisterTerms("ScholarlyWorkItemIsAddedBy", "addedBy");
            RegisterTerms("ScholarlyWorkIsCitedBy", "cite"); 
            RegisterTerms("ScholarlyWorkItemHasTag", "tag");
            RegisterTerms("CategoryNodeHasScholarlyWorkItem", "category");
            RegisterTerms("ResourceHasFile", "file");

            RegisterTerms("field:Notes", "notes");
            RegisterTerms("field:Description", "description");
            RegisterTerms("field:Scope", "scope");
            RegisterTerms("field:Uri", "uri");
            RegisterTerms("field:Title", "title");
            RegisterTerms("field:DateAdded", "dateadded");
            RegisterTerms("field:DateModified", "datemodified");
            RegisterTerms("field:DateAvailableFrom", "dateavailablefrom");
            RegisterTerms("field:DateAvailableUntil", "dateavailableuntil");
            RegisterTerms("field:DateModified", "datemodified");
            RegisterTerms("field:DateValidFrom", "datevalidfrom");
            RegisterTerms("field:DateValidUntil", "datevaliduntil");
            RegisterTerms("field:BitRate", "bitrate");
            RegisterTerms("field:Codec", "codec");
            RegisterTerms("field:Mode", "mode");
            RegisterTerms("field:Language", "language");
            RegisterTerms("field:Duration", "duration");
            RegisterTerms("field:Copyright", "copyright");
            RegisterTerms("field:License", "license");
            RegisterTerms("field:DateCopyrighted","datecopyrighted");
            RegisterTerms("field:DownloadRequirements", "downloadrequirements");
            RegisterTerms("field:EULA", "eula");
            RegisterTerms("field:Abstract", "abstract");
            RegisterTerms("field:HardwareRequirements", "hardwarerequirements");
            RegisterTerms("field:OperatingSystem", "operatingsystem");
            RegisterTerms("field:SystemRequirements", "systemrequirements");
            RegisterTerms("field:VersionInformation", "versioninformation");
            RegisterTerms("field:Reference", "reference");
            RegisterTerms("field:Subject", "subject");
            RegisterTerms("field:From", "from");
            RegisterTerms("field:Name", "name");
            RegisterTerms("field:Plan", "plan");
            RegisterTerms("field:Report", "report");
            RegisterTerms("field:Status", "status");
            RegisterTerms("field:FileExtension", "fileextension");
            RegisterTerms("field:Checksum", "checksum");
            RegisterTerms("field:MimeType", "mimetype");
            RegisterTerms("field:Size", "size");
            RegisterTerms("field:JournalName", "journalname");
            RegisterTerms("field:Journal", "journal");
            RegisterTerms("field:Audience", "audience");
            RegisterTerms("field:DateEnd", "dateend");
            RegisterTerms("field:DateStart", "datestart");
            RegisterTerms("field:Venue", "venue");
            RegisterTerms("field:DateApproved", "dateapproved");
            RegisterTerms("field:DateRejected", "daterejected");
            RegisterTerms("field:FirstName", "firstname");
            RegisterTerms("field:LastName", "lastname");
            RegisterTerms("field:MiddleName", "middlename");
            RegisterTerms("field:DateExchanged", "dateexchanged");
            RegisterTerms("field:EventName", "eventname");
            RegisterTerms("field:Category", "category");
            RegisterTerms("field:DatePresented", "datepresented");
            RegisterTerms("field:Length", "length");
            RegisterTerms("field:AspectRatio", "aspectratio");
            RegisterTerms("field:BitsPerPixel", "bitsperpixel");
            RegisterTerms("field:ColorModel", "colormodel");
            RegisterTerms("field:Director", "director");
            RegisterTerms("field:FrameHeight", "frameheight");
            RegisterTerms("field:FrameWidth", "framewidth");
            RegisterTerms("field:FramesPerSecond", "framespersecond");
            RegisterTerms("field:PSNR", "psnr");
            RegisterTerms("field:Resolution", "resolution");
            RegisterTerms("field:ScanningMethod", "scanningmethod");
            RegisterTerms("field:Stereoscopic", "stereoscopic");
            RegisterTerms("field:BookTitle", "booktitle");
            RegisterTerms("field:Chapter", "chapter");
            RegisterTerms("field:CatalogNumber", "catalognumber");
            RegisterTerms("field:DateAccepted", "dateaccepted");
            RegisterTerms("field:DateSubmitted", "datesubmitted");
            RegisterTerms("field:DatePublished", "datepublished");
            RegisterTerms("field:DayPublished", "daypublished");
            RegisterTerms("field:DOI", "doi");
            RegisterTerms("field:Edition", "edition");
            RegisterTerms("field:Institution", "institution");
            RegisterTerms("field:Location", "location");
            RegisterTerms("field:MonthPublished", "monthpublished");
            RegisterTerms("field:Number", "number");
            RegisterTerms("field:Organization", "organization");
            RegisterTerms("field:Pages", "pages");
            RegisterTerms("field:Publisher", "publisher");
            RegisterTerms("field:PublisherAddress", "publisheraddress");
            RegisterTerms("field:PublisherUri", "publisheruri");
            RegisterTerms("field:Series", "series");
            RegisterTerms("field:Volume", "volume");
            RegisterTerms("field:YearPublished", "yearpublished");
            RegisterTerms("field:ISBN", "isbn");
            RegisterTerms("field:ChangeHistory", "changehistory");
            RegisterTerms("field:ProgrammingLanguage", "programminglanguage");
            RegisterTerms("field:Technology", "technology");

            //// Special terms
            RegisterTerms("Tag", "tag");
            RegisterTerms("Category", "category");
            
            //// Resource types
            RegisterTerms("type:Zentity.ScholarlyWorks.Tutorial", "Tutorial");
            RegisterTerms("type:Zentity.ScholarlyWorks.Contact", "Contact");

            RegisterTerms("type:Zentity.Core.Resource", "Resource");
            RegisterTerms("type:Zentity.Core.File", "File");
            RegisterTerms("type:Zentity.ScholarlyWorks.ElectronicSource", "ElectronicSource");
            RegisterTerms("type:Zentity.ScholarlyWorks.Media", "Media");
            RegisterTerms("type:Zentity.ScholarlyWorks.Tag", "Tag");
            RegisterTerms("type:Zentity.ScholarlyWorks.ScholarlyWorkContainer", "ScholarlyWorkContainer");
            RegisterTerms("type:Zentity.ScholarlyWorks.ScholarlyWorkItem", "ScholarlyWorkItem");
            RegisterTerms("type:Zentity.ScholarlyWorks.Video", "Video");
            RegisterTerms("type:Zentity.ScholarlyWorks.Letter", "Letter");
            RegisterTerms("type:Zentity.ScholarlyWorks.PersonalCommunication", "PersonalCommunication");
            RegisterTerms("type:Zentity.ScholarlyWorks.Manual", "Manual");
            RegisterTerms("type:Zentity.ScholarlyWorks.Unpublished", "Unpublished");
            RegisterTerms("type:Zentity.ScholarlyWorks.ProceedingsArticle", "ProceedingsArticle");
            RegisterTerms("type:Zentity.ScholarlyWorks.Proceedings", "Proceedings");
            RegisterTerms("type:Zentity.ScholarlyWorks.Booklet", "Booklet");
            RegisterTerms("type:Zentity.ScholarlyWorks.Publication", "Publication");
            RegisterTerms("type:Zentity.ScholarlyWorks.Experiment", "Experiment");
            RegisterTerms("type:Zentity.ScholarlyWorks.Lecture", "Lecture");
            RegisterTerms("type:Zentity.ScholarlyWorks.Code", "Code");
            RegisterTerms("type:Zentity.ScholarlyWorks.Data", "Data");
            RegisterTerms("type:Zentity.ScholarlyWorks.Download", "Download");
        }
    }

    sealed class DCIterms : Terms
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DCIterms"/> class.
        /// </summary>
        /// <param name="namespaceURL">The namespace URL.</param>
        /// <param name="namespacename">The namespacename.</param>
        public DCIterms(string namespaceURL, string namespacename)
            : base(namespaceURL, namespacename)
        {
            this.RegisterTerms();
        }

        /// <summary>
        /// Registers the terms.
        /// </summary>
        override protected void RegisterTerms()
        {
            RegisterTerms("type:Zentity.ScholarlyWorks.Book", "Book");

            RegisterTerms("type:Zentity.ScholarlyWorks.ScholarlyWork", "ScholarlyText");
            RegisterTerms("type:Zentity.ScholarlyWorks.Journal", "Journal");
            RegisterTerms("type:Zentity.ScholarlyWorks.JournalArticle", "JournalArticle");

            RegisterTerms("type:Zentity.ScholarlyWorks.TechnicalReport", "TechnicalReport");
            RegisterTerms("type:Zentity.ScholarlyWorks.Chapter", "Chapter");

            RegisterTerms("type:Zentity.ScholarlyWorks.Thesis", "Thesis");

            RegisterTerms("type:Zentity.ScholarlyWorks.ThesisPhD", "ThesisPhD");
            RegisterTerms("type:Zentity.ScholarlyWorks.ThesisMsc", "ThesisMsc");
            RegisterTerms("type:Zentity.ScholarlyWorks.Patent", "Patent");
        }
    }

    sealed class DCMITypes : Terms
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DCMITypes"/> class.
        /// </summary>
        /// <param name="namespaceURL">The namespace URL.</param>
        /// <param name="namespacename">The namespacename.</param>
        public DCMITypes(string namespaceURL, string namespacename)
            : base(namespaceURL, namespacename)
        {
            this.RegisterTerms();
        }

        /// <summary>
        /// Registers the terms.
        /// </summary>
        override protected void RegisterTerms()
        {
            RegisterTerms("type:Zentity.ScholarlyWorks.Audio", "Sound");
            RegisterTerms("type:Zentity.ScholarlyWorks.Software", "Software");
            RegisterTerms("type:Zentity.ScholarlyWorks.Image", "Image");
        }
    }
}
