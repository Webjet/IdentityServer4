using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Xml;
using System.Web.Mvc;

namespace AdminPortal.BusinessServices
{
    public class ResourceToApplicationRoleMapper
    {
        private readonly string _filepath = HostingEnvironment.ApplicationPhysicalPath + "config\\RoleBasedMenuItemMap.xml";
        //private string test = Assembly.GetExecutingAssembly().Location;
        //private string test1 = HostingEnvironment.ApplicationPhysicalPath + "config\\RoleBasedMenuItemMap.xml";

        //private string test2= HttpContext.Current.Server.MapPath("~/config/RoleBasedMenuItemMap.xml");

        //private string test3= Environment.CurrentDirectory + "\\config\\RoleBasedMenuItemMap.xml";

        public Dictionary<string, string[]> ResourceItemsWithRoles { get; set; }

        public ResourceToApplicationRoleMapper()
        {
            ReadRoleBasedResourceItemConfiguration();
        }
        
        public List<string> GetAllowedRolesForResource(string resourceKey)
        {
            if (ResourceItemsWithRoles != null && ResourceItemsWithRoles.ContainsKey(resourceKey))
            {
                return ResourceItemsWithRoles[resourceKey].ToList();
            }
            return null;
        }
        
        private void ReadRoleBasedResourceItemConfiguration()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_filepath);
            XmlNode xmlNode = xmlDoc.SelectSingleNode("requiredResourceAccess");
         
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
        }

    }
    
}