using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdminPortal.Controllers
{
    public class ReviewPendingBookingsController : Controller
    {
        // GET: ReviewPendingBookings
        public ActionResult Index()
        {
            return View();
        }
    }
}