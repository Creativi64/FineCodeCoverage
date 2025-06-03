using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;

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
            _ = this.eventAggregator.AddListener(this);
        }

        protected override void Execute(object sender, EventArgs e)
        {
            if (this.fileUtil.Exists(this.coberturaFile))
            {
                this.vsOpenFile.OpenFileInDefaultViewer(this.coberturaFile);
            }
        }

        public void Handle(OutdatedOutputMessage message) => this.Command.Enabled = false;

        public void Handle(ReportFilesMessage message)
        {
            this.coberturaFile = message.CoberturaFile;
            this.Command.Enabled = true;
        }
    }
}