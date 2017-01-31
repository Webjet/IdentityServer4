using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using WebFormsOpenIdConnectAzureAD.Models;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security.Notifications;
using WebFormsOpenIdConnectAzureAD.AAD;

namespace WebFormsOpenIdConnectAzureAD
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string _clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"]; //aka AppKey
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
 
        private static string _authority = aadInstance + tenantId;
        // This is the resource ID of the AAD Graph API.  We'll need this to request a token to call the Graph API.
        private static string graphResourceId = "https://graph.windows.net";
         
        public void ConfigureAuth(IAppBuilder app)
        {
           string str= HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path);

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            //https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            //app.UseCookieAuthentication(options =>
            //{
            //    options.Events = new CookieAuthenticationEvents
            //    {
            //        // Set other options
            //        OnValidatePrincipal = LastChangedValidator.ValidateAsync
            //    }
            //});
            LoggerCallbackHandler.Callback = new AdalLoggerCallback(); //https://www.schaeflein.net/adal-v3-diagnostic-logging/

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    Authority = _authority,
                    ResponseType = OpenIdConnectResponseTypes.CodeIdToken,
                  
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        //  //ADAL V2 doesn't use Async
                        // If there is a code in the OpenID Connect response, redeem it for an access token and refresh token, and store those away.
                        //AuthorizationCodeReceived = (context) =>
                        // {
                        //     var code = context.Code;
                        //     ClientCredential credential = new ClientCredential(clientId, _clientSecret);
                        //     string signedInUserID = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                        //     var authContext = NewAuthenticationContext(signedInUserID, HttpContext.Current);

                        //     AuthenticationResult result = authContext.AcquireTokenByAuthorizationCode(
                        //     code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, graphResourceId);
                        //    Debug.Assert(authContext.TokenCache.Count>0);
                        //     return Task.FromResult(0);
                        // },
                        AuthorizationCodeReceived = OnAuthorizationCodeReceived,//ADAL V3 Async  
                        RedirectToIdentityProvider = (context) =>
                        {
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

                }
                );
            

            // This makes any middleware defined above this line run before the Authorization rule is applied in web.config
            app.UseStageMarker(PipelineStage.Authenticate);
        }
        /// <summary>
        /// If there is a code in the OpenID Connect response, redeem it for an access token and refresh token, and store those away.
        /// ADAL V3 Async from  https://github.com/Azure-Samples/active-directory-dotnet-webapp-webapi-openidconnect/blob/master/TodoListWebApp/App_Start/Startup.Auth.cs
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification context)
        {
            var code = context.Code;

            ClientCredential credential = new ClientCredential(clientId, _clientSecret);//aka appKey;
            string userObjectId = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            AuthenticationContext authContext = NewAuthenticationContext(userObjectId, HttpContext.Current);

            // If you create the redirectUri this way, it will contain a trailing slash.  
            // Make sure you've registered the same exact Uri in the Azure Portal (including the slash).
            Uri uri = new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path));

            AuthenticationResult result = await authContext.AcquireTokenByAuthorizationCodeAsync(code, uri, credential, graphResourceId);
        }
        /// <summary>
        /// See other alternatives in http://www.cloudidentity.com/blog/2014/07/09/the-new-token-cache-in-adal-v2/
        /// FileCache, EFADALTokenCache, NaiveSessionCache
        /// </summary>
        /// <param name="signedInUserId"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static AuthenticationContext NewAuthenticationContext(string signedInUserId, HttpContext httpContext)
        {
            AuthenticationContext authContext = null;
            if (httpContext != null)
            {
                authContext = new AuthenticationContext(_authority, new NaiveSessionCache(httpContext, signedInUserId));
            }
            else
            {
                authContext = new AuthenticationContext(_authority, new ADALTokenCache(signedInUserId));
            }
            return authContext;
        }
    }
}
