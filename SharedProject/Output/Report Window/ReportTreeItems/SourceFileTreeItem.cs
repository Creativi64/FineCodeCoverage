using System.Linq;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;
using FineCodeCoverage.Engine.ReportGenerator;
using System.IO;

namespace FineCodeCoverage.Output
{
    internal class SourceFileTreeItem : ReportTreeItemBase
    {
        public SourceFileTreeItem(ISourceFile sourceFile)
        {
            this.Name = Path.GetFileName(sourceFile.Path);
            this.ImageMoniker = KnownMonikers.TextFile;
            sourceFile.Classes.ToList().ForEach(clss => this.observableChildren.Add(new ClassTreeItem(clss) { Parent = this }));

            this.CoverableLines = this.observableChildren.Sum(c => c.CoverableLines);
            this.NPathComplexity = this.observableChildren.Sum(c => c.NPathComplexity);
            this.CrapScore = this.observableChildren.Sum(c => c.CrapScore);
            this.CyclomaticComplexity = this.observableChildren.Sum(c => c.CyclomaticComplexity);
            this.BlocksCovered = this.observableChildren.Sum(c => c.BlocksCovered);
            this.BlocksNotCovered = this.observableChildren.Sum(c => c.BlocksNotCovered);
        }
        public override ImageMoniker ImageMoniker { get; }
    }
}
