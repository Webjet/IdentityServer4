using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Protocols;
using WebFormsOpenIdConnectAzureAD.Models;
using Microsoft.Owin.Security;
using NLog;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;

namespace WebFormsOpenIdConnectAzureAD
{
    public partial class Contact : Page
    {
        private string adminPortalApiResourceId = ConfigurationManager.AppSettings["AdminPortalApi:ResourceId"];
        private string adminPortalApiBaseAddress = ConfigurationManager.AppSettings["AdminPortalApi:BaseAddress"];
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:AppKey"];

        private NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        protected void Page_Load(object sender, EventArgs e)
        {
            GetAADToken();
        }

        private async void GetAADToken()
        {
            try
            {
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority, new ADALTokenCache(userObjectID));
                
                ClientCredential credential = new ClientCredential(clientId, appKey);
                 
                AuthenticationResult result = await authContext.AcquireTokenSilentAsync(adminPortalApiResourceId, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));
                
                GetHttpResponseFromAdminPortalApi(result.AccessToken);
            }
            catch (AdalException ex)
            {
                File.WriteAllText("c:\\Exception.txt", ex.ToString());
              
            }
        }

        private async void GetHttpResponseFromAdminPortalApi(string accessToken)
        {
            string adminPortalApiSuffix = "/api/AllowedRolesForResource/ReviewPendingBookings_WebjetAU";
            //string adminPortalApiSuffix = "/api/AllowedRolesForResource";

            HttpClient client = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, adminPortalApiBaseAddress + adminPortalApiSuffix);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {

            }
        }
    }
}