using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Funding;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommand))]
    internal sealed class OpenFundingCommand : CommandBase
    {
        private readonly IFundingService fundingService;

        protected override int CommandId { get; } = PackageIds.cmdidOpenFundingCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public OpenFundingCommand(IFundingService fundingService)
        {
            this.fundingService = fundingService;
        }

        protected override void Execute(object sender, EventArgs e) => this.fundingService.Execute();
    }
}

