using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminPortal.BusinessServices;
using AdminPortal.BusinessServices.GraphApiHelper;
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
        [TestMethod(),Ignore]
        public async Task TeamLeadersRetrieval_ServiceCenterRole_ReturnsEmailList()
        {
            //Arrange
            var loggedInUser = PrincipalStubBuilder.GetClaimPrincipalWithServiceCenterRole();
            TeamLeadersRetrieval teamLeadersRetrieval = InitializeTeamLeadersRetrieval();

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
       var loggedInUser = PrincipalStubBuilder.GetClaimPrincipalWithMarketingRole();
         TeamLeadersRetrieval teamLeadersRetrieval = InitializeTeamLeadersRetrieval();

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
                var leadersRetrieval = new TeamLeadersRetrieval(groupToTeamNameMapper,null);
                
            };
     
            //Assert
            act.ShouldThrow<AuthenticationException>();
            
        }

        [TestMethod()]
        public  void TeamLeadersRetrieval_GraphAPINotNull_ThrowsException()
        {
            //Arrange
            TeamLeadersRetrieval leadersRetrieval = null;
            //Act
            Action act = () =>
            {
                leadersRetrieval = InitializeTeamLeadersRetrieval();

            };

            //Assert
            act.ShouldNotThrow<AuthenticationException>();
            leadersRetrieval.Should().NotBeNull();

        }

        private TeamLeadersRetrieval InitializeTeamLeadersRetrieval()
        {
            var config = ConfigurationHelper.GetConfigurationSubsitituteForGraphAPIClient();
            var groupToTeamNameMapper = BusinessServiceHelper.GetGroupToTeamNameMapper();
            var graphClient = Substitute.For<IActiveDirectoryClient>();
            var activeGraphClientHelper = Substitute.For<ActiveDirectoryGraphHelper>(config, graphClient);
            TeamLeadersRetrieval teamLeadersRetrieval = new TeamLeadersRetrieval(groupToTeamNameMapper, activeGraphClientHelper);
            return teamLeadersRetrieval;
        }
    }
}
