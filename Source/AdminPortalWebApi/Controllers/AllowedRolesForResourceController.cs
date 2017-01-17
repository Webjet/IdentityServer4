using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminPortal.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace AdminPortalWebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class AllowedRolesForResourceController : Controller
    {
        // GET: api/AllowedRolesForResource
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new ResourceToApplicationRolesMapper().GetAllowedRolesForResource("ReviewPendingBookings_WebjetAU");
        }
        [HttpGet("{resourceKey}")]
        public bool Get(string resourceKey)
        {
            return new ResourceToApplicationRolesMapper().IsUserRoleAllowedForResource(resourceKey, User);
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
