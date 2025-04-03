using FineCodeCoverage.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TreeGrid;

namespace FineCodeCoverage.Output
{
    internal class ReportViewState {
        public ReportViewState(
            ReportStyle reportStyle,
            ReportContentType reportContentType,
            string selectedRepository,
            string selectedBranchName,
            List<string> repositories,
            bool canUseRepositories
        )
        {
            ReportStyle = reportStyle;
            ReportContentType = reportContentType;
            SelectedRepository = selectedRepository;
            SelectedBranchName = selectedBranchName;
            RepositoryPaths = repositories;
            CanUseRepositories = canUseRepositories;
        }

        public ReportStyle ReportStyle { get; }
        public ReportContentType ReportContentType { get; }
        public string SelectedRepository { get; }
        public string SelectedBranchName { get; }
        public List<string> RepositoryPaths { get; }
        public bool CanUseRepositories { get; }
    }

    internal class ReportStyleViewModel
    {
        public ReportStyleViewModel(ReportStyle reportStyle, string display)
        {
            ReportStyle = reportStyle;
            Display = display;
        }

        public ReportStyle ReportStyle { get; }
        public string Display { get; }
        public override string ToString()
        {
            return Display;
        }
    }

    internal class ReportContentTypeViewModel
    {
        public ReportContentTypeViewModel(ReportContentType reportContentType, string display)
        {
            ReportContentType = reportContentType;
            Display = display;
        }

        public ReportContentType ReportContentType { get; }
        public string Display { get; }
        public override string ToString()
        {
            return Display;
        }
    }

    internal interface IReportViewSelectorModel
    {
        ReportViewState GetState();
        void Update(ReportStyle reportStyle,
            ReportContentType reportContentType,
            string selectedBranchName,
            string selectedRepositoryPath);
        IEnumerable<string> GetBranches(string selectedRepositoryPath);
    }

    internal class ReportViewSelectorViewModel : ObservableBase, IDialogViewModel
    {
        public ObservableCollection<string> Branches { get; } = new ObservableCollection<string>();
        private string selectedBranch;
        public string SelectedBranch
        {
            get => selectedBranch ?? Branches.FirstOrDefault();
            set
            {
                Set(ref selectedBranch, value);
                RaiseOkChangedIfChanged();
            }
        }

        public ObservableCollection<ReportStyleViewModel> ReportStyles { get; } = new ObservableCollection<ReportStyleViewModel>();
        public ObservableCollection<ReportContentTypeViewModel> ReportContentTypes { get; } = new ObservableCollection<ReportContentTypeViewModel>();
        private ReportContentTypeViewModel selectedReportContentType;
        public ReportContentTypeViewModel SelectedReportContentType
        {
            get => selectedReportContentType;
            set
            {
                Set(ref selectedReportContentType, value);
                OnPropertyChanged(nameof(GitCombosEnabled));
                RaiseOkChangedIfChanged();
            }
        }


        private ReportStyleViewModel _selectedReportStyle;
        public ReportStyleViewModel SelectedReportStyle
        {
            get { return _selectedReportStyle; }
            set
            {
                this.Set(ref _selectedReportStyle, value);
                RaiseOkChangedIfChanged();
            }
        }

        private bool changed;

        private void RaiseOkChangedIfChanged()
        {
            var changedFromInitial = ChangedFromInitial();
            if(changed != changedFromInitial)
            {
                changed = changedFromInitial;
                notifyOkCommandCanExecuteChanged();
            }
        }
        private readonly Action notifyOkCommandCanExecuteChanged;
        private readonly bool initializing = true;

        private bool ChangedFromInitial()
        {
            if (initializing) return false;
            return initialReportViewState.ReportContentType != SelectedReportContentType.ReportContentType ||
                initialReportViewState.ReportStyle != SelectedReportStyle.ReportStyle ||
                initialReportViewState.SelectedBranchName != SelectedBranch ||
                initialReportViewState.SelectedRepository != SelectedRepositoryPath;
        }

        public ReportViewSelectorViewModel(IReportViewSelectorModel reportViewSelectorModel)
        {
            this.reportViewSelectorModel = reportViewSelectorModel;
            var okRelayCommand = new RelayCommand(() =>
            {
                // this will only allow pressing if anything has changed
                reportViewSelectorModel.Update(SelectedReportStyle.ReportStyle,SelectedReportContentType.ReportContentType,SelectedBranch, SelectedRepositoryPath);
                Done?.Invoke(this, EventArgs.Empty);
            }, () => changed);
            notifyOkCommandCanExecuteChanged = okRelayCommand.NotifyCanExecuteChanged;
            OkCommand = okRelayCommand;
            CancelCommand = new RelayCommand(() => Done?.Invoke(this, EventArgs.Empty), () => true);

            initialReportViewState = reportViewSelectorModel.GetState();
            RepositoryPaths = initialReportViewState.RepositoryPaths;
            hasRepositories = RepositoryPaths.Any();
            var canUseRepositories = initialReportViewState.CanUseRepositories;
            if (hasRepositories)
            {
                SelectedRepositoryPath = initialReportViewState.SelectedRepository ?? RepositoryPaths[0];
                SelectedBranch = initialReportViewState.SelectedBranchName;
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
            SelectedReportContentType = initialReportViewState.ReportContentType == ReportContentType.Full ?
            fullReportContentTypeVM : changesetReportContentTypeVM;

            var assemblyReportStyle = new ReportStyleViewModel(ReportStyle.Assembly, "Assembly");
            var sourceReportStyle = new ReportStyleViewModel(ReportStyle.Source, "Source");
            ReportStyles.Add(assemblyReportStyle);
            ReportStyles.Add(sourceReportStyle);
            SelectedReportStyle = initialReportViewState.ReportStyle == ReportStyle.Assembly ?
                assemblyReportStyle : sourceReportStyle;
            initializing = false;
            changed = ChangedFromInitial();
        }

        private readonly bool hasRepositories;
        public List<string> RepositoryPaths { get; }
        public string NoRepositoriesMessage { get; }
        public bool ShowNoRepositoriesMessage { get; }

        public ICommand CancelCommand { get; }

        private readonly ReportViewState initialReportViewState;

        public ICommand OkCommand { get; }

        public bool GitCombosEnabled => hasRepositories && SelectedReportContentType.ReportContentType == ReportContentType.Changeset;
        public bool ShowReportContentTypeCombo { get; }
        public bool ShowBranchesCombo { get; }
        public bool ShowRepositoriesCombo { get; }

        private string selectedRepositoryPath;
        private readonly IReportViewSelectorModel reportViewSelectorModel;

        public string SelectedRepositoryPath
        {
            get => selectedRepositoryPath;
            set
            {
                Set(ref selectedRepositoryPath, value);
                if (hasRepositories)
                {
                    ClearRepositoryBranches();
                    foreach (var branch in reportViewSelectorModel.GetBranches(selectedRepositoryPath))
                    {
                        this.Branches.Add(branch);
                    }
                }
                RaiseOkChangedIfChanged();
            }
        }

        private void ClearRepositoryBranches()
        {
            this.Branches.Clear();
            SelectedBranch = null;
        }

        public event EventHandler Done;
    }
}
