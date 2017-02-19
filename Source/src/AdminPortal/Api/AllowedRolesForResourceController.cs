using System.Collections.Generic;
using AdminPortal.BusinessServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace AdminPortal.Api
{
    [Authorize(ActiveAuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class AllowedRolesForResourceController : Controller
    {
        static Serilog.ILogger _logger = Log.ForContext<AllowedRolesForResourceController>();

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
        [HttpGet("GetResourcesForUser")]
        public List<string> GetResourcesForUser()
        {
            var resources = _resourceToApplicationRolesMapper.GetAllowedForUserResources(User);
            _logger.Debug(resources.ToString());

            return resources;
        }
        [HttpGet("{resourceKey}")]
        public bool Get(string resourceKey)
        {
            bool result = _resourceToApplicationRolesMapper.IsUserRoleAllowedForResource(resourceKey, User);

            return result;
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