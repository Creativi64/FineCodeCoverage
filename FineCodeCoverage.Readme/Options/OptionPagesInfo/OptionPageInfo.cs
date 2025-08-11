using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace FineCodeCoverage.Readme.OptionPagesInfo
{
    internal sealed class OptionPageInfo
    {
        public OptionPageInfo(Type optionType, string pageName, List<string> coverageSettingsPropertyNames)
        {
            TypeName = optionType.Name;
            PageName = pageName;
            PropertyCategories = optionType.GetProperties().Select(p =>
            {
                DisplayNameAttribute displayNameAttribute = p.GetCustomAttribute<DisplayNameAttribute>();
                string displayName = displayNameAttribute == null ? p.Name : displayNameAttribute.DisplayName;

                DescriptionAttribute descriptionAttribute = p.GetCustomAttribute<DescriptionAttribute>();
                string description = descriptionAttribute == null ? displayName : descriptionAttribute.Description;

                CategoryAttribute categoryAttribute = p.GetCustomAttribute<CategoryAttribute>();
                string category = categoryAttribute == null ? "Misc" : categoryAttribute.Category;
                bool isCoverageSetting = coverageSettingsPropertyNames.Contains(p.Name);
                return new OptionPropertyInfoWithCategory(displayName, description, category, p.Name, isCoverageSetting);
            }).GroupBy(propertyCategoryDisplayNameDescription => propertyCategoryDisplayNameDescription.Category)
            .Select(g => new CategorizedOptionPropertyInfos(g.Key, g));
        }

        public string TypeName { get; }

        public string PageName { get; }

        public IEnumerable<CategorizedOptionPropertyInfos> PropertyCategories { get; }
    }
}
