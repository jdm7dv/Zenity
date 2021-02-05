// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

 ﻿



namespace Zentity.Platform
{
    #region Using namespace

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Configuration;
    using System.Xml;
    using System.Xml.Linq;

    #endregion

    #region OaiPmhService Class

    /// <summary>
    /// OaiPmhService class implements the <typeref name="System.Web.IHttpHandler"/> so as to process OAI-PMH 
    /// specific requests that  are to be handled.
    /// </summary>
    public class OaiPmhService : IZentityService
    {
        #region Verbs

        /// <summary>
        /// Identify request verb.
        /// </summary>
        private const string Verb_Identify = "Identify";

        /// <summary>
        /// ListIdentifiers request verb.
        /// </summary>
        private const string Verb_ListIdentifiers = "ListIdentifiers";

        /// <summary>
        /// GetRecord request verb.
        /// </summary>
        private const string Verb_GetRecord = "GetRecord";

        /// <summary>
        /// ListSets request verb.
        /// </summary>
        private const string Verb_ListSets = "ListSets";

        /// <summary>
        /// ListMetadataFormats request verb.
        /// </summary>
        private const string Verb_ListMetadataFormats = "ListMetadataFormats";

        /// <summary>
        /// ListRecords request verb.
        /// </summary>
        private const string Verb_ListRecords = "ListRecords";

        #endregion

        #region Request Specific

        /// <summary>
        /// The verb query parameter associated with the HTTP Request.
        /// </summary>
        public const string RequestVerb = "verb";

        /// <summary>
        /// The metadataPrefix query parameter associated with the HTTP Request.
        /// </summary>
        public const string RequestMetadataPrefix = "metadataPrefix";

        /// <summary>
        /// The identifier query parameter associated with the HTTP Request.
        /// </summary>
        public const string RequestIdentifier = "identifier";

        /// <summary>
        /// The resumptionToken query parameter associated with the HTTP Request.
        /// </summary>
        public const string RequestResumptionToken = "resumptionToken";

        /// <summary>
        /// The set query parameter associated with the HTTP Request.
        /// </summary>
        public const string RequestSet = "set";

        /// <summary>
        /// The from query parameter associated with the HTTP Request.
        /// </summary>
        public const string RequestFrom = "from";

        /// <summary>
        /// The until query parameter associated with the HTTP Request.
        /// </summary>
        public const string RequestUntil = "until";

        #endregion

        #region Attributes

        /// <summary>
        /// Xml namespace attribute name xmlns.
        /// </summary>
        public const string XmlnsAttribute = "xmlns";

        /// <summary>
        /// Xml namespace associated with the OAI-PMH protocol.
        /// </summary>
        public const string Xmlns = "http://www.openarchives.org/OAI/2.0/";

        /// <summary>
        /// Xml namespace attribute name for oai_dc i.e. xmlns:oai_dc.
        /// </summary>
        public const string XmlnsOaidcAttribute = "xmlns:oai_dc";

        /// <summary>
        /// /// Xml namespace associated with oai_dc.
        /// </summary>
        public const string XmlnsOaidc = "http://www.openarchives.org/OAI/2.0/oai_dc/";

        /// <summary>
        /// Xml namespace attribute name for Dc i.e. xmlns:oai_dc.
        /// </summary>
        public const string XmlnsDCAttribute = "xmlns:dc";

        /// <summary>
        /// /// Xml namespace associated with Dc.
        /// </summary>
        public const string XmlnsDC = "http://purl.org/dc/elements/1.1/";

        /// <summary>
        /// Xml namespace xsi attribute name xmlns:xsi.
        /// </summary>
        public const string XmlnsXsiAttribute = "xmlns:xsi";

        /// <summary>
        /// Xml schema instance location associated with the OAI-PMH protocol.
        /// </summary>
        public const string XmlnsXsi = "http://www.w3.org/2001/XMLSchema-instance";

        /// <summary>
        /// Xml schema location attribute name xsi:schemaLocation.
        /// </summary>
        public const string XsiSchemaLocationAttribute = "xsi:schemaLocation";

        /// <summary>
        /// Xml schema location associated with the OAI-PMH protocol.
        /// </summary>
        public const string XsiSchemaLocation = "http://www.openarchives.org/OAI/2.0/ http://www.openarchives.org/OAI/2.0/OAI-PMH.xsd";

        /// <summary>
        /// Xml schema location associated with the OAI-PMH protocol for oai-dc.
        /// </summary>
        public const string XsiSchemaLocationOaidc = "http://www.openarchives.org/OAI/2.0/oai_dc/ http://www.openarchives.org/OAI/2.0/oai_dc.xsd";

        /// <summary>
        /// Xml schema namespace associated with the OAI-PMH protocol for oai-dc.
        /// </summary>
        public const string OaidcSchemaNamespace = "http://www.openarchives.org/OAI/2.0/oai_dc/";

        /// <summary>
        /// Xml schema namespace associated with the OAI-PMH protocol.
        /// </summary>
        public const string OaidcSchema = "http://www.openarchives.org/OAI/2.0/oai_dc.xsd";

        #endregion

        #region Node Names

        /// <summary>
        /// The root node associated with the Response Xml.
        /// </summary>
        public const string XmlResponseRootNode = "OAI-PMH";

        /// <summary>
        /// The root node associated with the oai_dc tag for the metadata in the Response Xml.
        /// </summary>
        public const string XmlResponseOaidcRootNode = "oai_dc:dc";

        /// <summary>
        /// The response node associated with the Response Xml.
        /// </summary>
        public const string XmlResponseResponseDate = "responseDate";

        /// <summary>
        /// The request node associated with the Response Xml.
        /// </summary>
        public const string XmlResponseRequest = "request";

        /// <summary>
        /// The error node associated with the Response Xml.
        /// </summary>
        public const string XmlResponseError = "error";

        /// <summary>
        /// The code node associated with the Response Xml.
        /// </summary>
        public const string XmlResponseCode = "code";

        /// <summary>
        /// The RepositoryName node associated with the Response Xml.
        /// </summary>
        public const string XmlResponseRepositoryName = "repositoryName";

        /// <summary>
        /// The baseUrl node associated with the Response Xml.
        /// </summary>
        public const string XmlResponseBaseUrl = "baseURL";

        /// <summary>
        /// The protocolVersion node associated with the Response Xml.
        /// </summary>
        public const string XmlResponseProtocolVersion = "protocolVersion";

        /// <summary>
        /// The compression node associated with the Response Xml.
        /// </summary>
        public const string XmlResponseCompression = "compression";

        /// <summary>
        /// The ListMetadataFormats node associated with the Response Xml.
        /// </summary>
        public const string XmlResponseListMetadataFormats = "ListMetadataFormats";

