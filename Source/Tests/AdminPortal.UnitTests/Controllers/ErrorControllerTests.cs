using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdminPortal.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
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
    public class ErrorControllerTests
    {
        [TestMethod()]
        public void ShowErrorTest()
        {
            //Arrange
            var httpContext = Substitute.For<HttpContextBase>();
            httpContext.User = PrincipalStubBuilder.GetUserWithServiceCenterAnalyticsAndFinanceRoles();
            string errorMessage = "You do not have sufficient priviliges to view this page.";
            
            //Act
            var controller = new ErrorController();
            controller.ControllerContext = new ControllerContext()
            {
                Controller = (ErrorController)controller,
                RequestContext = new RequestContext(httpContext, new RouteData())
            };

            var result = controller.ShowError(errorMessage, null) as ViewResult;

            //Assert
            result.ViewData["SignIn"].ShouldBeEquivalentTo(null);
            result.ViewData["ErrorMessage"].ShouldBeEquivalentTo(errorMessage);


        }
    }
}