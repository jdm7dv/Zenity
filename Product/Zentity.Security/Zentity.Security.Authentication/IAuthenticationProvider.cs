// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.Authentication
{
    using System.IdentityModel.Tokens;

    /// <summary>
    /// Allows implementing a class to act as a provider of authentication service 
    /// for use in zentity based applications/services.
    /// </summary>
    /// <remarks>
    /// Implement this interface to create different authentication providers. 
    /// For developing an authentication provider you need to decide on two strategies - 
    /// <list type="bullet">
    /// <item>How to authenticate a SecurityToken - Choose a subclass of System.IdentityModel.Tokens.SecurityToken
    /// This namespace provides token classes for most of the authentication types in existence today, like
    /// integrated windows authentication, active directory authentication, certificate authentication, 
    /// SAML authentication etc.</item>
    /// <item>How to validate an AuthenticatedToken returned by the Authenticate() method. You can choose to store 
    /// some properties of the SecurityToken passed by user to the Authenticate() method, and use these
    /// to check whether the identity is still valid and authenticated, and that it is not tampered with
    /// over the network or otherwise.</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// The following code provides a sample implementation of the IAuthenticationProvider interface, 
    /// and the AuthenticatedToken abstract class.
    /// It implements integrated windows security in the Authenticate() method.
    /// <code>
    /// using System;
    /// using System.Collections.Generic;
    /// using System.Linq;
    /// using System.Text;
    /// using Zentity.Security.Authentication;
    /// using System.IdentityModel.Tokens;
    /// using System.IdentityModel.Selectors;
    /// using System.Collections.ObjectModel;
    /// using System.IdentityModel.Policy;
    /// using System.Security.Principal;
    /// namespace AuthenticationProviderImplementations
    /// {
    /// // The integrated windows authentication provider implements the IAuthenticationProvider interface.
    /// // The Authenticate() method requires WindowsSecurityToken to be passed for authenticating using 
    /// // WindowsSecurityTokenAuthenticator
    /// public class IntegratedWindowsAuthenticationProvider : IAuthenticationProvider
    /// {
    ///    #region IAuthenticationProvider Members
    ///    //Provide implementation of Authenticate() method for integrated windows authentication.
    ///    public AuthenticatedToken Authenticate(SecurityToken credentialToken)
    ///    {
    ///        #region Input validation
    ///        if (credentialToken == null)
    ///        {
    ///            throw new ArgumentNullException(&quot;credentialToken&quot;);
    ///        }
    ///        if (credentialToken as WindowsSecurityToken == null)
    ///        {
    ///            throw new ArgumentException(&quot;Incorrect type of token passed. WindowsSecurityToken is expected.&quot;);
    ///        }
    ///        #endregion
    ///
    ///        WindowsSecurityToken windowsToken = credentialToken as WindowsSecurityToken;
    ///        WindowsSecurityTokenAuthenticator authenticator = new WindowsSecurityTokenAuthenticator();
    ///        ReadOnlyCollection&lt;IAuthorizationPolicy&gt; policyCollection = authenticator.ValidateToken(windowsToken);
    ///        bool isAuthenticated = policyCollection.Count > 0 ? true : false;
    ///        if (isAuthenticated)
    ///        {
    ///            IntegratedWindowsAuthenticationToken outputToken = new IntegratedWindowsAuthenticationToken(windowsToken.WindowsIdentity);
    ///            return outputToken;
    ///        }
    ///        return null;
    ///    }
    ///
    ///    //The IsReusable property indicates whether an instance of this provider can be reused for authenticating multiple
    ///    //WindowsSecurityTokens. If there is no token data stored in an instance this property should be set to true.
    ///    public bool IsReusable
    ///    {
    ///        get { return true; }
    ///    }
    ///
    ///    #endregion
    /// }
    ///
    /// //This class implements the AuthenticatedToken class. The following is just a sample implementation. 
    /// //There can be several different implementations for the same.
    /// //This class stores WindowsIdentity object and validates the token based on the identity's IsAuthenticated property.
    /// public class IntegratedWindowsAuthenticationToken : AuthenticatedToken
    /// {
    ///    WindowsIdentity _identity;
    ///    //Constructor - initializes Identity and IdentityName data members.
    ///    internal IntegratedWindowsAuthenticationToken(WindowsIdentity identity)
    ///    {
    ///        _identityName = identity.Name;
    ///        _identity = identity;
    ///    }
    ///
    ///    string _identityName;
    ///    //Gets the identity name.
    ///    public override string IdentityName
    ///    {
    ///        get { return _identityName; }
    ///    }
    ///    //Checks whether the windows identity object stored is authenticated.
    ///    public override bool Validate()
    ///    {
    ///        return _identity.IsAuthenticated;
    ///    }
    /// }
    /// }
    /// </code>
    /// </example>
    public interface IAuthenticationProvider
    {
        /// <summary>
        ///  Gets a value indicating whether another request can use
        ///  the IAuthenticationProvider instance.
        /// </summary>
        bool IsReusable
        {
            get;
        }

        /// <summary>
        /// Authenticates an identity based on the credentials provided via
        /// <see cref="SecurityToken"/>.
        /// </summary>
        /// <param name="credentialToken">
        /// Token containing the credentials of the identity to be authenticated.
        /// </param>
        /// <returns><see cref="AuthenticatedToken"/>Authenticated Token representing the user.</returns>
        /// <example>
        /// Pre-requisites for running this code sample
        /// <list type="bullet">
        /// <item>Build the sample integrated windows authentication provider given in the IAuthenticationProvider documentation.</item>
        /// <item>Refer to the sample application configuration file given below. Create a similar one for your application.</item>
        /// <item>Replace the assembly name (type, namespace names, if changed in the provider sample) 
        /// in Providers -&gt; add tag given in the configuration file below.</item>
        /// <item>Make sure Zentity.Security.Authentication.dll and the Authentication Provider DLL are present in
        /// the application's bin\debug or bin\release folder.</item>
        /// </list>
        /// <para>&lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;</para>
        ///    <para>&lt;configuration&gt;</para>
        ///      <para>&lt;configSections&gt;</para>
        ///        <para>&lt;!-- Add references to the assembly containing definition of config sections--&gt;</para>
        ///        <para>&lt;section name=&quot;Authentication&quot; type=&quot;Zentity.Security.Authentication.AuthenticationConfigurationSection, Zentity.Security.Authentication&quot; /&gt;</para>
        ///      <para>&lt;/configSections&gt;</para>
        ///      <para>&lt;!-- Authentication configuration section</para>
        ///          <para>Add one provider entry for each authentication implementation. </para>
        ///          <para>name = Choose a unique name for authentication provider. </para>
        ///          <para>type = Name of the type implementing IAuthenticationProvider, Fully qualified assembly name
        ///      --&gt;</para>
        ///      <para>&lt;Authentication&gt;</para>
        ///        <para>&lt;Providers&gt;</para>
        ///          <para>&lt;add name=&quot;IntegratedWindows&quot; type=&quot;AuthenticationProviderImplementations.IntegratedWindowsAuthenticationProvider, 
        ///               AuthenticationProviders&quot; /&gt;</para>
        ///        <para>&lt;/Providers&gt;</para>
        ///      <para>&lt;/Authentication&gt;</para>
        ///   <para>&lt;/configuration&gt;</para>
        ///  <code>
        /// public static AuthenticatedToken AuthenticateIntegratedWindowsUser()
        /// {
        ///    //Get an instance of authentication provider for integrated authentication.
        ///    IAuthenticationProvider provider = AuthenticationProviderFactory.CreateAuthenticationProvider(&quot;IntegratedWindows&quot;);
        ///    //This provider expects WindowsSecurityToken. So create one using the current windows identity.
        ///    WindowsIdentity current = WindowsIdentity.GetCurrent();
        ///    WindowsSecurityToken currentIdentityToken = new WindowsSecurityToken(current);
        ///    AuthenticatedToken outputToken = provider.Authenticate(currentIdentityToken);
        ///    return outputToken;
        /// }
        /// </code>
        /// </example>
        AuthenticatedToken Authenticate(SecurityToken credentialToken);
    }
}
