using AdminPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AdminPortal.BusinessServices;
using AdminPortal.BusinessServices.Common.Debugging;
using AdminPortal.BusinessServices.LandingPage;
using Microsoft.SDC.Common;
using WebGrease.Css.Ast.Selectors;

namespace AdminPortal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private static  ResourceToApplicationRolesMapper _resourceToApplicationRolesMapper;
        private static RegionIndicatorList _regionIndicatorList;// to load once
        private LandingPageLayoutLoader _landingPageLayoutLoader;

        public HomeController() : this(new LandingPageLayoutLoader(), new ResourceToApplicationRolesMapper())
        {
  
        }

        public HomeController(LandingPageLayoutLoader landingPageLayoutLoader, ResourceToApplicationRolesMapper resourceToApplicationRolesMapper)
        {

            _landingPageLayoutLoader = landingPageLayoutLoader;
            _resourceToApplicationRolesMapper = resourceToApplicationRolesMapper;
        }

        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                var landingPageModel = GetLandingPageTabs(_landingPageLayoutLoader);

                return View(landingPageModel);
            }
            catch 
            {
                //TODO: Handle with application error or custom error handling via config. Temporaty added, working on it.
               
                // Redirect to Error page in case of exception while get LandingPageTabs.
                return RedirectToAction("ShowError", "Error");
            }

        }


        private LandingPageModel GetLandingPageTabs(LandingPageLayoutLoader landingPageLayoutLoader)
        {
            var landingPageModel = new LandingPageModel();
            _regionIndicatorList = landingPageLayoutLoader.GetRegionIndicators();

            List<UiLinksLandingPageTab> landingPageTabs = new List<UiLinksLandingPageTab>();
            UiLinks uiLinks = landingPageLayoutLoader.GetUiLinks();

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

            landingPageModel.LandingPageTabs = landingPageTabs;
            return landingPageModel;

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
                if (_resourceToApplicationRolesMapper.IsUserRoleAllowedForResource(configMenuItem.Key, User))
                {
                    if (string.IsNullOrEmpty(configMenuItem.RegionIndicator))
                    {
                        configMenuItem.RegionIndicator = GetShortDescriptionForRegionId(configTabKey);
                    }
                    landingPageSectionMenuItems.Add(configMenuItem);
                }
            }

            return landingPageSectionMenuItems;
        }

        private string GetShortDescriptionForRegionId(string configTabKey)
        {
            if (_regionIndicatorList != null && _regionIndicatorList.RegionIndicator != null)
            {
                foreach (var regionIndicator in _regionIndicatorList.RegionIndicator)
                {
                    if (regionIndicator.Id == configTabKey)
                        return regionIndicator.ShortDescription;
                }
            }
            return null;

        }

    }
}