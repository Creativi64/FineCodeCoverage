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
                this.GitRepo = gitRepo;
                this.RepositoryPath = repositoryPath;
                this.SelectedBranchName = selectedBranchName;
            }

            public bool Deleted() => this.GitRepo.Deleted();

            public void SetSelectedBranchIfExists(string selectedBranchName)
            {
                selectedBranchName = this.GitRepo.HasBranch(selectedBranchName) ? selectedBranchName : null;
                this.SelectedBranchName = selectedBranchName;
            }

            public IGitRepo GitRepo { get; }
            public string RepositoryPath { get; }
            public string SelectedBranchName { get; set; }

            public void Dispose() => this.GitRepo.Dispose();
        }

        [ImportingConstructor]
        public ReportViews(IReportViewSolutionOption reportViewSolutionOption, IGitService gitService)
        {
            this.reportViewSolutionOption = reportViewSolutionOption;
            this.gitService = gitService;
            reportViewSolutionOption.UnloadedEvent += this.ReportViewSolutionOption_UnloadedEvent;
        }

        private void ReportViewSolutionOption_UnloadedEvent(object sender, EventArgs e) => this.DisposeSelectedGitRepo();

        public ReportViewState GetState()
        {
            this.Initialize();
            return new ReportViewState(this.reportViewSolutionOption.Value, this.repositoryPaths, this.gitService.CanUseChangeset);
        }

        private void Initialize()
        {
            this.InitializeGit();
            this.UpdateOptionValueFromSelectedGitRepo();
        }

        private void UpdateOptionValueFromSelectedGitRepo()
        {
            ReportViewSolutionOptionValue optionValue = this.reportViewSolutionOption.Value;
            optionValue.SelectedBranchName = this.selectedGitRepo?.SelectedBranchName;
            optionValue.SelectedRepository = this.selectedGitRepo?.RepositoryPath;
        }

        private void DisposeSelectedGitRepo()
        {
            this.selectedGitRepo?.Dispose();
            this.selectedGitRepo = null;
        }

        private void InitializeGit()
        {
            if (this.gitService.CanUseChangeset)
            {
                ReportViewSolutionOptionValue optionValue = this.reportViewSolutionOption.Value;
                string selectedRepositoryPath = optionValue.SelectedRepository;

                var possibleRepositoryPaths = new List<string>(this.gitService.GetRepositoryPaths());
                if (possibleRepositoryPaths.Contains(selectedRepositoryPath))
                {
                    this.EnsureSelectedGitRepo(selectedRepositoryPath);
                    if (this.selectedGitRepo != null)
                    {
                        this.selectedGitRepo.SetSelectedBranchIfExists(optionValue.SelectedBranchName);
                    }
                    else
                    {
                        _ = possibleRepositoryPaths.Remove(selectedRepositoryPath);
                    }
                }
                else
                {
                    this.DisposeSelectedGitRepo();
                }

                this.repositoryPaths = possibleRepositoryPaths;
            }
        }

        public void Update(
            ReportStyle reportStyle,
            ReportContentType reportContentType,
            string selectedBranchName,
            string selectedRepositoryPath)
        {
            ReportViewSolutionOptionValue oldValue = this.reportViewSolutionOption.Value;
            bool oldHasChangeset = this.HasChangeSet();
            if (selectedRepositoryPath != null)
            {
                this.EnsureSelectedGitRepo(selectedRepositoryPath);
                this.selectedGitRepo?.SetSelectedBranchIfExists(selectedBranchName);
            }
            else
            {
                this.DisposeSelectedGitRepo();
            }

            this.SetUpdatedOptionValue(reportStyle, reportContentType);
            bool changesetChanged = this.ChangesetChanged(oldHasChangeset, oldValue);

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
            this.reportViewSolutionOption.Value = new ReportViewSolutionOptionValue
            {
                ReportStyle = reportStyle,
                ReportContent = reportContentType,
            };
            this.UpdateOptionValueFromSelectedGitRepo();
        }

        private bool ChangesetChanged(bool oldHasChangeset, ReportViewSolutionOptionValue oldValue)
        {
            bool hasChangeset = this.HasChangeSet();
            bool hasChangeSetChanged = oldHasChangeset != hasChangeset;
            // if both false then the change set would be null and not changed
            // if one false and one true then one would be null and the other not
            // if both true the changeset will have changed if different repo/branch combo
            bool changesetChanged = hasChangeSetChanged;
            if (!changesetChanged && hasChangeset)
            {
                ReportViewSolutionOptionValue newValue = this.reportViewSolutionOption.Value;
                changesetChanged = oldValue.SelectedRepository != newValue.SelectedRepository
                    || oldValue.SelectedBranchName != newValue.SelectedBranchName;
            }

            return changesetChanged;
        }

        private void EnsureSelectedGitRepo(string selectedRepositoryPath)
        {
            if (this.selectedGitRepo?.Deleted() == true)
            {
                this.DisposeSelectedGitRepo();
                return;
            }

            if (this.selectedGitRepo == null || this.selectedGitRepo.RepositoryPath != selectedRepositoryPath)
            {
                this.selectedGitRepo?.Dispose();
                IGitRepo gitRepo = this.gitService.GetRepository(selectedRepositoryPath);
                if (gitRepo != null)
                {
                    this.selectedGitRepo = new SelectedGitRepo(gitRepo, selectedRepositoryPath, null);
                }
            }
        }

        public IEnumerable<string> GetBranches(string selectedRepositoryPath)
        {
            this.EnsureSelectedGitRepo(selectedRepositoryPath);
            return this.selectedGitRepo != null ? this.selectedGitRepo.GitRepo.GetBranches() : Enumerable.Empty<string>();
        }

        public IChangeset GetChangeset()
        {
            if (this.gitService.CanUseChangeset)
            {
                this.Initialize();
                if (this.HasChangeSet())
                {
                    IDictionary<string, HashSet<int>> cs = this.selectedGitRepo.GitRepo.GetChangeset(this.selectedGitRepo.SelectedBranchName);
                    return this.gitService.GetChangeset(cs);
                }
            }

            return null;
        }

        private bool HasChangeSet() => this.reportViewSolutionOption.Value.ReportContent == ReportContentType.Changeset
            && this.selectedGitRepo?.SelectedBranchName != null;

        public ReportStyle ReportStyle => this.reportViewSolutionOption.Value.ReportStyle;

        public event EventHandler<ReportViewChangedEventArgs> Changed;
    }
}
