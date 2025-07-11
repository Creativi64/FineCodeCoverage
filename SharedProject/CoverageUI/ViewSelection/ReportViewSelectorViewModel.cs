using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VsThemedDialogs;
using WpfHelpers;

namespace FineCodeCoverage.Output
{
    internal sealed class ReportViewSelectorViewModel : ObservableBase, IDialogViewModel
    {
        private readonly IReportViewSelectorModel _reportViewSelectorModel;
        private readonly Action _notifyOkCommandCanExecuteChanged;
        private readonly bool _initializing = true;
        private readonly bool _hasRepositories;
        private readonly ReportViewState _initialReportViewState;
        private BranchViewModel _selectedBranch;
        private ReportContentTypeViewModel _selectedReportContentType;
        private ReportStyleViewModel _selectedReportStyle;
        private bool _changed;
        private string _selectedRepositoryPath;

        public event EventHandler<bool> Done;

        public ObservableCollection<BranchViewModel> Branches { get; } = new ObservableCollection<BranchViewModel>();

        public BranchViewModel SelectedBranch
        {
            get => _selectedBranch ?? Branches.FirstOrDefault();
            set
            {
                Set(ref _selectedBranch, value);
                RaiseOkChangedIfChanged();
            }
        }

        public ObservableCollection<ReportStyleViewModel> ReportStyles { get; } = new ObservableCollection<ReportStyleViewModel>();

        public ObservableCollection<ReportContentTypeViewModel> ReportContentTypes { get; } = new ObservableCollection<ReportContentTypeViewModel>();

        public ReportContentTypeViewModel SelectedReportContentType
        {
            get => _selectedReportContentType;
            set
            {
                Set(ref _selectedReportContentType, value);
                OnPropertyChanged(nameof(GitCombosEnabled));
                RaiseOkChangedIfChanged();
            }
        }

        public ReportStyleViewModel SelectedReportStyle
        {
            get => _selectedReportStyle;
            set
            {
                Set(ref _selectedReportStyle, value);
                RaiseOkChangedIfChanged();
            }
        }

        private void RaiseOkChangedIfChanged()
        {
            bool changedFromInitial = ChangedFromInitial();
            if (_changed == changedFromInitial)
            {
                return;
            }

            _changed = changedFromInitial;
            _notifyOkCommandCanExecuteChanged();
        }

        private bool ChangedFromInitial() => !_initializing &&
            (_initialReportViewState.ReportContentType != SelectedReportContentType.ReportContentType ||
            _initialReportViewState.ReportStyle != SelectedReportStyle.ReportStyle ||
            _initialReportViewState.SelectedBranchName != SelectedBranch?.BranchName ||
            _initialReportViewState.SelectedRepositoryPath != SelectedRepositoryPath);

        public ReportViewSelectorViewModel(IReportViewSelectorModel reportViewSelectorModel)
        {
            _reportViewSelectorModel = reportViewSelectorModel;
            var okRelayCommand = new RelayCommand(
                () =>
                {
                    reportViewSelectorModel.Update(
                        SelectedReportStyle.ReportStyle,
                        SelectedReportContentType.ReportContentType,
                        SelectedBranch.BranchName,
                        SelectedRepositoryPath);
                    Done?.Invoke(this, true);
                },
                () => _changed);
            _notifyOkCommandCanExecuteChanged = okRelayCommand.NotifyCanExecuteChanged;
            OkCommand = okRelayCommand;
            CancelCommand = new RelayCommand(() => Done?.Invoke(this, false), () => true);

            _initialReportViewState = reportViewSelectorModel.GetState();
            RepositoryPaths = _initialReportViewState.RepositoryPaths;
            _hasRepositories = RepositoryPaths.Any();
            bool canUseRepositories = _initialReportViewState.CanUseRepositories;
            if (_hasRepositories)
            {
                SelectedRepositoryPath = _initialReportViewState.SelectedRepositoryPath ?? RepositoryPaths[0];
                SelectedBranch = _initialReportViewState.SelectedBranchName == null ? null : new BranchViewModel(_initialReportViewState.SelectedBranchName);
                ShowBranchesCombo = true;
                ShowRepositoriesCombo = RepositoryPaths.Count > 1;
                ShowReportContentTypeCombo = true;
            }
            else
            {
                NoRepositoriesMessage = canUseRepositories ? "Add a git repo to filter by changeset." : "Changeset filtering available in VS2022.";
                ShowNoRepositoriesMessage = true;
            }

            var fullReportContentTypeVM = new ReportContentTypeViewModel(ReportContentType.Full, "Full");
            var changesetReportContentTypeVM = new ReportContentTypeViewModel(ReportContentType.Changeset, "Changeset");
            ReportContentTypes.Add(changesetReportContentTypeVM);
            ReportContentTypes.Add(fullReportContentTypeVM);
            SelectedReportContentType = _initialReportViewState.ReportContentType == ReportContentType.Full ?
            fullReportContentTypeVM : changesetReportContentTypeVM;

            var assemblyReportStyle = new ReportStyleViewModel(ReportStyle.Assembly, "Assembly");
            var sourceReportStyle = new ReportStyleViewModel(ReportStyle.Source, "Source");
            ReportStyles.Add(assemblyReportStyle);
            ReportStyles.Add(sourceReportStyle);
            SelectedReportStyle = _initialReportViewState.ReportStyle == ReportStyle.Assembly ?
                assemblyReportStyle : sourceReportStyle;
            _initializing = false;
            _changed = ChangedFromInitial();
        }

        public IReadOnlyList<string> RepositoryPaths { get; }

        public string NoRepositoriesMessage { get; }

        public bool ShowNoRepositoriesMessage { get; }

        public System.Windows.Input.ICommand CancelCommand { get; }

        public System.Windows.Input.ICommand OkCommand { get; }

        public bool GitCombosEnabled => _hasRepositories && SelectedReportContentType.ReportContentType == ReportContentType.Changeset;

        public bool ShowReportContentTypeCombo { get; }

        public bool ShowBranchesCombo { get; }

        public bool ShowRepositoriesCombo { get; }

        public string SelectedRepositoryPath
        {
            get => _selectedRepositoryPath;
            set
            {
                Set(ref _selectedRepositoryPath, value);
                ClearRepositoryBranches();
                foreach (string branch in _reportViewSelectorModel.GetBranches(_selectedRepositoryPath))
                {
                    Branches.Add(new BranchViewModel(branch));
                }

                RaiseOkChangedIfChanged();
            }
        }

        private void ClearRepositoryBranches()
        {
            Branches.Clear();
            SelectedBranch = null;
        }
    }
}
