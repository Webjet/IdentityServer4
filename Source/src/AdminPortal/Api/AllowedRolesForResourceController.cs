using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AdminPortal.BusinessServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;
using System.Linq;

namespace AdminPortal.Api
{
    [Microsoft.AspNetCore.Authorization.Authorize(ActiveAuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    public class AllowedRolesForResourceController : Controller
    {
        static Serilog.ILogger _logger = Log.ForContext<AllowedRolesForResourceController>();

        private readonly ResourceToApplicationRolesMapper _resourceToApplicationRolesMapper;
        private readonly GroupToTeamNameMapper groupToTeamNameMapper;
        public const string ScopeClaim = "Scope";
        public const string TeamNameClaim = "TeamName";

        public AllowedRolesForResourceController(ResourceToApplicationRolesMapper resourceToApplicationRolesMapper = null,
            GroupToTeamNameMapper groupToTeamNameMapper = null)
        {
            _resourceToApplicationRolesMapper = resourceToApplicationRolesMapper ?? new ResourceToApplicationRolesMapper();
            this.groupToTeamNameMapper = groupToTeamNameMapper ?? new GroupToTeamNameMapper();
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
               
        [Microsoft.AspNetCore.Mvc.HttpGet("GetTeamNameForUser")]
        public string GetTeamNameForUser()
        {
            var usrClaims = ((ClaimsIdentity)User?.Identity)?.Claims;

            //TODO: Need to check with Alvin? User can belong to multiple Groups
            var groupIds = usrClaims
                ?.Where(c => c.Type == "groups")?.Select(c => c.Value);

            return this.groupToTeamNameMapper.GetTeamGroup(groupIds)?.TeamName;   
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
            string customeMessage = "TEST EXCEPTION for troubleshooting ";
            HttpResponseMessage message = new HttpResponseMessage();
            message.StatusCode = HttpStatusCode.InternalServerError;
            message.ReasonPhrase = customeMessage;
            //throw new HttpResponseException(message);

            throw new Exception(customeMessage);
            //throw new HttpResponseException(HttpStatusCode.InternalServerError);
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