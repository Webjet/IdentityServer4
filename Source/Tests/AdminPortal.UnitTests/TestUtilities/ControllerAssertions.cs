using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FluentAssertions;
using NSubstitute;

namespace AdminPortal.UnitTests.TestUtilities
{
    public class ControllerAssertions
    {
        public static T ArrangeController<T>() where T : Controller
        {
            var httpContext = Substitute.For<HttpContextBase>();
            httpContext.User = PrincipalStubBuilder.GetUserWithServiceCenterAnalyticsAndFinanceRoles();

            var controller = (T)Activator.CreateInstance(typeof(T)); // to add args , see  http://stackoverflow.com/questions/840261/passing-arguments-to-c-sharp-generic-new-of-templated-type
            controller.ControllerContext = new ControllerContext()
            {
                Controller = controller,
                RequestContext = new RequestContext(httpContext, new RouteData())
            };
            return controller;
        }
        public  static void AssertViewWithViewData(ViewResult result, string viewName, string viewDataKey, string viewDataValue)
        {
            result.Should().NotBeNull();
            result.ViewName.Should().Be(viewName);
            result.ViewData.Count().Should().Be(1);
            result.ViewData.ContainsKey(viewDataKey).Should().BeTrue();
            result.ViewData[viewDataKey].Should().Be(viewDataValue);
        }
    }
}
