using AdminPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using AdminPortal.BusinessServices;
using Microsoft.SDC.Common;
using WebGrease.Css.Ast.Selectors;

namespace AdminPortal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private static readonly ResourceToApplicationRolesMapper ResourceToApplicationRolesMapper = new ResourceToApplicationRolesMapper();

        private LandingPageModel _landingPageModel;

        [HttpGet]
        [Authorize(Roles = "ServiceCenter,ServiceCenterManager,ProductTeam,Finance,Analytics,DevSupport")]
        public ActionResult Index()
        {
            _landingPageModel = new LandingPageModel
            {
                LandingPageTabs = GetLandingPageTabs(new LandingPageLayoutLoader())
            };

            return View(_landingPageModel);
        }

        private List<LandingPageTab> GetLandingPageTabs(LandingPageLayoutLoader landingPageLayoutLoader)
        {
            List<LandingPageTab> landingPageTabs = new List<LandingPageTab>();

            foreach (LandingPageTab configTab in landingPageLayoutLoader.GetConfiguration())
            {
                LandingPageTab userAllowedTab = new LandingPageTab { Sections = GetLandingPageTabSections(configTab) };

                if (userAllowedTab.Sections.Count > 0)
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

        private List<LandingPageSection> GetLandingPageTabSections(LandingPageTab configTab)
        {
            List<LandingPageSection> landingPageSections = new List<LandingPageSection>();
            foreach (LandingPageSection configSection in configTab.Sections)
            {
                var userAllowedSection = new LandingPageSection { MenuItems = GetLandingPageSectionMenuItems(configSection) };

                if (userAllowedSection.MenuItems.Count > 0)
                {
                    userAllowedSection.Key = configSection.Key;
                    userAllowedSection.Text = configSection.Text;
                    landingPageSections.Add(userAllowedSection);
                }
            }

            return landingPageSections;
        }

        private List<LandingPageSectionMenuItem> GetLandingPageSectionMenuItems(LandingPageSection configSection)
        {
            List<LandingPageSectionMenuItem> landingPageSectionMenuItems = new List<LandingPageSectionMenuItem>();
            foreach (LandingPageSectionMenuItem configMenuItem in configSection.MenuItems)
            {
                if (ResourceToApplicationRolesMapper.IsUserRoleAllowedForResource(configMenuItem.Key, User))
                {
                    landingPageSectionMenuItems.Add(configMenuItem);
                }
            }

            return landingPageSectionMenuItems;
        }


        //private List<LandingPageTab> GetLandingPageTabs()
        //{
        //    LandingPageLayoutLoader landingPageLayoutLoader = new LandingPageLayoutLoader();

        //    foreach (LandingPageTab configTab in landingPageLayoutLoader.GetConfiguration())
        //    {
        //        var userAllowedTab = new LandingPageTab { Sections = new List<LandingPageSection>() };

        //        GetLandingPageTabSections(configTab, userAllowedTab);
        //    }
        //    return null;
        //}

        //private void GetLandingPageTabSections(LandingPageTab configTab, LandingPageTab userAllowedTab)
        //{
        //    foreach (LandingPageSection configSection in configTab.Sections)
        //    {
        //        var userAllowedSection = new LandingPageSection { MenuItems = new List<LandingPageSectionMenuItem>() };
        //        GetLandingPageSectionMenuItems(configSection, userAllowedSection, userAllowedTab);
        //    }

        //    if (userAllowedTab.Sections.Count > 0)
        //    {
        //        userAllowedTab.Key = configTab.Key;
        //        userAllowedTab.Text = configTab.Text;
        //        _landingPageModel.LandingPageTabs.Add(userAllowedTab);
        //    }
        //}

        //private void GetLandingPageSectionMenuItems(LandingPageSection configSection, LandingPageSection userAllowedSection, LandingPageTab userAllowedTab)
        //{
        //    foreach (LandingPageSectionMenuItem configMenuItem in configSection.MenuItems)
        //    {
        //        if (ResourceToApplicationRolesMapper.IsUserRoleAllowedForResource(configMenuItem.Key, User))
        //        {
        //            userAllowedSection.MenuItems.Add(configMenuItem);
        //        }
        //    }

        //    if (userAllowedSection.MenuItems.Count > 0)
        //    {
        //        userAllowedSection.Key = configSection.Key;
        //        userAllowedSection.Text = configSection.Text;
        //        userAllowedTab.Sections.Add(userAllowedSection);
        //    }
        //}


    }
}