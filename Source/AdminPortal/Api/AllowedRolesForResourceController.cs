using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using AdminPortal.BusinessServices;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AdminPortal
{
    /* Moved to separate project WEB.API Project C:\GitRepos\AdminPortal\Source\AdminPortalWebApi\Controllers\AllowedRolesForResourceController.cs. 
     * May be merged back later
        //http://stackoverflow.com/questions/19152109/system-web-http-authorize-versus-system-web-mvc-authorize
        [System.Web.Http.Authorize()]
        public class AllowedRolesForResourceController : ApiController
        {

            //// GET api/<controller>/ReviewPendingBookings_WebjetAU
            //public IEnumerable<string> Get(string resourceKey)
            //{
            //    return new ResourceToApplicationRolesMapper().GetAllowedRolesForResource(resourceKey);
            //}

            public bool Get(string resourceKey)
            {
                return new ResourceToApplicationRolesMapper().IsUserRoleAllowedForResource(resourceKey, User);
            }

    #if INCLUDE_NOT_COVERED_BY_TESTS
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
        */
}