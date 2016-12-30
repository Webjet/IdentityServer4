﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using AdminPortal.BusinessServices;
using FluentAssertions;
using NSubstitute;

namespace AdminPortal.UnitTests.BusinessServices
{
    [TestClass()]
    public class ResourceToApplicationRolesMapperTests
    {
        //Arrange
        //TODO: Embedded Resource and read xml and pass XML doc to LandingPageLayoutLoader().
        const string ConfigFolder = "\\config\\";
        private NLog.ILogger _nlogger = Substitute.For<NLog.ILogger>();
        private  string _filepath = TestHelper.GetExecutingAssembly() + ConfigFolder;
        
        [TestMethod()]
        public void ResourceToApplicationRolesMapperTest()
        {
            _filepath += "RoleBasedMenuItemMap.xml";

            //Act
            ResourceToApplicationRolesMapper resourceToApplicationRolesMapper = new ResourceToApplicationRolesMapper(_filepath, _nlogger);

            //Assert
            resourceToApplicationRolesMapper.ResourceItemsWithRoles.Count.ShouldBeEquivalentTo(7);
            resourceToApplicationRolesMapper.ResourceItemsWithRoles.ContainsKey("GoogleBigQueryItinerary").Should().BeTrue();
            resourceToApplicationRolesMapper.ResourceItemsWithRoles.ContainsKey("ReviewPendingBookingsAU").Should().BeTrue();
            resourceToApplicationRolesMapper.ResourceItemsWithRoles.ContainsKey("FareEscalationJournalAU").Should().BeTrue();
            resourceToApplicationRolesMapper.ResourceItemsWithRoles.ContainsKey("CreditCardTransactionsToCheckAU").Should().BeTrue();
            resourceToApplicationRolesMapper.ResourceItemsWithRoles.ContainsKey("ReviewPendingBookingsNZ").Should().BeTrue();
            resourceToApplicationRolesMapper.ResourceItemsWithRoles.ContainsKey("FareEscalationJournalNZ").Should().BeTrue();
            resourceToApplicationRolesMapper.ResourceItemsWithRoles.ContainsKey("CreditCardTransactionsToCheckNZ").Should().BeTrue();
        }
        
        [TestMethod()]
        public void ResourceToApplicationRolesMapper_RootNodeNull_NullResourceItemsWithRolesCollection()
        {
            //Arrange
            _filepath += "RoleBasedMenuItemMap_RootNodeNull.xml";

            //Act
            ResourceToApplicationRolesMapper resourceToApplicationRolesMapper = new ResourceToApplicationRolesMapper(_filepath, _nlogger);

            //Assert
            resourceToApplicationRolesMapper.ResourceItemsWithRoles.Should().BeNull();
        }
        
        [TestMethod()]
        public void ResourceToApplicationRolesMapper_NullAttribute_ResourceItemsWithRolesWithCount0()
        {
            //Arrange
            _filepath +="RoleBasedMenuItemMap_NullAttribute.xml";

            //Act
            ResourceToApplicationRolesMapper resourceToApplicationRolesMapper = new ResourceToApplicationRolesMapper(_filepath, _nlogger);

            //Assert
            resourceToApplicationRolesMapper.ResourceItemsWithRoles.Count.Should().Be(0);
        }

        [TestMethod()]
        public void ResourceToApplicationRolesMapper_HostApplicationFilePath_Test()
        {
            //Act
            ResourceToApplicationRolesMapper resourceToApplicationRolesMapper = new ResourceToApplicationRolesMapper(null, _nlogger);

            //Assert
            resourceToApplicationRolesMapper.ResourceItemsWithRoles.Should().NotBeNull();
        }
        
        [TestMethod()]
        public void GetAllowedRolesForResourceTest()
        {
            //Act
            _filepath += "RoleBasedMenuItemMap.xml";
            ResourceToApplicationRolesMapper mapper = new ResourceToApplicationRolesMapper(_filepath, _nlogger);

            //Assert
            mapper.ResourceItemsWithRoles.Should().NotBeNull();
            mapper.ResourceItemsWithRoles.Count.Should().Be(7);
            GoogleBigQueryItineraryAU_ServiceCenterAndAnalytics(mapper);
            GoogleBigQueryItineraryNZ_ServiceCenterAndAnalytics(mapper);
            ReviewPendingBookingsAU_ServiceCenterAndDevSupport(mapper);
            ReviewPendingBookingsNZ_ServiceCenterAndDevSupport(mapper);
            CreditCardTransactionsToCheckAU_ServiceCenterAndDevSupportAndProductTeam(mapper);
            CreditCardTransactionsToCheckNZ_ServiceCenterAndDevSupportAndProductTeam(mapper);
            FareEscalationJournalAU_FinanceTeam(mapper);
            FareEscalationJournalNZ_FinanceTeamAndProductTeam(mapper);

        }

