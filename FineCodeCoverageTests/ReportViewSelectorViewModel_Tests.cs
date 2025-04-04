using AutoMoq;
using NUnit.Framework;
using FineCodeCoverage.Output;
using System.Collections.Generic;
using System.Linq;
using System;

namespace FineCodeCoverageTests
{
    internal class ReportViewSelectorViewModel_Tests
    {
        private class ReportViewState : IReportViewState
        {
            public ReportViewState(
                ReportStyle reportStyle, 
                ReportContentType reportContentType,
                string selectedRepositoryPath,
                string selectedBranchName,
                IReadOnlyList<string> repositoryPaths,
                bool canUseRepositories

            ) {
                ReportStyle = reportStyle;
                ReportContentType = reportContentType;
                SelectedRepositoryPath = selectedRepositoryPath;
                SelectedBranchName = selectedBranchName;
                RepositoryPaths = repositoryPaths;
                CanUseRepositories = canUseRepositories;
            }

            public bool CanUseRepositories { get; }
            public ReportContentType ReportContentType { get; }
            public ReportStyle ReportStyle { get; }
            public IReadOnlyList<string> RepositoryPaths { get; }
            public string SelectedBranchName { get; }
            public string SelectedRepositoryPath { get; }
        } 
        private ReportViewSelectorViewModel Setup(ReportViewState reportViewState)
        {
            var autoMoqer = new AutoMoqer();
            autoMoqer.GetMock<IReportViewSelectorModel>().Setup(model => model.GetState()).Returns(reportViewState);

            return autoMoqer.Create<ReportViewSelectorViewModel>();
        }

        [Test]
        public void Should_GetState_From_The_Model()
        {
            var autoMoqer = new AutoMoqer();
            var reportViewState = new ReportViewState(ReportStyle.Assembly, ReportContentType.Full, null, null, new List<string>(), false);
            autoMoqer.GetMock<IReportViewSelectorModel>().Setup(model => model.GetState()).Returns(reportViewState);
            
            autoMoqer.Create<ReportViewSelectorViewModel>();
            
            autoMoqer.Verify<IReportViewSelectorModel>(model => model.GetState());
        }

