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
        private readonly IEventAggregator eventAggregator;
        private readonly IHotspotsService hotspotsService;
        private readonly IVsOpenFile vsOpenFile;
        private IReportResult reportResult;
        private string hotspotsPath;

        protected override int CommandId { get; } = PackageIds.cmdidOpenHotspotsCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public OpenHotspotsCommand(IEventAggregator eventAggregator, IHotspotsService hotspotsService, IVsOpenFile vsOpenFile)
        {
            this.eventAggregator = eventAggregator;
            this.hotspotsService = hotspotsService;
            this.vsOpenFile = vsOpenFile;
        }

        protected override void Initialized()
        {
            this.Command.Enabled = false;
            _ = this.eventAggregator.AddListener(this);
        }

        protected override void Execute(object sender, EventArgs e)
        {
            if (this.reportResult != null)
            {
                this.hotspotsService.WriteHotspotsToXml(this.reportResult.Assemblies, this.hotspotsPath);
                this.vsOpenFile.OpenFileInCodeEditor(this.hotspotsPath);
            }
        }

        public void Handle(OutdatedOutputMessage message) => this.Command.Enabled = false;

        public void Handle(ReportFilesMessage message)
        {
            this.reportResult = message.ReportResult;
            this.hotspotsPath = Path.Combine(Path.GetDirectoryName(message.CoberturaFile), "hotspots.xml");
            this.Command.Enabled = true;
        }
    }
}
