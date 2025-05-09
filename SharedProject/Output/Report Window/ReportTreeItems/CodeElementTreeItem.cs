using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;
using FineCodeCoverage.Engine.ReportGenerator;
using System.Collections.Generic;
using System.Linq;

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
            this.ImageMoniker = codeElement.CodeElementType == CodeElementType.Method ?
                KnownMonikers.Method : KnownMonikers.Property;
            this.lines = codeElement.Lines;
            this.CoverableLines = codeElement.Lines.Count;
            this.CoveredLines = codeElement.Lines.Count(l => l.CoverageType == CoverageType.Covered);
            this.NotCoveredLines = codeElement.Lines.Count(l => l.CoverageType == CoverageType.NotCovered);
            this.PartialLines = codeElement.Lines.Count(l => l.CoverageType == CoverageType.Partial);
            this.NPathComplexity = codeElement.NPathComplexity;
            this.CrapScore = codeElement.CrapScore;
            this.CyclomaticComplexity = codeElement.CyclomaticComplexity;
            this.BlocksCovered = codeElement.BlocksCovered;
            this.BlocksNotCovered = codeElement.BlocksNotCovered;
            this.TotalBranches = codeElement.TotalBranches;
            this.CoveredBranches = codeElement.BranchesCovered;
            this.codeElement = codeElement;
        }
        private readonly IReadOnlyList<ICoberturaLine> lines;
        public override ImageMoniker ImageMoniker { get; }
        public int FileLine => this.lines[0].Number;
        public string FilePath => codeElement.Path;
    }
}
