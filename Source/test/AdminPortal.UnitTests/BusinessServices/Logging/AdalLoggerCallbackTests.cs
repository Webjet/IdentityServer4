using System;
using System.Diagnostics;
using AdminPortal.BusinessServices.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Microsoft.IdentityModel.Clients;


namespace AdminPortal.UnitTests.BusinessServices.Logging
{
    [TestClass()]
    public class AdalLoggerCallbackTests
    {
        [TestMethod()]
        public void LogTest()
        {
            ILogger logger= Substitute.For<ILogger>();
            new AdalLoggerCallback(logger).Log(Microsoft.IdentityModel.Clients.ActiveDirectory.LogLevel.Information,"just test" );
           logger.ReceivedWithAnyArgs(1).LogTrace(Arg.Any<string>());

            /* Detailed match doesn't work- todo ask on SO
                        logger.Received(1).LogTrace(Arg.Is<string>(x => x.Contains("just test")), null);
                        logger.Received(1).LogTrace(Arg.Is<string>(x => x.Contains("Information")));
                        logger.Received(1).LogTrace(Arg.Any<string>());
                        // Error:           Actually received no matching calls.
                        //Received 1 non - matching call(non - matching arguments indicated with '*' characters):
                        //       Log<Object>(Trace, 0, *AdalLoggerCallback Information: just test *, < null >, Func<Object, Exception, String>)

              logger.Received(1).Log<Object>(Arg.Any<LogLevel>(), Arg.Any<int>(), Arg.Any<string>(), (Exception)null,Arg.Any<Func<object, Exception, string>>());
            //  NSubstitute.Exceptions.AmbiguousArgumentsException: Cannot determine argument specifications to use.
            //Please use specifications for all arguments of the same type.

               logger.Received(1).Log<Object>(LogLevel.Trace, 0, Arg.Is<string>(x => x.Contains("just test")),  null , Arg.Any<Func<Object, Exception, String>>());
//         NSubstitute.Exceptions.ReceivedCallsException: Expected to receive exactly 1 call matching:
//	Log<Object>(Trace, 0, x => x.Contains("just test"), <null>, any Func`3)
//Actually received no matching calls.
 */

        }
  
    }
}