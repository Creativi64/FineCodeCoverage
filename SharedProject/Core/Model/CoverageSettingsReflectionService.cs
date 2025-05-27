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
            public OptionInfo(object option)
            {
                Option = option;
                Type = option.GetType();
                InterfaceTypes = Type.GetInterfaces();
            }
            public object Option { get; }
            public Type Type { get; }
            public Type[] InterfaceTypes { get; }
        }

        private class OptionCoverageSettingsInterfacesPropertyInfos
        {
            public OptionCoverageSettingsInterfacesPropertyInfos(object option, List<PropertyInfo> propertyInfos)
            {
                Option = option;
                PropertyInfos = propertyInfos;
            }
            public object Option { get; }
            public List<PropertyInfo> PropertyInfos { get; }
        }

        private readonly Dictionary<Type, PropertyInfo[]> coverageSettingsInterfacesPropertyInfosLookup;
        private Dictionary<Type, List<PropertyInfo>> optionsTypeCoverageSettingsInterfacesPropertyLookup;
        public CoverageSettingsReflectionService()
        {
            var interfaces = typeof(CoverageSettings).FindInterfaces((type, _) => type != typeof(ICoverageSettings), null);
            coverageSettingsInterfacesPropertyInfosLookup = interfaces.ToDictionary(iFace => iFace, iFace => iFace.GetProperties());
            CoverageSettingsPropertyInfos = coverageSettingsInterfacesPropertyInfosLookup.Values.SelectMany(v => v).ToList();
        }

        private IEnumerable<OptionCoverageSettingsInterfacesPropertyInfos> GetOptionCoverageSettingsInterfacesPropertyInfos(
           IEnumerable<object> coverageSettingsOptions
        )
        {
            var optionInfos = coverageSettingsOptions.Select(option => new OptionInfo(option));
            return GetOptionCoverageSettingsInterfacesPropertyInfos(optionInfos);
        }

        private IEnumerable<OptionCoverageSettingsInterfacesPropertyInfos> GetOptionCoverageSettingsInterfacesPropertyInfos(
            IEnumerable<OptionInfo> optionInfos
        )
        {
            if (optionsTypeCoverageSettingsInterfacesPropertyLookup == null)
            {
                CreateOptionsTypeCoverageSettingsInterfacesPropertyLookup(optionInfos);
            }
            return optionInfos.Select(o => new OptionCoverageSettingsInterfacesPropertyInfos(
                o.Option, optionsTypeCoverageSettingsInterfacesPropertyLookup[o.Type]));
        }

        private void CreateOptionsTypeCoverageSettingsInterfacesPropertyLookup(IEnumerable<OptionInfo> optionInfos)
        {
            this.optionsTypeCoverageSettingsInterfacesPropertyLookup = new Dictionary<Type, List<PropertyInfo>>();
            foreach (var optionInfo in optionInfos)
            {
                var optionInterfacesPropertyInfos = new List<PropertyInfo>();
                foreach (var optionInterfaceType in optionInfo.InterfaceTypes)
                {
                    if (coverageSettingsInterfacesPropertyInfosLookup.TryGetValue(optionInterfaceType, out var optionInterfacePropertyInfos))
                    {
                        optionInterfacesPropertyInfos.AddRange(optionInterfacePropertyInfos);
                    }
                }
                optionsTypeCoverageSettingsInterfacesPropertyLookup[optionInfo.Type] = optionInterfacesPropertyInfos;
            }
        }

        public List<PropertyInfo> CoverageSettingsPropertyInfos { get; }

        public CoverageSettings CreateCoverageSettingsFromOptions(IEnumerable<object> coverageSettingsOptions)
        {
            var coverageSettings = new CoverageSettings();

            foreach (var optionCoverageSettingsInterfacesPropertyInfos in GetOptionCoverageSettingsInterfacesPropertyInfos(coverageSettingsOptions))
            {
                SetCovergeSettingsFromOptions(coverageSettings, optionCoverageSettingsInterfacesPropertyInfos);
            }
            return coverageSettings;
        }

        private void SetCovergeSettingsFromOptions(CoverageSettings coverageSettings, OptionCoverageSettingsInterfacesPropertyInfos optionPropertyInfos)
        {
            foreach (var property in optionPropertyInfos.PropertyInfos)
            {
                var value = GetOptionValueCloneArrays(optionPropertyInfos.Option, property);
                property.SetValue(coverageSettings, value);
            }
        }

        private object GetOptionValueCloneArrays(object option, PropertyInfo property)
        {
            var value = property.GetValue(option);
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
            return value;
        }
    }
}