        [TestMethod()]
        public void AllowedRolesForResourceTest()
        {
            _filepath += "RoleBasedMenuItemMap.xml";

            //Act
            ResourceToApplicationRolesMapper mapper = new ResourceToApplicationRolesMapper(_filepath, _nlogger);

            //Act
            string roles = mapper.AllowedRolesForResource("GoogleBigQueryItinerary");

            //Assert
            mapper.ResourceItemsWithRoles.Should().NotBeNull();
            mapper.ResourceItemsWithRoles.Count.Should().Be(7);
            roles.Should().NotBeEmpty();
            roles.Contains("ServiceCenter").Should().BeTrue();
            roles.Contains("AnalyticsTeam").Should().BeTrue();
        }

        [TestMethod()]
        public void IsUserRoleAllowedForResource_Allowed()
        {
            //Arrange
            _filepath += "RoleBasedMenuItemMap.xml";
            String[] loggedInUserRole = { "ServiceCenter", "AnalyticsTeam" , "FinanceTeam" };
            IPrincipal loggedInUser = new GenericPrincipal(new GenericIdentity("LoggedInUser"), loggedInUserRole);

            //Act
            ResourceToApplicationRolesMapper mapper = new ResourceToApplicationRolesMapper(_filepath, _nlogger);

            mapper.ResourceItemsWithRoles.Should().NotBeNull();
            ServiceCenterUser_GoogleBigQueryItineraryAU_True(loggedInUser, "GoogleBigQueryItinerary", mapper);
            AnalyticsTeamUser_GoogleBigQueryItineraryNZ_True(loggedInUser, "GoogleBigQueryItinerary", mapper);
            FinanceTeamUser_FareEscalationJournalAU_True(loggedInUser, "FareEscalationJournalAU", mapper);
           
        }

        [TestMethod()]
        public void IsUserRoleAllowedForResource_NotAllowed()
        {
            //Arrange
            _filepath += "RoleBasedMenuItemMap.xml";
            String[] loggedInUserRole = { "AnalyticsTeam", "FinanceTeam" };
            IPrincipal loggedInUser = new GenericPrincipal(new GenericIdentity("LoggedInUser"), loggedInUserRole);

            //Act
            ResourceToApplicationRolesMapper mapper = new ResourceToApplicationRolesMapper(_filepath, _nlogger);
            
            AnalyticsTeamUser_ReviewPendingBookingsNZ_False(loggedInUser, "ReviewPendingBookingsNZ", mapper);
            FinanceTeamUser_CreditCardTransactionsToCheckNZ_False(loggedInUser, "CreditCardTransactionsToCheckNZ", mapper);
        }
        
