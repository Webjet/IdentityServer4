using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AdminPortal.Api;
using AdminPortal.BusinessServices;
using AdminPortal.BusinessServices.GraphApiHelper;
using AdminPortal.UnitTests.Common;
using AdminPortal.UnitTests.TestUtilities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace AdminPortal.UnitTests.Api
{
    [TestClass()]
    public class EmailTemplateGenerationControllerTest
    {
      
        [TestMethod()] 
        public async Task GetServiceCenterTeamLeadersEmailList_LoggedInWithServiceCenter_EmailList()
        {
            //Arrange
            var loggedInUser = PrincipalStubBuilder.GetClaimPrincipalWithServiceCenterRole();
            var teamLeaderRetrieval = GetTeamLeadersRetrievalForServiceCenter(loggedInUser);
            var controller = InitEmailTemplateGenerationController(loggedInUser, teamLeaderRetrieval);

            //Act
            IEnumerable<string> emailList = await controller.GetServiceCenterTeamLeadersEmailList();

            //Assert
            emailList.Should().NotBeNullOrEmpty();
            emailList.Count().Should().Be(1);
            emailList.Contains("test@webjet.com.au").Should().BeTrue();

        }

        [TestMethod()] 
        public async Task GetServiceCenterTeamLeadersEmailList_LoggedInWithMarketingRole_NullEmailList()
        {
            //Arrange
            var loggedInUser = PrincipalStubBuilder.GetClaimPrincipalWithMarketingRole();
            var teamLeaderRetrieval = GetTeamLeadersRetrievalForMarketing(loggedInUser);
            var controller = InitEmailTemplateGenerationController(loggedInUser, teamLeaderRetrieval);

            // Act
            IEnumerable<string> emailList = await controller.GetServiceCenterTeamLeadersEmailList();

            //Assert
            emailList.Should().BeNullOrEmpty();

        }

        private EmailTemplateGenerationController InitEmailTemplateGenerationController(ClaimsPrincipal userClaimsPrincipal, ITeamLeadersRetrieval teamLeaderRetrieval)
        {
       
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

        private static ITeamLeadersRetrieval GetTeamLeadersRetrievalForServiceCenter(ClaimsPrincipal userClaimsPrincipal)
        {

            var teamLeaderRetrieval = Substitute.For<ITeamLeadersRetrieval>();
              

            List<string> emailList = new List<string>() {"test@webjet.com.au"};
            teamLeaderRetrieval.GetServiceCenterTeamLeaderEmailListAsync(userClaimsPrincipal).Returns(emailList);
            return teamLeaderRetrieval;
        }


        private static ITeamLeadersRetrieval GetTeamLeadersRetrievalForMarketing(ClaimsPrincipal userClaimsPrincipal)
        {
          
            var teamLeaderRetrieval = Substitute.For<ITeamLeadersRetrieval>();
          
            List<string> emailList = null;
            teamLeaderRetrieval.GetServiceCenterTeamLeaderEmailListAsync(userClaimsPrincipal).Returns(emailList);
            return teamLeaderRetrieval;
        }

    }
}