        /// <summary>
        /// The metadataFormat node associated with the Response Xml.
        /// </summary>
        public const string XmlResponseMetadataFormat = "metadataFormat";

        /// <summary>
        /// The metadataPrefix node associated with the Response Xml.
        /// </summary>
        public const string XmlResponseMetadataPrefix = "metadataPrefix";

        /// <summary>
        /// The schema node associated with the Response Xml.
        /// </summary>
        public const string XmlResponseSchema = "schema";

        /// <summary>
        /// The metadataNamespace node associated with the Response Xml.
        /// </summary>
        public const string XmlResponseMetadataNamespace = "metadataNamespace";

        /// <summary>
        /// The resumptionToken node associated with the Response Xml.
        /// </summary>
        public const string XmlResponseResumptionToken = "resumptionToken";

        /// <summary>
        /// The header node associated with the Response Xml.
        /// </summary>
        public const string XmlResponseHeader = "header";

        /// <summary>
        /// The metadata node associated with the Response Xml.
        /// </summary>
        public const string XmlResponseMetadata = "metadata";

        #endregion

        #region Error Codes

        /// <summary>
        /// The badVerb attribute associated with the Response Xml.
        /// </summary>
        public const string XmlResponseBadVerb = "badVerb";

        /// <summary>
        /// The badArgument attribute associated with the Response Xml.
        /// </summary>
        public const string XmlResponseBadArgument = "badArgument";

        /// <summary>
        /// The cannotDisseminateFormat attribute associated with the Response Xml.
        /// </summary>
        public const string XmlResponseCannotDisseminateFormat = "cannotDisseminateFormat";

        /// <summary>
        /// The noRecordsMatch attribute associated with the Response Xml.
        /// </summary>
        public const string XmlResponseNoRecordsMatch = "noRecordsMatch";

        /// <summary>
        /// The idDoesNotExist attribute associated with the Response Xml.
        /// </summary>
        public const string XmlResponseIdDoesNotExist = "idDoesNotExist";

        /// <summary>
        /// The badResumptionToken attribute associated with the Response Xml.
        /// </summary>
        public const string XmlResponseBadResumptionToken = "badResumptionToken";

        /// <summary>
        /// The noSetHierarchy attribute associated with the Response Xml.
        /// </summary>
        public const string XmlResponseNoSetHierarchy = "noSetHierarchy";

        #endregion

        #region OAI-PMH Service

        #region Protocol Specific

        /// <summary>
        /// MetadataPrefix associated with the OAI-PMH protocol; Currently only "oai_dc" is supported.
        /// </summary>
        public const string MetadataFormat = "oai_dc";

        /// <summary>
        /// protocol associated with OAI-PMH set to "2.0".
        /// </summary>
        public const string ProtocolVersion = "2.0";

        /// <summary>
        /// Compression Supported associated with OAI-PMH Protocol set to "no".
        /// </summary>
        public const string CompressionSupported = "no";

        #endregion

        #region Miscellaneous

        /// <summary>
        /// The content type associated with the HTTP Response for e.g. "text/xml".
        /// </summary>
        public const string ContentType = "text/xml";

        /// <summary>
        /// The prefix associated with the oai_dc namespace.
        /// </summary>
        public const string OaidcPrefix = "dc:";

        /// <summary>
        /// The date time format granularity that is supported by the Repository.
        /// </summary>
        public const string DateTimeGranularity = "yyyy-MM-ddTHH:mm:ssZ";

        /// <summary>
        /// The short date time format to check if the input date is correct or not.
        /// </summary>
        public const string DateTimeShortFormat = "yyyy-MM-dd";

        /// <summary>
        /// The date time format granularity that is supported by the Repository in UTC Format.
        /// </summary>
        public const string DateTimeGranularityUtcFormat = "YYYY-MM-DDThh:mm:ssZ";

        #endregion

        #endregion

        #region Private Member variables

        private static string[] OaiPmhVerbs = 
                                    { 
                                        Verb_Identify, 
                                        Verb_ListIdentifiers, 
                                        Verb_GetRecord,                                         
                                        Verb_ListSets, 
                                        Verb_ListMetadataFormats,
                                        Verb_ListRecords
                                    };

        private static string[] OaiPmhValidArguments =
                                    {
                                        RequestIdentifier,
                                        RequestMetadataPrefix,
                                        RequestFrom,
                                        RequestUntil,
                                        RequestSet,
                                        RequestResumptionToken
                                    };

        private string supportedMetadataFormat = string.Empty;
        private string selectedVerb = string.Empty;
        private static string _entityConnectionString;
        XDocument _result;

        #endregion

        #region .ctor

        /// <summary>
        /// Default constructor for OaiPmhService class.
        /// </summary>
        public OaiPmhService()
        {
            this.supportedMetadataFormat = OaiPmhService.GetMetadataFormat();
            _entityConnectionString = ConfigurationManager.AppSettings["CoreConnectionString"];
        }

        /// <summary>
        /// Constructor for OaiPmhService class.
        /// </summary>
        public OaiPmhService(string entityConnectionString)
        {
            this.supportedMetadataFormat = OaiPmhService.GetMetadataFormat();
            _entityConnectionString = entityConnectionString;
        }

        #endregion

        #region IHttpHandler Members

        /// <summary>
        /// Gets a value indicating whether another request can use the System.Web.IHttpHandler
        /// instance.
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Validates the request uri and returns the appropriate error message. 
        /// </summary>
        /// <param name="context">HttpContext containing the request object.</param>
        /// <param name="errorMessage">Contains the error message.</param>
        /// <returns>True if the request is valid, else false.</returns>
        public bool ValidateRequest(HttpContext context, out string errorMessage)
        {
            errorMessage = string.Empty;

            #region Retrieve Metadata

            if(null == context.Response.OutputStream)
            {
                return false;
            }

            try
            {
                this.selectedVerb = OaiPmhService.GetVerb(context.Request.QueryString);

                this.supportedMetadataFormat = OaiPmhService.GetMetadataFormat();

                //Retrieve the query parameters in hashtable and check for supported metadataPrefix
                Hashtable hashTableQueryParamaters = OaiPmhService.GetQueryParameters(context.Request.QueryString);

                _result = this.RetrieveMetadata(hashTableQueryParamaters);
            }
            catch(BadArgumentException exception)
            {
                _result = GetXmlResponse_ErrorCode(XmlResponseBadArgument, exception.Message);
                errorMessage = XmlResponseBadArgument + ":" + exception.Message;
            }
            catch(BadVerbException exception)
            {
                _result = GetXmlResponse_ErrorCode(XmlResponseBadVerb, exception.Message);
                errorMessage = XmlResponseBadVerb + ":" + exception.Message;
            }
            catch(CannotDisseminateFormatException exception)
            {
                _result = GetXmlResponse_ErrorCode(XmlResponseCannotDisseminateFormat, exception.Message);
                errorMessage = XmlResponseCannotDisseminateFormat + ":" + exception.Message;
            }
            #endregion
            //Always returns true - we need to pass on the validation error/s in XML
            return true;
        }

