using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Web;
using System.Web.Hosting;
using System.Xml;
using System.Web.Mvc;
using System.Security.Principal;
using Microsoft.SDC.Common;
using NLog;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace AdminPortal.BusinessServices
{
    public class ResourceToApplicationRolesMapper
    {
        private readonly string _filepath = HostingEnvironment.ApplicationPhysicalPath + "config\\RoleBasedMenuItemMap.xml";
        private static readonly NLog.ILogger StaticLogger = LogManager.GetCurrentClassLogger();
        private readonly NLog.ILogger _logger;
        
        public Dictionary<string, string[]> ResourceItemsWithRoles { get; set; }
        
        public ResourceToApplicationRolesMapper(string filepath = null, ILogger logger = null)
        {
            if (!string.IsNullOrEmpty(filepath))
                _filepath = filepath;

            _logger = logger ?? StaticLogger;

            ParseXmlToObject();
           
        }

        private void ParseXmlToObject()
        {
            try
            {
                string xml = StreamHelper.FileToString(_filepath); 
                ResourceToApplicationRolesMap mapper = xml.ParseXml<ResourceToApplicationRolesMap>();

                ResourceItemsWithRoles = new Dictionary<string, string[]>();
                foreach (ResourceToApplicationRolesMapResourceToRoles resourceToRolesItem in mapper.ResourceToRoles)
                {
                    ResourceItemsWithRoles.Add(resourceToRolesItem.ResourceId, resourceToRolesItem.Roles.Split(','));
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warn, ex,
                    "Error parsing xml in RoleBasedMenuItemMap.xml ");
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


        public string AllowedRolesForResource(string resourceKey)
        {
            if (ResourceItemsWithRoles != null && ResourceItemsWithRoles.ContainsKey(resourceKey))
            {
                return string.Join(",", ResourceItemsWithRoles[resourceKey]);
            }
            return null;
        }

        public bool IsUserRoleAllowedForResource(string resourceKey, IPrincipal loggedUser)
        {
            List<string> roles = GetAllowedRolesForResource(resourceKey);
            if (roles != null && loggedUser != null)
            {
                return roles.Any(role => loggedUser.IsInRole(role));
            }
            return false;
        }


    }

}