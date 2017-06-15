using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminPortal.BusinessServices;
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
            TeamLeadersRetrieval teamLeadersRetrieval = new TeamLeadersRetrieval(config);

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
            TeamLeadersRetrieval teamLeadersRetrieval = new TeamLeadersRetrieval(config);

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
          
            //Act
            Action act = () =>
            {
                var leadersRetrieval = new TeamLeadersRetrieval(config);
                
            };
     
            //Assert
            act.ShouldThrow<AuthenticationException>();
            
        }

        [TestMethod()]
        public  void TeamLeadersRetrieval_GraphAPINotNull_ThrowsException()
        {
            //Arrange
            var config = ConfigurationHelper.GetConfigurationSubsitituteForGraphAPIClient();
            TeamLeadersRetrieval leadersRetrieval = null;
            //Act
            Action act = () =>
            {
                leadersRetrieval = new TeamLeadersRetrieval(config);

            };

            //Assert
            act.ShouldNotThrow<AuthenticationException>();
            leadersRetrieval.Should().NotBeNull();

        }
    }
}
