// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Zentity.Core;

    #region MetadataProvider class

    /// <summary>
    /// Provides API's used by the OAI-PMH protocol Service Provider.    
    /// </summary>
    public class MetadataProvider
    {
        //TODO: Should Core.Contact be blocked what should be the use case if the user specifies set as contact.

        #region Private member variables

        CoreHelper coreHelperUtility = new CoreHelper();
        string entityConnectionString;
        #endregion

        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataProvider"/> class.
        /// </summary>
        /// <param name="entityConnectionString">Entity connection string for connecting to database.</param>
        public MetadataProvider(string entityConnectionString)
        {
            this.entityConnectionString = entityConnectionString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataProvider"/> class.
        /// </summary>
        /// <param name="maximumHarvestCount">
        /// The maximum harvest count used to specify the 
        /// maximum number of records that can be harvested at a given time.
        /// </param>
        /// <param name="resumptionTokenExpirationTimeSpan">The resumption token expiration time span.</param>
        /// <param name="entityConnectionString">Connection string for establishing context connection to database</param>
        public MetadataProvider(
                            int maximumHarvestCount,
                            int resumptionTokenExpirationTimeSpan,
                            string entityConnectionString)
            : this(entityConnectionString)
        {
            PlatformSettings.MaximumHarvestCount = maximumHarvestCount;
            PlatformSettings.ResumptionTokenExpirationTimeSpan = resumptionTokenExpirationTimeSpan;
        }

        #endregion
        #region Public Property

        /// <summary>
        /// Gets the entity connection string.
        /// </summary>
        /// <value>The entity connection string.</value>
        public string EntityConnectionString
        {
            get
            {
                return entityConnectionString;
            }
        }
        #endregion

        /// <summary>
        /// Gets the resource types.
        /// </summary>
        /// <value>The resource types.</value>
        private IEnumerable<ResourceType> ResourceTypes
        {
            get
            {
                using (ZentityContext context = CoreHelper.CreateZentityContext(entityConnectionString))
                {
                    return CoreHelper.GetResourceTypes(context)
                                             .Where(type => !(type.Name.Equals(MetadataProviderHelper.Contact) ||
                                                    (null != type.BaseType && type.BaseType.Equals(typeof(ScholarlyWorks.Contact)))
                                                    ));
                }
            }
        }

        #region Public Methods

        /// <summary>
        /// Gets the Repository Details. 
        /// &lt;Identify&gt;
        ///     &lt;repositoryName&gt; Zentity Repository 1&lt;/repositoryName&gt;                                
        ///    &lt;adminEmail&gt;admin@zentity.com&lt;/adminEmail&gt;  
        ///    &lt;earliestDatestamp&gt;1990-02-01T12:00:00Z&lt;/earliestDatestamp&gt;
        ///    &lt;deletedRecord&gt;no&lt;/deletedRecord&gt;
        ///    &lt;granularity&gt;YYYY-MM-DDThh:mm:ssZ&lt;/granularity&gt;        
        /// &lt;/Identify&gt;   
        /// </summary>
        /// <example>
        /// This example creates retrieves the details which uniquely identify the Repository.
        /// You will need to add references to System.Data.Entity and Zentity.dll to successfully compile this sample.
        /// <code>
        /// using System;
        /// using System.Xml.Linq;
        /// using System.Net;
        ///
        /// using Zentity.Platform;
        ///
        /// namespace Zentity.Platform.Samples
        /// {
        ///     class Program
        ///     {
        ///         static void Main(string[] args)
        ///         {
        ///             try
        ///             {
        ///                 // Requests would be like http://localhost/OaiPmhService.ashx?verb=Identify"
        ///                 MetadataProvider oaipmhDataProvider = new MetadataProvider();
        ///                 XDocument resultSet = oaipmhDataProvider.RepositoryDetails;
        ///                 if (null != resultSet)
        ///                 {
        ///                     Console.WriteLine("Repository Details :\n {0}", resultSet.ToString());
        ///                 }
        ///                 else
        ///                 {
        ///                     Console.WriteLine("Could not provide Repository Details");
        ///                 }
        ///             }
        ///             catch (ArgumentException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///             catch (InvalidOperationException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <returns>
        /// An instance of XDocument containing details about the Repository.
        /// </returns>
        public XDocument RepositoryDetails
        {
            get
            {
                //Expected XDoc
                // <Identify>
                //     <repositoryName> Zentity Repository 1</repositoryName>  
                //    <adminEmail>admin@zentity.com</adminEmail>  
                //    <earliestDatestamp>1990-02-01T12:00:00Z</earliestDatestamp>
                //    <deletedRecord>no</deletedRecord>
                //    <granularity>YYYY-MM-DDThh:mm:ssZ</granularity>               
                // </Identify> 


                //Algorithm:

                XDocument repositoryDetails = null;
                //  Generate XDocument consisting of element and their predefined values
                XElement identifyElement = MetadataProviderHelper.GetElement(MetadataProviderHelper.Verb_Identify);

                identifyElement.Add(MetadataProviderHelper.GetElement(MetadataProviderHelper.Identify_RepositoryName, MetadataProviderHelper.RepositoryName));

                identifyElement.Add(MetadataProviderHelper.GetElement(MetadataProviderHelper.Identify_AdminEmail, MetadataProviderHelper.AdminEmail));

                // For earliestdatestamp query core to get the dateadded value of first resource
                identifyElement.Add(MetadataProviderHelper.GetElement(MetadataProviderHelper.Identify_EarliestDateStamp, this.coreHelperUtility.GetEarliestdateStamp()));

                identifyElement.Add(MetadataProviderHelper.GetElement(MetadataProviderHelper.Identify_Deleted, MetadataProviderHelper.DeletedRecords));

                identifyElement.Add(MetadataProviderHelper.GetElement(MetadataProviderHelper.Identify_Granularity, MetadataProviderHelper.Identify_DateTimeGranularity));

                repositoryDetails = new XDocument();
                repositoryDetails.Add(identifyElement);
                // return XDocument
                return repositoryDetails;
            }
        }

        /// <summary>
        /// Creates XDocumet containing resource type details.
        /// &lt;ListSets&gt;
        ///     &lt;set&gt;
        ///         &lt;setSpec&gt;music&lt;/setSpec&gt;
        ///         &lt;setName&gt;Music collection&lt;/setName&gt;
        ///     &lt;/set&gt;
        ///     &lt;set&gt;
        ///         &lt;setSpec&gt;music:(muzak)&lt;/setSpec&gt;
        ///         &lt;setName&gt;Muzak collection&lt;/setName&gt;
        ///     &lt;/set&gt;
        /// &lt;/ListSets&gt;
        /// </summary>
        /// <example>
        /// This example creates retrieves the details of the sets supported by the Repository.
        /// <code>
        /// using System;
        /// using System.Xml.Linq;
        /// using System.Net;
        /// using Zentity.Platform;
        /// namespace Zentity.Platform.Samples
        /// {
        ///     class Program
        ///     {
        ///          static void Main(string[] args)
        ///          {
        ///              try
        ///              {
        ///                 // Requests would be like http://localhost/OaiPmhService.ashx?verb=ListSets"
        ///                 MetadataProvider oaipmhDataProvider = new MetadataProvider();
        ///                 XDocument resultSet = oaipmhDataProvider.ListSets();
        ///                 if (null != resultSet)
        ///                 {
        ///                     Console.WriteLine("Sets supported by Repository :\n {0}", resultSet.ToString());
        ///                 }
        ///                 else
        ///                 {
        ///                     Console.WriteLine("Could not provide Set details for the Repository");
        ///                 }
        ///             }
        ///             catch (ArgumentException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///             catch (InvalidOperationException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        /// 
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <returns>Returns an instance of XDocument.</returns>
        /// <exception cref="System.ArgumentException">
        /// This exception is thrown if any of the parameters is invalid.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// This exception is thrown if the identifier does not exist in the Repository.
        /// </exception>
        public XDocument ListSets()
        {
            XDocument resourceTypeDetails = null;
            // <ListSets>
            //     <set>
            //         <setSpec>bf301223-c445-44c1-b7d7-89468df55df2</setSpec>
            //         <setName>Thesis</setName>
            //     </set>
            //     <set>
            //         <setSpec>bf301223-d445-44c1-b7d7-89468df79df3</setSpec>
            //         <setName>Book</setName>
            //     </set>
            // </ListSets>

            //Algorithm:

            resourceTypeDetails = new XDocument();

            //  generate XDocument, if resourcetype.id or resourcetype.name are not valid then throw invalid operation exception
            resourceTypeDetails.Add(MetadataProviderHelper.GetListSetElements(this.ResourceTypes));

            return resourceTypeDetails;
        }

        /// <summary>
        /// Generates Xdocument object specific to GetRecord verb requirement.
        /// &lt;GetRecord&gt;
        ///     &lt;record&gt;
        ///         &lt;header&gt;
        ///             &lt;identifier&gt;8d5ab630-2d35-4810-a970-617e34b3f8da&lt;/identifier&gt; 
        ///             &lt;datestamp&gt;2008-08-28T10:15:00Z&lt;/datestamp&gt; 
        ///             &lt;setSpec&gt;File&lt;/setSpec&gt; 
        ///             &lt;setSpec&gt;Resource&lt;/setSpec&gt; 
        ///         &lt;/header&gt;
        ///         &lt;metadata&gt; 
        ///             &lt;title&gt;File 3&lt;/title&gt; 
        ///             &lt;description&gt;Description Of File 3&lt;/description&gt; 
        ///             &lt;date&gt;2008-08-28T10:15:00Z&lt;/date&gt;   
        ///         &lt;/metadata&gt;
        ///     &lt;/record&gt;
        /// &lt;/GetRecord&gt;
        /// </summary>
        /// <param name="identifier">Record identifier</param>
        /// <example>
        /// This example creates retrieves the metadata associated with the identifier. 
        /// <code>
        /// using System;
        /// using System.Xml.Linq;
        /// using System.Net;
        /// using System.Reflection;
        /// using System.Linq;
        /// using System.Collections.Generic;
        /// using System.Web;
        ///
        /// using Zentity.Platform;
        ///
        /// namespace Zentity.Platform.Samples
        /// {
        ///     class Program
        ///     {
        ///         static void Main(string[] args)
        ///         {
        ///             try
        ///             {        
        ///                 MetadataProvider oaipmhDataProvider = new MetadataProvider();
        ///                 Guid identifier = new Guid("94732029-7860-4043-9612-6b16bc791fc7");
        ///                 XDocument resultSet = oaipmhDataProvider.GetRecord(identifier);
        ///                 if (null != resultSet)
        ///                 {
        ///                     Console.WriteLine("Metadata associated with Identifier {0} :\n {1}", new object[] { identifier.ToString(), resultSet.ToString() });
        ///                 }
        ///                 else
        ///                 {
        ///                     Console.WriteLine("Metadata associated with Identifier could not be found in the Repository.");
        ///                 }
        ///             }
        ///             catch (ArgumentException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <returns>Returns instance of XDocument object.</returns>
        /// <exception cref="System.ArgumentException">
        /// This exception is thrown if any of the parameters is invalid.
        /// </exception>
        public XDocument GetRecord(Guid identifier)
        {
            //Expected XDoc
            //  <GetRecord>
            //      <record> 
            //          <header>
            //              <identifier>bf301223-c445-44c1-b7d7-89468df55df2</identifier> 
            //              <datestamp>2001-12-14</datestamp>
            //              <setSpec>ba97d2b9-d2f0-4d04-864f-240073cb5954</setSpec>          
            //          </header>
            //          <metadata>            
            //              <title>Using Structural Metadata to Localize Experience of 
            //                      Digital Content<title> 
            //                <creator>Dushay, Naomi<creator>
            //                <subject>Digital Libraries<subject> 
            //                <description>With the increasing technical sophistication of 
            //                    both information consumers and providers, there is 
            //                  increasing demand for more meaningful experiences of digital 
            //                  information. We present a framework that separates digital 
            //                  object experience, or rendering, from digital object storage 
            //                  and manipulation, so the rendering can be tailored to 
            //                  particular communities of users.
            //              <description>             
            //              <date>2001-12-14<date>            
            //          </metadata>
            //      </record>
            //  </GetRecord>


            // Algorithm:                                                                         

            XDocument resourceDetails = null;
            try
            {

                // Check if id is valid => not guid.empty or null
                //       if (true) throw argument null exception
                if (identifier == Guid.Empty || identifier == null)
                    throw new ArgumentNullException(MetadataProviderHelper.Error_Parameter_Identifier,
                        MetadataProviderHelper.Error_Message_Invalid_Identifier);

                //       else retrieve resource and continue     
                Core.Resource resource = this.coreHelperUtility.GetResource(identifier);

                //       Check if resource object is not null, if null throw invalid operation exception else continue
                if (resource == null)
                    throw new ArgumentException(MetadataProviderHelper.Error_Message_Invalid_Identifier
                        , MetadataProviderHelper.Error_Parameter_Identifier);

                MetadataProviderHelper metadataHelper = new MetadataProviderHelper(this.entityConnectionString);

                XElement record = new XElement(MetadataProviderHelper.Record);
                record.Add(metadataHelper.GetHeaderElement(resource));
                record.Add(MetadataProviderHelper.GetMetadataElement(resource));

                XElement getRecord = MetadataProviderHelper.GetElement(MetadataProviderHelper.Verb_GetRecord);
                getRecord.Add(record);

                resourceDetails = new XDocument();

                resourceDetails.Add(getRecord);

            }
            catch (Exception)
            {
                throw;
            }

            // return instance XDocument object
            return resourceDetails;
        }

        /// <summary>
        /// Checks whether resource with specified identifier exist or not
        /// </summary>
        /// <param name="identifier"> resource identifier </param>
        /// <returns> boolean indicating whether resource exist or not  </returns>
        /// <example>
        /// This example checks if the given identifier exists in the Repository.         
        /// <code>
        /// using System.Xml.Linq;
        /// using System.Net;
        /// using System.Reflection;
        /// using System.Linq;
        /// using System.Collections.Generic;
        /// using System.Web;
        ///
        /// using Zentity.Platform;
        ///
        /// namespace Zentity.Platform.Samples
        /// {
        ///     class Program
        ///     {
        ///         static void Main(string[] args)
        ///         {
        ///             try
        ///             {
        ///
        ///                 MetadataProvider oaipmhDataProvider = new MetadataProvider();
        ///                 Guid identifier = new Guid("94732029-7860-4043-9612-6b16bc791fc7");
        ///                 if (oaipmhDataProvider.IdentifierExists(identifier))
        ///                 {
        ///                     Console.WriteLine(@"The Identifier '{0}' exists in the Repository ", identifier.ToString());
        ///                 }
        ///                 else
        ///                 {
        ///                     Console.WriteLine(@"The Identifier '{0}' does not exists in the Repository ", identifier.ToString());
        ///                 }
        ///             }
        ///             catch (ArgumentException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///         }
        ///     }
        /// }        
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentException">
        /// This exception is thrown if any of the parameters is invalid.
        /// </exception>
        public bool IdentifierExists(Guid identifier)
        {
            if (identifier == Guid.Empty || identifier == null)
                throw new ArgumentNullException(MetadataProviderHelper.Error_Parameter_Identifier,
                        MetadataProviderHelper.Error_Message_Invalid_Identifier);

            return (this.coreHelperUtility.CheckIfResourceExists(identifier));
        }

        /// <summary>
        /// Generates XDocument consiting of list identifier verb specific element.
        /// &lt;ListIdentifiers&gt;
        /// &lt;header&gt;
        /// &lt;identifier&gt;90384ccd-b7ce-4699-bcc8-53a4cfac1eff&lt;/identifier&gt; 
        /// &lt;datestamp&gt;2008-09-01T16:15:00Z&lt;/datestamp&gt; 
        /// &lt;setSpec&gt;ProceedingsArticle&lt;/setSpec&gt; 
        /// &lt;setSpec&gt;Proceedings&lt;/setSpec&gt; 
        /// &lt;setSpec&gt;Publication&lt;/setSpec&gt; 
        /// &lt;setSpec&gt;ScholarlyWork&lt;/setSpec&gt; 
        /// &lt;setSpec&gt;Resource&lt;/setSpec&gt; 
        /// &lt;/header&gt;
        /// &lt;header&gt;
        /// &lt;identifier&gt;6cb26ef8-6cc5-4304-9887-a85d52f97561&lt;/identifier&gt; 
        /// &lt;datestamp&gt;2008-08-28T04:15:00Z&lt;/datestamp&gt; 
        /// &lt;setSpec&gt;ProceedingsArticle&lt;/setSpec&gt; 
        /// &lt;setSpec&gt;Proceedings&lt;/setSpec&gt; 
        /// &lt;setSpec&gt;Publication&lt;/setSpec&gt; 
        /// &lt;setSpec&gt;ScholarlyWork&lt;/setSpec&gt; 
        /// &lt;setSpec&gt;Resource&lt;/setSpec&gt; 
        /// &lt;/header&gt;
        /// &lt;resumptionToken expirationDate="2008-09-30T07:30:38Z" completeListSize="165" cursor="0"&gt;
        /// fa6e49bd-f22f-4856-8dd6-c3dc21c0262d&lt;/resumptionToken&gt; 
        /// &lt;/ListIdentifiers&gt;
        /// </summary>
        /// <param name="queryParameters"> a hashtable containing filtering criteria :from, until,metadataprefix,set </param>
        /// <returns>an instance of XDocument containing list identifiers</returns>
        /// <returns>Boolean indicating whether resource exist or not  </returns>
        /// <example>
        /// This example lists all the identifiers that exists  in the Repository.        
        /// <code>
        /// using System;
        /// using System.Xml.Linq;
        /// using System.Net;
        /// using System.Reflection;
        /// using System.Linq;
        /// using System.Collections.Generic;
        /// using System.Web;
        /// using System.Collections;
        ///
        /// using Zentity.Platform;
        ///
        /// namespace Zentity.Platform.Samples
        /// {
        ///     class Program
        ///     {
        ///         static void Main(string[] args)
        ///         {
        ///             try
        ///             {
        ///
        ///                 MetadataProvider oaipmhDataProvider = new MetadataProvider();
        ///                 Hashtable queryParameters = new Hashtable();
        ///
        ///                 queryParameters.Add("set", "Publication");
        ///                 queryParameters.Add("from", "2007-01-01");
        ///                 queryParameters.Add("until", "2009-01-01");
        ///
        ///                 XDocument resultSet = oaipmhDataProvider.ListIdentifiers(queryParameters);
        ///                 if (null != resultSet)
        ///                 {
        ///                     Console.WriteLine("List of Identifier OfType{0} From {1}- Until  {2} :\n {3}", new object[] { queryParameters["set"], queryParameters["from"], queryParameters["until"], resultSet.ToString() });
        ///                 }
        ///                 else
        ///                 {
        ///                     Console.WriteLine("Repository does not contain any identifiers.");
        ///                 }
        ///             }
        ///             catch (ArgumentException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentException">
        /// This exception is thrown if there are no records in the Repository.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// This exception is thrown if any of the parameters is invalid.
        /// </exception>
        public XDocument ListIdentifiers(Hashtable queryParameters)
        {
            // XDocument expected
            //<ListIdentifiers>
            //   <header>  
            //    <identifier>59935eea-4e4a-417d-8fd4-a7daf8e0e7a2</identifier>
            //    <datestamp>1999-02-23</datestamp>
            //    <setSpec>168ef850-63df-4599-88e3-06833383df4e</setSpec>
            //   </header>
            //   <header>    
            //    <identifier>6c5693e1-3a40-4001-bff5-1c9148c7c52a</identifier>
            //    <datestamp>1999-03-20</datestamp>
            //    <setSpec>bf301223-c445-44c1-b7d7-89468df55df2</setSpec>           
            //   </header>                   
            //   <resumptionToken expirationDate="2002-06-01T23:20:00Z" 
            //      completeListSize="6" 
            //      cursor="0">d2bd64df-6609-4ea4-ae99-9669da69bf7a</resumptionToken>
            // </ListIdentifiers>


            //Algorithm:

            XDocument listIdentifiersDoc = null;
            try
            {
                if (queryParameters == null)
                    throw new ArgumentNullException(MetadataProviderHelper.Error_Parameter_QueryParameters,
                        MetadataProviderHelper.Error_Message_Invalid_QueryParams);


                if (queryParameters.Count == 0)
                    throw new ArgumentException(MetadataProviderHelper.Error_Message_Invalid_QueryParams
                        , MetadataProviderHelper.Error_Parameter_QueryParameters);

                DateTime queryExecutionTime = DateTime.Now;

                //  Parse query parameters
                //  if query parameters contain both from and until check if from less than until if not throw argument exception
                //  generate LinQ Query and retrieve resources
                //  check if count =0 if yes throw argument exception else continue
                List<Core.Resource> resourceList = this.coreHelperUtility.GetResources(queryParameters, queryExecutionTime, false);

                listIdentifiersDoc = new XDocument();

                MetadataProviderHelper metadataHelper = new MetadataProviderHelper(this.entityConnectionString);

                //  check if resumption required
                if (MetadataProviderHelper.IsResumptionRequired(this.coreHelperUtility.ResourceCount))
                {
                    Guid resumptionToken = Guid.Empty;

                    //  if yes insert new resumption token in local store with following parameters
                    //      1)QueryExecutiondateTime 2)TotalRecords 3) Pending Records
                    //      parse query string to Base64 string                  
                    //      return new resumption token id
                    if (metadataHelper.InsertResumptionToken(queryExecutionTime, this.coreHelperUtility.ResourceCount,
                                    MetadataProviderHelper.GetPendingRecords(this.coreHelperUtility.ResourceCount),
                                    this.coreHelperUtility.ActualHarvestedCount, this.coreHelperUtility.ActualResourceCount,
                            MetadataProviderHelper.TransformQueryToBase64(queryParameters),
                            out resumptionToken))
                    {
                        if (resumptionToken != Guid.Empty)
                        {
                            // Generate XDocument, if important elements do not contain valid value throw invalid operation exception 
                            listIdentifiersDoc.Add(metadataHelper.GenerateIdentifierElements(resourceList, resumptionToken,
                                    this.coreHelperUtility.ResourceCount, queryExecutionTime, 0));
                        }
                    }
                }
                else
                {
                    // Generate XDocument, if important elements do not contain valid value throw invalid operation exception 
                    listIdentifiersDoc.Add(metadataHelper.GenerateIdentifierElements(resourceList));
                }

            }
            catch (Exception)
            {
                throw;
            }

            // return XDocument
            return listIdentifiersDoc;

        }

        /// <summary>
        /// Generates XDocument consiting of list identifier verb specific element.
        /// &lt;ListIdentifiers&gt;
        /// &lt;header&gt;
        /// &lt;identifier&gt;90384ccd-b7ce-4699-bcc8-53a4cfac1eff&lt;/identifier&gt; 
        /// &lt;datestamp&gt;2008-09-01T16:15:00Z&lt;/datestamp&gt; 
        /// &lt;setSpec&gt;ProceedingsArticle&lt;/setSpec&gt; 
        /// &lt;setSpec&gt;Proceedings&lt;/setSpec&gt; 
        /// &lt;setSpec&gt;Publication&lt;/setSpec&gt; 
        /// &lt;setSpec&gt;ScholarlyWork&lt;/setSpec&gt; 
        /// &lt;setSpec&gt;Resource&lt;/setSpec&gt; 
        /// &lt;/header&gt;
        /// &lt;header&gt;
        /// &lt;identifier&gt;6cb26ef8-6cc5-4304-9887-a85d52f97561&lt;/identifier&gt; 
        /// &lt;datestamp&gt;2008-08-28T04:15:00Z&lt;/datestamp&gt; 
        /// &lt;setSpec&gt;ProceedingsArticle&lt;/setSpec&gt; 
        /// &lt;setSpec&gt;Proceedings&lt;/setSpec&gt; 
        /// &lt;setSpec&gt;Publication&lt;/setSpec&gt; 
        /// &lt;setSpec&gt;ScholarlyWork&lt;/setSpec&gt; 
        /// &lt;setSpec&gt;Resource&lt;/setSpec&gt; 
        /// &lt;/header&gt;
        /// &lt;resumptionToken expirationDate="2008-09-30T07:30:38Z" completeListSize="165" cursor="0"&gt;
        /// fa6e49bd-f22f-4856-8dd6-c3dc21c0262d&lt;/resumptionToken&gt; 
        /// &lt;/ListIdentifiers&gt;
        /// </summary>        
        /// <param name="resumptionToken"> a uniqueidentifier provided to get further sequence of records as provided by previous incomplete list </param>
        /// <returns>an instance of xdocument containing list identifiers</returns>
        /// <example>
        /// This example lists all the pending identifiers that exists  in the Repository for the given resumtionToken.        
        /// <code>
        /// using System;
        /// using System.Xml.Linq;
        /// using System.Net;
        /// using System.Reflection;
        /// using System.Linq;
        /// using System.Collections.Generic;
        /// using System.Web;
        /// using System.Collections;
        ///
        /// using Zentity.Platform;
        ///
        /// namespace Zentity.Platform.Samples
        /// {
        ///     class Program
        ///     {
        ///         static void Main(string[] args)
        ///         {
        ///             try
        ///             {
        ///
        ///                 MetadataProvider oaipmhDataProvider = new MetadataProvider();
        ///                 Hashtable queryParameters = new Hashtable();
        ///
        ///                 // resumptionToken=b31927c8-9a6f-4449-b377-8e343e1d7b46
        ///                 queryParameters.Add("resumptionToken", "b31927c8-9a6f-4449-b377-8e343e1d7b46");
        ///                 XDocument resultSet = oaipmhDataProvider.ListIdentifiers(queryParameters);
        ///                 if (null != resultSet)
        ///                 {
        ///                     Console.WriteLine("List of Pending Identifiers for resumption Token {0} :\n {1}", new object[] { queryParameters["resumptionToken"], resultSet.ToString() });
        ///                 }
        ///                 else
        ///                 {
        ///                     Console.WriteLine("Repository does not contain any identifiers.");
        ///                 }
        ///             }
        ///             catch (ArgumentException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentException">
        /// This exception is thrown if there are no records in the Repository.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// This exception is thrown if any of the parameters is invalid.
        /// </exception>
        public XDocument ListIdentifiers(Guid resumptionToken)
        {
            // XDocument expected
            //<ListIdentifiers>
            //   <header>  
            //    <identifier>59935eea-4e4a-417d-8fd4-a7daf8e0e7a2</identifier>
            //    <datestamp>1999-02-23</datestamp>
            //    <setSpec>168ef850-63df-4599-88e3-06833383df4e</setSpec>
            //   </header>
            //   <header>    
            //    <identifier>6c5693e1-3a40-4001-bff5-1c9148c7c52a</identifier>
            //    <datestamp>1999-03-20</datestamp>
            //    <setSpec>bf301223-c445-44c1-b7d7-89468df55df2</setSpec>           
            //   </header>                   
            //   <resumptionToken expirationDate="2002-06-01T23:20:00Z" 
            //      completeListSize="6" 
            //      cursor="0">d2bd64df-6609-4ea4-ae99-9669da69bf7a</resumptionToken>
            // </ListIdentifiers>


            //Algorithm:


            XDocument listIdentifiersDoc = null;

            try
            {
                if (resumptionToken == Guid.Empty || resumptionToken == null)
                    throw new ArgumentException(MetadataProviderHelper.Error_Message_Invalid_ResumptionToken,
                        MetadataProviderHelper.Error_Parameter_ResumptionToken);

                MetadataProviderHelper metadataHelper = new MetadataProviderHelper(this.entityConnectionString);

                MetadataProviderHelper.TokenDetails token;

                //  Check if resumption token provided exist in local store
                bool tokenExist = metadataHelper.GetMetadataTokenRecord(resumptionToken, out token);

                //  if not exist throw argument exception else continue by retrieving all it details
                if (!tokenExist)
                    throw new ArgumentException(MetadataProviderHelper.Error_Message_Invalid_ResumptionToken,
                        MetadataProviderHelper.Error_Parameter_ResumptionToken);

                //  Check if resumption token provided has expired, if expired throw argument exception
                if (MetadataProviderHelper.IsResumptionTokenExpired(token.QueryExecutionDateTime))
                    throw new ArgumentException(MetadataProviderHelper.Error_Message_Invalid_ResumptionToken,
                       MetadataProviderHelper.Error_Parameter_ResumptionToken);

                //  generate LinQ Query from query string preserved in local store and retrieve resources
                Hashtable queryParameters = MetadataProviderHelper.ConstructQueryTable(token.QueryString);
                if (queryParameters.Count == 0)
                    throw new ArgumentException(MetadataProviderHelper.Error_Message_Invalid_ResumptionToken,
                       MetadataProviderHelper.Error_Parameter_ResumptionToken);

                //  check if count =0 if yes throw argument exception else continue
                //  check if resource list count matches that of preserved in local store , if not throw argument exception
                List<Core.Resource> resourceList = this.coreHelperUtility.GetResources(queryParameters, token, false);

                listIdentifiersDoc = new XDocument();

                //  check if resumption required
                if (MetadataProviderHelper.IsResumptionRequired(token.PendingRecords))
                {
                    Guid newResumptionToken = Guid.Empty;

                    //  if yes delete previous resumption token and insert new resumption token in local store with following parameters
                    //      1)QueryExecutiondateTime 2)TotalRecords 3) Pending Records
                    //      parse query string to Base64 string                  
                    //      return new resumption token id                    
                    if (metadataHelper.UpdateResumptionToken(token.ResumptionToken, this.coreHelperUtility.ActualHarvestedCount, out newResumptionToken))
                    {
                        if (newResumptionToken != Guid.Empty)
                        {
                            // Generate XDocument, if important elements do not contain valid value throw invalid operation exception           
                            listIdentifiersDoc.Add(metadataHelper.GenerateIdentifierElements(resourceList, newResumptionToken, this.coreHelperUtility.ResourceCount,
                                token.QueryExecutionDateTime, MetadataProviderHelper.GetRecordsCount(token.TotalRecords, token.PendingRecords)));
                        }
                    }
                }
                else
                {
                    // else delete previous resumption token
                    if (metadataHelper.DeleteResumptionToken(token.ResumptionToken))
                    {
                        // Generate XDocument, if important elements do not contain valid value throw invalid operation exception           
                        listIdentifiersDoc.Add(metadataHelper.GenerateIdentifierElements(resourceList, Guid.Empty, this.coreHelperUtility.ResourceCount,
                            token.QueryExecutionDateTime, MetadataProviderHelper.GetRecordsCount(token.TotalRecords, token.PendingRecords)));
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }

            // return XDocument
            return listIdentifiersDoc;

        }

        /// <summary>
        /// Generates XDocument consiting of list records verb specific element.
        /// &lt;ListRecords&gt;
        ///     &lt;record&gt;
        ///         &lt;header&gt;
        ///             &lt;identifier&gt;8d5ab630-2d35-4810-a970-617e34b3f8da&lt;/identifier&gt; 
        ///             &lt;datestamp&gt;2008-08-28T10:15:00Z&lt;/datestamp&gt; 
        ///             &lt;setSpec&gt;File&lt;/setSpec&gt; 
        ///             &lt;setSpec&gt;Resource&lt;/setSpec&gt; 
        ///         &lt;/header&gt;
        ///         &lt;metadata&gt; 
        ///             &lt;title&gt;File 3&lt;/title&gt; 
        ///             &lt;description&gt;Description Of File 3&lt;/description&gt; 
        ///             &lt;date&gt;2008-08-28T10:15:00Z&lt;/date&gt;   
        ///         &lt;/metadata&gt;
        ///     &lt;/record&gt;
        ///     &lt;resumptionToken expirationDate="2008-09-30T07:49:06Z" completeListSize="165" cursor="0"&gt;75e690cd-c5e7-4056-8fb2-10d73dcb6ca0&lt;/resumptionToken&gt;
        /// &lt;/ListRecords&gt;
        /// </summary>
        /// <param name="queryParameters"> A hashtable containing filtering criteria :from, until,metadataprefix,set </param>
        /// <returns>An instance of XDocument containing list records.</returns>
        /// <example>
        /// This example lists all the records that exists  in the Repository.        
        /// <code>
        /// using System;
        /// using System.Xml.Linq;
        /// using System.Net;
        /// using System.Reflection;
        /// using System.Linq;
        /// using System.Collections.Generic;
        /// using System.Web;
        /// using System.Collections;
        ///
        /// using Zentity.Platform;
        ///
        /// namespace Zentity.Platform.Samples
        /// {
        ///     class Program
        ///     {
        ///         static void Main(string[] args)
        ///         {
        ///             try
        ///             {
        ///
        ///                 MetadataProvider oaipmhDataProvider = new MetadataProvider();
        ///                 Hashtable queryParameters = new Hashtable();
        ///
        ///                 queryParameters.Add("set", "Publication");
        ///                 queryParameters.Add("from", "2007-01-01");
        ///                 queryParameters.Add("until", "2009-01-01");
        ///
        ///                 XDocument resultSet = oaipmhDataProvider.ListRecords(queryParameters);
        ///                 if (null != resultSet)
        ///                 {
        ///                     Console.WriteLine("List of Records OfType{0} From {1}- Until  {2} :\n {3}", new object[] { queryParameters["set"], queryParameters["from"], queryParameters["until"], resultSet.ToString() });
        ///                 }
        ///                 else
        ///                 {
        ///                     Console.WriteLine("Repository does not contain any records.");
        ///                 }
        ///             }
        ///             catch (ArgumentException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentException">
        /// This exception is thrown if there are no records in the Repository.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// This exception is thrown if any of the parameters is invalid.
        /// </exception>
        public XDocument ListRecords(Hashtable queryParameters)
        {
            // XDocument expected
            //<ListRecords>
            //  <record>
            //    <header>
            //      <identifier>bf301223-c445-44c1-b7d7-89468df55df2</identifier>
            //      <datestamp>2002-05-01T14:16:12Z</datestamp>
            //      <setSpec>168ef850-63df-4599-88e3-06833383df4e</setSpec>
            //    </header>
            //    <metadata>       
            //      <title>Using Structural Metadata to Localize Experience of 
            //                Digital Content<title> 
            //      <creator>Dushay, Naomi<creator>
            //      <subject>Digital Libraries<subject> 
            //      <description>With the increasing technical sophistication of 
            //              both information consumers and providers, there is 
            //             increasing demand for more meaningful experiences of digital 
            //            information. We present a framework that separates digital 
            //            object experience, or rendering, from digital object storage 
            //            and manipulation, so the rendering can be tailored to 
            //            particular communities of users.
            //        <description>             
            //        <date>2001-12-14<date>                        
            //    </metadata>
            //  </record>           
            //</ListRecords>      

            //Algorithm:

            XDocument listRecordsDoc = null;
            try
            {

                if (queryParameters == null)
                    throw new ArgumentNullException(MetadataProviderHelper.Error_Parameter_QueryParameters,
                        MetadataProviderHelper.Error_Message_Invalid_QueryParams);

                if (queryParameters.Count == 0)
                    throw new ArgumentException(MetadataProviderHelper.Error_Message_Invalid_QueryParams
                        , MetadataProviderHelper.Error_Parameter_QueryParameters);

                DateTime queryExecutionTime = DateTime.Now;

                //  Parse query parameters
                //  if query parameters contain both from and until check if from less than until if not throw argument exception
                //  generate LinQ Query and retrieve resources
                //  check if count = 0 if yes throw argument exception else continue
                List<Core.Resource> resourceList = this.coreHelperUtility.GetResources(queryParameters, queryExecutionTime, true);

                listRecordsDoc = new XDocument();

                MetadataProviderHelper metadataHelper = new MetadataProviderHelper(this.entityConnectionString);

                //  check if resumption required
                if (MetadataProviderHelper.IsResumptionRequired(this.coreHelperUtility.ResourceCount))
                {
                    Guid resumptionToken = Guid.Empty;

                    //  if yes insert new resumption token in local store with following parameters
                    //      1)QueryExecutiondateTime 2)TotalRecords 3) Pending Records
                    //      parse query string to Base64 string                  
                    //      return new resumption token id
                    if (metadataHelper.InsertResumptionToken(queryExecutionTime, this.coreHelperUtility.ResourceCount,
                            MetadataProviderHelper.GetPendingRecords(this.coreHelperUtility.ResourceCount),
                            this.coreHelperUtility.ActualHarvestedCount, this.coreHelperUtility.ActualResourceCount,
                            MetadataProviderHelper.TransformQueryToBase64(queryParameters),
                            out resumptionToken))
                    {
                        // Generate XDocument, if important elements do not contain valid value throw invalid operation exception 
                        listRecordsDoc.Add(metadataHelper.GenerateRecordElements(resourceList, resumptionToken, this.coreHelperUtility.ResourceCount,
                                                queryExecutionTime, 0));
                    }
                }
                else
                {
                    // Generate XDocument, if important elements do not contain valid value throw invalid operation exception 
                    listRecordsDoc.Add(metadataHelper.GenerateRecordElements(resourceList));
                }
            }
            catch (Exception)
            {
                throw;
            }

            // return XDocument
            return listRecordsDoc;
        }

        /// <summary>
        /// Generates XDocument consiting of list records verb specific elements.
        /// &lt;ListRecords&gt;
        ///     &lt;record&gt;
        ///         &lt;header&gt;
        ///             &lt;identifier&gt;8d5ab630-2d35-4810-a970-617e34b3f8da&lt;/identifier&gt; 
        ///             &lt;datestamp&gt;2008-08-28T10:15:00Z&lt;/datestamp&gt; 
        ///             &lt;setSpec&gt;File&lt;/setSpec&gt; 
        ///             &lt;setSpec&gt;Resource&lt;/setSpec&gt; 
        ///         &lt;/header&gt;
        ///         &lt;metadata&gt; 
        ///             &lt;title&gt;File 3&lt;/title&gt; 
        ///             &lt;description&gt;Description Of File 3&lt;/description&gt; 
        ///             &lt;date&gt;2008-08-28T10:15:00Z&lt;/date&gt;   
        ///         &lt;/metadata&gt;
        ///     &lt;/record&gt;
        ///     &lt;resumptionToken expirationDate="2008-09-30T07:49:06Z" completeListSize="165" cursor="0"&gt;75e690cd-c5e7-4056-8fb2-10d73dcb6ca0&lt;/resumptionToken&gt;
        /// &lt;/ListRecords&gt;
        /// </summary>        
        /// <param name="resumptionToken"> a unique identifier provided to get further sequence of records as provided by previous incomplete list </param>
        /// <returns>an instance of XDocument containing list records</returns>
        /// <example>
        /// This example lists all the pending records that exists  in the Repository for the given resumtionToken.        
        /// <code>
        /// using System;
        /// using System.Xml.Linq;
        /// using System.Net;
        /// using System.Reflection;
        /// using System.Linq;
        /// using System.Collections.Generic;
        /// using System.Web;
        /// using System.Collections;
        ///
        /// using Zentity.Platform;
        ///
        /// namespace Zentity.Platform.Samples
        /// {
        ///     class Program
        ///     {
        ///         static void Main(string[] args)
        ///         {
        ///             try
        ///             {
        ///
        ///                 MetadataProvider oaipmhDataProvider = new MetadataProvider();
        ///                 Hashtable queryParameters = new Hashtable();
        ///
        ///                 // resumptionToken=b31927c8-9a6f-4449-b377-8e343e1d7b46
        ///                 queryParameters.Add("resumptionToken", "b31927c8-9a6f-4449-b377-8e343e1d7b46");
        ///                 XDocument resultSet = oaipmhDataProvider.ListRecords(queryParameters);
        ///                 if (null != resultSet)
        ///                 {
        ///                     Console.WriteLine("List of Pending Records metadata for the given resumption Token {0} :\n {1}", new object[] { queryParameters["resumptionToken"], resultSet.ToString() });
        ///                 }
        ///                 else
        ///                 {
        ///                     Console.WriteLine("Repository does not contain any records.");
        ///                 }
        ///             }
        ///             catch (ArgumentException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentException">
        /// This exception is thrown if there are no records in the Repository.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// This exception is thrown if any of the parameters is invalid.
        /// </exception>
        public XDocument ListRecords(Guid resumptionToken)
        {
            // XDocument expected
            //<ListRecords>
            //  <record>
            //    <header>
            //      <identifier>bf301223-c445-44c1-b7d7-89468df55df2</identifier>
            //      <datestamp>2002-05-01T14:16:12Z</datestamp>
            //      <setSpec>168ef850-63df-4599-88e3-06833383df4e</setSpec>
            //    </header>
            //    <metadata>       
            //      <title>Using Structural Metadata to Localize Experience of 
            //                Digital Content<title> 
            //      <creator>Dushay, Naomi<creator>
            //      <subject>Digital Libraries<subject> 
            //      <description>With the increasing technical sophistication of 
            //              both information consumers and providers, there is 
            //             increasing demand for more meaningful experiences of digital 
            //            information. We present a framework that separates digital 
            //            object experience, or rendering, from digital object storage 
            //            and manipulation, so the rendering can be tailored to 
            //            particular communities of users.
            //        <description>             
            //        <date>2001-12-14<date>                        
            //    </metadata>
            //  </record>           
            //</ListRecords>      


            //Algorithm:

            XDocument listRecordsDoc = null;

            try
            {
                if (resumptionToken == Guid.Empty || resumptionToken == null)
                    throw new ArgumentException(MetadataProviderHelper.Error_Message_Invalid_ResumptionToken,
                       MetadataProviderHelper.Error_Parameter_ResumptionToken);

                MetadataProviderHelper metadataHelper = new MetadataProviderHelper(this.entityConnectionString);

                MetadataProviderHelper.TokenDetails token;

                //  Check if resumption token provided exist in local store
                bool tokenExist = metadataHelper.GetMetadataTokenRecord(resumptionToken, out token);

                //  if not exist throw argument exception else continue by retrieving all it details
                if (!tokenExist)
                    throw new ArgumentException(MetadataProviderHelper.Error_Message_Invalid_ResumptionToken,
                       MetadataProviderHelper.Error_Parameter_ResumptionToken);

                //  Check if resumption token provided has expired, if expired throw argument exception
                if (MetadataProviderHelper.IsResumptionTokenExpired(token.QueryExecutionDateTime))
                    throw new ArgumentException(MetadataProviderHelper.Error_Message_Invalid_ResumptionToken,
                       MetadataProviderHelper.Error_Parameter_ResumptionToken);

                //  generate LinQ Query from query string preserved in local store and retrieve resources
                Hashtable queryParameters = MetadataProviderHelper.ConstructQueryTable(token.QueryString);
                if (queryParameters.Count == 0)
                    throw new ArgumentException(MetadataProviderHelper.Error_Message_Invalid_ResumptionToken,
                      MetadataProviderHelper.Error_Parameter_ResumptionToken);

                //  check if count =0 if yes throw argument exception else continue
                //  check if resource list count matches that of preserved in local store , if not throw argument exception
                List<Core.Resource> resourceList = this.coreHelperUtility.GetResources(queryParameters, token, true);

                listRecordsDoc = new XDocument();

                //  check if resumption required
                if (MetadataProviderHelper.IsResumptionRequired(token.PendingRecords))
                {
                    Guid newResumptionToken = Guid.Empty;

                    //  if yes delete previous resumption token and insert new resumption token in local store with following parameters
                    //      1)QueryExecutiondateTime 2)TotalRecords 3) Pending Records
                    //      parse query string to Base64 string                  
                    //      return new resumption token id                    
                    if (metadataHelper.UpdateResumptionToken(token.ResumptionToken, this.coreHelperUtility.ActualHarvestedCount, out newResumptionToken))
                    {
                        if (newResumptionToken != Guid.Empty)
                        {
                            // Generate XDocument, if important elements do not contain valid value throw invalid operation exception           
                            listRecordsDoc.Add(metadataHelper.GenerateRecordElements(resourceList, newResumptionToken, this.coreHelperUtility.ResourceCount,
                                token.QueryExecutionDateTime, MetadataProviderHelper.GetRecordsCount(token.TotalRecords, token.PendingRecords)));
                        }
                    }
                }
                else
                {
                    // else delete previous resumption token
                    if (metadataHelper.DeleteResumptionToken(token.ResumptionToken))
                    {
                        // Generate XDocument, if important elements do not contain valid value throw invalid operation exception           
                        listRecordsDoc.Add(metadataHelper.GenerateRecordElements(resourceList, Guid.Empty, this.coreHelperUtility.ResourceCount,
                            token.QueryExecutionDateTime, MetadataProviderHelper.GetRecordsCount(token.TotalRecords, token.PendingRecords)));
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }

            // return XDocument
            return listRecordsDoc;
        }

        /// <summary>
        /// Checks whether set hierachy exist.
        /// </summary>
        /// <returns> Boolean value indicating set hierarchy exist. False - does Not exist </returns>
        public bool CheckIfSetHierarchyExists()
        {
            return this.ResourceTypes.Count() > 0;
        }

        /// <summary>
        /// Checks whether specified setSpec exists. 
        /// </summary>
        /// <param name="setSpec">The associated setSpec</param>
        /// <returns> true if the given setSpec exists; else false</returns>
        public bool CheckIfSetSpecExists(string setSpec)
        {
            if (string.IsNullOrEmpty(setSpec))
            {
                return false;
            }
            ResourceType resourceTypeInfo = this.ResourceTypes.FirstOrDefault(rtInfo => rtInfo.Name.Equals(setSpec, StringComparison.OrdinalIgnoreCase));
            if (null != resourceTypeInfo)
            {
                //TODO: Check if Contact is to be ignored.
                return true;
            }

            return false;
        }

        #endregion
    }

    #endregion
}
