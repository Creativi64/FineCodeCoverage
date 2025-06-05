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
            ICodeElement codeElement)
        {
            Name = codeElement.Name;
            ImageMoniker = codeElement.CodeElementType == CodeElementType.Method ?
                KnownMonikers.Method : KnownMonikers.Property;
            _lines = codeElement.Lines;
            CoverableLines = codeElement.Lines.Count;
            CoveredLines = codeElement.Lines.Count(l => l.CoverageType == CoverageType.Covered);
            NotCoveredLines = codeElement.Lines.Count(l => l.CoverageType == CoverageType.NotCovered);
            PartialLines = codeElement.Lines.Count(l => l.CoverageType == CoverageType.Partial);
            NPathComplexity = codeElement.NPathComplexity;
            CrapScore = codeElement.CrapScore;
            CyclomaticComplexity = codeElement.CyclomaticComplexity;
            TotalBranches = codeElement.TotalBranches;
            CoveredBranches = codeElement.BranchesCovered;
            _codeElement = codeElement;
        }

        public override ImageMoniker ImageMoniker { get; }

        public int FileLine => _lines[0].Number;

        public string FilePath => _codeElement.Path;
    }
}
