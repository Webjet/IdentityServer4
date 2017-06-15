using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AdminPortal.BusinessServices.Common;
using AdminPortal.BusinessServices.GraphApiHelper;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using Microsoft.Extensions.Configuration;
using NLog;


namespace AdminPortal.BusinessServices
{
    public class TeamLeadersRetrieval
    {
        //TODO: temp public for testing
        public ActiveDirectoryClient _graphClient;
        private static readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();

        public static IConfigurationRoot ConfigurationRoot
        {
            private get; set;
        }

        public TeamLeadersRetrieval(IConfigurationRoot appConfig = null)
        {
            appConfig = appConfig ?? ConfigurationRoot;
            try
            {
                _graphClient = new ActiveDirectoryGraphHelper(appConfig).GetActiveDirectoryGraphClient();
          
            }
            catch (Exception ex)
            {
                throw new AuthenticationException(HttpStatusCode.BadGateway, "Unable to get Active Directory Graph API client." + ex.Message);
            }
        }
        public async Task<List<string>> GetServiceCenterTeamLeaderEmailListAsync(ClaimsPrincipal loggedUser)
        {
            List<string> serviceCenterTeamLeads = null;
           
            string applicationId = GetLoggedUserApplicationId(loggedUser);
            Guid serviceCenterManagerRoleId = await GetRoleIdForServiceCenterManagerRoleAsync(applicationId);

            if (serviceCenterManagerRoleId.Equals(Guid.Empty))
            {
                return null; //TODO: log warning
            }

            string groupId = GetLoggedUserGroupId(loggedUser);
            var groupMembers = await GetServiceCenterGroupMembersAsync(groupId);
            if (groupMembers != null)
            {
                serviceCenterTeamLeads = new List<string>();
                var moreGroupMembersAvailable = false;
                do
                {
                    moreGroupMembersAvailable = groupMembers.MorePagesAvailable;
                    var users = groupMembers.CurrentPage.ToList();
                    foreach (var member in users)
                    {
                        if (member is User)
                        {
                            IUser user = (IUser)member;
                            string memberEmailAddress = GetMemberEmailAddress(user);
                            IUserFetcher userFetcher = (IUserFetcher)user;
                            var userAppRoleAssignments = userFetcher.AppRoleAssignments.ExecuteAsync().Result;
                            if (userAppRoleAssignments != null)
                            {
                                var moreUserRolesAvailable =false;
                                do
                                {
                                    moreUserRolesAvailable = userAppRoleAssignments.MorePagesAvailable;
                                    IList<IAppRoleAssignment> assignments = userAppRoleAssignments.CurrentPage.ToList();
                                    IAppRoleAssignment scmAppRoleAssignment = null;
                                    scmAppRoleAssignment = assignments.FirstOrDefault(ara => ara.Id.Equals(serviceCenterManagerRoleId));
                                    
                                    if (scmAppRoleAssignment != null)
                                    {
                                        serviceCenterTeamLeads.Add(memberEmailAddress);
                                        break;
                                    }
                                    if (moreUserRolesAvailable)
                                    {
                                        userAppRoleAssignments = await userAppRoleAssignments.GetNextPageAsync();
                                    }
                                } while (moreUserRolesAvailable);
                            }
                        }

                    }

                    if (moreGroupMembersAvailable)
                    {
                        groupMembers = await groupMembers.GetNextPageAsync();
                    }
                } while (moreGroupMembersAvailable); 

            }
            else
            {
                _logger.Log(LogLevel.Error, "No member found for azure groupId: " + groupId);
              
            }

            return serviceCenterTeamLeads;
        }

       
        private string GetMemberEmailAddress(IUser member)
        {
            string memberEmailAddress;
            if (!string.IsNullOrEmpty(member.Mail))
            {
                memberEmailAddress = member.Mail;
            }
            else if (member.OtherMails != null && member.OtherMails.Count > 0)
            {
                memberEmailAddress = member.OtherMails.FirstOrDefault();
            }
            else
            {
             
                _logger.Log(LogLevel.Warn, "Email address is not found for member: " + member.DisplayName);
                memberEmailAddress = member.MailNickname;
            }
            return memberEmailAddress;
        }
        private string GetLoggedUserApplicationId(ClaimsPrincipal loggedUser)
        {
            var userClaims = ((ClaimsIdentity)loggedUser.Identity).Claims;
            var applicationId = userClaims.FirstOrDefault(c => c.Type == "aud")?.Value;
            return applicationId;
        }

        //TODO: Call Alvin's method once fixed with GroupIDs in GetTeamNameForUser() by him
        private string GetLoggedUserGroupId(ClaimsPrincipal loggedUser)
        {
            var userClaims = ((ClaimsIdentity)loggedUser.Identity).Claims;
            var groupId = userClaims.FirstOrDefault(c => c.Type == "groups")?.Value;
            return groupId;
        }

        private async Task<Guid> GetRoleIdForServiceCenterManagerRoleAsync(string applicationId)
        {
            Guid roleId = Guid.Empty;
            IPagedCollection<IApplication> appCollection = await _graphClient.Applications.ExecuteAsync();
            var morePagesAvailable = false;
            do
            {
                 morePagesAvailable = appCollection.MorePagesAvailable;
                List<IApplication> applications = appCollection.CurrentPage.ToList();
                IApplication adminPortalApplication = applications.FirstOrDefault(ap => ap.AppId == applicationId);
                if (adminPortalApplication != null)
                {
                    roleId = GetServiceCenterManagerRoleId(adminPortalApplication, roleId);
                    break;
                }
                if (morePagesAvailable)
                {
                    appCollection = await appCollection.GetNextPageAsync();
                }
            } while (morePagesAvailable);

            return roleId;
        }

        //TODO: get RoleName constant 'ServiceCenterManager' after Alvin Check In
        private static Guid GetServiceCenterManagerRoleId(IApplication adminPortalApplication, Guid serviceCenterManagerRoleId)
        {
            IList<AppRole> appRoles = adminPortalApplication.AppRoles;
            AppRole serviceCenterManagerRole = appRoles.FirstOrDefault(roles => roles.Value == "ServiceCenterManager");
            if (serviceCenterManagerRole != null)
            {
                serviceCenterManagerRoleId = serviceCenterManagerRole.Id;
            }
            return serviceCenterManagerRoleId;
        }

        //TODO: get GroupName constant 'Service Centre' after Alvin Check In
        private async Task<IPagedCollection<IDirectoryObject>> GetServiceCenterGroupMembersAsync(string groupId)
        {
            IPagedCollection<IDirectoryObject> groupMembers = null;
            IGroup group = await _graphClient.Groups[groupId].ExecuteAsync();
            if (group != null)
            {
                if (group.DisplayName.Equals("Service Centre", StringComparison.OrdinalIgnoreCase))
                {
                    groupMembers = await GetGroupMembersAsync((IGroupFetcher)group);
                }
            }
            return groupMembers;
        }


        private static async Task<IPagedCollection<IDirectoryObject>> GetGroupMembersAsync(IGroupFetcher groupFetcher)
        {
            IPagedCollection<IDirectoryObject> groupMembers = null;
            groupMembers = await groupFetcher.Members.ExecuteAsync();
            return groupMembers;
        }

    }
}

