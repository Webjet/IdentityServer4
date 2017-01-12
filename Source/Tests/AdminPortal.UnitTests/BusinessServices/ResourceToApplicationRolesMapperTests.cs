using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using AdminPortal.BusinessServices;
using AdminPortal.UnitTests.TestUtilities;
using FluentAssertions;
using NSubstitute;

namespace AdminPortal.UnitTests.BusinessServices
{
    [TestClass()]
    public class ResourceToApplicationRolesMapperTests
    {
        //Arrange
        //TODO: Embedded Resource and read xml and pass XML doc to LandingPageLayoutLoader().
        const string ConfigFolder = "\\BusinessServices\\config\\";
        private readonly NLog.ILogger _logger = Substitute.For<NLog.ILogger>();
        private readonly string _filepath = AssemblyHelper.GetExecutingAssemblyDirectoryPath() + ConfigFolder;

        [TestMethod()]
        public void ResourceToApplicationRolesMapper_ResourceItemsWithRolesDictionary_NotNull()
        {
            var file= _filepath + "ResourceToRolesMapSample.xml";

            //Act
            ResourceToApplicationRolesMapper mapper = new ResourceToApplicationRolesMapper(file, _logger);

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
            ResourceToApplicationRolesMapper mapper = new ResourceToApplicationRolesMapper(file, _logger);
            List<string> roles = mapper.GetAllowedRolesForResource("GoogleBigQueryItinerary");

            //Assert
            mapper.ResourceItemsWithRoles.Should().NotBeNull();
            mapper.ResourceItemsWithRoles.ContainsKey("GoogleBigQueryItinerary").Should().BeTrue();
            roles.Count.Should().Be(2);
            roles[0].ShouldBeEquivalentTo("ServiceCenter");
            roles[1].ShouldBeEquivalentTo("AnalyticsTeam");
        }

        [TestMethod()]
        public void ResourceToApplicationRolesMapper_RootNodeNull_NullResourceItemsWithRolesCollection()
        {
            //Arrange
            var file= _filepath + "ResourceToRolesMapSample_RootNodeNull.xml";

            //Act
            ResourceToApplicationRolesMapper resourceToApplicationRolesMapper = new ResourceToApplicationRolesMapper(file, _logger);

            //Assert
            resourceToApplicationRolesMapper.ResourceItemsWithRoles.Should().BeNull();
        }

        [TestMethod()]
        public void ResourceToApplicationRolesMapper_NullAttribute_ResourceItemsWithRolesWithCount0()
        {
            //Arrange
            var file= _filepath + "ResourceToRolesMapSample_NullAttribute.xml";

            //Act
            ResourceToApplicationRolesMapper resourceToApplicationRolesMapper = new ResourceToApplicationRolesMapper(file, _logger);

            //Assert
            resourceToApplicationRolesMapper.ResourceItemsWithRoles.Count.Should().Be(0);
        }

        [TestMethod()]
        public void ResourceToApplicationRolesMapper_IncorrectXML_NullResourceItemsWithRoles()
        {
            //Arrange
            var file= _filepath + "ResourceToRolesMapSample_IncorrectFormat.xml";

            //Act
            ResourceToApplicationRolesMapper resourceToApplicationRolesMapper = new ResourceToApplicationRolesMapper(file, _logger);

            //Assert
            resourceToApplicationRolesMapper.ResourceItemsWithRoles.Should().BeNull();
        }

        [TestMethod()]
        public void ResourceToApplicationRolesMapper_HostApplicationFilePath_Test()
        {
            //Act
            ResourceToApplicationRolesMapper resourceToApplicationRolesMapper = new ResourceToApplicationRolesMapper(null, _logger);

            //Assert
            resourceToApplicationRolesMapper.ResourceItemsWithRoles.Should().NotBeNull();
        }

     

        [TestMethod()]
        public void GetAllowedRolesForResource_NullResourceItemWithRoles_ReturnsNull()
        {
            //Act
            var file= _filepath + "ResourceToRolesMapSample_RootNodeNull.xml";
            ResourceToApplicationRolesMapper mapper = new ResourceToApplicationRolesMapper(file, _logger);

            //Assert
            mapper.ResourceItemsWithRoles.Should().BeNull();
            
        }
        

        [TestMethod()]
        public void IsUserRoleAllowedForResource_Allowed()
        {
            //Arrange
            var file= _filepath + "ResourceToRolesMapSample.xml";
            IPrincipal loggedInUser = PrincipalStubBuilder.GetLoggedInUser();

            //Act
            ResourceToApplicationRolesMapper mapper = new ResourceToApplicationRolesMapper(file, _logger);

            mapper.ResourceItemsWithRoles.Should().NotBeNull();
            ServiceCenterUser_GoogleBigQueryItinerary_True(loggedInUser, "GoogleBigQueryItinerary", mapper);
            FinanceTeamUser_FareEscalationJournalAU_True(loggedInUser, "FareEscalationJournal_WebjetAU", mapper);

        }

        [TestMethod()]
        public void IsUserRoleAllowedForResource_NotAllowed()
        {
            //Arrange
            var file= _filepath + "ResourceToRolesMapSample.xml";
            IPrincipal loggedInUser = PrincipalStubBuilder.GetUserFromAnalyticsAndFinanceRole();

            //Act
            ResourceToApplicationRolesMapper mapper = new ResourceToApplicationRolesMapper(file, _logger);

            AnalyticsTeamUser_ReviewPendingBookingsNZ_False(loggedInUser, "ReviewPendingBookings_WebjetNZ", mapper);
            FinanceTeamUser_CreditCardTransactionsToCheckNZ_False(loggedInUser, "CreditCardTransactionsToCheck_WebjetNZ", mapper);
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