using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Collection.CoverageToolOutput;
using FineCodeCoverage.Collection.Messages;
using FineCodeCoverage.Core.Utilities;

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
            _eventAggregator = eventAggregator;
            _vsOpenFile = vsOpenFile;
            _fileUtil = fileUtil;
        }

        protected override void Initialized()
        {
            Command.Enabled = false;
            _ = _eventAggregator.AddListener(this);
        }

        protected override void Execute(object sender, EventArgs e)
        {
            if (!_fileUtil.Exists(_coberturaFile))
            {
                return;
            }

            _vsOpenFile.OpenFileInDefaultViewer(_coberturaFile);
        }

        public void Handle(OutdatedOutputMessage message) => Command.Enabled = false;

        public void Handle(ReportFilesMessage message)
        {
            _coberturaFile = message.CoberturaFile;
            Command.Enabled = true;
        }
    }
}
