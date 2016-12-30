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
using NLog.Common;

namespace AdminPortal.BusinessServices
{
    public class LandingPageLayoutLoader
    {
        private readonly string _filepath = HostingEnvironment.ApplicationPhysicalPath + "config\\UILinksMapping.xml";
        private readonly NLog.ILogger _nLogger;

        public LandingPageLayoutLoader() : this(null, LogManager.GetCurrentClassLogger())
        {

        }

        public LandingPageLayoutLoader(string filepath, ILogger nLogger)
        {
            if (!string.IsNullOrEmpty(filepath))
            {
                _filepath = filepath;
            }
            _nLogger = nLogger;
        }

        public List<LandingPageTab> GetConfiguration()
        {
            List<LandingPageTab> landingPageTabs = new List<LandingPageTab>();
            try
            {


                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(_filepath);
                XmlNode xmlNode = xmlDoc.SelectSingleNode("uilinks");

                if (xmlNode != null && xmlNode.HasChildNodes)
                {
                    foreach (XmlNode tabNode in xmlNode.ChildNodes)
                    {
                        LandingPageTab landingPageTab = PopulateTabObject(tabNode);
                        if (landingPageTab != null)
                        {
                            landingPageTabs.Add(landingPageTab);
                        }
                    }
                }
                else
                {
                    _nLogger.Log(LogLevel.Warn,
                        "XML node is null for uilinks in UILinksMapping.xml on filepath- " + _filepath);
                }
            }
            catch (Exception ex)
            {
                _nLogger.Log(LogLevel.Error, ex,
                      "Error in XML parsing UILinksMapping.xml ");
            }

            if (landingPageTabs.Count > 0)
                return landingPageTabs;
            return null;

        }

        private LandingPageTab PopulateTabObject(XmlNode tabNode)
        {
            LandingPageTab landingPageTab = null;
            try
            {
                if (tabNode.Attributes != null && tabNode.Attributes["id"] != null && tabNode.Attributes["text"] != null)
                {
                    List<LandingPageSection> landingPageTabSections = PopulateSectionObject(tabNode);

                    if (landingPageTabSections != null)
                    {
                        landingPageTab = new LandingPageTab();
                        landingPageTab.Sections = landingPageTabSections;
                        landingPageTab.Key = tabNode.Attributes["id"].InnerText;
                        landingPageTab.Text = tabNode.Attributes["text"].InnerText;
                    }

                }
            }
            catch (Exception ex)
            {
                _nLogger.Log(LogLevel.Warn, ex, "Error in XML parsing UILinksMapping.xml");
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
                    try
                    {
                        List<LandingPageSectionMenuItem> landingPageSectionMenuItems = PopulateMenuItemObject(sectionNode);
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
                        _nLogger.Log(LogLevel.Warn, ex, "Error in XML parsing UILinksMapping.xml");

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
                        _nLogger.Log(LogLevel.Warn, ex, "Error in XML parsing UILinksMapping.xml");

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
