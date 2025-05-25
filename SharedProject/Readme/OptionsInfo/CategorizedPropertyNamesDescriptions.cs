using System.Collections.Generic;

namespace FineCodeCoverage.Readme
{
    public class CategorizedPropertyNamesDescriptions
    {
        public CategorizedPropertyNamesDescriptions(
            string category, IEnumerable<OptionPropertyInfo> propertyNamesDescriptions)
        {
            Category = category;
            PropertyNamesDescriptions = propertyNamesDescriptions;
        }
        public string Category { get; }
        public IEnumerable<OptionPropertyInfo> PropertyNamesDescriptions { get; }
    }
}
