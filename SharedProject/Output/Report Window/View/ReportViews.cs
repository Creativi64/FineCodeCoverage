using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IReportViews))]
    [Export(typeof(IReportViewSelectorModel))]
    internal class ReportViews : IReportViews, IReportViewSelectorModel
    {
        private readonly IReportViewSolutionOption reportViewSolutionOption;
        private readonly IGitService gitService;
        private SelectedGitRepo selectedGitRepo;
        private IReadOnlyList<string> repositoryPaths = new List<string>();
        private bool initialized;

        private class SelectedGitRepo : IDisposable
        {
            public SelectedGitRepo(IGitRepo gitRepo,string repositoryPath, string selectedBranchName)
            {
                GitRepo = gitRepo;
                RepositoryPath = repositoryPath;
                SelectedBranchName = selectedBranchName;
            }

            public void SetSelectedBranchIfExists(string selectedBranchName)
            {
                selectedBranchName = GitRepo.HasBranch(selectedBranchName) ? selectedBranchName : null;
                SelectedBranchName = selectedBranchName;
            }

            public IGitRepo GitRepo { get; }
            public string RepositoryPath { get; }
            public string SelectedBranchName { get; set; }

            public void Dispose()
            {
                GitRepo.Dispose();
            }
        }

        [ImportingConstructor]
        public ReportViews(IReportViewSolutionOption reportViewSolutionOption, IGitService gitService)
        {
            this.reportViewSolutionOption = reportViewSolutionOption;
            this.gitService = gitService;
            reportViewSolutionOption.UnloadedEvent += ReportViewSolutionOption_UnloadedEvent;
        }

        private void ReportViewSolutionOption_UnloadedEvent(object sender, EventArgs e)
        {
            DisposeSelectedGitRepo();
            initialized = false;
        }

        public IReportViewState GetState()
        {
            var optionValue = reportViewSolutionOption.Value;
            InitializeRepositories(optionValue.SelectedRepository, optionValue.SelectedBranchName);
            optionValue.SelectedBranchName = selectedGitRepo?.SelectedBranchName;
            optionValue.SelectedRepository = selectedGitRepo?.RepositoryPath;
            initialized = true;
            return new ReportViewState(optionValue, repositoryPaths, gitService.CanUseChangeset);
        }

        private void DisposeSelectedGitRepo()
        {
            selectedGitRepo?.Dispose();
            selectedGitRepo = null;
        }

        private void InitializeRepositories(string selectedRepositoryPath, string selectedBranchName)
        {
            if (gitService.CanUseChangeset)
            {
                repositoryPaths = gitService.GetRepositoryPaths();
                if (repositoryPaths.Contains(selectedRepositoryPath))
                {
                    EnsureSelectedGitRepo(selectedRepositoryPath);
                    selectedGitRepo.SetSelectedBranchIfExists(selectedBranchName);
                }
                else
                {
                    DisposeSelectedGitRepo();
                }
            }
        }

        public void Update(
            ReportStyle reportStyle,
            ReportContentType reportContentType,
            string selectedBranchName,
            string selectedRepositoryPath)
        {
            if (selectedRepositoryPath != null)
            {
                EnsureSelectedGitRepo(selectedRepositoryPath);
                selectedGitRepo.SelectedBranchName = selectedBranchName;
            }
            else
            {
                DisposeSelectedGitRepo();
            }
                var oldValue = reportViewSolutionOption.Value;
            var newValue = new ReportViewSolutionOptionValue
            {
                ReportStyle = reportStyle,
                ReportContent = reportContentType,
                SelectedBranchName = selectedBranchName,
                SelectedRepository = selectedRepositoryPath
            };
            reportViewSolutionOption.Value = newValue;
            var changeSetChanged = oldValue.ReportContent != newValue.ReportContent
                || oldValue.SelectedRepository != newValue.SelectedRepository
                || oldValue.SelectedBranchName != newValue.SelectedBranchName;
            Changed?.Invoke(this, new ReportViewChangedEventArgs(changeSetChanged));
        }

        private void EnsureSelectedGitRepo(string selectedRepositoryPath)
        {
            if (selectedGitRepo == null || selectedGitRepo.RepositoryPath != selectedRepositoryPath)
            {
                selectedGitRepo?.Dispose();
                var gitRepo = gitService.GetRepository(selectedRepositoryPath);
                selectedGitRepo = new SelectedGitRepo(gitRepo, selectedRepositoryPath, null);
            }
        }

        public IEnumerable<string> GetBranches(string selectedRepositoryPath)
        {
            EnsureSelectedGitRepo(selectedRepositoryPath);
            return selectedGitRepo.GitRepo.GetBranches();
        }

        public IChangeset GetChangeset()
        {
            if (gitService.CanUseChangeset)
            {
                if (!initialized)
                {
                    GetState();
                }
                if (ReportContentType == ReportContentType.Changeset && selectedGitRepo?.SelectedBranchName != null)
                {
                    var cs = selectedGitRepo.GitRepo.GetChangeset(selectedGitRepo.SelectedBranchName);
                    return gitService.GetChangeset(cs);
                }
            }
            return null;
        }

        public ReportStyle ReportStyle => reportViewSolutionOption.Value.ReportStyle;
        private ReportContentType ReportContentType => reportViewSolutionOption.Value.ReportContent;

        public event EventHandler<ReportViewChangedEventArgs> Changed;
    }

}
