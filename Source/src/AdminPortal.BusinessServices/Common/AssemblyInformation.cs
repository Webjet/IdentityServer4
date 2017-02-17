using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AdminPortal.BusinessServices.Common
{
    //from http://stackoverflow.com/questions/2050396/getting-the-date-of-a-net-assembly
    //TODO: move to Common, but allow to pass assembly instead of ExecutingAssembly  
    public static class AssemblyInformation
    {
        //TODO: remove static and make instance class
        public static void SetMainAssembly(Assembly mainAssembly)
        {
            _executingAssembly = mainAssembly;
        }
        private static System.Reflection.Assembly _executingAssembly;
        /// <summary>
        /// Gets the executing assembly.
        /// </summary>
        /// <value>The executing assembly.</value>
        public static System.Reflection.Assembly ExecutingAssembly
        {
            get { return _executingAssembly ?? (_executingAssembly = System.Reflection.Assembly.GetExecutingAssembly()); }
        }
        /// <summary>
        /// Gets the executing assembly version.
        /// </summary>
        /// <value>The executing assembly version.</value>
        public static System.Version ExecutingAssemblyVersion
        {
            get { return executingAssemblyVersion ?? (executingAssemblyVersion = ExecutingAssembly.GetName().Version); }
        }
        private static System.Version executingAssemblyVersion;

        /// <summary>
        /// Gets the compile date of the currently executing assembly.
        /// </summary>
        /// <value>The compile date.</value>
        public static System.DateTime CompileDate
        {
            get
            {
                if (!compileDate.HasValue)
                    compileDate = RetrieveLinkerTimestamp(ExecutingAssembly.Location);
                return compileDate ?? new System.DateTime();
            }
        }
        private static System.DateTime? compileDate;

        /// <summary>
        /// Retrieves the linker timestamp.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        /// <remarks>http://www.codinghorror.com/blog/2005/04/determining-build-date-the-hard-way.html</remarks>
        private static System.DateTime RetrieveLinkerTimestamp(string filePath)
        {
            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;
            var b = new byte[2048];
            System.IO.FileStream s = null;
            try
            {
                s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                    s.Close();
            }
            var dt = new System.DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(System.BitConverter.ToInt32(b, System.BitConverter.ToInt32(b, peHeaderOffset) + linkerTimestampOffset));
            return dt.AddHours(System.TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
        }
    }

}
