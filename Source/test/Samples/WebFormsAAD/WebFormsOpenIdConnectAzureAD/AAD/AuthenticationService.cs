using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using Aliencube.AdalWrapper;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security.Notifications;
using WebFormsOpenIdConnectAzureAD;
using WebFormsOpenIdConnectAzureAD.Models;

//using TSA.Applications.WebjetTsa.Models;

namespace TSA.Applications.WebjetTsa.Admin.AAD
{
    public class AuthenticationService
    {
        public static string ClientId = ConfigurationManager.AppSettings["ida:ClientId"];
        public static string ClientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"]; //aka AppKey
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        //   private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];

        // This is the resource ID of the AAD Graph API.  We'll need this to request a token to call the Graph API.
        private static string graphResourceId = "https://graph.windows.net";
        public static string Authority = aadInstance + tenantId;

        private IAuthenticationContextWrapper _authenticationContextWrapper;

        public AuthenticationService(IAuthenticationContextWrapper wrapper=null)
        {
            _authenticationContextWrapper = wrapper;
        }
        public bool IsMock { get; set; }
        public string MockResponseString { get; set; }

        /// <summary>
        /// If there is a code in the OpenID Connect response, redeem it for an access token and refresh token, and store those away.
        /// ADAL V3 Async from  https://github.com/Azure-Samples/active-directory-dotnet-webapp-webapi-openidconnect/blob/master/TodoListWebApp/App_Start/Startup.Auth.cs
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification context)
        {
            var code = context.Code;

            ClientCredential credential = new ClientCredential(ClientId, ClientSecret);//aka appKey;
            string userObjectId = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            var authContext = new AuthenticationService().NewAuthenticationContext(userObjectId, HttpContext.Current);

            // If you create the redirectUri this way, it will contain a trailing slash.  
            // Make sure you've registered the same exact Uri in the Azure Portal (including the slash).
            Uri uri = new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path));

            var result = await authContext.AcquireTokenByAuthorizationCodeAsync(code, uri, credential, graphResourceId);
        }
        /// <summary>
        /// See other alternatives in http://www.cloudidentity.com/blog/2014/07/09/the-new-token-cache-in-adal-v2/
        /// FileCache, EFADALTokenCache, NaiveSessionCache
        /// </summary>
        /// <param name="signedInUserId"></param>
        /// <param name="httpContext"></param>
        /// <param name="tokenCache">Null in production, not null for tests</param>
        /// <returns></returns>
        public virtual IAuthenticationContextWrapper NewAuthenticationContext(string signedInUserId, HttpContext httpContext, TokenCache tokenCache=null)
        {
            if (_authenticationContextWrapper == null)
            {
                bool useDB = false;//TODO EncryptTsa
                bool useSession = false;
                AuthenticationContext authContext = null;
                AuthenticationContextWrapper wrapper = null;
                if (useDB == true)
                {
                    var adalTokenCache = tokenCache ?? (IsMock ? new TokenCache() : new ADALTokenCache(signedInUserId));
                    authContext = new AuthenticationContext(Authority, adalTokenCache);
                   
                }
//#if TRY_OTHER_TokenCache_Options
             //CreateEnum
           
             if (useDB==false && httpContext != null)
            {
                authContext = new AuthenticationContext(Authority, new NaiveSessionCache(httpContext, signedInUserId,useSession));
            }
            else
            {
                authContext = new AuthenticationContext(Authority, new ADALTokenCache(signedInUserId));
            }
                //#endif //TRY_OTHER_TokenCache_Options
                wrapper = new AuthenticationContextWrapper(authContext);
                return wrapper;
            }
            else
            {
                return _authenticationContextWrapper;
            }
        }
    }
}