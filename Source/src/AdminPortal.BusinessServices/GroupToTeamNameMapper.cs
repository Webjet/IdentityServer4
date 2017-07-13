using AdminPortal.BusinessServices.Common;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Webjet.Common.Common;

namespace AdminPortal.BusinessServices
{
    public class GroupToTeamNameMapper
    {
        private IConfigurationRoot _config;
        static ILogger _logger = Log.ForContext<GroupToTeamNameMapper>();

        public static IConfigurationRoot ConfigurationRoot
        {
            private get; set;
        }

        private string AppConfigFilePath
        {
            get
            {
                string relPath = _config?["GroupToTeamNameMapRelativePath"];               
                return Path.Combine(WebApplicationHelper.WebApplicationRootDirectory(), relPath);
            }
        }

        private readonly string _filepath = null;

        private readonly GroupToTeamNameMap _map;

        public GroupToTeamNameMapper(string filepath = null)
        {            
            _config = ConfigurationRoot;
            _filepath = ConfigurationRoot != null ? AppConfigFilePath : filepath;

            string xml = StreamHelper.FileToString(_filepath);
            _map = xml.ParseXml<GroupToTeamNameMap>();
            
        }
       
        public GroupToTeamNameMapGroupToTeamName GetTeamGroup(IEnumerable<string> groupIds)
        {
            int count = 0;
            GroupToTeamNameMapGroupToTeamName foundGroup = null;

            foreach (var groupId in groupIds)
            {
                var group = _map.GroupToTeamName?.FirstOrDefault(c => c.GroupId == groupId);                         

                if (group != null)
                {
                    count++;
                    foundGroup = group;                   
                }                                
            }

            if (count > 1)
            {
                _logger.Warning("User belongs to more than one team group");
            }

            return foundGroup;          
        }
    }
}
