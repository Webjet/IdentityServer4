using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Hosting;
using AdminPortal.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace AdminPortalWebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class AllowedRolesForResourceController : Controller
    {
        //private var t = Directory.GetCurrentDirectory();
        private readonly string _filepath = Directory.GetCurrentDirectory() + "\\config\\ResourceToRolesMap.xml";

        // GET: api/AllowedRolesForResource
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new ResourceToApplicationRolesMapper().GetAllowedRolesForResource("ReviewPendingBookings_WebjetAU");
        }
        [HttpGet("{resourceKey}")]
        public object Get(string resourceKey)
        {
            bool result = new ResourceToApplicationRolesMapper(_filepath).IsUserRoleAllowedForResource(resourceKey, User);

            return new {isAllowed=result};
            
        }



#if INCLUDE_NOT_COVERED_BY_TESTS

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
#endif
    }
}
