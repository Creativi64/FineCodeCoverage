using System.ComponentModel.Composition;

namespace FineCodeCoverage.Core.Utilities
{

    [Export(typeof(IOpenFCCVsMarketplace))]
    internal class OpenFCCVsMarketplace : IOpenFCCVsMarketplace
    {
        private const string RootPath = "https://marketplace.visualstudio.com/items?itemName=FortuneNgwenya.FineCodeCoverage";
        private readonly string _ratingAndReviewPath;
        private readonly IProcess _process;

        [ImportingConstructor]
        public OpenFCCVsMarketplace(
            IProcess process,
            IVsVersion vsVersion
        )
        {
            _ratingAndReviewPath = RootPath;
            if (vsVersion.Is2022)
            {
                _ratingAndReviewPath += "2022";
            }

            _ratingAndReviewPath += "&ssr=false#review-details";
            _process = process;
        }

        public void OpenRatingAndReview() => _process.Start(_ratingAndReviewPath);
    }
}
