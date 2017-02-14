using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Webjobs.ImportSurveys.Common
{
    public static class TestContextExtensions
    {
        /// <summary>
        /// Declare        
        ///    public TestContext TestContext { get; set; } un test class
        /// and call 
        ///    TestContext.WriteString(msg);
        /// 
        /// Benefits: debug output is visible immediately,TestContext shown in test report.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public static void WriteString(this TestContext context, string message)
        {
            Debug.WriteLine(message);
            context.WriteLine("{0}",message);
        }
 
    }
}