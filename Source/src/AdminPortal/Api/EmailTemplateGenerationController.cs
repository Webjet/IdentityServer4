﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AdminPortal.BusinessServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;
using System.Web.Http;
using Microsoft.Azure.ActiveDirectory.GraphClient;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace AdminPortal.Api
{
   [Microsoft.AspNetCore.Authorization.Authorize(ActiveAuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    public class EmailTemplateGenerationController : Controller
    {
        static Serilog.ILogger _logger = Log.ForContext<EmailTemplateGenerationController>();

        private readonly ITeamLeadersRetrieval _teamLeadersRetrieval;
       

        public EmailTemplateGenerationController(TeamLeadersRetrieval teamLeadersRetrieval)
        {
            if (teamLeadersRetrieval == null)
            {
                _logger.Debug("Null TeamLeadersRetrieval. Check DI?");
            }
         
            _teamLeadersRetrieval = teamLeadersRetrieval; 
          
        }

        // GET: api/values
        [Microsoft.AspNetCore.Mvc.HttpGet("GetServiceCenterTeamLeadersEmailList")]
        public async Task<List<string>> GetServiceCenterTeamLeadersEmailList()
        {

            List<string> emaiList;
            try
            {
                emaiList = await _teamLeadersRetrieval.GetServiceCenterTeamLeaderEmailListAsync(User);

                if (emaiList != null)
                {
                    _logger.Debug("Email List: " + emaiList.ToString());
                }
                else
                {
                    _logger.Debug(" Null Email List of ServiceCenterManager");
                }
            }
            catch (HttpResponseException ex)
            {
                _logger.Debug("Exception logged for GetServiceCenterTeamLeadersEmailList. " + ex.ToString() + " Logged In User: " +  User.Identity.Name);
                throw;
            }

            catch (Exception ex)
            {
                _logger.Debug("Exception logged for GetServiceCenterTeamLeadersEmailList. " + ex.ToString() + " Logged In User: " + User.Identity.Name);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
            return emaiList;
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
