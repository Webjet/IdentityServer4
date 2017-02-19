using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Protocols;
using WebFormsOpenIdConnectAzureAD.Models;
using Microsoft.Owin.Security;
using NLog;
using WebFormsOpenIdConnectAzureAD.AAD;
using WebFormsOpenIdConnectAzureAD.AAD.Common;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;

namespace WebFormsOpenIdConnectAzureAD
{
    public partial class Contact : Page
    {
        //private string adminPortalApiResourceId = ConfigurationManager.AppSettings["AdminPortalApi:ResourceId"];
        //private string adminPortalApiBaseAddress = ConfigurationManager.AppSettings["AdminPortalApi:BaseAddress"];
        //private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        //private static string appKey = ConfigurationManager.AppSettings["ida:AppKey"];

        private NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        private static string _brandRegion = ConfigurationManager.AppSettings["BrandRegionSuffix"];//ToDO change new SiteInfo().BrandRegion;//

   
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["RequestCounter_" + Session.Count] = Session.Count + 1;
            Global.LogSession(MethodBase.GetCurrentMethod().Name, sender);
            new ResourcePermissionsChecker().Authorise(this);
            //var attrib = new Authorize(SecurityAction.Demand);
            //attrib.ResourceKey = "ReviewPendingBookings";
            //var perm=attrib.CreatePermission() as PrincipalPermission;
            //Debug.Assert(perm !=null);
            //if (!perm.IsUnrestricted())
            //{   //http://stackoverflow.com/questions/217678/how-to-generate-an-401-error-programatically-in-an-asp-net-page/1627748#1627748
            //    throw new HttpException((int) HttpStatusCode.Forbidden, "No permissions to access " + attrib.ResourceKey);
            //    //Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            //    //Response.End();
            //}
        }

      
        public bool IsAuthorised(string securityKey)//, bool throwExceptionIfNoPermission=false)
        {
            var attrib = new Authorize(SecurityAction.Demand) { //ThrowExceptionIfNoPermission = throwExceptionIfNoPermission,
                ResourceKey = securityKey };
            var perm = attrib.CreatePermission() as PrincipalPermission;
            Debug.Assert(perm != null);
            return perm.IsUnrestricted();
        }


    }

    
}