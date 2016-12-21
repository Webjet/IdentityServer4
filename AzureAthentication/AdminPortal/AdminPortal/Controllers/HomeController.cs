using AdminPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace AdminPortal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
       
        public ActionResult Index()
        {
            // var identity = (ClaimsIdentity)User.Identity;
            // IEnumerable<System.Security.Claims.Claim> claims = identity.Claims.Where(n => n.Value == "ServiceCenter");

            //Claim cl =   claims.ToList().Find(x => x.Value == "ServiceCenter");
            //RoleBasedResourceItemMapper mapper = new RoleBasedResourceItemMapper();
            //ViewData["RoleBasedResourceItemMapper"] = new RoleBasedResourceItemMapper();

           return View(new RoleBasedResourceItemMapper());
        }

    }
}