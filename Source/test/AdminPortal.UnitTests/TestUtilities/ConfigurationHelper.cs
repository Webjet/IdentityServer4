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
            string googleBigQueryItineraryDirectoryPath = directoryPath + @"Controllers\Content\GoogleBigQueryItinerary.html";
            string generateRandomNumberDirectoryPath = directoryPath + @"Controllers\Content\GenerateRandomNumber.html";
            string googleBigQueryHostUrl = "http://127.0.0.1:5000/api/v1.0/query";
            
            config["GoogleBigQueryItineraryDirectoryPath"].Returns(googleBigQueryItineraryDirectoryPath);
            config["GenerateRandomNumberDirectoryPath"].Returns(generateRandomNumberDirectoryPath);
            config["GoogleBigQueryHostUrl"].Returns(googleBigQueryHostUrl);

            //config.GetValue<string>("GoogleBigQueryItineraryDirectoryPath", googleBigQueryItineraryDirectoryPath).Returns(googleBigQueryItineraryDirectoryPath);
            // config.GetValue<string>("GenerateRandomNumberDirectoryPath", generateRandomNumberDirectoryPath).Returns(generateRandomNumberDirectoryPath);

            return config;
        }

    }
}
