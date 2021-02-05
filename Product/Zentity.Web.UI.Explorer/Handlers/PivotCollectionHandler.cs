// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Web.UI.Explorer.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Xml.Serialization;
    using Zentity.Pivot;
    using Zentity.Services.Web;

    /// <summary>
    /// HttpHandler to find if a collection exists with the required CollectionUri and ResourceId
    /// </summary>
    public class PivotCollectionHandler : IHttpHandler
    {
        #region Implementation of IHttpHandler
        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.
        /// </returns>
        public bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests. </param>
        public void ProcessRequest(HttpContext context)
        {
            //// Zentity.ScholarlyWorks.Person/Person.cxml#Id=EQ.43ce0b33-7a89-42a0-a50e-39934dc6b13b

            if (ValidateUrl(context))
            {
                RedirectToCollection(context);
            }
            else
            {
                SendErrorResponse(context, HttpStatusCode.NotFound);
            }
        }
        #endregion

        /// <summary>
        /// Redirects to collection.
        /// </summary>
        /// <param name="context">The context.</param>
        private static void RedirectToCollection(HttpContext context)
        {
            // Fetch the file and folder path from the request uri
            string filePath = context.Server.MapPath(context.Request.Url.LocalPath);
            string folderPath = Path.GetDirectoryName(filePath);

            if (string.IsNullOrWhiteSpace(context.Request.QueryString["Id"]))
            {
                if (File.Exists(filePath))
                {
                    SendFileContent(context, filePath);
                }
                else
                {
                    RedirectToFirstCollection(context, folderPath);
                }
            }
            else
            {
                string resourceId = context.Request.QueryString["Id"].Trim().ToLowerInvariant().Replace("eq.", string.Empty);
                Guid resourceIdGuid;

                if (!Guid.TryParse(resourceId, out resourceIdGuid))
                {
                    RedirectToFirstCollection(context, folderPath);
                }
                else
                {
                    RedirectToCollectionWithResource(context, folderPath, resourceIdGuid);
                }
            }
        }

        /// <summary>
        /// Redirects to collection with resource.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="resourceIdGuid">The resource id GUID.</param>
        private static void RedirectToCollectionWithResource(HttpContext context, string folderPath, Guid resourceIdGuid)
        {
            // Check if there are any collection files in the folder
            DirectoryInfo collectionFolder = new DirectoryInfo(folderPath);
            FileInfo[] collectionFileList = collectionFolder.GetFiles("*.cxml");
            string redirectUrl = string.Empty;

            try
            {
                foreach (var collectionFile in collectionFileList)
                {
                    Collection pivotCollection = DeserializeCollection(collectionFile);
                    if (pivotCollection != null)
                    {
                        if (pivotCollection.Items != null && pivotCollection.Items.Length > 0)
                        {
                            IEnumerable<Item> collectionItems = pivotCollection.Items[0].Item;
                            if (collectionItems != null && collectionItems.Count() > 0)
                            {
                                var resourceItem = (from collectionItem in collectionItems
                                                    where collectionItem != null &&
                                                          !string.IsNullOrWhiteSpace(collectionItem.Id) &&
                                                          collectionItem.Id.Equals(resourceIdGuid.ToString(), StringComparison.OrdinalIgnoreCase)
                                                    select collectionItem).FirstOrDefault();

                                if (resourceItem != default(Item))
                                {
                                    redirectUrl = string.Format("{0}{1}#Id=EQ.{2}", collectionFile.Name, GetQueryString(context.Request), resourceIdGuid);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.InnerException != null ? ex.InnerException.ToString() : ex.ToString(), ex.Message);
                SendErrorResponse(context, HttpStatusCode.InternalServerError);
            }

            if (string.IsNullOrWhiteSpace(redirectUrl))
            {
                // If the resource is not found then serve the first collection file.
                RedirectToFirstCollection(context, folderPath);
            }
            else
            {
                context.Response.Redirect(redirectUrl);
            }
        }

        /// <summary>
        /// Redirects to first collection.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="folderPath">The folder path.</param>
        private static void RedirectToFirstCollection(HttpContext context, string folderPath)
        {
            // Check if there are any collection files in the folder
            DirectoryInfo collectionFolder = new DirectoryInfo(folderPath);
            FileInfo[] collectionFileList = collectionFolder.GetFiles("*.cxml");

            if (collectionFileList.Length > 0)
            {
                SendFileContent(context, collectionFileList[0].FullName);
            }
            else
            {
                SendErrorResponse(context, HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Deserializes the collection.
        /// </summary>
        /// <param name="collectionFile">The collection file.</param>
        /// <returns>Collection object created using the collection file</returns>
        private static Collection DeserializeCollection(FileInfo collectionFile)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Collection));
                using (StreamReader fileReader = new StreamReader(collectionFile.FullName))
                {
                    return (Collection)xmlSerializer.Deserialize(fileReader);
                }
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.InnerException != null ? ex.InnerException.ToString() : ex.ToString(), ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Sends the content of the file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="filePath">The file path.</param>
        private static void SendFileContent(HttpContext context, string filePath)
        {
            try
            {
                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.WriteFile(filePath);
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.InnerException != null ? ex.InnerException.ToString() : ex.ToString(), ex.Message);
                SendErrorResponse(context, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Sends the error response.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="statusCode">The status code.</param>
        private static void SendErrorResponse(HttpContext context, HttpStatusCode statusCode)
        {
            context.Response.Clear();
            context.Response.StatusCode = (int)statusCode;
            context.Response.End();
        }

        /// <summary>
        /// Generates the string from the request's querystring collection.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>string representation of querystring</returns>
        private static string GetQueryString(HttpRequest request)
        {
            var allQueryStringKeys = request.QueryString.AllKeys.Where(key => !key.Equals("Id", StringComparison.OrdinalIgnoreCase));
            if (allQueryStringKeys == null || allQueryStringKeys.Count() == 0)
            {
                return string.Empty;
            }

            IEnumerable<string> queryStringPairs = from queryStringKey in allQueryStringKeys
                                                   let queryStringValue = HttpUtility.UrlEncode(request.QueryString[queryStringKey])
                                                   select string.Format("{0}={1}", queryStringKey, queryStringValue);

            if (queryStringPairs.Count() > 0)
            {
                return "?" + string.Join<string>("&", queryStringPairs);
            }

            return string.Empty;
        }

        /// <summary>
        /// Validates the URL.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>System.Boolean value; true if the Url is correct, false otherwise.</returns>
        private static bool ValidateUrl(HttpContext context)
        {
            // Fetch the file and folder path from the request uri
            string filePath = context.Server.MapPath(context.Request.Url.LocalPath);
            string folderPath = Path.GetDirectoryName(filePath);

            try
            {
                // Check if the local folder exists or not
                if (Directory.Exists(folderPath))
                {
                    // Check if there are any collection files in the folder
                    DirectoryInfo collectionFolder = new DirectoryInfo(folderPath);
                    FileInfo[] collectionFileList = collectionFolder.GetFiles("*.cxml");

                    if (collectionFileList.Length > 0)
                    {
                        // If collection files exist then we can go ahead with deserialization
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.InnerException != null ? ex.InnerException.ToString() : ex.ToString(), ex.Message);
            }

            return false;
        }
    }
}