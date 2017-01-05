using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Caching;
using AdminPortal.BusinessServices;
using AdminPortal.BusinessServices.LandingPage;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using NSubstitute;

namespace AdminPortal.UnitTests.BusinessServices
{
    [TestClass()]
    public class LandingPageLayoutLoaderTests
    {
        const string ConfigFolder = "\\config\\";
        private string _filepath = TestHelper.GetExecutingAssembly() + ConfigFolder;
        private readonly NLog.ILogger _logger = Substitute.For<NLog.ILogger>();

        [TestMethod()]
        public void LandingPageLayoutLoaderTest()
        {
            //Arrange
            //TODO: Embedded Resource and read xml and pass XML doc to LandingPageLayoutLoader().
            _filepath += "UILinksMapping.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(_filepath, _logger);

            //Act
           
            UiLinks uiLinks = landingPage.GetParsedXmlToObject();
            
            //Assert
            ValidateLandingPageTabsSectionsMenuItems(uiLinks.LandingPageTab);
        }

        [TestMethod()]
        public void LandingPageLayoutLoader_ParseRegionIndicatorListXML_RegionIndicatorListObject()
        {
            //Arrange
            //TODO: Embedded Resource and read xml and pass XML doc to LandingPageLayoutLoader().
            _filepath += "RegionIndicatorList.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(null, _logger,_filepath);

            //Act
            RegionIndicatorList regionIndicator = landingPage.GetParsedRegionIndicatorXmlToObject();

            //Assert
            regionIndicator.Should().NotBeNull();
        }

        [TestMethod()]
        public void LandingPageLayoutLoader_RootNodeNull_NullLandingPageTabs()
        {
            //Arrange
            //TODO: Embedded Resource and read xml and pass XML doc to LandingPageLayoutLoader().
            _filepath += "UILinksMapping_RootNodeNull.xml";
             LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(_filepath, _logger);
            
            //Act
            UiLinks uiLinks = landingPage.GetParsedXmlToObject();

            //Assert
            uiLinks.Should().BeNull();
        }

        [TestMethod()]
        public void LandingPageLayoutLoader_HostApplicationFilePath_LandingPageTabs()
        {
            //Arrange
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(null, _logger);
            
            //Act
            UiLinks uiLinks = landingPage.GetParsedXmlToObject();

            //Assert
            uiLinks.Should().NotBeNull();
            uiLinks.LandingPageTab.Should().NotBeNull();
        }

        [TestMethod()]
        public void LandingPageLayoutLoader_InCorrectFilePath_ExceptionLogAndNullLandingPageTabs()
        {
            //Arrange
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(_filepath, _logger);
            
            //Act
            UiLinks uiLinks = landingPage.GetParsedXmlToObject();

            //Assert
            uiLinks.Should().BeNull();
        }

        
        [TestMethod()]
        public void LandingPageLayoutLoader_TabNodeNull_NullLandingPageTabs()
        {
            //Arrange
            _filepath += "UILinksMapping_TabNodeNull.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(_filepath, _logger);

            //Act
            UiLinks uiLinks = landingPage.GetParsedXmlToObject();
            
            //Assert
            uiLinks.Should().BeNull();
        }

        [TestMethod()]
        public void LandingPageLayoutLoader_TabWith0SectionItem_NullLandingPageTabs()
        {
            //Arrange
            _filepath += "UILinksMapping_ZeroSectionItem.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(_filepath, _logger);

            //Act
            UiLinks uiLinks = landingPage.GetParsedXmlToObject();

            //Assert
            uiLinks.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Key.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Text.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Section.Should().BeNull();
        }

        [TestMethod()]
        public void LandingPageLayoutLoader_TabSectionsWith0MenuItem_NullLandingPageTabs()
        {
            //Arrange
            _filepath += "UILinksMapping_ZeroMenuItem.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(_filepath, _logger);

            //Act
            UiLinks uiLinks = landingPage.GetParsedXmlToObject();

            //Assert
            uiLinks.Should().NotBeNull();
             uiLinks.LandingPageTab[0].Section.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Section[0].MenuItem.Should().BeNull();
        }

        [TestMethod()]
        public void LandingPageLayoutLoader_TabSectionsWith0Attribute_NullLandingPageTabs()
        {
            //Arrange
            _filepath += "UILinksMapping_ZeroTabAttributes.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(_filepath, _logger);

            //Act
            UiLinks uiLinks = landingPage.GetParsedXmlToObject();

            //Assert
            uiLinks.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Key.Should().BeNull();
            uiLinks.LandingPageTab[0].Text.Should().BeNull();
            uiLinks.LandingPageTab[0].Section.Should().NotBeNull();
        }

       

        [TestMethod()]
        public void LandingPageLayoutLoader_MenuItemNullAttributes_NullLandingPageTabs()
        {
            //Arrange
            _filepath += "UILinksMapping_MenuItemNullAttributes.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(_filepath, _logger);

            //Act
            UiLinks uiLinks = landingPage.GetParsedXmlToObject();

            //Assert
            uiLinks.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Section.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Section[0].MenuItem.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Section[0].MenuItem[0].Key.Should().BeNull();
            uiLinks.LandingPageTab[0].Section[0].MenuItem[0].Text.Should().BeNull();

        }

        [TestMethod()]
        public void LandingPageLayoutLoader_TabNullAttributes_NullLandingPageTabs()
        {
            //Arrange
            _filepath += "UILinksMapping_TabNullAttributes.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(_filepath, _logger);

            //Act
            UiLinks uiLinks = landingPage.GetParsedXmlToObject();

            //Assert
            uiLinks.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Section.Should().NotBeNull();
            uiLinks.LandingPageTab[0].Section[0].MenuItem.Should().NotBeNull();
        }

        private void ValidateLandingPageTabsSectionsMenuItems(UiLinksLandingPageTab[]  tabs)
        {
            //Assert
            tabs.Should().NotBeNull();
            tabs.Length.Should().Be(2);
            tabs[0].Key.Should().Be("WebjetAU");
            tabs[1].Key.Should().Be("WebjetNZ");

            tabs[0].Section.Length.ShouldBeEquivalentTo(3);
            tabs[1].Section.Length.ShouldBeEquivalentTo(3);

            tabs[0].Section[0].Key.Should().Be("ServiceCenterSectionAU");
            tabs[0].Section[0].MenuItem.Length.ShouldBeEquivalentTo(2);
            tabs[0].Section[1].Key.Should().Be("ProductTeamSectionAU");
            tabs[0].Section[1].MenuItem.Length.ShouldBeEquivalentTo(1);
            tabs[0].Section[2].Key.Should().Be("FinanceTeamSectionAU");
            tabs[0].Section[2].MenuItem.Length.ShouldBeEquivalentTo(1);
            tabs[1].Section[0].Key.Should().Be("ServiceCenterSectionNZ");
            tabs[1].Section[0].MenuItem.Length.ShouldBeEquivalentTo(2);
            tabs[1].Section[1].Key.Should().Be("ProductTeamSectionNZ");
            tabs[1].Section[1].MenuItem.Length.ShouldBeEquivalentTo(2);
            tabs[1].Section[2].Key.Should().Be("FinanceTeamSectionNZ");
            tabs[1].Section[2].MenuItem.Length.ShouldBeEquivalentTo(1);

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
    }
}