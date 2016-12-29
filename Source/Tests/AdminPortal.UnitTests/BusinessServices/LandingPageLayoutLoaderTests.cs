using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AdminPortal.BusinessServices;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdminPortal.UnitTests.BusinessServices
{
    [TestClass()]
    public class LandingPageLayoutLoaderTests
    {

        [TestMethod()]
        public void LandingPageLayoutLoaderTest()
        {
            //Arrange
            //TODO: Embedded Resource and read xml and pass XML doc to LandingPageLayoutLoader().
            string filepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\config\\UILinksMapping.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(filepath);

            //Act
            List<LandingPageTab> tabs = landingPage.GetConfiguration();

            //Assert
            ValidateLandingPageTabsSectionsMenuItems(tabs);
        }
        
        [TestMethod()]
        public void LandingPageLayoutLoader_TabWith0SectionItem_NullLandingPageTabs()
        {
            //Arrange
            string filepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\config\\UILinksMapping_ZeroSectionItem.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(filepath);

            //Act
            List<LandingPageTab> tabs = landingPage.GetConfiguration();

            //Assert
            tabs.Should().BeNull();
        }

        [TestMethod()]
        public void LandingPageLayoutLoader_TabSectionsWith0MenuItem_NullLandingPageTabs()
        {
            //Arrange
            string filepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\config\\UILinksMapping_ZeroMenuItem.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(filepath);

            //Act
            List<LandingPageTab> tabs = landingPage.GetConfiguration();

            //Assert
            tabs.Should().BeNull();
        }

        [TestMethod()]
        public void LandingPageLayoutLoader_TabSectionsWith0Attribute_NullLandingPageTabs()
        {
            //Arrange
            string filepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\config\\UILinksMapping_ZeroTabAttributes.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(filepath);

            //Act
            List<LandingPageTab> tabs = landingPage.GetConfiguration();

            //Assert
            tabs.Should().BeNull();
        }

        [TestMethod()]
        //[ExpectedException(typeof(Exception))]
        public void LandingPageLayoutLoader_TabWAUNullAttribute_WNZLandingPageTabs()
        {
            //Arrange
            string filepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\config\\UILinksMapping_TabWAUNullAttributes.xml";
            LandingPageLayoutLoader landingPage = new LandingPageLayoutLoader(filepath);

            //Act
            List<LandingPageTab> tabs = landingPage.GetConfiguration();

            //Assert
            tabs.Should().NotBeNull();
            tabs.Count.Should().Be(1);
           
            tabs[0].Key.Should().Be("WebjetNZ");
            tabs[0].Sections.Count.ShouldBeEquivalentTo(3);
            tabs[0].Sections[0].Key.Should().Be("ServiceCenterSectionNZ");
            tabs[0].Sections[0].MenuItems.Count.ShouldBeEquivalentTo(2);
            tabs[0].Sections[1].Key.Should().Be("ProductTeamSectionNZ");
            tabs[0].Sections[1].MenuItems.Count.ShouldBeEquivalentTo(2);
            tabs[0].Sections[2].Key.Should().Be("FinanceTeamSectionNZ");
            tabs[0].Sections[2].MenuItems.Count.ShouldBeEquivalentTo(1);
        }
        
        private void ValidateLandingPageTabsSectionsMenuItems(List<LandingPageTab> tabs)
        {
            //Assert
            tabs.Should().NotBeNull();
            tabs.Count.Should().Be(2);
            tabs[0].Key.Should().Be("WebjetAU");
            tabs[1].Key.Should().Be("WebjetNZ");

            tabs[0].Sections.Count.ShouldBeEquivalentTo(3);
            tabs[1].Sections.Count.ShouldBeEquivalentTo(3);

            tabs[0].Sections[0].Key.Should().Be("ServiceCenterSectionAU");
            tabs[0].Sections[0].MenuItems.Count.ShouldBeEquivalentTo(2);
            tabs[0].Sections[1].Key.Should().Be("ProductTeamSectionAU");
            tabs[0].Sections[1].MenuItems.Count.ShouldBeEquivalentTo(1);
            tabs[0].Sections[2].Key.Should().Be("FinanceTeamSectionAU");
            tabs[0].Sections[2].MenuItems.Count.ShouldBeEquivalentTo(1);
            tabs[1].Sections[0].Key.Should().Be("ServiceCenterSectionNZ");
            tabs[1].Sections[0].MenuItems.Count.ShouldBeEquivalentTo(2);
            tabs[1].Sections[1].Key.Should().Be("ProductTeamSectionNZ");
            tabs[1].Sections[1].MenuItems.Count.ShouldBeEquivalentTo(2);
            tabs[1].Sections[2].Key.Should().Be("FinanceTeamSectionNZ");
            tabs[1].Sections[2].MenuItems.Count.ShouldBeEquivalentTo(1);

            tabs[0].Sections[0].MenuItems[0].Key.Should().Be("ReviewPendingBookingsAU");
            tabs[0].Sections[0].MenuItems[1].Key.Should().Be("GoogleBigQueryItinerary");
            tabs[0].Sections[1].MenuItems[0].Key.Should().Be("CreditCardTransactionsToCheckAU");
            tabs[0].Sections[2].MenuItems[0].Key.Should().Be("FareEscalationJournalAU");
            tabs[1].Sections[0].MenuItems[0].Key.Should().Be("ReviewPendingBookingsNZ");
            tabs[1].Sections[0].MenuItems[1].Key.Should().Be("GoogleBigQueryItinerary");
            tabs[1].Sections[1].MenuItems[0].Key.Should().Be("ReviewPendingBookingsNZ");
            tabs[1].Sections[1].MenuItems[1].Key.Should().Be("CreditCardTransactionsToCheckNZ");
            tabs[1].Sections[2].MenuItems[0].Key.Should().Be("FareEscalationJournalNZ");
        }
    }
}