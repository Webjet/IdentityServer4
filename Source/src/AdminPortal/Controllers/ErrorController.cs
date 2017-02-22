using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;
using System.Web.ModelBinding;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using Serilog;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace AdminPortal.Controllers
{
    public class ErrorController : Controller
    {
        static readonly Serilog.ILogger _logger = Log.ForContext<ErrorController>();

        // GET: /<controller>/
        [Route("/Error")]
        public IActionResult ShowError()
        {
            string errorMessage;
            string errorToLog;
            var error = HttpContext.Features.Get<IExceptionHandlerFeature>();

            if (error != null)
            {
                errorMessage = error.Error.Message;
                errorToLog = error.Error.ToString();
            }
            else
            {
                errorMessage = "No error message found for unhandled exception";
                errorToLog = errorMessage;
            }

            _logger.Error(errorToLog);
            
            ViewBag.ErrorMessage = errorMessage;

            return View();
        }

        // GET: /<controller>/
        [HttpGet("/Error/{statusCode}")]
        public IActionResult Index(int statusCode)
        {
            var httpResponseStatusCode = HttpContext.Response.StatusCode;
            var statusCodePagesFeature = HttpContext.Features.Get<IStatusCodePagesFeature>();

            if (!User.Identity.IsAuthenticated)
            {
                if (statusCodePagesFeature != null && httpResponseStatusCode == ((int)HttpStatusCode.Unauthorized))
                {
                    statusCodePagesFeature.Enabled = false;
                    return RedirectToAction("SignIn", "Account");
                }
            }

            return View(viewName: statusCode.ToString());

        }


    }
}