        /// <summary>
        /// Enables processing of HTTP Web requests.
        /// </summary>
        /// <param name="context">
        /// An System.Web.HttpContext object that provides references to the intrinsic
        ///  server objects (for example, Request, Response, Session, and Server) used
        ///  to service HTTP requests.
        ///  </param>
        ///  <param name="statusCode">Returns the status of the request.</param>
        /// <returns>A string containing the response for the specified AtomPub request.</returns>
        public string ProcessRequest(HttpContext context, out HttpStatusCode statusCode)
        {
            #region Write output to Stream
            statusCode = HttpStatusCode.OK;
            try
            {
                context.Response.ContentType = ContentType;
                return UpdateOutputStream(context.Request);
            }
            catch(Exception)
            {
                statusCode = System.Net.HttpStatusCode.BadRequest;
                throw;
                //TODO: Would be SQL Exceptions if any and for Logging 
            }
            #endregion
        }

        #endregion

        #region Internal memeber functions

        /// <summary>
        /// Retrieves the verb associated with the request.
        /// </summary>
        /// <param name="queryVerb">
        /// The query string associated with the HTTP Request.
        /// </param>
        /// <returns>
        /// Returns the verb associated with the request.
        /// </returns>
        /// <exception cref="Zentity.Platform.BadVerbException">
        /// given <paramref name="queryVerb"/> is invalid.
        /// </exception>
        internal static string GetVerb(NameValueCollection queryVerb)
        {
            string requestVerb = Convert.ToString(queryVerb[RequestVerb], CultureInfo.InvariantCulture);
            if(string.IsNullOrEmpty(requestVerb) || !OaiPmhService.OaiPmhVerbs.Contains(requestVerb))
            {
                throw new BadVerbException(Properties.Resources.OAI_EXCEPTION_ILLEGAL_VERB);
            }
            return requestVerb;
        }

        /// <summary>
        /// Retrieves the metadata format associated with the OAI-PMH Service.
        /// </summary>
        /// <returns>
        /// Returns the supported metadata Format <example>oai_dc.</example>
        /// </returns>
        internal static string GetMetadataFormat()
        {
            return MetadataFormat;
        }

        /// <summary>
        /// Retrieves the query parameter associated with the request.
        /// </summary>
        /// <param name="queryString">
        /// The query string associated with the HTTP Request.
        /// </param>
        /// <returns>
        /// Returns the query parameter associated with the request.
        /// </returns>
        internal static Hashtable GetQueryParameters(NameValueCollection queryString)
        {
            Hashtable hashTable = new Hashtable();
            foreach(string key in queryString.Keys)
            {
                if(null == key)
                {
                    throw new BadArgumentException(Properties.Resources.OAI_EXCEPTION_BADARGUMENT);
                }
                hashTable.Add(key, queryString[key]);
            }
            return hashTable;
        }

        #endregion

        #region Private member functions

        #region Platform Invoke Functionality and Query String parameter checks

        /// <summary>
        /// Checks if the metadataPrefix associated with the request is supported by the Repository.
        /// </summary>
        /// <param name="hashTableQueryParamaters">The query parameters associated with the request.</param>
        /// <exception cref="Zentity.Platform.CannotDisseminateFormatException">
        /// the associated metadataPrefix is not supported.
        /// </exception>
        private void CheckForSupportedMetadataFormat(Hashtable hashTableQueryParamaters)
        {
            if(null == hashTableQueryParamaters ||
                !hashTableQueryParamaters.ContainsKey(RequestMetadataPrefix))
            {
                //We assume that if no metadata Prefix is specified in the request it would always be oai_dc.
                return;
            }

            string metadataPrefix = hashTableQueryParamaters[RequestMetadataPrefix] as string;
            if(!string.Equals(this.supportedMetadataFormat, metadataPrefix, StringComparison.Ordinal))
            {
                throw new CannotDisseminateFormatException(
                    string.Format(CultureInfo.InvariantCulture, Properties.Resources.OAI_EXCEPTION_INVALID_METADATAPREFIX, metadataPrefix));
            }
        }

