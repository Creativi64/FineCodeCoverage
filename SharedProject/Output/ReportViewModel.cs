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
    internal class ReportViewModel : TreeGridViewModelBase<ReportTreeItemBase, ReportColumnManager>, 
        IListener<NewReportMessage>,
        IListener<CoverageStartingMessage>,
        IListener<CoverageEndedMessage>
    {
        private List<string> testAssemblyNames;
        //ReportColumnManager to be injected by interface
        // Factory for the specific tree items
        [ImportingConstructor]
        public ReportViewModel(
            IEventAggregator eventAggregator,
            ISourceFileOpener sourceFileOpener
        )
        {
            this.TreeViewAutomationName = "Coverage Report Tree";
            _ = eventAggregator.AddListener(this);
            this.SetItems(this._items);
            this.sourceFileOpener = sourceFileOpener;
        }
        private readonly ObservableCollection<ReportTreeItemBase> _items = new ObservableCollection<ReportTreeItemBase>();
        private readonly ISourceFileOpener sourceFileOpener;

        protected override ReportColumnManager ColumnManagerImpl { get; set; } = new ReportColumnManager();
        
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
                _ = ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    double firstColumnWidth = this.ColumnManagerImpl.Columns[0].Width.Value;
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    this._items.Clear();

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
                            assemblyTreeItem.AdjustWidth(firstColumnWidth);
                            this._items.Add(assemblyTreeItem);
                        }
                    }
                    else
                    {
                        var directoryTreeItem = new DirectoryTreeItem(rootDirectory);
                        directoryTreeItem.AdjustWidth(firstColumnWidth);
                        this._items.Add(directoryTreeItem);
                    }
                });
            }
            else
            {
                this._items.Clear();
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
