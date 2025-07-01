using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using FineCodeCoverage.Collection.Messages;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.Shell;
using TreeGrid;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ReportViewModel))]
    internal sealed class ReportViewModel : TreeGridViewModelBase<ReportTreeItemBase, IReportColumnManager>,
        IListener<NewReportMessage>,
        IListener<CoverageStartingMessage>,
        IListener<CoverageEndedMessage>,
        IListener<ClearReportMessage>,
        IListener<NewCodeChangedMessage>
    {
        private readonly ObservableCollection<ReportTreeItemBase> _items = new ObservableCollection<ReportTreeItemBase>();
        private readonly ISourceFileOpener _sourceFileOpener;
        private readonly IReportTreeExpander _treeExpander;
        private readonly IReportViews _reportViews;
        private readonly IIconMeasurementOptions _iconsOptions;
        private bool _coverageRunning;
        private ReportTotalRow _reportTotalRow;
        private bool _rootDirectoryNameFromPath;
        private ReportModel _lastReport;
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
            IIconMeasurementOptions iconsOptions,
            IOptionsProvider<ReportOptions> reportOptionsProvider)
        {
            ReportOptions reportOptions = reportOptionsProvider.Get();
            _sourceFileStructure = reportOptions.SourceFileStructure;
            _reportTotalRow = reportOptions.ReportTotalRow;
            _rootDirectoryNameFromPath = reportOptions.RootDirectoryNameFromPath;
            reportOptionsProvider.OptionsChanged += (newOptions) =>
            {
                if (newOptions.ReportTotalRow != _reportTotalRow)
                {
                    UpdateTotalRow(newOptions.ReportTotalRow);
                }

                if (newOptions.RootDirectoryNameFromPath != _rootDirectoryNameFromPath)
                {
                    UpdateRootDirectoryTreeItemNames(newOptions.RootDirectoryNameFromPath);
                }

                if (newOptions.SourceFileStructure == _sourceFileStructure)
                {
                    return;
                }

                UpdateSourceViewStructure(newOptions.SourceFileStructure);
            };
            TreeViewAutomationName = "Coverage Report Tree";
            _ = eventAggregator.AddListener(this);
            SetItems(_items);
            _sourceFileOpener = sourceFileOpener;
            _treeExpander = treeExpander;
            ColumnManagerImpl = reportColumnManager;
            _reportViews = reportViews;
            _iconsOptions = iconsOptions;
            reportViews.Changed += ReportViews_Changed;
            SetIconsAdjustment();
            iconsOptions.ShowIconsChanged += AdjustIcons;
            iconsOptions.IconSizeChanged += AdjustIcons;
        }

        private void UpdateSourceViewStructure(SourceFileStructure newSourceFileStructure)
        {
            _sourceFileStructure = newSourceFileStructure;
            if (_lastReportStyle != ReportStyle.Source)
            {
                return;
            }

            GenerateReport(null);
        }

        private void UpdateRootDirectoryTreeItemNames(bool rootDirectoryNameFromPath)
        {
            _rootDirectoryNameFromPath = rootDirectoryNameFromPath;
            if (_lastReportStyle != ReportStyle.Source)
            {
                return;
            }

            Items.OfType<RootDirectoryTreeItem>().ToList().ForEach(d => d.SetName(rootDirectoryNameFromPath));
        }

        private void UpdateTotalRow(ReportTotalRow newReportTotalRow)
        {
            _reportTotalRow = newReportTotalRow;
            if (_items.Count == 0)
            {
                return;
            }

            bool firstItemIsTotal = _items[0] is TotalTreeItem;
            switch (_reportTotalRow)
            {
                case ReportTotalRow.Always:
                    if (!firstItemIsTotal)
                    {
                        InsertTotalRow();
                    }

                    break;
                case ReportTotalRow.WhenRequired:
                    if (!firstItemIsTotal && _items.Count > 1)
                    {
                        InsertTotalRow();
                    }

                    break;
                case ReportTotalRow.Never:
                    if (firstItemIsTotal)
                    {
                        _items.RemoveAt(0);
                    }

                    break;
                default:
                    break;
            }
        }

        private void AdjustIcons(object sender, EventArgs e)
        {
            SetIconsAdjustment();
            AdjustWidths(Items);
        }

        private void InsertTotalRow()
        {
            _items.Insert(0, _totalTreeItem);
            AdjustWidths(new ITreeItem[] { _totalTreeItem });
        }

        private void AdjustWidths(IEnumerable<ITreeItem> items)
        {
            double firstColumnWidth = ColumnManagerImpl.Columns[0].Width.Value;
            foreach (ITreeItem item in items)
            {
                item.AdjustWidth(firstColumnWidth);
            }
        }

        private void SetIconsAdjustment()
            => ReportTreeItemBase.SharedAdditionalAdjustment = _iconsOptions.ShowIcons ?
            _iconsOptions.IconSize + 10 : 0;

        private void ReportViews_Changed(object sender, ReportViewChangedEventArgs e)
        {
            if (_lastReport == null)
            {
                return;
            }

            if (e.ChangesetChanged)
            {
                GenerateReport(_reportViews.GetChangeset());
            }
            else
            {
                GenerateReport(null);
            }
        }

        protected override IReportColumnManager ColumnManagerImpl { get; set; }

        public bool CoverageRunning
        {
            get => _coverageRunning;
            set => Set(ref _coverageRunning, value);
        }

        private void GenerateReport(IChangeset newChangeset)
            => ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                List<ReportTreeItemBase> newItems = _reportViews.ReportStyle == ReportStyle.Assembly ? CreateAssemblyTreeItems() :
                    CreateSourceTreeItems();
                AddTotalRowIfRequired(newItems);
                RestoreExpansionStateIfRequired(newItems);
                AdjustWidths(newItems);
                AddTreeItems(newItems);
                _lastReportStyle = _reportViews.ReportStyle;
            });

        private void AddTreeItems(IList<ReportTreeItemBase> newItems)
        {
            _items.Clear();
            _items.AddRange(newItems);
        }

        private List<ReportTreeItemBase> CreateAssemblyTreeItems()
            => _lastReport.Assemblies.Select(assembly =>
            {
                bool isTestAssembly = false;
                if (_lastReport.TestAssemblyNames.Contains(assembly.Name))
                {
                    isTestAssembly = true;
                }

                return (ReportTreeItemBase)new AssemblyTreeItem(assembly, isTestAssembly);
            }).ToList();

        private List<ReportTreeItemBase> CreateSourceTreeItems()
        {
            IDirectory rootDirectory = _lastReport.Directory;
            if (rootDirectory != null)
            {
                if (rootDirectory.SourceFiles.Any())
                {
                    var directoryTreeItem = new RootDirectoryTreeItem(rootDirectory, rootDirectory.Name, _rootDirectoryNameFromPath, _sourceFileStructure);
                    return new List<ReportTreeItemBase>
                    {
                        directoryTreeItem,
                    };
                }

                return rootDirectory.SubDirectories.Select(d => (ReportTreeItemBase)new RootDirectoryTreeItem(
                    d, Path.Combine(rootDirectory.Name, d.Name), _rootDirectoryNameFromPath, _sourceFileStructure))
                .ToList();
            }

            return new List<ReportTreeItemBase>();
        }

        private void RestoreExpansionStateIfRequired(IList<ReportTreeItemBase> newItems)
        {
            if (_items.Count == 0 || _reportViews.ReportStyle != _lastReportStyle)
            {
                return;
            }

            _treeExpander.RestoreExpansionState(_items, newItems);
        }

        private void AddTotalRowIfRequired(List<ReportTreeItemBase> newItems)
        {
            _totalTreeItem = new TotalTreeItem(newItems);
            switch (_reportTotalRow)
            {
                case ReportTotalRow.Always:
                    newItems.Insert(0, _totalTreeItem);
                    break;
                case ReportTotalRow.WhenRequired:
                    if (newItems.Count > 1)
                    {
                        newItems.Insert(0, _totalTreeItem);
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
            _lastReport = new ReportModel(message);
            _lastReport.DirectoryStructureChanged += LastReport_DirectoryStructureChanged;
            ColumnManagerImpl.ShowRelevantColumns(_lastReport.MetricTypes);
            GenerateReport(_reportViews.GetChangeset());
        }

        private void LastReport_DirectoryStructureChanged(object sender, EventArgs e)
        {
            if (_lastReport == null || _reportViews.ReportStyle != ReportStyle.Source)
            {
                return;
            }

            GenerateReport(null);
        }

        public void Handle(ClearReportMessage message)
        {
            _lastReport = null;
            _lastReportStyle = null;
            _totalTreeItem = null;
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                double firstColumnWidth = ColumnManagerImpl.Columns[0].Width.Value;
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                _items.Clear();
            });
        }

        public override void Sort(int displayIndex) => ColumnManagerImpl.SortColumns(displayIndex);

        protected override void LeafTreeItemDoubleClick(ReportTreeItemBase treeItem)
        {
            var codeElementTreeItem = treeItem as CodeElementTreeItem;
            if (IsRelativePath(codeElementTreeItem.FilePath) || !File.Exists(codeElementTreeItem.FilePath))
            {
                return;
            }

            _ = _sourceFileOpener.OpenAsync(codeElementTreeItem.FilePath, codeElementTreeItem.FileLine);
        }

        public static bool IsRelativePath(string path) => Uri.IsWellFormedUriString(path, UriKind.Relative);

        public void Handle(CoverageStartingMessage message) => CoverageRunning = true;

        public void Handle(CoverageEndedMessage message) => CoverageRunning = false;

        public void Handle(NewCodeChangedMessage message)
            => _lastReport?.NewCodeChanged(message.Path, message.HasNewCode);
    }
}
