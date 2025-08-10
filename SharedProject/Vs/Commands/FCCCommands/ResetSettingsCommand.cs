using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Vs.Commands.CommandInitializer;

namespace FineCodeCoverage.Vs.Commands.FCCCommands
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class ResetSettingsCommand : CommandInitializerBase
    {
        private readonly ResetOptionsService _resetOptionsService;

        protected override int CommandId { get; } = PackageIds.cmdidResetSettingsCommand;

        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public ResetSettingsCommand(ResetOptionsService resetOptionsService) => _resetOptionsService = resetOptionsService;

        protected override void Execute(object sender, EventArgs e) => _resetOptionsService.Reset();
    }
}
