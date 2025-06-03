using System;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class ClearUICommand : CommandInitializerBase
    {
        private readonly IUIClearer uiClearer;

        protected override int CommandId { get; } = PackageIds.cmdidClearUICommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public ClearUICommand(IUIClearer uiClearer) => this.uiClearer = uiClearer;

        protected override void Execute(object sender, EventArgs e) => this.uiClearer.ClearUI();
    }
}