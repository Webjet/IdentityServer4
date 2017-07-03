using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
    public interface ITeamLeadersRetrieval
    {
        Task<List<string>> GetServiceCenterTeamLeaderEmailListAsync(ClaimsPrincipal loggedUser);
    }
    public class TeamLeadersRetrieval : ITeamLeadersRetrieval
    {
        //TODO: temp public for testing
        public IActiveDirectoryClient _graphClient;
        private static readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly GroupToTeamNameMapper _groupToTeamNameMapper;
        private const string ServiceCenterManagerRole = "ServiceCenterManager"; //TODO: consider to move generic Constant class e.g "Roles"


        public TeamLeadersRetrieval(GroupToTeamNameMapper groupToTeamNameMapper, IActiveDirectoryGraphHelper graphHelper)
        {

            try
            {
                _graphClient = graphHelper.ActiveDirectoryClient;
                _groupToTeamNameMapper = groupToTeamNameMapper;
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

            if (serviceCenterManagerRoleId == Guid.Empty)
            {
                _logger.Log(LogLevel.Warn, "There is no role with 'Service Center Manager' in Azure application: " + applicationId);
                return null;
            }

            GroupToTeamNameMapGroupToTeamName serviceCenterGroup = GetLoggedUserTeamGroup(loggedUser);
            if (serviceCenterGroup == null)
            {
                _logger.Log(LogLevel.Info, "Logged-in user does not belong to 'Service Center Team Group': " + loggedUser.Identity.Name);
                return null;
            }


            var groupMembers = await GetServiceCenterGroupMembersAsync(serviceCenterGroup.GroupId); //(groupId);
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
                                var moreUserRolesAvailable = false;
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
                _logger.Log(LogLevel.Error, "No member found for azure groupId: " + serviceCenterGroup.GroupId);

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
            //In past we were getting appid from 'aud' but now we are getting it from proper property 'appid'
            // var applicationId = userClaims.FirstOrDefault(c => c.Type == "aud")?.Value;
            var applicationId = userClaims.FirstOrDefault(c => c.Type == "appid")?.Value;
            _logger.Log(LogLevel.Info, "ApplicationId: " + applicationId);

            return applicationId;
        }


        private GroupToTeamNameMapGroupToTeamName GetLoggedUserTeamGroup(ClaimsPrincipal loggedUser)
        {
            var userClaims = ((ClaimsIdentity)loggedUser.Identity).Claims;

            var groupIds = userClaims
               ?.Where(c => c.Type == "groups")?.Select(c => c.Value);

            return _groupToTeamNameMapper.GetTeamGroup(groupIds);

        }

        private async Task<Guid> GetRoleIdForServiceCenterManagerRoleAsync(string applicationId)
        {
            Guid roleId = Guid.Empty;
            var graphClientApplications = _graphClient.Applications;
            var appCollection = await graphClientApplications.ExecuteAsync();
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


        private static Guid GetServiceCenterManagerRoleId(IApplication adminPortalApplication, Guid serviceCenterManagerRoleId)
        {
            IList<AppRole> appRoles = adminPortalApplication.AppRoles;
            AppRole serviceCenterManagerRole = appRoles.FirstOrDefault(roles => roles.Value == ServiceCenterManagerRole);
            if (serviceCenterManagerRole != null)
            {
                serviceCenterManagerRoleId = serviceCenterManagerRole.Id;
            }
            return serviceCenterManagerRoleId;
        }


        private async Task<IPagedCollection<IDirectoryObject>> GetServiceCenterGroupMembersAsync(string groupId)
        {
            IPagedCollection<IDirectoryObject> groupMembers = null;
            IGroup group = await _graphClient.Groups[groupId].ExecuteAsync();

            if (group != null)
            {
                var groupFetcher = (IGroupFetcher)group;
                groupMembers = await GetGroupMembersAsync(groupFetcher);

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

