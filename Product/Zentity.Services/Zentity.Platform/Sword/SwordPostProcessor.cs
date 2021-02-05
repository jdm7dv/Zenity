// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.ServiceModel.Syndication;
    using System.Web;

    /// <summary>
    /// The class responsible for handling Sword POST request.
    /// </summary>
    public class SwordPostProcessor : AtomPubPostProcessor
    {
        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SwordPostProcessor"/> class.
        /// </summary>
        internal SwordPostProcessor()
            : base(SwordHelper.GetBaseUri())
        {
        }

        #endregion

        /// <summary>
        /// Handles the POST request send to the collection Uri. The method assumes that the request is 
        /// already validated using ValidateRequest method.
        /// </summary>
        /// <param name="context">HttpContext containing the request.</param>
        /// <param name="statusCode">Contains the status of the request.</param>
        /// <returns>A string containing the response for the specified AtomPub request.</returns>
        /// <remarks>If a zip file is sent with the POST request to the collection uri, 
        /// the contents of the zip file are extracted and the individual files will be uploaded 
        /// in the repository.
        /// The zip file should contain the mets.xml containing the metadata of the individual files.
        /// If mets.xml is not found in the specified zip file, status code 'PreconditionFailed' will 
        /// be returned.
        /// </remarks>
        public override string ProcessRequest(HttpContext context, out System.Net.HttpStatusCode statusCode)
        {
            string response = string.Empty;

            AtomEntryDocument atomEntryDocument = null;
            bool isAtomEntryType = AtomPubHelper.IsAtomEntryMediaType(context.Request.ContentType);

            try
            {
                if (isAtomEntryType)
                {
                    response = base.ProcessRequest(context, out statusCode);

                    System.Xml.XmlTextReader responseReader = null;
                    try
                    {
                        responseReader = new System.Xml.XmlTextReader(response, System.Xml.XmlNodeType.Document, null);
                        SyndicationItem responseItem = SyndicationItem.Load(responseReader);

                        string collectionName = AtomPubHelper.GetValueOfParameterFromUri(context,
                                                    base.BaseUri,
                                                    AtomPubParameterType.CollectionName);
                        // Add <sword:treatment>Successfully created a {Collection Name}</sword:treatment> element.
                        SwordPostProcessor.AddTreatmentElement(collectionName, responseItem);

                        // Create atom entry document from syndication item.
                        atomEntryDocument = new AtomEntryDocument(responseItem);
                    }
                    finally
                    {
                        responseReader.Close();
                    }

                    context.Response.ContentType = AtomPubConstants.AtomEntryContentType;
                }
                else
                {
                    // If request stream contains something other than AtomEntryDocument, 
                    // then client wants to create new member with Media.
                    atomEntryDocument = this.CreateMedia(context);

                    // Done this separately because if AtomEntry is null due to some reasons,
                    // then also it will set the content type.
                    context.Response.ContentType = AtomPubConstants.AtomEntryContentType;
                }

                // return the atom entry response.
                if (null != atomEntryDocument)
                {
                    response = atomEntryDocument.AtomEntry;
                    statusCode = System.Net.HttpStatusCode.Created;
                }
                else
                {
                    statusCode = System.Net.HttpStatusCode.InternalServerError;
                }
            }
            catch (MetsException ex)
            {
                statusCode = System.Net.HttpStatusCode.InternalServerError;
                response = ex.Message;
            }
            catch (SwordException ex)
            {
                statusCode = System.Net.HttpStatusCode.UnsupportedMediaType;
                response = ex.Message;
            }
            finally
            {
                string zipExtractedPath = context.Items[SwordConstants.ZipExtractedPath] as string;

                if (!string.IsNullOrEmpty(zipExtractedPath) && Directory.Exists(zipExtractedPath))
                {
                    Directory.Delete(zipExtractedPath, true);
                    context.Items[SwordConstants.ZipExtractedPath] = null;
                }
            }

            return response;
        }

        #region Private Methods

        /// <summary>
        /// Creates a media resource using the information present in the HttpContext.
        /// </summary>
        /// <param name="context">The HttpContext for the incoming HTTP request.</param>
        /// <returns>An AtomEntryDocument corresponding to the newly created media resource.</returns>
        private AtomEntryDocument CreateMedia(HttpContext context)
        {
            SyndicationItem item = null;

            string collectionName = AtomPubHelper.GetValueOfParameterFromUri(context,
                                                    base.BaseUri,
                                                    AtomPubParameterType.CollectionName);

            // Get byte array to update media.
            BinaryReader reader = new BinaryReader(context.Request.InputStream);
            byte[] media = new byte[context.Request.InputStream.Length];
            reader.Read(media, 0, media.Length);

            string fileExtention = string.Empty;
            if (context.Request.Headers.AllKeys.Contains("Content-Disposition"))
            {
                fileExtention = AtomPubHelper.GetFileExtentionFromContentDisposition(
                    context.Request.Headers["Content-Disposition"]);
            }

            IAtomPubStoreWriter swordStoreWriter = AtomPubStoreFactory.GetSwordStoreWriter();
            item = swordStoreWriter.CreateMedia(collectionName, context.Request.ContentType, media, fileExtention);

            AtomEntryDocument atomEntryDocument = null;
            if (null != item)
            {
                // Add <sword:treatment>Successfully created a {Collection Name}</sword:treatment> element.
                SwordPostProcessor.AddTreatmentElement(collectionName, item);

                // Create atom entry document from syndication item.
                atomEntryDocument = new AtomEntryDocument(item);
            }

            return atomEntryDocument;
        }

        /// <summary>
        /// Adds &lt;sword:treatment&gt;Successfully created a {Collection Name}&lt;/sword:treatment&gt; element.
        /// </summary>
        /// <param name="collectionName">Name of the collection</param>
        /// <param name="item">Syndication item</param>
        private static void AddTreatmentElement(string collectionName, SyndicationItem item)
        {
            string swordTreatmentValue = string.Format(CultureInfo.InvariantCulture, SwordConstants.SwordTreatmentValue, collectionName);

            // Add <sword:treatment>Successfully created a {Collection Name}</sword:treatment> element.
            item.ElementExtensions.Add(SwordConstants.SwordTreatment,
                                       SwordConstants.SwordNamespace,
                                       swordTreatmentValue);

        }

        #endregion
    }
}
