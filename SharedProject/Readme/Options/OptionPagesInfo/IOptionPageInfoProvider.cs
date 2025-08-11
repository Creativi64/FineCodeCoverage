using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Readme.OptionPagesInfo
{
    internal interface IOptionPageInfoProvider
    {
        IEnumerable<OptionPageInfo> Provide(Type packageType, List<string> coverageSettingsPropertyNames);
    }
}
