using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using NLog;

namespace WebFormsOpenIdConnectAzureAD
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
           

        }
        void Session_Start(object sender, EventArgs e)
        {
            LogSession(MethodBase.GetCurrentMethod().Name, sender);
            Session["RequestCounter"] = 0;
            LogSession(MethodBase.GetCurrentMethod().Name + " after assignment " , sender);
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
  //           LogSession(MethodBase.GetCurrentMethod().Name, sender);

        }

        protected void Application_EndRequest(Object sender, EventArgs e)
        {
          //  LogSession(MethodBase.GetCurrentMethod().Name, sender);

        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            // Logger.Debug(" FormsAuthenticationCookie    {0} \n FormsAuthentication={1} Request.IsAuthenticated={2} ", TraceOutputHelper.OutputFormsAuthenticationCookie(Request), TraceOutputHelper.FormsAuthenticationAsString("Application_AuthenticateRequest"), Request.IsAuthenticated);
           // LogSession(MethodBase.GetCurrentMethod().Name, sender);
        }
        protected void Application_PreRequestHandlerExecute(Object sender, EventArgs e)
        {
            LogSession(MethodBase.GetCurrentMethod().Name, sender);

        }
        protected void Application_PostRequestHandlerExecute(Object sender, EventArgs e)
        {
            LogSession(MethodBase.GetCurrentMethod().Name, sender);

        }

        public static void LogSession(string methodName, object sender)
        {
            Debug.WriteLine("LogSession:" + methodName + " sender " + sender + " Sessions Count " + HttpContext.Current?.Session?.Count);
        }
    }
}
