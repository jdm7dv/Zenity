// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Net;
using System.Web;
using Zentity.Platform;

namespace Zentity.Services
{
    /// <summary>
    /// This class is responsible for handling Http Requests.
    /// </summary>
    public class ServiceHandler : IHttpHandler
    {
        #region .ctor

        /// <summary>
        /// Initializes a new instance of the Zentity.Services.ServiceHandler class.
        /// </summary>
        public ServiceHandler()
        {
        }

        #endregion

        #region IHttpHandler Members

        /// <summary>
        /// Gets a value indicating whether the context is reusable.
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Handles the Http requests.
        /// </summary>
        /// <param name="context">An instance of HttpContext containing request details.</param>
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                IZentityService service = ServiceFactory.GetServiceInstance(context);

                if(null == service)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.Write(Properties.Resources.COULD_NOT_HANDLE_REQUEST);
                    return;
                }

                string error;

                if(service.ValidateRequest(context, out error))
                {
                    HttpStatusCode statusCode;
                    string responseString = service.ProcessRequest(context,
                                                                   out statusCode);
                    context.Response.StatusCode = (int)statusCode;
                    context.Response.BufferOutput = true;
                    context.Response.Write(responseString);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.Write(error);
                }

                if(context.Items.Contains("AuthenticatedToken"))
                {
                    context.Items.Remove("AuthenticatedToken");
                }
            }
            //We don't want to throw any error to user. instead we throw 500 (Internal server error)
            catch(Exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Write(Properties.Resources.INTERNAL_SERVER_ERROR);
            }
        }

        #endregion
    }
}
