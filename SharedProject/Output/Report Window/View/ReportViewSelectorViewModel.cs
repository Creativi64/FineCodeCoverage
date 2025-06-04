using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FineCodeCoverage.Wpf;
using WpfHelpers;

namespace FineCodeCoverage.Output
{
    internal class ReportViewSelectorViewModel : ObservableBase, IDialogViewModel
    {
        private string _selectedBranch;
        private ReportContentTypeViewModel _selectedReportContentType;
        private ReportStyleViewModel _selectedReportStyle;
        private bool _changed;
        private readonly Action _notifyOkCommandCanExecuteChanged;
        private readonly bool _initializing = true;
        private readonly bool _hasRepositories;
        private readonly ReportViewState _initialReportViewState;
        private string _selectedRepositoryPath;
        private readonly IReportViewSelectorModel _reportViewSelectorModel;

        public event EventHandler Done;

        public ObservableCollection<string> Branches { get; } = new ObservableCollection<string>();

        public string SelectedBranch
        {
            get => this._selectedBranch ?? this.Branches.FirstOrDefault();
            set
            {
                this.Set(ref this._selectedBranch, value);
                this.RaiseOkChangedIfChanged();
            }
        }

        public ObservableCollection<ReportStyleViewModel> ReportStyles { get; } = new ObservableCollection<ReportStyleViewModel>();

        public ObservableCollection<ReportContentTypeViewModel> ReportContentTypes { get; } = new ObservableCollection<ReportContentTypeViewModel>();

        public ReportContentTypeViewModel SelectedReportContentType
        {
            get => this._selectedReportContentType;
            set
            {
                this.Set(ref this._selectedReportContentType, value);
                this.OnPropertyChanged(nameof(this.GitCombosEnabled));
                this.RaiseOkChangedIfChanged();
            }
        }

        public ReportStyleViewModel SelectedReportStyle
        {
            get => this._selectedReportStyle;
            set
            {
                this.Set(ref this._selectedReportStyle, value);
                this.RaiseOkChangedIfChanged();
            }
        }

        private void RaiseOkChangedIfChanged()
        {
            bool changedFromInitial = this.ChangedFromInitial();
            if (this._changed != changedFromInitial)
            {
                this._changed = changedFromInitial;
                this._notifyOkCommandCanExecuteChanged();
            }
        }

        private bool ChangedFromInitial() => !this._initializing &&
            (this._initialReportViewState.ReportContentType != this.SelectedReportContentType.ReportContentType ||
            this._initialReportViewState.ReportStyle != this.SelectedReportStyle.ReportStyle ||
            this._initialReportViewState.SelectedBranchName != this.SelectedBranch ||
            this._initialReportViewState.SelectedRepositoryPath != this.SelectedRepositoryPath);

        public ReportViewSelectorViewModel(IReportViewSelectorModel reportViewSelectorModel)
        {
            this._reportViewSelectorModel = reportViewSelectorModel;
            var okRelayCommand = new RelayCommand(() =>
            {
                reportViewSelectorModel.Update(
                    this.SelectedReportStyle.ReportStyle,
                    this.SelectedReportContentType.ReportContentType,
                    this.SelectedBranch,
                    this.SelectedRepositoryPath);
                Done?.Invoke(this, EventArgs.Empty);
            }, () => this._changed);
            this._notifyOkCommandCanExecuteChanged = okRelayCommand.NotifyCanExecuteChanged;
            this.OkCommand = okRelayCommand;
            this.CancelCommand = new RelayCommand(() => Done?.Invoke(this, EventArgs.Empty), () => true);

            this._initialReportViewState = reportViewSelectorModel.GetState();
            this.RepositoryPaths = this._initialReportViewState.RepositoryPaths;
            this._hasRepositories = this.RepositoryPaths.Any();
            bool canUseRepositories = this._initialReportViewState.CanUseRepositories;
            if (this._hasRepositories)
            {
                this.SelectedRepositoryPath = this._initialReportViewState.SelectedRepositoryPath ?? this.RepositoryPaths[0];
                this.SelectedBranch = this._initialReportViewState.SelectedBranchName;
                this.ShowBranchesCombo = true;
                this.ShowRepositoriesCombo = this.RepositoryPaths.Count > 1;
                this.ShowReportContentTypeCombo = true;
            }
            else
            {
                this.NoRepositoriesMessage = canUseRepositories ? "Add a git repo to filter by changeset." : "Changeset filtering available in VS2022.";
                this.ShowNoRepositoriesMessage = true;
            }

            var fullReportContentTypeVM = new ReportContentTypeViewModel(ReportContentType.Full, "Full");
            var changesetReportContentTypeVM = new ReportContentTypeViewModel(ReportContentType.Changeset, "Changeset");
            this.ReportContentTypes.Add(changesetReportContentTypeVM);
            this.ReportContentTypes.Add(fullReportContentTypeVM);
            this.SelectedReportContentType = this._initialReportViewState.ReportContentType == ReportContentType.Full ?
            fullReportContentTypeVM : changesetReportContentTypeVM;

            var assemblyReportStyle = new ReportStyleViewModel(ReportStyle.Assembly, "Assembly");
            var sourceReportStyle = new ReportStyleViewModel(ReportStyle.Source, "Source");
            this.ReportStyles.Add(assemblyReportStyle);
            this.ReportStyles.Add(sourceReportStyle);
            this.SelectedReportStyle = this._initialReportViewState.ReportStyle == ReportStyle.Assembly ?
                assemblyReportStyle : sourceReportStyle;
            this._initializing = false;
            this._changed = this.ChangedFromInitial();
        }

        public IReadOnlyList<string> RepositoryPaths { get; }

        public string NoRepositoriesMessage { get; }

        public bool ShowNoRepositoriesMessage { get; }

        public System.Windows.Input.ICommand CancelCommand { get; }

        public System.Windows.Input.ICommand OkCommand { get; }

        public bool GitCombosEnabled => this._hasRepositories && this.SelectedReportContentType.ReportContentType == ReportContentType.Changeset;

        public bool ShowReportContentTypeCombo { get; }

        public bool ShowBranchesCombo { get; }

        public bool ShowRepositoriesCombo { get; }

        public string SelectedRepositoryPath
        {
            get => this._selectedRepositoryPath;
            set
            {
                this.Set(ref this._selectedRepositoryPath, value);
                this.ClearRepositoryBranches();
                foreach (string branch in this._reportViewSelectorModel.GetBranches(this._selectedRepositoryPath))
                {
                    this.Branches.Add(branch);
                }

                this.RaiseOkChangedIfChanged();
            }
        }

        private void ClearRepositoryBranches()
        {
            this.Branches.Clear();
            this.SelectedBranch = null;
        }
    }
}