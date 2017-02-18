using AdminPortal.BusinessServices.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace AdminPortal.UnitTests.BusinessServices.Logging
{
    [TestClass()]
    public class AdalLoggerCallbackTests
    {
       
        [TestMethod()]
        public void LogTest()
        {
          //  var memoryTarget = NlogTestsHelper.CreateMemoryTarget();
            ILogger logger= Substitute.For<ILogger>();
            new AdalLoggerCallback(logger).Log(Microsoft.IdentityModel.Clients.ActiveDirectory.LogLevel.Information,"just test" );
            logger.Received(1).LogTrace(Arg.Is<string>(x => x.Contains("just test")));// Arg.Any<string>());
            logger.Received(1).LogTrace(Arg.Is<string>(x => x.Contains("Information"))); 
            //logEntry.Should().Contain("just test");
            //logEntry.Should().Contain("Information");

        }

    }
}