        private ReportViewSelectorViewModel SetupRepositories(int numRepositories)
        {
            var repositories = new List<string>();
            for(var i = 0; i < numRepositories; i++)
            {
                repositories.Add($"repopath{i}");
            }
            
            return Setup(new ReportViewState(ReportStyle.Assembly, ReportContentType.Full, null, null, repositories, true));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_ShowBranchesCombo_If_Has_Repositories(bool hasRepositories)
        {
            var reportViewSelectorViewModel = SetupRepositories(hasRepositories ? 1 : 0);

            Assert.That(reportViewSelectorViewModel.ShowBranchesCombo, Is.EqualTo(hasRepositories));
        }

        [Test]
        public void Should_Set_RepositoryPaths_From_State()
        {
            var repositoryPaths = new List<string> { "repopath", "repopath2" };
            var reportViewSelectorViewModel = Setup(
                new ReportViewState(
                    ReportStyle.Assembly, 
                    ReportContentType.Full, 
                    "repopath", 
                    "selectedbranch", 
                    repositoryPaths, true));

            Assert.That(reportViewSelectorViewModel.RepositoryPaths, Is.EqualTo(repositoryPaths));
        }

        [TestCase(0,false)]
        [TestCase(1, false)]
        [TestCase(2, true)]
        public void Should_ShowRepositoriesCombo_If_Has_More_Than_One_Repository(int numRepositories,bool expectedShow)
        {
            var reportViewSelectorViewModel = SetupRepositories(numRepositories);

            Assert.That(reportViewSelectorViewModel.ShowRepositoriesCombo, Is.EqualTo(expectedShow));
        }

        [Test]
        public void Should_Set_SelectedRepositoryPath_From_State_If_Not_Null()
        {
            var reportViewSelectorViewModel = Setup(new ReportViewState(ReportStyle.Assembly, ReportContentType.Full, "selectedrepopath", "selectedbranch", new List<string> { "firstrepopath", "selectedrepopath" }, true));

            Assert.That(reportViewSelectorViewModel.SelectedRepositoryPath, Is.EqualTo("selectedrepopath"));
        }

        [Test]
        public void Should_Set_SelectedRepositoryPath_To_The_First_If_Not_Set_In_State()
        {
            var reportViewSelectorViewModel = Setup(new ReportViewState(ReportStyle.Assembly, ReportContentType.Full, null, null, new List<string> { "firstrepopath", "secondrepopath" }, true));

            Assert.That(reportViewSelectorViewModel.SelectedRepositoryPath, Is.EqualTo("firstrepopath"));
        }

        [Test]
        public void Should_Have_Two_ReportContentType_When_Has_Repositories()
        {
            var reportViewSelectorViewModel = SetupRepositories(1);

            Assert.That(reportViewSelectorViewModel.ReportContentTypes, Has.Count.EqualTo(2));
        }

        [TestCase(ReportContentType.Full)]
        [TestCase(ReportContentType.Changeset)]
        public void Should_Initially_Have_SelectedReportContentType_From_State(ReportContentType reportContentType)
        {
            var repositoryPaths = new List<string> { "repopath", "repopath2" };
            var reportViewSelectorViewModel = Setup(
                new ReportViewState(
                    ReportStyle.Assembly,
                    reportContentType,
                    "repopath",
                    "selectedbranch",
                    repositoryPaths, true));

            Assert.That(reportViewSelectorViewModel.SelectedReportContentType.ReportContentType, Is.EqualTo(reportContentType));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_ShowReportContentTypeCombo_If_Has_Repositories(bool hasRepositories)
        {
            var reportViewSelectorViewModel = SetupRepositories(hasRepositories ? 1 : 0);

            Assert.That(reportViewSelectorViewModel.ShowReportContentTypeCombo, Is.EqualTo(hasRepositories));
        }

        [Test]
        public void Should_Have_Two_ReportStyle()
        {
            var reportViewSelectorViewModel = SetupRepositories(0);

            Assert.That(reportViewSelectorViewModel.ReportStyles, Has.Count.EqualTo(2));
        }

        [TestCase(ReportStyle.Assembly)]
        [TestCase(ReportStyle.Source)]
        public void Should_Initially_Have_SelectedReportStyle_From_State(ReportStyle reportStyle)
        {
            var repositoryPaths = new List<string> { "repopath", "repopath2" };
            var reportViewSelectorViewModel = Setup(
                new ReportViewState(
                    reportStyle,
                    ReportContentType.Full,
                    "repopath",
                    "selectedbranch",
                    repositoryPaths, true));

            Assert.That(reportViewSelectorViewModel.SelectedReportStyle.ReportStyle, Is.EqualTo(reportStyle));
        }

        [Test]
        public void Should_Have_An_Add_A_Repo_NoRepositoriesMessage_When_No_Repositories_And_CanUseRepositories()
        {
            var reportViewSelectorViewModel = Setup(
                new ReportViewState(
                    ReportStyle.Source,
                    ReportContentType.Full,
                    null,
                    null,
                    new List<string>(), true));

            Assert.That(reportViewSelectorViewModel.NoRepositoriesMessage,Is.EqualTo("Add a git repo to filter by changeset."));
            Assert.That(reportViewSelectorViewModel.ShowNoRepositoriesMessage, Is.True);
        }

        [Test]
        public void Should_Have_An_AvailableIn2022_NoRepositoriesMessage_When_No_Repositories_And_CanUseRepositories_Is_False()
        {
            var reportViewSelectorViewModel = Setup(
                new ReportViewState(
                    ReportStyle.Source,
                    ReportContentType.Full,
                    null,
                    null,
                    new List<string>(), false));

            Assert.That(reportViewSelectorViewModel.NoRepositoriesMessage, Is.EqualTo("Changeset filtering available in VS2022."));
            Assert.That(reportViewSelectorViewModel.ShowNoRepositoriesMessage, Is.True);
        }

        [Test]
        public void Should_Not_ShowNoRepositoriesMessage_When_Has_Repositories()
        {
            var reportViewSelectorViewModel = SetupRepositories(1);

            Assert.That(reportViewSelectorViewModel.ShowNoRepositoriesMessage, Is.False);
        }

        [TestCase(ReportContentType.Full, false)]
        [TestCase(ReportContentType.Changeset, true)]
        public void Should_Have_GitCombosEnabled_When_Changeset(ReportContentType reportContentType, bool expectedEnabled)
        {
            var repositoryPaths = new List<string> { "repopath", "repopath2" };
            var reportViewSelectorViewModel = Setup(
                new ReportViewState(
                    ReportStyle.Assembly,
                    reportContentType,
                    "repopath",
                    "selectedbranch",
                    repositoryPaths, true));

            Assert.That(reportViewSelectorViewModel.GitCombosEnabled, Is.EqualTo(expectedEnabled));
        }

        [Test]
        public void Should_GetBranches_When_SelectedRepositoryPath_Is_Set()
        {
            var autoMoqer = new AutoMoqer();
            var reportViewState = new ReportViewState(ReportStyle.Assembly, ReportContentType.Full, null, null, new List<string> { "repopath" }, true);
            var mockReportViewSelectorModel = autoMoqer.GetMock<IReportViewSelectorModel>();
            mockReportViewSelectorModel.Setup(model => model.GetState()).Returns(reportViewState);
            var selectedRepositoryBranches = new List<string> { "Branch1", "Branch2" };
            mockReportViewSelectorModel.Setup(model => model.GetBranches("repopath")).Returns(selectedRepositoryBranches);

            var reportViewSelectorViewModel =  autoMoqer.Create<ReportViewSelectorViewModel>();

            Assert.That(reportViewSelectorViewModel.Branches, Is.EquivalentTo(selectedRepositoryBranches));
        }

        [Test]
        public void Should_Have_Selected_Branch_As_The_First_When_No_Explicitly_Selected()
        {
            var autoMoqer = new AutoMoqer();
            var reportViewState = new ReportViewState(ReportStyle.Assembly, ReportContentType.Full, null, null, new List<string> { "repopath" }, true);
            var mockReportViewSelectorModel = autoMoqer.GetMock<IReportViewSelectorModel>();
            mockReportViewSelectorModel.Setup(model => model.GetState()).Returns(reportViewState);
            var selectedRepositoryBranches = new List<string> { "Branch1", "Branch2" };
            mockReportViewSelectorModel.Setup(model => model.GetBranches("repopath")).Returns(selectedRepositoryBranches);

            var reportViewSelectorViewModel = autoMoqer.Create<ReportViewSelectorViewModel>();

            Assert.That(reportViewSelectorViewModel.SelectedBranch, Is.EquivalentTo("Branch1"));
        }

        [Test]
        public void Should_Have_SelectedBranch_From_State_When_Set()
        {
            var autoMoqer = new AutoMoqer();
            var reportViewState = new ReportViewState(ReportStyle.Assembly, ReportContentType.Full, "repopath", "selectedbranch", new List<string> { "repopath" }, true);
            var mockReportViewSelectorModel = autoMoqer.GetMock<IReportViewSelectorModel>();
            mockReportViewSelectorModel.Setup(model => model.GetState()).Returns(reportViewState);
            var selectedRepositoryBranches = new List<string> { "Branch1", "selectedbranch" };
            mockReportViewSelectorModel.Setup(model => model.GetBranches("repopath")).Returns(selectedRepositoryBranches);

            var reportViewSelectorViewModel = autoMoqer.Create<ReportViewSelectorViewModel>();

            Assert.That(reportViewSelectorViewModel.SelectedBranch, Is.EquivalentTo("selectedbranch"));
        }

        [Test]
        public void Should_Clear_Existing_Branches_When_Select_New_Repository()
        {
            var autoMoqer = new AutoMoqer();
            var reportViewState = new ReportViewState(ReportStyle.Assembly, ReportContentType.Full, "repopath", "selectedbranch", new List<string> { "repopath","repopath2" }, true);
            var mockReportViewSelectorModel = autoMoqer.GetMock<IReportViewSelectorModel>();
            mockReportViewSelectorModel.Setup(model => model.GetState()).Returns(reportViewState);
            var selectedRepositoryBranches = new List<string> { "Branch1", "selectedbranch" };
            mockReportViewSelectorModel.Setup(model => model.GetBranches("repopath")).Returns(selectedRepositoryBranches);
            var repo2Branches = new List<string> { "Repo2Branch1" };
            mockReportViewSelectorModel.Setup(model => model.GetBranches("repopath2")).Returns(repo2Branches);

            var reportViewSelectorViewModel = autoMoqer.Create<ReportViewSelectorViewModel>();

            reportViewSelectorViewModel.SelectedRepositoryPath = "repopath2";

            Assert.That(reportViewSelectorViewModel.Branches, Is.EquivalentTo(repo2Branches));
            Assert.That(reportViewSelectorViewModel.SelectedBranch, Is.EqualTo("Repo2Branch1"));
        }

        // OkCommand tests
        [Test]
        public void Should_Not_Have_Executable_OkCommand_When_No_Changes()
        {
            var autoMoqer = new AutoMoqer();
            var reportViewState = new ReportViewState(ReportStyle.Assembly, ReportContentType.Full, "repopath", "selectedbranch", new List<string> { "repopath", "repopath2" }, true);
            var mockReportViewSelectorModel = autoMoqer.GetMock<IReportViewSelectorModel>();
            mockReportViewSelectorModel.Setup(model => model.GetState()).Returns(reportViewState);
            var selectedRepositoryBranches = new List<string> { "Branch1", "selectedbranch" };
            mockReportViewSelectorModel.Setup(model => model.GetBranches("repopath")).Returns(selectedRepositoryBranches);

            var reportViewSelectorViewModel = autoMoqer.Create<ReportViewSelectorViewModel>();

            Assert.That(reportViewSelectorViewModel.OkCommand.CanExecute(null), Is.False);
        }

        private void CanExecuteChanged_True_Test(Action<ReportViewSelectorViewModel> change)
        {
            var autoMoqer = new AutoMoqer();
            var reportViewState = new ReportViewState(ReportStyle.Assembly, ReportContentType.Full, "repopath", "selectedbranch", new List<string> { "repopath", "repopath2" }, true);
            var mockReportViewSelectorModel = autoMoqer.GetMock<IReportViewSelectorModel>();
            mockReportViewSelectorModel.Setup(model => model.GetState()).Returns(reportViewState);
            var selectedRepositoryBranches = new List<string> { "Branch1", "selectedbranch" };
            mockReportViewSelectorModel.Setup(model => model.GetBranches("repopath")).Returns(selectedRepositoryBranches);

            var reportViewSelectorViewModel = autoMoqer.Create<ReportViewSelectorViewModel>();
            var canExecuteChanged = false;
            reportViewSelectorViewModel.OkCommand.CanExecuteChanged += (_, __) =>
            {
                canExecuteChanged = true;
                Assert.That(reportViewSelectorViewModel.OkCommand.CanExecute(null), Is.True);
            };
            change(reportViewSelectorViewModel);

            Assert.That(canExecuteChanged, Is.True);
        }

        [Test]
        public void Should_Raise_OKCommand_CanExecuteChanged_True_When_SelectedReportStyle_Changed()
        {
            CanExecuteChanged_True_Test((reportViewSelectorViewModel) =>
            {
                reportViewSelectorViewModel.SelectedReportStyle = reportViewSelectorViewModel.ReportStyles.First(vm => vm.ReportStyle == ReportStyle.Source);
            });
        }

        [Test]
        public void Should_Raise_OKCommand_CanExecuteChanged_True_When_SelectedReportContentType_Changed()
        {
            CanExecuteChanged_True_Test((reportViewSelectorViewModel) =>
            {
                reportViewSelectorViewModel.SelectedReportContentType = reportViewSelectorViewModel.ReportContentTypes.First(vm => vm.ReportContentType == ReportContentType.Changeset);
            });
        }

        [Test]
        public void Should_Raise_OKCommand_CanExecuteChanged_True_When_SelectedBranch_Changed()
        {
            CanExecuteChanged_True_Test((reportViewSelectorViewModel) =>
            {
                reportViewSelectorViewModel.SelectedBranch = "Branch1";
            });
        }

        [Test]
        public void Should_Raise_OKCommand_CanExecuteChanged_True_When_SelectedRepositoryPath_Changed()
        {
            CanExecuteChanged_True_Test((reportViewSelectorViewModel) =>
            {
                reportViewSelectorViewModel.SelectedRepositoryPath = "repopath2";
            });
        }

        [Test]
        public void Should_Raise_OKCommand_CanExecuteChanged_False_When_Change_Back_To_Original()
        {
            var autoMoqer = new AutoMoqer();
            var reportViewState = new ReportViewState(ReportStyle.Assembly, ReportContentType.Full, "repopath", "selectedbranch", new List<string> { "repopath", "repopath2" }, true);
            var mockReportViewSelectorModel = autoMoqer.GetMock<IReportViewSelectorModel>();
            mockReportViewSelectorModel.Setup(model => model.GetState()).Returns(reportViewState);
            var selectedRepositoryBranches = new List<string> { "Branch1", "selectedbranch" };
            mockReportViewSelectorModel.Setup(model => model.GetBranches("repopath")).Returns(selectedRepositoryBranches);

            var reportViewSelectorViewModel = autoMoqer.Create<ReportViewSelectorViewModel>();

            reportViewSelectorViewModel.SelectedReportStyle = reportViewSelectorViewModel.ReportStyles.First(vm => vm.ReportStyle == ReportStyle.Source);

            var canExecuteChanged = false;
            reportViewSelectorViewModel.OkCommand.CanExecuteChanged += (_, __) =>
            {
                canExecuteChanged = true;
                Assert.That(reportViewSelectorViewModel.OkCommand.CanExecute(null), Is.False);
            };

            reportViewSelectorViewModel.SelectedReportStyle = reportViewSelectorViewModel.ReportStyles.First(vm => vm.ReportStyle == ReportStyle.Assembly);


            Assert.That(canExecuteChanged, Is.True);
        }

        [Test]
        public void Should_Not_Raise_OkCommand_CanExecuteChanged_When_No_Change()
        {
            var autoMoqer = new AutoMoqer();
            var reportViewState = new ReportViewState(ReportStyle.Assembly, ReportContentType.Full, "repopath", "selectedbranch", new List<string> { "repopath", "repopath2" }, true);
            var mockReportViewSelectorModel = autoMoqer.GetMock<IReportViewSelectorModel>();
            mockReportViewSelectorModel.Setup(model => model.GetState()).Returns(reportViewState);
            var selectedRepositoryBranches = new List<string> { "Branch1", "selectedbranch" };
            mockReportViewSelectorModel.Setup(model => model.GetBranches("repopath")).Returns(selectedRepositoryBranches);

            var reportViewSelectorViewModel = autoMoqer.Create<ReportViewSelectorViewModel>();

            reportViewSelectorViewModel.SelectedReportStyle = reportViewSelectorViewModel.ReportStyles.First(vm => vm.ReportStyle == ReportStyle.Source);

            var canExecuteChanged = false;
            reportViewSelectorViewModel.OkCommand.CanExecuteChanged += (_, __) =>
            {
                canExecuteChanged = true;
            };

            reportViewSelectorViewModel.SelectedReportContentType = reportViewSelectorViewModel.ReportContentTypes.First(vm => vm.ReportContentType == ReportContentType.Changeset);


            Assert.That(canExecuteChanged, Is.False);
        }

        [Test]
        public void Should_Update_The_Model_When_Ok_Executed()
        {
            var autoMoqer = new AutoMoqer();
            var reportViewState = new ReportViewState(ReportStyle.Assembly, ReportContentType.Full, null, null, new List<string> { "repopath" }, true);
            var mockReportViewSelectorModel = autoMoqer.GetMock<IReportViewSelectorModel>();
            mockReportViewSelectorModel.Setup(model => model.GetState()).Returns(reportViewState);
            var selectedRepositoryBranches = new List<string> { "Branch1" };
            mockReportViewSelectorModel.Setup(model => model.GetBranches("repopath")).Returns(selectedRepositoryBranches);

            var reportViewSelectorViewModel = autoMoqer.Create<ReportViewSelectorViewModel>();

            reportViewSelectorViewModel.SelectedReportStyle = reportViewSelectorViewModel.ReportStyles.First(rs => rs.ReportStyle == ReportStyle.Source);
            reportViewSelectorViewModel.SelectedReportContentType = reportViewSelectorViewModel.ReportContentTypes.First(rct => rct.ReportContentType == ReportContentType.Changeset);

            reportViewSelectorViewModel.OkCommand.Execute(null);

            mockReportViewSelectorModel.Verify(m => m.Update(ReportStyle.Source, ReportContentType.Changeset, "Branch1", "repopath"));
        }
    }
}
