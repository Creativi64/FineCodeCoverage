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

        private class SelectedGitRepo : IDisposable
        {
            public SelectedGitRepo(IGitRepo gitRepo, string repositoryPath, string selectedBranchName)
            {
                GitRepo = gitRepo;
                RepositoryPath = repositoryPath;
                SelectedBranchName = selectedBranchName;
            }

            public bool Deleted()
            {
                return GitRepo.Deleted();
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
        }

        public IReportViewState GetState()
        {
            Initialize();
            return new ReportViewState(reportViewSolutionOption.Value, repositoryPaths, gitService.CanUseChangeset);
        }

        private void Initialize()
        {
            InitializeGit();
            UpdateOptionValueFromSelectedGitRepo();
        }

        private void UpdateOptionValueFromSelectedGitRepo()
        {
            var optionValue = reportViewSolutionOption.Value;
            optionValue.SelectedBranchName = selectedGitRepo?.SelectedBranchName;
            optionValue.SelectedRepository = selectedGitRepo?.RepositoryPath;
        }

        private void DisposeSelectedGitRepo()
        {
            selectedGitRepo?.Dispose();
            selectedGitRepo = null;
        }

        private void InitializeGit()
        {
            if (gitService.CanUseChangeset)
            {
                var optionValue = reportViewSolutionOption.Value;
                var selectedRepositoryPath = optionValue.SelectedRepository;

                var possibleRepositoryPaths = new List<string>(gitService.GetRepositoryPaths());
                if (possibleRepositoryPaths.Contains(selectedRepositoryPath))
                {
                    EnsureSelectedGitRepo(selectedRepositoryPath);
                    if (selectedGitRepo != null)
                    {
                        selectedGitRepo.SetSelectedBranchIfExists(optionValue.SelectedBranchName);
                    }
                    else
                    {
                        possibleRepositoryPaths.Remove(selectedRepositoryPath);
                    }
                }
                else
                {
                    DisposeSelectedGitRepo();
                }
                repositoryPaths = possibleRepositoryPaths;
            }
        }

        public void Update(
            ReportStyle reportStyle,
            ReportContentType reportContentType,
            string selectedBranchName,
            string selectedRepositoryPath)
        {
            var oldValue = reportViewSolutionOption.Value;
            var oldHasChangeset = HasChangeSet();
            if (selectedRepositoryPath != null)
            {
                EnsureSelectedGitRepo(selectedRepositoryPath);
                selectedGitRepo?.SetSelectedBranchIfExists(selectedBranchName);
            }
            else
            {
                DisposeSelectedGitRepo();
            }

            SetUpdatedOptionValue(reportStyle, reportContentType);
            var changesetChanged = ChangesetChanged(oldHasChangeset, oldValue);

            if ((oldValue.ReportStyle != reportStyle) || changesetChanged)
            {
                Changed?.Invoke(this, new ReportViewChangedEventArgs(changesetChanged));
            }
        }

        private void SetUpdatedOptionValue(
            ReportStyle reportStyle,
            ReportContentType reportContentType
        )
        {
            reportViewSolutionOption.Value = new ReportViewSolutionOptionValue
            {
                ReportStyle = reportStyle,
                ReportContent = reportContentType,
            };
            UpdateOptionValueFromSelectedGitRepo();
        }

        private bool ChangesetChanged(bool oldHasChangeset, ReportViewSolutionOptionValue oldValue)
        {
            var hasChangeset = HasChangeSet();
            var hasChangeSetChanged = oldHasChangeset != hasChangeset;
            // if both false then the change set would be null and not changed
            // if one false and one true then one would be null and the other not
            // if both true the changeset will have changed if different repo/branch combo
            var changesetChanged = hasChangeSetChanged;
            if (!changesetChanged && hasChangeset)
            {
                var newValue = reportViewSolutionOption.Value;
                changesetChanged = oldValue.SelectedRepository != newValue.SelectedRepository
                    || oldValue.SelectedBranchName != newValue.SelectedBranchName;
            }
            return changesetChanged;
        }

        private void EnsureSelectedGitRepo(string selectedRepositoryPath)
        {
            if (selectedGitRepo?.Deleted() == true)
            {
                DisposeSelectedGitRepo();
                return;
            }
            if (selectedGitRepo == null || selectedGitRepo.RepositoryPath != selectedRepositoryPath)
            {
                selectedGitRepo?.Dispose();
                var gitRepo = gitService.GetRepository(selectedRepositoryPath);
                if (gitRepo != null)
                {
                    selectedGitRepo = new SelectedGitRepo(gitRepo, selectedRepositoryPath, null);
                }
            }
        }

        public IEnumerable<string> GetBranches(string selectedRepositoryPath)
        {
            EnsureSelectedGitRepo(selectedRepositoryPath);
            if (selectedGitRepo != null)
            {
                return selectedGitRepo.GitRepo.GetBranches();
            }
            return Enumerable.Empty<string>();
        }

        public IChangeset GetChangeset()
        {
            if (gitService.CanUseChangeset)
            {
                Initialize();
                if (HasChangeSet())
                {
                    var cs = selectedGitRepo.GitRepo.GetChangeset(selectedGitRepo.SelectedBranchName);
                    return gitService.GetChangeset(cs);
                }
            }
            return null;
        }

        private bool HasChangeSet()
        {
            return reportViewSolutionOption.Value.ReportContent == ReportContentType.Changeset && selectedGitRepo?.SelectedBranchName != null;
        }

        public ReportStyle ReportStyle => reportViewSolutionOption.Value.ReportStyle;

        public event EventHandler<ReportViewChangedEventArgs> Changed;
    }

}
