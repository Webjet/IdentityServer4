﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using AdminPortal.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace AdminPortal
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class ResourceAuthorizeAttribute : AuthorizeAttribute
    {
        public static IConfigurationRoot ConfigurationRoot
        {
            private get;set;
        }
        public ResourceAuthorizeAttribute(string resourceKey)
        {
            base.Roles = GetAllowedRolesForResource(resourceKey);
            
        }

        private string GetAllowedRolesForResource(string resourceKey)
        {
            string allowedRoles;
            if (!string.IsNullOrEmpty(resourceKey))
            {
                 allowedRoles = string.Join(",", new ResourceToApplicationRolesMapper(ConfigurationRoot).GetAllowedRolesForResource(resourceKey));
                return allowedRoles;
                
            }
            return null;
        }
    }

    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    //public class AdminPortalAuthorize : System.Web.Mvc.AuthorizeAttribute
    //{
    //    private readonly string _resourceKey;

    //    public AdminPortalAuthorize() : this(null)
    //    {

    //    }
    //    public AdminPortalAuthorize(string resourceKey)
    //    {
    //        this._resourceKey = resourceKey;
    //    }

    //    /// <summary>
    //    /// By Default, MVC returns a 401 Unauthorized when a user's roles do not meet the AuthorizeAttribute requirements.
    //    /// This initializes a reauthentication request to our identity provider.  Since the user is already logged in, 
    //    /// AAD returns to the same page, which then issues another 401, creating a redirect loop.
    //    /// Here, we override the AuthorizeAttribute's HandleUnauthorizedRequest method to show something that makes 
    //    /// sense in the context of our application.
    //    /// </summary>
    //    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    //    {
    //        if (filterContext.HttpContext.Request.IsAuthenticated)
    //        {
    //            //One Strategy:
    //            //filterContext.Result = new System.Web.Mvc.HttpStatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);

    //            //Another Strategy:
    //            filterContext.Result = new RedirectToRouteResult(
    //                new RouteValueDictionary(
    //                    new
    //                    {
    //                        controller = "Error",
    //                        action = "ShowError",
    //                        errorMessage = "You do not have sufficient priviliges to view this page."
    //                    })
    //                );
    //        }
    //        else
    //        {
    //            base.HandleUnauthorizedRequest(filterContext);
    //        }
    //    }

    //    protected override bool AuthorizeCore(HttpContextBase httpContext)
    //    {
    //        if (!httpContext.Request.IsAuthenticated)
    //            return false;

    //        if (!string.IsNullOrEmpty(_resourceKey))
    //        {
    //            Roles  = string.Join(",", new ResourceToApplicationRolesMapper().GetAllowedRolesForResource(_resourceKey));
    //        }

    //        return base.AuthorizeCore(httpContext);
    //    }

    //}
}