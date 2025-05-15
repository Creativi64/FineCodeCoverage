using FineCodeCoverage.Options;
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
        public CoverageSettingsReflectionService()
        {
            var interfaces = typeof(CoverageSettings).FindInterfaces((type, _) => type != typeof(ICoverageSettings), null);
            CoverageSettingsPropertyInfos = interfaces.SelectMany(iFace => iFace.GetProperties()).ToList();
        }
        public List<PropertyInfo> CoverageSettingsPropertyInfos { get; }

        public CoverageSettings CreateCoverageSettingsFromAppOptions(AppOptions appOptions)
        {
            var coverageSettings = new CoverageSettings();
            foreach (var property in CoverageSettingsPropertyInfos)
            {
                var value = property.GetValue(appOptions);
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
            return coverageSettings;
        }
    }
}
