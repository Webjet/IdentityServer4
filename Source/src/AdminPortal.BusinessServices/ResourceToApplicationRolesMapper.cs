using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Xml;
using System.Security.Principal;
using NLog;
using System.Xml.Serialization;
using AdminPortal.BusinessServices.Common;
using AdminPortal.BusinessServices.Common.Debugging;
using Microsoft.Extensions.Configuration;
using Webjet.Common;
using Webjet.Common.Common;
using Webjet.Common.Strings;


namespace AdminPortal.BusinessServices
{
    public class ResourceToApplicationRolesMapper
    {
        private IConfigurationRoot _config;

        private string AppConfigFilePath
        {

            get
            {
                //DebugEnvironmentSettings.DebugApplicationFolderPath();

                //DI configuration
                //var relPath = _config["ResourceToRolesMapRelativePath"] ?? @"config\ResourceToRolesMap.xml";

                string relPath = _config?["ResourceToRolesMapRelativePath"];
                //if (relPath.IsNullOrBlank()) //Call from "AdminPortal\ResourceAuthorizeAttribute"
                //{
                //    relPath = @"config\ResourceToRolesMap.xml";
                //}

                //relPath is NullOrBlank -> ArgumentNullException will be thrown? Need to discuss with MF
                return Path.Combine(WebApplicationHelper.WebApplicationRootDirectory(), relPath);

            }
        }

        private readonly string _filepath = null;

        public Dictionary<string, string[]> ResourceItemsWithRoles { get; set; }

        public ResourceToApplicationRolesMapper(IConfigurationRoot appConfig = null, string filepath = null)
        {
            //_config = config;
            //_filepath = !string.IsNullOrEmpty(filepath) ? filepath : DefaultConfigFilePath;

            _config = appConfig;
            _filepath = appConfig != null ? AppConfigFilePath : filepath;

            ParseXmlToObject();
        }

        private void ParseXmlToObject()
        {
            string xml = StreamHelper.FileToString(_filepath);
            ResourceToApplicationRolesMap map = xml.ParseXml<ResourceToApplicationRolesMap>();
            var validator = new ResourceIdsConfigValidator();

            var errorMessage = validator.Validate(map, _filepath);
            if (!errorMessage.IsNullOrBlank())
            {
                throw new ApplicationException(errorMessage);
            }
            ResourceItemsWithRoles = new Dictionary<string, string[]>(StringComparer.InvariantCultureIgnoreCase);
            foreach (ResourceToApplicationRolesMapResourceToRoles resourceToRolesItem in map.ResourceToRoles)
            {
                ResourceItemsWithRoles.Add(resourceToRolesItem.ResourceId, resourceToRolesItem.Roles.Split(','));
            }
        }

        public List<string> GetAllowedRolesForResource(string resourceKey)
        {
            if (ResourceItemsWithRoles != null && ResourceItemsWithRoles.ContainsKey(resourceKey))
            {
                return ResourceItemsWithRoles[resourceKey].ToList();
            }
            return null;
        }

        public bool IsUserRoleAllowedForResource(string resourceKey, IPrincipal loggedUser)
        {
            List<string> roles = GetAllowedRolesForResource(resourceKey);
            bool isAllowed = false;
            if (roles != null && loggedUser != null)
            {
                isAllowed = roles.Any(role => loggedUser.IsInRole(role));
            }
            return isAllowed;
        }

        public List<string> GetAllowedForUserResources(IPrincipal loggedUser)
        {
            return (from kvp in ResourceItemsWithRoles where IsUserRoleAllowedForResource(kvp.Key, loggedUser) select kvp.Key).ToList();
        }
    }

}