using System.Collections.Generic;

namespace FineCodeCoverage.Readme
{
    internal class CategorizedOptionPropertyInfos
    {
        public CategorizedOptionPropertyInfos(
            string category, IEnumerable<OptionPropertyInfo> optionPropertyInfos)
        {
            this.Category = category;
            this.OptionPropertyInfos = optionPropertyInfos;
        }
        public string Category { get; }
        public IEnumerable<OptionPropertyInfo> OptionPropertyInfos { get; }
    }
}