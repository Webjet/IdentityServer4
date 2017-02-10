using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace AdminPortal.UnitTests.TestUtilities
{
    public static class AssemblyHelper
    {
        public static string GetExecutingAssemblyDirectoryPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        //Returns 4 folders up from the executing assembly folder i.e. test\AdminPortal.UnitTests
        public static string GetExecutingAssemblyRootPath()
        {
            return Path.GetFullPath(Path.Combine(GetExecutingAssemblyDirectoryPath(), @"..\..\..\..\"));
        }

    }
}
