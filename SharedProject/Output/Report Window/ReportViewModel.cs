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
        private bool _coverageRunning;
        private ReportTotalRow _reportTotalRow;
        private bool _rootDirectoryNameFromPath;
        private Report _lastReport;
        private readonly ObservableCollection<ReportTreeItemBase> _items = new ObservableCollection<ReportTreeItemBase>();
        private readonly ISourceFileOpener _sourceFileOpener;
        private readonly IReportTreeExpander _treeExpander;
        private readonly IReportViews _reportViews;
        private readonly IIconsOptions _iconsOptions;
        private ReportStyle? _lastReportStyle = null;
        private TotalTreeItem _totalTreeItem;
        private SourceFileStructure _sourceFileStructure;

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
            this._sourceFileStructure = reportOptions.SourceFileStructure;
            this._reportTotalRow = reportOptions.ReportTotalRow;
            this._rootDirectoryNameFromPath = reportOptions.RootDirectoryNameFromPath;
            reportOptionsProvider.OptionsChanged += (newOptions) =>
            {
                if (newOptions.ReportTotalRow != this._reportTotalRow)
                {
                    this.UpdateTotalRow(newOptions.ReportTotalRow);
                }

                if (newOptions.RootDirectoryNameFromPath != this._rootDirectoryNameFromPath)
                {
                    this.UpdateRootDirectoryTreeItemNames(newOptions.RootDirectoryNameFromPath);
                }

                if (newOptions.SourceFileStructure == this._sourceFileStructure)
                {
                    return;
                }

                this.UpdateSourceViewStructure(newOptions.SourceFileStructure);
            };
            this.TreeViewAutomationName = "Coverage Report Tree";
            _ = eventAggregator.AddListener(this);
            this.SetItems(this._items);
            this._sourceFileOpener = sourceFileOpener;
            this._treeExpander = treeExpander;
            this.ColumnManagerImpl = reportColumnManager;
            this._reportViews = reportViews;
            this._iconsOptions = iconsOptions;
            reportViews.Changed += this.ReportViews_Changed;
            this.SetIconsAdjustment();
            iconsOptions.ShowIconsChanged += this.AdjustIcons;
            iconsOptions.IconSizeChanged += this.AdjustIcons;
        }

        private void UpdateSourceViewStructure(SourceFileStructure newSourceFileStructure)
        {
            this._sourceFileStructure = newSourceFileStructure;
            if (this._lastReportStyle != ReportStyle.Source)
            {
                return;
            }

            this.GenerateReport(null);
        }
        private void UpdateRootDirectoryTreeItemNames(bool rootDirectoryNameFromPath)
        {
            this._rootDirectoryNameFromPath = rootDirectoryNameFromPath;
            if (this._lastReportStyle != ReportStyle.Source)
            {
                return;
            }

            this.Items.OfType<RootDirectoryTreeItem>().ToList().ForEach(d => d.SetName(rootDirectoryNameFromPath));
        }

        private void UpdateTotalRow(ReportTotalRow newReportTotalRow)
        {
            this._reportTotalRow = newReportTotalRow;
            if (this._items.Count == 0)
            {
                return;
            }

            bool firstItemIsTotal = this._items[0] is TotalTreeItem;
            switch (this._reportTotalRow)
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
                default:
                    break;
            }
        }

        private void AdjustIcons(object sender, EventArgs e)
        {
            this.SetIconsAdjustment();
            this.AdjustWidths(this.Items);
        }

        private void InsertTotalRow()
        {
            this._items.Insert(0, this._totalTreeItem);
            this.AdjustWidths(new ITreeItem[] { this._totalTreeItem });
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
            => ReportTreeItemBase.SharedAdditionalAdjustment = this._iconsOptions.ShowIcons ?
            this._iconsOptions.IconSize + 10 : 0;

        private void ReportViews_Changed(object sender, ReportViewChangedEventArgs e)
        {
            if (this._lastReport == null)
            {
                return;
            }

            if (e.ChangesetChanged)
            {
                this.GenerateReport(this._reportViews.GetChangeset());
            }
            else
            {
                this.GenerateReport(null);
            }
        }

        protected override IReportColumnManager ColumnManagerImpl { get; set; }

        public bool CoverageRunning
        {
            get => this._coverageRunning;
            set => this.Set(ref this._coverageRunning, value);
        }

        private void GenerateReport(IChangeset newChangeset)
            => ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                List<ReportTreeItemBase> newItems = this._reportViews.ReportStyle == ReportStyle.Assembly ? this.CreateAssemblyTreeItems() :
                    this.CreateSourceTreeItems();
                this.AddTotalRowIfRequired(newItems);
                this.RestoreExpansionStateIfRequired(newItems);
                this.AdjustWidths(newItems);
                this.AddTreeItems(newItems);
                this._lastReportStyle = this._reportViews.ReportStyle;
            });

        private void AddTreeItems(IList<ReportTreeItemBase> newItems)
        {
            this._items.Clear();
            this._items.AddRange(newItems);
        }

        private List<ReportTreeItemBase> CreateAssemblyTreeItems()
            => this._lastReport.Assemblies.Select(assembly =>
            {
                bool isTestAssembly = false;
                if (this._lastReport.TestAssemblyNames.Contains(assembly.Name))
                {
                    isTestAssembly = true;
                }

                return (ReportTreeItemBase)new AssemblyTreeItem(assembly, isTestAssembly);
            }).ToList();

        private List<ReportTreeItemBase> CreateSourceTreeItems()
        {
            IDirectory rootDirectory = this._lastReport.Directory;
            if (rootDirectory != null)
            {
                if (rootDirectory.SourceFiles.Any())
                {
                    var directoryTreeItem = new RootDirectoryTreeItem(rootDirectory, rootDirectory.Name, this._rootDirectoryNameFromPath, this._sourceFileStructure);
                    return new List<ReportTreeItemBase>
                    {
                        directoryTreeItem
                    };
                }

                return rootDirectory.SubDirectories.Select(d => (ReportTreeItemBase)new RootDirectoryTreeItem(
                    d, Path.Combine(rootDirectory.Name, d.Name), this._rootDirectoryNameFromPath, this._sourceFileStructure)
                ).ToList();
            }

            return new List<ReportTreeItemBase>();
        }

        private void RestoreExpansionStateIfRequired(IList<ReportTreeItemBase> newItems)
        {
            if (this._items.Count == 0 || this._reportViews.ReportStyle != this._lastReportStyle)
            {
                return;
            }

            this._treeExpander.RestoreExpansionState(this._items, newItems);
        }

        private void AddTotalRowIfRequired(List<ReportTreeItemBase> newItems)
        {
            this._totalTreeItem = new TotalTreeItem(newItems);
            switch (this._reportTotalRow)
            {
                case ReportTotalRow.Always:
                    newItems.Insert(0, this._totalTreeItem);
                    break;
                case ReportTotalRow.WhenRequired:
                    if (newItems.Count > 1)
                    {
                        newItems.Insert(0, this._totalTreeItem);
                    }

                    break;
                case ReportTotalRow.Never:
                    break;
                default:
                    break;
            }
        }

        public void Handle(NewReportMessage message)
        {
            this._lastReport = new Report(message);
            this._lastReport.DirectoryStructureChanged += this.LastReport_DirectoryStructureChanged;
            this.ColumnManagerImpl.ShowRelevantColumns(this._lastReport.MetricTypes);
            this.GenerateReport(this._reportViews.GetChangeset());
        }

        private void LastReport_DirectoryStructureChanged(object sender, EventArgs e)
        {
            if (this._lastReport == null || this._reportViews.ReportStyle != ReportStyle.Source)
            {
                return;
            }

            this.GenerateReport(null);
        }

        public void Handle(ClearReportMessage message)
        {
            this._lastReport = null;
            this._lastReportStyle = null;
            this._totalTreeItem = null;
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
            if (IsRelativePath(codeElementTreeItem.FilePath) || !File.Exists(codeElementTreeItem.FilePath))
            {
                return;
            }

            _ = this._sourceFileOpener.OpenAsync(codeElementTreeItem.FilePath, codeElementTreeItem.FileLine);
        }

        public static bool IsRelativePath(string path) => Uri.IsWellFormedUriString(path, UriKind.Relative);

        public void Handle(CoverageStartingMessage message) => this.CoverageRunning = true;

        public void Handle(CoverageEndedMessage message) => this.CoverageRunning = false;

        public void Handle(NewCodeChangedMessage message)
            => this._lastReport?.NewCodeChanged(message.Path, message.HasNewCode);
    }
}