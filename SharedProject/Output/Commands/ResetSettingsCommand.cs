using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class ResetSettingsCommand : CommandInitializerBase
    {
        private readonly ResetOptionsService _resetOptionsService;

        protected override int CommandId { get; } = PackageIds.cmdidResetSettingsCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public ResetSettingsCommand(ResetOptionsService resetOptionsService) => this._resetOptionsService = resetOptionsService;

        protected override void Execute(object sender, EventArgs e) => this._resetOptionsService.Reset();

    }
}
