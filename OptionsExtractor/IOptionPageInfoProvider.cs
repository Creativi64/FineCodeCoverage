using System;
using System.Collections.Generic;

namespace OptionsExtractor
{
    internal interface IOptionPageInfoProvider
    {
        IEnumerable<OptionPageInfo> Provide(Type packageType);
    }
}
