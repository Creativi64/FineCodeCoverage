using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

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
                this.LoadSettingsFromStorage();
                return this.allSettings;
            }

            set => this.allSettings = value;
        }

        public ProfileManager()
        {
            this.optionsProviders = MefServiceProvider.GetAll<IProfileOptionsProvider>().ToList();
            this.jsonConvertService = MefServiceProvider.Get<IJsonConvertService>();
        }

        // may be called by VS prior to LoadSettingsFromXML and SaveSettingsToStorage
        public void LoadSettingsFromStorage()
        {
            if (this.allSettings == null)
            {
                this.allSettings = this.optionsProviders.ConvertAll(optionsProvider => optionsProvider.Options);
            }
        }

        private object DeserializeStringArray(string value, PropertyDescriptor _)
            => this.jsonConvertService.DeserializeObject<string[]>(value);

        private object DeserializeFromPropertyDescriptor(string value, PropertyDescriptor propertyDescriptor)
        {
            TypeConverter converter = propertyDescriptor.Converter;
            return converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string))
                ? propertyDescriptor.Converter.ConvertFromInvariantString(value)
                : null;
        }

        public void LoadSettingsFromXml(IVsSettingsReader reader)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            int index = 0;
            foreach (object setting in this.AllSettings)
            {
                IProfileOptionsProvider optionProvider = this.optionsProviders[index];
                optionProvider.Initializing = true;
                foreach (PropertyDescriptor propertyDescriptor in optionProvider.LazyOptionsPropertyDescriptorCollection.Value)
                {
                    Func<string, PropertyDescriptor, object> deserialize = propertyDescriptor.PropertyType == typeof(string[]) ?
                        (Func<string, PropertyDescriptor, object>)this.DeserializeStringArray :
                        this.DeserializeFromPropertyDescriptor;

                    object obj = null;
                    try
                    {
                        if (reader.ReadSettingString(propertyDescriptor.Name, out string pbstrSettingValue) >= 0)
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
        }

        public void ResetSettings()
        {
            // https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.shell.interop.__usersettingsflags
            // https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.shell.interop.ivsusersettings.importsettings
        }

        public void SaveSettingsToStorage()
            => this.optionsProviders.ForEach(optionProvider => optionProvider.SaveSettingsToStorage());

        private string SerializeStringArray(object value, PropertyDescriptor _)
            => this.jsonConvertService.SerializeObject(value);

        private string SerializeFromPropertyDescriptor(object value, PropertyDescriptor propertyDescriptor)
        {
            TypeConverter converter = propertyDescriptor.Converter;
            return converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string))
                ? propertyDescriptor.Converter.ConvertToInvariantString(value)
                : null;
        }

        public void SaveSettingsToXml(IVsSettingsWriter writer)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            int index = 0;
            foreach (object setting in this.AllSettings)
            {
                IProfileOptionsProvider optionProvider = this.optionsProviders[index];
                foreach (PropertyDescriptor propertyDescriptor in optionProvider.LazyOptionsPropertyDescriptorCollection.Value)
                {
                    object propertyValue = propertyDescriptor.GetValue(setting);
                    if (propertyValue != null)
                    {
                        Func<object, PropertyDescriptor, string> serialize = propertyDescriptor.PropertyType == typeof(string[]) ?
                            (Func<object, PropertyDescriptor, string>)this.SerializeStringArray :
                            this.SerializeFromPropertyDescriptor;

                        _ = writer.WriteSettingString(propertyDescriptor.Name, serialize(propertyValue, propertyDescriptor));
                    }
                }

                index++;
            }
        }
    }
}