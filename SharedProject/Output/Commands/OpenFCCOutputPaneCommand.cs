using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Output.Pane;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommand))]
    internal sealed class OpenFCCOutputPaneCommand : CommandBase
    {
        private readonly IShowFCCOutputPane showFCCOutputPane;

        protected override int CommandId { get; } = PackageIds.cmdidOpenFCCOutputPaneCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public OpenFCCOutputPaneCommand(IShowFCCOutputPane showFCCOutputPane)
        {
            this.showFCCOutputPane = showFCCOutputPane;
        }

        protected override void Execute(object sender, EventArgs e) => _ = this.showFCCOutputPane.ShowAsync();
    }
}

