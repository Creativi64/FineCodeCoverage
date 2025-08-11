using System.Collections.Generic;

namespace FineCodeCoverage.Readme.OptionPagesInfo
{
    internal sealed class CategorizedOptionPropertyInfos
    {
        public CategorizedOptionPropertyInfos(
            string category, IEnumerable<OptionPropertyInfo> optionPropertyInfos)
        {
            Category = category;
            OptionPropertyInfos = optionPropertyInfos;
        }

        public string Category { get; }

        public IEnumerable<OptionPropertyInfo> OptionPropertyInfos { get; }
    }
}
