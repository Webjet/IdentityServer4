using System;
using System.Collections.Generic;
//The following libraries were added to this sample.
using System.Security.Claims;
using System.Web.Mvc;


//The following libraries were defined and added to this sample.



namespace AdminPortal.Controllers
{

    public class HomeController : Controller
    {
        /// <summary>
        /// Shows the generic MVC Get Started Home Page. Allows unauthenticated
        /// users to see the home page and click the sign-in link.
        /// </summary>
        /// <returns>Generic Home <see cref="View"/>.</returns>

        public ActionResult Index()
        {          
            return View();
        }

      }
}