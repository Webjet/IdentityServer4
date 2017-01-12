using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdminPortal.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AdminPortal.UnitTests;
using AdminPortal.UnitTests.TestUtilities;
using FluentAssertions;
using NSubstitute;

namespace AdminPortal.Controllers.Tests
{
    [TestClass()]
    public class GoogleBigQueryItineraryControllerTests
    {
        [TestMethod()]
        public void IndexTest()
        {
            //Arrange
            var httpContext = Substitute.For<HttpContextBase>();
            httpContext.User = PrincipalStubBuilder.GetLoggedInUser();
          
            //Act
            var controller = new GoogleBigQueryItineraryController();
            controller.ControllerContext = new ControllerContext()
            {
                Controller = (GoogleBigQueryItineraryController)controller,
                RequestContext = new RequestContext(httpContext, new RouteData())
            };

            var result = controller.Index() as ViewResult;

            //Assert
            result.Should().NotBeNull();

        }
    }
}