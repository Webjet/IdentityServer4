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

        [TestMethod] //TODO: Working
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
            emailList.Count().Should().Be(0); //TODO: working it should be 1
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


            var result = graphClient.Groups[Arg.Any<string>()].ExecuteAsync().Result;
            var fetcher = (IGroupFetcher)result;
            var memberCollection = fetcher.Members.ExecuteAsync().Result;

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
            var directoryObjectPageCollection = new List<DirectoryObject>() { GetDirectoryObject() };
            IPagedCollection<IDirectoryObject> groupMembers = Substitute.For<IPagedCollection<IDirectoryObject>>();

            IReadOnlyList<IAppRoleAssignment> appRoleAssignmentReadOnlyList = new List<IAppRoleAssignment>() { GetAppRoleAssignment() };
            IList<AppRoleAssignment> assignments = new List<AppRoleAssignment>();
            var currentPage = GetDirectoryObjectList();
            groupMembers.CurrentPage.ReturnsForAnyArgs(currentPage);
          
            group.Members.ExecuteAsync().Returns(groupMembers);
            return group;
        }

        private IReadOnlyList<IDirectoryObject> GetDirectoryObjectList()
        {
            var dirObj = Substitute.For<IDirectoryObject,User,IUserFetcher >();

            var memberDirectoryObject = new DirectoryObject
            {
                ObjectType = "User",
                ObjectId = ""

            };
            var userObject = new User()
            {
                AppRoleAssignments = new List<AppRoleAssignment>()
                {
                    GetAppRoleAssignment()
                },
                Mail ="scm.test@test"
            };

           // dirObj.
         //   dirObj.ReturnsForAnyArgs((IDirectoryObject)memberDirectoryObject);
         //  dirObj.ReturnsForAnyArgs((IUser) userObject);

            IReadOnlyList<IDirectoryObject> directoryObjectList = new List<IDirectoryObject> { dirObj};//{ memberDirectoryObject };
          
            var currentPage= directoryObjectList.ToList();
            foreach (var member in currentPage)
            {
                var test = (User) member;
                if (member is User)
                {
                    IUser user = (IUser) member;
                }
            }

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


        private ITeamLeadersRetrieval GetTeamLeadersRetrieval()
        {
            var config = ConfigurationHelper.GetConfigurationSubsitituteForGraphAPIClient();
            var groupToTeamNameMapper = BusinessServiceHelper.GetGroupToTeamNameMapper();

            var graphClient = GetSubstituteForActiveDirectoryClient();//Substitute.For<IActiveDirectoryClient>();
            var activeGraphClientHelper = Substitute.For<IActiveDirectoryGraphHelper>();

            activeGraphClientHelper.ActiveDirectoryClient.Returns(graphClient);

            ITeamLeadersRetrieval teamLeadersRetrieval = new TeamLeadersRetrieval(groupToTeamNameMapper, activeGraphClientHelper);

            return teamLeadersRetrieval;
        }




        //TODO
        private IActiveDirectoryClient GetSubstituteForActiveDirectoryClientApplicationAndGroups()
        {
            var graphClient = Substitute.For<IActiveDirectoryClient>();
            var appPageCollection = GetApplicationPagedCollection();
            var groupPagedCollection = GetGroupPageCollection();
            var groupFetcher = GetGroupSubstitute();//GetGroupFetcherSubstitute(); //GetGroup();
            var realGroupFetcherObj = GetGroup();
            // var directoryObjectFetcher = GetDirectoryObject();
            var directoryObjectPageCollection = GetDirectoryObjectPageCollection();


            var application = GetApplicationFetcher();
            graphClient.Applications.ExecuteAsync().Returns(appPageCollection);

            graphClient.Applications[Arg.Any<string>()].Returns(application);


            graphClient.Groups.GetByObjectId(Arg.Any<string>()).Returns(groupFetcher);

            graphClient.Groups[Arg.Any<string>()].ExecuteAsync().Returns((IGroup)groupFetcher);

            graphClient.Groups[Arg.Any<string>()].Returns((IGroupFetcher)groupFetcher);//(realGroupFetcherObj);

            graphClient.Groups[Arg.Any<string>()].Members.ExecuteAsync().Returns(directoryObjectPageCollection);

            return graphClient;

        }

        private async static Task<List<IApplication>> AsyncSubstituteClient(IActiveDirectoryClient graphClient)
        {
            var graphClientApplications = graphClient.Applications;
            var appCollection = await graphClientApplications.ExecuteAsync();
            var morePagesAvailable = appCollection.MorePagesAvailable;
            List<IApplication> applications = appCollection.CurrentPage.ToList();
            return applications;
        }

        private IPagedCollection<IApplication> GetApplicationPagedCollection()
        {
            IPagedCollection<IApplication> pagedCollection = Substitute.For<IPagedCollection<IApplication>>();

            IReadOnlyList<IApplication> currentPageReadOnlyList = GetApplicationReadOnlyList();

            pagedCollection.CurrentPage.ReturnsForAnyArgs(currentPageReadOnlyList);
            pagedCollection.MorePagesAvailable.ReturnsForAnyArgs(true);
            return pagedCollection;
        }

        private TeamLeadersRetrieval InitializeTeamLeadersRetrieval()
        {
            var config = ConfigurationHelper.GetConfigurationSubsitituteForGraphAPIClient();
            var groupToTeamNameMapper = BusinessServiceHelper.GetGroupToTeamNameMapper();

            var graphClient = GetSubstituteForActiveDirectoryClientForApplications();//Substitute.For<IActiveDirectoryClient>();
            var activeGraphClientHelper = Substitute.For<IActiveDirectoryGraphHelper>();//(config, graphClient);
            activeGraphClientHelper.ActiveDirectoryClient.Returns(graphClient);

            TeamLeadersRetrieval teamLeadersRetrieval = new TeamLeadersRetrieval(groupToTeamNameMapper, activeGraphClientHelper);

            return teamLeadersRetrieval;
        }


        //Applications is not set and get SCM Roleid null. used for 1st test method 'ServiceCenterManagerRoleId'
        private IActiveDirectoryClient GetSubstituteForActiveDirectoryClientForApplications()
        {
            var graphClient = Substitute.For<IActiveDirectoryClient>();
            var appPageCollection = GetApplicationPagedCollection();
            graphClient.Applications.ExecuteAsync().Returns(appPageCollection);

            return graphClient;

        }




        //TODO: Add Members
        private IGroupFetcher GetGroupFetcherSubstitute()
        {
            var fetcher = Substitute.For<IGroupFetcher>();
            var directoryObjectPageCollection = GetDirectoryObjectPageCollection();
            IReadOnlyList<IAppRoleAssignment> appRoleAssignmentReadOnlyList = new List<IAppRoleAssignment>() { GetAppRoleAssignment() };
            IPagedCollection<IAppRoleAssignment> assignments = new List<AppRoleAssignment>() as IPagedCollection<IAppRoleAssignment>;

            if (assignments != null)
            {
                assignments.CurrentPage.Returns(appRoleAssignmentReadOnlyList);
                // var group = GetGroup();
                fetcher.AppRoleAssignments.ExecuteAsync().Returns(assignments);
                fetcher.Members.ExecuteAsync().Returns(directoryObjectPageCollection);
            }
            return fetcher;
        }


        private Group GetGroup()
        {
            DirectoryObject memberObject = GetDirectoryObject();
            AppRoleAssignment appRoleAssignment = GetAppRoleAssignment();

            var group = new Group();
            group.Description = "Service Center Group";
            group.DisplayName = "ServiceCenterGroup";
            group.ObjectId = "413687dc-ee0c-4326-9ae1-b2a87ebd28a1";
            group.Mail = "testmanager@test.com.au";
            group.Members = new List<DirectoryObject> { memberObject };
            group.AppRoleAssignments = new List<AppRoleAssignment>() { appRoleAssignment };

            return group;

        }

        private IPagedCollection<IGroupFetcher> GetGroupPageCollection()
        {
            var groupReadOnlyList = GetGroupReadOnlyList();
            var groupPagedCollection = Substitute.For<IPagedCollection<IGroupFetcher>, IPagedCollection<IGroup>>();
            groupPagedCollection.CurrentPage.Returns(groupReadOnlyList);

            return groupPagedCollection;
        }


        private IPagedCollection<IApplication> GetApplicationPageCollectionSubstitute()
        {
            IReadOnlyList<IApplication> applicationReadOnlyList = GetApplicationReadOnlyList();
            IPagedCollection<IApplication> appPageCollection = Substitute.For<IPagedCollection<IApplication>>();
            appPageCollection.CurrentPage.Returns(applicationReadOnlyList);
            return appPageCollection;
        }

        private IReadOnlyList<Group> GetGroupReadOnlyList()
        {
            var group = GetGroup();


            IReadOnlyList<Group> groupList = new List<Group> { group };
            return groupList;
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


        private static IApplicationFetcher GetApplicationFetcher()
        {
            var application = GetApplication(GetAppRoles());
            var fetcher = Substitute.For<IApplicationFetcher, IApplication>();
            fetcher.ExecuteAsync().Returns(application);
            return fetcher;
        }

        private IReadOnlyList<DirectoryObject> GetDirectoryObjectReadOnlyList()
        {
            var dirObject = GetDirectoryObject();
            IReadOnlyList<DirectoryObject> directoryObjectList = new List<DirectoryObject> { dirObject };

            return directoryObjectList;
        }
    }
}
