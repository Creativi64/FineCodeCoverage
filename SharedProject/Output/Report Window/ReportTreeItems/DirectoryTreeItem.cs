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
            this.Name = directory.Name;
            foreach (IDirectory subDirectory in directory.SubDirectories)
            {
                this.ObservableChildren.Add(new DirectoryTreeItem(subDirectory, sourceFileStructure) { Parent = this });
            }

            foreach (ISourceFile sourceFile in directory.SourceFiles)
            {
                this.ObservableChildren.Add(new SourceFileTreeItem(sourceFile, sourceFileStructure) { Parent = this });
            }

            this.CoverableLines = this.ObservableChildren.Sum(c => c.CoverableLines);
            this.CoveredLines = this.ObservableChildren.Sum(c => c.CoveredLines);
            this.NotCoveredLines = this.ObservableChildren.Sum(c => c.NotCoveredLines);
            this.PartialLines = this.ObservableChildren.Sum(c => c.PartialLines);
            this.NPathComplexity = this.ObservableChildren.Sum(c => c.NPathComplexity);
            this.CrapScore = this.ObservableChildren.Sum(c => c.CrapScore);
            this.CyclomaticComplexity = this.ObservableChildren.Sum(c => c.CyclomaticComplexity);
            this.TotalBranches = this.ObservableChildren.Sum(c => c.TotalBranches);
            this.CoveredBranches = this.ObservableChildren.Sum(c => c.CoveredBranches);
        }

        public override ImageMoniker ImageMoniker => KnownMonikers.FolderClosed;
    }
}
