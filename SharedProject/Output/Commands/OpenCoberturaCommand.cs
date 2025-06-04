using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class OpenCoberturaCommand : CommandInitializerBase, IListener<ReportFilesMessage>, IListener<OutdatedOutputMessage>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IVsOpenFile _vsOpenFile;
        private readonly IFileUtil _fileUtil;
        private string _coberturaFile;

        protected override int CommandId { get; } = PackageIds.cmdidOpenCoberturaCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public OpenCoberturaCommand(IEventAggregator eventAggregator, IVsOpenFile vsOpenFile, IFileUtil fileUtil)
        {
            this._eventAggregator = eventAggregator;
            this._vsOpenFile = vsOpenFile;
            this._fileUtil = fileUtil;
        }

        protected override void Initialized()
        {
            this.Command.Enabled = false;
            _ = this._eventAggregator.AddListener(this);
        }

        protected override void Execute(object sender, EventArgs e)
        {
            if (!this._fileUtil.Exists(this._coberturaFile))
            {
                return;
            }

            this._vsOpenFile.OpenFileInDefaultViewer(this._coberturaFile);
        }

        public void Handle(OutdatedOutputMessage message) => this.Command.Enabled = false;

        public void Handle(ReportFilesMessage message)
        {
            this._coberturaFile = message.CoberturaFile;
            this.Command.Enabled = true;
        }
    }
}