using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using SharedProject.Core.CoverageToolOutput;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class OpenCoberturaCommand : CommandInitializerBase, IListener<ReportFilesMessage>, IListener<OutdatedOutputMessage>
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IVsOpenFile vsOpenFile;
        private readonly IFileUtil fileUtil;
        private string coberturaFile;

        protected override int CommandId { get; } = PackageIds.cmdidOpenCoberturaCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public OpenCoberturaCommand(IEventAggregator eventAggregator, IVsOpenFile vsOpenFile, IFileUtil fileUtil)
        {
            this.eventAggregator = eventAggregator;
            this.vsOpenFile = vsOpenFile;
            this.fileUtil = fileUtil;
        }

        protected override void Initialized()
        {
            this.Command.Enabled = false;
            this.eventAggregator.AddListener(this);
        }

        protected override void Execute(object sender, EventArgs e)
        {
            if (fileUtil.Exists(coberturaFile))
            {
                this.vsOpenFile.OpenFileInDefaultViewer(coberturaFile);
            }
        }

        public void Handle(OutdatedOutputMessage message)
        {
            Command.Enabled = false;
        }

        public void Handle(ReportFilesMessage message)
        {
            this.coberturaFile = message.CoberturaFile;
            Command.Enabled = true;
        }
    }
}
