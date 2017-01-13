using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdminPortal.Controllers
{
    public class ViewHtmlController : Controller
    {
        // GET: AnalyticsScripts
        [Authorize("GoogleBigQueryItinerary")]
        [HttpGet]
        public ActionResult GoogleBigQueryItinerary()
        {
            ViewData["GoogleBigQueryHostUrl"] = ConfigurationManager.AppSettings["GoogleBigQueryHostUrl"];
            return View();
        }
        [Authorize("GenerateRandomNumber")]
        [HttpGet]
        public ActionResult GenerateRandomNumber()
        {
            var key = "GenerateRandomNumber";//TODO use reflection or Fody to generate
            ViewData["PageName"] =key+".html";
            return View("Index");
        }

    }
}