using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;


namespace AdminPortal.BusinessServices.GraphApiHelper
{
    public class ActiveDirectoryGraphHelper
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
        public static IConfigurationRoot ConfigurationRoot
        {
            private get; set;
        }

        public IActiveDirectoryClient ActiveDirectoryClient { get; set; }

        public ActiveDirectoryGraphHelper() : this(null, null)
        {

        }

        public ActiveDirectoryGraphHelper(IConfigurationRoot config, IActiveDirectoryClient client)
        {
            ConfigurationRoot = config ?? ConfigurationRoot;
            _azureGraphAPI = ConfigurationRoot["Authentication:AzureAd:ResourceId"];
            _tenantId = ConfigurationRoot["Authentication:AzureAd:TenantId"];
            _clientId = ConfigurationRoot["Authentication:AzureAd:ClientId"];
            _clientSecret = ConfigurationRoot["Authentication:AzureAd:ClientSecret"];
            _authority = ConfigurationRoot["Authentication:AzureAd:AADInstance"] + ConfigurationRoot["Authentication:AzureAd:TenantId"];
            _userName = ConfigurationRoot["Authentication:AzureAd:UserName"];
            _password = ConfigurationRoot["Authentication:AzureAd:Password"];
            _grantType = ConfigurationRoot["Authentication:AzureAd:GrantType"];
            _tokenEndpoint = ConfigurationRoot["Authentication:AzureAd:TokenEndpoint"];

            ActiveDirectoryClient = client ?? GetActiveDirectoryGraphClient();
        }

        //https://github.com/Azure-Samples/active-directory-dotnet-graphapi-web
        //active-directory-dotnet-graphapi-web
        public IActiveDirectoryClient GetActiveDirectoryGraphClient()
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
