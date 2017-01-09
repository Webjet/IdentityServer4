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


 