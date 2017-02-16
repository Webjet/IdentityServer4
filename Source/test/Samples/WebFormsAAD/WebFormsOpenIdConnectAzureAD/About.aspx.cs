using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebFormsOpenIdConnectAzureAD
{
    [Authorize(SecurityAction.Demand, ResourceKey = "ReviewPendingBookings")]
    public partial class About : Page
    {
        protected About() : base()
        {
            Global.LogSession(MethodBase.GetCurrentMethod().Name, this);
        }
        [Authorize(SecurityAction.Demand, ResourceKey = "About Page_Load")]
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["RequestCounter_" + Session.Count] = Session.Count + 1;
            Global.LogSession(MethodBase.GetCurrentMethod().Name, sender);
        }
    }
}