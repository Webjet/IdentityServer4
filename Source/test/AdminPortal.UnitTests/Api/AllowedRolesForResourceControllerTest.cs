using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AdminPortal.Api;
using AdminPortal.BusinessServices;
using AdminPortal.Controllers;
using AdminPortal.UnitTests.TestUtilities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace AdminPortal.UnitTests.Api
{
    [TestClass()]
    public class AllowedRolesForResourceControllerTest
    {
        const string ConfigFolder = @"BusinessServices\config\";
        //TODO: unable to get the current directory path. Travelling 4 folders up from the executing assembly folder.
        private static readonly string _rootFolder = AssemblyHelper.GetExecutingAssemblyRootPath();
        private readonly string _filepath = _rootFolder + ConfigFolder;

        [TestMethod()]
        public void Initialize_ResourceToApplicationRolesMapperObject_NotNull()
        {
            //Arrange and Act
            var controller = InitAllowedRolesForResourceController();

            //Assert
            controller.Should().NotBeNull();
           
        }

        [TestMethod()]
        public void GetResourcesForUser_UserWithServiceCenterAnalyticsAndFinanceRoles_ListOfResources()
        {
            //Arrange
            var controller = InitAllowedRolesForResourceController();

            //Act
            var resources = controller.GetResourcesForUser();

            //Assert
            resources.Count.Should().Be(7);
            var expected = new List<String>() { "GoogleBigQueryItinerary", "ReviewPendingBookings_WebjetAU", "FareEscalationJournal_WebjetAU", "CreditCardTransactionsToCheck_WebjetAU", "ReviewPendingBookings_WebjetNZ", "FareEscalationJournal_WebjetNZ", "CreditCardTransactionsToCheck_WebjetNZ" };
            resources.Union(expected).Count().Should().Be(7);
        }

        [TestMethod()]
        public void Get_IsUserRoleAllowedForReviewPendingBookingsWebjetAU_True()
        {
            //Arrange
            string resourceKey = "ReviewPendingBookings_WebjetAU";
            var controller = InitAllowedRolesForResourceController();

           //Act
            var response = controller.Get(resourceKey);

            //Assert
            response.Should().BeTrue();
        }

        [TestMethod()]
        public void Get_AllResourcesWithRoles()
        {
            //Arrange
            string resourceKey = "ReviewPendingBookings_WebjetAU";
            var controller = InitAllowedRolesForResourceController();

            //Act
            var resourceItemsWithRoles = controller.Get();

            //Assert
            resourceItemsWithRoles.Should().NotBeNull();
            resourceItemsWithRoles.ContainsKey(resourceKey).Should().BeTrue();
            
        }

        private AllowedRolesForResourceController InitAllowedRolesForResourceController()
        {

            var file = _filepath + "ResourceToRolesMapSample.xml";
            ResourceToApplicationRolesMapper mapper = new ResourceToApplicationRolesMapper(null, file);
            var controller = new AllowedRolesForResourceController(mapper);
            //InitAllowedRolesForResourceController();

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User =
                        PrincipalStubBuilder.GetClaimPrincipalWithServiceCenterAnalyticsAndFinanceRoles()
                }
            };
            return controller;
        }
    }
}
