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
            ImageMoniker = KnownMonikers.FolderClosed;
            foreach (var subDirectory in directory.SubDirectories)
            {
                this.observableChildren.Add(new DirectoryTreeItem(subDirectory, sourceFileStructure) { Parent = this });
            }
            foreach (var sourceFile in directory.SourceFiles)
            {
                this.observableChildren.Add(new SourceFileTreeItem(sourceFile, sourceFileStructure) { Parent = this });
            }

            this.CoverableLines = this.observableChildren.Sum(c => c.CoverableLines);
            this.CoveredLines = this.observableChildren.Sum(c => c.CoveredLines);
            this.NotCoveredLines = this.observableChildren.Sum(c => c.NotCoveredLines);
            this.PartialLines = this.observableChildren.Sum(c => c.PartialLines);
            this.NPathComplexity = this.observableChildren.Sum(c => c.NPathComplexity);
            this.CrapScore = this.observableChildren.Sum(c => c.CrapScore);
            this.CyclomaticComplexity = this.observableChildren.Sum(c => c.CyclomaticComplexity);
            this.TotalBranches = this.observableChildren.Sum(c => c.TotalBranches);
            this.CoveredBranches = this.observableChildren.Sum(c => c.CoveredBranches);
        }


        public override ImageMoniker ImageMoniker { get; }
    }

    internal class RootDirectoryTreeItem : DirectoryTreeItem
    {
        private readonly string name;
        private readonly string path;

        public RootDirectoryTreeItem(IDirectory directory, string path, bool nameIsPath, SourceFileStructure sourceFileStructure) : base(directory, sourceFileStructure)
        {
            this.name = Name;
            this.path = path;
            SetName(nameIsPath);
        }

        public void SetName(bool fromPath)
        {
            this.Name = fromPath ? path : name;
        }

    }

}
