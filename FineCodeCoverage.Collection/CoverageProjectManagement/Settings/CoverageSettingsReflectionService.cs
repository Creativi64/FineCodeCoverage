using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.Settings
{
    [Export(typeof(ICoverageSettingsReflectionService))]
    internal sealed class CoverageSettingsReflectionService : ICoverageSettingsReflectionService
    {
        private sealed class OptionInfo
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

        private sealed class OptionCoverageSettingsInterfacesPropertyInfos
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
        private readonly object _optionsTypeLookupLock = new object();
        private Dictionary<Type, List<PropertyInfo>> _optionsTypeCoverageSettingsInterfacesPropertyLookup;

        public CoverageSettingsReflectionService()
        {
            Type[] interfaces = typeof(CoverageSettings).FindInterfaces((type, _) => type != typeof(ICoverageSettings), null);
            _coverageSettingsInterfacesPropertyInfosLookup = interfaces.ToDictionary(iFace => iFace, iFace => iFace.GetProperties());
            CoverageSettingsPropertyInfos = _coverageSettingsInterfacesPropertyInfosLookup.Values.SelectMany(v => v).ToList();
        }

        private IEnumerable<OptionCoverageSettingsInterfacesPropertyInfos> GetOptionCoverageSettingsInterfacesPropertyInfos(
           IEnumerable<object> coverageSettingsOptions)
        {
            List<OptionInfo> optionInfos = coverageSettingsOptions.Select(option => new OptionInfo(option)).ToList();
            return GetOptionCoverageSettingsInterfacesPropertyInfos(optionInfos);
        }

        private IEnumerable<OptionCoverageSettingsInterfacesPropertyInfos> GetOptionCoverageSettingsInterfacesPropertyInfos(
            IReadOnlyList<OptionInfo> optionInfos)
        {
            // This is a shared MEF singleton.  The VS project system invokes the per-project
            // IProjectGlobalPropertiesProvider concurrently across test projects, so the lazy
            // initialization below must be synchronized - otherwise two threads mutate the same
            // Dictionary at once and corrupt its internal arrays, surfacing as a NullReferenceException
            // inside Dictionary.Insert.
            Dictionary<Type, List<PropertyInfo>> lookup = GetOptionsTypeCoverageSettingsInterfacesPropertyLookup(optionInfos);

            return optionInfos.Select(o => new OptionCoverageSettingsInterfacesPropertyInfos(
                o.Option, lookup[o.Type]));
        }

        private Dictionary<Type, List<PropertyInfo>> GetOptionsTypeCoverageSettingsInterfacesPropertyLookup(
            IReadOnlyList<OptionInfo> optionInfos)
        {
            lock (_optionsTypeLookupLock)
            {
                if (_optionsTypeCoverageSettingsInterfacesPropertyLookup == null)
                {
                    _optionsTypeCoverageSettingsInterfacesPropertyLookup =
                        CreateOptionsTypeCoverageSettingsInterfacesPropertyLookup(optionInfos);
                }

                return _optionsTypeCoverageSettingsInterfacesPropertyLookup;
            }
        }

        private Dictionary<Type, List<PropertyInfo>> CreateOptionsTypeCoverageSettingsInterfacesPropertyLookup(IEnumerable<OptionInfo> optionInfos)
        {
            var lookup = new Dictionary<Type, List<PropertyInfo>>();
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

                lookup[optionInfo.Type] = optionInterfacesPropertyInfos;
            }

            return lookup;
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
