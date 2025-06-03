using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.MsTestPlatform.TestingPlatform;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICollectTUnitCommand))]
    internal sealed class CollectTUnitCommand : CommandInitializerBase, ICollectTUnitCommand
    {
        private readonly ITUnitCoverage tUnitCoverage;

        protected override int CommandId { get; } = PackageIds.cmdidCollectTUnitCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public CollectTUnitCommand(ITUnitCoverage tUnitCoverage) => this.tUnitCoverage = tUnitCoverage;

        protected override void Initialized()
        {
            this.Command.Enabled = this.tUnitCoverage.Ready;
            this.tUnitCoverage.ReadyEvent += (_, __) => this.Command.Enabled = this.tUnitCoverage.Ready;
        }

        protected override void Execute(object sender, EventArgs e) => this.tUnitCoverage.CollectCoverage();

        public void SetVisible(bool isVisible) => this.Command.Visible = isVisible;
    }
}