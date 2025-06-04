using System;
using System.ComponentModel.Composition;
using System.IO;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.ReportGenerator;
using FineCodeCoverage.ReportGeneration;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class OpenHotspotsCommand : CommandInitializerBase, IListener<ReportFilesMessage>, IListener<OutdatedOutputMessage>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IHotspotsService _hotspotsService;
        private readonly IVsOpenFile _vsOpenFile;
        private IReportResult _reportResult;
        private string _hotspotsPath;

        protected override int CommandId { get; } = PackageIds.cmdidOpenHotspotsCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public OpenHotspotsCommand(IEventAggregator eventAggregator, IHotspotsService hotspotsService, IVsOpenFile vsOpenFile)
        {
            _eventAggregator = eventAggregator;
            _hotspotsService = hotspotsService;
            _vsOpenFile = vsOpenFile;
        }

        protected override void Initialized()
        {
            Command.Enabled = false;
            _ = _eventAggregator.AddListener(this);
        }

        protected override void Execute(object sender, EventArgs e)
        {
            if (_reportResult == null)
            {
                return;
            }

            _hotspotsService.WriteHotspotsToXml(_reportResult.Assemblies, _hotspotsPath);
            _vsOpenFile.OpenFileInCodeEditor(_hotspotsPath);
        }

        public void Handle(OutdatedOutputMessage message) => Command.Enabled = false;

        public void Handle(ReportFilesMessage message)
        {
            _reportResult = message.ReportResult;
            _hotspotsPath = Path.Combine(Path.GetDirectoryName(message.CoberturaFile), "hotspots.xml");
            Command.Enabled = true;
        }
    }
}
