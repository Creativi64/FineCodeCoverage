using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Options
{
    interface IOptionsStorageProvider<TOptions>
    {
        void SaveSettingsToStorage(TOptions appOptions);
        void LoadSettingsFromStorage(TOptions instance);
    }
    interface IOptionsProvider
    {
        object LoadSettingsFromStorage();
        void SaveSettingToStorage(object settings);
        Lazy<PropertyDescriptorCollection> LazyOptionsPropertyDescriptorCollection { get; }
    }

    internal interface ITypeDescriptorService
    {
        void SetPropertyValue(PropertyDescriptor propertyDescriptor, object instance, object value);
        object GetPropertyValue(PropertyDescriptor propertyDescriptor, object instance);
        PropertyDescriptorCollection GetAllProperties(Type type);
    }

    [Export(typeof(ITypeDescriptorService))]
    internal class TypeDescriptorService : ITypeDescriptorService
    {
        public PropertyDescriptorCollection GetAllProperties(Type type)
        {
            if (type.IsInterface)
            {
                return TypeDescriptorHelper.GetAllProperties(type);
            }
            return TypeDescriptor.GetProperties(type);
        }

        public object GetPropertyValue(PropertyDescriptor propertyDescriptor, object instance)
        {
            return propertyDescriptor.GetValue(instance);
        }

        public void SetPropertyValue(PropertyDescriptor propertyDescriptor, object instance, object value)
        {
            propertyDescriptor.SetValue(instance, value);
        }
    }

    public static class TypeDescriptorHelper
    {
        public static PropertyDescriptorCollection GetAllProperties(Type interfaceType)
        {
            if (!interfaceType.IsInterface)
                throw new ArgumentException("Only interfaces are supported", nameof(interfaceType));

            var interfaces = GetAllInterfaces(interfaceType);
            interfaces.Add(interfaceType); // include the main interface itself

            var allProps = new List<PropertyDescriptor>();
            foreach (var iface in interfaces)
            {
                allProps.AddRange(TypeDescriptor.GetProperties(iface).Cast<PropertyDescriptor>());
            }

            // Optionally remove duplicates based on property name
            return new PropertyDescriptorCollection(
                allProps.GroupBy(p => p.Name).Select(g => g.First()).ToArray(), true);
        }

        private static HashSet<Type> GetAllInterfaces(Type type)
        {
            var result = new HashSet<Type>();
            var stack = new Stack<Type>(type.GetInterfaces());

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (result.Add(current))
                {
                    foreach (var sub in current.GetInterfaces())
                        stack.Push(sub);
                }
            }

            return result;
        }
    }


    // to consider adding the DialogPage type and loading immediately
    internal abstract class OptionsProviderBase<TOptions,TTempOption> : IOptionsProvider, IOptionsStorageProvider<TOptions> where TTempOption:TOptions, new()
    {
        private readonly ILogger logger;
        private readonly IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider;
        private readonly IJsonConvertService jsonConvertService;
        private readonly ITypeDescriptorService typeDescriptorService;
        public Lazy<PropertyDescriptorCollection> LazyOptionsPropertyDescriptorCollection { get; }

        public event Action<TOptions> OptionsChanged;
        protected abstract TOptions DefaultOptions { get; }

        protected OptionsProviderBase(
            ILogger logger,
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
            IJsonConvertService jsonConvertService,
            ITypeDescriptorService typeDescriptorService
        )
        {
            this.logger = logger;
            this.writableUserSettingsStoreProvider = writableUserSettingsStoreProvider;
            this.jsonConvertService = jsonConvertService;
            this.typeDescriptorService = typeDescriptorService;
            LazyOptionsPropertyDescriptorCollection = new Lazy<PropertyDescriptorCollection>(() => typeDescriptorService.GetAllProperties(typeof(TOptions)));
        }

        public void RaiseOptionsChanged(TOptions options)
        {
            OptionsChanged?.Invoke(options);
        }

        public TOptions Get()
        {
            var option = new TTempOption();
            LoadSettingsFromStorage(option);
            return option;
        }

        private WritableSettingsStore EnsureStore()
        {
            var settingsStore = writableUserSettingsStoreProvider.LazySettingsStore.GetValue();
            if (!settingsStore.CollectionExists(Vsix.Code))
            {
                settingsStore.CreateCollection(Vsix.Code);
            }
            return settingsStore;
        }

        public void LoadSettingsFromStorage(TOptions instance)
        {
            var settingsStore = EnsureStore();

            foreach (PropertyDescriptor property in LazyOptionsPropertyDescriptorCollection.Value)
            {
                try
                {
                    if (!settingsStore.PropertyExists(Vsix.Code, property.Name))
                    {
                        this.typeDescriptorService.SetPropertyValue(property, instance, this.typeDescriptorService.GetPropertyValue(property, DefaultOptions));
                        continue;
                    }

                    var strValue = settingsStore.GetString(Vsix.Code, property.Name);

                    if (string.IsNullOrWhiteSpace(strValue))
                    {
                        continue;
                    }

                    var objValue = jsonConvertService.DeserializeObject(strValue, property.PropertyType);
                    this.typeDescriptorService.SetPropertyValue(property, instance, objValue);
                }
                catch (Exception exception)
                {
                    logger.LogFileAndForget($"Failed to load '{property.Name}' setting", exception.ToString());
                }
            }
        }

        public void SaveSettingsToStorage(TOptions options)
        {
            var settingsStore = EnsureStore();

            foreach (PropertyDescriptor property in LazyOptionsPropertyDescriptorCollection.Value)
            {
                try
                {
                    var objValue = this.typeDescriptorService.GetPropertyValue(property, options);
                    var strValue = jsonConvertService.SerializeObject(objValue);

                    settingsStore.SetString(Vsix.Code, property.Name, strValue);
                }
                catch (Exception exception)
                {
                    logger.LogFileAndForget($"Failed to save '{property.Name}' setting", exception.ToString());
                }
            }
            RaiseOptionsChanged(options);
        }

        public object LoadSettingsFromStorage()
        {
            return Get();
        }

        public void SaveSettingToStorage(object settings)
        {
            this.SaveSettingToStorage((TOptions)settings);
        }
    }

    internal class ProfileManager : Component, IProfileManager
    {
        private readonly List<IOptionsProvider> optionsProviders;
        private List<object> allSettings;

        public ProfileManager() {
            this.optionsProviders = MefServiceProvider.GetAll<IOptionsProvider>().ToList();
        }


        // should be called by VS prior to LoadSettingsFromXML and SaveSettingsToStorage
        public void LoadSettingsFromStorage()
        {
            this.allSettings = this.optionsProviders.ConvertAll(optionsProvider => optionsProvider.LoadSettingsFromStorage());
        }

        public void LoadSettingsFromXml(IVsSettingsReader reader)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var index = 0;
            foreach(var setting in allSettings)
            {
                var optionProvider = optionsProviders[index];
                foreach(PropertyDescriptor propertyDescriptor in optionProvider.LazyOptionsPropertyDescriptorCollection.Value)
                {
                    TypeConverter converter = propertyDescriptor.Converter;
                    if (converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string)))
                    {
                        string pbstrSettingValue = (string)null;
                        object obj = (object)null;
                        try
                        {
                            if (reader.ReadSettingString(propertyDescriptor.Name, out pbstrSettingValue)>0)
                            {
                                if (pbstrSettingValue != null)
                                    obj = propertyDescriptor.Converter.ConvertFromInvariantString(pbstrSettingValue);
                            }
                        }
                        catch
                        {
                        }
                        if (obj != null)
                            propertyDescriptor.SetValue(setting, obj);
                    }
                }
                index++;
            }
            this.allSettings = null;
        }

        public void ResetSettings()
        {

        }

        public void SaveSettingsToStorage()
        {
            var index = 0;
            foreach (var setting in allSettings)
            {
                var optionProvider = optionsProviders[index];
                optionProvider.SaveSettingToStorage(setting);
                index++;
            }
            allSettings = null;
        }

        public void SaveSettingsToXml(IVsSettingsWriter writer)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (allSettings == null)
            {
                LoadSettingsFromStorage();
            }

            var index = 0;
            foreach (var setting in allSettings)
            {
                var optionProvider = optionsProviders[index];
                foreach (PropertyDescriptor propertyDescriptor in optionProvider.LazyOptionsPropertyDescriptorCollection.Value)
                {
                    TypeConverter converter = propertyDescriptor.Converter;
                    if (converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string)))
                    {
                        var propertyValue = propertyDescriptor.GetValue(setting);
                        if(propertyValue == null)
                        {
                            Debug.WriteLine($"Null for {propertyDescriptor.Name}");
                            continue;
                        }
                        var convertedValue = converter.ConvertToInvariantString(propertyValue);
                        Debug.WriteLine($"Converted {propertyDescriptor.Name} - {convertedValue}");
                        var res = writer.WriteSettingString(
                            propertyDescriptor.Name,
                            converter.ConvertToInvariantString(propertyValue)
                        );
                        if(res < 0)
                        {
                            Debug.WriteLine($"Failed for {propertyDescriptor.Name} - {res}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Not converting {propertyDescriptor.Name} for type {propertyDescriptor.PropertyType} with {converter.GetType().Name}");
                    }

                }
                index++;
            }
        }
    }
}
