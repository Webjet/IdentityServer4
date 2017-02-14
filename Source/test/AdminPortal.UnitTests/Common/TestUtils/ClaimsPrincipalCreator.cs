using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace TSA.TestsCommon.TestUtilities
{
    public class ClaimsPrincipalCreator
    {
#if HTTP_HELPER_AVAILABLE
        public static void CreateClaimsPrincipalAsHttpContextUser()
        {
            HttpContext.Current = HttpHelper.FakeHttpContext();
            HttpContext.Current.User = CreateClaimsPrincipal();
            //ClaimsPrincipal.Current will be assigned as well;
        }
#endif //HTTP_HELPER_AVAILABLE

        //Consider to move to common
        public static ClaimsPrincipal CreateClaimsPrincipal()
        { //http://stackoverflow.com/questions/38323895/how-to-add-claims-in-a-mock-claimsprincipal/38325059#38325059
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, "userId"),
                new Claim(ClaimTypes.GivenName, "John"),
                new Claim(ClaimTypes.Surname, "Doe"),
                new Claim(ClaimTypes.Email, "emailAddress"),
                new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", "userObjectId")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            Thread.CurrentPrincipal = claimsPrincipal;
            return claimsPrincipal;
        }
    }
}
