using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Engine.ReportGenerator;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Output
{
    public class CodeElementTreeItem : ReportTreeItemBase
    {
        private readonly ICodeElement _codeElement;
        private readonly IReadOnlyList<ICoberturaLine> _lines;

        public CodeElementTreeItem(
            ICodeElement codeElement
        )
        {
            this.Name = codeElement.Name;
            this.ImageMoniker = codeElement.CodeElementType == CodeElementType.Method ?
                KnownMonikers.Method : KnownMonikers.Property;
            this._lines = codeElement.Lines;
            this.CoverableLines = codeElement.Lines.Count;
            this.CoveredLines = codeElement.Lines.Count(l => l.CoverageType == CoverageType.Covered);
            this.NotCoveredLines = codeElement.Lines.Count(l => l.CoverageType == CoverageType.NotCovered);
            this.PartialLines = codeElement.Lines.Count(l => l.CoverageType == CoverageType.Partial);
            this.NPathComplexity = codeElement.NPathComplexity;
            this.CrapScore = codeElement.CrapScore;
            this.CyclomaticComplexity = codeElement.CyclomaticComplexity;
            this.TotalBranches = codeElement.TotalBranches;
            this.CoveredBranches = codeElement.BranchesCovered;
            this._codeElement = codeElement;
        }

        public override ImageMoniker ImageMoniker { get; }

        public int FileLine => this._lines[0].Number;

        public string FilePath => this._codeElement.Path;
    }
}