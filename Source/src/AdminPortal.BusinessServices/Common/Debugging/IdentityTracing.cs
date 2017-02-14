using System.Diagnostics;
using System.Security.Claims;
using System.Text;

namespace AdminPortal.BusinessServices.Common.Debugging
{
    public static class IdentityTracing
    {
        public static string WriteClaims(this ClaimsPrincipal principal)
        {
            var sb=new StringBuilder();
            if (null != principal)
            {
                foreach (Claim claim in principal.Claims)
                {
                    sb.AppendLine("CLAIM TYPE: " + claim.Type + "; CLAIM VALUE: " + claim.Value );
                }
            }
            else
            {
                sb.Append("principal is null");
            }
            var outString = sb.ToString();
            Debug.WriteLine(outString);
            return outString;
        }
    }
}
