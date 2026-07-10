using System.Collections.Generic;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Output
{
    internal sealed class TotalTreeItem : ReportTreeItemBase
    {
        public TotalTreeItem(IEnumerable<ReportTreeItemBase> children)
        {
            foreach (ReportTreeItemBase child in children)
            {
                CoverableLines += child.CoverableLines;
                CoveredLines += child.CoveredLines;
                NotCoveredLines += child.NotCoveredLines;
                PartialLines += child.PartialLines;

                NPathComplexity += child.NPathComplexity;
                CrapScore += child.CrapScore;
                CyclomaticComplexity += child.CyclomaticComplexity;

                TotalBranches += child.TotalBranches;
                CoveredBranches += child.CoveredBranches;
            }

            Name = "Total";
        }

        // possibilities: AutoSum, Aggregate, Summary, Statistics
        public override ImageMoniker ImageMoniker { get; } = KnownMonikers.AutoSum;
    }
}
