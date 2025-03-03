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
using Microsoft.VisualStudio.Threading;
using TreeGrid;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ReportViewModel))]
    internal class ReportViewModel : TreeGridViewModelBase<ReportTreeItemBase, IReportColumnManager>, 
        IListener<NewReportMessage>,
        IListener<CoverageStartingMessage>,
        IListener<CoverageEndedMessage>
    {
        private List<string> testAssemblyNames;
        // Factory for the specific tree items
        [ImportingConstructor]
        public ReportViewModel(
            IEventAggregator eventAggregator,
            ISourceFileOpener sourceFileOpener,
            ITreeExpander treeExpander,
            IReportColumnManager reportColumnManager
        )
        {
            this.TreeViewAutomationName = "Coverage Report Tree";
            _ = eventAggregator.AddListener(this);
            this.SetItems(this._items);
            this.sourceFileOpener = sourceFileOpener;
            this.treeExpander = treeExpander;
            ColumnManagerImpl = reportColumnManager;
        }
        private readonly ObservableCollection<ReportTreeItemBase> _items = new ObservableCollection<ReportTreeItemBase>();
        private readonly ISourceFileOpener sourceFileOpener;
        private readonly ITreeExpander treeExpander;

        protected override IReportColumnManager ColumnManagerImpl { get; set; }
        
        private bool coverageRunning;
        public bool CoverageRunning
        {
            get => this.coverageRunning;
            set => this.Set(ref this.coverageRunning, value, nameof(this.CoverageRunning));
        }

        public void Handle(NewReportMessage message)
        {
            if(message.Report != null)
            {
                this.ColumnManagerImpl.ShowRelevantColumns(message.Report.MetricTypes);
                IReadOnlyCollection<IAssembly> assemblies = message.Report.Assemblies;
                var rootDirectory = message.Report.Directory;
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    double firstColumnWidth = this.ColumnManagerImpl.Columns[0].Width.Value;
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    

                    List<ReportTreeItemBase> newItems = new List<ReportTreeItemBase>();
                    var assembliesView = false;
                    if (assembliesView)
                    {
                        foreach (IAssembly assembly in assemblies)
                        {
                            bool isTestAssembly = false;
                            if (this.testAssemblyNames != null && this.testAssemblyNames.Contains(assembly.Name))
                            {
                                isTestAssembly = true;
                            }

                            var assemblyTreeItem = new AssemblyTreeItem(assembly, isTestAssembly);
                            newItems.Add(assemblyTreeItem);
                        }
                    }
                    else
                    {
                        var directoryTreeItem = new DirectoryTreeItem(rootDirectory);
                        newItems.Add(directoryTreeItem);
                    }

                    if (this._items.Count > 0)
                    {
                        this.treeExpander.RestoreExpansionState(this._items,newItems);
                    }

                    this._items.Clear();
                    foreach (var newItem in newItems)
                    {
                        newItem.AdjustWidth(firstColumnWidth);
                        this._items.Add(newItem);
                    }
                    
                }).Join();
            }
            else
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    double firstColumnWidth = this.ColumnManagerImpl.Columns[0].Width.Value;
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    this._items.Clear();
                }).Join();
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
            this.testAssemblyNames = message.CoverageProjects?.Select(cp => cp.ProjectName).ToList();
            this.CoverageRunning = false;
        }
    }
}
