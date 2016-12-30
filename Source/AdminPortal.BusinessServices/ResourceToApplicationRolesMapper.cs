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

namespace AdminPortal.BusinessServices
{
    public class ResourceToApplicationRolesMapper
    {
        private readonly string _filepath = HostingEnvironment.ApplicationPhysicalPath + "config\\RoleBasedMenuItemMap.xml";
        private readonly NLog.ILogger _nLogger;


        public Dictionary<string, string[]> ResourceItemsWithRoles { get; set; }

        public ResourceToApplicationRolesMapper() : this(null, LogManager.GetCurrentClassLogger())
        {
        }

        public ResourceToApplicationRolesMapper(string filepath, ILogger nLogger)
        {
            if (!string.IsNullOrEmpty(filepath))
            {
                _filepath = filepath;
            }
            _nLogger = nLogger;

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

        private void ReadConfiguration()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_filepath);
            XmlNode xmlNode = xmlDoc.SelectSingleNode("requiredResourceAccess");

            if (xmlNode != null)
            {
                ResourceItemsWithRoles = new Dictionary<string, string[]>();
                foreach (XmlNode node in xmlNode.ChildNodes)
                {
                    try
                    {
                        if (node.Attributes != null)
                        {
                            ResourceItemsWithRoles.Add(
                                node.Attributes["key"].InnerText,
                                node.Attributes["value"].InnerText.Split(',')
                                );
                        }
                    }
                    catch (Exception ex)
                    {
                        _nLogger.Log(LogLevel.Warn, ex,
                    "XML node attribute parsing error in RoleBasedMenuItemMap.xml ");
                    }
                }
            }
            else
            {
                _nLogger.Log(LogLevel.Warn,
                    "XML node is null for requiredResourceAccess in RoleBasedMenuItemMap.xml on filepath- " + _filepath);

            }
        }

    }

}