using System.Collections.Generic;

namespace FineCodeCoverage.Readme.Options.OptionPagesInfo
{
    public interface IFCCOptionPageInfoProvider
    {
        IEnumerable<OptionPageInfo> Provide();
    }
}
