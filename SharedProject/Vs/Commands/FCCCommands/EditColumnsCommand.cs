using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Output;
using FineCodeCoverage.Vs.Commands.CommandInitializer;

namespace FineCodeCoverage.Vs.Commands.FCCCommands
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class EditColumnsCommand : CommandInitializerBase
    {
        private readonly IReportColumnsService _reportColumnsService;

        protected override int CommandId { get; } = PackageIds.cmdidEditColumnsCommand;

        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public EditColumnsCommand(IReportColumnsService reportColumnsService)
            => _reportColumnsService = reportColumnsService;

        protected override void Execute(object sender, EventArgs e) => _reportColumnsService.ManageColumns();
    }
}
