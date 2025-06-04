using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IReportViews))]
    [Export(typeof(IReportViewSelectorModel))]
    internal class ReportViews : IReportViews, IReportViewSelectorModel, IDisposable
    {
        private readonly IReportViewSolutionOption _reportViewSolutionOption;
        private readonly IGitService _gitService;
        private SelectedGitRepo _selectedGitRepo;
        private IReadOnlyList<string> _repositoryPaths = new List<string>();

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
            this._reportViewSolutionOption = reportViewSolutionOption;
            this._gitService = gitService;
            reportViewSolutionOption.UnloadedEvent += this.ReportViewSolutionOption_UnloadedEvent;
        }

        private void ReportViewSolutionOption_UnloadedEvent(object sender, EventArgs e) => this.DisposeSelectedGitRepo();

        public ReportViewState GetState()
        {
            this.Initialize();
            return new ReportViewState(this._reportViewSolutionOption.Value, this._repositoryPaths, this._gitService.CanUseChangeset);
        }

        private void Initialize()
        {
            this.InitializeGit();
            this.UpdateOptionValueFromSelectedGitRepo();
        }

        private void UpdateOptionValueFromSelectedGitRepo()
        {
            ReportViewSolutionOptionValue optionValue = this._reportViewSolutionOption.Value;
            optionValue.SelectedBranchName = this._selectedGitRepo?.SelectedBranchName;
            optionValue.SelectedRepository = this._selectedGitRepo?.RepositoryPath;
        }

        private void DisposeSelectedGitRepo()
        {
            this._selectedGitRepo?.Dispose();
            this._selectedGitRepo = null;
        }

        private void InitializeGit()
        {
            if (this._gitService.CanUseChangeset)
            {
                ReportViewSolutionOptionValue optionValue = this._reportViewSolutionOption.Value;
                string selectedRepositoryPath = optionValue.SelectedRepository;

                var possibleRepositoryPaths = new List<string>(this._gitService.GetRepositoryPaths());
                if (possibleRepositoryPaths.Contains(selectedRepositoryPath))
                {
                    this.EnsureSelectedGitRepo(selectedRepositoryPath);
                    if (this._selectedGitRepo != null)
                    {
                        this._selectedGitRepo.SetSelectedBranchIfExists(optionValue.SelectedBranchName);
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

                this._repositoryPaths = possibleRepositoryPaths;
            }
        }

        public void Update(
            ReportStyle reportStyle,
            ReportContentType reportContentType,
            string selectedBranchName,
            string selectedRepositoryPath)
        {
            ReportViewSolutionOptionValue oldValue = this._reportViewSolutionOption.Value;
            bool oldHasChangeset = this.HasChangeSet();
            if (selectedRepositoryPath != null)
            {
                this.EnsureSelectedGitRepo(selectedRepositoryPath);
                this._selectedGitRepo?.SetSelectedBranchIfExists(selectedBranchName);
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
            this._reportViewSolutionOption.Value = new ReportViewSolutionOptionValue
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
                ReportViewSolutionOptionValue newValue = this._reportViewSolutionOption.Value;
                changesetChanged = oldValue.SelectedRepository != newValue.SelectedRepository
                    || oldValue.SelectedBranchName != newValue.SelectedBranchName;
            }

            return changesetChanged;
        }

        private void EnsureSelectedGitRepo(string selectedRepositoryPath)
        {
            if (this._selectedGitRepo?.Deleted() == true)
            {
                this.DisposeSelectedGitRepo();
                return;
            }

            if (this._selectedGitRepo == null || this._selectedGitRepo.RepositoryPath != selectedRepositoryPath)
            {
                this._selectedGitRepo?.Dispose();
                IGitRepo gitRepo = this._gitService.GetRepository(selectedRepositoryPath);
                if (gitRepo != null)
                {
                    this._selectedGitRepo = new SelectedGitRepo(gitRepo, selectedRepositoryPath, null);
                }
            }
        }

        public IEnumerable<string> GetBranches(string selectedRepositoryPath)
        {
            this.EnsureSelectedGitRepo(selectedRepositoryPath);
            return this._selectedGitRepo != null ? this._selectedGitRepo.GitRepo.GetBranches() : Enumerable.Empty<string>();
        }

        public IChangeset GetChangeset()
        {
            if (this._gitService.CanUseChangeset)
            {
                this.Initialize();
                if (this.HasChangeSet())
                {
                    IDictionary<string, HashSet<int>> cs = this._selectedGitRepo.GitRepo.GetChangeset(this._selectedGitRepo.SelectedBranchName);
                    return this._gitService.GetChangeset(cs);
                }
            }

            return null;
        }

        private bool HasChangeSet() => this._reportViewSolutionOption.Value.ReportContent == ReportContentType.Changeset
            && this._selectedGitRepo?.SelectedBranchName != null;
        public void Dispose() => this.DisposeSelectedGitRepo();

        public ReportStyle ReportStyle => this._reportViewSolutionOption.Value.ReportStyle;

        public event EventHandler<ReportViewChangedEventArgs> Changed;
    }
}