using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace AdminPortal.Controllers
{
    public class ErrorController : Controller
    {
        // GET: /<controller>/
        [Route("/Error")]
        public IActionResult ShowError(string errorMessage, string signIn)
        {
            ViewBag.SignIn = signIn;
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
