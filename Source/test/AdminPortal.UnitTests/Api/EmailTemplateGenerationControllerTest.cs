using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AdminPortal.Api;
using AdminPortal.BusinessServices;
using AdminPortal.UnitTests.TestUtilities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdminPortal.UnitTests.Api
{
    [TestClass()]
    public class EmailTemplateGenerationControllerTest
    {
        [TestMethod()]
        public void Initialize_EmailTemplateGenerationController_NotNull()
        {
            //Arrange and Act
            var loggedInUser = PrincipalStubBuilder.GetClaimPrincipalWithServiceCenterRole();
            var controller = InitEmailTemplateGenerationController(loggedInUser);
            
            //Assert
            controller.Should().NotBeNull();

        }

        [TestMethod()]
        public async Task GetServiceCenterTeamLeadersEmailList_LoggedInWithServiceCenter_EmailList()
        {
            //Arrange
            var loggedInUser = PrincipalStubBuilder.GetClaimPrincipalWithServiceCenterRole();
            var controller = InitEmailTemplateGenerationController(loggedInUser);

            //Act
            IEnumerable<string> emailList= await  controller.GetServiceCenterTeamLeadersEmailList();
            
            //Assert
            emailList.Should().NotBeEmpty();
            emailList.Count().Should().Be(2);
            emailList.Contains("scm.test@webjet.com.au").Should().BeTrue();

        }

        [TestMethod()]
        public async Task GetServiceCenterTeamLeadersEmailList_LoggedInWithMarketingRole_NullEmailList()
        {
            //Arrange
            var loggedInUser = PrincipalStubBuilder.GetClaimPrincipalWithMarketingRole();
            var controller = InitEmailTemplateGenerationController(loggedInUser);
           
            // Act
            IEnumerable<string> emailList = await controller.GetServiceCenterTeamLeadersEmailList();

            //Assert
            emailList.Should().BeNullOrEmpty();
            
        }

        private EmailTemplateGenerationController InitEmailTemplateGenerationController(ClaimsPrincipal userClaimsPrincipal)
        {
            //Arrange
            var config = ConfigurationHelper.GetConfigurationSubsitituteForGraphAPIClient();
            TeamLeadersRetrieval teamLeaderRetrieval = new TeamLeadersRetrieval(config);

            //Act
            var controller = new EmailTemplateGenerationController(teamLeaderRetrieval);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = userClaimsPrincipal
                      
                }
            };

            return controller;
        }
    }
}
