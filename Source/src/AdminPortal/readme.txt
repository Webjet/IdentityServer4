https://docs.microsoft.com/en-us/azure/app-service-mobile/app-service-mobile-how-to-configure-active-directory-authentication
https://www.asp.net/identity/overview/getting-started/developing-aspnet-apps-with-windows-azure-active-directory
http://stackoverflow.com/questions/29791557/why-azure-ad-fails-to-login-non-admins-in-multi-tenant-scenario

Claim based
https://msdn.microsoft.com/library/ff359102.aspx

To force all users (anonymous user or authenticated user) to redirect to Webjet Admin Portal via AAD login page only,
append ".auth/login/aad/callback" with application home page URL in Reply URLs setting value (www.portal.azure.com) 

Reply URLs - https://localhost/WebjetAdminPortal/.auth/login/aad/callback


Which Authorize Attribute 
http://stackoverflow.com/questions/19152109/system-web-http-authorize-versus-system-web-mvc-authorize

https://localhost/WebjetAdminPortal/api/allowedRolesForResource/ReviewPendingBookings_WebjetAU

https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/issues/144

//ASP.NET Core Web Application (.NET Framework) project template is missing System.Web.MVC dll
http://stackoverflow.com/questions/40912473/can-system-web-be-used-with-asp-net-core-with-full-framework
 http://stackoverflow.com/questions/38600865/asp-net-core-web-application-net-framework-project-template-is-missing-system
 http://stackoverflow.com/questions/38183649/net-core-1-0-visual-studio-referencing-external-dll

 //Package location in aspnet core
 http://stackoverflow.com/questions/40902578/wheres-the-package-location-in-aspnet-core
 http://stackoverflow.com/questions/35205092/net-core-and-nuget
 http://stackoverflow.com/questions/35089116/asp-net-core-1-0-dnx-packages-folder

 //Reference an existing .Net Frameowrk project in an ASP.NET Core 1.0 web app
 http://www.hanselman.com/blog/HowToReferenceAnExistingNETFrameworkProjectInAnASPNETCore10WebApp.aspx


 //npm dependency
 http://stackoverflow.com/questions/37949833/dependencies-not-installed-in-visual-studio

 //
 https://blog.elmah.io/config-transformations-in-aspnetcore/