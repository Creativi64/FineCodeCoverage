using FineCodeCoverage.Output;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FineCodeCoverage.Engine.Model
{
    internal interface ISettingsMergeLogic
    {
        bool CanMerge(Type type);
        object Merge(Type type,object first, object second);
    }

    public class SettingsMergeLogic : ISettingsMergeLogic
    {
        private interface ITypeMerger
        {
            object Merge(object first, object second);
        }

        private abstract class TypeMerger<T> : ITypeMerger
        {
            public abstract T Merge(T first, T second);

            public object Merge(object first, object second)
            {
                return Merge((T)first, (T)second);
            }
        }

        private class StringArrayMerger : TypeMerger<string[]>
        {
            public override string[] Merge(string[] first, string[] second)
            {
                return first.Concat(second).ToArray();
            }
        }

        private readonly Dictionary<Type, ITypeMerger> typeMergers;

        public SettingsMergeLogic()
        {
            typeMergers = new Dictionary<Type, ITypeMerger>
            {
                { typeof(string[]),new StringArrayMerger()}
            };
        }

        public bool CanMerge(Type type)
        {
            return typeMergers.ContainsKey(type);
        }

        public object Merge(Type type,object first, object second)
        {
            return typeMergers[type].Merge(first, second);
        }
    }

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
        )
        {
            this.logger = logger;
        }

        public async Task MergeAsync(
            CoverageSettings coverageSettings,
            List<PropertyInfo> coverageSettingsPropertyInfos,
            List<XElement> settingsFileElements,
            XElement projectSettingsElement)
        {
            settingsPropertyInfos = coverageSettingsPropertyInfos;
            await MergeAsync(coverageSettings, GetElementDefaultMergeStrategies(settingsFileElements,projectSettingsElement));
        }

        private List<SettingsElementDefaultMerge> GetElementDefaultMergeStrategies(List<XElement> settingsFileElements, XElement projectSettingsElement)
        {
            var settingsElementsWithDefaultMergeStrategy =
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
            foreach (var settingsProperty in settingsPropertyInfos)
            {
                await MergeAsync(coverageSettings, settingsProperty, settingsElementsWithDefaultMergeStrategy);
            }
        }

        private async Task MergeAsync(
            CoverageSettings coverageSettings,
            PropertyInfo settingPropertyInfo,
            List<SettingsElementDefaultMerge> settingsElementsWithDefaultMergeStrategy
        )
        {
            var canMerge = settingsMergeLogic.CanMerge(settingPropertyInfo.PropertyType);
            if (canMerge)
            {
                await MergeOrOverwriteAsync(coverageSettings, settingPropertyInfo, settingsElementsWithDefaultMergeStrategy);
            }
            else
            {
                await OverwriteAsync(coverageSettings, settingPropertyInfo, settingsElementsWithDefaultMergeStrategy);
            }
        }

        private async Task MergeOrOverwriteAsync(
            CoverageSettings coverageSettings,
            PropertyInfo settingPropertyInfo,
            List<SettingsElementDefaultMerge> settingsElementsWithDefaultMergeStrategy
        )
        {
            foreach (var settingsElementWithDefaultMerge in settingsElementsWithDefaultMergeStrategy)
            {
                var settingsElement = settingsElementWithDefaultMerge.SettingsElement;
                var defaultMerge = GetDefaultMerge(settingsElementWithDefaultMerge.DefaultMerge, settingsElement);
                var propertyElement = GetPropertyElement(settingsElement, settingPropertyInfo.Name);
                if (propertyElement != null)
                {
                    await ApplyPropertyElementAsync(
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
            var merge = GetMerge(defaultMerge, propertyElement);
            if (merge)
            {
                await MergeAsync(coverageSettings, settingPropertyInfo, propertyElement, fromProjectSettings);
            }
            else
            {
                await OverwriteAsync(coverageSettings, settingPropertyInfo, propertyElement, fromProjectSettings);
            }
        }

        private async Task MergeAsync(CoverageSettings coverageSettings, PropertyInfo settingPropertyInfo, XElement propertyElement,bool fromProjectSettings)
        {
            var value = await TryGetValueFromXmlAsync(propertyElement, settingPropertyInfo,fromProjectSettings);
            if (value != null)
            {
                var currentValue = settingPropertyInfo.GetValue(coverageSettings);
                object merged;
                if (currentValue == null)
                {
                    merged = value;
                }
                else
                {
                    merged = settingsMergeLogic.Merge(settingPropertyInfo.PropertyType, currentValue, value);
                }

                settingPropertyInfo.SetValue(coverageSettings, merged);
            }
        }

        private bool GetMerge(bool defaultMerge, XElement propertyElement)
        {
            var mergeAttribute = propertyElement.Attribute(mergeAttributeName);
            if (mergeAttribute == null)
            {
                return defaultMerge;
            }
            return string.Equals(mergeAttribute.Value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private bool GetDefaultMerge(bool defaultDefaultMerge, XElement root)
        {
            var defaultMergeAttribute = root.Attribute(defaultMergeAttributeName);
            if (defaultMergeAttribute == null)
            {
                return defaultDefaultMerge;
            }
            return string.Equals(defaultMergeAttribute.Value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private async Task OverwriteAsync(CoverageSettings coverageSettings, PropertyInfo settingPropertyInfo, IEnumerable<SettingsElementDefaultMerge> settingsElementsDefaultMerge)
        {
            foreach (var settingsElementDefaultMerge in settingsElementsDefaultMerge)
            {
                var propertyElement = GetPropertyElement(settingsElementDefaultMerge.SettingsElement, settingPropertyInfo.Name);
                if (propertyElement != null)
                {
                    await OverwriteAsync(coverageSettings, settingPropertyInfo, propertyElement, settingsElementDefaultMerge.FromProjectSettings);
                }
            }
        }

        private async Task OverwriteAsync(CoverageSettings coverageSettings, PropertyInfo settingPropertyInfo, XElement propertyElement,bool fromProjectSettings)
        {
            var value = await TryGetValueFromXmlAsync(propertyElement, settingPropertyInfo,fromProjectSettings);
            if (value != null)
            {
                settingPropertyInfo.SetValue(coverageSettings, value);
            }
        }

        private XElement GetPropertyElement(XElement settingsElement, string propertyName)
        {
            return settingsElement.Descendants().FirstOrDefault(x => x.Name.LocalName.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<object> TryGetValueFromXmlAsync(XElement settingsElement, PropertyInfo property,bool fromProjectSettings)
        {
            try
            {
                return GetValueFromXml(settingsElement, property.PropertyType, property.Name);
            }
            catch (Exception exception)
            {
                var from = fromProjectSettings ? "project settings" : "settings file";
                await logger.LogAsync($"Failed to get '{property.Name}' setting from {from}", exception.ToString());
            }
            return null;
        }

        internal object GetValueFromXml(XElement xproperty, Type type, string name)
        {

            if (xproperty == null)
            {
                return null;
            }

            var strValue = xproperty.Value;

            var strValueArr = strValue.Split('\n', '\r').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();

            if (TypeMatch(type, typeof(string)))
            {
                var value = strValueArr.FirstOrDefault();
                return value ?? "";
            }
            else if (TypeMatch(type, typeof(string[])))
            {
                return strValueArr;
            }

            else if (TypeMatch(type, typeof(bool), typeof(bool?)))
            {
                if (bool.TryParse(strValueArr.FirstOrDefault(), out bool value))
                {
                    return value;
                }
            }
            else if (TypeMatch(type, typeof(bool[]), typeof(bool?[])))
            {
                var arr = strValueArr.Where(x => bool.TryParse(x, out var _)).Select(x => bool.Parse(x));
                if (arr.Any())
                {
                    return arr;
                }
            }

            else if (TypeMatch(type, typeof(int), typeof(int?)))
            {
                if (int.TryParse(strValueArr.FirstOrDefault(), out var value))
                {
                    return value;
                }
            }
            else if (TypeMatch(type, typeof(int[]), typeof(int?[])))
            {
                var arr = strValueArr.Where(x => int.TryParse(x, out var _)).Select(x => int.Parse(x));
                if (arr.Any())
                {
                    return arr;
                }
            }

            else if (TypeMatch(type, typeof(short), typeof(short?)))
            {
                if (short.TryParse(strValueArr.FirstOrDefault(), out var vaue))
                {
                    return vaue;
                }
            }
            else if (TypeMatch(type, typeof(short[]), typeof(short?[])))
            {
                var arr = strValueArr.Where(x => short.TryParse(x, out var _)).Select(x => short.Parse(x));
                if (arr.Any())
                {
                    return arr;
                }
            }

            else if (TypeMatch(type, typeof(long), typeof(long?)))
            {
                if (long.TryParse(strValueArr.FirstOrDefault(), out var value))
                {
                    return value;
                }
            }
            else if (TypeMatch(type, typeof(long[]), typeof(long?[])))
            {
                var arr = strValueArr.Where(x => long.TryParse(x, out var _)).Select(x => long.Parse(x));
                if (arr.Any())
                {
                    return arr;
                }
            }

            else if (TypeMatch(type, typeof(decimal), typeof(decimal?)))
            {
                if (decimal.TryParse(strValueArr.FirstOrDefault(), out var value))
                {
                    return value;
                }
            }
            else if (TypeMatch(type, typeof(decimal[]), typeof(decimal?[])))
            {
                var arr = strValueArr.Where(x => decimal.TryParse(x, out var _)).Select(x => decimal.Parse(x));
                if (arr.Any())
                {
                    return arr;
                }
            }

            else if (TypeMatch(type, typeof(double), typeof(double?)))
            {
                if (double.TryParse(strValueArr.FirstOrDefault(), out var value))
                {
                    return value;
                }
            }
            else if (TypeMatch(type, typeof(double[]), typeof(double?[])))
            {
                var arr = strValueArr.Where(x => double.TryParse(x, out var _)).Select(x => double.Parse(x));
                if (arr.Any())
                {
                    return arr;
                }
            }

            else if (TypeMatch(type, typeof(float), typeof(float?)))
            {
                if (float.TryParse(strValueArr.FirstOrDefault(), out var value))
                {
                    return value;
                }
            }
            else if (TypeMatch(type, typeof(float[]), typeof(float?[])))
            {
                var arr = strValueArr.Where(x => float.TryParse(x, out var _)).Select(x => float.Parse(x));
                if (arr.Any())
                {
                    return arr;
                }
            }

            else if (TypeMatch(type, typeof(char), typeof(char?)))
            {
                if (char.TryParse(strValueArr.FirstOrDefault(), out var value))
                {
                    return value;
                }
            }
            else if (TypeMatch(type, typeof(char[]), typeof(char?[])))
            {
                var arr = strValueArr.Where(x => char.TryParse(x, out var _)).Select(x => char.Parse(x));
                if (arr.Any())
                {
                    return arr;
                }
            }
            else if (type.IsEnum)
            {
                return Enum.Parse(type, strValueArr.FirstOrDefault(), true);

            }

            else
            {
                throw new Exception($"Unexpected settings type '{type.Name}' for setting {name} in settings merger GetValueFromXml");
            }
            return null;

        }

        private bool TypeMatch(Type type, params Type[] otherTypes)
        {
            return (otherTypes ?? new Type[0]).Any(ot => type == ot);
        }
    }
}
