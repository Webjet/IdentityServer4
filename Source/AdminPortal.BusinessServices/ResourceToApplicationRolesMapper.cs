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
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace AdminPortal.BusinessServices
{
    public class ResourceToApplicationRolesMapper
    {
       public static string ConfigFilePath = Path.Combine(Directory.GetCurrentDirectory(), @"config\ResourceToRolesMap.xml");
        private readonly string _filepath ;

        public Dictionary<string, string[]> ResourceItemsWithRoles { get; set; }

         public ResourceToApplicationRolesMapper(string filepath=null)
        {
            _filepath = !string.IsNullOrEmpty(filepath) ? filepath : ConfigFilePath;
            ParseXmlToObject();
        }

        private void ParseXmlToObject()
        {
            string xml = StreamHelper.FileToString(_filepath); 
            ResourceToApplicationRolesMap mapper = xml.ParseXml<ResourceToApplicationRolesMap>();

            ResourceItemsWithRoles = new Dictionary<string, string[]>(StringComparer.InvariantCultureIgnoreCase);
            foreach (ResourceToApplicationRolesMapResourceToRoles resourceToRolesItem in mapper.ResourceToRoles)
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