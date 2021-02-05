// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System;
using System.Web;

namespace Zentity.Services.Data
{
    /// <summary>
    /// Authorizes requests to Zentity Data Service
    /// </summary>
    public class ZentityDataServiceAuthorizationModule : IHttpModule
    {
        #region IHttpModule Members

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
            // Nothing to dispose
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application</param>
        public void Init(HttpApplication context)
        {
            context.AuthorizeRequest += Context_AuthorizeRequest;
        }

        #endregion

        /// <summary>
        /// Handles the AuthorizeRequest event of the Context control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        static void Context_AuthorizeRequest(object sender, EventArgs e)
        {
            HttpApplication currentApp = sender as HttpApplication;

            if (currentApp.Context.User != null)
            {
                if (!currentApp.Context.User.IsInRole("ZentityAdministrators") &&
                    !currentApp.Context.User.IsInRole("ZentityUsers"))
                {
                    currentApp.Context.Response.StatusCode = (int) System.Net.HttpStatusCode.Forbidden;
                    currentApp.Context.Response.StatusDescription = string.Format(Properties.Messages.AuthorizationFailedForUser, currentApp.Context.User.Identity.Name);
                    currentApp.Context.Response.End();
                }
            }
            else
            {
                currentApp.Context.Response.StatusCode = (int) System.Net.HttpStatusCode.Unauthorized;
                currentApp.Context.Response.StatusDescription = Properties.Messages.UnauthorizedUser;
                currentApp.Context.Response.End();
            }
        }
    }
}