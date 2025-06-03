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
        public ObservableCollection<string> Branches { get; } = new ObservableCollection<string>();
        private string selectedBranch;
        public string SelectedBranch
        {
            get => this.selectedBranch ?? this.Branches.FirstOrDefault();
            set
            {
                this.Set(ref this.selectedBranch, value);
                this.RaiseOkChangedIfChanged();
            }
        }

        public ObservableCollection<ReportStyleViewModel> ReportStyles { get; } = new ObservableCollection<ReportStyleViewModel>();
        public ObservableCollection<ReportContentTypeViewModel> ReportContentTypes { get; } = new ObservableCollection<ReportContentTypeViewModel>();
        private ReportContentTypeViewModel selectedReportContentType;
        public ReportContentTypeViewModel SelectedReportContentType
        {
            get => this.selectedReportContentType;
            set
            {
                this.Set(ref this.selectedReportContentType, value);
                this.OnPropertyChanged(nameof(this.GitCombosEnabled));
                this.RaiseOkChangedIfChanged();
            }
        }

        private ReportStyleViewModel _selectedReportStyle;
        public ReportStyleViewModel SelectedReportStyle
        {
            get => this._selectedReportStyle;
            set
            {
                this.Set(ref this._selectedReportStyle, value);
                this.RaiseOkChangedIfChanged();
            }
        }

        private bool changed;

        private void RaiseOkChangedIfChanged()
        {
            bool changedFromInitial = this.ChangedFromInitial();
            if (this.changed != changedFromInitial)
            {
                this.changed = changedFromInitial;
                this.notifyOkCommandCanExecuteChanged();
            }
        }
        private readonly Action notifyOkCommandCanExecuteChanged;
        private readonly bool initializing = true;

        private bool ChangedFromInitial() => !this.initializing &&
            (this.initialReportViewState.ReportContentType != this.SelectedReportContentType.ReportContentType ||
            this.initialReportViewState.ReportStyle != this.SelectedReportStyle.ReportStyle ||
            this.initialReportViewState.SelectedBranchName != this.SelectedBranch ||
            this.initialReportViewState.SelectedRepositoryPath != this.SelectedRepositoryPath);

        public ReportViewSelectorViewModel(IReportViewSelectorModel reportViewSelectorModel)
        {
            this.reportViewSelectorModel = reportViewSelectorModel;
            var okRelayCommand = new RelayCommand(() =>
            {
                reportViewSelectorModel.Update(
                    this.SelectedReportStyle.ReportStyle,
                    this.SelectedReportContentType.ReportContentType,
                    this.SelectedBranch,
                    this.SelectedRepositoryPath);
                Done?.Invoke(this, EventArgs.Empty);
            }, () => this.changed);
            this.notifyOkCommandCanExecuteChanged = okRelayCommand.NotifyCanExecuteChanged;
            this.OkCommand = okRelayCommand;
            this.CancelCommand = new RelayCommand(() => Done?.Invoke(this, EventArgs.Empty), () => true);

            this.initialReportViewState = reportViewSelectorModel.GetState();
            this.RepositoryPaths = this.initialReportViewState.RepositoryPaths;
            this.hasRepositories = this.RepositoryPaths.Any();
            bool canUseRepositories = this.initialReportViewState.CanUseRepositories;
            if (this.hasRepositories)
            {
                this.SelectedRepositoryPath = this.initialReportViewState.SelectedRepositoryPath ?? this.RepositoryPaths[0];
                this.SelectedBranch = this.initialReportViewState.SelectedBranchName;
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
            this.SelectedReportContentType = this.initialReportViewState.ReportContentType == ReportContentType.Full ?
            fullReportContentTypeVM : changesetReportContentTypeVM;

            var assemblyReportStyle = new ReportStyleViewModel(ReportStyle.Assembly, "Assembly");
            var sourceReportStyle = new ReportStyleViewModel(ReportStyle.Source, "Source");
            this.ReportStyles.Add(assemblyReportStyle);
            this.ReportStyles.Add(sourceReportStyle);
            this.SelectedReportStyle = this.initialReportViewState.ReportStyle == ReportStyle.Assembly ?
                assemblyReportStyle : sourceReportStyle;
            this.initializing = false;
            this.changed = this.ChangedFromInitial();
        }

        private readonly bool hasRepositories;
        public IReadOnlyList<string> RepositoryPaths { get; }
        public string NoRepositoriesMessage { get; }
        public bool ShowNoRepositoriesMessage { get; }

        public System.Windows.Input.ICommand CancelCommand { get; }

        private readonly ReportViewState initialReportViewState;

        public System.Windows.Input.ICommand OkCommand { get; }

        public bool GitCombosEnabled => this.hasRepositories && this.SelectedReportContentType.ReportContentType == ReportContentType.Changeset;
        public bool ShowReportContentTypeCombo { get; }
        public bool ShowBranchesCombo { get; }
        public bool ShowRepositoriesCombo { get; }

        private string selectedRepositoryPath;
        private readonly IReportViewSelectorModel reportViewSelectorModel;

        public string SelectedRepositoryPath
        {
            get => this.selectedRepositoryPath;
            set
            {
                this.Set(ref this.selectedRepositoryPath, value);
                this.ClearRepositoryBranches();
                foreach (string branch in this.reportViewSelectorModel.GetBranches(this.selectedRepositoryPath))
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

        public event EventHandler Done;
    }
}