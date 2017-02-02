using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AdminPortal.BusinessServices.Common.Debugging
{
    public class DebugEnvironmentSettings
    {

        public static string DebugApplicationFolderPath()
        {
            //Addition to http://stackoverflow.com/questions/6041332/best-way-to-get-application-folder-path/6041420
            var sb = new StringBuilder();
            try
            {
                //  Application.StartupPath   Windows.Forms
                // Application.ExecutablePath)); Windows.Forms
                sb.AppendLine("GetExecutingAssembly().Location: " + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                sb.AppendLine("AppDomain.CurrentDomain.BaseDirectory:" + AppDomain.CurrentDomain.BaseDirectory);
                sb.AppendLine("Directory.GetCurrentDirectory:" + System.IO.Directory.GetCurrentDirectory());
                sb.AppendLine("Environment.CurrentDirectory:" + Environment.CurrentDirectory);
                sb.AppendLine("System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase:" + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase));
                sb.AppendLine("System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath" + System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath);
                sb.AppendLine("HostingEnvironment.MapPath(\"~\")" + System.Web.Hosting.HostingEnvironment.MapPath("~"));
                sb.AppendLine("HttpContext.Current.Server.MapPath(\"~\")" + HttpContext.Current?.Server?.MapPath("~/"));
            }
            catch (Exception e)
            {
                sb.AppendLine(e.ToString());
            }
            Debug.WriteLine(sb);
            return sb.ToString();
        }

    }
}
