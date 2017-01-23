using System.Web;
using System.Web.Mvc;

namespace AdminPortal
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
 #if INCLUDE_NOT_COVERED_BY_TESTS
//old filters.Add(new HandleErrorAttribute());
            filters.Add(new ErrorHandler.AiHandleErrorAttribute());
#endif
        }
    }
}
