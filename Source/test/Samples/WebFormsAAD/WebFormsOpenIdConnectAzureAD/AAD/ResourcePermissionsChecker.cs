using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using TSA.Applications.WebjetTsa.Admin.AAD;
using WebFormsOpenIdConnectAzureAD.AAD.Common;

namespace WebFormsOpenIdConnectAzureAD.AAD
{
    public class ResourcePermissionsChecker
    {
        private string adminPortalApiResourceId = ConfigurationManager.AppSettings["AdminPortalApi:ResourceId"];
        private string adminPortalApiBaseAddress = ConfigurationManager.AppSettings["AdminPortalApi:BaseAddress"];
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string _clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private static string _regionSuffix ="_"+ ConfigurationManager.AppSettings["BrandRegionSuffix"];//ToDO change new SiteInfo().BrandRegion;//
        private bool _adminPortalApiMock = true;
        private HttpContext _httpContext = null;
        private const string _allowedResourcesForUser = "AllowedResourcesForUser";


        public async void Authorise(Control theControl)
        {
            var securityKey = theControl.GetType().Name.TrimEnd("_aspx").TrimEnd("_ascx");
            var session = theControl.Page.Session;
            if (session != null)
            {
                var resourcesList = session[_allowedResourcesForUser] as List<String>;
                if (resourcesList == null)
                {
                    try
                    {
                        resourcesList = await GetResourcesForUser(HttpContext.Current);//theControl.Context protected;
                    }
                    catch (Exception e)
                    {
                       Debug.Assert(false,e.ToString());
                        //TODO nlog
                    }
                    session[_allowedResourcesForUser] = resourcesList;
                }
                if (resourcesList != null)
                {
                    bool found = resourcesList.Contains(securityKey, StringComparer.InvariantCultureIgnoreCase);
                    if (!found)
                    {
                        var list = resourcesList.TrimEnds(_regionSuffix).ToList();
                        found = list.Contains(securityKey, StringComparer.InvariantCultureIgnoreCase);
                    }
                    if (!found)
                    {
                        ThrowException(securityKey);
                    }
                    else
                    {
                        return;
                    }
                }
            }

            //No session, call for each resource individually
            // AuthorizeIsResourceAllowed(securityKey);
            ThrowException(securityKey);
        }
        private async Task<List<string>> GetResourcesForUser(HttpContext httpContext)
        {
            string resourceKey = "GetResourcesForUser";
            var responseString = await CallApiAllowedRolesForResource(httpContext, resourceKey);
            var resourcesList = JsonConvert.DeserializeObject<List<string>>(responseString);

            return resourcesList;
        }
        internal void ThrowException(string resourceKey)
        {
                _httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                throw new SecurityException("Sorry!! You do not have access permission for requested resource " + resourceKey);
            }
        private async Task<bool> GetIsResourceAllowedAsync(HttpContext httpContext, string resourceKey)
        {
            var responseString = await CallApiAllowedRolesForResource(httpContext, resourceKey);
            var userAllowedForResource = Boolean.Parse(responseString);
            return userAllowedForResource;
        }
        private async Task<string> CallApiAllowedRolesForResource(HttpContext httpContext, string resourceKey)
        {
            string accessToken;
            string responseString = "";
            try
            {
                string userObjectID =
                    ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

                var authContext = new AuthenticationService().NewAuthenticationContext(userObjectID, httpContext);
                //new AuthenticationContext(Startup.Authority, new ADALTokenCache(userObjectID));
                Debug.Assert(authContext.TokenCache.Count > 0);
                ClientCredential credential = new ClientCredential(clientId, _clientSecret);

                var result =
                    await authContext.AcquireTokenSilentAsync(adminPortalApiResourceId, credential,
                        new UserIdentifier(userObjectID, UserIdentifierType.UniqueId)).ConfigureAwait(false);

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

            //// API in ASP .Net MVC, named as 'WebejetAdminPortal', coupled with AdminPortal solution
            //string adminPortalApiSuffix = "/api/AllowedRolesForResource/ReviewPendingBookings_WebjetAU";
            //string adminPortalApiSuffix = "/api/AllowedRolesForResource"; 

            //// API in ASP Core named as 'AdminPortalWebApi' and added as separate project in AdminPortal solution
            string adminPortalApiSuffix = "/api/AllowedRolesForResource/" + resourceKey; //"/api/values"; 
            HttpResponseMessage response = null;
            if (!adminPortalApiBaseAddress.StartsWith("Mock:"))
            {
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                    adminPortalApiBaseAddress + adminPortalApiSuffix);

                //http://brainof-dave.blogspot.com.au/2008/08/remote-certificate-is-invalid-according.html
                ServicePointManager.ServerCertificateValidationCallback =
                    delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                    {
                        return true;
                    };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    responseString = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    Debug.WriteLine("request:" + request.ToString() + "response:" + response.ToString()); //TODO: Logger.
                }
            }
            else
            {
                response = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = new StringContent("") };
                responseString = "true";
            }
            return responseString;
        }

        public bool IsResourceAllowed(string resourceKey)
        {
            string resourceKeyWithRegion = resourceKey + _regionSuffix;
            Task<bool> task = Task.Run<bool>(async () => await GetIsResourceAllowedAsync(_httpContext, resourceKeyWithRegion));
            bool isAllowed = task.Result;
            return isAllowed;
        }

        public void AuthorizeIsResourceAllowed(string resourceKey)
        {
            bool isAllowed = IsResourceAllowed(resourceKey);
            if (isAllowed == false)
            {
                ThrowException(resourceKey);
            }
        }

    }
}