using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace AdminPortal.UnitTests.TestUtilities
{
    public static class PrincipalStubBuilder
    {
        public static IPrincipal GetUserWithServiceCenterAnalyticsAndFinanceRoles()
        {
            String[] loggedInUserRoles = {"ServiceCenter", "AnalyticsTeam", "FinanceTeam"};
            IPrincipal loggedInUser = new GenericPrincipal(new GenericIdentity("LoggedInUser"), loggedInUserRoles);
            return loggedInUser;
        }

      //  http://stackoverflow.com/questions/38557942/mocking-iprincipal-in-asp-net-core
        public static ClaimsPrincipal GetClaimPrincipalWithServiceCenterAnalyticsAndFinanceRoles()
        {
            ClaimsPrincipal user = new ClaimsPrincipal(GetUserWithServiceCenterAnalyticsAndFinanceRoles());
            return user;
        }

      
        public static IPrincipal GetUserWithAnalyticsAndFinanceRoles()
        {
            String[] loggedInUserRole = { "AnalyticsTeam", "FinanceTeam" };
            IPrincipal loggedInUser = new GenericPrincipal(new GenericIdentity("LoggedInUser"), loggedInUserRole);
            
            return loggedInUser;
        }
        public static IPrincipal GetUserWithDevRole()
        {
            String[] loggedInUserRoles = { "Developer" };
            IPrincipal loggedInUser = new GenericPrincipal(new GenericIdentity("LoggedInUser"), loggedInUserRoles);
            return loggedInUser;
        }


        public static ClaimsPrincipal GetClaimPrincipalWithServiceCenterRole()
        {
            String[] loggedInUserRole = { "ServiceCenter" };
        
            ClaimsIdentity  userIdentity = new ClaimsIdentity();
            userIdentity.AddClaim(new Claim("groups", "413687dc-ee0c-4326-9ae1-b2a87ebd28a1"));  //Service Center GroupId
            userIdentity.AddClaim(new Claim("aud", "43c42f66-e21c-4d89-a5ca-8a8ebc2be260"));

            ClaimsPrincipal loggedInUser = new GenericPrincipal(userIdentity, loggedInUserRole);
              var userClaims = ((ClaimsIdentity)loggedInUser.Identity).Claims;
              var groupId = userClaims.FirstOrDefault(c => c.Type == "groups")?.Value;
            return loggedInUser;
        }

        public static ClaimsPrincipal GetClaimPrincipalWithMarketingRole()
        {
            String[] loggedInUserRole = { "MarketingTeam" };

            ClaimsIdentity userIdentity = new ClaimsIdentity();
            userIdentity.AddClaim(new Claim("groups", "3721ff55-fe4c-4afd-96e3-5c06e7f93ccf")); //MarketingTeam Group Id
            userIdentity.AddClaim(new Claim("aud", "43c42f66-e21c-4d89-a5ca-8a8ebc2be260"));

            ClaimsPrincipal loggedInUser = new GenericPrincipal(userIdentity, loggedInUserRole);
            var userClaims = ((ClaimsIdentity)loggedInUser.Identity).Claims;
            var groupId = userClaims.FirstOrDefault(c => c.Type == "groups")?.Value;
            return loggedInUser;
        }
    }
}
