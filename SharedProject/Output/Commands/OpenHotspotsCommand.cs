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
            this._eventAggregator = eventAggregator;
            this._hotspotsService = hotspotsService;
            this._vsOpenFile = vsOpenFile;
        }

        protected override void Initialized()
        {
            this.Command.Enabled = false;
            _ = this._eventAggregator.AddListener(this);
        }

        protected override void Execute(object sender, EventArgs e)
        {
            if (this._reportResult == null)
            {
                return;
            }

            this._hotspotsService.WriteHotspotsToXml(this._reportResult.Assemblies, this._hotspotsPath);
            this._vsOpenFile.OpenFileInCodeEditor(this._hotspotsPath);
        }

        public void Handle(OutdatedOutputMessage message) => this.Command.Enabled = false;

        public void Handle(ReportFilesMessage message)
        {
            this._reportResult = message.ReportResult;
            this._hotspotsPath = Path.Combine(Path.GetDirectoryName(message.CoberturaFile), "hotspots.xml");
            this.Command.Enabled = true;
        }
    }
}