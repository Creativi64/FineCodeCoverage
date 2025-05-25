using System;
using System.Collections.Generic;
using System.Text;

namespace FineCodeCoverage.Readme
{
    internal interface IOptionPageInfoProvider
    {
        IEnumerable<OptionPageInfo> Provide(Type packageType, List<string> coverageSettingsPropertyNames);
    }
}
