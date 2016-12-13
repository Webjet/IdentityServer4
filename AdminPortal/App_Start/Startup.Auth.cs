//The following libraries were defined and added to this sample.
using AdminPortal.Utils;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using Microsoft.Owin.Host.SystemWeb;
using System.Globalization;
//The following libraries were added to this sample.
using System.Threading.Tasks;


namespace AdminPortal
{
    public partial class Startup
    {
        /// <summary>
        /// Configures OpenIDConnect Authentication & Adds Custom Application Authorization Logic on User Login.
        /// </summary>
        /// <param name="app">The application represented by a <see cref="IAppBuilder"/> object.</param>
        public void ConfigureAuth(IAppBuilder app)
        {
            // For single tenant
            string aadInstance =ConfigHelper.AadInstance;
            string tenant = ConfigHelper.Tenant;
            var authority = string.Format(
              CultureInfo.InvariantCulture,
              aadInstance,
              tenant);

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            //Configure OpenIDConnect, register callbacks for OpenIDConnect Notifications
            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = ConfigHelper.ClientId,
                    //Authority = String.Format(CultureInfo.InvariantCulture, ConfigHelper.AadInstance, ConfigHelper.Tenant), // For Single-Tenant
                    // Authority = ConfigHelper.CommonAuthority, // For Multi-Tenant
                    Authority = authority,  // For single tenant
                    PostLogoutRedirectUri = ConfigHelper.PostLogoutRedirectUri,

                    // Here, we've disabled issuer validation for the multi-tenant sample.  This enables users
                    // from ANY tenant to sign into the application (solely for the purposes of allowing the sample
                    // to be run out-of-the-box.  For a real multi-tenant app, reference the issuer validation in 
                    // WebApp-MultiTenant-OpenIDConnect-DotNet.  If you're running this sample as a single-tenant
                    // app, you can delete the ValidateIssuer property below.
                    TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
                    {
                        //ValidateIssuer = false, // For Multi-Tenant Only
                        RoleClaimType = "roles",
                    },

                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        AuthenticationFailed = context =>
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/Error/ShowError?signIn=true&errorMessage=" + context.Exception.Message);
                            return Task.FromResult(0);
                        }
                    }
                });
        }
    }
}