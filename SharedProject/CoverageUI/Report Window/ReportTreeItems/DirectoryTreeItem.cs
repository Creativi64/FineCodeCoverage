using System.Linq;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Output
{
    internal class DirectoryTreeItem : ReportTreeItemBase
    {
        public DirectoryTreeItem(IDirectory directory, SourceFileStructure sourceFileStructure)
        {
            Name = directory.Name;
            foreach (IDirectory subDirectory in directory.SubDirectories)
            {
                ObservableChildren.Add(new DirectoryTreeItem(subDirectory, sourceFileStructure) { Parent = this });
            }

            foreach (ISourceFile sourceFile in directory.SourceFiles)
            {
                ObservableChildren.Add(new SourceFileTreeItem(sourceFile, sourceFileStructure) { Parent = this });
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
