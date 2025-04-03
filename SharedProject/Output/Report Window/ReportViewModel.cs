using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Messages;
using FineCodeCoverage.Engine.ReportGenerator;
using Microsoft.VisualStudio.Shell;
using TreeGrid;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ReportViewModel))]
    internal class ReportViewModel : TreeGridViewModelBase<ReportTreeItemBase, IReportColumnManager>,
        IListener<NewReportMessage>,
        IListener<CoverageStartingMessage>,
        IListener<CoverageEndedMessage>
    {
        [ImportingConstructor]
        public ReportViewModel(
            IEventAggregator eventAggregator,
            ISourceFileOpener sourceFileOpener,
            ITreeExpander treeExpander,
            IReportColumnManager reportColumnManager,
            IReportViews reportViews
        )
        {
            this.TreeViewAutomationName = "Coverage Report Tree";
            _ = eventAggregator.AddListener(this);
            this.SetItems(this._items);
            this.sourceFileOpener = sourceFileOpener;
            this.treeExpander = treeExpander;
            ColumnManagerImpl = reportColumnManager;
            this.reportViews = reportViews;
            reportViews.Changed += ReportViews_Changed;
        }

        private void ReportViews_Changed(object sender, EventArgs e)
        {
            TakeViews();
            if(lastReport != null)
            {
                GenerateReport();
            }
        }

        private class Report
        {
            public Report(NewReportMessage message)
            {
                TestAssemblyNames = message.CoverageProjects?.Select(cp => cp.ProjectName).ToList();
                Assemblies = message.Report.Assemblies;
                MetricTypes = message.Report.MetricTypes;
                Directory = message.Report.Directory; // lazy ?
            }

            public List<string> TestAssemblyNames { get; }
            public IReadOnlyCollection<IAssembly> Assemblies { get; }
            public List<MetricType> MetricTypes { get; internal set; }
            public IDirectory Directory { get; internal set; }
        }

        private bool initializedView;
        private ReportStyle reportStyle;
        private ReportContentType reportContentType;
        private Report lastReport;
        private readonly ObservableCollection<ReportTreeItemBase> _items = new ObservableCollection<ReportTreeItemBase>();
        private readonly ISourceFileOpener sourceFileOpener;
        private readonly ITreeExpander treeExpander;
        private readonly IReportViews reportViews;

        protected override IReportColumnManager ColumnManagerImpl { get; set; }

        private bool coverageRunning;
        public bool CoverageRunning
        {
            get => this.coverageRunning;
            set => this.Set(ref this.coverageRunning, value, nameof(this.CoverageRunning));
        }

        private void TakeViews()
        {
            reportStyle = reportViews.ReportStyle;
            reportContentType = reportViews.ReportContentType;
        }

        private void EnsureInitializedView()
        {
            if (!initializedView)
            {
                TakeViews();
                initializedView = true;
            }
        }

        private void GenerateReport()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                double firstColumnWidth = this.ColumnManagerImpl.Columns[0].Width.Value;
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                List<ReportTreeItemBase> newItems = new List<ReportTreeItemBase>();
                if (reportStyle == ReportStyle.Assembly)
                {
                    IReadOnlyCollection<IAssembly> assemblies = lastReport.Assemblies;
                    foreach (IAssembly assembly in assemblies)
                    {
                        bool isTestAssembly = false;
                        if (lastReport.TestAssemblyNames.Contains(assembly.Name))
                        {
                            isTestAssembly = true;
                        }

                        var assemblyTreeItem = new AssemblyTreeItem(assembly, isTestAssembly);
                        newItems.Add(assemblyTreeItem);
                    }
                }
                else
                {
                    var rootDirectory = lastReport.Directory;
                    if (rootDirectory != null)
                    {
                        var directoryTreeItem = new DirectoryTreeItem(rootDirectory);
                        newItems.Add(directoryTreeItem);
                    }
                }

                if (this._items.Count > 0)
                {
                    this.treeExpander.RestoreExpansionState(this._items, newItems);
                }

                this._items.Clear();
                foreach (var newItem in newItems)
                {
                    newItem.AdjustWidth(firstColumnWidth);
                    this._items.Add(newItem);
                }
            });
        }

        public void Handle(NewReportMessage message)
        {
            if(message.Report != null)
            {
                EnsureInitializedView();
                lastReport = new Report(message);
                this.ColumnManagerImpl.ShowRelevantColumns(lastReport.MetricTypes);
                GenerateReport();
            }
            else
            {
                lastReport = null;
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    double firstColumnWidth = this.ColumnManagerImpl.Columns[0].Width.Value;
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    this._items.Clear();
                });
            }
        }

        public override void Sort(int displayIndex) => this.ColumnManagerImpl.SortColumns(displayIndex);
        protected override void LeafTreeItemDoubleClick(ReportTreeItemBase treeItem)
        {
            var codeElementTreeItem = treeItem as CodeElementTreeItem;
            if (!IsRelativePath(codeElementTreeItem.FilePath))
            {
                _ = this.sourceFileOpener.OpenAsync(codeElementTreeItem.FilePath, codeElementTreeItem.FileLine);
            }
        }

        public static bool IsRelativePath(string path) => Uri.IsWellFormedUriString(path, UriKind.Relative);
        public void Handle(CoverageStartingMessage message) => this.CoverageRunning = true;
        public void Handle(CoverageEndedMessage message)
        {
            this.CoverageRunning = false;
        }
    }
}
