using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Messages;
using FineCodeCoverage.Engine.ReportGenerator;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output.Report_Window.ReportTreeItems;
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

        private ReportTotalRow reportTotalRow;
        [ImportingConstructor]
        public ReportViewModel(
            IEventAggregator eventAggregator,
            ISourceFileOpener sourceFileOpener,
            IReportTreeExpander treeExpander,
            IReportColumnManager reportColumnManager,
            IReportViews reportViews,
            IIconsOptions iconsOptions,
            IAppOptionsProvider appOptionsProvider
        )
        {
            this.reportTotalRow = appOptionsProvider.Get().ReportTotalRow;
            appOptionsProvider.OptionsChanged += (newOptions) =>
            {
                if (newOptions.ReportTotalRow != this.reportTotalRow)
                {
                    this.reportTotalRow = newOptions.ReportTotalRow;
                    if (this._items.Count > 0)
                    {
                        var firstItemIsTotal = this._items[0] is TotalTreeItem;
                        switch (this.reportTotalRow)
                        {
                            case ReportTotalRow.Always:
                                
                                if (!firstItemIsTotal)
                                {
                                    this._items.Insert(0, this.totalTreeItem);
                                    AdjustWidths(this.totalTreeItem);
                                }
                                break;
                            case ReportTotalRow.WhenRequired:
                                if (!firstItemIsTotal && this._items.Count > 1)
                                {
                                    this._items.Insert(0, this.totalTreeItem);
                                    AdjustWidths(this.totalTreeItem);
                                }
                                break;
                            case ReportTotalRow.Never:
                                if (firstItemIsTotal)
                                {
                                    this._items.RemoveAt(0);
                                }
                                break;
                        }
                    }
                }
            };
            this.TreeViewAutomationName = "Coverage Report Tree";
            _ = eventAggregator.AddListener(this);
            this.SetItems(this._items);
            this.sourceFileOpener = sourceFileOpener;
            this.treeExpander = treeExpander;
            ColumnManagerImpl = reportColumnManager;
            this.reportViews = reportViews;
            this.iconsOptions = iconsOptions;
            reportViews.Changed += ReportViews_Changed;
            SetIconsAdjustment();
            iconsOptions.ShowIconsChanged += AdjustIcons;
            iconsOptions.IconSizeChanged += AdjustIcons;
        }

        private void AdjustIcons(object sender, EventArgs e)
        {
            SetIconsAdjustment();
            AdjustWidths(Items);
        }

        private void AdjustWidths(params ITreeItem[] items)
        {
            AdjustWidths((IEnumerable<ITreeItem>)items);
        }

        private void AdjustWidths(IEnumerable<ITreeItem> items)
        {
            double firstColumnWidth = this.ColumnManagerImpl.Columns[0].Width.Value;
            foreach (var item in items)
            {
                item.AdjustWidth(firstColumnWidth);
            }
        }

        private void SetIconsAdjustment()
        {
            ReportTreeItemBase.SharedAdditionalAdjustment = this.iconsOptions.ShowIcons ? this.iconsOptions.IconSize + 10 : 0;
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
        private readonly IIconsOptions iconsOptions;

        protected override IReportColumnManager ColumnManagerImpl { get; set; }

        private bool coverageRunning;

        public bool CoverageRunning
        {
            get => this.coverageRunning;
            set => this.Set(ref this.coverageRunning, value);
        }
        private ReportStyle? lastReportStyle = null;
        private TotalTreeItem totalTreeItem;

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
                        if (rootDirectory.SourceFiles.Any())
                        {
                            var directoryTreeItem = new DirectoryTreeItem(rootDirectory);
                            newItems.Add(directoryTreeItem);
                        }
                        else
                        {
                            newItems.AddRange(rootDirectory.SubDirectories.Select(d => new DirectoryTreeItem(d)));
                        }
                        
                    }
                }
                this.AddTotalRowIfRequired(newItems);
                this.RestoreExpansionStateIfRequired(newItems);

                this._items.Clear();
                double firstColumnWidth = this.ColumnManagerImpl.Columns[0].Width.Value;
                foreach (var newItem in newItems)
                {
                    newItem.AdjustWidth(firstColumnWidth);
                    this._items.Add(newItem);
                }
            });
        }

        private void RestoreExpansionStateIfRequired(IList<ReportTreeItemBase> newItems)
        {

            if (this._items.Count > 0 && reportViews.ReportStyle == lastReportStyle)
            {
                this.treeExpander.RestoreExpansionState(this._items, newItems);
            }
            lastReportStyle = reportViews.ReportStyle;
        }

        private void AddTotalRowIfRequired(List<ReportTreeItemBase> newItems)
        {
            this.totalTreeItem = new TotalTreeItem(newItems);
            switch (this.reportTotalRow)
            {
                case ReportTotalRow.Always:
                    newItems.Insert(0, this.totalTreeItem);
                    break;
                case ReportTotalRow.WhenRequired:
                    if (newItems.Count > 1)
                    {
                        newItems.Insert(0, this.totalTreeItem);
                    }
                    break;
            }
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
            this.totalTreeItem = null;
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
