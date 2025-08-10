using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Collection.Runners;
using FineCodeCoverage.Vs.Commands.CommandInitializer;

namespace FineCodeCoverage.Vs.Commands.FCCCommands
{
    [Export(typeof(ICancelCollectTUnitCommand))]
    internal sealed class CancelCollectTUnitCommand : CommandInitializerBase, ICancelCollectTUnitCommand
    {
        private readonly ITUnitCoverage _tUnitCoverage;

        protected override int CommandId { get; } = PackageIds.cmdidCancelCollectTUnitCommand;

        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public CancelCollectTUnitCommand(ITUnitCoverage tUnitCoverage) => _tUnitCoverage = tUnitCoverage;

        protected override void Execute(object sender, EventArgs e) => _tUnitCoverage.CollectCoverage();

        public void SetVisible(bool isVisible) => Command.Visible = isVisible;
    }
}
