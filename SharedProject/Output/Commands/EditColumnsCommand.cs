using System;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class EditColumnsCommand : CommandInitializerBase
    {
        private readonly IReportColumnsService reportColumnsService;

        protected override int CommandId { get; } = PackageIds.cmdidEditColumnsCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public EditColumnsCommand(IReportColumnsService reportColumnsService) => this.reportColumnsService = reportColumnsService;

        protected override void Execute(object sender, EventArgs e) => this.reportColumnsService.ManageColumns();
    }
}
