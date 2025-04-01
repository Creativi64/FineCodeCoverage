using FineCodeCoverage.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TreeGrid;

namespace FineCodeCoverage.Output
{
    internal class InitialReportViewState { 
        public InitialReportViewState(ReportStyle reportStyle)
        {
            ReportStyle = reportStyle;
        }

        public ReportStyle ReportStyle { get; }
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
    }

    internal interface IReportViewSelectorModel
    {
        InitialReportViewState Initialize();
        bool CanUseChangeset { get; }
        void Update(ReportStyle reportStyle);
    }

    internal class ReportViewSelectorViewModel : ObservableBase, IDialogViewModel
    {
        public ObservableCollection<ReportStyleViewModel> ReportStyles { get; }
        private ReportStyleViewModel _selectedReportStyle;
        public ReportStyleViewModel SelectedReportStyle
        {
            get { return _selectedReportStyle; }
            set
            {
                this.Set(ref _selectedReportStyle, value);
            }
        }

        public ReportViewSelectorViewModel(IReportViewSelectorModel reportViewModel)
        {
            OkCommand = new RelayCommand(() =>
            {
                // this will only allow pressing if anything has changed
                reportViewModel.Update(SelectedReportStyle.ReportStyle);
                Done?.Invoke(this, EventArgs.Empty);
            }, () => true);
            CancelCommand = new RelayCommand(() => Done?.Invoke(this, EventArgs.Empty), () => true);

            var initialReportViewState = reportViewModel.Initialize();
            ReportStyles = new ObservableCollection<ReportStyleViewModel>();
            var assemblyReportStyle = new ReportStyleViewModel(ReportStyle.Assembly, "Assembly");
            var sourceReportStyle = new ReportStyleViewModel(ReportStyle.Source, "Source");
            ReportStyles.Add(assemblyReportStyle);
            ReportStyles.Add(sourceReportStyle);
            SelectedReportStyle = initialReportViewState.ReportStyle == ReportStyle.Assembly ?
                assemblyReportStyle : sourceReportStyle;

        }

        public ICommand CancelCommand { get; }
        public ICommand OkCommand { get; }

        public event EventHandler Done;
    }
}
