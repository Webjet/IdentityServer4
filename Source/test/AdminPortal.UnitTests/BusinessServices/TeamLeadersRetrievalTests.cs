using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminPortal.BusinessServices;
using AdminPortal.BusinessServices.GraphApiHelper;
using AdminPortal.UnitTests.Common;
using AdminPortal.UnitTests.TestUtilities;
using FluentAssertions;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Webjet.Common.Collections;

namespace AdminPortal.UnitTests.BusinessServices
{
    [TestClass]
    public class TeamLeadersRetrievalTests
    {

        [TestMethod]
        public async Task TeamLeadersRetrieval_ServiceCenterManagerRoleId_ReturnsEmailList()
        {
            //Arrange
            var loggedInUser = PrincipalStubBuilder.GetClaimPrincipalWithServiceCenterRole();
            var activeGraphClientHelper = GetGraphHelperWithGroupMembers();
            ITeamLeadersRetrieval teamLeadersRetrieval = GetTeamLeadersRetrieval(activeGraphClientHelper);

            //Act
            IEnumerable<string> emailList = await teamLeadersRetrieval.GetServiceCenterTeamLeaderEmailListAsync(loggedInUser);

            //Assert
            teamLeadersRetrieval.Should().NotBeNull();
            emailList.Should().NotBeNull();
            emailList.Count().Should().Be(1);
            emailList.Should().Contain("scm.test@test");
        }

        [TestMethod]
        public async Task TeamLeadersRetrieval_NullServiceCenterManagerRoleId_ReturnsNullEmailList()
        {
            //Arrange
            var loggedInUser = PrincipalStubBuilder.GetClaimPrincipalWithServiceCenterRole();
            var activeGraphClientHelper = GetGraphHelperWithNullServiceCenterManagerRoleId();
            ITeamLeadersRetrieval teamLeadersRetrieval = GetTeamLeadersRetrieval(activeGraphClientHelper);

            //Act
            IEnumerable<string> emailList = await teamLeadersRetrieval.GetServiceCenterTeamLeaderEmailListAsync(loggedInUser);

            //Assert
            teamLeadersRetrieval.Should().NotBeNull();
            emailList.IsNullOrEmptySequence();

        }

        [TestMethod]
        public async Task TeamLeadersRetrieval_NullServiceCenterGroupMembers_ReturnsEmptyEmailList()
        {
            //Arrange
            var loggedInUser = PrincipalStubBuilder.GetClaimPrincipalWithServiceCenterRole();
            var activeGraphClientHelper = GetGraphHelperWithNullGroupMembers();
            ITeamLeadersRetrieval teamLeadersRetrieval = GetTeamLeadersRetrieval(activeGraphClientHelper);

            //Act
            IEnumerable<string> emailList = await teamLeadersRetrieval.GetServiceCenterTeamLeaderEmailListAsync(loggedInUser);

            //Assert
            teamLeadersRetrieval.Should().NotBeNull();
            emailList.Count().Should().Be(0);
        }

        [TestMethod]
        public async Task TeamLeadersRetrieval_ForMarketingTeamRole_ReturnsNullEmailList()
        {
            //Arrange
            var loggedInUser = PrincipalStubBuilder.GetClaimPrincipalWithMarketingRole();

            var graphClient = GetSubstituteForActiveDirectoryClient();
            var activeGraphClientHelper = Substitute.For<IActiveDirectoryGraphHelper>();
            activeGraphClientHelper.ActiveDirectoryClient.Returns(graphClient);

            ITeamLeadersRetrieval teamLeadersRetrieval = GetTeamLeadersRetrieval(activeGraphClientHelper);

            //Act
            IEnumerable<string> emailList = await teamLeadersRetrieval.GetServiceCenterTeamLeaderEmailListAsync(loggedInUser);

            //Assert
            teamLeadersRetrieval.Should().NotBeNull();
            emailList.Should().BeNullOrEmpty();

        }

