using System.Linq;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Output
{
    public class CodeElementTreeItem : ReportTreeItemBase
    {
        public CodeElementTreeItem(
            ICodeElement codeElement
        )
        {
            this.Name = codeElement.Name;
            this.FilePath = codeElement.Path;
            this.FileLine = codeElement.StartLine;
            this.ImageMoniker = codeElement.CodeElementType == CodeElementType.Method ? KnownMonikers.Method : KnownMonikers.Property;
            var lineVisitStatuses = codeElement.LineVisitStatuses;
            this.CoverableLines = lineVisitStatuses.Count(lineVisitStatus => lineVisitStatus != LineVisitStatus.NotCoverable);
        }

        public override ImageMoniker ImageMoniker { get; }
        public int FileLine { get; internal set; }
        public string FilePath { get; internal set; }
    }
}
