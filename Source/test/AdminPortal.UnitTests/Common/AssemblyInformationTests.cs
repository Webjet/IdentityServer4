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
using Webjobs.ImportSurveys.Common;

namespace AdminPortal.UnitTests.Common
{
    [TestClass()]
    public class AssemblyInformationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod()]
        public void CompileDateTest()
        {
            TestContext.WriteString(AssemblyInformation.ExecutingAssembly.ToString());
            var filetime = File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
            TestContext.WriteString("filetime " + filetime);
            var compileDate = AssemblyInformation.CompileDate;
            TestContext.WriteString("compileDate =" +compileDate);
            filetime.Should().BeOnOrAfter(compileDate);
        }
    }
}
