using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AdminPortal.BusinessServices;
using AdminPortal.BusinessServices.Common.Debugging;
using AdminPortal.BusinessServices.LandingPage;
using AdminPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using Serilog;







namespace AdminPortal.Controllers
{
     public class HomeController : Controller
    {
        private static ResourceToApplicationRolesMapper _resourceToApplicationRolesMapper;
        private static RegionIndicatorList _regionIndicatorList;// to load once
        private LandingPageLayoutLoader _landingPageLayoutLoader;
        static Serilog.ILogger _logger = Log.ForContext<HomeController>();


        public HomeController(IConfigurationRoot config, LandingPageLayoutLoader landingPageLayoutLoader, ResourceToApplicationRolesMapper resourceToApplicationRolesMapper)
        {
            _landingPageLayoutLoader = landingPageLayoutLoader;
            _resourceToApplicationRolesMapper = resourceToApplicationRolesMapper;
        }

        [Authorize]
        [HttpGet]
        public ActionResult Index()
        {
            _logger.Debug((User as ClaimsPrincipal).WriteClaims());
             var landingPageModel = GetLandingPageTabs(_landingPageLayoutLoader);
             return View(landingPageModel);
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
            if (configSection.MenuItem != null)
            {
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
        public IActionResult About()
        {
            return View();
        }
        //private static readonly ResourceToApplicationRolesMapper ResourceToApplicationRolesMapper = new ResourceToApplicationRolesMapper();
        //private static RegionIndicatorList _regionIndicatorList;
        //private LandingPageLayoutLoader _landingPageLayoutLoader;
        //private ILogger<HomeController> _logger;

        ////TODO: Replace with Anotar.Serlog.Fordy
        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //    _landingPageLayoutLoader = new LandingPageLayoutLoader();
        //}

        ////public HomeController(LandingPageLayoutLoader landingPageLayoutLoader)
        ////{

        ////    _landingPageLayoutLoader = landingPageLayoutLoader;
        ////}

        //[HttpGet]
        //public ActionResult Index()
        //{
        //    //TODO: Replace with Anotar.Serlog.Fordy as LogTo.Inforametion("")
        //    _logger.LogInformation("DEBUG The Index action was invoked ");


        //    //try
        //    //{
        //        var landingPageModel = GetLandingPageTabs(_landingPageLayoutLoader);

        //        return View(landingPageModel);
        //    //}
        //    //catch
        //    //{
        //    //    //TODO: Handle with application error or custom error handling via config. Temporaty added, working on it.

        //    //    // Redirect to Error page in case of exception while get LandingPageTabs.
        //    //    return RedirectToAction("ShowError", "Error");
        //    //}

        //}


        //private LandingPageModel GetLandingPageTabs(LandingPageLayoutLoader landingPageLayoutLoader)
        //{
        //    var landingPageModel = new LandingPageModel();
        //    _regionIndicatorList = landingPageLayoutLoader.GetRegionIndicators();

        //    List<UiLinksLandingPageTab> landingPageTabs = new List<UiLinksLandingPageTab>();
        //    UiLinks uiLinks = landingPageLayoutLoader.GetUiLinks();

        //    foreach (UiLinksLandingPageTab configTab in uiLinks.LandingPageTab)
        //    {
        //        UiLinksLandingPageTab userAllowedTab = new UiLinksLandingPageTab { Section = GetLandingPageTabSections(configTab).ToArray() };

        //        if (userAllowedTab.Section.Length > 0)
        //        {
        //            userAllowedTab.Key = configTab.Key;
        //            userAllowedTab.Text = configTab.Text;
        //            landingPageTabs.Add(userAllowedTab);
        //        }
        //    }

        //    landingPageModel.LandingPageTabs = landingPageTabs;
        //    return landingPageModel;

        //}

        //private List<UiLinksLandingPageTabSection> GetLandingPageTabSections(UiLinksLandingPageTab configTab)
        //{
        //    List<UiLinksLandingPageTabSection> landingPageSections = new List<UiLinksLandingPageTabSection>();
        //    foreach (UiLinksLandingPageTabSection configSection in configTab.Section)
        //    {
        //        var userAllowedSection = new UiLinksLandingPageTabSection { MenuItem = GetLandingPageSectionMenuItems(configSection, configTab.Key).ToArray() };

        //        if (userAllowedSection.MenuItem.Length > 0)
        //        {
        //            userAllowedSection.Key = configSection.Key;
        //            userAllowedSection.Text = configSection.Text;
        //            landingPageSections.Add(userAllowedSection);
        //        }
        //    }

        //    return landingPageSections;
        //}

        //private List<UiLinksLandingPageTabSectionMenuItem> GetLandingPageSectionMenuItems(UiLinksLandingPageTabSection configSection, string configTabKey)
        //{
        //    List<UiLinksLandingPageTabSectionMenuItem> landingPageSectionMenuItems = new List<UiLinksLandingPageTabSectionMenuItem>();
        //    foreach (UiLinksLandingPageTabSectionMenuItem configMenuItem in configSection.MenuItem)
        //    {
        //        if (ResourceToApplicationRolesMapper.IsUserRoleAllowedForResource(configMenuItem.Key, User))
        //        {
        //            if (string.IsNullOrEmpty(configMenuItem.RegionIndicator))
        //            {
        //                configMenuItem.RegionIndicator = GetShortDescriptionForRegionId(configTabKey);
        //            }
        //            landingPageSectionMenuItems.Add(configMenuItem);
        //        }
        //    }

        //    return landingPageSectionMenuItems;
        //}

        //private string GetShortDescriptionForRegionId(string configTabKey)
        //{
        //    if (_regionIndicatorList != null && _regionIndicatorList.RegionIndicator != null)
        //    {
        //        foreach (var regionIndicator in _regionIndicatorList.RegionIndicator)
        //        {
        //            if (regionIndicator.Id == configTabKey)
        //                return regionIndicator.ShortDescription;
        //        }
        //    }
        //    return null;

        //}

        ////public IActionResult Index()
        ////{
        ////    return View();
        ////}

        ////[Authorize(Roles = "ProductTeam")]
        ////public IActionResult About()
        ////{
        ////    ViewData["Message"] = "Your application description page.";

        ////    return View();
        ////}

        ////public IActionResult Contact()
        ////{
        ////    ViewData["Message"] = "Your contact page.";

        ////    return View();
        ////}

        ////public IActionResult Error()
        ////{
        ////    return View();
        ////}

    }
}
