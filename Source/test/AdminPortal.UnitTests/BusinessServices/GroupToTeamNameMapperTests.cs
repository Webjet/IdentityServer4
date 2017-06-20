using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdminPortal.BusinessServices;
using AdminPortal.UnitTests.TestUtilities;

namespace AdminPortal.UnitTests.BusinessServices
{
    [TestClass()]
    public class GroupToTeamNameMapperTests
    {
        const string ConfigFolder = @"BusinessServices\config\";
        private static readonly string _rootFolder = AssemblyHelper.GetExecutingAssemblyRootPath();
        private readonly string _filepath = _rootFolder + ConfigFolder;
     
        [TestMethod()]
        public void GetTeamGroup_TwoGroupIdsNotMatched_NotFound()
        {
            // Arrange
            List<string> groupIds = new List<string>();
            groupIds.Add("123");
            groupIds.Add("456");

            // Act
            var foundGroup = this.GetGroupToTeamNameMapper().GetTeamGroup(groupIds);

            // Assert
            foundGroup.Should().BeNull();
        }

        [TestMethod()]
        public void GetTeamGroup_ThreeGroupIds_FoundGroupMatch()
        {
            // Arrange
            List<string> groupIds = new List<string>();
            groupIds.Add("123");
            groupIds.Add("413687dc-ee0c-4326-9ae1-b2a87ebd28a1");
            groupIds.Add("456");

            // Act
            var foundGroup = this.GetGroupToTeamNameMapper().GetTeamGroup(groupIds);

            // Assert
            foundGroup.GroupId.Should().Be("413687dc-ee0c-4326-9ae1-b2a87ebd28a1");
        }

        [TestMethod()]
        public void GetTeamGroup_ThreeGroupIds_FoundGroupMatchLast()
        {
            // Arrange
            List<string> groupIds = new List<string>();
            groupIds.Add("123");            
            groupIds.Add("413687dc-ee0c-4326-9ae1-b2a87ebd28a1");
            groupIds.Add("fd9379dc-af88-4eda-9a34-5e8974403ad7");
            groupIds.Add("456");

            // Act
            var foundGroup = this.GetGroupToTeamNameMapper().GetTeamGroup(groupIds);

            // Assert
            foundGroup.GroupId.Should().Be("fd9379dc-af88-4eda-9a34-5e8974403ad7");
        }

        private GroupToTeamNameMapper GetGroupToTeamNameMapper()
        {
            var teamNameFile = _filepath + "GroupToTeamNameMap.xml";
            return new GroupToTeamNameMapper(teamNameFile);
        }
    }
}
