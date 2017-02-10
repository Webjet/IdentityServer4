using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AdminPortal.BusinessServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
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

            try
            {
                //TODO: Serilog for SumoLogic, For test/development environment logging is done in EventViewer 
                //Reading Configuration for Serilog Sink from appsettings.json. Install Nuget package 'Serilog.Settings.Configuration'.
                Log.Logger = new LoggerConfiguration().ReadFrom.ConfigurationSection(Configuration.GetSection("Serilog")).CreateLogger();
            }
            catch (Exception ex)
            {

                Debug.Assert(false,ex.ToString());
            }
           

           
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
                SharedOptions => SharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
         
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //TODO: Will remove AddConsole, its is added by default.
           loggerFactory.AddConsole(Configuration.GetSection("Logging"));
           loggerFactory.AddDebug();

            try
            {
                //Adding Serilog log provider to logging pipeline for logging with configuration/settings from appsettings.json file, specially from the 'Serilog' configuartion section
                loggerFactory.AddSerilog();

            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
            }
          
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

            // Display friendly error pages for any non-success case
            // This will handle any situation where a status code is >= 400
            // and < 600, so long as no response body has already been generated
           
            //Unauthorised/access denied i.e. 401 are not handle by StatusCodePages middleware, for that we have used UseCookieAuthentication-AccessDeniedPath
            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.UseStaticFiles();
            
            //This tells the application that we want to store our session tokens in cookies 'UseCookieAuthentication'
            //Unauthorise request are handle by UseCookieAuthentication middleware by giving 'AccessDeniedPath' with explicit Http status code as 401
            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                AccessDeniedPath = new PathString("/Error/401")
            });

            //Authentication instructions.
            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                ClientId = Configuration["Authentication:AzureAd:ClientId"],
                Authority = Configuration["Authentication:AzureAd:AADInstance"] + Configuration["Authentication:AzureAd:TenantId"],
                CallbackPath = Configuration["Authentication:AzureAd:CallbackPath"]
                

                
            });
           
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            
        }
    }
}
