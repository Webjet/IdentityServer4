using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdminPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AdminPortal.Controllers;
using AdminPortal.UnitTests;
using AdminPortal.UnitTests.TestUtilities;
using FluentAssertions;
using NSubstitute;

namespace AdminPortal.Tests
{
    [TestClass()]
    public class AuthorizeAttributeTests : AuthorizeAttribute
    {

        [TestMethod()]
        public void AuthorizeAttribute_ResourceKey_Test()
        {
            //Arrange
            string resourceKey = "GoogleBigQueryItinerary";

            //Act
            AuthorizeAttribute target = new AuthorizeAttribute(resourceKey);

            //Assert
            resourceKey.Should().NotBeEmpty();
            target.Should().NotBeNull();
        }

     
        [TestMethod()]
        public void AuthorizeCore_HttpContextRequestAuthenticated_AuthorizeRoles()
        {
            //Arrange
            string resourceKey = "GoogleBigQueryItinerary";
            var filterContext = GetFilterAuthorizationContext();
            filterContext.HttpContext.Request.IsAuthenticated.Returns(true);

            //Act
            AuthorizeAttribute target = new AuthorizeAttribute(resourceKey);
             target.OnAuthorization(filterContext);
            
            //Assert
             target.Roles.Length.Should().BePositive();
             target.Roles.ShouldBeEquivalentTo("ServiceCenter,AnalyticsTeam");

        }

        [TestMethod()]
        public void AuthorizeCore_HttpContextRequestUnAuthenticated_AuthorizeRoles()
        {
            //Arrange
            string resourceKey = "GoogleBigQueryItinerary";
            var filterContext = GetFilterAuthorizationContext();
            filterContext.HttpContext.Request.IsAuthenticated.Returns(false);

            //Act
            AuthorizeAttribute target = new AuthorizeAttribute(resourceKey);
            target.OnAuthorization(filterContext);

            //Assert
            target.Roles.Should().BeNullOrEmpty();
            
        }


        [TestMethod()]
        public void HandleUnauthorizedRequest_FilterContextRequestAuthenticated_RedirectToErrorController()
        {
            //Arrange
            var filterContext = GetFilterAuthorizationContext();
            filterContext.HttpContext.Request.IsAuthenticated.Returns(true);

            //Act
            base.HandleUnauthorizedRequest(filterContext);

            //Assert
            filterContext.Result.Should().NotBeNull();

            ((System.Web.Mvc.RedirectToRouteResult)filterContext.Result).RouteValues.Keys.Count.Should().Be(3);
            ((System.Web.Mvc.RedirectToRouteResult)filterContext.Result).RouteValues["controller"].Equals("Error")
                .Should()
                .BeTrue();

            ((System.Web.Mvc.RedirectToRouteResult)filterContext.Result).RouteValues["action"].Equals("ShowError")
                .Should()
                .BeTrue();


            ((System.Web.Mvc.RedirectToRouteResult)filterContext.Result).RouteValues["errorMessage"].Equals("You do not have sufficient priviliges to view this page.")
                .Should()
                .BeTrue();

        }

        [TestMethod()]
        public void HandleUnauthorizedRequest_FilterContextRequestUnAuthenticated_CallsBaseHandleUnauthorizedRequest()
        {
            //Arrange
            var filterContext = GetFilterAuthorizationContext();
            filterContext.HttpContext.Request.IsAuthenticated.Returns(false);

            //Act
            base.HandleUnauthorizedRequest(filterContext);

            //Assert
            filterContext.Result.Should().NotBeNull();

        }

        private AuthorizationContext GetFilterAuthorizationContext()
        {
            var httpContext = Substitute.For<HttpContextBase>();
            httpContext.User = PrincipalStubBuilder.GetLoggedInUser();
            var controller = Substitute.For<ControllerBase>();
            var actionDescriptor = Substitute.For<ActionDescriptor>();
            var controllerContext = new ControllerContext(httpContext, new RouteData(), controller);
            return new AuthorizationContext(controllerContext, actionDescriptor);
        }

    }
}