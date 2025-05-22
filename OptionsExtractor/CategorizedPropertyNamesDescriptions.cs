using System.Collections.Generic;

namespace OptionsExtractor
{
    public class CategorizedPropertyNamesDescriptions
    {
        public CategorizedPropertyNamesDescriptions(
            string category, IEnumerable<PropertyNamesDescription> propertyNamesDescriptions)
        {
            Category = category;
            PropertyNamesDescriptions = propertyNamesDescriptions;
        }
        public string Category { get; }
        public IEnumerable<PropertyNamesDescription> PropertyNamesDescriptions { get; }
    }
}
