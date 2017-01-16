using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using AdminPortal.BusinessServices;
using AdminPortal.BusinessServices.LandingPage;

namespace AdminPortal.Models
{
    public class LandingPageModel
    {
        public LandingPageModel()
        {
           
        }
        
        public List<UiLinksLandingPageTab> LandingPageTabs { get; set; } 

        
        
    }
}