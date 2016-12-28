using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using AdminPortal.BusinessServices;

namespace AdminPortal.UnitTests.BusinessServices
{
    [TestClass(),Ignore]
    public class ResourceToApplicationRolesMapperTests
    {
        [TestMethod()]
        public void ResourceToApplicationRolesMapperTest()
        {
            //Arrange
            //TODO: Embedded Resource and read xml and pass XML doc to LandingPageLayoutLoader().
            string filepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\config\\RoleBasedMenuItemMap.xml";
            ResourceToApplicationRolesMapper resourceToApplicationRolesMapper = new ResourceToApplicationRolesMapper(filepath);

            //Act
         
        }

    }
}