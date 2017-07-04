using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using Webjet.Common.Strings;


namespace AdminPortal.BusinessServices.GraphApiHelper
{
    public interface IActiveDirectoryGraphHelper
    {
        IActiveDirectoryClient ActiveDirectoryClient { get; set; }

    }



    public class ActiveDirectoryGraphHelper : IActiveDirectoryGraphHelper
    {

        private string _azureGraphAPI;
        private  string _tenantId;
        private IActiveDirectoryClient _activeDirectoryClient;

        public static string Token;

        public IActiveDirectoryClient ActiveDirectoryClient
        {
            get
            {
                return _activeDirectoryClient ?? GetActiveDirectoryGraphClient();
            }
            set
            {
                _activeDirectoryClient = value;
            }
        }


        public ActiveDirectoryGraphHelper(IConfigurationRoot config)
        {
            _azureGraphAPI = config["Authentication:AzureAd:ResourceId"];
            _tenantId = config["Authentication:AzureAd:TenantId"];
        }

        //active-directory-dotnet-graphapi-web
        public static async Task<string> AcquireTokenAsync()
        {
            if (Token == null || Token.IsEmptyOrWhiteSpace())
            {
                throw new AuthorizationException(HttpStatusCode.InternalServerError, "Authorization Required.");
            }
            return Token;
        }

      

        private IActiveDirectoryClient GetActiveDirectoryGraphClient()
        {
            Uri baseServiceUri = new Uri(_azureGraphAPI);
            IActiveDirectoryClient activeDirectoryClient =
                new ActiveDirectoryClient(new Uri(baseServiceUri, _tenantId),
                    async () => await AcquireTokenAsync());
            return activeDirectoryClient;
        }

       
    }
}
