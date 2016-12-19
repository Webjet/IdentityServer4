using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdminPortal.Controllers
{
    [Authorize]
    public class GoogleBigQueryItineraryController : Controller
    {
        // GET: AnalyticsScripts
        [HttpGet]
        [Authorize(Roles = "Admin, ServiceCenter,ServiceCenterManager,Analytics,ProductTeam")]
        public ActionResult Index()
        {
            return View();
        }
    }
}