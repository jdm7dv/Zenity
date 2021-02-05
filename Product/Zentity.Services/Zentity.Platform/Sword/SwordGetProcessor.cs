// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System.ServiceModel.Syndication;
    using System.Web;

    /// <summary>
    /// This class is responsible for handling GET request sent to the Sword service.
    /// </summary>
    public class SwordGetProcessor : AtomPubGetProcessor
    {
        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SwordGetProcessor"/> class.
        /// </summary>
        public SwordGetProcessor()
            : base(SwordHelper.GetBaseUri())
        {
        }

        #endregion

        /// <summary>
        /// Processes the GET request sent to the sword service.
        /// </summary>
        /// <param name="context">Object of HttpContext containing the request information.</param>
        /// <param name="statusCode">Out parameter returns status of the request.</param>
        /// <returns>String containing the response to the specified request.</returns>
        public override string ProcessRequest(HttpContext context, out System.Net.HttpStatusCode statusCode)
        {
            string response = string.Empty;

            AtomPubRequestType requestType = AtomPubHelper.GetAtomPubRequestType(context, base.BaseUri);

            switch(requestType)
            {
                case AtomPubRequestType.ServiceDocument:
                    statusCode = System.Net.HttpStatusCode.OK;
                    response = this.GetServiceDocument(context);

                    // Set content type for Service Document - Bug Fix : 183173
                    context.Response.ContentType = AtomPubConstants.ServiceDocumentContentType;
                    break;
                case AtomPubRequestType.Collection:
                case AtomPubRequestType.CollectionWithPageNo:
                case AtomPubRequestType.EditMember:
                case AtomPubRequestType.EditMedia:
                    response = base.ProcessRequest(context, out statusCode);
                    break;
                case AtomPubRequestType.Unknwon:
                default:
                    statusCode = System.Net.HttpStatusCode.BadRequest;
                    response = Properties.Resources.ATOMPUB_BAD_REQUEST;
                    break;
            }

            return response;
        }

        /// <summary>
        /// Generates the service document for Sword service.
        /// </summary>
        /// <param name="context">Object of HttpContext containing the request information.</param>        
        /// <returns>String containing the service document.</returns>
        public string GetServiceDocument(HttpContext context)
        {
            ServiceDocument serviceDocument = base.GetServiceDocument(context, base.BaseUri);

            // Do some SWORD specific processing here.
            // Add <sword:level>0</sword:level> element.
            serviceDocument.ElementExtensions.Add(SwordConstants.SwordLevel,
                                                  SwordConstants.SwordNamespace,
                                                  SwordConstants.SwordLevelValue);

            return AtomPubGetProcessor.GetServiceDocument(serviceDocument);
        }
    }
}
