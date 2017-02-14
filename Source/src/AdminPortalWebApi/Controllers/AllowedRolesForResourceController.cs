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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.PlatformAbstractions;
using Serilog;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace AdminPortalWebApi.Controllers
{
    [Authorize]
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
