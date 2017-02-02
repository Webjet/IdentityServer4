using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Web.Caching;
using AdminPortal.BusinessServices;
using AdminPortal.BusinessServices.LandingPage;
using AdminPortal.UnitTests.TestUtilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using NSubstitute;

namespace AdminPortal.UnitTests.BusinessServices
{
    [TestClass()]
    public class LandingPageLayoutLoaderTests
    {
        const string ConfigFolder = "\\BusinessServices\\config\\";
        private readonly string _filepath = AssemblyHelper.GetExecutingAssemblyDirectoryPath() + ConfigFolder;
        private readonly NLog.ILogger _logger = Substitute.For<NLog.ILogger>();

        [TestMethod()]
        public void GetUiLinks_2TabsExample_MatchExpected()
        {
            //Arrange
            //TODO: Embedded Resource and read xml and pass XML doc to LandingPageLayoutLoader().
            var file = _filepath + "UILinksMapping_2Tabs.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(file, _logger);

            //Act
            UiLinks uiLinks = landingPage.GetUiLinks();
            UiLinksLandingPageTab[] tabs = uiLinks.LandingPageTab;

            //Assert
            AssertUILinksMapping2Tabs(tabs);
        }

        public static void AssertUILinksMapping2Tabs(UiLinksLandingPageTab[] tabs)
        {
            tabs.Should().NotBeNull();
            tabs.Length.Should().Be(2);
            tabs[0].Key.Should().Be("WebjetAU");
            tabs[1].Key.Should().Be("WebjetNZ");

            tabs[0].Section.Length.Should().Be(3);
            tabs[1].Section.Length.Should().Be(3);

            tabs[0].Section[0].Key.Should().Be("ServiceCenterSectionAU");
            tabs[0].Section[0].MenuItem.Length.Should().Be(2);
            tabs[0].Section[1].Key.Should().Be("ProductTeamSectionAU");
            tabs[0].Section[1].MenuItem.Length.Should().Be(1);
            tabs[0].Section[2].Key.Should().Be("FinanceTeamSectionAU");
            tabs[0].Section[2].MenuItem.Length.Should().Be(1);
            tabs[1].Section[0].Key.Should().Be("ServiceCenterSectionNZ");
            tabs[1].Section[0].MenuItem.Length.Should().Be(2);
            tabs[1].Section[1].Key.Should().Be("ProductTeamSectionNZ");
            tabs[1].Section[1].MenuItem.Length.Should().Be(2);
            tabs[1].Section[2].Key.Should().Be("FinanceTeamSectionNZ");
            tabs[1].Section[2].MenuItem.Length.Should().Be(1);

            tabs[0].Section[0].MenuItem[0].Key.Should().Be("ReviewPendingBookings_WebjetAU");
            tabs[0].Section[0].MenuItem[1].Key.Should().Be("GoogleBigQueryItinerary");
            tabs[0].Section[1].MenuItem[0].Key.Should().Be("CreditCardTransactionsToCheck_WebjetAU");
            tabs[0].Section[2].MenuItem[0].Key.Should().Be("FareEscalationJournal_WebjetAU");
            tabs[1].Section[0].MenuItem[0].Key.Should().Be("ReviewPendingBookings_WebjetNZ");
            tabs[1].Section[0].MenuItem[1].Key.Should().Be("GoogleBigQueryItinerary");
            tabs[1].Section[1].MenuItem[0].Key.Should().Be("ReviewPendingBookings_WebjetNZ");
            tabs[1].Section[1].MenuItem[1].Key.Should().Be("CreditCardTransactionsToCheck_WebjetNZ");
            tabs[1].Section[2].MenuItem[0].Key.Should().Be("FareEscalationJournal_WebjetNZ");
        }

        [TestMethod()]
        public void GetRegionIndicators_RegionIndicatorListObject()
        {
            //Arrange
            //TODO: Embedded Resource and read xml and pass XML doc to LandingPageLayoutLoader().
            var regionsfile = _filepath + "RegionIndicatorList.xml";
            var landingPageLayoutFile = _filepath + "UILinksMapping_2Tabs.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(landingPageLayoutFile, _logger, regionsfile);

            //Act
            RegionIndicatorList regionIndicator = landingPage.GetRegionIndicators();

            //Assert
            regionIndicator.Should().NotBeNull();
            regionIndicator.RegionIndicator.Length.Should().Be(4);
            regionIndicator.RegionIndicator[0].ShortDescription.Should().Be("WAU");
            regionIndicator.RegionIndicator[1].ShortDescription.Should().Be("WNZ");
            regionIndicator.RegionIndicator[2].ShortDescription.Should().Be("ZAU");
            regionIndicator.RegionIndicator[3].ShortDescription.Should().Be("ALL");

            regionIndicator.RegionIndicator[0].Description.Should().Be("Webjet AU");
            regionIndicator.RegionIndicator[1].Description.Should().Be("Webjet NZ");
            regionIndicator.RegionIndicator[2].Description.Should().Be("Zuji AU");
            regionIndicator.RegionIndicator[3].Description.Should().Be("All sites");

            regionIndicator.RegionIndicator[0].Id.Should().Be("WebjetAU");
            regionIndicator.RegionIndicator[1].Id.Should().Be("WebjetNZ");
            regionIndicator.RegionIndicator[2].Id.Should().Be("ZujiAU");
            regionIndicator.RegionIndicator[3].Id.Should().Be("ALL");

        }

