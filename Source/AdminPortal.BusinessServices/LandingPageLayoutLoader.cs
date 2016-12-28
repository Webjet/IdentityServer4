using Microsoft.SDC.Common;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Xml;

namespace AdminPortal.BusinessServices
{
    public class LandingPageLayoutLoader
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly string _filepath = HostingEnvironment.ApplicationPhysicalPath + "config\\UILinksMapping.xml";


        public LandingPageLayoutLoader() : this(null)
        {

        }
        public LandingPageLayoutLoader(string filepath)
        {
            if (!string.IsNullOrEmpty(filepath))
            {
                _filepath = filepath;
            }
        }

        public List<LandingPageTab> GetConfiguration()
        {
            List<LandingPageTab> landingPageTabs = new List<LandingPageTab>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_filepath);
            XmlNode xmlNode = xmlDoc.SelectSingleNode("uilinks");

            if (xmlNode != null && xmlNode.HasChildNodes)
            {
                foreach (XmlNode tabNode in xmlNode.ChildNodes)
                {
                    if (tabNode != null)
                    {
                        LandingPageTab landingPageTab = PopulateTabObject(tabNode);
                        if (landingPageTab != null)
                        {
                            landingPageTabs.Add(landingPageTab);
                        }
                    }
                }
            }
            else
            {
                LoggerHelper.LogEvent("XML node is null for uilinks in UILinksMapping.xml on filepath- " + _filepath, Logger,
                    TraceEventType.Warning);
            }

            if (landingPageTabs.Count > 0)
                return landingPageTabs;
            return null;

        }

        private LandingPageTab PopulateTabObject(XmlNode tabNode)
        {
            LandingPageTab landingPageTab = null;
            if (tabNode.Attributes != null)
            {
                List<LandingPageSection> landingPageTabSections = PopulateSectionObject(tabNode);
                try
                {
                    if (landingPageTabSections != null)
                    {
                        landingPageTab = new LandingPageTab();
                        landingPageTab.Sections = landingPageTabSections;
                        landingPageTab.Key = tabNode.Attributes["id"].InnerText;
                        landingPageTab.Text = tabNode.Attributes["text"].InnerText;
                    }
                }

                catch (Exception ex)
                {
                    LoggerHelper.LogEvent("Error in XML parsing UILinksMapping.xml- " + ex, Logger,
                        TraceEventType.Warning);
                }
            }
            return landingPageTab;
        }

        private List<LandingPageSection> PopulateSectionObject(XmlNode tabNode)
        {
            List<LandingPageSection> landingPageSections = new List<LandingPageSection>();

            foreach (XmlNode sectionNode in tabNode.ChildNodes)
            {
                if (sectionNode.Attributes != null)
                {
                    List<LandingPageSectionMenuItem> landingPageSectionMenuItems = PopulateMenuItemObject(sectionNode);
                    try
                    {
                        if (landingPageSectionMenuItems != null)
                        {
                            LandingPageSection section = new LandingPageSection();
                            section.MenuItems = landingPageSectionMenuItems;
                            section.Key = sectionNode.Attributes["id"].InnerText;
                            section.Text = sectionNode.Attributes["text"].InnerText;

                            landingPageSections.Add(section);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.LogEvent("Error in XML parsing UILinksMapping.xml- " + ex, Logger,
                            TraceEventType.Warning);
                    }
                }

            }

            if (landingPageSections.Count > 0)
                return landingPageSections;
            return null;
        }

        private List<LandingPageSectionMenuItem> PopulateMenuItemObject(XmlNode sectionNode)
        {
            List<LandingPageSectionMenuItem> landingPageMenuItems = new List<LandingPageSectionMenuItem>();
            if (sectionNode.HasChildNodes)
            {
                foreach (XmlNode menuItemNode in sectionNode.ChildNodes)
                {
                    try
                    {
                        if (menuItemNode.Attributes != null)
                        {
                            LandingPageSectionMenuItem menuItem = new LandingPageSectionMenuItem
                            {
                                Key = menuItemNode.Attributes["id"].InnerText,
                                Text = menuItemNode.Attributes["text"].InnerText,
                                Link = menuItemNode.Attributes["link"].InnerText
                            };

                            landingPageMenuItems.Add(menuItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.LogEvent("Error in XML parsing UILinksMapping.xml- " + ex, Logger,
                        TraceEventType.Warning);
                    }

                }
            }
            if (landingPageMenuItems.Count > 0)
                return landingPageMenuItems;
            return null;
        
        }

        //public List<LandingPageTab> GetConfiguration()
        //{
        //    List<LandingPageTab> landingPageTabs = null;
        //    XmlDocument xmlDoc = new XmlDocument();
        //    xmlDoc.Load(_filepath);
        //    XmlNode xmlNode = xmlDoc.SelectSingleNode("uilinks");

        //    if (xmlNode != null)
        //    {
        //        landingPageTabs = new List<LandingPageTab>();
        //        foreach (XmlNode tabNode in xmlNode.ChildNodes)
        //        {
        //            LandingPageTab tab = new LandingPageTab();
        //            if (tabNode != null)
        //            {
        //                PopulateTabObject(tabNode, tab);
        //            }
        //            landingPageTabs.Add(tab);
        //        }
        //    }
        //    else
        //    {
        //        LoggerHelper.LogEvent("XML node is null for uilinks in UILinksMapping.xml", Logger,
        //            TraceEventType.Warning);
        //    }

        //    return landingPageTabs;
        //}

        //private void PopulateTabObject(XmlNode tabNode, LandingPageTab tab)
        //{
        //    if (tabNode.Attributes != null)
        //    {
        //        tab.Key = tabNode.Attributes["id"].InnerText;
        //        tab.Text = tabNode.Attributes["text"].InnerText;
        //    }

        //    tab.Sections = new List<LandingPageSection>();
        //    PopulateSectionObject(tabNode, tab);
        //}

        //private void PopulateSectionObject(XmlNode tabNode, LandingPageTab tab)
        //{
        //    foreach (XmlNode sectionNode in tabNode.ChildNodes)
        //    {
        //        LandingPageSection section = new LandingPageSection();
        //        if (sectionNode.Attributes != null)
        //        {
        //            section.Key = sectionNode.Attributes["id"].InnerText;
        //            section.Text = sectionNode.Attributes["text"].InnerText;
        //        }

        //        section.MenuItems = new List<LandingPageSectionMenuItem>();
        //        PopulateMenuItemObject(section, sectionNode);
        //        tab.Sections.Add(section);
        //    }
        //}

        //private void PopulateMenuItemObject(LandingPageSection section, XmlNode sectionNode)
        //{
        //    foreach (XmlNode menuItemNode in sectionNode.ChildNodes)
        //    {
        //        LandingPageSectionMenuItem menuItem = new LandingPageSectionMenuItem();
        //        if (menuItemNode.Attributes != null)
        //        {
        //            menuItem.Key = menuItemNode.Attributes["id"].InnerText;
        //            menuItem.Text = menuItemNode.Attributes["text"].InnerText;
        //            menuItem.Link = menuItemNode.Attributes["link"].InnerText;
        //        }
        //        section.MenuItems.Add(menuItem);
        //    }
        //}
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


    }
}
