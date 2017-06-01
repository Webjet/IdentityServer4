using AdminPortal.BusinessServices.Common;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;
using Webjet.Common.Common;

namespace AdminPortal.BusinessServices
{
    public class GroupToTeamNameMapper
    {
        private IConfigurationRoot _config;

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

        private readonly GroupToTeamNameMap map;

        public GroupToTeamNameMapper(string filepath = null)
        {            
            _config = ConfigurationRoot;
            _filepath = ConfigurationRoot != null ? AppConfigFilePath : filepath;

            string xml = StreamHelper.FileToString(_filepath);
            this.map = xml.ParseXml<GroupToTeamNameMap>();
            
        }
       
        public string GetTeamName(string groupId)
        {
            return map.GroupToTeamName.FirstOrDefault(c => c.GroupId == groupId)?.TeamName;
        }
    }
}
