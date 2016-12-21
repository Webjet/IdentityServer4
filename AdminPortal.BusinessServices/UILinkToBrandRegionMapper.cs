using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Xml;

namespace AdminPortal.BusinessServices
{
    public class UiLinkMapper
    {
        private readonly string _filepath = HostingEnvironment.ApplicationPhysicalPath + "config\\UILinksMapping.xml";

        public List<Tab> Tabs { get; set; }

        public UiLinkMapper()
        {
            ReadUiLinkToBrandConfiguration();
        }

        private void ReadUiLinkToBrandConfiguration()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_filepath);
            XmlNode xmlNode = xmlDoc.SelectSingleNode("uilinks");

            if (xmlNode != null)
            {
                Tabs = new List<Tab>();
                foreach (XmlNode tabNode in xmlNode.ChildNodes)
                {
                    Tab tab = new Tab();
                    if (tabNode != null)
                    {
                        PopulateTabObject(tabNode, tab);
                    }
                    Tabs.Add(tab);
                }
            }
        }

        private static void PopulateTabObject(XmlNode tabNode, Tab tab)
        {
            if (tabNode.Attributes != null)
            {
                tab.Key = tabNode.Attributes["id"].InnerText;
                tab.Text = tabNode.Attributes["text"].InnerText;
            }

            Section section = new Section();
            tab.Sections = new List<Section>();
            PopulateSectionObject(tabNode, tab, section);
        }

        private static void PopulateSectionObject(XmlNode tabNode, Tab tab, Section section)
        {
            foreach (XmlNode sectionNode in tabNode.ChildNodes)
            {
                if (sectionNode.Attributes != null)
                {
                    section.Key = sectionNode.Attributes["id"].InnerText;
                    section.Text = sectionNode.Attributes["text"].InnerText;
                }
                MenuItem menuItem = new MenuItem();
                section.MenuItems = new List<MenuItem>();
                PopulateMenuItemObject(section, sectionNode, menuItem);
                tab.Sections.Add(section);
            }
        }

        private static void PopulateMenuItemObject(Section section, XmlNode sectionNode, MenuItem menuItem)
        {
            foreach (XmlNode menuItemNode in sectionNode.ChildNodes)
            {
                if (menuItemNode.Attributes != null)
                {
                    menuItem.Key = menuItemNode.Attributes["id"].InnerText;
                    menuItem.Text = menuItemNode.Attributes["text"].InnerText;
                    menuItem.Link = menuItemNode.Attributes["link"].InnerText;
                }
                section.MenuItems.Add(menuItem);
            }
        }
    }

    public class Tab
    {
        public string Key { get; set; }

        public string Text { get; set; }

        public List<Section> Sections { get; set; }
    }

    public class Section
    {
        public string Key { get; set; }

        public string Text { get; set; }

        public List<MenuItem> MenuItems { get; set; }
    }

    public class MenuItem
    {
        //Key is same as ResourceKey defined in RoleBasedMenuItemMap.xml
        public string Key { get; set; }

        public string Text { get; set; }

        public string Link { get; set; }

    }
}
