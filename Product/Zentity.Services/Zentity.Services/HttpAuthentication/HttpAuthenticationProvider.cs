// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;
using Zentity.Security.Authentication;
using Zentity.Services.Properties;
using System.IdentityModel.Tokens;
using System.Security.Principal;
using Zentity.Security.AuthorizationHelper;
using System.Globalization;

namespace Zentity.Services.HttpAuthentication
{
    /// <summary>
    /// This class authenticates any request to the service handler.
    /// </summary>
    public static class HttpAuthenticationProvider
    {
        #region AuthenticateRequest 
        /// <summary>
        /// This method processes the AuthenticateRequest.
        /// </summary>
        /// <param name="sender">The HttpApplication object</param>
        /// <param name="e">Event args</param>
        /// <remarks>This method reads the configuration setting for supported authentication type, and calls
        /// appropriate class to perform basic and / or digest authentication. If the service does not support
        /// both authentication types, it performs guest authentication.
        /// It stores the authenticated user's token in HttpContext's Items collection.</remarks>
        public static void OnAuthenticateRequest(object sender, EventArgs e)
        {
            //TODO handle proxy authentication.
            #region Algorithm for service authentication
            //Get the application object
            //Check the request header
            //If Authorization header is present
            //  If Basic Authentication is supported AND Authorization header contains 'Basic'
            //      Call Basic Authentication's Authenticate() method
            //      If user is authenticated
            //          Store AuthenticatedToken in Context.Items
            //          Get Authentication-Info header for Basic Authentication
            //          Add to the response headers
            //      Else
            //          Get WWW-Authenticate header for basic
            //          Send BadRequest or Unauthorized response, depending on the status given by Basic auth object
            //      End If
            //  Else if Digest Authentication is supported AND Authorization header contains 'Digest'
            //      Call Digest Authentication's Authenticate() method
            //      If user is authenticated
            //          Store AuthenticatedToken in Context.Items
            //          Get Authentication-Info header for Digest Authentication
            //          Add to the response headers
            //      Else
            //          Get WWW-Authenticate header for digest
            //          Send BadRequest or Unauthorized response, depending on status given by Digest auth object
            //      End If
            //Else
            //  If Basic is supported
            //      Get WWW-Authenticate header for Basic
            //  If Digest is supported
            //      Get WWW-Authenticate header for Digest
            //  End If
            //  Return 401 response with the headers
            //End If
            #endregion

            //Get the authentication provider instance from application cache
            HttpApplication application = (HttpApplication)sender;
            try
            {
                if (application.Context.Items["AuthenticatedToken"] == null)
                {
                    IAuthenticationProvider basicAuthenticationProvider =
                        application.Context.Application["ZentityAuthenticationProvider"]
                        as IAuthenticationProvider;
                    IAuthenticationProvider digestAuthenticationProvider =
                        application.Context.Application["HttpDigestAuthenticationProvider"]
                        as IAuthenticationProvider;

                    //Get the authorization header
                    string authorizationHeader = application.Context.Request.Headers["Authorization"];

                    //Read the authentication configuration settings for the service.
                    bool isBasicSupported;
                    bool isDigestSupported;
                    ReadHttpAuthenticationConfiguration(out isBasicSupported, out isDigestSupported);

                    //Attempt basic authentication
                    if (isBasicSupported)
                    {
                        bool isAuthenticated = ProcessBasicAuthentication(application.Context,
                            authorizationHeader, basicAuthenticationProvider);
                        if (!isAuthenticated && isDigestSupported)
                        {
                            isAuthenticated = ProcessDigestAuthentication(application.Context,
                                authorizationHeader, digestAuthenticationProvider);
                        }
                    }
                    else if (isDigestSupported)
                    {
                        ProcessDigestAuthentication(application.Context,
                            authorizationHeader, digestAuthenticationProvider);
                    }
                    else
                    {
                        ProcessGuestAuthentication(basicAuthenticationProvider, application.Context);
                    }
                }
                //If request is authenticated set the Context.User property
                //This will set the Request.IsAuthenticated property to true value.
                if (application.Context.Items["AuthenticatedToken"] != null)
                {
                    application.Context.User = new GenericPrincipal(
                        new GenericIdentity(((AuthenticatedToken)application.Context.Items["AuthenticatedToken"]).IdentityName),
                        new string[0] { });
                }
            }
            catch (AuthenticationException ex)
            {
                application.Context.Response.StatusCode = 500;
                application.Context.Response.StatusDescription = ex.Message;
                application.CompleteRequest();
            }
            catch (ConfigurationErrorsException ex)
            {
                application.Context.Response.StatusCode = 500;
                application.Context.Response.StatusDescription = ex.Message;
                application.CompleteRequest();
            }
            catch (NullReferenceException ex)
            {
                application.Context.Response.StatusCode = 500;
                application.Context.Response.StatusDescription = ex.Message;
                application.CompleteRequest();
            }
        }
        #endregion

