using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class ResetSettingsCommand : CommandInitializerBase
    {
        private readonly ResetOptionsService resetOptionsService;

        protected override int CommandId { get; } = PackageIds.cmdidResetSettingsCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public ResetSettingsCommand(ResetOptionsService resetOptionsService) => this.resetOptionsService = resetOptionsService;

        protected override void Execute(object sender, EventArgs e) => this.resetOptionsService.Reset();

    }
}

