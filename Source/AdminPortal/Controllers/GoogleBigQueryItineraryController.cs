using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdminPortal.Controllers
{
    [Authorize("GoogleBigQueryItinerary")]
    public class GoogleBigQueryItineraryController : Controller
    {
        // GET: AnalyticsScripts
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}