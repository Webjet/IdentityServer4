using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AdminPortal.BusinessServices;
using AdminPortal.BusinessServices.Common;
using AdminPortal.BusinessServices.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Owin;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;

namespace AdminPortal
{
    //https://stormpath.com/blog/openid-connect-user-authentication-in-asp-net-core

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }
            builder.AddEnvironmentVariables();

         
            Configuration = builder.Build();

            AssemblyInformation.SetMainAssembly(Assembly.GetExecutingAssembly());


            //Configuration for Serilog Sink, setting in c sharp syntax for EventLog
            // Log.Logger =new LoggerConfiguration().WriteTo.EventLog("WebjetAdminPortal", manageEventSource: true).CreateLogger();

        }
        
        public IConfigurationRoot Configuration { get; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //http://stackoverflow.com/questions/30263681/asp-net-5-vnext-getting-a-configuration-setting
            services.AddSingleton<IConfigurationRoot>(sp => { return Configuration; });

            //https://blogs.msdn.microsoft.com/webdev/2014/06/17/dependency-injection-in-asp-net-vnext/
            //http://www.dotnetcurry.com/aspnet-mvc/1250/dependency-injection-aspnet-mvc-core
            services.TryAddSingleton<LandingPageLayoutLoader>();
            services.TryAddSingleton<ResourceToApplicationRolesMapper>();

            services.AddAuthentication(
            sharedOptions => sharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);

         
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //TODO: Will remove AddConsole, its is added by default.
            //Log.Logger = new LoggerConfiguration().ReadFrom.AppSettings().CreateLogger();
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            ConfigureSerilogSinks(loggerFactory);
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                // Handle unhandled errors i.e. HTTP 500
                app.UseExceptionHandler("/Error");
                
            }

            app.UseStaticFiles();
            
            //This tells the application that we want to store our session tokens in cookies 'UseCookieAuthentication'
            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = false,
            });
            //Unauthorise request are handle by UseCookieAuthentication middleware by giving 'AccessDeniedPath' with explicit Http status code as 401
            //app.UseCookieAuthentication(new CookieAuthenticationOptions()
            //{
            //    AccessDeniedPath = new PathString("/Error/401")    // By Adding StatusCodePagesWithReExecute at bottom to pipeline, able to get 401- AccessDenied error code.
            //});

             //Authentication instructions.
            //https://stormpath.com/blog/openid-connect-user-authentication-in-asp-net-core
            //https://joonasw.net/view/asp-net-core-1-azure-ad-authentication
            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                ClientId = Configuration["Authentication:AzureAd:ClientId"],
                Authority = Configuration["Authentication:AzureAd:AADInstance"] + Configuration["Authentication:AzureAd:TenantId"],
                CallbackPath = Configuration["Authentication:AzureAd:CallbackPath"],
                //ResponseType = OpenIdConnectResponseType.IdToken,
                AutomaticAuthenticate = false,
                AutomaticChallenge = true,
            });
            bool includeJwtBearerAuthentication = true;
            if (includeJwtBearerAuthentication)
            {
                //JwtBearerAuthentication is used to access WebAPI from client app
                app.UseJwtBearerAuthentication(new JwtBearerOptions
                {
                    AutomaticAuthenticate = false,
                    AutomaticChallenge = false,

                    Authority =Configuration["Authentication:AzureAd:AADInstance"] + Configuration["Authentication:AzureAd:TenantId"],
                    Audience = Configuration["Authentication:AzureAd:Audience"]
                });
            }
            //https://www.schaeflein.net/adal-v3-diagnostic-logging/
            LoggerCallbackHandler.Callback = new AdalLoggerCallback(loggerFactory.CreateLogger("AdalLoggerCallback"));

            // Display friendly error pages for any non-success case
            // This will handle any situation where a status code is >= 400
            // and < 600, so long as no response body has already been generated
            //TODO: For UnAuthenticated user-HttpStatus code is returned as 401
            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

        }

        private void ConfigureSerilogSinks(ILoggerFactory loggerFactory)
        {
            try
            {
                //Adding Serilog log provider to logging pipeline for logging with configuration/settings from appsettings.json file, specially from the 'Serilog' configuartion section
                loggerFactory.AddSerilog();

                //Reading Configuration for Serilog Sink from appsettings.json. Install Nuget package 'Serilog.Settings.Configuration'.
                Log.Logger = new LoggerConfiguration().ReadFrom.ConfigurationSection(Configuration.GetSection("Serilog")).CreateLogger();

                // Log.Logger = new LoggerConfiguration().WriteTo.SumoLogic(new Uri(collectorUrl)).CreateLogger();

            }
            catch 
            {
                throw;
                //Debug.Assert(false, ex.ToString());
            }
        }

        private void ConfigureSerilogSinksToEventViewer(ILoggerFactory loggerFactory)
        {
            try
            {
                //Adding Serilog log provider to logging pipeline for logging with configuration/settings from appsettings.json file, specially from the 'Serilog' configuartion section
                loggerFactory.AddSerilog();

                //Reading Configuration for Serilog Sink from appsettings.json. Install Nuget package 'Serilog.Settings.Configuration'.
                Log.Logger = new LoggerConfiguration().ReadFrom.ConfigurationSection(Configuration.GetSection("Serilog")).CreateLogger();

            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
            }
        }

    }
}
