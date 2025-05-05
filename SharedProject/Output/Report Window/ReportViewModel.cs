using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage;
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
        IListener<CoverageEndedMessage>,
        IListener<ClearReportMessage>,
        IListener<NewCodeChangedMessage>
    {
        [ImportingConstructor]
        public ReportViewModel(
            IEventAggregator eventAggregator,
            ISourceFileOpener sourceFileOpener,
            IReportTreeExpander treeExpander,
            IReportColumnManager reportColumnManager,
            IReportViews reportViews,
            IShowIcons showIcons
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
            SetIconsAdjustment(showIcons.ShowIcons);
            showIcons.ShowIconsChanged += (sender, args) =>
            {
                SetIconsAdjustment(showIcons.ShowIcons);
                double firstColumnWidth = this.ColumnManagerImpl.Columns[0].Width.Value;
                foreach (var item in Items)
                {
                    item.AdjustWidth(firstColumnWidth);
                }
            };
        }

        private void SetIconsAdjustment(bool showIcons)
        {
            ReportTreeItemBase.SharedAdditionalAdjustment = showIcons ? 26 : 0;
        }

        private void ReportViews_Changed(object sender, ReportViewChangedEventArgs e)
        {
            if(lastReport != null)
            {
                if (e.ChangesetChanged)
                {
                    GenerateReport(reportViews.GetChangeset());
                }
                else
                {
                    GenerateReport(null);
                }
            }
        }

        private Report lastReport;
        private readonly ObservableCollection<ReportTreeItemBase> _items = new ObservableCollection<ReportTreeItemBase>();
        private readonly ISourceFileOpener sourceFileOpener;
        private readonly IReportTreeExpander treeExpander;
        private readonly IReportViews reportViews;


        protected override IReportColumnManager ColumnManagerImpl { get; set; }

        private bool coverageRunning;

        public bool CoverageRunning
        {
            get => this.coverageRunning;
            set => this.Set(ref this.coverageRunning, value);
        }
        private ReportStyle? lastReportStyle = null;
        private void GenerateReport(IChangeset newChangeset)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                List<ReportTreeItemBase> newItems = new List<ReportTreeItemBase>();
                if (reportViews.ReportStyle == ReportStyle.Assembly)
                {
                    foreach (IAssembly assembly in lastReport.Assemblies)
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

                if (this._items.Count > 0 && reportViews.ReportStyle == lastReportStyle)
                {
                    this.treeExpander.RestoreExpansionState(this._items, newItems);
                }

                this._items.Clear();
                double firstColumnWidth = this.ColumnManagerImpl.Columns[0].Width.Value;
                foreach (var newItem in newItems)
                {
                    newItem.AdjustWidth(firstColumnWidth);
                    this._items.Add(newItem);
                }
                lastReportStyle = reportViews.ReportStyle;
            });
        }

        public void Handle(NewReportMessage message)
        {
            lastReport = new Report(message);
            lastReport.DirectoryStructureChanged += LastReport_DirectoryStructureChanged;
            this.ColumnManagerImpl.ShowRelevantColumns(lastReport.MetricTypes);
            GenerateReport(reportViews.GetChangeset());
        }

        private void LastReport_DirectoryStructureChanged(object sender, EventArgs e)
        {
            if(this.lastReport != null && this.reportViews.ReportStyle == ReportStyle.Source)
            {
                GenerateReport(null);
            }
        }

        public void Handle(ClearReportMessage message)
        {
            lastReport = null;
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                double firstColumnWidth = this.ColumnManagerImpl.Columns[0].Width.Value;
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                this._items.Clear();
            });
        }

        public override void Sort(int displayIndex) => this.ColumnManagerImpl.SortColumns(displayIndex);
        protected override void LeafTreeItemDoubleClick(ReportTreeItemBase treeItem)
        {
            var codeElementTreeItem = treeItem as CodeElementTreeItem;
            if (!IsRelativePath(codeElementTreeItem.FilePath) && File.Exists(codeElementTreeItem.FilePath))
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

        public void Handle(NewCodeChangedMessage message)
        {
            lastReport?.NewCodeChanged(message.Path, message.HasNewCode);
        }
    }
}
