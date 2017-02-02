using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AdminPortal.BusinessServices.Common
{
    public class WebApplicationHelper
    {
        //http://stackoverflow.com/questions/6041332/best-way-to-get-application-folder-path/6041420
        public static string WebApplicationRootDirectory()
        {
            var rootDirectory = (HttpContext.Current?.Server != null)
                ? HttpContext.Current.Server.MapPath("~")
                : Directory.GetCurrentDirectory();
            return rootDirectory;
        }

    }
}
