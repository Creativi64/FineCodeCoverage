using System;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Funding
{
    public static class FundingMonikers
    {
        private static Guid Guid { get; } = new Guid("c07b27ae-9756-4632-a6e6-1578f6dfbedf");
        public static ImageMoniker BuyMeACoffee => new ImageMoniker { Guid = Guid, Id = 2 };
        public static ImageMoniker Kofi => new ImageMoniker { Guid = Guid, Id = 3 };
        public static ImageMoniker Liberapay => new ImageMoniker { Guid = Guid, Id = 4 };
        public static ImageMoniker Paypal => new ImageMoniker { Guid = Guid, Id = 5 };
    }
}
