using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminPortal.BusinessServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security .Claims ;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace AdminPortal.Api
{
    [Microsoft.AspNetCore.Authorization.Authorize(ActiveAuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class EmailTemplateGenerationController : Controller
    {
        static Serilog.ILogger _logger = Log.ForContext<AllowedRolesForResourceController>();

        private readonly TeamLeadersRetrival _teamLeadersRetrival;

        public EmailTemplateGenerationController(TeamLeadersRetrival teamLeadersRetrival)
        {
            _teamLeadersRetrival = teamLeadersRetrival ?? new TeamLeadersRetrival();
        }

        // GET: api/values
        [HttpGet]
        public async Task<IEnumerable<string>> GetServiceCenterTeamLeadersEmailList()
        {
            var userClaims = ((ClaimsIdentity)User.Identity).Claims;

           List<string> emaiList = await _teamLeadersRetrival.GetServiceCenterTeamLeaderEmaiListAsync(User);
            return new string[] { "value1", "value2" };
        }

#if INCLUDE_NOT_COVERED_BY_TESTS
        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

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
