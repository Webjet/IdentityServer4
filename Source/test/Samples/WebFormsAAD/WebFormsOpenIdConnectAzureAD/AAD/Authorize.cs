//It's prototype, the actual class is in TSA C:\GitRepos\tsa\main\TSA\Applications\WebjetTsa\Admin\AAD\Authorize.cs

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using WebFormsOpenIdConnectAzureAD.Models;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;
using System.Configuration;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
//using TSA.Applications.WebjetTsa;
//using TSA.BusinessEntities;

namespace WebFormsOpenIdConnectAzureAD
{
    public class Authorize : CodeAccessSecurityAttribute
    {
        private string adminPortalApiResourceId = ConfigurationManager.AppSettings["AdminPortalApi:ResourceId"];
        private string adminPortalApiBaseAddress = ConfigurationManager.AppSettings["AdminPortalApi:BaseAddress"];
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string _clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private static string _brandRegion = ConfigurationManager.AppSettings["BrandRegionSuffix"];//ToDO change new SiteInfo().BrandRegion;//
        private bool _adminPortalApiMock = true;
        private HttpContext _httpContext = null;
        public Authorize(SecurityAction action) : base(action)
        {
            Debug.Assert(HttpContext.Current!=null);
          _httpContext = HttpContext.Current;
            Global.LogSession(MethodBase.GetCurrentMethod().Name, this);
        }

       public string ResourceKey { get; set; }

        public override IPermission CreatePermission()
        {
            string resourceKey = ResourceKey + "_" + _brandRegion;
            Task<bool> task = Task.Run<bool>(async () => await GetHttpResponseFromAdminPortalApiAsync(_httpContext, resourceKey));
            bool isAllowed = task.Result;
            var permissionState = isAllowed ? PermissionState.Unrestricted : PermissionState.None;
            return new PrincipalPermission(permissionState);
           //     throw new SecurityException("Sorry!! You do not have access permission for requested resource " + resourceKey);


        }

        private async Task<bool> GetHttpResponseFromAdminPortalApiAsync(HttpContext httpContext, string resourceKey)
        {
            string accessToken = null;
            bool userAllowedForResource = false;
            try
            {
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            AuthenticationContext authContext = Startup.NewAuthenticationContext(userObjectID, httpContext); //new AuthenticationContext(Startup.Authority, new ADALTokenCache(userObjectID));
            Debug.Assert(authContext.TokenCache.Count > 0);
            ClientCredential credential = new ClientCredential(clientId, _clientSecret);

                AuthenticationResult result = await authContext.AcquireTokenSilentAsync(adminPortalApiResourceId, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId)).ConfigureAwait(false);

                accessToken = result.AccessToken;
            }
            catch (AdalSilentTokenAcquisitionException ex)
            {
                //http://stackoverflow.com/questions/41519132/how-to-store-the-token-received-in-acquiretokenasync-with-active-directory
                //https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie
                //try to get token as in start 

                //   Debug.WriteLine(ex.ToString());
                throw;
            }

            //try
            //{
            //// API in ASP .Net MVC, named as 'WebejetAdminPortal', coupled with AdminPortal solution
            //string adminPortalApiSuffix = "/api/AllowedRolesForResource/ReviewPendingBookings_WebjetAU";
            //string adminPortalApiSuffix = "/api/AllowedRolesForResource"; 

            //// API in ASP Core named as 'AdminPortalWebApi' and added as separate project in AdminPortal solution
            string adminPortalApiSuffix = "/api/AllowedRolesForResource/" + resourceKey; //"/api/values"; 
                HttpResponseMessage response = null;
                if (!adminPortalApiBaseAddress.StartsWith("Mock:"))
                {
                    HttpClient client = new HttpClient();
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, adminPortalApiBaseAddress + adminPortalApiSuffix);

                    //http://brainof-dave.blogspot.com.au/2008/08/remote-certificate-is-invalid-according.html
                    ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                    { return true; };

                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                    string responseString = await response.Content.ReadAsStringAsync();
                    userAllowedForResource = responseString == "True" ? true : false;
                    }
                    else
                    {
                       Debug.WriteLine("request:"+request.ToString() + "response:" + response.ToString());//TODO: Logger.
                    }
                }
                else
                {
                    response = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = new StringContent("") };
                    userAllowedForResource = true;
                }
            //}

            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.ToString());
            //}


            return userAllowedForResource;
        }

    }
}