        [TestMethod()]
        [Ignore()]//System.InvalidOperationException: There is an error in XML document (3, 2). ---> System.InvalidOperationException: <links xmlns=''> was not expected.
        public void UILinksXMlWithRootNodeNull_NullLandingPageTabs()
        {
            //Arrange
            //TODO: Embedded Resource and read xml and pass XML doc to LandingPageLayoutLoader().
            var file = _filepath + "UILinksMapping_RootNodeNull.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(file, _logger);

            //Act
            UiLinks uiLinks = landingPage.GetUiLinks();

            //Assert
            uiLinks.Should().BeNull();
        }

        [TestMethod()]
        public void GetUiLinks_NullFilePath_LandingPageTabs()
        {
            //Arrange
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(null, _logger);

            Action act = () => landingPage.GetUiLinks();
            //Act and Assert
            //<System.UnauthorizedAccessException>: System.UnauthorizedAccessException with message "Access to the path 'C:\GitRepos\AdminPortal\Source\Tests\AdminPortal.UnitTests\bin\Debug' is denied."
            act.ShouldThrow<System.UnauthorizedAccessException>();
        }

        [TestMethod()]
        public void GetUiLinks_IncorrectFilePath_DirectoryNotFoundExceptionThrown()
        {
            //Arrange
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(_filepath, _logger);

            Action act = () => landingPage.GetUiLinks();

            //Act and Assert
            act.ShouldThrow<System.IO.DirectoryNotFoundException>();
        }


        [TestMethod()]
        [Ignore]//System.InvalidOperationException: There is an error in XML document (3, 2). ---> System.InvalidOperationException: <uilinks xmlns=''> was not expected
        public void UiLinksXMLWithTabNodeNull_NullLandingPageTabs()
        {
            //Arrange
            var file = _filepath + "UILinksMapping_TabNodeNull.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(file, _logger);

            //Act
            UiLinks uiLinks = landingPage.GetUiLinks();

            //Assert
            uiLinks.Should().BeNull();
        }

        [TestMethod()]
        public void UiLinksXMLWithZeroSectionItem_NullLandingPageTabs()
        {
            //Arrange
            var file = _filepath + "UILinksMapping_ZeroSectionItem.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(file, _logger);

            //Act
            UiLinks uiLinks = landingPage.GetUiLinks();

            //Assert
            uiLinks.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Key.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Text.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Section.Should().BeNull();
        }

        [TestMethod()]
        public void UiLinksXMLWithTabSectionsAndZeroMenuItem_NullLandingPageTabs()
        {
            //Arrange
            var file = _filepath + "UILinksMapping_ZeroMenuItem.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(file, _logger);

            //Act
            UiLinks uiLinks = landingPage.GetUiLinks();

            //Assert
            uiLinks.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Section.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Section[0].MenuItem.Should().BeNull();
        }

        [TestMethod()]
        public void UiLinksXMLWithTabSectionsAndZeroAttribute_NullLandingPageTabs()
        {
            //Arrange
            var file = _filepath + "UILinksMapping_ZeroTabAttributes.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(file, _logger);

            //Act
            UiLinks uiLinks = landingPage.GetUiLinks();

            //Assert
            uiLinks.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Key.Should().BeNull();
            uiLinks.LandingPageTab[0].Text.Should().BeNull();
            uiLinks.LandingPageTab[0].Section.Should().NotBeNull();
        }



        [TestMethod()]
        public void UiLinksXMLWithMenuItemNullAttributes_NullLandingPageTabs()
        {
            //Arrange
            var file = _filepath + "UILinksMapping_MenuItemNullAttributes.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(file, _logger);

            //Act
            UiLinks uiLinks = landingPage.GetUiLinks();

            //Assert
            uiLinks.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Section.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Section[0].MenuItem.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Section[0].MenuItem[0].Key.Should().BeNull();
            uiLinks.LandingPageTab[0].Section[0].MenuItem[0].Text.Should().BeNull();

        }

        [TestMethod()]
        public void UiLinksXMLWithTabNullAttributes_NullLandingPageTabs()
        {
            //Arrange
            var file = _filepath + "UILinksMapping_TabNullAttributes.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(file, _logger);

            //Act
            UiLinks uiLinks = landingPage.GetUiLinks();

            //Assert
            uiLinks.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Section.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Section[0].MenuItem.Should().NotBeNull();
        }
    }
}