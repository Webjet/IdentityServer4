using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;
using AdminPortal.BusinessServices.Common;
using AdminPortal.BusinessServices.Common.Debugging;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSA.TestsCommon.TestUtilities;

namespace AdminPortal.UnitTests.Common
{
    [TestClass()]
    public class AssemblyInformationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod()]
        public void CompileDateTest()
        {
            TestContext.WriteLine(AssemblyInformation.ExecutingAssembly.ToString());
            var filetime = File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
            TestContext.WriteLine("filetime " + filetime);
            var compileDate = AssemblyInformation.CompileDate;
            TestContext.WriteLine("compileDate =" +compileDate.ToString());
            filetime.Should().BeCloseTo(compileDate, 60000);//within a minute
        }
    }
}
