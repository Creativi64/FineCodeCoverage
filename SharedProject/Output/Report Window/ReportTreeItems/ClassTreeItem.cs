using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Engine.ReportGenerator;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Output
{
    public class ClassTreeItem : ReportTreeItemBase
    {
        public ClassTreeItem(IClass clss)
        {
            this.Name = clss.DisplayName.Split('.').Last();
            IEnumerable<CodeElementTreeItem> codeElements = clss.CodeElements.Select(
                codeElement => new CodeElementTreeItem(codeElement)
                {
                    Parent = this
                });

            foreach (CodeElementTreeItem codeElement in codeElements)
            {
                this.observableChildren.Add(codeElement);
                this.CoverableLines += codeElement.CoverableLines;
                this.CoveredLines += codeElement.CoveredLines;
                this.NotCoveredLines += codeElement.NotCoveredLines;
                this.PartialLines += codeElement.PartialLines;
                this.NPathComplexity += codeElement.NPathComplexity;
                this.CyclomaticComplexity += codeElement.CyclomaticComplexity;
                this.CrapScore += codeElement.CrapScore;
                this.TotalBranches += codeElement.TotalBranches;
                this.CoveredBranches += codeElement.CoveredBranches;
            }
        }

        public override ImageMoniker ImageMoniker => KnownMonikers.Class;
    }
}