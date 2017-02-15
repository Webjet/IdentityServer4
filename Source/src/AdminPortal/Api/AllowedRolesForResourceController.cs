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
using AdminPortal.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace AdminPortal
{
    [Authorize]
    [Route("api/[controller]")]
    public class AllowedRolesForResourceController : Controller
    {
        private readonly ResourceToApplicationRolesMapper _resourceToApplicationRolesMapper;

        public AllowedRolesForResourceController()
        {
            _resourceToApplicationRolesMapper = new ResourceToApplicationRolesMapper();
        }

        // GET: api/AllowedRolesForResource
        [HttpGet]
        public Dictionary<string, string[]> Get()
        {
            return _resourceToApplicationRolesMapper.ResourceItemsWithRoles;
        }

        [HttpGet("{resourceKey}")]
        public bool Get(string resourceKey)
        {
            bool result = _resourceToApplicationRolesMapper.IsUserRoleAllowedForResource(resourceKey, User);

            return result;

        }
        //Consider to rename to Dictionary<string, string[]> Get()
        [HttpGet]
        public List<string> GetResourcesForUser(string resourceKey)
        {
            var resources = _resourceToApplicationRolesMapper.GetAllowedForUserResources(User);

            return resources;

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