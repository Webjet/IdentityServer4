using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebFormsOpenIdConnectAzureAD
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["RequestCounter_" + Session.Count] = Session.Count + 1;
            Global.LogSession(MethodBase.GetCurrentMethod().Name, sender);
        }
    }
}