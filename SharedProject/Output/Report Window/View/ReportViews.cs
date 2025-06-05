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
                GitRepo = gitRepo;
                RepositoryPath = repositoryPath;
                SelectedBranchName = selectedBranchName;
            }

            public bool Deleted() => GitRepo.Deleted();

            public void SetSelectedBranchIfExists(string selectedBranchName)
            {
                selectedBranchName = GitRepo.HasBranch(selectedBranchName) ? selectedBranchName : null;
                SelectedBranchName = selectedBranchName;
            }

            public IGitRepo GitRepo { get; }

            public string RepositoryPath { get; }

            public string SelectedBranchName { get; set; }

            public void Dispose() => GitRepo.Dispose();
        }

        [ImportingConstructor]
        public ReportViews(IReportViewSolutionOption reportViewSolutionOption, IGitService gitService)
        {
            _reportViewSolutionOption = reportViewSolutionOption;
            _gitService = gitService;
            reportViewSolutionOption.UnloadedEvent += ReportViewSolutionOption_UnloadedEvent;
        }

        private void ReportViewSolutionOption_UnloadedEvent(object sender, EventArgs e) => DisposeSelectedGitRepo();

        public ReportViewState GetState()
        {
            Initialize();
            return new ReportViewState(_reportViewSolutionOption.Value, _repositoryPaths, _gitService.CanUseChangeset);
        }

        private void Initialize()
        {
            InitializeGit();
            UpdateOptionValueFromSelectedGitRepo();
        }

        private void UpdateOptionValueFromSelectedGitRepo()
        {
            ReportViewSolutionOptionValue optionValue = _reportViewSolutionOption.Value;
            optionValue.SelectedBranchName = _selectedGitRepo?.SelectedBranchName;
            optionValue.SelectedRepository = _selectedGitRepo?.RepositoryPath;
        }

        private void DisposeSelectedGitRepo()
        {
            _selectedGitRepo?.Dispose();
            _selectedGitRepo = null;
        }

        private void InitializeGit()
        {
            if (!_gitService.CanUseChangeset)
            {
                return;
            }

            ReportViewSolutionOptionValue optionValue = _reportViewSolutionOption.Value;
            string selectedRepositoryPath = optionValue.SelectedRepository;

            var possibleRepositoryPaths = new List<string>(_gitService.GetRepositoryPaths());
            if (possibleRepositoryPaths.Contains(selectedRepositoryPath))
            {
                EnsureSelectedGitRepo(selectedRepositoryPath);
                if (_selectedGitRepo != null)
                {
                    _selectedGitRepo.SetSelectedBranchIfExists(optionValue.SelectedBranchName);
                }
                else
                {
                    _ = possibleRepositoryPaths.Remove(selectedRepositoryPath);
                }
            }
            else
            {
                DisposeSelectedGitRepo();
            }

            _repositoryPaths = possibleRepositoryPaths;
        }

        public void Update(
            ReportStyle reportStyle,
            ReportContentType reportContentType,
            string selectedBranchName,
            string selectedRepositoryPath)
        {
            ReportViewSolutionOptionValue oldValue = _reportViewSolutionOption.Value;
            bool oldHasChangeset = HasChangeSet();
            if (selectedRepositoryPath != null)
            {
                EnsureSelectedGitRepo(selectedRepositoryPath);
                _selectedGitRepo?.SetSelectedBranchIfExists(selectedBranchName);
            }
            else
            {
                DisposeSelectedGitRepo();
            }

            SetUpdatedOptionValue(reportStyle, reportContentType);
            bool changesetChanged = ChangesetChanged(oldHasChangeset, oldValue);

            if ((oldValue.ReportStyle == reportStyle) && !changesetChanged)
            {
                return;
            }

            Changed?.Invoke(this, new ReportViewChangedEventArgs(changesetChanged));
        }

        private void SetUpdatedOptionValue(
            ReportStyle reportStyle,
            ReportContentType reportContentType)
        {
            _reportViewSolutionOption.Value = new ReportViewSolutionOptionValue
            {
                ReportStyle = reportStyle,
                ReportContent = reportContentType,
            };
            UpdateOptionValueFromSelectedGitRepo();
        }

        private bool ChangesetChanged(bool oldHasChangeset, ReportViewSolutionOptionValue oldValue)
        {
            bool hasChangeset = HasChangeSet();
            bool hasChangeSetChanged = oldHasChangeset != hasChangeset;

            // if both false then the change set would be null and not changed
            // if one false and one true then one would be null and the other not
            // if both true the changeset will have changed if different repo/branch combo
            bool changesetChanged = hasChangeSetChanged;
            if (!changesetChanged && hasChangeset)
            {
                ReportViewSolutionOptionValue newValue = _reportViewSolutionOption.Value;
                changesetChanged = oldValue.SelectedRepository != newValue.SelectedRepository
                    || oldValue.SelectedBranchName != newValue.SelectedBranchName;
            }

            return changesetChanged;
        }

        private void EnsureSelectedGitRepo(string selectedRepositoryPath)
        {
            if (_selectedGitRepo?.Deleted() == true)
            {
                DisposeSelectedGitRepo();
                return;
            }

            if (_selectedGitRepo != null && _selectedGitRepo.RepositoryPath == selectedRepositoryPath)
            {
                return;
            }

            _selectedGitRepo?.Dispose();
            IGitRepo gitRepo = _gitService.GetRepository(selectedRepositoryPath);
            if (gitRepo == null)
            {
                return;
            }

            _selectedGitRepo = new SelectedGitRepo(gitRepo, selectedRepositoryPath, null);
        }

        public IEnumerable<string> GetBranches(string selectedRepositoryPath)
        {
            EnsureSelectedGitRepo(selectedRepositoryPath);
            return _selectedGitRepo != null ? _selectedGitRepo.GitRepo.GetBranches() : Enumerable.Empty<string>();
        }

        public IChangeset GetChangeset()
        {
            if (!_gitService.CanUseChangeset)
            {
                return null;
            }

            Initialize();
            if (!HasChangeSet())
            {
                return null;
            }

            IDictionary<string, HashSet<int>> cs = _selectedGitRepo.GitRepo.GetChangeset(_selectedGitRepo.SelectedBranchName);
            return _gitService.GetChangeset(cs);
        }

        private bool HasChangeSet() => _reportViewSolutionOption.Value.ReportContent == ReportContentType.Changeset
            && _selectedGitRepo?.SelectedBranchName != null;

        public void Dispose() => DisposeSelectedGitRepo();

        public ReportStyle ReportStyle => _reportViewSolutionOption.Value.ReportStyle;

        public event EventHandler<ReportViewChangedEventArgs> Changed;
    }
}
