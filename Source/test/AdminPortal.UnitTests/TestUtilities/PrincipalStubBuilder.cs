using System;
using System.Collections.Generic;
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
    }
}
