using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace AdminPortal.UnitTests.TestUtilities
{
    public static class ConfigurationHelper
    {
        public static IConfigurationRoot GetConfigurationSubsitituteForViewHtmlController()
        {
            var config = Substitute.For<IConfigurationRoot>();

            string directoryPath = AssemblyHelper.GetExecutingAssemblyRootPath();
            string googleBigQueryItineraryDirectoryPath = AssemblyHelper.GetExecutingAssemblyDirectoryPath() + @"\src\AdminPortal\Views\ViewHtml\GoogleAnalyticsCustomerJourney\Templates\index.html";
            string generateRandomNumberDirectoryPath = directoryPath + @"Controllers\Content\GenerateRandomNumber.html";
            string googleBigQueryHostUrl = "http://127.0.0.1:5000/api/v1.0/query";

            config["GoogleBigQueryItineraryDirectoryPath"].Returns(googleBigQueryItineraryDirectoryPath);
            config["GenerateRandomNumberDirectoryPath"].Returns(generateRandomNumberDirectoryPath);
            config["GoogleBigQueryHostUrl"].Returns(googleBigQueryHostUrl);

            //config.GetValue<string>("GoogleBigQueryItineraryDirectoryPath", googleBigQueryItineraryDirectoryPath).Returns(googleBigQueryItineraryDirectoryPath);
            // config.GetValue<string>("GenerateRandomNumberDirectoryPath", generateRandomNumberDirectoryPath).Returns(generateRandomNumberDirectoryPath);

            return config;
        }
        public static IConfigurationRoot GetConfigurationSubsitituteForResourceAuthorizeAttribute()
        {
            var config = Substitute.For<IConfigurationRoot>();
            string resourceToRolesMapRelativePath = "Config\\ResourceToRolesMap.xml";
            config["ResourceToRolesMapRelativePath"].Returns(resourceToRolesMapRelativePath);
            return config;
        }

        public static IConfigurationRoot GetConfigurationSubsitituteForGraphAPIClient()
        {
            var config = Substitute.For<IConfigurationRoot>();
            string azureGraphAPI = "https://graph.windows.net";
            string tenantId = "0f79d43c";
            string clientId = "43c42f66";
            string clientSecret = "5arVK/5i4dNjgWp8JniOV";
            string aadInstance = "https://login.microsoftonline.com/";
            string userName = "Kajal.Bhatia@mfreidgeimwebjetcom.onmicrosoft.com";
            string password = "password";
            string grantType = "password";
            string tokenEndpoint = "https://login.windows.net/up8779dc-34ea-tee3-34gfg-34453/oauth2/token";

            config["Authentication:AzureAd:ResourceId"].Returns(azureGraphAPI);
            config["Authentication:AzureAd:TenantId"].Returns(tenantId);
            config["Authentication:AzureAd:ClientId"].Returns(clientId);
            config["Authentication:AzureAd:ClientSecret"].Returns(clientSecret);
            config["Authentication:AzureAd:AADInstance"].Returns(aadInstance);
            config["Authentication:AzureAd:UserName"].Returns(userName);
            config["Authentication:AzureAd:Password"].Returns(password);
            config["Authentication:AzureAd:GrantType"].Returns(grantType);
            config["Authentication:AzureAd:TokenEndpoint"].Returns(tokenEndpoint);


            return config;
        }

        //string azureGraphAPI = "https://graph.windows.net";
        //string tenantId = "0f79d43c-098d-49b1-b8bc-4c9107c64dd4";
        //string clientId = "43c42f66-e21c-4d89-a5ca-8a8ebc2be260";
        //string clientSecret = "BuAIZdge7qO5arVK/5i4dNjgWx3OKtUWp8JniOVlr8c=";
        //string aadInstance = "https://login.microsoftonline.com/";
        //string userName = "Kajal.Bhatia@mfreidgeimwebjetcom.onmicrosoft.com";
        //string password = "ADMTest1";
        //string grantType = "password";
        //string tokenEndpoint = "https://login.windows.net/0f79d43c-098d-49b1-b8bc-4c9107c64dd4/oauth2/token";

    }
}
