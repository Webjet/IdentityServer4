using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AdminPortal.Controllers;
using AdminPortal.UnitTests.TestUtilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog.LayoutRenderers.Wrappers;
using NSubstitute;

namespace AdminPortal.UnitTests.Controllers
{
    [TestClass()]
    public class GoogleBigQueryItineraryControllerTests
    {
        [TestMethod()]
        public void GoogleBigQueryItineraryTest()
        {
            //Arrange
            var controller = ControllerAssertions.ArrangeController<ViewHtmlController>();

            //Act
            var result = controller.GoogleBigQueryItinerary() as ViewResult;

            //Assert
            //TODO DI AppSettings and assign value for AppSettings["GoogleBigQueryHostUrl"];
            ControllerAssertions.AssertViewWithViewData(result,"", "GoogleBigQueryHostUrl",null);

        }


        [TestMethod()]
        public void GenerateRandomNumberTest()
        {
            //Arrange
            var controller = ControllerAssertions.ArrangeController<ViewHtmlController>();

            //Act
            var result = controller.GenerateRandomNumber() as ViewResult;

            //Assert
            ControllerAssertions.AssertViewWithViewData(result, "Index", "PageName", "GenerateRandomNumber.html");

        }

     
       
    }
}