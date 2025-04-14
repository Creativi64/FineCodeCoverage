using System.Linq;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Output
{
    public class CodeElementTreeItem : ReportTreeItemBase
    {
        public CodeElementTreeItem(
            ICodeElement codeElement
        )
        {
            this.Name = codeElement.Name;
            this.FilePath = codeElement.Path;
            this.FileLine = codeElement.StartLine;
            this.ImageMoniker = codeElement.CodeElementType == CodeElementType.Method ? 
                KnownMonikers.Method : KnownMonikers.Property;
            this.CoverableLines = codeElement.Lines.Count;
            this.NPathComplexity = codeElement.NPathComplexity;
            this.CrapScore = codeElement.CrapScore;
            this.CyclomaticComplexity = codeElement.CyclomaticComplexity;
            this.BlocksCovered = codeElement.BlocksCovered;
            this.BlocksNotCovered = codeElement.BlocksNotCovered;
        }

        public override ImageMoniker ImageMoniker { get; }
        public int FileLine { get; internal set; }
        public string FilePath { get; internal set; }
    }
}
