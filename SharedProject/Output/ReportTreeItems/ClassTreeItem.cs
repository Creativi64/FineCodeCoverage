using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;
using System.Linq;
using System.Collections.Generic;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Output
{
    public class ClassTreeItem : ReportTreeItemBase
    {
        public ClassTreeItem(IClass clss)
        {
            this.Name = clss.DisplayName.Split('.').Last();
            IEnumerable<CodeElementTreeItem> codeElements = clss.CodeElements.Select(codeElement =>
            {
                return new CodeElementTreeItem(codeElement)
                {
                    Parent = this
                };
            });

            foreach (CodeElementTreeItem codeElement in codeElements)
            {
                this.observableChildren.Add(codeElement);
                this.CoverableLines += codeElement.CoverableLines;
            }
        }

        public override ImageMoniker ImageMoniker => KnownMonikers.Class;
    }
}
