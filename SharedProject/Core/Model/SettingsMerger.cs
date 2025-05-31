using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Engine.Model
{
    [Export(typeof(ISettingsMerger))]
    internal class SettingsMerger : ISettingsMerger
    {
        private const bool projectSettingsDefaultMerge = false;
        private const bool settingsFileDefaultMerge = false;
        private const string defaultMergeAttributeName = "defaultMerge";
        private const string mergeAttributeName = "merge";
        private readonly ILogger logger;
        private readonly ISettingsMergeLogic settingsMergeLogic = new SettingsMergeLogic();

        private class SettingsElementDefaultMerge
        {
            public XElement SettingsElement { get; set; }
            public bool DefaultMerge { get; set; }
            public bool FromProjectSettings { get; internal set; }
        }

        private List<PropertyInfo> settingsPropertyInfos;

        [ImportingConstructor]
        public SettingsMerger(
            ILogger logger
        ) => this.logger = logger;

        public async Task MergeAsync(
            CoverageSettings coverageSettings,
            List<PropertyInfo> coverageSettingsPropertyInfos,
            List<XElement> settingsFileElements,
            XElement projectSettingsElement)
        {
            this.settingsPropertyInfos = coverageSettingsPropertyInfos;
            await this.MergeAsync(coverageSettings, this.GetElementDefaultMergeStrategies(settingsFileElements, projectSettingsElement));
        }

        private List<SettingsElementDefaultMerge> GetElementDefaultMergeStrategies(List<XElement> settingsFileElements, XElement projectSettingsElement)
        {
            List<SettingsElementDefaultMerge> settingsElementsWithDefaultMergeStrategy =
                settingsFileElements.ConvertAll(e => new SettingsElementDefaultMerge
                {
                    SettingsElement = e,
                    DefaultMerge = settingsFileDefaultMerge,
                    FromProjectSettings = false
                });

            if (projectSettingsElement != null)
            {
                settingsElementsWithDefaultMergeStrategy.Add(
                    new SettingsElementDefaultMerge
                    {
                        SettingsElement = projectSettingsElement,
                        DefaultMerge = projectSettingsDefaultMerge,
                        FromProjectSettings = true
                    }
                );
            }

            return settingsElementsWithDefaultMergeStrategy;
        }

        private async Task MergeAsync(CoverageSettings coverageSettings, List<SettingsElementDefaultMerge> settingsElementsWithDefaultMergeStrategy)
        {
            foreach (PropertyInfo settingsProperty in this.settingsPropertyInfos)
            {
                await this.MergeAsync(coverageSettings, settingsProperty, settingsElementsWithDefaultMergeStrategy);
            }
        }

        private async Task MergeAsync(
            CoverageSettings coverageSettings,
            PropertyInfo settingPropertyInfo,
            List<SettingsElementDefaultMerge> settingsElementsWithDefaultMergeStrategy
        )
        {
            bool canMerge = this.settingsMergeLogic.CanMerge(settingPropertyInfo.PropertyType);
            if (canMerge)
            {
                await this.MergeOrOverwriteAsync(coverageSettings, settingPropertyInfo, settingsElementsWithDefaultMergeStrategy);
            }
            else
            {
                await this.OverwriteAsync(coverageSettings, settingPropertyInfo, settingsElementsWithDefaultMergeStrategy);
            }
        }

        private async Task MergeOrOverwriteAsync(
            CoverageSettings coverageSettings,
            PropertyInfo settingPropertyInfo,
            List<SettingsElementDefaultMerge> settingsElementsWithDefaultMergeStrategy
        )
        {
            foreach (SettingsElementDefaultMerge settingsElementWithDefaultMerge in settingsElementsWithDefaultMergeStrategy)
            {
                XElement settingsElement = settingsElementWithDefaultMerge.SettingsElement;
                bool defaultMerge = this.GetDefaultMerge(settingsElementWithDefaultMerge.DefaultMerge, settingsElement);
                XElement propertyElement = this.GetPropertyElement(settingsElement, settingPropertyInfo.Name);
                if (propertyElement != null)
                {
                    await this.ApplyPropertyElementAsync(
                        coverageSettings,
                        propertyElement,
                        settingPropertyInfo,
                        defaultMerge,
                        settingsElementWithDefaultMerge.FromProjectSettings
                    );
                }
            }
        }

        private async Task ApplyPropertyElementAsync(
            CoverageSettings coverageSettings,
            XElement propertyElement,
            PropertyInfo settingPropertyInfo,
            bool defaultMerge,
            bool fromProjectSettings)
        {
            bool merge = this.GetMerge(defaultMerge, propertyElement);
            if (merge)
            {
                await this.MergeAsync(coverageSettings, settingPropertyInfo, propertyElement, fromProjectSettings);
            }
            else
            {
                await this.OverwriteAsync(coverageSettings, settingPropertyInfo, propertyElement, fromProjectSettings);
            }
        }

        private async Task MergeAsync(CoverageSettings coverageSettings, PropertyInfo settingPropertyInfo, XElement propertyElement, bool fromProjectSettings)
        {
            object value = await this.TryGetValueFromXmlAsync(propertyElement, settingPropertyInfo, fromProjectSettings);
            if (value != null)
            {
                object currentValue = settingPropertyInfo.GetValue(coverageSettings);
                object merged = currentValue == null ?
                    value :
                    this.settingsMergeLogic.Merge(settingPropertyInfo.PropertyType, currentValue, value);
                settingPropertyInfo.SetValue(coverageSettings, merged);
            }
        }

        private bool GetMerge(bool defaultMerge, XElement propertyElement)
        {
            XAttribute mergeAttribute = propertyElement.Attribute(mergeAttributeName);
            return mergeAttribute == null ?
                defaultMerge :
                string.Equals(mergeAttribute.Value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private bool GetDefaultMerge(bool defaultDefaultMerge, XElement root)
        {
            XAttribute defaultMergeAttribute = root.Attribute(defaultMergeAttributeName);
            return defaultMergeAttribute == null
                ? defaultDefaultMerge
                : string.Equals(defaultMergeAttribute.Value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private async Task OverwriteAsync(CoverageSettings coverageSettings, PropertyInfo settingPropertyInfo, IEnumerable<SettingsElementDefaultMerge> settingsElementsDefaultMerge)
        {
            foreach (SettingsElementDefaultMerge settingsElementDefaultMerge in settingsElementsDefaultMerge)
            {
                XElement propertyElement = this.GetPropertyElement(settingsElementDefaultMerge.SettingsElement, settingPropertyInfo.Name);
                if (propertyElement != null)
                {
                    await this.OverwriteAsync(coverageSettings, settingPropertyInfo, propertyElement, settingsElementDefaultMerge.FromProjectSettings);
                }
            }
        }

        private async Task OverwriteAsync(CoverageSettings coverageSettings, PropertyInfo settingPropertyInfo, XElement propertyElement, bool fromProjectSettings)
        {
            object value = await this.TryGetValueFromXmlAsync(propertyElement, settingPropertyInfo, fromProjectSettings);
            if (value != null)
            {
                settingPropertyInfo.SetValue(coverageSettings, value);
            }
        }

        private XElement GetPropertyElement(XElement settingsElement, string propertyName)
            => settingsElement.Descendants()
            .FirstOrDefault(x => x.Name.LocalName.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

        private async Task<object> TryGetValueFromXmlAsync(XElement settingsElement, PropertyInfo property, bool fromProjectSettings)
        {
            try
            {
                return this.GetValueFromXml(settingsElement, property.PropertyType, property.Name);
            }
            catch (Exception exception)
            {
                string from = fromProjectSettings ? "project settings" : "settings file";
                await this.logger.LogAsync($"Failed to get '{property.Name}' setting from {from}", exception.ToString());
            }

            return null;
        }

        internal object GetValueFromXml(XElement xproperty, Type type, string name)
        {

            if (xproperty == null)
            {
                return null;
            }

            string strValue = xproperty.Value;

            string[] strValueArr = strValue.Split('\n', '\r').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();

            if (this.TypeMatch(type, typeof(string)))
            {
                string value = strValueArr.FirstOrDefault();
                return value ?? "";
            }
            else if (this.TypeMatch(type, typeof(string[])))
            {
                return strValueArr;
            }

            else if (this.TypeMatch(type, typeof(bool), typeof(bool?)))
            {
                if (bool.TryParse(strValueArr.FirstOrDefault(), out bool value))
                {
                    return value;
                }
            }
            else if (this.TypeMatch(type, typeof(bool[]), typeof(bool?[])))
            {
                IEnumerable<bool> arr = strValueArr.Where(x => bool.TryParse(x, out bool _)).Select(x => bool.Parse(x));
                if (arr.Any())
                {
                    return arr;
                }
            }

            else if (this.TypeMatch(type, typeof(int), typeof(int?)))
            {
                if (int.TryParse(strValueArr.FirstOrDefault(), out int value))
                {
                    return value;
                }
            }
            else if (this.TypeMatch(type, typeof(int[]), typeof(int?[])))
            {
                IEnumerable<int> arr = strValueArr.Where(x => int.TryParse(x, out int _)).Select(x => int.Parse(x));
                if (arr.Any())
                {
                    return arr;
                }
            }

            else if (this.TypeMatch(type, typeof(short), typeof(short?)))
            {
                if (short.TryParse(strValueArr.FirstOrDefault(), out short vaue))
                {
                    return vaue;
                }
            }
            else if (this.TypeMatch(type, typeof(short[]), typeof(short?[])))
            {
                IEnumerable<short> arr = strValueArr.Where(x => short.TryParse(x, out short _)).Select(x => short.Parse(x));
                if (arr.Any())
                {
                    return arr;
                }
            }

            else if (this.TypeMatch(type, typeof(long), typeof(long?)))
            {
                if (long.TryParse(strValueArr.FirstOrDefault(), out long value))
                {
                    return value;
                }
            }
            else if (this.TypeMatch(type, typeof(long[]), typeof(long?[])))
            {
                IEnumerable<long> arr = strValueArr.Where(x => long.TryParse(x, out long _)).Select(x => long.Parse(x));
                if (arr.Any())
                {
                    return arr;
                }
            }

            else if (this.TypeMatch(type, typeof(decimal), typeof(decimal?)))
            {
                if (decimal.TryParse(strValueArr.FirstOrDefault(), out decimal value))
                {
                    return value;
                }
            }
            else if (this.TypeMatch(type, typeof(decimal[]), typeof(decimal?[])))
            {
                IEnumerable<decimal> arr = strValueArr.Where(x => decimal.TryParse(x, out decimal _)).Select(x => decimal.Parse(x));
                if (arr.Any())
                {
                    return arr;
                }
            }

            else if (this.TypeMatch(type, typeof(double), typeof(double?)))
            {
                if (double.TryParse(strValueArr.FirstOrDefault(), out double value))
                {
                    return value;
                }
            }
            else if (this.TypeMatch(type, typeof(double[]), typeof(double?[])))
            {
                IEnumerable<double> arr = strValueArr.Where(x => double.TryParse(x, out double _)).Select(x => double.Parse(x));
                if (arr.Any())
                {
                    return arr;
                }
            }

            else if (this.TypeMatch(type, typeof(float), typeof(float?)))
            {
                if (float.TryParse(strValueArr.FirstOrDefault(), out float value))
                {
                    return value;
                }
            }
            else if (this.TypeMatch(type, typeof(float[]), typeof(float?[])))
            {
                IEnumerable<float> arr = strValueArr.Where(x => float.TryParse(x, out float _)).Select(x => float.Parse(x));
                if (arr.Any())
                {
                    return arr;
                }
            }

            else if (this.TypeMatch(type, typeof(char), typeof(char?)))
            {
                if (char.TryParse(strValueArr.FirstOrDefault(), out char value))
                {
                    return value;
                }
            }
            else if (this.TypeMatch(type, typeof(char[]), typeof(char?[])))
            {
                IEnumerable<char> arr = strValueArr.Where(x => char.TryParse(x, out char _)).Select(x => char.Parse(x));
                if (arr.Any())
                {
                    return arr;
                }
            }
            else
            {
                return type.IsEnum
                    ? Enum.Parse(type, strValueArr.FirstOrDefault(), true)
                    : throw new UnexpectedSettingsTypeException($"Unexpected settings type '{type.Name}' for setting {name} in settings merger GetValueFromXml");

            }

            return null;
        }

        private bool TypeMatch(Type type, params Type[] otherTypes) => (otherTypes ?? new Type[0]).Any(ot => type == ot);
    }
}
