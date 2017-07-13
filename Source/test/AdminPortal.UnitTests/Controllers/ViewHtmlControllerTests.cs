using System;
using System.Linq;
using System.Web;
using System.Web.Routing;
using AdminPortal.Controllers;
using AdminPortal.UnitTests.TestUtilities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog.LayoutRenderers.Wrappers;
using NSubstitute;

namespace AdminPortal.UnitTests.Controllers
{
    [TestClass()]
    public class ViewHtmlControllerTests
    {
        [TestMethod()]
        public void GoogleBigQueryItineraryTest()
        {
            //Arrange
            var config = ConfigurationHelper.GetConfigurationSubsitituteForViewHtmlController();
            object[] args = new object[] { config };
            var controller = ControllerAssertions.ArrangeController<ViewHtmlController>(args);

            //Act
            var result = controller.GoogleBigQueryItinerary() as ViewResult;

            //Assert
            //TODO DI AppSettings and assign value for AppSettings["GoogleBigQueryHostUrl"];

            ControllerAssertions.AssertViewWithViewData(result,null, "GoogleBigQueryHostUrl");
            result.ViewData.Count().Should().Be(2);
            result.ViewData["GoogleBigQueryHostUrl"].Should().Be("http://127.0.0.1:5000/api/v1.0/query");
        }

        [TestMethod()]
        public void GoogleBigQueryItinerary_ReplaceGoogleAnalyticsCustomerJourneyStaticUrls_AllStaticUrlReplaced()
        {
            //Arrange
            var config = ConfigurationHelper.GetConfigurationSubsitituteForViewHtmlController();
            object[] args = new object[] { config };
            var controller = ControllerAssertions.ArrangeController<ViewHtmlController>(args);
            
            //Act
            var result = controller.GoogleBigQueryItinerary() as ViewResult;

            //Assert
            var htmlContent = result.ViewData["HtmlContent"].ToString();
            htmlContent.Should().NotContain("src=\"static/");
            htmlContent.Should().NotContain("href=\"static/");
            htmlContent.Should().Contain("script src=\"http://localhost/GoogleAnalyticsCustomerJourney/Static/");
            htmlContent.Should().Contain("href=\"http://localhost/GoogleAnalyticsCustomerJourney/Static/");
        }

        [TestMethod()]
        public void GenerateRandomNumberTest()
        {
            //Arrange
            var config = ConfigurationHelper.GetConfigurationSubsitituteForViewHtmlController();
            object[] args = new object[] { config };
            var controller = ControllerAssertions.ArrangeController<ViewHtmlController>(args);

            //Act
            var result = controller.GenerateRandomNumber() as ViewResult;

            //Assert
            ControllerAssertions.AssertViewWithViewData(result, "Index", "HtmlContent");
            result.ViewData.Count().Should().Be(1);
        }

     
       
    }
}