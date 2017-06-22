using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminPortal.BusinessServices;
using AdminPortal.UnitTests.Common;
using AdminPortal.UnitTests.TestUtilities;
using FluentAssertions;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace AdminPortal.UnitTests.BusinessServices
{
    [TestClass()]
    public class TeamLeadersRetrievalTest
    {
        [TestMethod()]
        public async Task TeamLeadersRetrieval_ServiceCenterRole_ReturnsEmailList()
        {
            //Arrange
            var config = ConfigurationHelper.GetConfigurationSubsitituteForGraphAPIClient();
            var loggedInUser = PrincipalStubBuilder.GetClaimPrincipalWithServiceCenterRole();
            var groupToTeamNameMapper = BusinessServiceHelper.GetGroupToTeamNameMapper();

            TeamLeadersRetrieval teamLeadersRetrieval = new TeamLeadersRetrieval(config, groupToTeamNameMapper);

            //Act
            IEnumerable<string> emailList = await teamLeadersRetrieval.GetServiceCenterTeamLeaderEmailListAsync(loggedInUser);

            //Assert
            teamLeadersRetrieval.Should().NotBeNull();
            emailList.Should().NotBeNullOrEmpty();
            emailList.Count().Should().Be(2);


        }

        [TestMethod()]
        public async Task TeamLeadersRetrieval_MarketingTeamRole_ReturnsNullEmailList()
        {
            //Arrange
            var config = ConfigurationHelper.GetConfigurationSubsitituteForGraphAPIClient();
            var loggedInUser = PrincipalStubBuilder.GetClaimPrincipalWithMarketingRole();
            var groupToTeamNameMapper = BusinessServiceHelper.GetGroupToTeamNameMapper();
            TeamLeadersRetrieval teamLeadersRetrieval = new TeamLeadersRetrieval(config, groupToTeamNameMapper);

            //Act
            IEnumerable<string> emailList = await teamLeadersRetrieval.GetServiceCenterTeamLeaderEmailListAsync(loggedInUser);

            //Assert
            teamLeadersRetrieval.Should().NotBeNull();
            emailList.Should().BeNullOrEmpty();
        
        }

        [TestMethod()]
        public void TeamLeadersRetrieval_GraphAPINull_ThrowsException()
        {
            //Arrange
            var config = Substitute.For<IConfigurationRoot>();
            var groupToTeamNameMapper = BusinessServiceHelper.GetGroupToTeamNameMapper();
            //Act
            Action act = () =>
            {
                var leadersRetrieval = new TeamLeadersRetrieval(config, groupToTeamNameMapper);
                
            };
     
            //Assert
            act.ShouldThrow<AuthenticationException>();
            
        }

        [TestMethod()]
        public  void TeamLeadersRetrieval_GraphAPINotNull_ThrowsException()
        {
            //Arrange
            var config = ConfigurationHelper.GetConfigurationSubsitituteForGraphAPIClient();
            var groupToTeamNameMapper = BusinessServiceHelper.GetGroupToTeamNameMapper();
            TeamLeadersRetrieval leadersRetrieval = null;
            //Act
            Action act = () =>
            {
                leadersRetrieval = new TeamLeadersRetrieval(config, groupToTeamNameMapper);

            };

            //Assert
            act.ShouldNotThrow<AuthenticationException>();
            leadersRetrieval.Should().NotBeNull();

        }
    }
}
