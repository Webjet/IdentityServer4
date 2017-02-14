using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using Serilog.Sinks;

namespace AdminPortal.UnitTests.TestUtilities
{
    public class AssemblyInitialize
    {

        [AssemblyInitialize()] 
        public static void MyTestInitialize(TestContext testContext)
        {
            //Alternatively consider [TestInitialize()] http://stackoverflow.com/questions/639326/mstest-executing-method-before-each-test-in-an-assembly/640989#640989
            //  https://github.com/serilog/serilog/issues/703 
         //   Log.Logger.Should().BeOfType<SilentLogger>();

            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().CreateLogger();

            //var sink = new DisposableSink();
            //var logger = (IDisposable)new LoggerConfiguration()
            //    .WriteTo.Sink(sink)
            //    .CreateLogger();
        }
 
    }
}