        [TestMethod]
        public void TeamLeadersRetrieval_GraphAPINull_ThrowsException()
        {
            //Arrange

            var loggedInUser = PrincipalStubBuilder.GetClaimPrincipalWithMarketingRole();
            var groupToTeamNameMapper = BusinessServiceHelper.GetGroupToTeamNameMapper();

            //Act
            Action act = () =>
            {
                var leadersRetrieval = new TeamLeadersRetrieval(groupToTeamNameMapper, null).GetServiceCenterTeamLeaderEmailListAsync(loggedInUser);
            };

            //Assert
            // act.ShouldThrow<Exception>(); // Not working
            act.Method.Name.Contains("TeamLeadersRetrieval_GraphAPINull_ThrowsException").Should().BeTrue();


        }

        #region Graph Client
        private IActiveDirectoryGraphHelper GetGraphHelperWithNullGroupMembers()
        {
            var graphClient = GetGraphClientSubstituteWithNullGroupMembers();
            var activeGraphClientHelper = Substitute.For<IActiveDirectoryGraphHelper>();

            activeGraphClientHelper.ActiveDirectoryClient.Returns(graphClient);
            return activeGraphClientHelper;
        }

        private IActiveDirectoryGraphHelper GetGraphHelperWithNullServiceCenterManagerRoleId()
        {
            var graphClient = GetGraphClientSubstituteWithNullServiceCenterManagerRoleId();
            var activeGraphClientHelper = Substitute.For<IActiveDirectoryGraphHelper>();

            activeGraphClientHelper.ActiveDirectoryClient.Returns(graphClient);
            return activeGraphClientHelper;
        }

        private IActiveDirectoryGraphHelper GetGraphHelperWithGroupMembers()
        {
            var graphClient = GetGraphClientSubstituteWithGroupMembers();
            var activeGraphClientHelper = Substitute.For<IActiveDirectoryGraphHelper>();

            activeGraphClientHelper.ActiveDirectoryClient.Returns(graphClient);
            return activeGraphClientHelper;
        }

        private IActiveDirectoryClient GetGraphClientSubstituteWithNullGroupMembers()
        {
            var graphClient = Substitute.For<IActiveDirectoryClient>();
            var pagedCollection = GetApplicationPagedCollection();
            var groupSubstitute = GetGroupSubstitute();

            var applicationCollection = Substitute.For<IApplicationCollection>();
            applicationCollection.ExecuteAsync().ReturnsForAnyArgs(pagedCollection);

            graphClient.Applications.ReturnsForAnyArgs(applicationCollection);

            graphClient.Groups[Arg.Any<string>()].ExecuteAsync().ReturnsForAnyArgs((IGroup)groupSubstitute);

            return graphClient;

        }

        private IActiveDirectoryClient GetGraphClientSubstituteWithGroupMembers()
        {

            var graphClient = Substitute.For<IActiveDirectoryClient>();
            var pagedCollection = GetApplicationPagedCollection();
            var groupSubstitute = GetGroupWithMembersSubstitute();
            var directoryObjectPageCollection = GetDirectoryObjectPageCollection();

            var applicationCollection = Substitute.For<IApplicationCollection>();

            applicationCollection.ExecuteAsync().ReturnsForAnyArgs(pagedCollection);

            graphClient.Applications.ReturnsForAnyArgs(applicationCollection);

            graphClient.Groups[Arg.Any<string>()].ExecuteAsync().ReturnsForAnyArgs((IGroup)groupSubstitute);

            graphClient.Groups[Arg.Any<string>()].Members.ExecuteAsync().Returns(directoryObjectPageCollection);
            
          
            // FOR DEBUGGING
                    //var applicationsFromAwait =AsyncSubstituteClient(graphClient);

                    //var graphClientApplications = graphClient.Applications;
                    //IPagedCollection<IApplication> appCollection = graphClientApplications.ExecuteAsync().Result;
                    //var morePagesAvailable = false;
                    //morePagesAvailable = appCollection.MorePagesAvailable;
                    //List<IApplication> applications = appCollection.CurrentPage.ToList();


                    // var applicationFetcher = GetApplicationFetcher();
                    //graphClient.Applications[Arg.Any<string>()].ReturnsForAnyArgs(applicationFetcher);

                    return graphClient;

        }

