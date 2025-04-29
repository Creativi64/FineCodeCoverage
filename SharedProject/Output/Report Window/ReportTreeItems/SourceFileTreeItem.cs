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
        private readonly ISourceFile sourceFile;
        public SourceFileTreeItem(ISourceFile sourceFile)
        {
            this.sourceFile = sourceFile;
            this.Name = Path.GetFileName(sourceFile.Path);
            sourceFile.HasNewCodeChanged += (_, __) => {
                MainThreadHelper.SwitchAndFileAndForget(
                    FCCFaultEventName.Create<SourceFileTreeItem>("Report"),
                    () =>
                    {
                        HasNewCode = sourceFile.HasNewCode;
                    }, "sourceFile.HasNewCodeChanged");
            };
            sourceFile.PathChanged += (_, __) => {
                MainThreadHelper.SwitchAndFileAndForget(
                    FCCFaultEventName.Create<SourceFileTreeItem>("Report"),
                    () => {
                        this.Name = Path.GetFileName(sourceFile.Path); 
                    }, "sourceFile.PathChanged");
            };

            this.ImageMoniker = KnownMonikers.TextFile;
            sourceFile.Classes.ToList().ForEach(clss => this.observableChildren.Add(new ClassTreeItem(clss) { Parent = this }));

            this.CoverableLines = this.observableChildren.Sum(c => c.CoverableLines);
            this.CoveredLines = this.observableChildren.Sum(c => c.CoveredLines);
            this.NPathComplexity = this.observableChildren.Sum(c => c.NPathComplexity);
            this.CrapScore = this.observableChildren.Sum(c => c.CrapScore);
            this.CyclomaticComplexity = this.observableChildren.Sum(c => c.CyclomaticComplexity);
            this.BlocksCovered = this.observableChildren.Sum(c => c.BlocksCovered);
            this.BlocksNotCovered = this.observableChildren.Sum(c => c.BlocksNotCovered);
        }

        private bool hasNewCode;
        public bool HasNewCode
        {
            get => hasNewCode;
            set
            {
                Set(ref hasNewCode, value);
            }
        }
        public override ImageMoniker ImageMoniker { get; }
    }
}
