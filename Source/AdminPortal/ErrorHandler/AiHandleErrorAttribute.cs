using System;using System.Web.Mvc;using Microsoft.ApplicationInsights;namespace AdminPortal.ErrorHandler{

#if INCLUDE_NOT_COVERED_BY_TESTS
    // Consider to enable https://adamstephensen.com/2015/05/21/add-application-insights-exception-handling-to-mvc-5/
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]     public class AiHandleErrorAttribute : HandleErrorAttribute    {        public override void OnException(ExceptionContext filterContext)        {            if (filterContext != null && filterContext.HttpContext != null && filterContext.Exception != null)            {                //If customError is Off, then AI HTTPModule will report the exception                if (filterContext.HttpContext.IsCustomErrorEnabled)                {                       var ai = new TelemetryClient();                    ai.TrackException(filterContext.Exception);                }             }            base.OnException(filterContext);        }    }
#endif}