        private void ServiceCenterUser_GoogleBigQueryItineraryAU_True(IPrincipal loggedInUser, string resourceKey,ResourceToApplicationRolesMapper mapper)
        {
            //Assert
            bool result = mapper.IsUserRoleAllowedForResource(resourceKey, loggedInUser);
            result.Should().BeTrue();
        }
        private void AnalyticsTeamUser_GoogleBigQueryItineraryNZ_True(IPrincipal loggedInUser, string resourceKey, ResourceToApplicationRolesMapper mapper)
        {
            //Assert
            bool result = mapper.IsUserRoleAllowedForResource(resourceKey, loggedInUser);
            result.Should().BeTrue();
        }
        private void AnalyticsTeamUser_ReviewPendingBookingsNZ_False(IPrincipal loggedInUser, string resourceKey, ResourceToApplicationRolesMapper mapper)
        {
            //Assert
            bool result = mapper.IsUserRoleAllowedForResource(resourceKey, loggedInUser);
            result.Should().BeFalse();
        }
        private void FinanceTeamUser_FareEscalationJournalAU_True(IPrincipal loggedInUser, string resourceKey, ResourceToApplicationRolesMapper mapper)
        {
            //Assert
            bool result = mapper.IsUserRoleAllowedForResource(resourceKey, loggedInUser);
            result.Should().BeTrue();
        }
        private void FinanceTeamUser_CreditCardTransactionsToCheckNZ_False(IPrincipal loggedInUser, string resourceKey, ResourceToApplicationRolesMapper mapper)
        {
            //Assert
            bool result = mapper.IsUserRoleAllowedForResource(resourceKey, loggedInUser);
            result.Should().BeFalse();
        }
        private void GoogleBigQueryItineraryAU_ServiceCenterAndAnalytics(ResourceToApplicationRolesMapper mapper)
        {
            //Act
            List<string> roles = mapper.GetAllowedRolesForResource("GoogleBigQueryItinerary");

            //Assert
            roles.Count.Should().Be(2);
            roles[0].ShouldBeEquivalentTo("ServiceCenter");
            roles[1].ShouldBeEquivalentTo("AnalyticsTeam");
        }
        private void ReviewPendingBookingsAU_ServiceCenterAndDevSupport(ResourceToApplicationRolesMapper mapper)
        {
            //Act
            List<string> roles = mapper.GetAllowedRolesForResource("ReviewPendingBookingsAU");

            //Assert
            roles.Count.Should().Be(2);
            roles[0].ShouldBeEquivalentTo("ServiceCenter");
            roles[1].ShouldBeEquivalentTo("DevSupport");
        }
        private void FareEscalationJournalAU_FinanceTeam(ResourceToApplicationRolesMapper mapper)
        {
            //Act
            List<string> roles = mapper.GetAllowedRolesForResource("FareEscalationJournalAU");

            //Assert
            roles.Count.Should().Be(1);
            roles[0].ShouldBeEquivalentTo("FinanceTeam");
           
        }
        private void GoogleBigQueryItineraryNZ_ServiceCenterAndAnalytics(ResourceToApplicationRolesMapper mapper)
        {
            //Act
            List<string> roles = mapper.GetAllowedRolesForResource("GoogleBigQueryItinerary");

            //Assert
            roles.Count.Should().Be(2);
            roles[0].ShouldBeEquivalentTo("ServiceCenter");
            roles[1].ShouldBeEquivalentTo("AnalyticsTeam");
        }
        private void CreditCardTransactionsToCheckAU_ServiceCenterAndDevSupportAndProductTeam(ResourceToApplicationRolesMapper mapper)
        {
            //Act
            List<string> roles = mapper.GetAllowedRolesForResource("CreditCardTransactionsToCheckAU");

            //Assert
            roles.Count.Should().Be(3);
            roles[0].ShouldBeEquivalentTo("ServiceCenter");
            roles[1].ShouldBeEquivalentTo("DevSupport");
            roles[2].ShouldBeEquivalentTo("ProductTeam");
        }
        private void ReviewPendingBookingsNZ_ServiceCenterAndDevSupport(ResourceToApplicationRolesMapper mapper)
        {
            //Act
            List<string> roles = mapper.GetAllowedRolesForResource("ReviewPendingBookingsNZ");

            //Assert
            roles.Count.Should().Be(2);
            roles[0].ShouldBeEquivalentTo("ServiceCenter");
            roles[1].ShouldBeEquivalentTo("DevSupport");
        }
        private void FareEscalationJournalNZ_FinanceTeamAndProductTeam(ResourceToApplicationRolesMapper mapper)
        {
            //Act
            List<string> roles = mapper.GetAllowedRolesForResource("FareEscalationJournalNZ");

            //Assert
            roles.Count.Should().Be(2);
            roles[0].ShouldBeEquivalentTo("FinanceTeam");
            roles[1].ShouldBeEquivalentTo("ProductTeam");
        }
        private void CreditCardTransactionsToCheckNZ_ServiceCenterAndDevSupportAndProductTeam(ResourceToApplicationRolesMapper mapper)
        {
            //Act
            List<string> roles = mapper.GetAllowedRolesForResource("CreditCardTransactionsToCheckNZ");

            //Assert
            roles.Count.Should().Be(3);
            roles[0].ShouldBeEquivalentTo("ServiceCenter");
            roles[1].ShouldBeEquivalentTo("DevSupport");
            roles[2].ShouldBeEquivalentTo("ProductTeam");
            
        }
        
    }
}