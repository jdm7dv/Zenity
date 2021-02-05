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
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using Microsoft.Practices.EnterpriseLibrary.Data;
    using Zentity.Core;

    #region MetaDataProviderHelper

    /// <summary>
    /// Helper utility supporting metadataprovider.cs 
    /// </summary>
    internal class MetadataProviderHelper
    {
        #region Internal Member variables

        //TODO: 1.Currently “RepositoryName “ and “adminEmail” and “AdminEmail” are added as constants ;
        //          but they need to be accessed from Registry and need to be configured during Installation.
        internal const string RepositoryName = "Zentity.Core";

        internal const string AdminEmail = "admin1@zentity.com";

        internal const string DeletedRecords = "no";
        private readonly string entityConnectionString;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataProviderHelper"/> class.
        /// </summary>
        internal MetadataProviderHelper()
            : this(CoreHelper.DefaultConnectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataProviderHelper"/> class.
        /// </summary>
        /// <param name="entityConnectionString">The entity connection string.</param>
        internal MetadataProviderHelper(string entityConnectionString)
        {
            this.entityConnectionString = entityConnectionString;
        }

        #endregion

        #region Custom Struct

        /// <summary>
        /// Struct containing resumption token details
        /// </summary>
        public struct TokenDetails
        {
            public Guid ResumptionToken;
            public DateTime QueryExecutionDateTime;
            public int TotalRecords;
            public int ActualTotalRecords;
            public int ActualHarvestedRecords;
            public int PendingRecords;
            public string QueryString;
        }

        #endregion

        #region Tag Elements Constants

        internal const string Verb_Identify = "Identify";

        internal const string Verb_GetRecord = "GetRecord";

        internal const string Verb_ListIdentifiers = "ListIdentifiers";

        internal const string Verb_ListRecords = "ListRecords";

        internal const string Verb_ListSets = "ListSets";

        internal const string Identify_RepositoryName = "repositoryName";

        internal const string Identify_EarliestDateStamp = "earliestDatestamp";

        internal const string Identify_Deleted = "deletedRecord";

        internal const string Identify_Granularity = "granularity";

        internal const string Identify_AdminEmail = "adminEmail";

        internal const string Header = "header";

        internal const string Header_Identifier = "identifier";

        internal const string Header_DateStamp = "datestamp";

        internal const string Header_SetSpec = "setSpec";

        internal const string Header_SetName = "setName";

        internal const string Metadata = "metadata";

        internal const string Metadata_Title = "title";

        internal const string Metadata_Creator = "creator";

        internal const string Metadata_Contributor = "contributor";

        internal const string Metadata_Subject = "subject";

        internal const string Metadata_Description = "description";

        internal const string Metadata_Date = "date";

        internal const string ListSets_Set = "set";

        internal const string Record = "record";

        internal const string Resumption_Token = "resumptionToken";

        internal const string Resumption_Cursor = "cursor";

        internal const string Resumption_CompleteSize = "completeListSize";

        internal const string Resumption_ExpirationDate = "expirationDate";

        internal const string DB_ConnectionString = "ZentityPlatform";

        internal const string Proc_Insert = "OaiPmh.InsertResumptionToken";

        internal const string Proc_Delete = "OaiPmh.DeleteMetadata";

        internal const string Proc_Retrieve = "OaiPmh.GetMetadataTokenRecord";

        internal const string Proc_Update = "OaiPmh.UpdateResumptionToken";

        internal const string Proc_Param_QueryTime = "QueryExecutionDateTime";

        internal const string Proc_Param_TotalRecords = "TotalRecords";

        internal const string Proc_Param_ActualTotalRecords = "ActualTotalRecords";

        internal const string Proc_Param_ActualHarvestedRecords = "ActualHarvestedRecords";

        internal const string Proc_Param_PendingRecords = "PendingRecords";

        internal const string Proc_Param_QueryString = "QueryString";

        internal const string Proc_Param_NewResumptionToken = "NewResumptionToken";

        internal const string Proc_Param_ResumptionToken = "ResumptionToken";

        internal const string Proc_Param_MaxHarvestCount = "MaxHarvestCount";

        internal const string Proc_Param_ID = "Id";

        internal const string DateTimeGranularity = "yyyy-MM-ddTHH:mm:ssZ";

        internal const string Identify_DateTimeGranularity = "YYYY-MM-DDThh:mm:ssZ";

        internal const string DateTimeFormat = "yyyy-MM-dd";

        internal const string Error_Message_NoRecords = "No records found";

        internal const string Error_Message_Invalid_Identifier = "Invalid Identifier";

        internal const string Error_Message_Invalid_QueryParams = "Invalid Query Parameters";

        internal const string Error_Message_Invalid_ResumptionToken = "Invalid Resumption Token";

        internal const string Error_Parameter_Identifier = "identifier";

        internal const string Error_Parameter_QueryParameters = "queryParameters";

        internal const string Error_Parameter_ResumptionToken = "resumptionToken";

        internal const string Contact = "Contact";

        #endregion

        #region Internal Member functions

        #region Static

        /// <summary>
        /// Creates XElement specific to Name and Value
        /// </summary>
        /// <param name="name"> Element Name </param>
        /// <param name="value">Element Value </param>
        /// <returns>instance of XElement</returns>
        internal static XElement GetElement(string name, string value)
        {
            return new XElement(name, value);
        }

        /// <summary>
        /// Creates XElement specific to Name
        /// </summary>
        /// <param name="name">Element Name</param>
        /// <returns>instance of XElement</returns>
        internal static XElement GetElement(string name)
        {
            return new XElement(name);
        }

        /// <summary>
        /// Checks whether Harvester will need to re-issue the request
        /// </summary>
        /// <param name="recordsToReturn">Total list size</param>
        /// <returns>returns should harvester reissue token</returns>
        internal static bool IsResumptionRequired(int recordsToReturn)
        {
            return (recordsToReturn > PlatformSettings.MaximumHarvestCount);
        }

        /// <summary>
        /// Checks whether token has expired on basis of initial query execution time
        /// </summary>
        /// <param name="queryexecutionTime">initial query execution time i.e. of first request of incomplete list</param>
        /// <returns>returns if token has expired</returns>
        internal static bool IsResumptionTokenExpired(DateTime queryexecutionTime)
        {
            return ((queryexecutionTime.AddMinutes(PlatformSettings.ResumptionTokenExpirationTimeSpan) < DateTime.Now));
        }

        /// <summary>
        /// calculates records returned 
        /// </summary>
        /// <param name="totalRecords">Total list size</param>
        /// <param name="pendingRecords">Pending list size</param>
        /// <returns>return records returned from complete list size</returns>
        internal static int GetRecordsCount(int totalRecords, int pendingRecords)
        {
            return (totalRecords - pendingRecords);
        }

        /// <summary>
        /// Calculates pending records
        /// </summary>
        /// <param name="totalRecords">complete list size</param>        
        /// <returns>returns pending records count</returns>
        internal static int GetPendingRecords(int totalRecords)
        {
            return MetadataProviderHelper.GetRecordsCount(totalRecords, PlatformSettings.MaximumHarvestCount);
        }

        /// <summary>
        /// encodes string to Base64
        /// </summary>
        /// <param name="dataToEncode"> string to encode </param>
        /// <returns> encoded string </returns>
        internal static string EncodeToBase64(string dataToEncode)
        {
            byte[] encodeQuery = new byte[dataToEncode.Length];
            encodeQuery = System.Text.Encoding.UTF8.GetBytes(dataToEncode);
            string encodedData = Convert.ToBase64String(encodeQuery);
            return encodedData;

        }

        /// <summary>
        /// decodes Base64 string
        /// </summary>
        /// <param name="data"> string to be decoded</param>
        /// <returns> instance of string decoded </returns>
        internal static string DecodeFromBase64(string data)
        {
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            System.Text.Decoder utf8Decode = encoder.GetDecoder();

            byte[] todecode_byte = Convert.FromBase64String(data);
            int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            string result = new String(decoded_char);
            return result;
        }

        /// <summary>
        /// transforms hashtable data to Base64
        /// </summary>
        /// <param name="query">hashtable</param>
        /// <returns> encoded string </returns>
        internal static string TransformQueryToBase64(Hashtable query)
        {
            System.Text.StringBuilder queryString = new StringBuilder();
            int i = query.Count - 1;
            foreach(DictionaryEntry dictionarykey in query)
            {
                queryString.Append(System.Convert.ToString(dictionarykey.Key, CultureInfo.InvariantCulture)
                        + "=" + System.Convert.ToString(dictionarykey.Value, CultureInfo.InvariantCulture));
                if(i < query.Count)
                {
                    queryString.Append("&");
                }
            }

            return EncodeToBase64(queryString.ToString());
        }

        /// <summary>
        /// Constructs the query table.
        /// </summary>
        /// <param name="base64EncodedQuery">The base64 encoded query.</param>
        /// <returns>The hash table.</returns>
        internal static Hashtable ConstructQueryTable(string base64EncodedQuery)
        {
            Hashtable queryParameters = new Hashtable();
            string query = DecodeFromBase64(base64EncodedQuery);

            string[] tmpArr = query.Split('=', '&');

            DictionaryEntry de = new DictionaryEntry();

            int iterator = 0;
            foreach(string data in tmpArr)
            {
                if(iterator % 2 == 0)
                {
                    de.Key = data;
                }
                else
                {
                    de.Value = data;
                    queryParameters.Add(de.Key, de.Value);
                }

                iterator = iterator + 1;
            }

            return queryParameters;
        }

        #endregion

        #region Non Static

        /// <summary>
        /// Inserts metadata record in Zentity.Platform db
        /// </summary>
        /// <param name="queryExecutionDateTime">DateTime of query execution</param>
        /// <param name="totalRecords">total list size </param>
        /// <param name="pendingRecords">pending list size</param>
        /// <param name="actualHarvestedRecords">actual harvested record count</param>
        /// <param name="actualTotalRecords">actual Total number of records</param>
        /// <param name="queryString">The query string.</param>
        /// <param name="resumptionToken">resumption token generated by db </param>
        /// <returns>
        ///     <c>true</c> if insert was successful; otherwise <c>false</c>.
        /// </returns>
        internal bool InsertResumptionToken(
                                        DateTime queryExecutionDateTime, 
                                        int totalRecords, 
                                        int pendingRecords,
                                        int actualHarvestedRecords, 
                                        int actualTotalRecords,
                                        string queryString,
                                        out Guid resumptionToken)
        {
            bool result = false;
            Database db = DatabaseFactory.CreateDatabase(DB_ConnectionString);
            string sqlString = Proc_Insert;
            System.Data.Common.DbCommand databaseCommand = db.GetSqlStringCommand(sqlString);
            databaseCommand.CommandType = System.Data.CommandType.StoredProcedure;
            db.AddInParameter(databaseCommand, Proc_Param_QueryTime, System.Data.DbType.DateTime, queryExecutionDateTime);
            db.AddInParameter(databaseCommand, Proc_Param_TotalRecords, System.Data.DbType.Int32, totalRecords);
            db.AddInParameter(databaseCommand, Proc_Param_ActualTotalRecords, System.Data.DbType.Int32, actualTotalRecords);
            db.AddInParameter(databaseCommand, Proc_Param_PendingRecords, System.Data.DbType.Int32, pendingRecords);
            db.AddInParameter(databaseCommand, Proc_Param_ActualHarvestedRecords, System.Data.DbType.Int32, actualHarvestedRecords);
            db.AddInParameter(databaseCommand, Proc_Param_QueryString, System.Data.DbType.String, queryString);
            db.AddOutParameter(databaseCommand, Proc_Param_NewResumptionToken, System.Data.DbType.Guid, 0);
            db.ExecuteNonQuery(databaseCommand);

            Guid id = new Guid(System.Convert.ToString(db.GetParameterValue(databaseCommand, Proc_Param_NewResumptionToken), CultureInfo.InvariantCulture));

            resumptionToken = id;

            result = true;
            return result;
        }

        /// <summary>
        /// Updates the resumption token.
        /// </summary>
        /// <param name="resumptionToken">The resumption token.</param>
        /// <param name="actualHarvestedRecords">The actual harvested records.</param>
        /// <param name="newResumptionToken">The new resumption token.</param>
        /// <returns>
        ///     <c>true</c> if update is successful; otherwise <c>false</c>.
        /// </returns>
        internal bool UpdateResumptionToken(Guid resumptionToken, int actualHarvestedRecords, out Guid newResumptionToken)
        {
            bool result = false;
            Database db = DatabaseFactory.CreateDatabase(DB_ConnectionString);
            string sqlString = Proc_Update;
            System.Data.Common.DbCommand databaseCommand = db.GetSqlStringCommand(sqlString);
            databaseCommand.CommandType = System.Data.CommandType.StoredProcedure;
            db.AddInParameter(databaseCommand, Proc_Param_ResumptionToken, System.Data.DbType.Guid, resumptionToken);
            db.AddInParameter(databaseCommand, Proc_Param_MaxHarvestCount, System.Data.DbType.Int32, PlatformSettings.MaximumHarvestCount);
            db.AddInParameter(databaseCommand, Proc_Param_ActualHarvestedRecords, System.Data.DbType.Int32, actualHarvestedRecords);
            db.AddOutParameter(databaseCommand, Proc_Param_NewResumptionToken, System.Data.DbType.Guid, 0);
            db.ExecuteNonQuery(databaseCommand);

            newResumptionToken = new Guid(System.Convert.ToString(db.GetParameterValue(databaseCommand, Proc_Param_NewResumptionToken), CultureInfo.InvariantCulture));

            result = true;
            return result;
        }

        /// <summary>
        ///  Deletes the specified resumption token from local store
        /// </summary>
        /// <param name="resumptionToken"> identifier of record to be deleted </param>
        /// <returns> Boolean value indicating success of operation </returns>
        internal bool DeleteResumptionToken(Guid resumptionToken)
        {
            bool result = false;
            Database db = DatabaseFactory.CreateDatabase(DB_ConnectionString);
            string sqlString = Proc_Delete;
            System.Data.Common.DbCommand databaseCommand = db.GetSqlStringCommand(sqlString);
            databaseCommand.CommandType = System.Data.CommandType.StoredProcedure;
            db.AddInParameter(databaseCommand, Proc_Param_ID, System.Data.DbType.Guid, resumptionToken);
            db.ExecuteNonQuery(databaseCommand);
            result = true;
            return result;
        }

        /// <summary>
        /// retrieves metadata token record
        /// </summary>
        /// <param name="id">record id to be retrieved</param>
        /// <param name="tokenDetails"> struct type to be filled by token details  </param>
        /// <returns>Boolean result</returns>
        internal bool GetMetadataTokenRecord(Guid id, out TokenDetails tokenDetails)
        {
            bool result = false;

            Database db = DatabaseFactory.CreateDatabase(DB_ConnectionString);
            string sqlString = Proc_Retrieve;
            System.Data.Common.DbCommand databaseCommand = db.GetSqlStringCommand(sqlString);
            databaseCommand.CommandType = System.Data.CommandType.StoredProcedure;
            db.AddInParameter(databaseCommand, Proc_Param_ID, System.Data.DbType.Guid, id);
            SqlDataReader rdr = (SqlDataReader)db.ExecuteReader(databaseCommand);

            tokenDetails.ResumptionToken = Guid.Empty;
            tokenDetails.QueryExecutionDateTime = DateTime.MinValue;
            tokenDetails.TotalRecords = 0;
            tokenDetails.ActualHarvestedRecords = 0;
            tokenDetails.ActualTotalRecords = 0;
            tokenDetails.PendingRecords = 0;
            tokenDetails.QueryString = string.Empty;
            if(!rdr.HasRows)
                return false;
            while(rdr.Read())
            {

                tokenDetails.ResumptionToken = new Guid(System.Convert.ToString(rdr[Proc_Param_ID], CultureInfo.InvariantCulture));
                tokenDetails.QueryExecutionDateTime = System.Convert.ToDateTime(rdr[Proc_Param_QueryTime], CultureInfo.InvariantCulture);
                tokenDetails.TotalRecords = System.Convert.ToInt32(rdr[Proc_Param_TotalRecords], CultureInfo.InvariantCulture);
                tokenDetails.ActualHarvestedRecords = System.Convert.ToInt32(rdr[Proc_Param_ActualHarvestedRecords], CultureInfo.InvariantCulture);
                tokenDetails.ActualTotalRecords = System.Convert.ToInt32(rdr[Proc_Param_ActualTotalRecords], CultureInfo.InvariantCulture);
                tokenDetails.PendingRecords = System.Convert.ToInt32(rdr[Proc_Param_PendingRecords], CultureInfo.InvariantCulture);
                tokenDetails.QueryString = System.Convert.ToString(rdr[Proc_Param_QueryString], CultureInfo.InvariantCulture);
                result = true;
            }
            rdr.Close();

            return result;
        }

        /// <summary>
        /// generates XElement containing set elements
        /// </summary>
        /// <param name="resourceTypesList">used for creating set elements</param>        
        /// <returns>instance of XElement</returns>
        internal static XElement GetListSetElements(IEnumerable<ResourceType> resourceTypesList)
        {
            if(null == resourceTypesList || resourceTypesList.Count() <= 0)
            {
                throw new ArgumentException(MetadataProviderHelper.Error_Message_NoRecords, "resourceTypesList");
            }

            XElement listSets = null;
            listSets = new XElement(Verb_ListSets);
            foreach(ResourceType resourceType in resourceTypesList)
            {
                XElement set = GetElement(ListSets_Set);

                bool isValidName = !(string.IsNullOrEmpty(resourceType.Name));

                if(isValidName)
                {
                    set.Add(GetElement(Header_SetSpec, resourceType.Name));
                    //TODO: How do we distinguish between SetSPec i.e. Id and SetName
                    set.Add(GetElement(Header_SetName, resourceType.Name));
                }

                listSets.Add(set);
            }
            return listSets;
        }

        /// <summary>
        /// performs processing for headers of List Identifiers
        /// </summary>
        /// <param name="resourceList"> resource list</param>
        /// <param name="resumptionToken"> resumption token for resumption element </param>
        /// <param name="totalResourceCount"> total list count </param>
        /// <param name="queryExecutionTime"> initial query execution time </param>
        /// <param name="recordsReturned"> total records returned till now </param>
        /// <returns> an instance of XElement </returns>
        internal XElement GenerateIdentifierElements(List<Core.Resource> resourceList, Guid resumptionToken,
                                                        int totalResourceCount, DateTime queryExecutionTime,
                                                        int recordsReturned)
        {
            XElement listIdentifiers = null;
            XElement resumptionElement = GetResumptionTokenElement(totalResourceCount, recordsReturned, resumptionToken, queryExecutionTime);

            listIdentifiers = GetIdentifierHeaders(resourceList);
            listIdentifiers.Add(resumptionElement);
            return listIdentifiers;
        }

        /// <summary>
        /// Generates ListIdentifier element for ListIdentifier verb
        /// </summary>
        /// <param name="resourceList"> list of resources </param>
        /// <returns> an instance of XElement </returns>
        internal XElement GenerateIdentifierElements(List<Core.Resource> resourceList)
        {
            if(null == resourceList || resourceList.Count <= 0)
            {
                throw new ArgumentException(MetadataProviderHelper.Error_Message_NoRecords, "resourceList");
            }

            XElement listIdentifiers = null;
            listIdentifiers = GetIdentifierHeaders(resourceList);
            return listIdentifiers;
        }


        /// <summary>
        /// Generates header element for ListIdentifier verb
        /// </summary>
        /// <param name="resourceList"> list of resources </param>
        /// <returns> an instance of XElement </returns>
        internal XElement GetIdentifierHeaders(List<Core.Resource> resourceList)
        {
            if(null == resourceList || resourceList.Count <= 0)
            {
                throw new ArgumentException(MetadataProviderHelper.Error_Message_NoRecords, "resourceList");
            }

            XElement listIdentifiers = null;
            listIdentifiers = new XElement(Verb_ListIdentifiers);
            foreach(Core.Resource resource in resourceList)
            {
                listIdentifiers.Add(GetHeaderElement(resource));
            }
            return listIdentifiers;
        }

        /// <summary>
        /// performs processing for records of List Records
        /// </summary>
        /// <param name="resourceList"> resource list</param>
        /// <param name="resumptionToken"> resumption token for resumption element </param>
        /// <param name="totalResourceCount"> total list count </param>
        /// <param name="queryExecutionTime"> initial query execution time </param>
        /// <param name="recordsReturned"> total records returned till now </param>
        /// <returns> an instance of XElement </returns>
        internal XElement GenerateRecordElements(List<Core.Resource> resourceList, Guid resumptionToken,
                                                        int totalResourceCount, DateTime queryExecutionTime, int recordsReturned)
        {
            XElement listRecords = null;
            XElement resumptionElement = GetResumptionTokenElement(totalResourceCount, recordsReturned, resumptionToken, queryExecutionTime);

            listRecords = GetRecordElement(resourceList);
            listRecords.Add(resumptionElement);
            return listRecords;
        }

        /// <summary>
        /// Performs processing for records of List Records
        /// </summary>
        /// <param name="resourceList"> list of resources </param>
        /// <returns> an instance of XElement </returns>
        internal XElement GenerateRecordElements(List<Core.Resource> resourceList)
        {
            XElement listRecords = null;
            listRecords = GetRecordElement(resourceList);
            return listRecords;
        }

        /// <summary>
        /// Generates record element for ListRecord verb
        /// </summary>
        /// <param name="resourceList"> list of resources </param>
        /// <returns> an instance of XElement </returns>
        internal XElement GetRecordElement(List<Core.Resource> resourceList)
        {
            if(null == resourceList || resourceList.Count <= 0)
            {
                throw new ArgumentException(MetadataProviderHelper.Error_Message_NoRecords, "resourceList");
            }

            XElement listRecords = null;
            listRecords = new XElement(Verb_ListRecords);
            foreach(Core.Resource resource in resourceList)
            {
                XElement record = new XElement(Record);
                record.Add(this.GetHeaderElement(resource));
                record.Add(MetadataProviderHelper.GetMetadataElement(resource));
                listRecords.Add(record);
            }
            return listRecords;
        }

        /// <summary>
        /// Creates header element 
        /// </summary>
        /// <param name="resource"> resource object</param>
        /// <returns> instance of XElement </returns>
        internal XElement GetHeaderElement(Core.Resource resource)
        {
            XElement header = null;
            header = new XElement(Header);

            header.Add(MetadataProviderHelper.GetElement(Header_Identifier, resource.Id.ToString()));
            header.Add(MetadataProviderHelper.GetElement(Header_DateStamp, resource.DateModified.Value.ToString(MetadataProviderHelper.DateTimeGranularity, CultureInfo.InvariantCulture)));

            ResourceType resourceTypeInfo = new CoreHelper(entityConnectionString).GetResourceType(resource);

            if(resourceTypeInfo != null)
            {
                List<string> listOfResourceTypeInfo = new CoreHelper(entityConnectionString).GetResourceTypeHierarchy(resourceTypeInfo);
                listOfResourceTypeInfo.ForEach(delegate(string resourceType)
                {
                    header.Add(MetadataProviderHelper.GetElement(Header_SetSpec, resourceType));
                });
            }
            return header;
        }

        /// <summary>
        /// Creates metadata element
        /// </summary>
        /// <param name="resource"> resource object </param>
        /// <returns> an instance of XElement </returns>
        internal static XElement GetMetadataElement(Core.Resource resource)
        {
            XElement metadata = null;
            metadata = new XElement(Metadata);

            if(!String.IsNullOrEmpty(resource.Title))
            {
                metadata.Add(MetadataProviderHelper.GetElement(Metadata_Title, resource.Title));
            }

            ScholarlyWorks.ScholarlyWork scholarlyWorks = resource as ScholarlyWorks.ScholarlyWork;
            if(null != scholarlyWorks)
            {
                if(scholarlyWorks.Authors != null)
                {
                    MetadataProviderHelper.AddResourceAuthors(metadata, scholarlyWorks);
                }
                if(scholarlyWorks.Contributors != null)
                {
                    MetadataProviderHelper.AddResourceContributors(metadata, scholarlyWorks);
                }
            }

            // TODO : 2.Currently Tags for a given resource are retrieved without 
            //          checking any relationships, but the implementation needs to be changed if required
            ScholarlyWorks.ScholarlyWorkItem scholarlyWorkItem = resource as ScholarlyWorks.ScholarlyWorkItem;
            if(scholarlyWorkItem != null && scholarlyWorkItem.Tags != null)
            {
                MetadataProviderHelper.AddResourceTags(metadata, scholarlyWorkItem);
            }

            metadata.Add(MetadataProviderHelper.GetElement(Metadata_Description, resource.Description));

            if(!String.IsNullOrEmpty(resource.DateModified.Value.ToString()))
            {
                metadata.Add(MetadataProviderHelper.GetElement(Metadata_Date, resource.DateModified.Value.ToString(DateTimeGranularity, CultureInfo.InvariantCulture)));
            }
            return metadata;
        }

        /// <summary>
        /// Gets the resumption token element.
        /// </summary>
        /// <param name="listSize">Size of the list.</param>
        /// <param name="cursor">The cursor attribute value.</param>
        /// <param name="resumptionToken">The resumption token.</param>
        /// <param name="executionQueryTime">The initial query execution time.</param>
        /// <returns>The <see cref="XElement"/>.</returns>
        internal XElement GetResumptionTokenElement(int listSize, int cursor, Guid resumptionToken, DateTime executionQueryTime)
        {
            XElement token = new XElement(Resumption_Token);
            if(resumptionToken != Guid.Empty)
            {
                token.SetAttributeValue(Resumption_ExpirationDate, executionQueryTime.AddMinutes(PlatformSettings.ResumptionTokenExpirationTimeSpan).ToUniversalTime().ToString(DateTimeGranularity, CultureInfo.InvariantCulture));
                token.Value = resumptionToken.ToString();
            }
            token.SetAttributeValue(Resumption_CompleteSize, listSize);
            token.SetAttributeValue(Resumption_Cursor, cursor);

            return token;
        }

        #endregion

        #endregion

        #region Private member functions

        /// <summary>
        /// retrieves authors of resource and adds them to the associated metadata element
        /// </summary>
        /// <param name="metadata">the associated metadata</param>
        /// <param name="scholarlyWorks">scholarlyWorks object whose authors to be retrieved</param>
        private static void AddResourceAuthors(XElement metadata, ScholarlyWorks.ScholarlyWork scholarlyWorks)
        {
            if(null == metadata || null == scholarlyWorks)
            {
                return;
            }

            scholarlyWorks.Authors.ToList().ForEach(delegate(ScholarlyWorks.Contact contact)
            {
                ScholarlyWorks.Person person = contact as ScholarlyWorks.Person;
                string personDetails = string.Empty;
                if(null != person)
                {
                    personDetails = CoreHelper.GetCompleteName(person.FirstName, person.MiddleName, person.LastName);
                    //In case if any other details are to be added then they can be added here
                }

                if(!string.IsNullOrEmpty(personDetails))
                {
                    metadata.Add(MetadataProviderHelper.GetElement(MetadataProviderHelper.Metadata_Creator, personDetails));
                }
            });
        }

        /// <summary>
        /// retrieves contributors of resource and adds them to the associated metadata element
        /// </summary>
        /// <param name="metadata">the associated metadata</param>
        /// <param name="scholarlyWorks">scholarlyWorks object whose contributors to be retrieved</param>
        private static void AddResourceContributors(XElement metadata, ScholarlyWorks.ScholarlyWork scholarlyWorks)
        {
            if(null == metadata || null == scholarlyWorks)
            {
                return;
            }

            scholarlyWorks.Contributors.ToList().ForEach(delegate(ScholarlyWorks.Contact contact)
            {
                ScholarlyWorks.Person person = contact as ScholarlyWorks.Person;
                string personDetails = string.Empty;
                if(null != person)
                {
                    personDetails = CoreHelper.GetCompleteName(person.FirstName, person.MiddleName, person.LastName);
                    //In case if any other details are to be added then they can be added here
                }

                if(!string.IsNullOrEmpty(personDetails))
                {
                    metadata.Add(MetadataProviderHelper.GetElement(MetadataProviderHelper.Metadata_Contributor, personDetails));
                }
            });
        }

        /// <summary>
        /// Creates string of tags related to resource
        /// </summary>
        /// <param name="metadata">the associated metadata</param>
        /// <param name="resource"> core.resource  </param>
        private static void AddResourceTags(XElement metadata, ScholarlyWorks.ScholarlyWorkItem resource)
        {
            if(null == metadata || null == resource)
            {
                return;
            }

            resource.Tags.ToList().ForEach(delegate(ScholarlyWorks.Tag tag)
            {
                if(!String.IsNullOrEmpty(tag.Name))
                {
                    metadata.Add(MetadataProviderHelper.GetElement(MetadataProviderHelper.Metadata_Subject, tag.Name));
                }
            });
        }

        #endregion
    }

    #endregion
}
