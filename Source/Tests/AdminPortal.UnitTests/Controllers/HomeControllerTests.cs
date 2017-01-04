﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdminPortal.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.WebControls;
using AdminPortal.UnitTests;
using FluentAssertions;
using NSubstitute;

namespace AdminPortal.Controllers.Tests
{
    [TestClass()]
    public class HomeControllerTests
    {
        [TestMethod()]
        public void IndexTest()
        {
            //Arrange
            var httpContext = Substitute.For<HttpContextBase>();
            httpContext.User = TestHelper.GetUserIdentityPrincipal();
           
            //Act
            var controller = new HomeController();
            controller.ControllerContext = new ControllerContext()
            {
                Controller = (HomeController)controller,
                RequestContext = new RequestContext(httpContext,new RouteData())
            };
            
            var result = controller.Index() as ViewResult;

            //Assert
            result.Model.Should().NotBeNull();
            

        }
    }
}