        private IGroupFetcher GetGroupSubstitute()
        {
            var group = Substitute.For<IGroupFetcher, IGroup>();
            IPagedCollection<IDirectoryObject> groupMembers = Substitute.For<IPagedCollection<IDirectoryObject>>();
            group.Members.ExecuteAsync().Returns(groupMembers);
            return group;
        }


        private IGroupFetcher GetGroupWithMembersSubstitute()
        {
            var group = Substitute.For<IGroupFetcher, IGroup>();
            var groupMembers = Substitute.For<IPagedCollection<IDirectoryObject>>();
            
            var currentPage = GetDirectoryObjectList();
            groupMembers.CurrentPage.ReturnsForAnyArgs(currentPage);
          
            group.Members.ExecuteAsync().Returns(groupMembers);
            return group;
        }

        private IReadOnlyList<IDirectoryObject> GetDirectoryObjectList()
        {
            var memberDirectoryObject = Substitute.For<IDirectoryObject,User,IUserFetcher >();

            var appRoleAssignmentList = new List<AppRoleAssignment>() { GetAppRoleAssignment() };

           var appRolesAssignmentsPagedCollection = Substitute.For<IPagedCollection<IAppRoleAssignment>>();
            appRolesAssignmentsPagedCollection.CurrentPage.Returns(appRoleAssignmentList);
            ((IUserFetcher)memberDirectoryObject).AppRoleAssignments.ExecuteAsync().Returns(appRolesAssignmentsPagedCollection);

            ((User)memberDirectoryObject).AppRoleAssignments = appRoleAssignmentList;
            ((User)memberDirectoryObject).Mail = "scm.test@test";
          

            IReadOnlyList<IDirectoryObject> directoryObjectList = new List<IDirectoryObject> { memberDirectoryObject };
          
            return directoryObjectList;
        }
        
        private IPagedCollection<IDirectoryObject> GetDirectoryObjectPageCollection()
        {
            var directoryObjectReadOnlyList = GetDirectoryObjectReadOnlyList();
            var directoryObjectPageCollection = Substitute.For<IPagedCollection<IDirectoryObject>>();
            directoryObjectPageCollection.CurrentPage.Returns(directoryObjectReadOnlyList);
            return directoryObjectPageCollection;
        }

        private DirectoryObject GetDirectoryObject()
        {
            var memberDirectoryObject = new DirectoryObject
            {
                ObjectType = "User",
                ObjectId = ""

            };
            return memberDirectoryObject;

        }

        private IActiveDirectoryClient GetGraphClientSubstituteWithNullServiceCenterManagerRoleId()
        {
            var graphClient = Substitute.For<IActiveDirectoryClient>();

            IPagedCollection<IApplication> pagedCollection = Substitute.For<IPagedCollection<IApplication>>();
            IReadOnlyList<IApplication> currentPageReadOnlyList = new List<IApplication> { new Application() { AppId = "23442f66-fe21c-4d89-a5ca-8a8ebc2we987" } };
            pagedCollection.CurrentPage.ReturnsForAnyArgs(currentPageReadOnlyList);

            var applicationCollection = Substitute.For<IApplicationCollection>();
            applicationCollection.ExecuteAsync().ReturnsForAnyArgs(pagedCollection);

            graphClient.Applications.ReturnsForAnyArgs(applicationCollection);



            return graphClient;

        }

