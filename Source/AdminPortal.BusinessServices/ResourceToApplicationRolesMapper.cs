using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Web;
using System.Web.Hosting;
using System.Xml;
using System.Web.Mvc;
using System.Security.Principal;

namespace AdminPortal.BusinessServices
{
    public class ResourceToApplicationRolesMapper
    {
        private readonly string _filepath = HostingEnvironment.ApplicationPhysicalPath + "config\\RoleBasedMenuItemMap.xml";
       
        public Dictionary<string, string[]> ResourceItemsWithRoles { get; set; }

      
        public ResourceToApplicationRolesMapper()
        {
            ReadConfiguration();
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
            if (roles != null && loggedUser != null)
            {
                return roles.Any(role => loggedUser.IsInRole(role));
            }
            return false;
        }

        private void ReadConfiguration()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_filepath);
            XmlNode xmlNode = xmlDoc.SelectSingleNode("requiredResourceAccess");
            //TODO: Add UTC
            if (xmlNode != null)
            {
                ResourceItemsWithRoles = new Dictionary<string, string[]>();
                foreach (XmlNode node in xmlNode.ChildNodes)
                {
                    if (node.Attributes != null)
                    {
                        ResourceItemsWithRoles.Add(
                            node.Attributes["key"].InnerText,
                            node.Attributes["value"].InnerText.Split(',')
                            );

                    }

                }
            }
            //TODO: add logging incase xmlNode == null
        }

    }

}