using System.Collections.Generic;

namespace FineCodeCoverage.Readme
{
    public interface IFCCOptionPageInfoProvider
    {
        IEnumerable<OptionPageInfo> Provide();
    }
}
