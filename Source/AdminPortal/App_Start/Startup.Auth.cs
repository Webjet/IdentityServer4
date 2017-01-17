using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IdentityModel.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Owin;
using AdminPortal.Models;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security.Notifications;

namespace AdminPortal
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        //  use RedirectToIdentityProvider instead of private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];

        public static readonly string Authority = aadInstance + tenantId;

        // This is the resource ID of the AAD Graph API.  We'll need this to request a token to call the Graph API.
        string graphResourceId = "https://graph.windows.net";

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());
    //        System.Diagnostics.Debugger.Break();
            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    Authority = Authority,
                    //  use RedirectToIdentityProvider instead of PostLogoutRedirectUri = postLogoutRedirectUri,

                    //Required for AAD Role Based Authentication
                    TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
                    {
                        //ValidateIssuer = false, // For Multi-Tenant Only
                        RoleClaimType = "roles",
                    },

                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        AuthorizationCodeReceived = OnAuthorizationCodeReceived,
                        AuthenticationFailed = OnAuthenticationFailed,
                        RedirectToIdentityProvider = (context) =>
                        {
             //               System.Diagnostics.Debugger.Break();
                            //from https://github.com/Azure-Samples/active-directory-dotnet-webapp-multitenant-openidconnect/blob/master/TodoListWebApp/App_Start/Startup.Auth.cs
                            // This ensures that the address used for sign in and sign out is picked up dynamically from the request
                            // this allows you to deploy your app (to Azure Web Sites, for example)without having to change settings
                            // Remember that the base URL of the address used here must be provisioned in Azure AD beforehand.
                            string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
                            context.ProtocolMessage.RedirectUri = appBaseUrl + "/"; //tail slash (from https://www.microsoftpressstore.com/articles/article.aspx?p=2473126&seqNum=2)
                            context.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl;
                            return Task.FromResult(0);
                        },

                    }
                });
        }

        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context)
        {
   //         System.Diagnostics.Debugger.Break();
            context.HandleResponse();
            context.Response.Redirect("/Error/ShowError?message=" + context.Exception.Message); 
            return Task.FromResult(0);
        }

        // If there is a code in the OpenID Connect response, redeem it for an access token and refresh token, and store those away.
        private Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification context)
        {
  //          System.Diagnostics.Debugger.Break();
            var code = context.Code;
            ClientCredential credential = new ClientCredential(clientId, appKey);
            string signedInUserID = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;
            AuthenticationContext authContext = new AuthenticationContext(Authority, false); //new ADALTokenCache(signedInUserID));
            AuthenticationResult result = authContext.AcquireTokenByAuthorizationCode(
                code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, graphResourceId);

            return Task.FromResult(0);
        }

    }
}