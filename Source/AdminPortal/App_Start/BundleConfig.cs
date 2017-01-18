using System.Web;
using System.Web.Optimization;

namespace AdminPortal
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/css/Main").Include(
                      "~/Content/css/main.css"));

            bundles.Add(new ScriptBundle("~/Content/scripts/Main").Include(
                      "~/Content/scripts/webjet.ui.build.js",
                      "~/Content/scripts/admin.js"));
        }
    }
}
