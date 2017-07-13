using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdminPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using AdminPortal.BusinessServices;
using AdminPortal.Controllers;
using AdminPortal.UnitTests;
using AdminPortal.UnitTests.TestUtilities;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using NSubstitute;

namespace AdminPortal.UnitTests
{
    [TestClass()]
    public class ResourceAuthorizeAttributeTests : AuthorizeAttribute
    {
        
        private readonly string _filepath = FilePathHelper.GetConfigFileFolderPath();


        [TestMethod()]
        //https://developercommunity.visualstudio.com/content/problem/19415/deploymentitem-attribute-not-working.html
        //Not supported for .Net Core project
        //[DeploymentItem(@"BusinessServices\config\ResourceToRolesMapSample.xml", "config\\ResourceToRolesMapSample.xml")]
        public void GetAllowedRolesForResource_GoogleBigQueryItinerary_Test()
        {
            //Arrange
            string resourceKey = "GoogleBigQueryItinerary";
            var file = _filepath + "ResourceToRolesMapSample.xml";
        
            ResourceToApplicationRolesMapper mapper = new ResourceToApplicationRolesMapper(null, file);


            //Act
            ResourceAuthorizeAttribute target = new ResourceAuthorizeAttribute(resourceKey, mapper);

            //Assert
            resourceKey.Should().NotBeEmpty();
            target.Roles.Should().NotBeNull();
            target.Roles.Should().Contain("ServiceCenter");
            target.Roles.Should().Contain("AnalyticsTeam");
        }


        [TestMethod()]
        public void GetAllowedRolesForResource_NullResource_Null()
        {
            //Arrange
            string resourceKey = null;
            var file = _filepath + "ResourceToRolesMapSample.xml";

            ResourceToApplicationRolesMapper mapper = new ResourceToApplicationRolesMapper(null, file);


            //Act
            ResourceAuthorizeAttribute target = new ResourceAuthorizeAttribute(resourceKey, mapper);

            //Assert
            resourceKey.Should().BeNullOrEmpty();
            target.Roles.Should().BeNull();
        }


     

    }
}