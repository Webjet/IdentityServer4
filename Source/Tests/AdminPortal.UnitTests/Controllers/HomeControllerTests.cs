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
using AdminPortal.BusinessServices.LandingPage;
using AdminPortal.Models;
using AdminPortal.UnitTests;
using AdminPortal.UnitTests.BusinessServices;
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
            var controller = InitHomeController(landingPageLayout);
            //Act

            var result = controller.Index() as ViewResult;

            //Assert
            result.Model.Should().NotBeNull();
            var uiLinksLandingPageTabs = ((LandingPageModel)result.Model).LandingPageTabs.ToArray();
            LandingPageLayoutLoaderTests.AssertUILinksMapping2Tabs(uiLinksLandingPageTabs);
        }

        private static HomeController InitHomeController(LandingPageLayoutLoader landingPageLayout)
        {
            var httpContext = Substitute.For<HttpContextBase>();
            httpContext.User = PrincipalStubBuilder.GetUserWithServiceCenterAnalyticsAndFinanceRoles();

            var controller = new HomeController(landingPageLayout);
            controller.ControllerContext = new ControllerContext()
            {
                Controller = (HomeController) controller,
                RequestContext = new RequestContext(httpContext, new RouteData())
            };
            return controller;
        }

        [TestMethod()]
        public void Index_RegionIndicatorListNull()
        {
            //Arrange
            var file = _filepath + "UILinksMapping_2Tabs.xml";
            var regionsfile = _filepath ;//wrong path
            LandingPageLayoutLoader landingPageLayout = new LandingPageLayoutLoader(file, _logger, regionsfile);

            var controller = InitHomeController(landingPageLayout);
            //Act

            var result = controller.Index() as ViewResult;

            //Assert
            result.Model.Should().NotBeNull();
            var uiLinksLandingPageTabs = ((LandingPageModel)result.Model).LandingPageTabs.ToArray();
            LandingPageLayoutLoaderTests.AssertUILinksMapping2Tabs(uiLinksLandingPageTabs);

            var allMenus = from t in uiLinksLandingPageTabs.SelectMany(t => t.Section).SelectMany(s=>s.MenuItem)
                           select t;
            var regionIndicators = allMenus.Select(m=>m.RegionIndicator).Distinct();
            regionIndicators.Count().Should().Be(2);//null & "all"
            regionIndicators.Should().Contain((string)null);
           regionIndicators.Should().Contain("ALL");
            regionIndicators.Should().NotContain("WAU");
        }
    }
}