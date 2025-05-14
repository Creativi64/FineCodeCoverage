using FineCodeCoverage.Core.Utilities;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System;

namespace FineCodeCoverage.Options
{
   internal class ProfileManager : Component, IProfileManager
    {
        private readonly List<IProfileOptionsProvider> optionsProviders;
        private readonly IJsonConvertService jsonConvertService;
        private List<object> allSettings;
        private List<object> AllSettings
        {
            get
            {
                if (allSettings == null)
                {
                    LoadSettingsFromStorage();
                }
                return allSettings;
            }
            set
            {
                allSettings = value;
            }
        }


        public ProfileManager()
        {
            this.optionsProviders = MefServiceProvider.GetAll<IProfileOptionsProvider>().ToList();
            this.jsonConvertService = MefServiceProvider.Get<IJsonConvertService>();
        }

        // may be called by VS prior to LoadSettingsFromXML and SaveSettingsToStorage
        public void LoadSettingsFromStorage()
        {
            this.allSettings = this.optionsProviders.ConvertAll(optionsProvider => optionsProvider.LoadSettingsFromStorage());
        }

        private object DeserializeStringArray(string value,PropertyDescriptor _)
        {
            return this.jsonConvertService.DeserializeObject<string[]>(value);
        }

        private object DeserializeFromPropertyDescriptor(string value, PropertyDescriptor propertyDescriptor)
        {
            TypeConverter converter = propertyDescriptor.Converter;
            if (converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string)))
            {
                return propertyDescriptor.Converter.ConvertFromInvariantString(value);
            }
            return null;
        }

        public void LoadSettingsFromXml(IVsSettingsReader reader)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var index = 0;
            foreach (var setting in AllSettings)
            {
                var optionProvider = optionsProviders[index];
                optionProvider.Initializing = true;
                foreach (PropertyDescriptor propertyDescriptor in optionProvider.LazyOptionsPropertyDescriptorCollection.Value)
                {
                    Func<string, PropertyDescriptor, object> deserialize = propertyDescriptor.PropertyType == typeof(string[]) ?
                        (Func<string, PropertyDescriptor, object>)DeserializeStringArray :
                        DeserializeFromPropertyDescriptor;

                    object obj = null;
                    try
                    {
                        if (reader.ReadSettingString(propertyDescriptor.Name, out var pbstrSettingValue) >= 0)
                        {
                            if (pbstrSettingValue != null)
                                obj = deserialize(pbstrSettingValue, propertyDescriptor);
                        }
                    }
                    catch { }
                    if (obj != null)
                        propertyDescriptor.SetValue(setting, obj);
                }
                optionProvider.Initializing = false;
                index++;
            }
            AllSettings = null;
        }

        public void ResetSettings()
        {

        }

        public void SaveSettingsToStorage()
        {
            optionsProviders.ForEach(optionProvider => optionProvider.SaveSettingsToStorage());
        }

        private string SerializeStringArray(object value,PropertyDescriptor _)
        {
            return this.jsonConvertService.SerializeObject(value);
        }

        private string SerializeFromPropertyDescriptor(object value, PropertyDescriptor propertyDescriptor)
        {
            TypeConverter converter = propertyDescriptor.Converter;
            if (converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string)))
            {
                return propertyDescriptor.Converter.ConvertToInvariantString(value);
            }
            return null;
        }

        public void SaveSettingsToXml(IVsSettingsWriter writer)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var index = 0;
            foreach (var setting in AllSettings)
            {
                var optionProvider = optionsProviders[index];
                foreach (PropertyDescriptor propertyDescriptor in optionProvider.LazyOptionsPropertyDescriptorCollection.Value)
                {
                    var propertyValue = propertyDescriptor.GetValue(setting);
                    if (propertyValue != null)
                    {
                        Func<object, PropertyDescriptor, string> serialize = propertyDescriptor.PropertyType == typeof(string[]) ?
                            (Func<object, PropertyDescriptor, string>)SerializeStringArray :
                            SerializeFromPropertyDescriptor;

                        writer.WriteSettingString(propertyDescriptor.Name, serialize(propertyValue, propertyDescriptor));
                    }
                }
                index++;
            }
            AllSettings = null;
        }
    }

}
