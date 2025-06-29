using System.Collections.Generic;

namespace FineCodeCoverage.Readme.OptionPagesInfo
{
    internal interface IFCCOptionPageInfoProvider
    {
        IEnumerable<OptionPageInfo> Provide();
    }
}
