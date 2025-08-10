using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Output.Pane;
using FineCodeCoverage.Vs.Commands.CommandInitializer;

namespace FineCodeCoverage.Vs.Commands.FCCCommands
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class OpenFCCOutputPaneCommand : CommandInitializerBase
    {
        private readonly IShowFCCOutputPane _showFCCOutputPane;

        protected override int CommandId { get; } = PackageIds.cmdidOpenFCCOutputPaneCommand;

        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public OpenFCCOutputPaneCommand(IShowFCCOutputPane showFCCOutputPane) => _showFCCOutputPane = showFCCOutputPane;

        protected override void Execute(object sender, EventArgs e) => _ = _showFCCOutputPane.ShowAsync();
    }
}
