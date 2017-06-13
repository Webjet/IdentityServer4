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
    public class TeamLeadersRetrivalTest
    {
        [TestMethod()]
        public async Task TeamLeadersRetrival_ServiceCenterRole_ReturnsEmailList()
        {
            //Arrange
            var config = ConfigurationHelper.GetConfigurationSubsitituteForGraphAPIClient();
            var loggedInUser = PrincipalStubBuilder.GetClaimPrincipalWithServiceCenterRole();
            TeamLeadersRetrival teamLeadersRetrival = new TeamLeadersRetrival(config);

            //Act
            IEnumerable<string> emailList = await teamLeadersRetrival.GetServiceCenterTeamLeaderEmaiListAsync(loggedInUser);

            //Assert
            teamLeadersRetrival.Should().NotBeNull();
            emailList.Should().NotBeNullOrEmpty();
            emailList.Count().Should().Be(2);


        }

        [TestMethod()]
        public async Task TeamLeadersRetrival_MarketingTeamRole_ReturnsNullEmailList()
        {
            //Arrange
            var config = ConfigurationHelper.GetConfigurationSubsitituteForGraphAPIClient();
            var loggedInUser = PrincipalStubBuilder.GetClaimPrincipalWithMarketingRole();
            TeamLeadersRetrival teamLeadersRetrival = new TeamLeadersRetrival(config);

            //Act
            IEnumerable<string> emailList = await teamLeadersRetrival.GetServiceCenterTeamLeaderEmaiListAsync(loggedInUser);

            //Assert
            teamLeadersRetrival.Should().NotBeNull();
            emailList.Should().BeNullOrEmpty();
        
        }

        [TestMethod()]
        public void TeamLeadersRetrival_GraphAPINull_ThrowsException()
        {
            //Arrange
            var config = Substitute.For<IConfigurationRoot>();
          
            //Act
            Action act = () =>
            {
                var leadersRetrival = new TeamLeadersRetrival(config);
                
            };
     
            //Assert
            act.ShouldThrow<AuthenticationException>();
            
        }

        [TestMethod()]
        public  void TeamLeadersRetrival_GraphAPINotNull_ThrowsException()
        {
            //Arrange
            var config = ConfigurationHelper.GetConfigurationSubsitituteForGraphAPIClient();
            TeamLeadersRetrival leadersRetrival = null;
            //Act
            Action act = () =>
            {
                leadersRetrival = new TeamLeadersRetrival(config);

            };

            //Assert
            act.ShouldNotThrow<AuthenticationException>();
            leadersRetrival.Should().NotBeNull();

        }
    }
}
