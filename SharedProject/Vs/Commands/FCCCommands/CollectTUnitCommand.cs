using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Collection.Runners;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICollectTUnitCommand))]
    internal sealed class CollectTUnitCommand : CommandInitializerBase, ICollectTUnitCommand
    {
        private readonly ITUnitCoverage _tUnitCoverage;

        protected override int CommandId { get; } = PackageIds.cmdidCollectTUnitCommand;

        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public CollectTUnitCommand(ITUnitCoverage tUnitCoverage) => _tUnitCoverage = tUnitCoverage;

        protected override void Initialized()
        {
            Command.Enabled = _tUnitCoverage.Ready;
            _tUnitCoverage.ReadyEvent += (_, __) => Command.Enabled = _tUnitCoverage.Ready;
        }

        protected override void Execute(object sender, EventArgs e) => _tUnitCoverage.CollectCoverage();

        public void SetVisible(bool isVisible) => Command.Visible = isVisible;
    }
}
