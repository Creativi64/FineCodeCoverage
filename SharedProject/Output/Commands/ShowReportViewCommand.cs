using System;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class ShowReportViewCommand : CommandInitializerBase
    {
        private readonly IReportViewService _reportViewService;
        protected override int CommandId { get; } = PackageIds.cmdidShowReportViewCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public ShowReportViewCommand(IReportViewService reportViewService) => this._reportViewService = reportViewService;

        protected override void Execute(object sender, EventArgs e) => this._reportViewService.Show();
    }
}