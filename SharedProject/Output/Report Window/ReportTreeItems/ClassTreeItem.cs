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
            Name = clss.DisplayName.Split('.').Last();
            IEnumerable<CodeElementTreeItem> codeElements = clss.CodeElements.Select(
                codeElement => new CodeElementTreeItem(codeElement)
                {
                    Parent = this,
                });

            foreach (CodeElementTreeItem codeElement in codeElements)
            {
                ObservableChildren.Add(codeElement);
                CoverableLines += codeElement.CoverableLines;
                CoveredLines += codeElement.CoveredLines;
                NotCoveredLines += codeElement.NotCoveredLines;
                PartialLines += codeElement.PartialLines;
                NPathComplexity += codeElement.NPathComplexity;
                CyclomaticComplexity += codeElement.CyclomaticComplexity;
                CrapScore += codeElement.CrapScore;
                TotalBranches += codeElement.TotalBranches;
                CoveredBranches += codeElement.CoveredBranches;
            }
        }

        public override ImageMoniker ImageMoniker => KnownMonikers.Class;
    }
}
