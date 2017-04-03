using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using AdminPortal.BusinessServices;
using AdminPortal.BusinessServices.Common.Debugging;
using AdminPortal.BusinessServices.LandingPage;
using AdminPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Serilog;
using Serilog.Events;
using Webjet.Common.Strings;

namespace AdminPortal.Controllers
{
    public class HomeController : Controller
    {
        private static ResourceToApplicationRolesMapper _resourceToApplicationRolesMapper;
        private static RegionIndicatorList _regionIndicatorList;// to load once
        private LandingPageLayoutLoader _landingPageLayoutLoader;
        static ILogger _logger = Log.ForContext<HomeController>();
        private static string AdminPortalAccessTokenKey = Guid.NewGuid().ToString();
        private AccessTokenCache _accessTokenCache;

        public HomeController(IConfigurationRoot config, LandingPageLayoutLoader landingPageLayoutLoader, ResourceToApplicationRolesMapper resourceToApplicationRolesMapper, IDistributedCache redisCache)
        {
            _accessTokenCache = new AccessTokenCache(redisCache);
            _landingPageLayoutLoader = landingPageLayoutLoader;
            _resourceToApplicationRolesMapper = resourceToApplicationRolesMapper;
        }

        [Authorize]
        [HttpGet]
        public ActionResult Index()
        {
            //TODO: Working on it.
            //if (!IsAdminPortalAccessTokenExistsInCache())
            //{
            //    StoreAdminPortalAccessToken();
            //}
            
            _logger.Debug((User as ClaimsPrincipal).WriteClaims()); // Debug output window

            _logger.Write(LogEventLevel.Verbose, "Testing logger");

            _logger.Information("AdminPortal -> information log " + DateTime.Now.ToLongDateString()); // Event Viewer and Sumologic

            var landingPageModel = GetLandingPageTabs(_landingPageLayoutLoader);
            return View(landingPageModel);
        }

        private void RedisCacheSample()
        {
            string value = _accessTokenCache.Get<string>("CacheTime");
            if (value == null)
            {
                value = DateTime.Now.ToString();

                _accessTokenCache.Store("CacheTime", value);
            }

            string redisServerTime = _accessTokenCache.Get<string>("CacheTime");
            string applicationTime = DateTime.Now.ToString(CultureInfo.CurrentCulture);

        }

        private bool IsAdminPortalAccessTokenExistsInCache()
        {
            bool isExists = true;
            var value = _accessTokenCache.Get<AccessTokenDetails>(AdminPortalAccessTokenKey);
            if (value == null)
            {
                isExists= false;
            }
            return isExists;
        }
        private void StoreAdminPortalAccessToken()
        {
            string emailAddress = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value;
            emailAddress = emailAddress.TrimStart("live.com#"); // For some accounts AAD inserts "live.com#"

            AccessTokenDetails accessTokenDetails = new AccessTokenDetails()
                {
                    UserName = User.FindFirst(ClaimTypes.GivenName)?.Value,
                    UserEmailAddress = emailAddress,
                    AllowedResources = _resourceToApplicationRolesMapper.GetAllowedForUserResources(User),
                    AdminPortalAccessToken = AdminPortalAccessTokenKey
                };

                _accessTokenCache.Store(AdminPortalAccessTokenKey, accessTokenDetails);
            
            //AccessTokenDetails fromCache = _accessTokenCache.Get<AccessTokenDetails>(AccessTokenCache.AccessTokenKey);
        }
        private void AADTokenCacheService()
        {
            var value = _accessTokenCache.Get<AccessTokenDetails>(AccessTokenCache.AccessTokenKey);
            if (value == null)
            {
                AccessTokenDetails accessTokenDetails = new AccessTokenDetails();
                accessTokenDetails.UserName = User.Identity.Name;
                accessTokenDetails.UserEmailAddress = User.Identity.Name;
                accessTokenDetails.AllowedResources = _resourceToApplicationRolesMapper.GetAllowedForUserResources(User);
                accessTokenDetails.AdminPortalAccessToken = Guid.NewGuid().ToString();
                _accessTokenCache.Store(AccessTokenCache.AccessTokenKey, accessTokenDetails);
            }

            AccessTokenDetails fromCache = _accessTokenCache.Get<AccessTokenDetails>(AccessTokenCache.AccessTokenKey);
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
                        if (String.IsNullOrEmpty(configMenuItem.RegionIndicator))
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
