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
        private bool coverageRunning;
        private ReportTotalRow reportTotalRow;
        private bool rootDirectoryNameFromPath;
        private Report lastReport;
        private readonly ObservableCollection<ReportTreeItemBase> _items = new ObservableCollection<ReportTreeItemBase>();
        private readonly ISourceFileOpener sourceFileOpener;
        private readonly IReportTreeExpander treeExpander;
        private readonly IReportViews reportViews;
        private readonly IIconsOptions iconsOptions;
        private ReportStyle? lastReportStyle = null;
        private TotalTreeItem totalTreeItem;
        private SourceFileStructure sourceFileStructure;

        [ImportingConstructor]
        public ReportViewModel(
            IEventAggregator eventAggregator,
            ISourceFileOpener sourceFileOpener,
            IReportTreeExpander treeExpander,
            IReportColumnManager reportColumnManager,
            IReportViews reportViews,
            IIconsOptions iconsOptions,
            IOptionsProvider<ReportOptions> reportOptionsProvider
        )
        {
            ReportOptions reportOptions = reportOptionsProvider.Get();
            this.sourceFileStructure = reportOptions.SourceFileStructure;
            this.reportTotalRow = reportOptions.ReportTotalRow;
            this.rootDirectoryNameFromPath = reportOptions.RootDirectoryNameFromPath;
            reportOptionsProvider.OptionsChanged += (newOptions) =>
            {
                if (newOptions.ReportTotalRow != this.reportTotalRow)
                {
                    this.UpdateTotalRow(newOptions.ReportTotalRow);
                }

                if (newOptions.RootDirectoryNameFromPath != this.rootDirectoryNameFromPath)
                {
                    this.UpdateRootDirectoryTreeItemNames(newOptions.RootDirectoryNameFromPath);
                }

                if (newOptions.SourceFileStructure != this.sourceFileStructure)
                {
                    this.UpdateSourceViewStructure(newOptions.SourceFileStructure);
                }
            };
            this.TreeViewAutomationName = "Coverage Report Tree";
            _ = eventAggregator.AddListener(this);
            this.SetItems(this._items);
            this.sourceFileOpener = sourceFileOpener;
            this.treeExpander = treeExpander;
            this.ColumnManagerImpl = reportColumnManager;
            this.reportViews = reportViews;
            this.iconsOptions = iconsOptions;
            reportViews.Changed += this.ReportViews_Changed;
            this.SetIconsAdjustment();
            iconsOptions.ShowIconsChanged += this.AdjustIcons;
            iconsOptions.IconSizeChanged += this.AdjustIcons;
        }

        private void UpdateSourceViewStructure(SourceFileStructure newSourceFileStructure)
        {
            this.sourceFileStructure = newSourceFileStructure;
            if (this.lastReportStyle == ReportStyle.Source)
            {
                this.GenerateReport(null);
            }
        }
        private void UpdateRootDirectoryTreeItemNames(bool rootDirectoryNameFromPath)
        {
            this.rootDirectoryNameFromPath = rootDirectoryNameFromPath;
            if (this.lastReportStyle == ReportStyle.Source)
            {
                this.Items.OfType<RootDirectoryTreeItem>().ToList().ForEach(d => d.SetName(rootDirectoryNameFromPath));
            }
        }

        private void UpdateTotalRow(ReportTotalRow newReportTotalRow)
        {
            this.reportTotalRow = newReportTotalRow;
            if (this._items.Count > 0)
            {
                bool firstItemIsTotal = this._items[0] is TotalTreeItem;
                switch (this.reportTotalRow)
                {
                    case ReportTotalRow.Always:
                        if (!firstItemIsTotal)
                        {
                            this.InsertTotalRow();
                        }

                        break;
                    case ReportTotalRow.WhenRequired:
                        if (!firstItemIsTotal && this._items.Count > 1)
                        {
                            this.InsertTotalRow();
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

        private void AdjustIcons(object sender, EventArgs e)
        {
            this.SetIconsAdjustment();
            this.AdjustWidths(this.Items);
        }

        private void InsertTotalRow()
        {
            this._items.Insert(0, this.totalTreeItem);
            this.AdjustWidths(new ITreeItem[] { this.totalTreeItem });
        }

        private void AdjustWidths(IEnumerable<ITreeItem> items)
        {
            double firstColumnWidth = this.ColumnManagerImpl.Columns[0].Width.Value;
            foreach (ITreeItem item in items)
            {
                item.AdjustWidth(firstColumnWidth);
            }
        }

        private void SetIconsAdjustment()
            => ReportTreeItemBase.SharedAdditionalAdjustment = this.iconsOptions.ShowIcons ?
            this.iconsOptions.IconSize + 10 : 0;

        private void ReportViews_Changed(object sender, ReportViewChangedEventArgs e)
        {
            if (this.lastReport != null)
            {
                if (e.ChangesetChanged)
                {
                    this.GenerateReport(this.reportViews.GetChangeset());
                }
                else
                {
                    this.GenerateReport(null);
                }
            }
        }

        protected override IReportColumnManager ColumnManagerImpl { get; set; }

        public bool CoverageRunning
        {
            get => this.coverageRunning;
            set => this.Set(ref this.coverageRunning, value);
        }

        private void GenerateReport(IChangeset newChangeset)
            => ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                List<ReportTreeItemBase> newItems = this.reportViews.ReportStyle == ReportStyle.Assembly ? this.CreateAssemblyTreeItems() :
                    this.CreateSourceTreeItems();
                this.AddTotalRowIfRequired(newItems);
                this.RestoreExpansionStateIfRequired(newItems);
                this.AdjustWidths(newItems);
                this.AddTreeItems(newItems);
                this.lastReportStyle = this.reportViews.ReportStyle;
            });

        private void AddTreeItems(IList<ReportTreeItemBase> newItems)
        {
            this._items.Clear();
            this._items.AddRange(newItems);
        }

        private List<ReportTreeItemBase> CreateAssemblyTreeItems()
            => this.lastReport.Assemblies.Select(assembly =>
            {
                bool isTestAssembly = false;
                if (this.lastReport.TestAssemblyNames.Contains(assembly.Name))
                {
                    isTestAssembly = true;
                }

                return (ReportTreeItemBase)new AssemblyTreeItem(assembly, isTestAssembly);
            }).ToList();

        private List<ReportTreeItemBase> CreateSourceTreeItems()
        {
            IDirectory rootDirectory = this.lastReport.Directory;
            if (rootDirectory != null)
            {
                if (rootDirectory.SourceFiles.Any())
                {
                    var directoryTreeItem = new RootDirectoryTreeItem(rootDirectory, rootDirectory.Name, this.rootDirectoryNameFromPath, this.sourceFileStructure);
                    return new List<ReportTreeItemBase>
                    {
                        directoryTreeItem
                    };
                }

                return rootDirectory.SubDirectories.Select(d => (ReportTreeItemBase)new RootDirectoryTreeItem(
                    d, Path.Combine(rootDirectory.Name, d.Name), this.rootDirectoryNameFromPath, this.sourceFileStructure)
                ).ToList();
            }

            return new List<ReportTreeItemBase>();
        }

        private void RestoreExpansionStateIfRequired(IList<ReportTreeItemBase> newItems)
        {
            if (this._items.Count > 0 && this.reportViews.ReportStyle == this.lastReportStyle)
            {
                this.treeExpander.RestoreExpansionState(this._items, newItems);
            }
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
            this.lastReport = new Report(message);
            this.lastReport.DirectoryStructureChanged += this.LastReport_DirectoryStructureChanged;
            this.ColumnManagerImpl.ShowRelevantColumns(this.lastReport.MetricTypes);
            this.GenerateReport(this.reportViews.GetChangeset());
        }

        private void LastReport_DirectoryStructureChanged(object sender, EventArgs e)
        {
            if (this.lastReport != null && this.reportViews.ReportStyle == ReportStyle.Source)
            {
                this.GenerateReport(null);
            }
        }

        public void Handle(ClearReportMessage message)
        {
            this.lastReport = null;
            this.lastReportStyle = null;
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

        public void Handle(CoverageEndedMessage message) => this.CoverageRunning = false;

        public void Handle(NewCodeChangedMessage message)
            => this.lastReport?.NewCodeChanged(message.Path, message.HasNewCode);
    }
}
