using AdminPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using AdminPortal.BusinessServices;
using AdminPortal.BusinessServices.LandingPage;
using Microsoft.SDC.Common;
using WebGrease.Css.Ast.Selectors;

namespace AdminPortal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private static readonly ResourceToApplicationRolesMapper ResourceToApplicationRolesMapper = new ResourceToApplicationRolesMapper();
        private static RegionIndicatorList _regionIndicatorList;
        private LandingPageModel _landingPageModel;

        [HttpGet]
        public ActionResult Index()
        {
            
            _landingPageModel = new LandingPageModel
            {
                LandingPageTabs = GetLandingPageTabs(new LandingPageLayoutLoader())
            };
            
            return View(_landingPageModel);
        }


        private List<UiLinksLandingPageTab> GetLandingPageTabs(LandingPageLayoutLoader landingPageLayoutLoader)
        {
            _regionIndicatorList = landingPageLayoutLoader.GetParsedRegionIndicatorXmlToObject();

            List<UiLinksLandingPageTab> landingPageTabs = new List<UiLinksLandingPageTab>();
            UiLinks uiLinks = landingPageLayoutLoader.GetParsedXmlToObject();

            foreach (UiLinksLandingPageTab configTab in uiLinks.LandingPageTab)
            {
                UiLinksLandingPageTab userAllowedTab = new UiLinksLandingPageTab { Section = GetLandingPageTabSections(configTab).ToArray() };

                if (userAllowedTab.Section.Length > 0)
                {
                    userAllowedTab.Key = configTab.Key;
                    userAllowedTab.Text = configTab.Text;
                    landingPageTabs.Add(userAllowedTab);
                }
            }

            if (landingPageTabs.Count > 0)
                return landingPageTabs;
            return null;
        }

        private List<UiLinksLandingPageTabSection> GetLandingPageTabSections(UiLinksLandingPageTab configTab)
        {
            List<UiLinksLandingPageTabSection> landingPageSections = new List<UiLinksLandingPageTabSection>();
            foreach (UiLinksLandingPageTabSection configSection in configTab.Section)
            {
                var userAllowedSection = new UiLinksLandingPageTabSection { MenuItem = GetLandingPageSectionMenuItems(configSection, configTab.Key).ToArray() };

                if (userAllowedSection.MenuItem.Length > 0)
                {
                    userAllowedSection.Key = configSection.Key;
                    userAllowedSection.Text = configSection.Text;
                    landingPageSections.Add(userAllowedSection);
                }
            }

            return landingPageSections;
        }

        private List<UiLinksLandingPageTabSectionMenuItem> GetLandingPageSectionMenuItems(UiLinksLandingPageTabSection configSection, string configTabKey)
        {
            List<UiLinksLandingPageTabSectionMenuItem> landingPageSectionMenuItems = new List<UiLinksLandingPageTabSectionMenuItem>();
            foreach (UiLinksLandingPageTabSectionMenuItem configMenuItem in configSection.MenuItem)
            {
                if (ResourceToApplicationRolesMapper.IsUserRoleAllowedForResource(configMenuItem.Key, User))
                {
                    if (string.IsNullOrEmpty(configMenuItem.RegionIndicator))
                    {

                        configMenuItem.RegionIndicator = GetDescriptionForRegionId(configTabKey);
                    }
                    landingPageSectionMenuItems.Add(configMenuItem);
                }
            }

            return landingPageSectionMenuItems;
        }

        private string GetDescriptionForRegionId(string id)
        {
            foreach (RegionIndicatorListRegionIndicator  regionIndicator in _regionIndicatorList.RegionIndicator)
            {
                if (regionIndicator.Id == id)
                    return regionIndicator.ShowDescription;
            }
            return null;

        }

    }
}