using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminPortal.UnitTests.TestUtilities
{
    public static class FilePathHelper
    {
        const string ConfigFolder = @"BusinessServices\config\";

        public static string GetConfigFileFolderPath()
        {
            //TODO: unable to get the current directory path. Travelling 4 folders up from the executing assembly folder.
            string rootFolder = AssemblyHelper.GetExecutingAssemblyRootPath();
            string filepath = rootFolder + ConfigFolder;
            return filepath;
        }
    }
}
