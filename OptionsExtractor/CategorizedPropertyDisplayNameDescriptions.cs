using System.Collections.Generic;

namespace OptionsExtractor
{
    public class CategorizedPropertyDisplayNameDescriptions
    {
        public CategorizedPropertyDisplayNameDescriptions(string category, IEnumerable<PropertyDisplayNameDescription> propertyDisplayNameDescriptions)
        {
            Category = category;
            PropertyDisplayNameDescriptions = propertyDisplayNameDescriptions;
        }
        public string Category { get; }
        public IEnumerable<PropertyDisplayNameDescription> PropertyDisplayNameDescriptions { get; }
    }
}
