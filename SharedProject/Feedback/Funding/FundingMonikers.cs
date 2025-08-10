using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Feedback.Funding
{
    public static class FundingMonikers
    {
        public static ImageMoniker BuyMeACoffee => new ImageMoniker { Guid = PackageGuids.guidMonikers, Id = 2 };

        public static ImageMoniker Kofi => new ImageMoniker { Guid = PackageGuids.guidMonikers, Id = 3 };

        public static ImageMoniker Liberapay => new ImageMoniker { Guid = PackageGuids.guidMonikers, Id = 4 };

        public static ImageMoniker Paypal => new ImageMoniker { Guid = PackageGuids.guidMonikers, Id = 5 };
    }
}
