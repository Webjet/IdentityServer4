using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using AdminPortal.BusinessServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace AdminPortal.Api
{
    [Microsoft.AspNetCore.Authorization.Authorize(ActiveAuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    public class AllowedRolesForResourceController : Controller
    {
        static Serilog.ILogger _logger = Log.ForContext<AllowedRolesForResourceController>();

        private readonly ResourceToApplicationRolesMapper _resourceToApplicationRolesMapper;

        public AllowedRolesForResourceController(ResourceToApplicationRolesMapper resourceToApplicationRolesMapper = null)
        {
            _resourceToApplicationRolesMapper = resourceToApplicationRolesMapper ?? new ResourceToApplicationRolesMapper();
        }

        // GET: api/AllowedRolesForResource
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public Dictionary<string, string[]> Get()
        {
            return _resourceToApplicationRolesMapper.ResourceItemsWithRoles;
        }

        [Microsoft.AspNetCore.Mvc.HttpGet("GetResourcesForUser")]
        public List<string> GetResourcesForUser()
        {
            var resources = _resourceToApplicationRolesMapper.GetAllowedForUserResources(User);
            _logger.Debug(resources.ToString());

            return resources;
        }

        [Microsoft.AspNetCore.Mvc.HttpGet("{resourceKey}")]
        public bool Get(string resourceKey)
        {
            bool result = _resourceToApplicationRolesMapper.IsUserRoleAllowedForResource(resourceKey, User);

            return result;
        }
        [Microsoft.AspNetCore.Mvc.HttpGet("GenerateInternalServerError")]
        public void GenerateInternalServerError()
        {
            throw new HttpResponseException(HttpStatusCode.InternalServerError);
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