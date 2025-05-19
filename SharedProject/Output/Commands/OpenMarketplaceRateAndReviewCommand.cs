using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class OpenMarketplaceRateAndReviewCommand : CommandInitializerBase
    {
        private readonly IOpenFCCVsMarketplace openFCCVsMarketplace;

        protected override int CommandId { get; } = PackageIds.cmdidMarketplaceRateAndReviewCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public OpenMarketplaceRateAndReviewCommand(IOpenFCCVsMarketplace openFCCVsMarketplace) => this.openFCCVsMarketplace = openFCCVsMarketplace;

        protected override void Execute(object sender, EventArgs e) => this.openFCCVsMarketplace.OpenRatingAndReview();
    }
}

