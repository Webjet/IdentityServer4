using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Umbraco.Core;
using Umbraco.Web.Security.Identity;
using Microsoft.Owin.Security.OpenIdConnect;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;

namespace AdminPortal
{
    public static class UmbracoADAuthExtensions
    {

        ///  <summary>
        ///  Configure ActiveDirectory sign-in
        ///  </summary>
        ///  <param name="app"></param>
        ///  <param name="tenant"> Webjet AD endpoints</param>
        ///  <param name="clientId">Registered application's AppID from AD</param>
        ///  <param name="postLoginRedirectUri">
        ///  The URL that will be redirected to after login is successful, example: http://mydomain.com/umbraco/;
        ///  </param>
        ///  <param name="issuerId">
        /// 
        ///  This is the "Issuer Id" for you Azure AD application. This a GUID value and can be found
        ///  in the Azure portal when viewing your configured application and clicking on 'View endpoints'
        ///  which will list all of the API endpoints. Each endpoint will contain a GUID value, this is
        ///  the Issuer Id which must be used for this value.        
        /// 
        ///  If this value is not set correctly then accounts won't be able to be detected 
        ///  for un-linking in the back office. 
        /// 
        ///  </param>
        /// <param name="caption"></param>
        /// <param name="style"></param>
        /// <param name="icon"></param>
        /// <remarks>
        ///  ActiveDirectory account documentation for ASP.Net Identity can be found:
        ///  https://github.com/AzureADSamples/WebApp-WebAPI-OpenIDConnect-DotNet
        ///  </remarks>
        public static void ConfigureBackOfficeAzureActiveDirectoryAuth(this IAppBuilder app, string aadInstance,
            string tenant, string clientId, string postLoginRedirectUri, Guid issuerId,
            string caption = "Active Directory", string style = "btn-microsoft", string icon = "fa-windows")
        {
            
            var authority = string.Format(
                CultureInfo.InvariantCulture,
                aadInstance,
                tenant);

            var adOptions = new OpenIdConnectAuthenticationOptions
            {
                SignInAsAuthenticationType = Constants.Security.BackOfficeExternalAuthenticationType,
                ClientId = clientId,
                Authority = authority,
                RedirectUri = postLoginRedirectUri,
               
            };

            adOptions.ForUmbracoBackOffice(style, icon);
            adOptions.Caption =caption;

            //Need to set the auth tyep as the issuer path
            adOptions.AuthenticationType = string.Format(
                CultureInfo.InvariantCulture,
                "https://sts.windows.net/{0}/",
                issuerId);

            app.UseOpenIdConnectAuthentication(adOptions);            
        }    
        

    //public static void ConfigureBackOfficeAzureActiveDirectoryAuth(this IAppBuilder app, string aadInstance,
    //       string tenant, string clientId, string postLoginRedirectUri, Guid issuerId,
    //       string caption = "Active Directory", string style = "btn-microsoft", string icon = "fa-windows")
    //{

    //        var authority = string.Format(
    //            CultureInfo.InvariantCulture,
    //            "https://login.windows.net/{0}",
    //            tenant);

    //        var adOptions = new OpenIdConnectAuthenticationOptions
    //        {
    //            SignInAsAuthenticationType = Constants.Security.BackOfficeExternalAuthenticationType,
    //            ClientId = clientId,
    //            Authority = authority
    //        };

    //        adOptions.ForUmbracoBackOffice(style, icon);
    //        adOptions.Caption = caption;
    //        app.UseOpenIdConnectAuthentication(adOptions);
    //    }

}

    ////Added
    //adOptions.Notifications = new OpenIdConnectAuthenticationNotifications()
    //{
    //    AuthorizationCodeReceived = async context =>
    //    {
    //        var userService = ApplicationContext.Current.Services.UserService;

    //        var email = context.JwtSecurityToken.Claims.First(x => x.Type == "email").Value;
    //        var issuer = context.JwtSecurityToken.Claims.First(x => x.Type == "iss").Value;
    //        var providerKey = context.JwtSecurityToken.Claims.First(x => x.Type == "sub").Value;
    //        var name = context.JwtSecurityToken.Claims.First(x => x.Type == "name").Value;
    //        var userManager = context.OwinContext.GetUserManager<BackOfficeUserManager>();

    //        var user = userService.GetByEmail(email);

    //        if (user == null)
    //        {
    //            var writerUserType = userService.GetUserTypeByName("writer");
    //            user = userService.CreateUserWithIdentity(email, email, writerUserType);
    //        }

    //        var identity = await userManager.FindByEmailAsync(email);
    //        if (identity.Logins.All(x => x.ProviderKey != providerKey))
    //        {
    //            identity.Logins.Add(new IdentityUserLogin(issuer, providerKey, user.Id));
    //            identity.Name = name;
    //            await userManager.UpdateAsync(identity);
    //        }
    //    }
    //};

}