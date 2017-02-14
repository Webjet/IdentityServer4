using System.Security.Claims;
using AdminPortal.BusinessServices.Common.Debugging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSA.TestsCommon.TestUtilities;
using Webjobs.ImportSurveys.Common;

namespace AdminPortal.UnitTests.Common.Debugging
{
    [TestClass()]
    public class IdentityTracingTests
    {
        public TestContext TestContext { get; set; }
        [TestMethod()]
        public void WriteClaimsTest()
        {
            ClaimsPrincipal principal = ClaimsPrincipalCreator.CreateClaimsPrincipal();
            var list=principal.WriteClaims();
            TestContext.WriteString(list);
        }
    }
}