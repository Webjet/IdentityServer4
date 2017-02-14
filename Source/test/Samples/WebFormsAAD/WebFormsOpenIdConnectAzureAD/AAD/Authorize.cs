//It's prototype, the actual class is in TSA C:\GitRepos\tsa\main\TSA\Applications\WebjetTsa\Admin\AAD\Authorize.cs

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using WebFormsOpenIdConnectAzureAD.Models;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;
using System.Configuration;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using WebFormsOpenIdConnectAzureAD.AAD;

//using TSA.Applications.WebjetTsa;
//using TSA.BusinessEntities;

namespace WebFormsOpenIdConnectAzureAD
{
    public class Authorize : CodeAccessSecurityAttribute
    {
          public Authorize(SecurityAction action) : base(action)
        {
            Debug.Assert(HttpContext.Current!=null);
  //        _httpContext = HttpContext.Current;
            Global.LogSession(MethodBase.GetCurrentMethod().Name, this);
            ThrowExceptionIfNoPermission = true;
        }

        public bool ThrowExceptionIfNoPermission { get; set; }

        public string ResourceKey { get; set; }

        public override IPermission CreatePermission()
        {
            var resourcePermissionsChecker = new ResourcePermissionsChecker();
            bool isAllowed = resourcePermissionsChecker.IsResourceAllowed(ResourceKey);
            //string resourceKey = ResourceKey + "_" + _brandRegion;
            //Task<bool> task = Task.Run<bool>(async () => await GetIsResourceAllowedAsync(_httpContext, resourceKey));
            //bool isAllowed = task.Result;
            var permissionState = isAllowed ? PermissionState.Unrestricted : PermissionState.None;
            if (isAllowed == false && ThrowExceptionIfNoPermission)
            {
                resourcePermissionsChecker.ThrowException(ResourceKey);
            }
            return new PrincipalPermission(permissionState);
        }

       
      
    }
}