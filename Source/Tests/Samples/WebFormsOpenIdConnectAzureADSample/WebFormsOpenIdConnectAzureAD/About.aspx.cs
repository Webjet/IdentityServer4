using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebFormsOpenIdConnectAzureAD
{
    [PrincipalPermission(SecurityAction.Demand, Role = "Admin")]
    public partial class About : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}