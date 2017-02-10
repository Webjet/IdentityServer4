using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;

namespace AdminPortal.BusinessServices.Common
{
    public class WebApplicationHelper
    {

        //http://stackoverflow.com/questions/6041332/best-way-to-get-application-folder-path/6041420
        public static string WebApplicationRootDirectory()
        {
#if NET40
            var rootDirectory = (HttpContext.Current?.Server != null)
                ? HttpContext.Current.Server.MapPath("~")
                : Directory.GetCurrentDirectory();

            return rootDirectory;
#endif  //NET40

            var rootDirectory = Directory.GetCurrentDirectory();
            return rootDirectory;
        }


    }
}