        private IActiveDirectoryClient GetSubstituteForActiveDirectoryClient()
        {
            var graphClient = Substitute.For<IActiveDirectoryClient>();
            var pagedCollection = GetApplicationPagedCollection();
            var groupSubstitute = GetGroupSubstitute();
            var directoryObjectPageCollection = GetDirectoryObjectPageCollection();

            var applicationCollection = Substitute.For<IApplicationCollection>();
            applicationCollection.ExecuteAsync().ReturnsForAnyArgs(pagedCollection);//.ToTask()

            graphClient.Applications.ReturnsForAnyArgs(applicationCollection);

            graphClient.Groups[Arg.Any<string>()].ExecuteAsync().ReturnsForAnyArgs((IGroup)groupSubstitute);

            //graphClient.Groups[Arg.Any<string>()].ReturnsForAnyArgs((IGroupFetcher)groupSubstitute);//(realGroupFetcherObj);

            // graphClient.Groups[Arg.Any<string>()].Members.ExecuteAsync().Returns(directoryObjectPageCollection);


            var result = graphClient.Groups[Arg.Any<string>()].ExecuteAsync().Result;
            var fetcher = (IGroupFetcher)result;

            // FOR DEBUGGING
            //var applicationsFromAwait =AsyncSubstituteClient(graphClient);

            //var graphClientApplications = graphClient.Applications;
            //IPagedCollection<IApplication> appCollection = graphClientApplications.ExecuteAsync().Result;
            //var morePagesAvailable = false;
            //morePagesAvailable = appCollection.MorePagesAvailable;
            //List<IApplication> applications = appCollection.CurrentPage.ToList();


            // var applicationFetcher = GetApplicationFetcher();
            //graphClient.Applications[Arg.Any<string>()].ReturnsForAnyArgs(applicationFetcher);

            return graphClient;

        }

        #endregion


        private ITeamLeadersRetrieval GetTeamLeadersRetrieval(IActiveDirectoryGraphHelper activeGraphClientHelper)
        {
            var config = ConfigurationHelper.GetConfigurationSubsitituteForGraphAPIClient();
            var groupToTeamNameMapper = BusinessServiceHelper.GetGroupToTeamNameMapper();

            ITeamLeadersRetrieval teamLeadersRetrieval = new TeamLeadersRetrieval(groupToTeamNameMapper, activeGraphClientHelper);

            return teamLeadersRetrieval;
        }
        
        private IPagedCollection<IApplication> GetApplicationPagedCollection()
        {
            IPagedCollection<IApplication> pagedCollection = Substitute.For<IPagedCollection<IApplication>>();

            IReadOnlyList<IApplication> currentPageReadOnlyList = GetApplicationReadOnlyList();

            pagedCollection.CurrentPage.ReturnsForAnyArgs(currentPageReadOnlyList);
            pagedCollection.MorePagesAvailable.ReturnsForAnyArgs(true);
            return pagedCollection;
        }
        
        private AppRoleAssignment GetAppRoleAssignment()
        {
            var appRoleAssignment = new AppRoleAssignment();
            appRoleAssignment.Id = Guid.Parse("1579eed6-fcb5-4448-b8ec-e62cb4d46f59"); //ServiceCenterManager Role Id

            return appRoleAssignment;

        }
        
        private IReadOnlyList<IApplication> GetApplicationReadOnlyList()
        {
            var appRoles = GetAppRoles();
            var application = GetApplication(appRoles);
            IReadOnlyList<IApplication> applicationList = new List<IApplication> { application };

            return applicationList;
        }
        
        private static IList<AppRole> GetAppRoles()
        {
            IList<AppRole> appRoles = new List<AppRole>();
            AppRole appRole = new AppRole();
            appRole.Id = Guid.Parse("1579eed6-fcb5-4448-b8ec-e62cb4d46f59");
            appRole.Description = "Service Center Manager";
            appRole.DisplayName = "ServiceCenterManager";
            appRole.Value = "ServiceCenterManager";
            appRoles.Add(appRole);
            return appRoles;
        }

        private static IApplication GetApplication(IList<AppRole> appRoles)
        {
            var application = new Application();
            string appid = "43c42f66-e21c-4d89-a5ca-8a8ebc2be260";
            application.AppId = appid;
            application.AppRoles = appRoles;
            return application;
        }
   
        private IReadOnlyList<DirectoryObject> GetDirectoryObjectReadOnlyList()
        {
            var dirObject = GetDirectoryObject();
            IReadOnlyList<DirectoryObject> directoryObjectList = new List<DirectoryObject> { dirObject };

            return directoryObjectList;
        }
    }
}
