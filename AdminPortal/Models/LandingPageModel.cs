using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using AdminPortal.BusinessServices;

namespace AdminPortal.Models
{
    public class LandingPageModel
    {
        public LandingPageModel()
        {
            LandingPageTabs = new List<LandingPageTab>();
        }
        
        public List<LandingPageTab> LandingPageTabs { get; set; }


    }
}