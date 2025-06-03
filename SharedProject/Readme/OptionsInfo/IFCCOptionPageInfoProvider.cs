using System.Collections.Generic;

namespace FineCodeCoverage.Readme
{
    internal interface IFCCOptionPageInfoProvider
    {
        IEnumerable<OptionPageInfo> Provide();
    }
}