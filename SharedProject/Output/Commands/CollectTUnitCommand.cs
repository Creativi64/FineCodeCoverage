using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.MsTestPlatform.TestingPlatform;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICollectTUnitCommand))]
    internal sealed class CollectTUnitCommand : CommandInitializerBase, ICollectTUnitCommand
    {
        private readonly ITUnitCoverage _tUnitCoverage;

        protected override int CommandId { get; } = PackageIds.cmdidCollectTUnitCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public CollectTUnitCommand(ITUnitCoverage tUnitCoverage) => this._tUnitCoverage = tUnitCoverage;

        protected override void Initialized()
        {
            this.Command.Enabled = this._tUnitCoverage.Ready;
            this._tUnitCoverage.ReadyEvent += (_, __) => this.Command.Enabled = this._tUnitCoverage.Ready;
        }

        protected override void Execute(object sender, EventArgs e) => this._tUnitCoverage.CollectCoverage();

        public void SetVisible(bool isVisible) => this.Command.Visible = isVisible;
    }
}
