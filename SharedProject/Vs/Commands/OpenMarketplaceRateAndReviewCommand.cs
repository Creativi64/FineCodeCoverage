using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class OpenMarketplaceRateAndReviewCommand : CommandInitializerBase
    {
        private readonly IOpenFCCVsMarketplace _openFCCVsMarketplace;

        protected override int CommandId { get; } = PackageIds.cmdidMarketplaceRateAndReviewCommand;

        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public OpenMarketplaceRateAndReviewCommand(IOpenFCCVsMarketplace openFCCVsMarketplace) => _openFCCVsMarketplace = openFCCVsMarketplace;

        protected override void Execute(object sender, EventArgs e) => _openFCCVsMarketplace.OpenRatingAndReview();
    }
}
