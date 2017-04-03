using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Principal;
using AdminPortal.BusinessServices;
using Newtonsoft.Json;

namespace AdminPortal
{
    public class AccessTokenDetails
    {
        public AccessTokenDetails()
        {

          
        }
        [JsonProperty("token")]
        public string AdminPortalAccessToken { get; set; }

        [JsonProperty("userEmailAddress")]
        public string UserEmailAddress { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("allowedResources")]
        public List<string> AllowedResources { get; set; }

    }
}
