using AdminPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using AdminPortal.BusinessServices;
using WebGrease.Css.Ast.Selectors;

namespace AdminPortal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private static readonly ResourceToApplicationRolesMapper ResourceToApplicationRolesMapper= new ResourceToApplicationRolesMapper();

        private  LandingPageModel _landingPageModel;

        [HttpGet]
        [Authorize(Roles = "ServiceCenter,ServiceCenterManager,ProductTeam,Finance,Analytics,DevSupport")]
        public ActionResult Index()
        {
          
             _landingPageModel = new LandingPageModel();

            FilterTabUiLinksBasedOnRoles();

            ViewData["uiBrandRegionTabs"] = _landingPageModel.LandingPageTabs;

            return View();
        }

        private void FilterTabUiLinksBasedOnRoles()
        {
            LandingPageLayoutLoader landingPageLayoutLoader = new LandingPageLayoutLoader();

            foreach (LandingPageTab configTab in landingPageLayoutLoader.GetConfiguration())
            {
                var userAllowedTab = new LandingPageTab {Sections = new List<LandingPageSection>()};

                FilterSectionItems(configTab, userAllowedTab);
            }
        }

        private void FilterSectionItems(LandingPageTab configTab, LandingPageTab userAllowedTab)
        {
            foreach (LandingPageSection configSection in configTab.Sections)
            {
                var userAllowedSection = new LandingPageSection {MenuItems = new List<LandingPageSectionMenuItem>()};
                FilterMenuItems(configSection, userAllowedSection, userAllowedTab);
            }

            if (userAllowedTab.Sections.Count > 0)
            {
                userAllowedTab.Key = configTab.Key;
                userAllowedTab.Text = configTab.Text;
                _landingPageModel.LandingPageTabs.Add(userAllowedTab);
            }
        }

        private void FilterMenuItems(LandingPageSection configSection, LandingPageSection userAllowedSection, LandingPageTab userAllowedTab)
        {
            foreach (LandingPageSectionMenuItem configMenuItem in configSection.MenuItems)
            {
                if (ResourceToApplicationRolesMapper.IsUserRoleAllowedForResource(configMenuItem.Key,User))
                {
                    userAllowedSection.MenuItems.Add(configMenuItem);
                }
            }

            if (userAllowedSection.MenuItems.Count > 0)
            {
                userAllowedSection.Key = configSection.Key;
                userAllowedSection.Text = configSection.Text;
                userAllowedTab.Sections.Add(userAllowedSection);
            }
        }
       
    }
}