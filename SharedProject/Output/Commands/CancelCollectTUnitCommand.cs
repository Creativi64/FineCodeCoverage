using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.MsTestPlatform.TestingPlatform;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICancelCollectTUnitCommand))]
    internal sealed class CancelCollectTUnitCommand : CommandInitializerBase, ICancelCollectTUnitCommand
    {
        private readonly ITUnitCoverage _tUnitCoverage;

        protected override int CommandId { get; } = PackageIds.cmdidCancelCollectTUnitCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public CancelCollectTUnitCommand(ITUnitCoverage tUnitCoverage) => this._tUnitCoverage = tUnitCoverage;

        protected override void Execute(object sender, EventArgs e) => this._tUnitCoverage.CollectCoverage();

        public void SetVisible(bool isVisible) => this.Command.Visible = isVisible;
    }
}