using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using System.Collections.Generic;

namespace FineCodeCoverage.Output.Report_Window.ReportTreeItems
{
    internal class TotalTreeItem : ReportTreeItemBase
    {
        public TotalTreeItem(IEnumerable<ReportTreeItemBase> children)
        {
            foreach (var child in children)
            {
                this.CoverableLines += child.CoverableLines;
                this.CoveredLines += child.CoveredLines;
                this.NotCoveredLines += child.NotCoveredLines;
                this.PartialLines += child.PartialLines;

                this.NPathComplexity += child.NPathComplexity;
                this.CrapScore += child.CrapScore;
                this.CyclomaticComplexity += child.CyclomaticComplexity;

                this.BlocksCovered += child.BlocksCovered;
                this.BlocksNotCovered += child.BlocksNotCovered;
                this.TotalBranches += child.TotalBranches;
                this.CoveredBranches += child.CoveredBranches;

            }
            this.Name = "Total";
        }
        public override ImageMoniker ImageMoniker { get; } = KnownMonikers.AutoSum;
    }
}
