using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;


namespace AdminPortal.BusinessServices.GraphApiHelper
{
    public interface IActiveDirectoryGraphHelper
    {
        IActiveDirectoryClient ActiveDirectoryClient { get; set; }

    }

    public class ActiveDirectoryGraphHelper: IActiveDirectoryGraphHelper
    {
        public static string token;
        public static string _azureGraphAPI;
        public static string _tenantId;
        public static string _authority;
        private static string _clientId;
        private static string _clientSecret;
        private static string _userName;
        private static string _password;
        private static string _contentType = "application/x-www-form-urlencoded";
        // Don't use password grant in your apps. Only use for legacy solutions and automated testing.
        private static string _grantType;
        private static string _tokenEndpoint;
        private static string _accessToken = null;
        private static string _tokenForUser = null;
        private static System.DateTimeOffset _expiration;
      

        public IActiveDirectoryClient ActiveDirectoryClient { get; set; }


        public ActiveDirectoryGraphHelper(IConfigurationRoot config)
        {
           
            _azureGraphAPI = config["Authentication:AzureAd:ResourceId"];
            _tenantId = config["Authentication:AzureAd:TenantId"];
            _clientId = config["Authentication:AzureAd:ClientId"];
            _clientSecret = config["Authentication:AzureAd:ClientSecret"];
            _authority = config["Authentication:AzureAd:AADInstance"] + config["Authentication:AzureAd:TenantId"];
            _userName = config["Authentication:AzureAd:UserName"];
            _password = config["Authentication:AzureAd:Password"];
            _grantType = config["Authentication:AzureAd:GrantType"];
            _tokenEndpoint = config["Authentication:AzureAd:TokenEndpoint"];

            ActiveDirectoryClient = GetActiveDirectoryGraphClient();
        }

        //https://github.com/Azure-Samples/active-directory-dotnet-graphapi-web
        //active-directory-dotnet-graphapi-web
        private IActiveDirectoryClient GetActiveDirectoryGraphClient()
        {
            Uri baseServiceUri = new Uri(_azureGraphAPI);
            IActiveDirectoryClient activeDirectoryClient =
                new ActiveDirectoryClient(new Uri(baseServiceUri, _tenantId),
                    async () => await AcquireTokenUsingPasswordGrantAsync());
            return activeDirectoryClient;
        }

        //https://github.com/microsoftgraph/msgraph-sdk-dotnet
        //Is used to acquire password grant token- refer test class in code: tests\Microsoft.Graph.Test\Requests\Functional\GraphTestBase.cs

        private static async Task<string> AcquireTokenUsingPasswordGrantAsync()
        {
            JObject jResult = null;
            string urlParameters = string.Format(
                    "grant_type={0}&resource={1}&client_id={2}&client_secret={3}&username={4}&password={5}",
                    _grantType,
                    _azureGraphAPI,
                    _clientId,
                    _clientSecret,
                    _userName,
                    _password
            );

            var client = new HttpClient();
            var createBody = new StringContent(urlParameters, System.Text.Encoding.UTF8, _contentType);

            HttpResponseMessage response = await client.PostAsync(_tokenEndpoint, createBody);

            if (response.IsSuccessStatusCode)
            {
                Task<string> responseTask = response.Content.ReadAsStringAsync();
                responseTask.Wait();
                string responseContent = responseTask.Result;
                jResult = JObject.Parse(responseContent);
            }
            _accessToken = (string)jResult["access_token"];

            if (!String.IsNullOrEmpty(_accessToken))
            {
                //Set ActiveDirectoryGraphHelper values so that the regular MSAL auth flow won't be triggered.
                _tokenForUser = _accessToken;
                _expiration = DateTimeOffset.UtcNow.AddHours(5);
            }

            return _accessToken;
        }

    }
}
