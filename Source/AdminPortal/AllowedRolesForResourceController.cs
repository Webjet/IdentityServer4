using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AdminPortal.BusinessServices;

namespace AdminPortal
{
    public class AllowedRolesForResourceController : ApiController
    {


        // GET api/<controller>/ReviewPendingBookings_WebjetAU
        public IEnumerable<string> Get()
        {
            return new ResourceToApplicationRolesMapper().GetAllowedRolesForResource("ReviewPendingBookings_WebjetAU");
        }

        // GET api/<controller>/ReviewPendingBookings_WebjetAU
        public IEnumerable<string> Get(string resourceKey)
        {
            return new ResourceToApplicationRolesMapper().GetAllowedRolesForResource(resourceKey);
        }

        

#if FUTURE
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
#endif
    }
}