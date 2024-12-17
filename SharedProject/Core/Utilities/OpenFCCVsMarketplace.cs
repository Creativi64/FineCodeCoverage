using System.ComponentModel.Composition;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Core.Utilities
{

    [Export(typeof(IOpenFCCVsMarketplace))]
    internal class OpenFCCVsMarketplace : IOpenFCCVsMarketplace
    {
        private const string rootPath = "https://marketplace.visualstudio.com/items?itemName=FortuneNgwenya.FineCodeCoverage";
        private readonly string ratingAndReviewPath;
        private readonly IProcess process;

        [ImportingConstructor]
        public OpenFCCVsMarketplace(
            IProcess process,
            IVsVersion vsVersion
        )
        {
            this.ratingAndReviewPath = rootPath;
            if (vsVersion.Is2022)
            {
                this.ratingAndReviewPath += "2022";
            }

            this.ratingAndReviewPath += "&ssr=false#review-details";
            this.process = process;
        }
        public void OpenRatingAndReview() => this.process.Start(this.ratingAndReviewPath);
    }
}
