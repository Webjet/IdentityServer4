using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using AdminPortal.BusinessServices;
using AdminPortal.UnitTests.TestUtilities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace AdminPortal.UnitTests.BusinessServices
{
    [TestClass()]
    public class ResourceToApplicationRolesMapperTests
    {
        //Arrange
        //TODO: Embedded Resource and read xml and pass XML doc to LandingPageLayoutLoader().
        //const string ConfigFolder = "\\BusinessServices\\config\\";
        //private readonly string _filepath = AssemblyHelper.GetExecutingAssemblyDirectoryPath() + ConfigFolder;

        const string ConfigFolder = @"BusinessServices\config\";
        //TODO: unable to get the current directory path. Travelling 4 folders up from the executing assembly folder.
        private static readonly string _rootFolder = AssemblyHelper.GetExecutingAssemblyRootPath(); 
        private readonly string _filepath = _rootFolder + ConfigFolder;

        [TestMethod()]
        public void ResourceToApplicationRolesMapper_ResourceItemsWithRolesDictionary_NotNull()
        {
            var file= _filepath + "ResourceToRolesMapSample.xml";

            //Act
            ResourceToApplicationRolesMapper mapper = new ResourceToApplicationRolesMapper(null, file);

            //Assert
            mapper.ResourceItemsWithRoles.Should().NotBeNull();
            mapper.ResourceItemsWithRoles.Count.Should().Be(7);
            mapper.ResourceItemsWithRoles.ContainsKey("GoogleBigQueryItinerary").Should().BeTrue();
            mapper.ResourceItemsWithRoles.ContainsKey("ReviewPendingBookings_WebjetAU").Should().BeTrue();
           
        }

        [TestMethod()]
        public void ResourceToApplicationRolesMapper_GetAllowedRolesForResource_ListOfString()
        {
            var file = _filepath + "ResourceToRolesMapSample.xml";

            //Act
            ResourceToApplicationRolesMapper mapper = new ResourceToApplicationRolesMapper(null, file);
            List<string> roles = mapper.GetAllowedRolesForResource("GoogleBigQueryItinerary");

            //Assert
            mapper.ResourceItemsWithRoles.Should().NotBeNull();
            mapper.ResourceItemsWithRoles.ContainsKey("GoogleBigQueryItinerary").Should().BeTrue();
            roles.Count.Should().Be(2);
            roles[0].ShouldBeEquivalentTo("ServiceCenter");
            roles[1].ShouldBeEquivalentTo("AnalyticsTeam");
        }

        [TestMethod()]
        public void ResourceToApplicationRolesMapper_NoRootNode_Exception()
        {
            //Arrange
            var file= _filepath + "ResourceToRolesMapSample_NoRootNode.xml";

            Action act = () => new ResourceToApplicationRolesMapper(null, file);

            //Act and Assert
            act.ShouldThrow<InvalidOperationException>()
                .Where(e => e.Message.StartsWith("There is an error in XML document")); ;
        }

        [TestMethod()]
        public void ResourceToApplicationRolesMapper_NullResourceId_Exception()
        {
            //Arrange
            var file= _filepath + "ResourceToRolesMapSample_NullResourceId.xml";
 
            Action act = () => new ResourceToApplicationRolesMapper(null, file);

            //Act and Assert
            act.ShouldThrow<NullReferenceException>()
                .Where(e => e.Message.StartsWith("Object reference not set to an instance of an object")); ;
        }

        [TestMethod()]
        public void ResourceToApplicationRolesMapper_IncorrectXML_Exception()
        {
            //Arrange
            var file= _filepath + "ResourceToRolesMapSample_IncorrectFormat.xml";

            Action act = () => new ResourceToApplicationRolesMapper(null, file);

            //Act and Assert
            act.ShouldThrow<InvalidOperationException>()
                .Where(e => e.Message.StartsWith("There is an error in XML document")); ;
        }

        [TestMethod()]
        public void ResourceToApplicationRolesMapper_NullFilePath_Test()
        {
            //Arrange
            var section = Substitute.For<IConfigurationSection>();

            var config = Substitute.For<IConfigurationRoot>();
            config.GetSection(Arg.Any<string>()).Returns(section);
            
            //Act and Assert
            Action act = () => new ResourceToApplicationRolesMapper(config, null);
            //<System.UnauthorizedAccessException>: System.UnauthorizedAccessException with message "Access to the path 'C:\GitRepos\AdminPortal\Source\Tests\AdminPortal.UnitTests\bin\Debug' is denied."
            act.ShouldThrow<System.IO.DirectoryNotFoundException>();
        }


        [TestMethod()]
        public void IsUserRoleAllowedForResource_Allowed()
        {
            //Arrange
            var file= _filepath + "ResourceToRolesMapSample.xml";
            IPrincipal loggedInUser = PrincipalStubBuilder.GetUserWithServiceCenterAnalyticsAndFinanceRoles();

            //Act
            ResourceToApplicationRolesMapper mapper = new ResourceToApplicationRolesMapper(null,file);

            mapper.ResourceItemsWithRoles.Should().NotBeNull();
            ServiceCenterUser_GoogleBigQueryItinerary_True(loggedInUser, "GoogleBigQueryItinerary", mapper);
            FinanceTeamUser_FareEscalationJournalAU_True(loggedInUser, "FareEscalationJournal_WebjetAU", mapper);

        }

        [TestMethod()]
        public void IsUserRoleAllowedForResource_NotAllowed()
        {
            //Arrange
            var file= _filepath + "ResourceToRolesMapSample.xml";
            IPrincipal loggedInUser = PrincipalStubBuilder.GetUserWithAnalyticsAndFinanceRoles();

            //Act
            ResourceToApplicationRolesMapper mapper = new ResourceToApplicationRolesMapper(null,file);

            AnalyticsTeamUser_ReviewPendingBookingsNZ_False(loggedInUser, "ReviewPendingBookings_WebjetNZ", mapper);
            FinanceTeamUser_CreditCardTransactionsToCheckNZ_False(loggedInUser, "CreditCardTransactionsToCheck_WebjetNZ", mapper);
        }
        [TestMethod()]
        public void IsUserRoleAllowedForResource_DiffferentCase_Allowed()
        {
            //Arrange
            var file = _filepath + "ResourceToRolesMapSample.xml";
            IPrincipal loggedInUser = PrincipalStubBuilder.GetUserWithServiceCenterAnalyticsAndFinanceRoles();
            var resourceKey = "ReviewPendingBookings_WEBJETnz";
            ResourceToApplicationRolesMapper mapper = new ResourceToApplicationRolesMapper(null, file);
            //Act
            bool result = mapper.IsUserRoleAllowedForResource(resourceKey, loggedInUser);
            //Assert
            result.Should().BeTrue();
        }

        [TestMethod()]
        public void GetAllowedForUserResources_Found()
        {
            //Arrange
            var file = _filepath + "ResourceToRolesMapSample.xml";
            IPrincipal user = PrincipalStubBuilder.GetUserWithAnalyticsAndFinanceRoles();
            ResourceToApplicationRolesMapper mapper = new ResourceToApplicationRolesMapper(null, file);
            //Act
            var resources = mapper.GetAllowedForUserResources(user);
            //Assert
            resources.Count.Should().Be(3);
            var expected=new List<String>() {"GoogleBigQueryItinerary","FareEscalationJournal_WebjetAU","FareEscalationJournal_WebjetNZ"};
            resources.Union(expected).Count().Should().Be(3);
        }

        [TestMethod()]
        public void IsUserRoleAllowedForResource_NotFound()
        {
            //Arrange
            var file = _filepath + "ResourceToRolesMapSample.xml";
            IPrincipal user = PrincipalStubBuilder.GetUserWithDevRole();
            ResourceToApplicationRolesMapper mapper = new ResourceToApplicationRolesMapper(null,file);
            //Act
            var resources = mapper.GetAllowedForUserResources(user);
            //Assert
            resources.Count.Should().Be(0);
        }
        private void ServiceCenterUser_GoogleBigQueryItinerary_True(IPrincipal loggedInUser, string resourceKey, ResourceToApplicationRolesMapper mapper)
        {
            //Act
            bool result = mapper.IsUserRoleAllowedForResource(resourceKey, loggedInUser);

            //Assert
            mapper.ResourceItemsWithRoles.ContainsKey(resourceKey).Should().BeTrue();
            loggedInUser.Should().NotBeNull();
            result.Should().BeTrue();
        }

        private void AnalyticsTeamUser_ReviewPendingBookingsNZ_False(IPrincipal loggedInUser, string resourceKey, ResourceToApplicationRolesMapper mapper)
        {
            //Act
            bool result = mapper.IsUserRoleAllowedForResource(resourceKey, loggedInUser);

            //Assert
            mapper.ResourceItemsWithRoles.ContainsKey(resourceKey).Should().BeTrue();
            loggedInUser.Should().NotBeNull();
            result.Should().BeFalse();
        }
        private void FinanceTeamUser_FareEscalationJournalAU_True(IPrincipal loggedInUser, string resourceKey, ResourceToApplicationRolesMapper mapper)
        {
            //Act
            bool result = mapper.IsUserRoleAllowedForResource(resourceKey, loggedInUser);

            //Assert
            mapper.ResourceItemsWithRoles.ContainsKey(resourceKey).Should().BeTrue();
            loggedInUser.Should().NotBeNull();
            result.Should().BeTrue();
        }
        private void FinanceTeamUser_CreditCardTransactionsToCheckNZ_False(IPrincipal loggedInUser, string resourceKey, ResourceToApplicationRolesMapper mapper)
        {
            //Act
            bool result = mapper.IsUserRoleAllowedForResource(resourceKey, loggedInUser);

            //Assert
            mapper.ResourceItemsWithRoles.ContainsKey(resourceKey).Should().BeTrue();
            loggedInUser.Should().NotBeNull();
            result.Should().BeFalse();
        }
        

    }
}