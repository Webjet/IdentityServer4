using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;

namespace AdminPortal.BusinessServices.Common.Debugging
{
    public class DebugEnvironmentSettings
    {
#if NET40 
        public static string DebugApplicationFolderPath()
        {
            //Addition to http://stackoverflow.com/questions/6041332/best-way-to-get-application-folder-path/6041420
            var sb = new StringBuilder();
            try
            {
                //  Application.StartupPath   Windows.Forms
                // Application.ExecutablePath)); Windows.Forms
                sb.AppendLine("GetExecutingAssembly().Location: " + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                sb.AppendLine("AppDomain.CurrentDomain.BaseDirectory: " + AppDomain.CurrentDomain.BaseDirectory);
                sb.AppendLine("Directory.GetCurrentDirectory: " + System.IO.Directory.GetCurrentDirectory());
                sb.AppendLine("Environment.CurrentDirectory: " + Environment.CurrentDirectory);
                sb.AppendLine("System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase: " + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase));
                sb.AppendLine("System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath: " + System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath);
                sb.AppendLine("HostingEnvironment.MapPath(\"~\"): " + System.Web.Hosting.HostingEnvironment.MapPath("~"));
                sb.AppendLine("HttpContext.Current.Server.MapPath(\"~\"): " + HttpContext.Current?.Server?.MapPath("~/"));
                sb.AppendLine("HostingEnvironment.ApplicationPhysicalPath: " + HostingEnvironment.ApplicationPhysicalPath);
                sb.AppendLine("HostingEnvironment.MapPath(\"~\"): " + HttpContext.Current?.Server?.MapPath("~"));
                sb.AppendLine("System.AppContext.BaseDirectory: " + System.AppContext.BaseDirectory);
                sb.AppendLine("System.IO.Path: " + System.IO.Path.GetDirectoryName("~"));
            }
            catch (Exception e)
            {
                sb.AppendLine(e.ToString());
            }
            Debug.WriteLine(sb);
            return sb.ToString();
        }

    
#endif // NET40
    }
}
