// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
#define TRACE

using System;
using System.Configuration;
using Zentity.Web.UI;
using Zentity.Security.Authentication;
using System.Diagnostics;
using Zentity.Security.AuthorizationHelper;
using Zentity.Core;

namespace Zentity.Web.UI
{
    public class Global : System.Web.HttpApplication
    {
        private static AuthenticatedToken guestSecurityToken = null;
        private const string _ftsPropertyName = "IsFullTextSearchEnabled";
        private const string _ftsStatus = "FtsStatus";

        void Application_Start(object sender, EventArgs e)
        {
            //Fired when the first instance of the HttpApplication class is created.
            //It allows you to create objects that are accessible by all
            //HttpApplication instances.

            string guestUserName = UserManager.GuestUserName;
            string guestPassword = Resources.Resources.GuestPassword;
            using (ResourceDataAccess resourceDataAccess = new ResourceDataAccess())
            {
                guestSecurityToken = resourceDataAccess.Authenticate(guestUserName, guestPassword);
            }

        }

        void Application_End(object sender, EventArgs e)
        {
            //The last event fired for an application request.
        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
            Exception lastException = Server.GetLastError();
            if (lastException != null)
            {
                System.Text.StringBuilder errorMsg = new System.Text.StringBuilder();
                errorMsg.AppendLine(lastException.Message);
                errorMsg.AppendLine(lastException.StackTrace);
                Trace.TraceError(errorMsg.ToString());
            }
        }

        void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started
            if (guestSecurityToken == null)
                throw new UnauthorizedAccessException(
                    string.Format(Resources.Resources.UserUnauthorizedAccessException,
                    UserManager.GuestUserName));
            Session[Constants.AuthenticationTokenKey] = guestSecurityToken;
            SetFtsStatus();
        }

        void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends.
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer
            // or SQLServer, the event is not raised.
        }

        //Queries the FTS status on the Zentity database and sets session variable.
        private void SetFtsStatus()
        {
            using (ZentityContext context = Utility.CreateContext())
            {
                bool ftsStatus;
                if (bool.TryParse(context.GetConfiguration(_ftsPropertyName), out ftsStatus))
                {
                    Session[_ftsStatus] = ftsStatus;
                }
                else
                {
                    Session[_ftsStatus] = false;
                }
            }
        }
    }
}