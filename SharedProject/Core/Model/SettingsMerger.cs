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
        private readonly SettingsMergeLogic settingsMergeLogic = new SettingsMergeLogic();
        private static readonly Dictionary<Type, ISettingsXmlParser> parsers = new Dictionary<Type, ISettingsXmlParser>()
        {
            { typeof(bool), new SettingsXmlParser<bool,bool?>(bool.TryParse)},
            { typeof(int), new SettingsXmlParser<int,int?>(int.TryParse)},
            { typeof(short), new SettingsXmlParser<short,short?>(short.TryParse)},
            { typeof(long), new SettingsXmlParser<long,long?>(long.TryParse)},
            { typeof(decimal), new SettingsXmlParser<decimal,decimal?>(decimal.TryParse)},
            { typeof(double), new SettingsXmlParser<double,double?>(double.TryParse)},
            { typeof(float), new SettingsXmlParser<float,float?>(float.TryParse)},
            { typeof(char), new SettingsXmlParser<char,char?>(char.TryParse)}
        };

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
            await this.MergeAsync(
                coverageSettings,
                GetElementDefaultMergeStrategies(settingsFileElements, projectSettingsElement)
            );
        }

        private static List<SettingsElementDefaultMerge> GetElementDefaultMergeStrategies(
            List<XElement> settingsFileElements, XElement projectSettingsElement)
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
                bool defaultMerge = GetDefaultMerge(settingsElementWithDefaultMerge.DefaultMerge, settingsElement);
                XElement propertyElement = GetPropertyElement(settingsElement, settingPropertyInfo.Name);
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
            bool merge = GetMerge(defaultMerge, propertyElement);
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

        private static bool GetMerge(bool defaultMerge, XElement propertyElement)
        {
            XAttribute mergeAttribute = propertyElement.Attribute(mergeAttributeName);
            return mergeAttribute == null ?
                defaultMerge :
                string.Equals(mergeAttribute.Value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static bool GetDefaultMerge(bool defaultDefaultMerge, XElement root)
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
                XElement propertyElement = GetPropertyElement(settingsElementDefaultMerge.SettingsElement, settingPropertyInfo.Name);
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

        private static XElement GetPropertyElement(XElement settingsElement, string propertyName)
            => settingsElement.Descendants()
            .FirstOrDefault(x => x.Name.LocalName.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

        private async Task<object> TryGetValueFromXmlAsync(XElement settingsElement, PropertyInfo property, bool fromProjectSettings)
        {
            try
            {
                return GetValueFromXml(settingsElement, property.PropertyType, property.Name);
            }
            catch (Exception exception)
            {
                string from = fromProjectSettings ? "project settings" : "settings file";
                await this.logger.LogAsync($"Failed to get '{property.Name}' setting from {from}", exception.ToString());
            }

            return null;
        }

        internal static object GetValueFromXml(XElement xproperty, Type type, string name)
        {
            if (xproperty == null)
            {
                return null;
            }

            string strValue = xproperty.Value;

            string[] strValueArr = strValue.Split('\n', '\r').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();

            return GetValueFromXml(strValueArr, type, name);
        }

        private static object GetValueFromXml(string[] strValueArr, Type type, string name)
        {
            if (type == typeof(string))
            {
                string value = strValueArr.FirstOrDefault();
                return value ?? "";
            }

            return type.IsEnum
                ? Enum.Parse(type, strValueArr.FirstOrDefault(), true)
                : type == typeof(string[]) ?
                    strValueArr :
                    GetValueFromParsers(strValueArr, type, name);
        }

        private static (Type lookupType, bool isNullable) GetLookupTypeInfo(Type type)
        {
            Type underlying = Nullable.GetUnderlyingType(type);
            return underlying != null ? ((Type lookupType, bool isNullable))(underlying, true) :
                ((Type lookupType, bool isNullable))(type, false);
        }

        private static object GetValueFromParsers(string[] strValueArr, Type type, string name)
        {
            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                (Type lookupType, bool isNullable) = GetLookupTypeInfo(elementType);
                if (parsers.TryGetValue(lookupType, out ISettingsXmlParser parser))
                {
                    return parser.ParseArray(strValueArr, isNullable);
                }
            }
            else
            {
                (Type lookupType, bool _) = GetLookupTypeInfo(type);
                if (parsers.TryGetValue(lookupType, out ISettingsXmlParser parser))
                {
                    return parser.Parse(strValueArr.FirstOrDefault());
                }
            }

            throw new UnexpectedSettingsTypeException($"Unexpected settings type '{type.Name}' for setting {name} in settings merger GetValueFromXml");
        }
    }
}
