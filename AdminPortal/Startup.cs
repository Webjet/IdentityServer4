using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using AdminPortal.Utils;
using Microsoft.Owin.Host.SystemWeb;

[assembly: OwinStartup(typeof(AdminPortal.Startup))]

namespace AdminPortal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
