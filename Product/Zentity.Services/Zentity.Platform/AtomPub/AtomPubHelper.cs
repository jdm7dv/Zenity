// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Data.Objects;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.Practices.EnterpriseLibrary.Data;
    using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
    using Zentity.Core;
    using Zentity.Platform.Properties;
    using Zentity.ScholarlyWorks;
    using Zentity.Security.Authentication;
    using Zentity.Security.AuthorizationHelper;

    /// <summary>
    /// Atom pub helper class.
    /// </summary>
    internal class AtomPubHelper
    {

        #region Fields

        private static Dictionary<string, string> mimeTypeMappings;
        private static string[] unknownContentHeaders = new string[] { "Content-Language",
                                                                        "Content-Location",
                                                                        "Content-MD5",
                                                                        "Content-Range"};
        #endregion

        #region Properties

        /// <summary>
        /// Gets the unknown content headers.
        /// </summary>
        /// <value>The unknown content headers.</value>
        public static string[] UnknownContentHeaders
        {
            get
            {
                return AtomPubHelper.unknownContentHeaders;
            }
        }

        #region UriTemplates

        /// <summary>
        /// Create array of all possible Uri templates
        /// </summary>
        private static Dictionary<AtomPubRequestType, UriTemplate> atomPubTemplates;

        /// <summary>
        /// Gets the atom pub templates.
        /// </summary>
        /// <value>The atom pub templates.</value>
        internal static Dictionary<AtomPubRequestType, UriTemplate> AtomPubTemplates
        {
            get
            {
                if(null == atomPubTemplates || 0 == atomPubTemplates.Count)
                {
                    AtomPubHelper.LoadUriTemplates();
                }
                return atomPubTemplates;
            }
        }

        /// <summary>
        /// Gets the collection URI template.
        /// </summary>
        /// <value>The collection URI template.</value>
        private static UriTemplate CollectionUriTemplate
        {
            get
            {
                // Resource Uri will be of format : {BaseUri}/{CollectionName}
                return new UriTemplate(string.Format(CultureInfo.InvariantCulture,
                                                    "{{{0}}}",
                                                    AtomPubParameterType.CollectionName.ToString()));
            }
        }

        /// <summary>
        /// Gets the collection URI with page number template.
        /// </summary>
        /// <value>The collection URI with page number template.</value>
        private static UriTemplate CollectionUriWithPageNoTemplate
        {
            get
            {
                // Resource Uri will be of format : {BaseUri}/{CollectionName}/{PageNo}
                return new UriTemplate(string.Format(CultureInfo.InvariantCulture,
                                                    "{{{0}}}/{{{1}}}",
                                                    AtomPubParameterType.CollectionName.ToString(),
                                                    AtomPubParameterType.PageNo.ToString()));
            }
        }

        /// <summary>
        /// Gets the edit member URI template.
        /// </summary>
        /// <value>The edit member URI template.</value>
        private static UriTemplate EditMemberUriTemplate
        {
            get
            {
                // Resource Uri will be of format : {BaseUri}/{CollectionName}/edit/{Id}
                return new UriTemplate(string.Format(CultureInfo.InvariantCulture,
                                                    "{{{0}}}/{1}/{{{2}}}",
                                                    AtomPubParameterType.CollectionName.ToString(),
                                                    AtomPubConstants.Edit,
                                                    AtomPubParameterType.Id.ToString()));
            }
        }

        /// <summary>
        /// Gets the edit media URI template.
        /// </summary>
        /// <value>The edit media URI template.</value>
        private static UriTemplate EditMediaUriTemplate
        {
            get
            {
                // Resource Uri will be of format : {BaseUri}/{CollectionName}/edit-media/{Id}
                return new UriTemplate(string.Format(CultureInfo.InvariantCulture,
                                                    "{{{0}}}/{1}/{{{2}}}",
                                                    AtomPubParameterType.CollectionName.ToString(),
                                                    AtomPubConstants.EditMedia,
                                                    AtomPubParameterType.Id.ToString()));
            }
        }

        /// <summary>
        /// Gets the service document template.
        /// </summary>
        /// <value>The service document template.</value>
        private static UriTemplate ServiceDocumentTemplate
        {
            get
            {
                // Resource Uri will be of format : {BaseUri}/ServiceDocument
                return new UriTemplate("ServiceDocument");
            }
        }

        #endregion

        /// <summary>
        /// Gets the MIME type mappings.
        /// </summary>
        /// <value>The MIME type mappings.</value>
        private static Dictionary<string, string> MimeTypeMappings
        {
            get
            {
                if(null == mimeTypeMappings)
                {
                    LoadFileExtensionMimTypes();
                }
                return AtomPubHelper.mimeTypeMappings;
            }
        }

        #endregion

        /// <summary>
        /// Gets the base uri from the configuration file.
        /// </summary>
        /// <returns>Base uri for the service.</returns>
        internal static string GetBaseUri()
        {
            string baseAddress = ConfigurationManager.AppSettings["AtomPubBaseUri"];
            // If base uri does not contain "/" at the end, then add it.
            if(!baseAddress.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                baseAddress += "/";
            }
            return PlatformConstants.GetServiceHostName() + baseAddress.ToLowerInvariant();
        }

        /// <summary>
        /// Gets the value of specified parameter from the request Uri.
        /// </summary>
        /// <param name="context">HttpContext containing the request uri.</param>
        /// <param name="baseUri">Base Uri for the specified request.</param>
        /// <param name="parameterName"><typeref name="AtomPubParameterType"/> name of the parameter whose value is to be retrieved.</param>
        /// <returns>String containing value for the specified parameter.</returns>
        internal static string GetValueOfParameterFromUri(HttpContext context, Uri baseUri, AtomPubParameterType parameterName)
        {
            UriTemplateMatch matchResult = AtomPubHelper.GetTemplateMatchFromUri(baseUri, context.Request.Url);

            if(null == matchResult)
            {
                return string.Empty;
            }

            string paramName = parameterName.ToString();
            string paramValue = string.Empty;

            foreach(string paramKey in matchResult.BoundVariables.Keys)
            {
                if(paramKey.Equals(paramName, StringComparison.OrdinalIgnoreCase))
                {
                    paramValue = matchResult.BoundVariables[paramKey];
                    break;
                }
            }

            return paramValue;
        }

        /// <summary>
        /// Get Date modified value of a specified resource.
        /// </summary>
        /// <param name="memberResourceId">Id of resource to get modified date.</param>
        /// <returns>Returns NULL if resource does not exists.
        /// Returns minimum date value if resource's modified date is empty in data store.</returns>
        private static DateTime? GetModifiedDate(string memberResourceId)
        {
            using(ZentityContext context = CoreHelper.CreateZentityContext())
            {
                Guid resourceId = new Guid(memberResourceId);
                return context.ScholarlyWorks()
                                      .Where(resource => resource.Id == resourceId)
                                      .AsEnumerable()
                                      .Select(resource => (null == resource) ? null :
                                             (null == resource.DateModified) ? DateTime.MinValue : resource.DateModified)
                                      .FirstOrDefault();
            }
        }

        /// <summary>
        /// Calculates the ETag value for a specified resource.
        /// </summary>
        /// <param name="memberResourceId">Is of the resource.</param>
        /// <returns>string containing ETag value for the resource.</returns>
        internal static string CalculateETag(string memberResourceId)
        {
            using(ZentityContext context = CoreHelper.CreateZentityContext())
            {
                DateTime? dateModified = GetModifiedDate(memberResourceId);

                if(null == dateModified)
                {
                    throw new ResourceNotFoundException(Resources.ATOMPUB_RESOURCE_NOT_FOUND);
                }

                return CalculateETag(memberResourceId, dateModified);
            }
        }

        /// <summary>
        /// Calculates the ETag value for a specified resource.
        /// </summary>
        /// <param name="memberResourceId">Is of the resource.</param>
        /// <param name="dateModified">Date modified value of resource</param>
        /// <returns>string containing ETag value for the resource.</returns>
        private static string CalculateETag(string memberResourceId, DateTime? dateModified)
        {
            if(null == dateModified)
            {
                return null;
            }

            string strDate = dateModified + memberResourceId;
            return strDate.GetHashCode().ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the type of request i.e. request to the Collection or Member or ServiceDocument etc.
        /// </summary>
        /// <param name="context">HttpContext containing the request uri.</param>
        /// <param name="baseUri">Base Uri for the specified request.</param>
        /// <returns>AtomPubRequestType</returns>
        internal static AtomPubRequestType GetAtomPubRequestType(HttpContext context, Uri baseUri)
        {
            UriTemplateMatch matchResults = GetTemplateMatchFromUri(baseUri, context.Request.Url);

            if(null == matchResults)
            {
                return AtomPubRequestType.Unknwon;
            }

            // Get the template from Uri
            UriTemplate templateUri = matchResults.Template;

            // Return the key of a matching template pattern.
            AtomPubRequestType requestType = AtomPubHelper.AtomPubTemplates
                                             .Where(pair => pair.Value.IsEquivalentTo(templateUri))
                                             .Select(pair => pair.Key)
                                             .FirstOrDefault();

            return requestType;
        }

        /// <summary>
        /// Checks the specified string value is unique identifier or not.
        /// </summary>
        /// <param name="id">String value containing unique identifier.</param>
        /// <returns>True if string value is unique identifier, else return false.</returns>
        internal static bool IsValidGuid(string id)
        {
            return null != id && Regex.IsMatch(id, AtomPubConstants.GuidFormat, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Checks if the specified collection name is valid or not.
        /// </summary>
        /// <param name="typeName">Name of the collection.</param>
        /// <returns>True if the collection name is valid.</returns>
        internal static bool IsValidCollectionType(string typeName)
        {
            Type resourceType = CoreHelper.GetSystemResourceType(typeName);

            if(resourceType == typeof(ScholarlyWorkContainer))
            {
                return false;
            }

            return typeof(ScholarlyWork).IsAssignableFrom(resourceType);
        }

        /// <summary>
        /// gets the file extension for a given MIME type.
        /// </summary>
        /// <param name="mimeType">MIME type name.</param>
        /// <returns>File extension for a given MIME type.</returns>
        internal static string GetFileExtension(string mimeType)
        {
            return MimeTypeMappings
                    .Where(pair => string.Compare(pair.Value, mimeType, StringComparison.OrdinalIgnoreCase) == 0)
                    .Select(pair => pair.Key)
                    .DefaultIfEmpty(AtomPubConstants.DefaultFileExtension)
                    .FirstOrDefault();
        }

        /// <summary>
        /// gets the file extension for a given MIME type.
        /// </summary>
        /// <param name="fileExtension">File extension for which MIME type is to be retrieved.</param>
        /// <returns>File extension for a given MIME type.</returns>
        internal static string GetMimeTypeFromFileExtension(string fileExtension)
        {
            return MimeTypeMappings
                    .Where(pair => string.Compare(pair.Key, fileExtension, StringComparison.OrdinalIgnoreCase) == 0)
                    .Select(pair => pair.Value)
                    .DefaultIfEmpty(AtomPubConstants.DefaultMimeType)
                    .FirstOrDefault();
        }

        /// <summary>
        /// Generates the person filter expression.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="paramExpression">The param expression.</param>
        /// <param name="filter">The filter.</param>
        internal static void GeneratePersonFilterExpression(
                                                        string propertyName, 
                                                        string propertyValue, 
                                                        System.Linq.Expressions.Expression paramExpression, 
                                                        ref System.Linq.Expressions.Expression filter)
        {
            if(!string.IsNullOrEmpty(propertyValue))
            {
                System.Linq.Expressions.Expression property = System.Linq.Expressions.Expression.Property(paramExpression, typeof(Person).GetProperty(propertyName));
                System.Linq.Expressions.Expression value = System.Linq.Expressions.Expression.Constant(propertyValue);

                if(null != filter)
                {
                    filter = System.Linq.Expressions.Expression.And(filter, System.Linq.Expressions.Expression.Equal(property, value));
                }
                else
                {
                    filter = System.Linq.Expressions.Expression.Equal(property, value);
                }
            }
        }

        /// <summary>
        /// Validates if the precondition is matching for the specified request type and 
        /// 'If-Match' or 'If-None-Match' header values. If none of the header values are specified, 
        /// then it will proceed.
        /// </summary>
        /// <param name="context">HttpContext containing the request details.</param>
        /// <param name="baseUri">Base uri to identify HTTP request.</param>
        /// <param name="statusCode">This out parameter returns the appropriate status code for the request.</param>
        /// <returns>True if the precondition is matching, else false.</returns>
        internal static bool ValidatePrecondition(
                                                HttpContext context, 
                                                Uri baseUri, 
                                                out HttpStatusCode statusCode)
        {
            string[] supportedHeaders = new string[] { AtomPubConstants.KeyIfMatch, 
                                                       AtomPubConstants.KeyIfNoneMatch,
                                                       AtomPubConstants.KeyIfModifiedSince,
                                                       AtomPubConstants.KeyIfUnmodifiedSince};

            if(0 == context.Request.Headers.AllKeys.Intersect(supportedHeaders).Count())
            {
                statusCode = HttpStatusCode.Accepted;
                return true;
            }

            // Calculate ETag
            string resourceId = GetValueOfParameterFromUri(context, baseUri, AtomPubParameterType.Id);
            DateTime? dateModified = GetModifiedDate(resourceId);
            string eTag = CalculateETag(resourceId, dateModified);

            statusCode = GetETagMatchedCode(context, AtomPubConstants.KeyIfMatch, eTag);

            if(statusCode == HttpStatusCode.Accepted)
            {
                if(context.Request.Headers.AllKeys.Contains(AtomPubConstants.KeyIfNoneMatch))
                {
                    statusCode = GetETagMatchedCode(context, AtomPubConstants.KeyIfNoneMatch, eTag);
                }
                else
                {
                    statusCode = GetDateMatchedCode(context, AtomPubConstants.KeyIfModifiedSince, dateModified);

                    if(statusCode == HttpStatusCode.Accepted)
                    {
                        statusCode = GetDateMatchedCode(context, AtomPubConstants.KeyIfUnmodifiedSince, dateModified);
                    }
                }
            }

            return HttpStatusCode.Accepted == statusCode;
        }

        /// <summary>
        /// Determines whether the atom entry is media type.
        /// </summary>
        /// <param name="mediaType">Type of the media.</param>
        /// <returns>
        /// 	<c>true</c> if the atom entry is of media type; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsAtomEntryMediaType(string mediaType)
        {
            string entryTypepattren = @"^\s*(application/atom\+xml;?)(;\s*type=entry;?)?(;\s*charset=""utf-8"";?)?\s*$";
            return Regex.IsMatch(mediaType, entryTypepattren, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Gets the member.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="memberResourceId">The member resource id.</param>
        /// <returns>The <see cref="Resource"/>.</returns>
        internal static Resource GetMember(ZentityContext context, string collectionName, string memberResourceId)
        {
            if(string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentNullException("collectionName");
            }

            if(string.IsNullOrEmpty(memberResourceId))
            {
                throw new ArgumentNullException("memberResourceId");
            }

            if(!AtomPubHelper.IsValidGuid(memberResourceId))
            {
                throw new ArgumentException(Resources.ATOMPUB_INVALID_RESOURCE_ID, "memberResourceId");
            }

            Type collectionType = CoreHelper.GetSystemResourceType(collectionName);
            if(collectionType == null)
            {
                throw new ResourceNotFoundException(Resources.ATOMPUB_RESOURCE_NOT_FOUND);
            }

            string commandText = string.Format(CultureInfo.InvariantCulture, AtomPubConstants.EsqlToGetResourceById,
                                               collectionType.FullName);

            ObjectQuery<ScholarlyWork> resourceQuery = new ObjectQuery<ScholarlyWork>(commandText, context);
            resourceQuery.Parameters.Add(new ObjectParameter("Id", new Guid(memberResourceId)));

            Resource resource = resourceQuery.FirstOrDefault();
            if(resource == null)
            {
                throw new ResourceNotFoundException(Resources.ATOMPUB_RESOURCE_NOT_FOUND);
            }

            return resource;
        }

        /// <summary>
        /// Gets the member.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="memberResourceId">The member resource id.</param>
        /// <param name="permissionName">Name of the permission.</param>
        /// <returns>A <see cref="Resource"/>.</returns>
        internal static Resource GetMember(
                                        ZentityContext context, 
                                        string collectionName, 
                                        string memberResourceId, 
                                        string permissionName)
        {
            Resource resource = GetMember(context, collectionName, memberResourceId);

            AuthenticatedToken authenticatedToken = CoreHelper.GetAuthenticationToken();

            if(!resource.Authorize(permissionName, context, authenticatedToken))
            {
                throw new UnauthorizedException(Resources.ATOMPUB_UNAUTHORIZED);
            }

            return resource;
        }

        /// <summary>
        /// Gets the file extention from the content disposition.
        /// </summary>
        /// <param name="contentDisposition">The content disposition.</param>
        /// <returns>The file extention from the content disposition.</returns>
        internal static string GetFileExtentionFromContentDisposition(string contentDisposition)
        {
            string fileExtention = string.Empty;
            string[] contents = contentDisposition.Split(';');
            foreach(string content in contents)
            {
                if(content.Trim().StartsWith("filename", StringComparison.OrdinalIgnoreCase))
                {
                    string[] fileName = content.Split('=');
                    fileExtention = Path.GetExtension(fileName[1]).Substring(1);
                }
            }
            return fileExtention;
        }

        #region Database Functions

        /// <summary>
        /// Executes the stored Procedure
        /// </summary>
        /// <param name="query">Database query</param>
        /// <param name="cmdParameters">List of parameters to the Command</param>
        /// <returns>
        ///     <c>true</c> if successfully executed; otherwise <c>false</c>.
        /// </returns>
        internal static bool ExecuteNonQuery(string query, List<SqlParameter> cmdParameters)
        {
            Database db = new SqlDatabase(AtomPubConstants.CoreDBConnectionString);

            DbCommand cmd = db.GetSqlStringCommand(query);
            foreach(DbParameter param in cmdParameters)
            {
                db.AddParameter(cmd, param.ParameterName, param.DbType, param.Direction, null, DataRowVersion.Current, param.Value);
            }
            using(DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    db.ExecuteNonQuery(cmd, transaction);
                }
                catch(Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Commit();
            }
            foreach(DbParameter param in cmdParameters)
            {
                if(ParameterDirection.Output == param.Direction ||
                    ParameterDirection.InputOutput == param.Direction ||
                    ParameterDirection.ReturnValue == param.Direction)
                {
                    param.Value = db.GetParameterValue(cmd, param.ParameterName);
                }
            }
            return true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the date matched code.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="headerName">Name of the header.</param>
        /// <param name="datemodified">The datemodified.</param>
        /// <returns>The http status code.</returns>
        private static HttpStatusCode GetDateMatchedCode(
                                                        HttpContext context, 
                                                        string headerName, 
                                                        DateTime? datemodified)
        {
            if(headerName != AtomPubConstants.KeyIfModifiedSince &&
               headerName != AtomPubConstants.KeyIfUnmodifiedSince)
            {
                return HttpStatusCode.Accepted;
            }

            if(!context.Request.Headers.AllKeys.Contains(headerName))
            {
                return HttpStatusCode.Accepted;
            }

            if(datemodified == null)
            {
                datemodified = DateTimeOffset.MinValue.Date;
            }

            string headerValue = context.Request.Headers[headerName];
            DateTime requestedDate;
            bool isValidDate = DateTime.TryParse(headerValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out requestedDate);

            // Please refere http://tools.ietf.org/html/rfc2616#section-14.25
            bool isResourceModified = null != datemodified && datemodified >= requestedDate;

            if(headerName == AtomPubConstants.KeyIfModifiedSince)
            {
                return (isResourceModified || !isValidDate || DateTime.Now <= requestedDate) ? HttpStatusCode.Accepted : HttpStatusCode.NotModified;
            }
            else
            {
                return (!isResourceModified || !isValidDate) ? HttpStatusCode.Accepted : HttpStatusCode.PreconditionFailed;
            }
        }

        /// <summary>
        /// Gets the E tag matched code.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="headerName">Name of the header.</param>
        /// <param name="eTag">The e tag.</param>
        /// <returns>The http status code.</returns>
        private static HttpStatusCode GetETagMatchedCode(HttpContext context, string headerName, string eTag)
        {
            if(headerName != AtomPubConstants.KeyIfMatch &&
               headerName != AtomPubConstants.KeyIfNoneMatch)
            {
                return HttpStatusCode.Accepted;
            }

            if(!context.Request.Headers.AllKeys.Contains(headerName))
            {
                return HttpStatusCode.Accepted;
            }

            bool isETagMatches = !string.IsNullOrEmpty(eTag);

            if(isETagMatches)
            {
                string headerValue = context.Request.Headers[headerName];
                string[] requestedETags = headerValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(header => header.Trim())
                                                     .ToArray();
                isETagMatches = requestedETags.Contains("*") ||
                                requestedETags.Contains(string.Format(CultureInfo.InvariantCulture, "\"{0}\"", eTag));
            }

            if(headerName == AtomPubConstants.KeyIfMatch)
            {
                return (isETagMatches) ? HttpStatusCode.Accepted : HttpStatusCode.PreconditionFailed;
            }
            else
            {
                return (!isETagMatches) ? HttpStatusCode.Accepted :
                    (context.Request.RequestType == PlatformConstants.GetRequestType) ?
                    HttpStatusCode.NotModified : HttpStatusCode.PreconditionFailed;
            }
        }

        /// <summary>
        /// Gets the template match from URI.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="requestUri">The request URI.</param>
        /// <returns>The <see cref="UriTemplateMatch"/>.</returns>
        private static UriTemplateMatch GetTemplateMatchFromUri(Uri baseUri, Uri requestUri)
        {
            UriTemplateMatch results = null;

            foreach(KeyValuePair<AtomPubRequestType, UriTemplate> pair in AtomPubHelper.AtomPubTemplates)
            {
                results = pair.Value.Match(baseUri, requestUri);
                if(null != results)
                {
                    break;
                }
            }

            return results;
        }

        /// <summary>
        /// Loads the URI templates.
        /// </summary>
        private static void LoadUriTemplates()
        {
            if(null == atomPubTemplates)
            {
                atomPubTemplates = new Dictionary<AtomPubRequestType, UriTemplate>();
            }

            atomPubTemplates.Add(AtomPubRequestType.ServiceDocument, AtomPubHelper.ServiceDocumentTemplate);
            atomPubTemplates.Add(AtomPubRequestType.CollectionWithPageNo, AtomPubHelper.CollectionUriWithPageNoTemplate);
            atomPubTemplates.Add(AtomPubRequestType.Collection, AtomPubHelper.CollectionUriTemplate);
            atomPubTemplates.Add(AtomPubRequestType.EditMember, AtomPubHelper.EditMemberUriTemplate);
            atomPubTemplates.Add(AtomPubRequestType.EditMedia, AtomPubHelper.EditMediaUriTemplate);
        }

        /// <summary>
        /// Catches the MimType for file extension from resource file.
        /// </summary>
        private static void LoadFileExtensionMimTypes()
        {
            mimeTypeMappings = new Dictionary<string, string>();
            XmlTextReader reader = null;
            try
            {
                reader = new XmlTextReader(Properties.Resources.MimeTypeMapping, XmlNodeType.Element, null);
                XElement mimeTypeElemtnt = XElement.Load(reader);

                var mappings = mimeTypeElemtnt.Descendants("Mapping")
                                .Select(map => new
                                {
                                    FileExtension = map.Attribute("FileExtension").Value,
                                    MimeType = map.Attribute("MimeType").Value
                                }).ToArray();
                foreach(var mapping in mappings)
                {
                    MimeTypeMappings.Add(mapping.FileExtension, mapping.MimeType);
                }
            }
            finally
            {
                if(null != reader)
                {
                    reader.Close();
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets the property URI.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The property Uri.</returns>
        internal static string GetPropertyUri(string propertyName)
        {
            return AtomPubConstants.ExtensionPropertyBaseUri + propertyName;
        }
    }
}
