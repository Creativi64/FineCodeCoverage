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

        private readonly Dictionary<Type, PropertyInfo[]> _coverageSettingsInterfacesPropertyInfosLookup;
        private Dictionary<Type, List<PropertyInfo>> _optionsTypeCoverageSettingsInterfacesPropertyLookup;

        public CoverageSettingsReflectionService()
        {
            Type[] interfaces = typeof(CoverageSettings).FindInterfaces((type, _) => type != typeof(ICoverageSettings), null);
            _coverageSettingsInterfacesPropertyInfosLookup = interfaces.ToDictionary(iFace => iFace, iFace => iFace.GetProperties());
            CoverageSettingsPropertyInfos = _coverageSettingsInterfacesPropertyInfosLookup.Values.SelectMany(v => v).ToList();
        }

        private IEnumerable<OptionCoverageSettingsInterfacesPropertyInfos> GetOptionCoverageSettingsInterfacesPropertyInfos(
           IEnumerable<object> coverageSettingsOptions
        )
        {
            IEnumerable<OptionInfo> optionInfos = coverageSettingsOptions.Select(option => new OptionInfo(option));
            return GetOptionCoverageSettingsInterfacesPropertyInfos(optionInfos);
        }

        private IEnumerable<OptionCoverageSettingsInterfacesPropertyInfos> GetOptionCoverageSettingsInterfacesPropertyInfos(
            IEnumerable<OptionInfo> optionInfos
        )
        {
            if (_optionsTypeCoverageSettingsInterfacesPropertyLookup == null)
            {
                CreateOptionsTypeCoverageSettingsInterfacesPropertyLookup(optionInfos);
            }

            return optionInfos.Select(o => new OptionCoverageSettingsInterfacesPropertyInfos(
                o.Option, _optionsTypeCoverageSettingsInterfacesPropertyLookup[o.Type]));
        }

        private void CreateOptionsTypeCoverageSettingsInterfacesPropertyLookup(IEnumerable<OptionInfo> optionInfos)
        {
            _optionsTypeCoverageSettingsInterfacesPropertyLookup = new Dictionary<Type, List<PropertyInfo>>();
            foreach (OptionInfo optionInfo in optionInfos)
            {
                var optionInterfacesPropertyInfos = new List<PropertyInfo>();
                foreach (Type optionInterfaceType in optionInfo.InterfaceTypes)
                {
                    if (_coverageSettingsInterfacesPropertyInfosLookup.TryGetValue(optionInterfaceType, out PropertyInfo[] optionInterfacePropertyInfos))
                    {
                        optionInterfacesPropertyInfos.AddRange(optionInterfacePropertyInfos);
                    }
                }

                _optionsTypeCoverageSettingsInterfacesPropertyLookup[optionInfo.Type] = optionInterfacesPropertyInfos;
            }
        }

        public List<PropertyInfo> CoverageSettingsPropertyInfos { get; }

        public CoverageSettings CreateCoverageSettingsFromOptions(IEnumerable<object> coverageSettingsOptions)
        {
            var coverageSettings = new CoverageSettings();

            foreach (OptionCoverageSettingsInterfacesPropertyInfos optionCoverageSettingsInterfacesPropertyInfos in GetOptionCoverageSettingsInterfacesPropertyInfos(coverageSettingsOptions))
            {
                SetCovergeSettingsFromOptions(coverageSettings, optionCoverageSettingsInterfacesPropertyInfos);
            }

            return coverageSettings;
        }

        private static void SetCovergeSettingsFromOptions(CoverageSettings coverageSettings, OptionCoverageSettingsInterfacesPropertyInfos optionPropertyInfos)
        {
            foreach (PropertyInfo property in optionPropertyInfos.PropertyInfos)
            {
                object value = GetOptionValueCloneArrays(optionPropertyInfos.Option, property);
                property.SetValue(coverageSettings, value);
            }
        }

        private static object GetOptionValueCloneArrays(object option, PropertyInfo property)
        {
            object value = property.GetValue(option);
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
