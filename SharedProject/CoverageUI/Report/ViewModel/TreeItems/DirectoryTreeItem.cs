using System.Linq;
using FineCodeCoverage.Options.Report;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Output
{
    internal class DirectoryTreeItem : ReportTreeItemBase
    {
        public DirectoryTreeItem(IDirectory directory, SourceFileStructure sourceFileStructure, IChangeset changeset = null)
        {
            Name = directory.Name;
            foreach (IDirectory subDirectory in directory.SubDirectories)
            {
                var subDirectoryTreeItem = new DirectoryTreeItem(subDirectory, sourceFileStructure, changeset) { Parent = this };
                if (changeset == null || subDirectoryTreeItem.HasChangesetContent)
                {
                    ObservableChildren.Add(subDirectoryTreeItem);
                }
            }

            foreach (ISourceFile sourceFile in directory.SourceFiles)
            {
                var sourceFileTreeItem = new SourceFileTreeItem(sourceFile, sourceFileStructure, changeset) { Parent = this };
                if (changeset == null || sourceFileTreeItem.HasChangesetContent)
                {
                    ObservableChildren.Add(sourceFileTreeItem);
                }
            }

            CoverableLines = ObservableChildren.Sum(c => c.CoverableLines);
            CoveredLines = ObservableChildren.Sum(c => c.CoveredLines);
            NotCoveredLines = ObservableChildren.Sum(c => c.NotCoveredLines);
            PartialLines = ObservableChildren.Sum(c => c.PartialLines);
            NPathComplexity = ObservableChildren.Sum(c => c.NPathComplexity);
            CrapScore = ObservableChildren.Sum(c => c.CrapScore);
            CyclomaticComplexity = ObservableChildren.Sum(c => c.CyclomaticComplexity);
            TotalBranches = ObservableChildren.Sum(c => c.TotalBranches);
            CoveredBranches = ObservableChildren.Sum(c => c.CoveredBranches);
        }

        public override ImageMoniker ImageMoniker => KnownMonikers.FolderClosed;
    }
}
