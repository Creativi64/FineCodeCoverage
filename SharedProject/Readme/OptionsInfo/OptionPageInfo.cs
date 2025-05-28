using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace FineCodeCoverage.Readme
{
    internal class OptionPageInfo
    {
        public OptionPageInfo(Type optionType, string pageName, List<string> coverageSettingsPropertyNames)
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
                var isCoverageSetting = coverageSettingsPropertyNames.Contains(p.Name);
                return new OptionPropertyInfoWithCategory(displayName, description, category, p.Name, isCoverageSetting);
            }).GroupBy(PropertyCategoryDisplayNameDescription => PropertyCategoryDisplayNameDescription.Category)
            .Select(g => new CategorizedOptionPropertyInfos(g.Key, g));
        }
        public string TypeName { get; }
        public string PageName { get; }
        public IEnumerable<CategorizedOptionPropertyInfos> PropertyCategories { get; }
    }

}