        #region Private methods
        
        /// <summary>
        /// Authenticates a user with guest credentials
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="context">The context.</param>
        private static void ProcessGuestAuthentication(IAuthenticationProvider provider, HttpContext context)
        {
            if (provider == null)
            {
                return;
            }
            AuthenticatedToken token = AuthenticateGuest(provider);
            if (token != null)
            {
                context.Items.Add("AuthenticatedToken", token);
            }
            else
            {
                context.Response.StatusCode = 500;
            }
        }

        /// <summary>
        /// Process basic authentication consists of verifying the user credentials sent in the authorization header, 
        /// and setting the response properties - status code, headers. 
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="authorizationHeader">The authorization header.</param>
        /// <param name="provider">The provider.</param>
        /// <returns>System.Boolean value; <c>true</c> if the processing was successful, <c>false</c> otherwise.</returns>
        private static bool ProcessBasicAuthentication(HttpContext context, 
            string authorizationHeader, 
            IAuthenticationProvider provider)
        {
            //Create an instance of BasicAuthentication.
            //Call Authenticate() on the instance to validate the user credentials.
            //If request is unauthorized, get the WWW-Authenticate header and add to response headers.
            //If request is authorized, add the AuthenticatedToken to HttpContext.Items.
            
            int statusCode;
            BasicAuthentication authenticator = new BasicAuthentication(provider);           
            
            if (IsGuest(BasicAuthentication.GetUserName(authorizationHeader), context))
            {
                context.Response.StatusCode = 200;
                return true;
            }

            AuthenticatedToken token = authenticator.Authenticate(authorizationHeader, out statusCode);
            
            context.Response.StatusCode = statusCode;
            //If the user credentials are invalid or empty, this method adds WWW-Authenticate header to the response
            if (statusCode == 401)
            {
                string wwwAuthHeader = BasicAuthentication.GetWwwAuthenticateHeader();
                context.Response.AppendHeader("WWW-Authenticate", wwwAuthHeader);
                return false;
            }
            else if (statusCode == 400)
            {
                //For bad request we do not send www-authenticate header.
                return false;
            }
            else
            {
                context.Items.Add("AuthenticatedToken", token);

                return true;
            }
        }

