using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Funding;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class OpenFundingCommand : CommandInitializerBase
    {
        private readonly IFundingService fundingService;

        protected override int CommandId { get; } = PackageIds.cmdidOpenFundingCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public OpenFundingCommand(IFundingService fundingService) => this.fundingService = fundingService;

        protected override void Execute(object sender, EventArgs e) => this.fundingService.Execute();
    }
}