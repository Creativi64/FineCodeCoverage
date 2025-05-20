using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace OptionsExtractor
{
    public class OptionPageInfo
    {
        public OptionPageInfo(Type optionType, string pageName)
        {
            TypeName = optionType.Name;
            PageName = pageName;
            PropertyCategories = optionType.GetProperties().Select(p =>
            {
                var displayNameAttribute = p.GetCustomAttribute<DisplayNameAttribute>();
                var displayName = displayNameAttribute == null ? p.Name : displayNameAttribute.DisplayName;

                var descriptionAttribute = p.GetCustomAttribute<DescriptionAttribute>();
                var description = descriptionAttribute == null ? displayName : descriptionAttribute.Description;

                var categoryAttribute = p.GetCustomAttribute<CategoryAttribute>();
                var category = categoryAttribute == null ? "Misc" : categoryAttribute.Category;
                return new PropertyCategoryDisplayNameDescription(displayName, description, category);
            }).GroupBy(PropertyCategoryDisplayNameDescription => PropertyCategoryDisplayNameDescription.Category)
            .Select(g => new CategorizedPropertyDisplayNameDescriptions(g.Key,g));
        }
        public string TypeName { get; }
        public string PageName { get; }
        public IEnumerable<CategorizedPropertyDisplayNameDescriptions> PropertyCategories { get; }
    }
}
