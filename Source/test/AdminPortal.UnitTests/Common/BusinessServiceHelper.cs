using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminPortal.BusinessServices;
using AdminPortal.UnitTests.TestUtilities;

namespace AdminPortal.UnitTests.Common
{
    public static class BusinessServiceHelper
    {
        const string ConfigFolder = @"BusinessServices\config\";
        private static readonly string _rootFolder = AssemblyHelper.GetExecutingAssemblyRootPath();
        private static readonly string _filepath = _rootFolder + ConfigFolder;

        public static GroupToTeamNameMapper GetGroupToTeamNameMapper()
        {
            var teamNameFile = _filepath + "GroupToTeamNameMap.xml";
            return new GroupToTeamNameMapper(teamNameFile);
        }
    }
}
