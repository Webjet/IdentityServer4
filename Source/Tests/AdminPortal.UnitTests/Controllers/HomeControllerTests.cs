using Microsoft.VisualStudio.TestTools.UnitTesting;
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
using AdminPortal.BusinessServices;
using AdminPortal.Models;
using AdminPortal.UnitTests;
using AdminPortal.UnitTests.TestUtilities;
using FluentAssertions;
using NSubstitute;

namespace AdminPortal.Controllers.Tests
{
    [TestClass()]
    public class HomeControllerTests
    {
        const string ConfigFolder = "\\BusinessServices\\config\\";
        private readonly string _filepath = AssemblyHelper.GetExecutingAssemblyDirectoryPath() + ConfigFolder;
        private readonly NLog.ILogger _logger = Substitute.For<NLog.ILogger>();

        [TestMethod()]
        public void IndexTest()
        {
            //Arrange
            var file = _filepath + "UILinksMapping_2Tabs.xml";
            var regionsfile = _filepath + "RegionIndicatorList.xml";
           
            LandingPageLayoutLoader landingPageLayout = new LandingPageLayoutLoader(file, _logger, regionsfile);
            var httpContext = Substitute.For<HttpContextBase>();
            httpContext.User = PrincipalStubBuilder.GetLoggedInUser();

            //Act
            var controller = new HomeController(landingPageLayout);
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