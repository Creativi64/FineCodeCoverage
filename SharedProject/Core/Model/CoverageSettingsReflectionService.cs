using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;

namespace FineCodeCoverage.Engine.Model
{
    [Export(typeof(ICoverageSettingsReflectionService))]
    internal class CoverageSettingsReflectionService : ICoverageSettingsReflectionService
    {
        private class OptionInfo
        {
            public OptionInfo(object option, Type type)
            {
                Option = option;
                Type = type;
            }
            public object Option { get; }
            Type Type { get; }
        }

        private class OptionPropertyInfos
        {
            public OptionPropertyInfos(object option, List<PropertyInfo> propertyInfos)
            {
                Option = option;
                PropertyInfos = propertyInfos;
            }
            public object Option { get; }
            public List<PropertyInfo> PropertyInfos { get; }
        }

        private readonly Dictionary<Type, PropertyInfo[]> coverageSettingsPropertyInfosLookup;
        private Dictionary<Type, List<PropertyInfo>> optionsPropertyLookup;
        public CoverageSettingsReflectionService()
        {
            var interfaces = typeof(CoverageSettings).FindInterfaces((type, _) => type != typeof(ICoverageSettings), null);
            coverageSettingsPropertyInfosLookup = interfaces.ToDictionary(iFace => iFace,iFace => iFace.GetProperties());
            CoverageSettingsPropertyInfos = coverageSettingsPropertyInfosLookup.Values.SelectMany(v => v).ToList();
        }

        private IEnumerable<OptionPropertyInfos> GetOptionPropertyInfos(IEnumerable<OptionInfo> optionInfos)
        {
            if (optionsPropertyLookup == null)
            {
                this.optionsPropertyLookup = new Dictionary<Type, List<PropertyInfo>>();
                foreach(var optionInfo in optionInfos)
                {
                    var optionPropertyInfos = new List<PropertyInfo>();
                    var optionType = optionInfo.Option.GetType();
                    foreach(var optionInterface in optionType.GetInterfaces())
                    {
                        if (coverageSettingsPropertyInfosLookup.TryGetValue(optionInterface, out var optionInterfacePropertyInfos))
                        {
                            optionPropertyInfos.AddRange(optionInterfacePropertyInfos);
                        }
                    }
                    optionsPropertyLookup[optionType] = optionPropertyInfos;
                }
            }
            return optionInfos.Select(o => new OptionPropertyInfos(o.Option, optionsPropertyLookup[o.Option.GetType()]));
        }

        public List<PropertyInfo> CoverageSettingsPropertyInfos { get; }

        public CoverageSettings CreateCoverageSettingsFromOptions(IEnumerable<object> coverageSettingsOptions)
        {
            var optionInfos = coverageSettingsOptions.Select(option => new OptionInfo(option, option.GetType()));
            var optionsPropertyInfos = GetOptionPropertyInfos(optionInfos);

            var coverageSettings = new CoverageSettings();
            foreach(var optionPropertyInfos in optionsPropertyInfos)
            {
                SetFromOptions(coverageSettings, optionPropertyInfos);
            }
            return coverageSettings;
        }

        private void SetFromOptions(CoverageSettings coverageSettings, OptionPropertyInfos optionPropertyInfos)
        {
            foreach (var property in optionPropertyInfos.PropertyInfos)
            {
                var value = property.GetValue(optionPropertyInfos.Option);
                if (!property.PropertyType.IsValueType && property.PropertyType != typeof(string))
                {
                    if (property.PropertyType == typeof(string[]))
                    {
                        if (value is string[] valueArray)
                        {
                            value = valueArray.Clone();
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"nameof(CoverageSettings) has property {property.Name} with unsupported type {property.PropertyType}");
                    }
                }
                property.SetValue(coverageSettings, value);
            }
        }
    }
}
