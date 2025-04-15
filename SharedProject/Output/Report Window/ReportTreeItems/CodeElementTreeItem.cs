using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Output
{
    public class CodeElementTreeItem : ReportTreeItemBase
    {
        private readonly ICodeElement codeElement;

        public CodeElementTreeItem(
            ICodeElement codeElement
        )
        {
            this.Name = codeElement.Name;
            this.FileLine = codeElement.StartLine;
            this.ImageMoniker = codeElement.CodeElementType == CodeElementType.Method ?
                KnownMonikers.Method : KnownMonikers.Property;
            this.CoverableLines = codeElement.Lines.Count;
            this.NPathComplexity = codeElement.NPathComplexity;
            this.CrapScore = codeElement.CrapScore;
            this.CyclomaticComplexity = codeElement.CyclomaticComplexity;
            this.BlocksCovered = codeElement.BlocksCovered;
            this.BlocksNotCovered = codeElement.BlocksNotCovered;
            this.codeElement = codeElement;
        }

        public override ImageMoniker ImageMoniker { get; }
        public int FileLine { get; internal set; }
        public string FilePath => codeElement.Path;
    }
}