        /// <summary>
        /// Checks if metadataPrefix is a part of query parameters. 
        /// Some OAI-PMH requests should give metadataPrefix as part of its request. 
        /// If not given then the functions returns false.
        /// </summary>
        /// <param name="hashTableQueryParamaters">
        /// The query parameters associated with the request.
        /// </param>
        /// <returns>
        /// true, if  metadataPrefix is a part of query parameters else false.
        /// </returns>
        private static bool IsMetadataPrefixaQueryParameter(Hashtable hashTableQueryParamaters)
        {
            if(!hashTableQueryParamaters.ContainsKey(RequestMetadataPrefix))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if set is a part of query parameters If not given then the functions returns false.
        /// </summary>
        /// <param name="hashTableQueryParamaters">
        /// The query parameters associated with the request.
        /// </param>
        /// <returns>
        /// true, if set is a part of query parameters else false.
        /// </returns>
        private static bool IsSetaQueryParameter(Hashtable hashTableQueryParamaters)
        {
            if(!hashTableQueryParamaters.ContainsKey(RequestSet))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Retrieves the metadata from the Zentity Repository.
        /// </summary>
        /// <param name="hashTableQueryParamaters">
        /// Query parameters associated with the request.
        /// </param>
        /// <returns>The result in XDocument format.</returns>
        private XDocument RetrieveMetadata(Hashtable hashTableQueryParamaters)
        {
            XDocument result = null;

            this.CheckForSupportedMetadataFormat(hashTableQueryParamaters);
            if(!this.CheckArguments(hashTableQueryParamaters, ref result))
            {
                return result;
            }

            XDocument badArgumentError = OaiPmhService.GetXmlResponse_ErrorCode(XmlResponseBadArgument, Properties.Resources.OAI_EXCEPTION_BADARGUMENT);

            #region Retrieve Identifier Value

            Guid identifierGuid = Guid.Empty;
            Guid resumptionToken = Guid.Empty;
            try
            {
                if(hashTableQueryParamaters.ContainsKey(RequestIdentifier))
                {
                    identifierGuid = OaiPmhService.RetrieveGuid(hashTableQueryParamaters, RequestIdentifier);
                }

                if(hashTableQueryParamaters.ContainsKey(RequestResumptionToken))
                {
                    resumptionToken = OaiPmhService.RetrieveGuid(hashTableQueryParamaters, RequestResumptionToken);
                }

                //if (hashTableQueryParamaters.ContainsKey(Request_Set))
                //{
                //    this.RetrieveGuid(hashTableQueryParamaters, Request_Set);
                //}
            }
            catch(FormatException)
            {
                return badArgumentError;
            }
            catch(OverflowException)
            {
                return badArgumentError;
            }

            #endregion

            Exception invalidArgumentException = null;
            try
            {
                MetadataProvider metadataProvider = OaiPmhService.MetadataProvider;
                switch(this.selectedVerb)
                {
                    case Verb_Identify:
                        result = metadataProvider.RepositoryDetails;
                        break;

                    case Verb_GetRecord:
                        if(null == identifierGuid || Guid.Empty == identifierGuid)
                        {
                            result = OaiPmhService.GetXmlResponse_ErrorCode(XmlResponseBadArgument);
                            break;
                        }

                        if(!OaiPmhService.IsMetadataPrefixaQueryParameter(hashTableQueryParamaters))
                        {
                            result = OaiPmhService.GetXmlResponse_ErrorCode(XmlResponseBadArgument, Properties.Resources.OAI_EXCEPTION_NO_METADATAPREFIX);
                            break;
                        }

                        result = metadataProvider.GetRecord(identifierGuid);

                        break;

                    case Verb_ListSets:
                        if(!metadataProvider.CheckIfSetHierarchyExists())
                        {
                            result = OaiPmhService.GetXmlResponse_ErrorCode(XmlResponseNoSetHierarchy, Properties.Resources.OAI_EXCEPTION_NOSETHIRARCHY);
                            break;
                        }

                        result = metadataProvider.ListSets();
                        break;

                    case Verb_ListMetadataFormats:
                        if(!(null == identifierGuid || Guid.Empty == identifierGuid))
                        {
                            if(!metadataProvider.IdentifierExists(identifierGuid))
                            {
                                result = OaiPmhService.GetXmlResponse_ErrorCode(XmlResponseIdDoesNotExist, Properties.Resources.OAI_EXCEPTION_ILLEGAL_ID);
                                break;
                            }
                        }

                        result = OaiPmhService.GetXmlResponse_MetadataFormat();
                        break;

                    case Verb_ListRecords:
                        if(hashTableQueryParamaters.ContainsKey(RequestResumptionToken))
                        {
                            if(null == resumptionToken || Guid.Empty == resumptionToken)
                            {
                                result = GetXmlResponse_ErrorCode(XmlResponseBadResumptionToken, Properties.Resources.OAI_EXCEPTION_INVALID_RESUMPTIONTOKEN);
                                break;
                            }
                            result = metadataProvider.ListRecords(resumptionToken);
                        }
                        else
                        {
                            if(OaiPmhService.CheckArgumentforListFunctions(hashTableQueryParamaters, metadataProvider, ref result))
                            {
                                //If its the first request call the overload for ListRecords which takes hash Table as the parameter.
                                result = metadataProvider.ListRecords(hashTableQueryParamaters);
                            }
                        }

                        break;

                    case Verb_ListIdentifiers:
                        if(hashTableQueryParamaters.ContainsKey(RequestResumptionToken))
                        {
                            if(null == resumptionToken || Guid.Empty == resumptionToken)
                            {
                                result = GetXmlResponse_ErrorCode(XmlResponseBadResumptionToken, Properties.Resources.OAI_EXCEPTION_INVALID_RESUMPTIONTOKEN);
                                break;
                            }
                            result = metadataProvider.ListIdentifiers(resumptionToken);
                        }
                        else
                        {
                            if(OaiPmhService.CheckArgumentforListFunctions(hashTableQueryParamaters, metadataProvider, ref  result))
                            {
                                //If its the first request call the overload for ListIdentifiers which takes hash Table as the parameter.
                                result = metadataProvider.ListIdentifiers(hashTableQueryParamaters);
                            }
                        }

                        break;
                }
            }
            catch(ArgumentNullException)
            {
                // In case of any badArgument.
                result = GetXmlResponse_ErrorCode(XmlResponseBadArgument, Properties.Resources.OAI_EXCEPTION_BADARGUMENT);
            }
            catch(ArgumentException ex)
            {
                // In case if there are no records for the values in .Core
                invalidArgumentException = ex;
            }
            catch(InvalidOperationException ex)
            {
                // In case if there are no records for the values in .Core
                invalidArgumentException = ex;
            }
            result = this.CheckExceptionConditions(hashTableQueryParamaters, invalidArgumentException, result);

            bool noRecordsRetrieved = null == result || string.IsNullOrEmpty(result.ToString());
            if(noRecordsRetrieved)
            {
                result = badArgumentError;
            }

            return result;
        }

        private static MetadataProvider MetadataProvider
        {
            get
            {
                string maxHarvestCount = ConfigurationManager.AppSettings["MaximumHarvestCount"];
                string resumptionTokenCount = ConfigurationManager.AppSettings["ResumptionTokenExpirationTime"];
                int maximumHarvestCount = 100;
                int resumptionTokenExpirationTime = 30;
                if(!string.IsNullOrEmpty(maxHarvestCount))
                {
                    if(!Int32.TryParse(maxHarvestCount, out maximumHarvestCount))
                    {
                        maximumHarvestCount = 100;
                    }
                }

                if(!string.IsNullOrEmpty(resumptionTokenCount))
                {
                    if(!Int32.TryParse(resumptionTokenCount, out resumptionTokenExpirationTime))
                    {
                        resumptionTokenExpirationTime = 30;
                    }
                }
                return new MetadataProvider(maximumHarvestCount, resumptionTokenExpirationTime, _entityConnectionString);
            }
        }

        /// <summary>
        /// Retrieves the Guid value from the given request query parameters.
        /// </summary>
        /// <param name="hashTableQueryParamaters">
        /// Query parameters associated with the request.
        /// </param>
        /// <param name="key">
        /// The key for which the guid is to be retrieved.
        /// </param>
        /// <returns>Valid Guid.</returns>
        private static Guid RetrieveGuid(Hashtable hashTableQueryParamaters, string key)
        {
            Guid validGuid = new Guid((string)hashTableQueryParamaters[key]);
            if(null == validGuid || Guid.Empty == validGuid)
            {
                throw new BadArgumentException(Properties.Resources.OAI_EXCEPTION_BADARGUMENT);
            }
            return validGuid;
        }

        /// <summary>
        /// Retrieves the DateTime value from the given request query parameters.
        /// </summary>
        /// <param name="hashTableQueryParamaters">
        /// Query parameters associated with the request.
        /// </param>
        /// <param name="key">
        /// The key for which the DateTime is to be retrieved.
        /// </param>
        /// <param name="result">
        /// The XDocument that would contain the error code in case of any errors.
        /// </param>
        /// <returns>Valid DateTime.</returns>
        private static bool CheckDateTimeFormat(Hashtable hashTableQueryParamaters, string key, ref XDocument result)
        {
            string date = hashTableQueryParamaters[key] as string;
            XDocument tempResult = OaiPmhService.GetXmlResponse_ErrorCode(
                                   XmlResponseBadArgument,
                                   string.Format(CultureInfo.InvariantCulture,
                                   Properties.Resources.OAI_EXCEPTION_INVALID_DATETIME,
                                   new object[] { date, DateTimeGranularityUtcFormat }));
            DateTime validDateTime = DateTime.MinValue;
            if(!DateTime.TryParseExact(date, DateTimeGranularity, CultureInfo.InvariantCulture, DateTimeStyles.None, out validDateTime))
            {
                if(!DateTime.TryParseExact(date, DateTimeShortFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out validDateTime))
                {
                    result = tempResult;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks the arguments if they are valid for the given request.
        /// </summary>
        /// <param name="hashTableQueryParamaters">
        /// Query parameters associated with the request.
        /// </param>
        /// <param name="result">
        /// XmlDocument containing error codes if any.
        /// </param>
        /// <returns>
        /// true, if all arguments are valid else returns false.
        /// </returns>
        private bool CheckArguments(Hashtable hashTableQueryParamaters, ref XDocument result)
        {
            XDocument tempDocument = OaiPmhService.GetXmlResponse_ErrorCode(
                                    XmlResponseBadArgument,
                                    Properties.Resources.OAI_EXCEPTION_BADARGUMENT);
            if(null == hashTableQueryParamaters)
            {
                result = tempDocument;
                return false;
            }

            // Checks if all the given identifiers are valid arguments.
            foreach(string key in hashTableQueryParamaters.Keys)
            {
                if(key.Equals(RequestVerb, StringComparison.Ordinal))
                {
                    continue;
                }

                if(!OaiPmhService.OaiPmhValidArguments.Contains(key))
                {
                    result = tempDocument;
                    return false;
                }
            }

            bool areValidArgumentsPassed = this.CheckArgumentsForSelectedVerb(hashTableQueryParamaters);

            if(!areValidArgumentsPassed)
            {
                result = tempDocument;
            }

            return areValidArgumentsPassed;
        }

        private bool CheckArgumentsForSelectedVerb(Hashtable hashTableQueryParamaters)
        {
            bool areValidArgumentsPassed = true;
            // For each of the selected verbs check if there are no extra arguments. 
            switch(this.selectedVerb)
            {
                case Verb_Identify:
                case Verb_ListSets:
                    // required: none [For Sets we do not support resumption Token (specific to Zentity)]
                    foreach(string argument in OaiPmhService.OaiPmhValidArguments)
                    {
                        if(hashTableQueryParamaters.ContainsKey(argument))
                        {
                            areValidArgumentsPassed = false;
                            break;
                        }
                    }

                    break;

                case Verb_GetRecord:
                    // required: "identifier,metadataPrefix"
                    foreach(string argument in OaiPmhService.OaiPmhValidArguments)
                    {
                        if(hashTableQueryParamaters.ContainsKey(argument))
                        {
                            if(!(argument.Equals(RequestIdentifier, StringComparison.Ordinal) ||
                               argument.Equals(RequestMetadataPrefix, StringComparison.Ordinal)))
                            {
                                areValidArgumentsPassed = false;
                                break;
                            }
                        }
                    }

                    break;

                case Verb_ListMetadataFormats:
                    // required: "identifier"
                    foreach(string argument in OaiPmhService.OaiPmhValidArguments)
                    {
                        if(hashTableQueryParamaters.ContainsKey(argument))
                        {
                            if(!argument.Equals(RequestIdentifier, StringComparison.Ordinal))
                            {
                                areValidArgumentsPassed = false;
                                break;
                            }
                        }
                    }

                    break;

                case Verb_ListIdentifiers:
                case Verb_ListRecords:
                    // required: "metadataPrefix, from, until, set" and resumptionToken exclusive
                    foreach(string argument in OaiPmhService.OaiPmhValidArguments)
                    {
                        if(hashTableQueryParamaters.ContainsKey(RequestResumptionToken))
                        {
                            // Resumption Token Flow 
                            if(hashTableQueryParamaters.ContainsKey(argument))
                            {
                                if(!argument.Equals(RequestResumptionToken, StringComparison.Ordinal))
                                {
                                    areValidArgumentsPassed = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // Normal Flow 
                            if(hashTableQueryParamaters.ContainsKey(argument))
                            {
                                if(argument.Equals(RequestIdentifier, StringComparison.Ordinal))
                                {
                                    areValidArgumentsPassed = false;
                                    break;
                                }
                            }
                        }
                    }

                    break;
            }
            return areValidArgumentsPassed;
        }

        /// <summary>
        /// Checks if the arguments associated with the request are valid or not. 
        /// </summary>
        /// <param name="hashTableQueryParamaters">
        /// The query parameters associated with the request.
        /// </param>
        /// <param name="metadataProvider">
        /// The metadataProvider associated with the request.
        /// </param>
        /// <param name="result">
        /// A reference parameter containing the result set.
        /// </param>
        /// <returns>true, if all arguments are valid else false.</returns>
        private static bool CheckArgumentforListFunctions(Hashtable hashTableQueryParamaters, MetadataProvider metadataProvider, ref XDocument result)
        {
            if(null == metadataProvider || null == hashTableQueryParamaters)
            {
                return true;
            }

            if(!OaiPmhService.IsMetadataPrefixaQueryParameter(hashTableQueryParamaters))
            {
                result = OaiPmhService.GetXmlResponse_ErrorCode(XmlResponseBadArgument, Properties.Resources.OAI_EXCEPTION_NO_METADATAPREFIX);
                return false;
            }

            if(OaiPmhService.IsSetaQueryParameter(hashTableQueryParamaters) &&
                !metadataProvider.CheckIfSetHierarchyExists())
            {
                result = OaiPmhService.GetXmlResponse_ErrorCode(XmlResponseNoSetHierarchy, Properties.Resources.OAI_EXCEPTION_NOSETHIRARCHY);
                return false;
            }

            if(hashTableQueryParamaters.ContainsKey(RequestFrom))
            {
                if(!OaiPmhService.CheckDateTimeFormat(hashTableQueryParamaters, RequestFrom, ref result))
                {
                    return false;
                }
            }

            if(hashTableQueryParamaters.ContainsKey(RequestUntil))
            {
                if(!OaiPmhService.CheckDateTimeFormat(hashTableQueryParamaters, RequestUntil, ref result))
                {
                    return false;
                }
            }
            if(hashTableQueryParamaters.ContainsKey(RequestSet))
            {
                string setSpec = hashTableQueryParamaters[RequestSet] as string;
                if(string.IsNullOrEmpty(setSpec) || !metadataProvider.CheckIfSetSpecExists(setSpec))
                {
                    result = OaiPmhService.GetXmlResponse_ErrorCode(XmlResponseBadArgument,
                                string.Format(CultureInfo.InvariantCulture,
                                Properties.Resources.OAI_EXCEPTION_INVALID_SET, string.IsNullOrEmpty(setSpec) ? string.Empty : setSpec));
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks for the given exception so as to return a appropriate error message if any.
        /// </summary>
        /// <param name="hashTableQueryParamaters">The query parameters associated with the request.</param>
        /// <param name="exception">The exception that is to be checked.</param>
        /// <param name="result">A XDocument containing the result set.</param>
        /// <returns>The result in XDocument format, contains the error node if any exceptions are thrown.</returns>
        private XDocument CheckExceptionConditions(Hashtable hashTableQueryParamaters, Exception exception, XDocument result)
        {
            if(null == exception ||
                !(exception is ArgumentException || exception is InvalidOperationException) ||
                string.IsNullOrEmpty(this.selectedVerb) ||
                null == hashTableQueryParamaters)
            {
                //Return the result as it is.
                return result;
            }

            switch(this.selectedVerb)
            {
                case Verb_Identify:
                case Verb_ListMetadataFormats:
                case Verb_ListSets:
                    result = OaiPmhService.GetXmlResponse_ErrorCode(XmlResponseBadArgument, Properties.Resources.OAI_EXCEPTION_BADARGUMENT);
                    break;

                case Verb_GetRecord:
                    result = OaiPmhService.GetXmlResponse_ErrorCode(XmlResponseIdDoesNotExist, Properties.Resources.OAI_EXCEPTION_ILLEGAL_ID);
                    break;

                case Verb_ListIdentifiers:
                case Verb_ListRecords:
                    if(hashTableQueryParamaters.ContainsKey(RequestResumptionToken))
                    {
                        //If its the request is for next set using resumption Token.
                        result = OaiPmhService.GetXmlResponse_ErrorCode(XmlResponseBadResumptionToken, Properties.Resources.OAI_EXCEPTION_INVALID_RESUMPTIONTOKEN);
                    }
                    else
                    {
                        result = OaiPmhService.GetXmlResponse_ErrorCode(XmlResponseNoRecordsMatch, Properties.Resources.OAI_EXCEPTION_EMPTY_RESULTSET);
                    }

                    break;
            }

            return result;
        }

        #endregion

        #region Local XDocument Creations

        /// <summary>
        /// Returns the error code xml tag.  
        /// </summary>
        /// <param name="errorCode">
        /// The associated error code.
        /// </param>
        /// <returns>
        /// Returns the supported metadata format.
        /// </returns>
        private static XDocument GetXmlResponse_ErrorCode(string errorCode)
        {
            //<error code ="<paramref name="errorCode"/>">errorMessage</error>
            return OaiPmhService.GetXmlResponse_ErrorCode(errorCode, string.Empty);
        }

        /// <summary>
        /// Returns the error code xml tag. 
        /// </summary>
        /// <param name="errorCode">
        /// The associated error code.
        /// </param>
        /// <param name="errorMessage">
        /// The associated error Message.
        /// </param>
        /// <returns>
        /// Returns the supported metadata format.
        /// </returns>
        private static XDocument GetXmlResponse_ErrorCode(string errorCode, string errorMessage)
        {
            // <error code ="<paramref name="errorCode"/>">errorMessage</error>
            if(string.IsNullOrEmpty(errorCode))
            {
                throw new ArgumentNullException("errorCode");
            }

            return new XDocument(
                                    new XElement(XmlResponseError,
                                        new XAttribute(XmlResponseCode, errorCode)
                                        , string.IsNullOrEmpty(errorMessage) ? string.Empty : errorMessage
                                    )
                                );
        }

        /// <summary>
        /// Returns metadataFormat in xml format. 
        /// Note: As of now we are not supporting identifier support for metadata formats.
        /// </summary>
        /// <returns>
        /// Returns the supported metadata format.
        /// </returns>
        private static XDocument GetXmlResponse_MetadataFormat()
        {
            // <metadataFormat>
            //     <metadataPrefix>oai_dc</metadataPrefix>
            //     <schema>http://www.openarchives.org/OAI/2.0/oai_dc.xsd</schema>
            //     <metadataNamespace>http://www.openarchives.org/OAI/2.0/oai_dc/</metadataNamespace>
            //  </metadataFormat>
            return new XDocument(
                                    new XElement(XmlResponseMetadataFormat,
                                        new XElement(XmlResponseMetadataPrefix, MetadataFormat),
                                        new XElement(XmlResponseSchema, OaidcSchema),
                                        new XElement(XmlResponseMetadataNamespace, OaidcSchemaNamespace)
                                    )
                                );
        }

        #endregion

        #region Response Creation Helper

        /// <summary>
        /// Retrieves the Request Url.
        /// </summary>
        /// <param name="request">
        /// The associated HTTP request.
        /// </param>
        /// <returns>
        /// The Url for the Request.
        /// </returns>
        private static string GetRequestUri(HttpRequest request)
        {
            if(null == request)
            {
                return string.Empty;
            }
            if(string.IsNullOrEmpty(request.Url.Query))
            {
                return request.Url.AbsoluteUri;
            }

            return request.Url.AbsoluteUri.Replace(request.Url.Query, string.Empty);
        }

        /// <summary>
        /// Retrieves the error code tag from the given xml if it exists else it returns an empty string.
        /// </summary>
        /// <param name="errorDocument">
        /// The Xml from which the error code is to be retrieved.
        /// </param>
        /// <returns>
        /// 
        /// </returns>
        private static string GetErrorToken(XDocument errorDocument)
        {
            if(null == errorDocument || string.IsNullOrEmpty(errorDocument.ToString()))
            {
                return string.Empty;
            }
            XElement element = errorDocument.Elements().
                                Where(
                                elementNode =>
                                elementNode.Name.LocalName.Equals(
                                XmlResponseError, StringComparison.Ordinal)).FirstOrDefault();

            if(null == element || element.Attributes().Count() <= 0)
            {
                return string.Empty;
            }
            return element.Attributes().FirstOrDefault().Value;
        }

        #endregion

        #region Response Xml Generation

        #region Main

        /// <summary>
        /// Converts the memory stream which containing xml data into string
        /// </summary>
        /// <param name="xmlStream">Memory srteam to be converted</param>
        /// <returns>Xml string</returns>
        public static string MemoryStreamToXmlString(Stream xmlStream)
        {
            XmlTextReader reader = new XmlTextReader(xmlStream);
            StringBuilder sb = new StringBuilder();

            while(reader.Read())
            {
                sb.Append(reader.ReadOuterXml());
            }

            reader.Close();
            return sb.ToString();
        }
        private string UpdateOutputStream(HttpRequest request)
        {
            string errorToken = OaiPmhService.GetErrorToken(_result);

            using(MemoryStream textWriter = new MemoryStream())//System.Text.Encoding.UTF8))
            {

                XmlTextWriter writer = new XmlTextWriter(textWriter, System.Text.Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                {
                    writer.WriteStartElement(XmlResponseRootNode);
                    {
                        writer.WriteAttributeString(XmlnsAttribute, Xmlns);
                        writer.WriteAttributeString(XmlnsXsiAttribute, XmlnsXsi);
                        writer.WriteAttributeString(XsiSchemaLocationAttribute, XsiSchemaLocation);

                        // Add Response Date
                        writer.WriteElementString(XmlResponseResponseDate, Xmlns, DateTime.Now.ToUniversalTime().ToString(DateTimeGranularity, CultureInfo.InvariantCulture));

                        // Add Request Uri
                        writer.WriteStartElement(XmlResponseRequest, Xmlns);
                        {
                            //If the error token is not associated with the verb enter the request details.
                            if(string.IsNullOrEmpty(errorToken) ||
                                (!errorToken.Equals(XmlResponseBadVerb, StringComparison.Ordinal)))
                            {
                                foreach(string key in request.QueryString.Keys)
                                {
                                    if(string.IsNullOrEmpty(key))
                                    {
                                        continue;
                                    }

                                    writer.WriteAttributeString(key, request.QueryString[key]);
                                }
                            }
                            writer.WriteString(OaiPmhService.GetRequestUri(request));

                        }
                        writer.WriteEndElement(); //request

                        //In case if error code is not null then write the response element
                        if(string.IsNullOrEmpty(errorToken) && _result != null)
                        {
                            writer.WriteStartElement(this.selectedVerb, Xmlns);
                            {
                                this.WriteSpecificVerb(writer, _result);
                            }
                            writer.WriteEndElement(); //selectedVerb
                        }
                        else
                        {
                            //If it is null then write the error code
                            OaiPmhService.WriteElements(writer, _result, false);
                        }
                    }
                    writer.WriteEndElement(); // OAI-PMH
                }
                writer.WriteEndDocument();
                writer.Flush();
                textWriter.Seek(0, SeekOrigin.Begin);

                return MemoryStreamToXmlString(textWriter);
            }
        }

        #endregion

        #region Helper

        private void WriteSpecificVerb(XmlTextWriter writer, XDocument result)
        {
            if(null == writer)
            {
                return;
            }
            switch(this.selectedVerb)
            {
                case Verb_Identify:
                    OaiPmhService.WriteElementsForIdentify(writer, result);
                    break;
                case Verb_GetRecord:
                case Verb_ListRecords:
                    OaiPmhService.WriteElementForRecords(writer, result);
                    break;

                case Verb_ListSets:
                case Verb_ListIdentifiers:
                    OaiPmhService.WriteElements(writer, result, true);
                    break;
                case Verb_ListMetadataFormats:
                    OaiPmhService.WriteElements(writer, result, false);
                    break;
            }

        }

        private static void WriteElements(XmlTextWriter writer, XDocument result, bool ignoreFirstElement)
        {
            if(null == writer)
            {
                return;
            }
            if(null == result)
            {
                return;
            }
            IEnumerable<XElement> listOfElements = result.Elements();
            if(ignoreFirstElement)
            {
                listOfElements = result.Elements().FirstOrDefault().Elements();
            }
            foreach(XElement element in listOfElements)
            {	// ToString returns XMLDocument.OutterXML
                writer.WriteRaw(element.ToString());
            }
        }

        /// <summary>
        /// Write all element attributes and tags in oai_dc format.
        /// </summary>
        /// <param name="writer">The writer to which the xml output is to be written.</param>
        /// <param name="metadata">The associated xml that is to be written.</param>
        private static void WriteElementsForOai_Dc(XmlTextWriter writer, XElement metadata)
        {
            if(null == writer)
            {
                return;
            }
            if(null == metadata)
            {
                return;
            }
            IEnumerable<XElement> listOfElements = metadata.Elements();
            foreach(XElement element in listOfElements)
            {
                if(!string.IsNullOrEmpty(element.Value))
                {
                    writer.WriteElementString(string.Concat(OaidcPrefix, element.Name.LocalName), element.Value);
                }
            }
        }

        /// <summary>
        /// Write all element attributes and tags for Identify tag.
        /// </summary>
        /// <param name="writer">The writer to which the xml output is to be written.</param>
        /// <param name="identifyResult">The associated xml that is to be written.</param>
        private static void WriteElementsForIdentify(XmlTextWriter writer, XDocument identifyResult)
        {
            if(null == writer)
            {
                return;
            }
            if(null == identifyResult)
            {
                return;
            }

            AddIdentifyElements(identifyResult);
            OaiPmhService.WriteElements(writer, identifyResult, true);
        }

        /// <summary>
        /// Adds base_url, protocol version and compression element for Identify verb
        /// </summary>
        /// <param name="identifyResult">XDocument object containing identify elements </param>
        private static void AddIdentifyElements(XDocument identifyResult)
        {

            // <sequence>
            //   <element name="repositoryName" type="string" /> 
            //   <element name="baseURL" type="anyURI" /> 
            //   <element name="protocolVersion" type="oai:protocolVersionType" /> 
            //   <element name="adminEmail" type="oai:emailType" maxOccurs="unbounded" /> 
            //   <element name="earliestDatestamp" type="oai:UTCdatetimeType" /> 
            //   <element name="deletedRecord" type="oai:deletedRecordType" /> 
            //   <element name="granularity" type="oai:granularityType" /> 
            //   <element name="compression" type="string" minOccurs="0" maxOccurs="unbounded" /> 
            //   <element name="description" type="oai:descriptionType" minOccurs="0" maxOccurs="unbounded" /> 
            // </sequence>

            // retrieving the root element from XDocument
            XElement identify = identifyResult.Element(Verb_Identify);

            // if root not does not exist then return
            if(identify == null)
                return;

            // retrieving the first child element from root element
            XElement repository = identify.Element(XmlResponseRepositoryName);

            // creating base url element
            XElement baseUrl = new XElement(XmlResponseBaseUrl, WebConfigurationManager.AppSettings["PmhBaseUri"]);


            // if repository element exist as first node
            // then add base url as its next sibling or add it as first node of identify element
            if(repository == null)
            {
                identify.AddFirst(baseUrl);
            }
            else
            {
                repository.AddAfterSelf(baseUrl);
            }

            // create protocol version element 
            XElement protocolVersion = new XElement(XmlResponseProtocolVersion, ProtocolVersion);

            // add protocol version element as next sibling of base url
            baseUrl.AddAfterSelf(protocolVersion);

            // create compression element
            XElement compression = new XElement(XmlResponseCompression, CompressionSupported);

            // add compression element as last child of identify element
            identify.Add(compression);
        }



        /// <summary>
        /// Write all element attributes and tags for the List of Records.
        /// </summary>
        /// <param name="writer">
        /// The writer to which the xml output is to be written.
        /// </param>
        /// <param name="metadata">
        /// The associated xml that is to be written.
        /// </param>
        private static void WriteElementForRecords(XmlTextWriter writer, XDocument metadata)
        {
            if(null == writer)
            {
                return;
            }
            if(null == metadata)
            {
                return;
            }

            //Input Xml Document
            // <record> 
            //  <header>
            //      <identifier>oai:arXiv.org:cs/0112017</identifier> 
            //      <datestamp>2001-12-14</datestamp>
            //      <setSpec>cs</setSpec> 
            //      <setSpec>math</setSpec>
            //  </header>
            //  <metadata>            
            //      <title>Using Structural Metadata to Localize Experience of 
            //              Digital Content</title> 
            //      <creator>Dushay, Naomi</creator>
            //      <subject>Digital Libraries</subject> 
            //      <description>With the increasing technical sophistication of 
            //                      both information consumers and providers, there is 
            //                      increasing demand for more meaningful experiences of digital 
            //                      information. We present a framework that separates digital 
            //                      object experience, or rendering, from digital object storage 
            //                      and manipulation, so the rendering can be tailored to 
            //                      particular communities of users.
            //      </description>             
            //      <date>2001-12-14</date>            
            //  </metadata>
            // </record>

            //Required Xml Document
            //<record> 
            //  <header>
            //      <identifier>oai:arXiv.org:cs/0112017</identifier> 
            //      <datestamp>2001-12-14</datestamp>
            //      <setSpec>cs</setSpec> 
            //      <setSpec>math</setSpec>
            //  </header>
            //  <metadata>
            //    <oai_dc:dc 
            //       xmlns:oai_dc="http://www.openarchives.org/OAI/2.0/oai_dc/" 
            //       xmlns:dc="http://purl.org/dc/elements/1.1/" 
            //       xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
            //       xsi:schemaLocation="http://www.openarchives.org/OAI/2.0/oai_dc/ 
            //       http://www.openarchives.org/OAI/2.0/oai_dc.xsd">
            //      <dc:title>Using Structural Metadata to Localize Experience of 
            //                Digital Content</dc:title> 
            //      <dc:creator>Dushay, Naomi</dc:creator>
            //      <dc:subject>Digital Libraries</dc:subject> 
            //      <dc:description>With the increasing technical sophistication of 
            //          both information consumers and providers, there is 
            //          increasing demand for more meaningful experiences of digital 
            //          information. We present a framework that separates digital 
            //          object experience, or rendering, from digital object storage 
            //          and manipulation, so the rendering can be tailored to 
            //          particular communities of users.
            //      </dc:description> 
            //      <dc:date>2001-12-14</dc:date>
            //    </oai_dc:dc>
            //  </metadata>
            //</record>

            //Retrieve List of Record Sets can be more than 1. Suppress GetRecord Tag.
            XElement getRecord = metadata.Elements().FirstOrDefault();
            //Retrieve Record Tag.
            foreach(XElement record in getRecord.Elements())
            {
                // This if condition is to check if the record tag is an resumption token tag 
                // if yes write the tag as it is.
                if(record.Name.LocalName.Equals(XmlResponseResumptionToken, StringComparison.Ordinal))
                {
                    OaiPmhService.WriteElements(writer, new XDocument(record), false);
                    continue;
                }
                //Retrieve Record Tag.                
                writer.WriteStartElement(record.Name.LocalName, Xmlns);//<record>
                {
                    //Retrieve Header Tag.                
                    foreach(XElement element in record.Elements())
                    {
                        switch(element.Name.LocalName)
                        {
                            case XmlResponseHeader://Write the header tag as it is to the Xml.
                                OaiPmhService.WriteElements(writer, new XDocument(element), false);//<header>
                                break;
                            case XmlResponseMetadata:
                                writer.WriteStartElement(element.Name.LocalName, Xmlns); //<metadata>
                                {
                                    OaiPmhService.WriteMetadataForRecords(writer, element);
                                }
                                writer.WriteEndElement(); //<metadata>
                                break;
                        }
                    }
                }
                writer.WriteEndElement(); //record           
            }

        }

        /// <summary>
        /// Write metadata for the List of Records.
        /// </summary>
        /// <param name="writer">
        /// The writer to which the xml output is to be written.
        /// </param>
        /// <param name="metadata">
        /// The associated xml that is to be written.
        /// </param>
        private static void WriteMetadataForRecords(XmlTextWriter writer, XElement metadata)
        {
            //    <oai_dc:dc 
            //       xmlns:oai_dc="http://www.openarchives.org/OAI/2.0/oai_dc/" 
            //       xmlns:dc="http://purl.org/dc/elements/1.1/" 
            //       xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
            //       xsi:schemaLocation="http://www.openarchives.org/OAI/2.0/oai_dc/ 
            //       http://www.openarchives.org/OAI/2.0/oai_dc.xsd">
            //      <dc:title>Using Structural Metadata to Localize Experience of 
            //                Digital Content</dc:title> 
            //      <dc:creator>Dushay, Naomi</dc:creator>
            //      <dc:subject>Digital Libraries</dc:subject> 
            //      <dc:description>With the increasing technical sophistication of 
            //          both information consumers and providers, there is 
            //          increasing demand for more meaningful experiences of digital 
            //          information. We present a framework that separates digital 
            //          object experience, or rendering, from digital object storage 
            //          and manipulation, so the rendering can be tailored to 
            //          particular communities of users.
            //      </dc:description> 
            //      <dc:date>2001-12-14</dc:date>
            //    </oai_dc:dc>

            writer.WriteStartElement(XmlResponseOaidcRootNode);
            {
                //Write all the namespace attributes.
                writer.WriteAttributeString(XmlnsOaidcAttribute, XmlnsOaidc);
                writer.WriteAttributeString(XmlnsDCAttribute, XmlnsDC);
                writer.WriteAttributeString(XmlnsXsiAttribute, XmlnsXsi);
                writer.WriteAttributeString(XsiSchemaLocationAttribute, XsiSchemaLocationOaidc);

                //Write the actual data.
                OaiPmhService.WriteElementsForOai_Dc(writer, metadata);
            }
            writer.WriteEndElement(); // oai_dc:dc
        }

        #endregion

        #endregion

        #endregion
    }

    #endregion
}
