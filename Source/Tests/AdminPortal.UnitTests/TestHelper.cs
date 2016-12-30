using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AdminPortal.UnitTests
{
   public static class TestHelper
    {
       public static string GetExecutingAssembly()
       {
           return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
       }
    }
}
