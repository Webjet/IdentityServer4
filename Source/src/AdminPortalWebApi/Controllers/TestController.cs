using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPortalWebApi.Controllers
{
   
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        // GET: api/Test
        //[HttpGet]
        //public string  Get()
        //{
        //    DebugApplicationFolderPath();
        //    return "Anonymous response" ;

        //}
        // GET: api/Test
        [HttpGet]
        public bool Get()
        {
             return true;

        }
        // GET api/Test/{id}
        //  [Authorize]
        [HttpGet("GetStringList")]
        public List<String> GetStringList()
        {
            return new List<String>{"Role1","Role2","Role3"};
        }

        [HttpGet("{id}")]
        public string Get(string id)
        {
            return "Authorized response " + id;
        }

        //// POST api/values
        //[HttpPost]
        //public void Post([FromBody]string value)
        //{
        //    // For more information on protecting this API from Cross Site Request Forgery (CSRF) attacks, see http://go.microsoft.com/fwlink/?LinkID=717803
        //}

        //// PUT api/values/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{
        //    // For more information on protecting this API from Cross Site Request Forgery (CSRF) attacks, see http://go.microsoft.com/fwlink/?LinkID=717803
        //}

        //// DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //    // For more information on protecting this API from Cross Site Request Forgery (CSRF) attacks, see http://go.microsoft.com/fwlink/?LinkID=717803
        //}
    }
}
