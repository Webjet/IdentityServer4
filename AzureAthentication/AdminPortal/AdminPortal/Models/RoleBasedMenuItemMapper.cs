using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Xml;

namespace AdminPortal.Models
{
    public class RoleBasedResourceItemMapper
    {
        private readonly string _filepath = HostingEnvironment.ApplicationPhysicalPath + "Resource\\RoleBasedMenuItemMap.xml";
        private List<RoleBasedResourceItem> _roleBasedResourceKeys;

        public RoleBasedResourceItemMapper()
        {
            ReadXmlResourceWithRoleConfiguration();
            //ReadJSONConfiguration();
        }
        
        public List<string> AllowedRolesForResource(string resourceKey)
        {
            RoleBasedResourceItem roleBasedResourceItem = _roleBasedResourceKeys.FirstOrDefault(x => x.ResourceKey == resourceKey);
            if (roleBasedResourceItem != null)
                return roleBasedResourceItem.Roles.ToList();
            return null;
        }

        private void ReadJSONConfiguration()
        {
            throw new NotImplementedException();
        }

        private void ReadXmlResourceWithRoleConfiguration()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_filepath);
            XmlNode xmlNode = xmlDoc.SelectSingleNode("requiredResourceAccess");
            _roleBasedResourceKeys = new List<RoleBasedResourceItem>();

            if (xmlNode != null)
            {
                foreach (XmlNode node in xmlNode.ChildNodes)
                {
                    RoleBasedResourceItem resourceItem = new RoleBasedResourceItem();
                    if (node.Attributes != null)
                    {
                        resourceItem.ResourceKey = node.Attributes["key"].InnerText;
                        resourceItem.Roles = node.Attributes["value"].InnerText.Split(',');
                    }

                    _roleBasedResourceKeys.Add(resourceItem);
                }
            }
        }
       
    }

    public class RoleBasedResourceItem
    {
        public string ResourceKey { get; set; }

        public string[] Roles { get; set; }

    }

}