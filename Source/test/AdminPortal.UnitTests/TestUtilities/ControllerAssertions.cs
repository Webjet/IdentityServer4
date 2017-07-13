using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using HttpContext = System.Web.HttpContext;

namespace AdminPortal.UnitTests.TestUtilities
{
    public class ControllerAssertions
    {
        public static T ArrangeController<T>(object[] args) where T : Controller
        {
            //string directoryPath = AssemblyHelper.GetExecutingAssemblyRootPath();
            //string googleBigQueryItineraryDirectoryPath = directoryPath + @"Controllers\Content\GoogleBigQueryItinerary.html";
            //string generateRandomNumberDirectoryPath = directoryPath + @"Controllers\Content\GenerateRandomNumber.html";
            //string googleBigQueryHostUrl= "http://127.0.0.1:5000/api/v1.0/query";

            //var config = Substitute.For<IConfigurationRoot>();
            //config["GoogleBigQueryItineraryDirectoryPath"].Returns(googleBigQueryItineraryDirectoryPath);
            //config["GenerateRandomNumberDirectoryPath"].Returns(generateRandomNumberDirectoryPath);
            //config["GoogleBigQueryHostUrl"].Returns(googleBigQueryHostUrl);

            var httpContext =
                    new DefaultHttpContext()
                    {
                        User =
                            PrincipalStubBuilder.GetClaimPrincipalWithServiceCenterAnalyticsAndFinanceRoles()
                    };

            httpContext.Request.Scheme = "http";
            httpContext.Request.Host = new HostString("localhost");

            var controller = (T)Activator.CreateInstance(typeof(T), args); // to add args , see  http://stackoverflow.com/questions/840261/passing-arguments-to-c-sharp-generic-new-of-templated-type
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };            

            return controller;
        }


        public static void AssertViewWithViewData(ViewResult result, string viewName, string viewDataKey)
        {
            result.Should().NotBeNull();
            result.ViewName.Should().Be(viewName);
            result.ViewData.ContainsKey(viewDataKey).Should().BeTrue();

        }
    }
}
