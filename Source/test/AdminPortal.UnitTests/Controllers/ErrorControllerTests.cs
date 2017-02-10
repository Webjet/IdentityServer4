using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdminPortal.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using AdminPortal.UnitTests;
using AdminPortal.UnitTests.TestUtilities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            string errorMessage = "You do not have sufficient priviliges to view this page.";
            var controller = ControllerAssertions.ArrangeController<ErrorController>(null);

            ////Act
            //var controller = new ErrorController();
            //controller.ControllerContext = new ControllerContext()
            //{
            //    HttpContext =
            //        new DefaultHttpContext()
            //        {
            //            User =
            //                PrincipalStubBuilder.GetClaimPrincipalWithServiceCenterAnalyticsAndFinanceRoles()
            //        }
            //};

            var result = controller.ShowError(errorMessage, null) as ViewResult;

            //Assert
            result.ViewData["SignIn"].ShouldBeEquivalentTo(null);
            result.ViewData["ErrorMessage"].ShouldBeEquivalentTo(errorMessage);


        }
    }
}