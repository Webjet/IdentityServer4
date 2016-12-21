using AdminPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using AdminPortal.BusinessServices;

namespace AdminPortal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [HttpGet]
        [Authorize(Roles = "ServiceCenter,ServiceCenterManager,ProductTeam,Finance,Analytics,DevSupport")]
        public ActionResult Index()
        {
            //Previous Code with Model 'RoleBasedResourceItemMapper' 
            //return View(new RoleBasedResourceItemMapper());

            //Working - RnD
            return View(new ResourceItems(User));
        }
       
    }
}