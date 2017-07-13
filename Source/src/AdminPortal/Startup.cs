using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AdminPortal.BusinessServices;
using AdminPortal.BusinessServices.Common;
using AdminPortal.BusinessServices.GraphApiHelper;
using AdminPortal.BusinessServices.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
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
using Microsoft.Extensions.Caching.Redis;

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
            services.TryAddSingleton<GroupToTeamNameMapper>();
            services.AddSingleton<IActiveDirectoryGraphHelper, ActiveDirectoryGraphHelper>();   //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
            services.AddSingleton<ITeamLeadersRetrieval, TeamLeadersRetrieval>();

           
 //  https://www.jeffogata.com/asp-net-core-caching/
            //services.AddSingleton<IDistributedCache>(
            //        serviceProvider => new RedisCache(new RedisCacheOptions
            //        {
            //            Configuration = Configuration.GetConnectionString("RedisConnection"),
            //            InstanceName = "AccessTokenCache"
            //        })
            //    );

            //https://blogs.msdn.microsoft.com/luisdem/2016/09/06/azure-redis-cache-on-asp-net-core/
            //Configure to access Redis Cache service. It is done by AddDistributedRedisCache method, by DI in every part of the code that expects an instance object 
            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = Configuration.GetConnectionString("RedisConnection");
                option.InstanceName = "AccessTokenCache";
            });
            

            services.AddAuthentication(
            sharedOptions => sharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);



            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
       public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,IDistributedCache distributedCache)
        {
            //TODO: Will remove AddConsole, its is added by default.
            //Log.Logger = new LoggerConfiguration().ReadFrom.AppSettings().CreateLogger();
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            ConfigureSerilogSinks(loggerFactory);

            if (env.IsDevelopment())
            {
                loggerFactory.AddDebug();
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
                AutomaticChallenge = false
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
                ResponseType = OpenIdConnectResponseType.CodeIdToken,//IdToken,
                AutomaticAuthenticate = false,
                AutomaticChallenge = true,
  
                Events =new OpenIdConnectEvents()
                {
                    OnAuthorizationCodeReceived = AuthorizationCodeReceived
                  
                }
            });
            bool includeJwtBearerAuthentication = true;
            if (includeJwtBearerAuthentication)
            {
                //JwtBearerAuthentication is used to access WebAPI from client app
                app.UseJwtBearerAuthentication(new JwtBearerOptions
                {
                    AutomaticAuthenticate = false,
                    AutomaticChallenge = false,

                    Authority = Configuration["Authentication:AzureAd:AADInstance"] + Configuration["Authentication:AzureAd:TenantId"],
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
ResourceAuthorizeAttribute.ConfigurationRoot = this.Configuration;
            GroupToTeamNameMapper.ConfigurationRoot = this.Configuration;
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

        }

  private Task MessageReceived(MessageReceivedContext arg)
        {
            Log.Logger.Debug("MessageReceived");
            return Task.FromResult(0);
           
        }

        private Task AuthenticationFailed(AuthenticationFailedContext arg)
        {
            Log.Logger.Debug("AuthenticationFailed");
            return Task.FromResult(0);
        }

        private Task TokenResponseReceived(TokenResponseReceivedContext arg)
        {
            Log.Logger.Debug("TokenResponseReceived");
            return Task.FromResult(0);
        }

        private Task TokenValidated(TokenValidatedContext arg)
        {
            Log.Logger.Debug("TokenValidated");
            return Task.FromResult(0);
        }

        //https://dzimchuk.net/setting-up-your-aspnet-core-apps-and-services-for-azure-ad-b2c/
        //https://github.com/Azure-Samples/active-directory-dotnet-webapp-webapi-openidconnect-aspnetcore/blob/master/WebApp-WebAPI-OpenIdConnect-DotNet/Startup.cs
        private async Task AuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
        {
            var code = context.TokenEndpointRequest.Code;
            var clientId = Configuration["Authentication:AzureAd:ClientId"];
            var appKey = Configuration["Authentication:AzureAd:ClientSecret"];
            var graphResourceId = Configuration["Authentication:AzureAd:ResourceId"];
            var authority = Configuration["Authentication:AzureAd:AADInstance"] + Configuration["Authentication:AzureAd:TenantId"];
            ClientCredential credential = new ClientCredential(clientId, appKey);

            //string userObjectID = context.Ticket.Principal.FindFirst(
               // "http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            AuthenticationContext authContext = new AuthenticationContext(authority);  //(authority, new NaiveSessionCache(userObjectID)); // If Token Refersh is required. We might consider to use NaiveSessionCache 
            AuthenticationResult result = await authContext.AcquireTokenByAuthorizationCodeAsync(code, new Uri(context.TokenEndpointRequest.RedirectUri, UriKind.RelativeOrAbsolute), credential, graphResourceId);
            ActiveDirectoryGraphHelper.Token = result.AccessToken;
            context.HandleCodeRedemption();
         
        }
        private void ConfigureSerilogSinks(ILoggerFactory loggerFactory)
        {
            try
            {
                //Adding Serilog log provider to logging pipeline for logging with configuration/settings from appsettings.json file, specially from the 'Serilog' configuartion section
                loggerFactory.AddSerilog();
                var file = File.CreateText(@"Logs\SerilogInternalErrors.log");
                Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
                //Reading Configuration for Serilog Sink from appsettings.json. Install Nuget package 'Serilog.Settings.Configuration'.
                Log.Logger = new LoggerConfiguration().ReadFrom.ConfigurationSection(Configuration.GetSection("Serilog")).CreateLogger();
            }
            catch (Exception ex)
            {
                Serilog.Debugging.SelfLog.WriteLine(ex.ToString());
                // Serilog.Debugging.SelfLog.Out.Flush();
                Debug.Assert(false, ex.ToString());
            }
        }
    }
}
