using AutoMoq;
using FineCodeCoverage.Output;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FineCodeCoverageTests
{
    class ReportViews_Tests
    {
        private AutoMoqer autoMoqer;
        private ReportViews reportViews;

        [SetUp]
        public void Setup()
        {
            autoMoqer = new AutoMoqer();
            reportViews = autoMoqer.Create<ReportViews>();
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue());
        }

        private void SetupInitialSolutionOption(ReportViewSolutionOptionValue value)
        {
            var mockReportViewSolutionOption = autoMoqer.GetMock<IReportViewSolutionOption>();
            mockReportViewSolutionOption.SetupAllProperties();
            mockReportViewSolutionOption.SetupGet(reportViewSolutionOption => reportViewSolutionOption.Value).Returns(value);
        }

        [TestCase(ReportStyle.Assembly)]
        [TestCase(ReportStyle.Source)]
        public void Should_Initially_Have_ReportStyle_From_IReportViewSolutionOption_Value(ReportStyle reportStyle)
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue { ReportStyle = reportStyle });

            Assert.That(reportViews.ReportStyle, Is.EqualTo(reportStyle));
        }

        // GetState tests
        [TestCase(ReportStyle.Assembly)]
        [TestCase(ReportStyle.Source)]
        public void Should_Initially_Have_ReportStyle_State_From_IReportViewSolutionOption_Value(ReportStyle reportStyle)
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue { ReportStyle = reportStyle });

            var state = reportViews.GetState();
            
            Assert.That(state.ReportStyle, Is.EqualTo(reportStyle));
        }

        // VS2022
        [Test]
        public void Should_Have_RepositoryPaths_From_The_Git_Service_When_CanUseChangeset()
        {
            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);
            var repositoryPaths = new List<string> { "repopath1", "repopath2" };
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths);
            
            var state = reportViews.GetState();

            Assert.That(state.RepositoryPaths, Is.EquivalentTo(repositoryPaths));
        }

        [Test]
        public void Should_Not_Have_The_Selected_Repository_Path_In_Repository_Paths_If_Repository_Does_Not_Exist()
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue { SelectedRepository = "selectedrepopath", SelectedBranchName = "selectedbranch" });

            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);
            var repositoryPaths = new List<string> { "selectedrepopath", "repopath2" };
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths);

            var state = reportViews.GetState();

            Assert.That(state.RepositoryPaths, Is.EquivalentTo(new List<string> { "repopath2" }));
        }

        [Test]
        public void Should_Have_The_Selected_Repository_Path_In_Repository_Paths_If_Repository_Exists()
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue { SelectedRepository = "selectedrepopath", SelectedBranchName = "selectedbranch" });

            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);
            var repositoryPaths = new List<string> { "selectedrepopath", "repopath2" };
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths);
            gitServiceMock.Setup(gitService => gitService.GetRepository("selectedrepopath")).Returns(new Mock<IGitRepo>().Object);
            var state = reportViews.GetState();

            Assert.That(state.RepositoryPaths, Is.EquivalentTo(repositoryPaths));
        }

        private void SetupInvalidSelectedRepository()
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue { SelectedRepository = "removed", SelectedBranchName = "removed" });

            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);
            var repositoryPaths = new List<string> { "repopath1", "repopath2" };
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths);
        }

        [Test]
        public void Should_Not_Use_The_SelectedRepository_If_No_Longer_Exists()
        {
            SetupInvalidSelectedRepository();

            var state = reportViews.GetState();

            Assert.That(state.SelectedRepositoryPath, Is.Null);
            Assert.That(state.SelectedBranchName, Is.Null);
        }

        [Test]
        public void Should_Use_The_SelectedRepository_If_Exists()
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue { SelectedRepository = "selectedrepopath", SelectedBranchName = "selectedbranch" });

            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);
            var repositoryPaths = new List<string> { "repopath1", "selectedrepopath" };
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths);
            var mockGitRepo = new Mock<IGitRepo>();
            gitServiceMock.Setup(gitService => gitService.GetRepository("selectedrepopath")).Returns(mockGitRepo.Object);

            var state = reportViews.GetState();

            Assert.That(state.SelectedRepositoryPath, Is.EqualTo("selectedrepopath"));
        }

        private void SetUpInvalidSelectedBranch()
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue { SelectedRepository = "selectedrepopath", SelectedBranchName = "removed" });

            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);
            var repositoryPaths = new List<string> { "repopath1", "selectedrepopath" };
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths);
            var mockGitRepo = new Mock<IGitRepo>();
            gitServiceMock.Setup(gitService => gitService.GetRepository("selectedrepopath")).Returns(mockGitRepo.Object);
        }

        [Test]
        public void Should_Not_Use_The_SelectedBranch_If_No_Longer_Exists()
        {
            SetUpInvalidSelectedBranch();

            var state = reportViews.GetState();

            Assert.That(state.SelectedRepositoryPath, Is.EqualTo("selectedrepopath"));
            Assert.That(state.SelectedBranchName, Is.Null);
        }

        [Test]
        public void Should_Use_The_SelectedBranch_If_Exists()
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue { SelectedRepository = "selectedrepopath", SelectedBranchName = "selectedbranch" });

            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);
            var repositoryPaths = new List<string> { "repopath1", "selectedrepopath" };
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths);
            var mockGitRepo = new Mock<IGitRepo>();
            mockGitRepo.Setup(gitRepo => gitRepo.HasBranch("selectedbranch")).Returns(true);
            gitServiceMock.Setup(gitService => gitService.GetRepository("selectedrepopath")).Returns(mockGitRepo.Object);

            var state = reportViews.GetState();

            Assert.That(state.SelectedRepositoryPath, Is.EqualTo("selectedrepopath"));
            Assert.That(state.SelectedBranchName, Is.EqualTo("selectedbranch"));
        }

        [Test]
        public void Should_Update_The_IReportViewSolutionOption_Value_SelectedBranchName_When_GetState_And_Initial_Is_Invalid()
        {
            SetUpInvalidSelectedBranch();

            reportViews.GetState();
            
            var optionValue = autoMoqer.GetMock<IReportViewSolutionOption>().Object.Value;
            Assert.That(optionValue.SelectedBranchName, Is.Null);
        }

        [Test]
        public void Should_Update_The_IReportViewSolutionOption_Value_SelectedRepository_When_GetState_And_Initial_Is_Invalid()
        {
            SetupInvalidSelectedRepository();

            reportViews.GetState();

            var optionValue = autoMoqer.GetMock<IReportViewSolutionOption>().Object.Value;
            Assert.That(optionValue.SelectedRepository, Is.Null);
        }

        // VS2019
        [Test]
        public void Should_Not_Be_Able_To_Use_Repositories_When_VS2019()
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue {  });

            var state = reportViews.GetState();

            Assert.That(state.CanUseRepositories, Is.False);
            Assert.That(state.RepositoryPaths, Is.Empty);
        }

        [Test]
        public void Should_Dispose_Of_Selected_IGitRepo_When_GetState_And_It_No_Longer_Exists()
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue { SelectedRepository = "selectedrepopath", SelectedBranchName = "selectedbranch" });

            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);
            var repositoryPaths = new List<string> { "repopath1", "selectedrepopath" };
            gitServiceMock.SetupSequence(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths).Returns(new List<string> { "repopath1" });
            var mockGitRepo = new Mock<IGitRepo>();
            gitServiceMock.Setup(gitService => gitService.GetRepository("selectedrepopath")).Returns(mockGitRepo.Object);

            reportViews.GetState();
            reportViews.GetState();

            mockGitRepo.Verify(gitRepo => gitRepo.Dispose());
        }

        [Test]
        public void Should_Store_The_Selected_GitRepo()
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue { SelectedRepository = "selectedrepopath", SelectedBranchName = "selectedbranch" });

            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);
            var repositoryPaths = new List<string> { "repopath1", "selectedrepopath" };
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths);
            var mockGitRepo = new Mock<IGitRepo>();
            gitServiceMock.Setup(gitService => gitService.GetRepository("selectedrepopath")).Returns(mockGitRepo.Object);

            reportViews.GetState();
            reportViews.GetState();

            gitServiceMock.Verify(gitService => gitService.GetRepository("selectedrepopath"), Times.Once());
        }

        [Test]
        public void Should_Use_Selected_GitRepo_For_GetBranches_If_Matches_Requested_Repository()
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue { SelectedRepository = "selectedrepopath", SelectedBranchName = "selectedbranch" });

            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);
            var repositoryPaths = new List<string> { "repopath1", "selectedrepopath" };
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths);
            var mockGitRepo = new Mock<IGitRepo>();
            var selectedRepoBranches = new List<string> { "Branch1", "Branch2" };
            mockGitRepo.Setup(gitRepo => gitRepo.GetBranches()).Returns(selectedRepoBranches);
            gitServiceMock.Setup(gitService => gitService.GetRepository("selectedrepopath")).Returns(mockGitRepo.Object);

            reportViews.GetState();

            Assert.That(reportViews.GetBranches("selectedrepopath"), Is.SameAs(selectedRepoBranches));
        }

        [Test]
        public void Should_Dispose_Of_SelectedRepo_If_Deleted_And_Return_No_Branches()
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue { SelectedRepository = "selectedrepopath", SelectedBranchName = "selectedbranch" });

            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);
            var repositoryPaths = new List<string> { "repopath1", "selectedrepopath" };
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths);
            var mockGitRepo = new Mock<IGitRepo>();
            var selectedRepoBranches = new List<string> { "Branch1", "Branch2" };
            mockGitRepo.Setup(gitRepo => gitRepo.GetBranches()).Returns(selectedRepoBranches);
            gitServiceMock.Setup(gitService => gitService.GetRepository("selectedrepopath")).Returns(mockGitRepo.Object);

            reportViews.GetState();

            mockGitRepo.Setup(gitRepo => gitRepo.Deleted()).Returns(true);
            
            Assert.That(reportViews.GetBranches("selectedrepopath"), Is.Empty);
            mockGitRepo.Verify(gitRepo => gitRepo.Dispose());
        }

        [Test]
        public void Should_Create_GitRepo_When_GetBranches_And_No_Selected_GitRepo()
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue { SelectedRepository = null, SelectedBranchName = null });

            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);
            var repositoryPaths = new List<string> { "repopath1" };
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths);
            var mockGitRepo = new Mock<IGitRepo>();
            var repoBranches = new List<string> { "Branch1", "Branch2" };
            mockGitRepo.Setup(gitRepo => gitRepo.GetBranches()).Returns(repoBranches);
            gitServiceMock.Setup(gitService => gitService.GetRepository("repopath")).Returns(mockGitRepo.Object);

            reportViews.GetState();
            gitServiceMock.Verify(gitService => gitService.GetRepository("repopath"), Times.Never());

            var branches = reportViews.GetBranches("repopath");

            Assert.That(branches, Is.EquivalentTo(repoBranches));
        }

        [Test]
        public void Should_Create_New_GitRepo_When_GetBranches_And_Different_Selected_GitRepo_Disposing_Selected()
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue { SelectedRepository = null, SelectedBranchName = null });

            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);
            var repositoryPaths = new List<string> { "repopath1","repopath2" };
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths);
            var mockGitRepo = new Mock<IGitRepo>();
            var repoBranches = new List<string> { "Branch1", "Branch2" };
            mockGitRepo.Setup(gitRepo => gitRepo.GetBranches()).Returns(repoBranches);
            gitServiceMock.Setup(gitService => gitService.GetRepository("repopath1")).Returns(mockGitRepo.Object);
            var mockGitRepo2 = new Mock<IGitRepo>();
            var repoBranches2 = new List<string> { "Branch3", "Branch4" };
            mockGitRepo2.Setup(gitRepo => gitRepo.GetBranches()).Returns(repoBranches2);
            gitServiceMock.Setup(gitService => gitService.GetRepository("repopath2")).Returns(mockGitRepo2.Object);

            reportViews.GetState();
            reportViews.GetBranches("repopath1");
            var branches = reportViews.GetBranches("repopath2");

            Assert.That(branches, Is.EquivalentTo(repoBranches2));
            mockGitRepo.Verify(gitRepo => gitRepo.Dispose());
        }

        private ReportViewSolutionOptionValue UpdateTest(bool repoExists, bool branchExists)
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue
            {
                ReportContent = ReportContentType.Full,
                ReportStyle = ReportStyle.Assembly,
            });
            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);

            var repositoryPaths = new List<string> { "repopath1", "selectedrepopath" };
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths);
            var mockGitRepo = new Mock<IGitRepo>();
            mockGitRepo.Setup(gitRepo => gitRepo.HasBranch("selectedbranch")).Returns(branchExists);
            gitServiceMock.Setup(gitService => gitService.GetRepository("selectedrepopath")).Returns(repoExists ? mockGitRepo.Object : null);
            
            reportViews.Update(ReportStyle.Source, ReportContentType.Changeset, "selectedbranch", "selectedrepopath");

            return autoMoqer.GetMock<IReportViewSolutionOption>().Object.Value; ;
        }

        //Update tests
        [Test]
        public void Should_Update_The_ReportViewSolutionOptionValue_Enum_Properties_When_Update()
        {
            var optionValue = UpdateTest(false, false);

            Assert.That(optionValue.ReportStyle, Is.EqualTo(ReportStyle.Source));
            Assert.That(optionValue.ReportContent, Is.EqualTo(ReportContentType.Changeset));
        }

        [Test]
        public void Should_Set_ReportViewSolutionOptionValue_RepositoryPath_If_Exists_When_Update()
        {
            var optionValue = UpdateTest(true, true);
            
            Assert.That(optionValue.SelectedRepository, Is.EqualTo("selectedrepopath"));
        }

        [Test]
        public void Should_Set_ReportViewSolutionOptionValue_SelectedBranchName_If_Exists_When_Update()
        {
            var optionValue = UpdateTest(true, true);
            
            Assert.That(optionValue.SelectedBranchName, Is.EqualTo("selectedbranch"));
        }

        [Test]
        public void Should_Set_ReportViewSolutionOptionValue_RepositoryPath_SelectedBranchName_Null_When_Update_And_Repository_Does_Not_Exist()
        {
            var optionValue = UpdateTest(false, true);

            Assert.That(optionValue.SelectedRepository, Is.Null);
            Assert.That(optionValue.SelectedBranchName, Is.Null);
        }

        [Test]
        public void Should_Set_ReportViewSolutionOptionValue_SelectedBranchName_Null_When_Update_And_Branch_Does_Not_Exist()
        {
            var optionValue = UpdateTest(true, false);

            Assert.That(optionValue.SelectedBranchName, Is.Null);
        }

        private void Should_Raise_The_Changed_Event(ReportViewSolutionOptionValue initial,ReportViewSolutionOptionValue changes,bool expectedChangesetChanged)
        {
            SetupInitialSolutionOption(initial);
            ReportViewChangedEventArgs reportViewChangedEventArgs = null;
            reportViews.Changed += (s, args) =>
            {
                reportViewChangedEventArgs = args;
            };

            reportViews.Update(changes.ReportStyle, changes.ReportContent, changes.SelectedBranchName, changes.SelectedRepository);

            Assert.That(reportViewChangedEventArgs.ChangesetChanged, Is.EqualTo(expectedChangesetChanged));
        }

        [Test]
        public void Should_Raise_The_Change_Event_ChangesetChanged_False_When_Only_ReportStyleChanged()
        {
            Should_Raise_The_Changed_Event(
                new ReportViewSolutionOptionValue
                {
                    ReportStyle = ReportStyle.Source,
                    ReportContent = ReportContentType.Full,
                    SelectedBranchName = null,
                    SelectedRepository = null
                },
                 new ReportViewSolutionOptionValue
                 {
                     ReportStyle = ReportStyle.Assembly,
                     ReportContent = ReportContentType.Full,
                     SelectedBranchName = null,
                     SelectedRepository = null
                 },
                 false);

        }

        [Test]
        public void Should_Raise_The_Change_Event_ChangesetChanged_True_When_ReportContentType_Changed()
        {
            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);
            var repositoryPaths = new List<string> { "repopath1", "selectedrepopath" };
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths);
            var mockGitRepo = new Mock<IGitRepo>();
            mockGitRepo.Setup(gitRepo => gitRepo.HasBranch("selectedbranch")).Returns(true);
            gitServiceMock.Setup(gitService => gitService.GetRepository("selectedrepopath")).Returns(mockGitRepo.Object);

            Should_Raise_The_Changed_Event(
                new ReportViewSolutionOptionValue
                {
                    ReportStyle = ReportStyle.Source,
                    ReportContent = ReportContentType.Full,
                    SelectedBranchName = "selectedbranch",
                    SelectedRepository = "selectedrepopath"
                },
                 new ReportViewSolutionOptionValue
                 {
                     ReportStyle = ReportStyle.Source,
                     ReportContent = ReportContentType.Changeset,
                     SelectedBranchName = "selectedbranch",
                     SelectedRepository = "selectedrepopath"
                 },
                 true);
        }

        [Test]
        public void Should_Raise_The_Change_Event_ChangesetChanged_True_When_SelectedRepository_Changed()
        {
            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);
            var repositoryPaths = new List<string> { "originalrepo", "changedrepo" };
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths);

            var mockOriginalGitRepo = new Mock<IGitRepo>();
            mockOriginalGitRepo.Setup(gitRepo => gitRepo.HasBranch("master")).Returns(true);
            gitServiceMock.Setup(gitService => gitService.GetRepository("original")).Returns(mockOriginalGitRepo.Object);

            var mockChangedGitRepo = new Mock<IGitRepo>();
            mockChangedGitRepo.Setup(gitRepo => gitRepo.HasBranch("master")).Returns(true);
            gitServiceMock.Setup(gitService => gitService.GetRepository("changedrepo")).Returns(mockChangedGitRepo.Object);

            Should_Raise_The_Changed_Event(
                new ReportViewSolutionOptionValue
                {
                    ReportStyle = ReportStyle.Source,
                    ReportContent = ReportContentType.Changeset,
                    SelectedBranchName = "master",
                    SelectedRepository = "originalrepo"
                },
                 new ReportViewSolutionOptionValue
                 {
                     ReportStyle = ReportStyle.Source,
                     ReportContent = ReportContentType.Changeset,
                     SelectedBranchName = "master",
                     SelectedRepository = "changedrepo"
                 },
                 true);
        }

        [Test]
        public void Should_Raise_The_Change_Event_ChangesetChanged_True_When_SelectedBranch_Changed()
        {
            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);
            var repositoryPaths = new List<string> { "repo" };
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths);

            var mockGitRepo = new Mock<IGitRepo>();
            mockGitRepo.Setup(gitRepo => gitRepo.HasBranch("branch1")).Returns(true);
            mockGitRepo.Setup(gitRepo => gitRepo.HasBranch("branch2")).Returns(true);
            gitServiceMock.Setup(gitService => gitService.GetRepository("repo")).Returns(mockGitRepo.Object);

           
            Should_Raise_The_Changed_Event(
                new ReportViewSolutionOptionValue
                {
                    ReportStyle = ReportStyle.Source,
                    ReportContent = ReportContentType.Changeset,
                    SelectedBranchName = "branch1",
                    SelectedRepository = "repo"
                },
                 new ReportViewSolutionOptionValue
                 {
                     ReportStyle = ReportStyle.Source,
                     ReportContent = ReportContentType.Changeset,
                     SelectedBranchName = "branch2",
                     SelectedRepository = "repo"
                 },
                 true);
        }

        // changeset tests
        [Test]
        public void Should_Have_Null_Changeset_If_CanUseChangeset_Is_False()
        {
            Assert.That(reportViews.GetChangeset(), Is.Null);
        }

        private IChangeset SetupForChangeset(ReportContentType reportContentType, bool selected = true)
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue
            {
                ReportContent = reportContentType,
                ReportStyle = ReportStyle.Assembly,
                SelectedRepository = selected ? "repopath": null,
                SelectedBranchName = selected ? "branch" : null
            });

            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);
            var repositoryPaths = new List<string> { "repopath" };
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths);
            var mockGitRepo = new Mock<IGitRepo>();
            mockGitRepo.Setup(gitRepo => gitRepo.HasBranch("branch")).Returns(true);
            gitServiceMock.Setup(gitService => gitService.GetRepository("repopath")).Returns(mockGitRepo.Object);
            var changesetLookup = new Dictionary<string, HashSet<int>>();
            mockGitRepo.Setup(gitRepo => gitRepo.GetChangeset("branch")).Returns(changesetLookup);
            var changeset = new Mock<IChangeset>().Object;
            gitServiceMock.Setup(gitService => gitService.GetChangeset(changesetLookup)).Returns(changeset);
            return changeset;
        }

        [Test]
        public void Should_Have_Null_Changeset_If_Not_Applicable_GetState_Not_Called()
        {
            SetupForChangeset(ReportContentType.Full);

            Assert.That(reportViews.GetChangeset(), Is.Null);
        }

        [Test]
        public void Should_Have_Changeset_If_Applicable_GetState_Not_Called()
        {
            var changeset = SetupForChangeset(ReportContentType.Changeset);

            Assert.That(reportViews.GetChangeset(), Is.SameAs(changeset));
        }

        [Test]
        public void Should_Not_Have_Changeset_If_No_Selected_Branch()
        {
            SetupForChangeset(ReportContentType.Changeset);

            reportViews.GetState();
            reportViews.Update(ReportStyle.Source, ReportContentType.Changeset, null, "repopath");

            Assert.That(reportViews.GetChangeset(), Is.Null);
        }

        [Test]
        public void Should_Not_Have_Changeset_If_No_Selected_Repo()
        {
            SetupForChangeset(ReportContentType.Changeset);

            reportViews.GetState();
            reportViews.Update(ReportStyle.Source, ReportContentType.Changeset, null, null);

            Assert.That(reportViews.GetChangeset(), Is.Null);
        }

        [Test]
        public void Should_Have_Changeset_When_Applicable_From_Update()
        {
            var changeset = SetupForChangeset(ReportContentType.Changeset, false);
            reportViews.GetState();
            reportViews.Update(ReportStyle.Source, ReportContentType.Changeset, "branch", "repopath");

            Assert.That(reportViews.GetChangeset(), Is.SameAs(changeset));
        }

        [Test]
        public void Should_Dispose_Selected_GitRepo_When_Option_Unloaded()
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue { SelectedRepository = "selectedrepopath", SelectedBranchName = "selectedbranch" });

            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);
            var repositoryPaths = new List<string> { "repopath1", "selectedrepopath" };
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths);
            var mockGitRepo = new Mock<IGitRepo>();
            gitServiceMock.Setup(gitService => gitService.GetRepository("selectedrepopath")).Returns(mockGitRepo.Object);

            reportViews.GetState();
            var mockReportViewSolutionOption = autoMoqer.GetMock<IReportViewSolutionOption>();
            mockReportViewSolutionOption.Raise(o => o.UnloadedEvent += null, EventArgs.Empty);

            mockGitRepo.Verify(gitRepo => gitRepo.Dispose());
        }

        [Test]
        public void Should_Re_Initialize_When_GetChangeset()
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue { SelectedRepository = "selectedrepopath", SelectedBranchName = "selectedbranch" });

            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);

            var repositoryPaths = new List<string> { "repopath1", "selectedrepopath" };
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(repositoryPaths);
            var mockGitRepo = new Mock<IGitRepo>();
            gitServiceMock.Setup(gitService => gitService.GetRepository("selectedrepopath")).Returns(mockGitRepo.Object);

            reportViews.GetState(); // first time initialization

            // change to another solution
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(new List<string> { "selectedrepopath2" });
            var mockGitRepo2 = new Mock<IGitRepo>();
            mockGitRepo2.Setup(gitRepo2 => gitRepo2.HasBranch("selectedbranch2")).Returns(true);
            gitServiceMock.Setup(gitService => gitService.GetRepository("selectedrepopath2")).Returns(mockGitRepo2.Object);
            var changesetLookup = new Dictionary<string, HashSet<int>>();
            mockGitRepo2.Setup(gitRepo => gitRepo.GetChangeset("selectedbranch2")).Returns(changesetLookup);
            var changeset = new Mock<IChangeset>().Object;
            gitServiceMock.Setup(gitService => gitService.GetChangeset(changesetLookup)).Returns(changeset);
            
            var mockReportViewSolutionOption = autoMoqer.GetMock<IReportViewSolutionOption>();
            mockReportViewSolutionOption.Raise(o => o.UnloadedEvent += null, EventArgs.Empty);

            mockReportViewSolutionOption.Object.Value = new ReportViewSolutionOptionValue
            {
                ReportContent = ReportContentType.Changeset,
                SelectedRepository = "selectedrepopath2",
                SelectedBranchName = "selectedbranch2"
            };

            Assert.That(reportViews.GetChangeset(), Is.SameAs(changeset));

            // repo deleted
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(new List<string>());

            Assert.That(reportViews.GetChangeset(), Is.Null);
        }

        [Test]
        public void Should_Have_Null_Changeset_When_Selected_Repo_Is_Invalid()
        {
            SetupInitialSolutionOption(new ReportViewSolutionOptionValue { SelectedRepository = "selectedrepopath", SelectedBranchName = "selectedbranch" });

            var gitServiceMock = autoMoqer.GetMock<IGitService>();
            gitServiceMock.SetupGet(gitService => gitService.CanUseChangeset).Returns(true);
            gitServiceMock.Setup(gitService => gitService.GetRepositoryPaths()).Returns(new List<string> { "selectedrepopath" });
            gitServiceMock.Setup(gitService => gitService.GetRepository("selectedrepopath")).Returns<IGitRepo>(null);

            Assert.That(reportViews.GetChangeset(), Is.Null);
        }
    
    }
}
