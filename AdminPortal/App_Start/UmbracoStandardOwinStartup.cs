using Microsoft.Owin;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Web;
using Umbraco.Web.Security.Identity;
using Umbraco.IdentityExtensions;
using AdminPortal;
using System.Configuration;
using System;

//To use this startup class, change the appSetting value in the web.config called 
// "owin:appStartup" to be "UmbracoStandardOwinStartup"

[assembly: OwinStartup("UmbracoStandardOwinStartup", typeof(UmbracoStandardOwinStartup))]

namespace AdminPortal
{
    /// <summary>
    /// The standard way to configure OWIN for Umbraco
    /// </summary>
    /// <remarks>
    /// The startup type is specified in appSettings under owin:appStartup - change it to "StandardUmbracoStartup" to use this class
    /// </remarks>
    public class UmbracoStandardOwinStartup : UmbracoDefaultOwinStartup
    {
        private readonly static string clientId = ConfigurationManager.AppSettings["ida:ClientId"]; // registered application id
        private readonly static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"]; // https://login.microsoftonline.com/{0}
        private readonly static string tenantURI = ConfigurationManager.AppSettings["ida:TenantURI"]; 
        private readonly static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"]; //Webjet AD endpoint
        private readonly static string authority = aadInstance + tenantId;
        private readonly static string loginUrl = ConfigurationManager.AppSettings["ida:PostLoginUrl"];

        public override void Configuration(IAppBuilder app)
        {
         base.Configuration(app);
            app.ConfigureBackOfficeAzureActiveDirectoryAuth(aadInstance, tenantURI, clientId, loginUrl, new Guid(tenantId));



        }
    }
}
