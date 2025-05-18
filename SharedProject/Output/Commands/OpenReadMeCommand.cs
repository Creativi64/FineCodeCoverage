using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Readme;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommand))]
    internal sealed class OpenReadMeCommand : CommandBase
    {
        private readonly IReadMeService readMeService;

        protected override int CommandId { get; } = PackageIds.cmdidOpenReadMeCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public OpenReadMeCommand(IReadMeService readMeService)
        {
            this.readMeService = readMeService;
        }

        protected override void Execute(object sender, EventArgs e)
        {
            this.readMeService.ShowReadMe();
        }
    }
}

