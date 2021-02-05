<%@ Application Language="C#" %>

<script runat="server">

        void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            Zentity.Security.Authentication.IAuthenticationProvider zentityProvider = Zentity.Security.Authentication.AuthenticationProviderFactory.CreateAuthenticationProvider("ZentityAuthenticationProvider");
            Zentity.Security.Authentication.IAuthenticationProvider digestProvider = Zentity.Security.Authentication.AuthenticationProviderFactory.CreateAuthenticationProvider("HttpDigestAuthenticationProvider");
            HttpApplication app = (HttpApplication)sender;
            app.Application["ZentityAuthenticationProvider"] = zentityProvider;
            app.Application["HttpDigestAuthenticationProvider"] = digestProvider;
            Zentity.Services.HttpAuthentication.HttpAuthenticationProvider.OnAuthenticateRequest(sender, e);
        }

</script>

       
