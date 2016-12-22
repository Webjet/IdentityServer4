using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using AdminPortal.BusinessServices;

namespace AdminPortal.Models
{
    public class ResourceItems
    {
        private readonly IPrincipal _loggedUser;

        public ResourceItems(IPrincipal user)
        {
            _loggedUser = user;
            UiLinkMenuItems = new List<LandingPageTab>();
        }
        public bool IsResourceAllowedForUserRole(string resourceKey)
        {
            List<string> roles = new ResourceToApplicationRolesMapper().GetAllowedRolesForResource(resourceKey);
            return roles.Any(role => _loggedUser.IsInRole(role));
        }


        public List<LandingPageTab> UiLinkMenuItems { get; set; }

       
    }
}