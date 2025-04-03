using FineCodeCoverage.Core.Utilities.Solution;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IReportViews))]
    [Export(typeof(IReportViewSelectorModel))]
    internal class ReportViews : IReportViews, IReportViewSelectorModel
    {
        private readonly ReportViewSolutionOption reportViewSolutionOption;
        private readonly IGitService gitService;

        private class GitRepoInfo : IDisposable
        {
            public GitRepoInfo(IGitRepo gitRepo,string repositoryPath, string selectedBranchName)
            {
                GitRepo = gitRepo;
                RepositoryPath = repositoryPath;
                SelectedBranchName = selectedBranchName;
            }

            public IGitRepo GitRepo { get; }
            public string RepositoryPath { get; }
            public string SelectedBranchName { get; }

            public void Dispose()
            {
                GitRepo.Dispose();
            }
        }

        private GitRepoInfo gitRepoInfo;
        private List<string> repositoryPaths = new List<string>();

        [ImportingConstructor]
        public ReportViews(ReportViewSolutionOption reportViewSolutionOption, IGitService gitService)
        {
            reportViewSolutionOption.LoadedEvent += ReportViewSolutionOption_LoadedEvent;
            this.reportViewSolutionOption = reportViewSolutionOption;
            this.gitService = gitService;
#if VS2022
            CanUseChangeset = true;
#endif
        }

        //todo
        private void ReportViewSolutionOption_LoadedEvent(object sender, SolutionOptionLoadEventArgs<ReportViewSolutionOptionValue> e)
        {
            var previous = e.PreviousValue;
            var reportStyleDidChange = previous.ReportStyle != ReportStyle;
            var reportContentDidChange = previous.ReportContent != ReportContentType;
            if (reportStyleDidChange && reportContentDidChange)
            {
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        // may have already initialized.  Need to do active repositories each time
        public IReportViewState GetState()
        {
            var state = reportViewSolutionOption.Value;
            InitializeRepositories(state.SelectedRepository, state.SelectedBranchName);
            var optionValue = new ReportViewSolutionOptionValue
            {
                ReportContent = ReportContentType,
                ReportStyle = ReportStyle,
                SelectedBranchName = gitRepoInfo?.SelectedBranchName,
                SelectedRepository = gitRepoInfo?.RepositoryPath
            };
            reportViewSolutionOption.Value = optionValue;
            return new ReportViewState(optionValue, repositoryPaths, CanUseChangeset);
        }

        private void InitializeRepositories(string selectedRepositoryPath, string selectedBranchName)
        {
            if (CanUseChangeset)
            {
                // todo previous gitrepo....
                repositoryPaths = gitService.GetRepositoryPaths();
                if (repositoryPaths.Contains(selectedRepositoryPath))
                {
                    IGitRepo gitRepo = null;
                    if(gitRepoInfo != null)
                    {
                        if(gitRepoInfo.RepositoryPath == selectedRepositoryPath)
                        {
                            gitRepo = gitRepoInfo.GitRepo;
                        }
                        else
                        {
                            gitRepoInfo.Dispose();
                        }
                        gitRepoInfo = null;
                    }
                    if (gitRepo == null)
                    {
                        gitRepo = gitService.GetRepository(selectedRepositoryPath);
                    }
                    

                    selectedBranchName = gitRepo.HasBranch(selectedBranchName) ? selectedBranchName : null;
                    gitRepoInfo = new GitRepoInfo(gitRepo, selectedRepositoryPath, selectedBranchName);
                }
                else
                {
                    if(gitRepoInfo != null)
                    {
                        gitRepoInfo.Dispose();
                    }
                    gitRepoInfo = null;
                }
            }
        }

        // todo
        public void Update(
            ReportStyle reportStyle,
            ReportContentType reportContentType,
            string selectedBranchName,
            string selectedRepositoryPath)
        {
            reportViewSolutionOption.Value = new ReportViewSolutionOptionValue
            {
                ReportStyle = reportStyle,
                ReportContent = reportContentType,
                SelectedBranchName = selectedBranchName,
                SelectedRepository = selectedRepositoryPath
            };
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public IEnumerable<string> GetBranches(string selectedRepositoryPath)
        {
            // todo dispose of old
            if(gitRepoInfo == null || gitRepoInfo.RepositoryPath != selectedRepositoryPath)
            {
                gitRepoInfo?.Dispose();
                var gitRepo = gitService.GetRepository(selectedRepositoryPath);
                gitRepoInfo = new GitRepoInfo(gitRepo, selectedRepositoryPath, null);
            }
            return gitRepoInfo.GitRepo.GetBranches();
        }

        public ReportStyle ReportStyle => reportViewSolutionOption.Value.ReportStyle;
        public ReportContentType ReportContentType => reportViewSolutionOption.Value.ReportContent;
        public bool CanUseChangeset { get; }

        public event EventHandler Changed;
    }

}
