using System;
using System.Data.SqlTypes;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AdminPortal.Controllers;
using AdminPortal.UnitTests.TestUtilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog.LayoutRenderers.Wrappers;
using NSubstitute;

namespace AdminPortal.UnitTests.Controllers
{
    [TestClass()]
    public class UserProfileControllerTests
    {
        [TestMethod()]
        public void RefreshSessionTest()
        {
            //Arrange
            var controller = ControllerAssertions.ArrangeController<UserProfileController>();

            //Act
            Action act = () => controller.RefreshSession();
            //Assert
            act.ShouldThrow<InvalidOperationException>()
                 .WithMessage("No owin.Environment item was found in the context."); 
            //         at System.Web.HttpContextBaseExtensions.GetOwinContext(HttpContextBase context)
            //at AdminPortal.Controllers.UserProfileController.RefreshSession() in C:\GitRepos\AdminPortal\Source\AdminPortal\Controllers\UserProfileController.cs:line 64

            //TODO: Set GetOwinContext and other to simulate valid operation
        }






    }
}