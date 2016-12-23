using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Xml;

namespace AdminPortal.BusinessServices
{
    public class LandingPageLayoutLoader
    {
        private string _filepath = HostingEnvironment.ApplicationPhysicalPath + "config\\UILinksMapping.xml";


        public LandingPageLayoutLoader() : this(null)
        {

        }
        public LandingPageLayoutLoader(string filePath = null)
        {
            if (filePath != null)
            {
                _filepath = filePath;
            }
        }

        public List<LandingPageTab> GetConfiguration()
        {
            List<LandingPageTab> landingPageTabs = null;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_filepath);
            XmlNode xmlNode = xmlDoc.SelectSingleNode("uilinks");

            if (xmlNode != null)
            {
                landingPageTabs = new List<LandingPageTab>();
                foreach (XmlNode tabNode in xmlNode.ChildNodes)
                {
                    LandingPageTab tab = new LandingPageTab();
                    if (tabNode != null)
                    {
                        PopulateTabObject(tabNode, tab);
                    }
                    landingPageTabs.Add(tab);
                }
            }
            //TODO: Logging
            return landingPageTabs;
        }

        private void PopulateTabObject(XmlNode tabNode, LandingPageTab tab)
        {
            if (tabNode.Attributes != null)
            {
                tab.Key = tabNode.Attributes["id"].InnerText;
                tab.Text = tabNode.Attributes["text"].InnerText;
            }

            tab.Sections = new List<LandingPageSection>();
            PopulateSectionObject(tabNode, tab);
        }

        private void PopulateSectionObject(XmlNode tabNode, LandingPageTab tab)
        {
            foreach (XmlNode sectionNode in tabNode.ChildNodes)
            {
                LandingPageSection section = new LandingPageSection();
                if (sectionNode.Attributes != null)
                {
                    section.Key = sectionNode.Attributes["id"].InnerText;
                    section.Text = sectionNode.Attributes["text"].InnerText;
                }

                section.MenuItems = new List<LandingPageSectionMenuItem>();
                PopulateMenuItemObject(section, sectionNode);
                tab.Sections.Add(section);
            }
        }

        private void PopulateMenuItemObject(LandingPageSection section, XmlNode sectionNode)
        {
            foreach (XmlNode menuItemNode in sectionNode.ChildNodes)
            {
                LandingPageSectionMenuItem menuItem = new LandingPageSectionMenuItem();
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

    public class LandingPageTab
    {
        public string Key { get; set; }

        public string Text { get; set; }

        public List<LandingPageSection> Sections { get; set; }
    }

    public class LandingPageSection
    {
        public string Key { get; set; }

        public string Text { get; set; }

        public List<LandingPageSectionMenuItem> MenuItems { get; set; }
    }

    public class LandingPageSectionMenuItem
    {
        //Key is same as ResourceKey defined in RoleBasedMenuItemMap.xml
        public string Key { get; set; }

        public string Text { get; set; }

        public string Link { get; set; }

        public string ControllerName => (this.Key = this.Key.Remove(this.Key.Length - 2));
    }
}
