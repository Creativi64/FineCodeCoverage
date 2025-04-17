using System.Linq;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;
using System.IO;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Core.Utilities.Telemetry;

namespace FineCodeCoverage.Output
{
    internal class SourceFileTreeItem : ReportTreeItemBase
    {
        private readonly string baseName;
        public SourceFileTreeItem(ISourceFile sourceFile)
        {
            sourceFile.HasNewCodeChanged += (_, __) => {
                MainThreadHelper.SwitchAndFileAndForget(
                    FCCFaultEventName.Create<SourceFileTreeItem>("Report"),
                    () => this.SetName(sourceFile.HasNewCode), "sourceFile.HasNewCodeChanged");
            };
            this.baseName = Path.GetFileName(sourceFile.Path);
            this.SetName(sourceFile.HasNewCode);
            this.ImageMoniker = KnownMonikers.TextFile;
            sourceFile.Classes.ToList().ForEach(clss => this.observableChildren.Add(new ClassTreeItem(clss) { Parent = this }));

            this.CoverableLines = this.observableChildren.Sum(c => c.CoverableLines);
            this.NPathComplexity = this.observableChildren.Sum(c => c.NPathComplexity);
            this.CrapScore = this.observableChildren.Sum(c => c.CrapScore);
            this.CyclomaticComplexity = this.observableChildren.Sum(c => c.CyclomaticComplexity);
            this.BlocksCovered = this.observableChildren.Sum(c => c.BlocksCovered);
            this.BlocksNotCovered = this.observableChildren.Sum(c => c.BlocksNotCovered);
        }

        private void SetName(bool hasNewCode)
        {
            var suffix = hasNewCode ? " ***" : "";
            this.Name = $"{this.baseName}{suffix}";
        }
        public override ImageMoniker ImageMoniker { get; }
    }
}
