using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

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
            int httpStatusCode=HttpContext.Response.StatusCode;
            var statusCodePageFeature = HttpContext.Features.Get<IStatusCodePagesFeature>();
            
            return View(viewName: statusCode.ToString());
        }
    }
}
