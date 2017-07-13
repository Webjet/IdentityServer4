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
        public static readonly string BusinessServicesConfigPath = _rootFolder + ConfigFolder;

        public static GroupToTeamNameMapper GetGroupToTeamNameMapper()
        {
            var teamNameFile = BusinessServicesConfigPath + "GroupToTeamNameMap.xml";
            return new GroupToTeamNameMapper(teamNameFile);
        }
    }
}
