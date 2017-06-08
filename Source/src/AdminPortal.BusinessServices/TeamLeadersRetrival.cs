using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AdminPortal.BusinessServices.Common;
using AdminPortal.BusinessServices.Helper;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using Microsoft.Extensions.Configuration;


namespace AdminPortal.BusinessServices
{
    public class TeamLeadersRetrival
    {
        //TODO: temp public for testing
        public ActiveDirectoryClient _graphClient;
    

        public static IConfigurationRoot ConfigurationRoot
        {
            private get; set;
        }

      

        public TeamLeadersRetrival(IConfigurationRoot appConfig = null)
        {
            appConfig = appConfig ?? ConfigurationRoot;
            _graphClient = new ActiveDirectoryGraphHelper(appConfig).GetActiveDirectoryGraphClient();
        }
        public async Task<List<string>> GetServiceCenterTeamLeaderEmaiListAsync(ClaimsPrincipal loggedUser)
        {
            List<string> serviceCenterTeamLeads = new List<string>();
            string groupId = GetLoggedUserGroupId(loggedUser);
            string applicationId = GetLoggedUserApplicationId(loggedUser);
            Guid serviceCenterManagerRoleId = await GetRoleIdForServiceCenterManagerRoleAsync(applicationId);

            if (serviceCenterManagerRoleId.Equals(Guid.Empty))
            {
                return null;
            }

            var groupMembers = await GetServiceCenterGroupMembersAsync(groupId);
            if (groupMembers != null)
            {
                do
                {
                    var users = groupMembers.CurrentPage.ToList();
                    foreach (var member in users)
                    {
                        if (member is User)
                        {
                            IUser user = (IUser) member;
                            string memberEmailAddress = GetMemberEmailAddress(user);
                            IUserFetcher userFetcher = (IUserFetcher) user;
                            var userAppRoleAssignment = userFetcher.AppRoleAssignments.ExecuteAsync().Result;
                            if (userAppRoleAssignment != null)
                            {
                                do
                                {
                                    IList<IAppRoleAssignment> assignments = userAppRoleAssignment.CurrentPage.ToList();
                                    IAppRoleAssignment scmAppRoleAssignment = null;
                                    scmAppRoleAssignment =
                                        assignments.FirstOrDefault(ara => ara.Id.Equals(serviceCenterManagerRoleId));

                                    if (scmAppRoleAssignment != null)
                                    {
                                        serviceCenterTeamLeads.Add(memberEmailAddress);

                                    }
                                    else if (userAppRoleAssignment.MorePagesAvailable)
                                    {
                                        userAppRoleAssignment = await userAppRoleAssignment.GetNextPageAsync();
                                    }
                                } while (userAppRoleAssignment.MorePagesAvailable);

                            }
                        }

                    }

                    if (groupMembers.MorePagesAvailable)
                    {
                        groupMembers = await groupMembers.GetNextPageAsync();
                    }
                } while (groupMembers.MorePagesAvailable); //(groupMembers != null);

            }

            return serviceCenterTeamLeads;
        }

        //Todo:  not getting Mail , retrieving from OtherMails property- need to check from AAD where to enter email address for user
        private string GetMemberEmailAddress(IUser member)
        {
            string memberEmailAddress;
            if (!string.IsNullOrEmpty(member.Mail))
            {
                memberEmailAddress = member.Mail;
            }
            else if(member.OtherMails!=null && member.OtherMails.Count>0)
            {
                memberEmailAddress = member.OtherMails.FirstOrDefault();
            }
            else
            {
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

        //TODO: Will fetch GroupId after Alvin check in
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

            do
            {
                List<IApplication> applications = appCollection.CurrentPage.ToList();
                IApplication adminPortalApplication = applications.FirstOrDefault(ap => ap.AppId == applicationId);
                if (adminPortalApplication != null)
                {
                    roleId = GetServiceCenterManagerRoleId(adminPortalApplication, roleId);
                }
                else if (appCollection.MorePagesAvailable)
                {
                    appCollection = await appCollection.GetNextPageAsync();
                }
            } while (appCollection.MorePagesAvailable);

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
        public async Task<IPagedCollection<IDirectoryObject>> GetServiceCenterGroupMembersAsync(string groupId)
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

