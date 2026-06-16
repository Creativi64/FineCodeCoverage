using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Collection.ReportGeneration;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Output
{
    public class CodeElementTreeItem : ReportTreeItemBase
    {
        private readonly ICodeElement _codeElement;
        private readonly IReadOnlyList<ICoberturaLine> _lines;

        public CodeElementTreeItem(
            ICodeElement codeElement,
            IChangeset changeset = null)
        {
            Name = codeElement.Name;
            ImageMoniker = codeElement.CodeElementType == CodeElementType.Method ?
                KnownMonikers.Method : KnownMonikers.Property;
            _lines = codeElement.Lines;
            IReadOnlyList<ICoberturaLine> coverageLines = ChangesetLines(codeElement, changeset);
            CoverableLines = coverageLines.Count;
            CoveredLines = coverageLines.Count(l => l.CoverageType == CoverageType.Covered);
            NotCoveredLines = coverageLines.Count(l => l.CoverageType == CoverageType.NotCovered);
            PartialLines = coverageLines.Count(l => l.CoverageType == CoverageType.Partial);
            NPathComplexity = codeElement.NPathComplexity;
            CrapScore = codeElement.CrapScore;
            CyclomaticComplexity = codeElement.CyclomaticComplexity;
            TotalBranches = codeElement.TotalBranches;
            CoveredBranches = codeElement.BranchesCovered;
            _codeElement = codeElement;
        }

        // In changeset mode only the changed lines count towards coverage; the
        // element drops out of the report when none of its lines were changed.
        private static IReadOnlyList<ICoberturaLine> ChangesetLines(ICodeElement codeElement, IChangeset changeset)
        {
            if (changeset == null)
            {
                return codeElement.Lines;
            }

            var changedLineNumbers = new HashSet<int>(changeset.GetLineNumbers(codeElement.Path));
            return codeElement.Lines.Where(line => changedLineNumbers.Contains(line.Number)).ToList();
        }

        internal override bool HasChangesetContent => CoverableLines > 0;

        public override ImageMoniker ImageMoniker { get; }

        public int FileLine => _lines[0].Number;

        public string FilePath => _codeElement.Path;
    }
}
