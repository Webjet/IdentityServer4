using AdminPortal.BusinessServices;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdminPortal.UnitTests
{
    [TestClass()]
    public class ResourceIdsConfigValidatorTests
    {
        [TestMethod()]
        public void FindDuplicatesInResourceItemsWithRoles_Duplicate()
        {
            ResourceToApplicationRolesMap mapper = new ResourceToApplicationRolesMap()
            {
                ResourceToRoles =new []
                {
                    new ResourceToApplicationRolesMapResourceToRoles() {ResourceId = "key1",Roles="role1,role2"} ,
                    new ResourceToApplicationRolesMapResourceToRoles() {ResourceId = "key1",Roles="role1,role2"} ,
                }
                
            };
            var validator = new ResourceIdsConfigValidator();

            var list= validator.FindDuplicatesInResourceItemsWithRoles(mapper);

            list.Count.Should().Be(1);
        }
        [TestMethod()]
        public void FindDuplicatesInResourceItemsWithRoles_NoDuplicates()
        {
            ResourceToApplicationRolesMap mapper = new ResourceToApplicationRolesMap()
            {
                ResourceToRoles = new[]
                {
                    new ResourceToApplicationRolesMapResourceToRoles() {ResourceId = "key1",Roles="role1,role2"} ,
                    new ResourceToApplicationRolesMapResourceToRoles() {ResourceId = "key2",Roles="role1,role2"} ,
                }

            };
            var validator = new ResourceIdsConfigValidator();

            var list = validator.FindDuplicatesInResourceItemsWithRoles(mapper);

            list.Count.Should().Be(0);
        }
    }
}