        /// <summary>
        /// Process digest authentication consists of verifying the user credentials sent in the 
        /// authorization header, and setting the response properties - status code, headers. 
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="authorizationHeader">The authorization header.</param>
        /// <param name="provider">The provider.</param>
        /// <returns>System.Boolean value; <c>true</c> if the processing was successful, <c>false</c> otherwise.</returns>
        private static bool ProcessDigestAuthentication(
                                                    HttpContext context,
                                                    string authorizationHeader,
                                                    IAuthenticationProvider provider)
        {
            //Create an instance of DigestAuthentication.
            //Call Authenticate() on the instance to validate the user credentials.
            //If request is unauthorized, get the WWW-Authenticate header and add to response headers.
            //If request is authorized, add the AuthenticatedToken to HttpContext.Items.
            //If request if authorized add 'Authentication-Info' header to response headers.

            int statusCode;
            DigestAuthentication authenticator = new DigestAuthentication(provider);

            if (IsGuest(authenticator.GetUserName(authorizationHeader), context))
            {
                context.Response.StatusCode = 200;
                return true;
            }

            AuthenticatedToken token = authenticator.Authenticate(authorizationHeader, 
                context.Request.HttpMethod, 
                out statusCode);
            context.Response.StatusCode = statusCode;
            //If the user credentials are invalid or empty, this method adds WWW-Authenticate header to the response
            if (statusCode == 401)
            {
                string wwwAuthHeader = authenticator.GetWwwAuthenticateHeader();
                context.Response.AppendHeader("WWW-Authenticate", wwwAuthHeader);
                return false;
            }
            else if (statusCode == 200) //OK
            {
                context.Items.Add("AuthenticatedToken", token);
                string authInfoHeader = authenticator.GetAuthenticationInfoHeader();
                context.Response.AppendHeader("Authentication-Info", authInfoHeader);
                return true;
            }
            else if (statusCode == 400) //Bad request
            {
                return false;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether the specified user name is a guest.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// 	<c>true</c> if the specified user name is a guest; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsGuest(string userName, HttpContext context)
        {
            string guestUserName = UserManager.GuestUserName;

            if (!String.IsNullOrEmpty(guestUserName)
                && !String.IsNullOrEmpty(userName)
                && userName.ToUpperInvariant().Equals(guestUserName.ToUpperInvariant()))
            {
                IAuthenticationProvider basicAuthenticationProvider =
                    context.Application["ZentityAuthenticationProvider"]
                    as IAuthenticationProvider;

                ProcessGuestAuthentication(basicAuthenticationProvider, context);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Reads configuration settings related to authentication support.
        /// </summary>
        /// <param name="isBasicSupported">if set to <c>true</c> supports basic authentication.</param>
        /// <param name="isDigestSupported">if set to <c>true</c> supports diget authentication.</param>
        private static void ReadHttpAuthenticationConfiguration(out bool isBasicSupported, out bool isDigestSupported)
        {
            //Read from the service configuration file whether the basic / digest authentication is supported.
            //At least one of the settings must be present in the configuration file.
            string basicSupported = ConfigurationManager.AppSettings["IsBasicSupported"];
            string digestSupported = ConfigurationManager.AppSettings["IsDigestSupported"];
            if (!string.IsNullOrEmpty(basicSupported))
            {
                isBasicSupported = Convert.ToBoolean(basicSupported, CultureInfo.InvariantCulture);
            }
            else
            {
                isBasicSupported = false;
            }
            if (!string.IsNullOrEmpty(digestSupported))
            {
                isDigestSupported = Convert.ToBoolean(digestSupported, CultureInfo.InvariantCulture);
            }
            else
            {
                isDigestSupported = false;
            }
        }
        
        /// <summary>
        /// Authenticates using the guest credentials.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns>The authentication token.</returns>
        private static AuthenticatedToken AuthenticateGuest(IAuthenticationProvider provider)
        {
            if (provider == null)
            {
                return null;
            }
            //Read guest credentials from config settings.
            string guestUserName = UserManager.GuestUserName;
            string guestPassword = Constants.GuestPassword;

            if (!string.IsNullOrEmpty(guestUserName) && !string.IsNullOrEmpty(guestPassword))
            {
                //Authenticate guest
                UserNameSecurityToken guestCredentials = new UserNameSecurityToken(guestUserName, guestPassword);
                AuthenticatedToken guestToken = provider.Authenticate(guestCredentials);
                return guestToken;
            }
            return null;
        }

        #endregion
    }
}
