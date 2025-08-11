using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Vs.Commands.CommandInitializer;
using FineCodeCoverage.VSAbstractions.Readme;

namespace FineCodeCoverage.Vs.Commands.FCCCommands
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class OpenReadMeCommand : CommandInitializerBase
    {
        private readonly IShowReadMeService _readMeService;

        protected override int CommandId { get; } = PackageIds.cmdidOpenReadMeCommand;

        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public OpenReadMeCommand(IShowReadMeService readMeService) => _readMeService = readMeService;

        protected override void Execute(object sender, EventArgs e) => _readMeService.Show();
    }
}
